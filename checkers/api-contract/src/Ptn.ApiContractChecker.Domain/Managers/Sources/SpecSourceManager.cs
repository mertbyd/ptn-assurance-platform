using System.Linq.Expressions;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Constants.Sources;
using Ptn.ApiContractChecker.Entities;
using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.ExceptionCodes.Sources;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Managers.Shared;
using Ptn.ApiContractChecker.Models.Sources;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Users;

namespace Ptn.ApiContractChecker.Managers.Sources;

// islevi: SpecSource ve SpecDocument kanoniklestirme, tekillik ve yasam dongusu kurallarini isletir.
// sistemdeki gorevi: Ince entity'leri korurken AppService'i is kararindan ve repository'yi aggregate davranisindan uzak tutar.
public class SpecSourceManager : BaseManager<SpecSource>
{
    // Kaynak adi tekillik ihlalini kararli alan hata koduna baglar.
    protected override string AlreadyExistsErrorCode => SpecSourceExceptionCodes.NameAlreadyExists;

    // Dis erisilebilirlik denemesini HTTP ve Vault ayrintilarini domaine sokmadan calistirir.
    private ISpecSourceReachabilityTester ReachabilityTester =>
        LazyGetRequiredService<ISpecSourceReachabilityTester>();

    // Canli spec govdesini guard'lariyla getiren cekim sinirini cozer.
    private ISpecFetcherClient FetcherClient => LazyGetRequiredService<ISpecFetcherClient>();
    private ISpecSourceRepository SourceRepository => (ISpecSourceRepository)Repository;

    // Tarihsel run bildirimi pasif kaynak veya dokumanla tamamlandiginda state gate'in calismasini saglar.
    private IDataFilter<IPassivable> PassivableFilter => LazyGetRequiredService<IDataFilter<IPassivable>>();

    // Host baglamindaki kaynak adi tekilligini kullanici sahipligiyle ayni kapsama indirger.
    private ICurrentUser CurrentUser => LazyGetRequiredService<ICurrentUser>();

    public SpecSourceManager(ISpecSourceRepository repository, IAbpLazyServiceProvider provider)
        : base(repository, provider)
    {
    }

    // Yeni kaynak modelini kanoniklestirip tenant kapsaminda ad benzersizliginden gecirir.
    public async Task<CreateSpecSourceModel> ValidateCreateAsync(CreateSpecSourceModel model)
    {
        NormalizeCreateModel(model);
        await EnsureUniqueAsync(BuildNameScope(model.Name));
        return model;
    }

    // Toplu kaynak modellerini kanoniklestirip istek-ici ve veritabani tekrarlarini tek sorguyla dogrular.
    public async Task<List<CreateSpecSourceModel>> ValidateCreateManyAsync(List<CreateSpecSourceModel> models)
    {
        foreach (var model in models)
        {
            NormalizeCreateModel(model);
        }

        await EnsureUniqueBulkAsync(
            models.Select(model => model.Name).ToList(),
            source => source.Name,
            BuildNameScope());
        return models;
    }

    // Guncelleme modelini kanoniklestirip degisen adi mevcut kaynak haric benzersiz tutar.
    public async Task<UpdateSpecSourceModel> ValidateUpdateAsync(SpecSource source, UpdateSpecSourceModel model)
    {
        NormalizeUpdateModel(model);
        if (source.Name != model.Name)
        {
            await EnsureUniqueAsync(BuildNameScope(model.Name), source.Id);
        }

        return model;
    }

    // Tenant icinde paylasimli, hostta ise kullaniciya ozel kaynak adi tekillik ifadesini kurar.
    private Expression<Func<SpecSource, bool>> BuildNameScope(string name)
    {
        if (CurrentTenant.Id.HasValue)
        {
            var tenantId = CurrentTenant.Id.Value;
            return source => source.TenantId == tenantId && source.Name == name;
        }

        var userId = CurrentUser.Id;
        return source => source.TenantId == null && source.CreatorId == userId && source.Name == name;
    }

    // Toplu create icin isimden bagimsiz tenant/kullanici sahiplik kapsamini kurar.
    private Expression<Func<SpecSource, bool>> BuildNameScope()
    {
        if (CurrentTenant.Id.HasValue)
        {
            var tenantId = CurrentTenant.Id.Value;
            return source => source.TenantId == tenantId;
        }

        var userId = CurrentUser.Id;
        return source => source.TenantId == null && source.CreatorId == userId;
    }

    // Dogrulanmis modelden tenant sahiplikli kaynak aggregate'ini ve dokumanlarini kurar.
    public SpecSource Create(Guid id, CreateSpecSourceModel model, Guid? tenantId)
    {
        NormalizeCreateModel(model);
        var source = new SpecSource(id, model.Name, model.BaseUrl, model.VaultSecretPath, tenantId);
        foreach (var document in model.Documents)
        {
            AddDocument(source, GuidGenerator.Create(), document.DocumentName, document.Path, document.IsActive);
        }

        return source;
    }

    // Kaynak tanimini ve dokuman niyetlerini tek aggregate mutasyonunda uygular.
    public void Update(SpecSource source, UpdateSpecSourceModel model)
    {
        NormalizeUpdateModel(model);
        SetDetails(source, model.Name, model.BaseUrl, model.VaultSecretPath);
        foreach (var document in model.Documents)
        {
            ApplyDocumentUpdate(source, document);
        }
    }

    // Vault yazimi sonrasinda yalniz secret referansini degistirir; diger kaynak alanlarini korur.
    public void SetVaultSecretPath(SpecSource source, string? vaultSecretPath)
    {
        source.VaultSecretPath = EntityCanonicalizer.NormalizeOptional(vaultSecretPath);
    }

    // Kaynaga kanonik alanli dokuman ekler ve aggregate icindeki ad tekilligini korur.
    public SpecDocument AddDocument(
        SpecSource source,
        Guid documentId,
        string documentName,
        string path,
        bool isActive = true)
    {
        var normalizedName = EntityCanonicalizer.NormalizeRequired(documentName);
        EnsureDocumentNameUnique(source, normalizedName);
        var document = new SpecDocument(
            documentId,
            source.Id,
            normalizedName,
            NormalizePath(path),
            source.TenantId,
            isActive);
        source.MutableDocuments.Add(document);
        return document;
    }

    // Mevcut dokumanin ad ve yolunu yeniler; izleme durumuna dokunmaz.
    public void UpdateDocument(SpecSource source, Guid documentId, string documentName, string path)
    {
        var document = GetDocument(source, documentId);
        var normalizedName = EntityCanonicalizer.NormalizeRequired(documentName);
        EnsureDocumentNameUnique(source, normalizedName, documentId);
        document.DocumentName = normalizedName;
        document.Path = NormalizePath(path);
    }

    // Aggregate cocugunu fiziksel silmeden yeni cekimler icin pasiflestirir.
    public void PassivateDocument(SpecSource source, Guid documentId)
    {
        GetDocument(source, documentId).IsActive = false;
    }

    // Dokumanin izleme tercihini uygular; kapatirken aralik ve vade alanlarini temizler.
    public SpecDocument ConfigureDocumentMonitoring(
        SpecSource source,
        Guid documentId,
        bool isMonitored,
        int? checkIntervalMinutes,
        DateTime now)
    {
        var document = GetDocument(source, documentId);
        if (!isMonitored)
        {
            document.IsMonitored = false;
            document.CheckIntervalMinutes = null;
            document.NextCheckAt = null;
            return document;
        }

        document.IsMonitored = true;
        document.CheckIntervalMinutes = EnsureIntervalInRange(checkIntervalMinutes);
        document.NextCheckAt = now;
        return document;
    }

    // DTO'dan ayrilmis izleme modelini aktif dokuman guard'i ve domain saatiyle uygular.
    public SpecDocument ConfigureDocumentMonitoring(
        SpecSource source,
        Guid documentId,
        SpecDocumentMonitoringModel model)
    {
        GetRequiredActiveDocument(source, documentId);
        return ConfigureDocumentMonitoring(
            source,
            documentId,
            model.IsMonitored,
            model.CheckIntervalMinutes,
            Clock.Now);
    }

    // Basarili veya basarisiz kontrol denemesini kaydeder ve izlenen dokumanin vadesini ilerletir.
    public SpecDocument MarkDocumentChecked(SpecSource source, Guid documentId, DateTime checkedAt)
    {
        var document = GetDocument(source, documentId);
        document.LastCheckedAt = checkedAt;
        document.NextCheckAt = document.IsMonitored
            ? checkedAt.AddMinutes(EnsureIntervalInRange(document.CheckIntervalMinutes))
            : null;
        return document;
    }

    // Zamanlanmis cekimin sonuc kategorisini diger dokuman alanlarini degistirmeden kaydeder.
    public void RecordDocumentFetchOutcome(SpecSource source, Guid documentId, string outcome)
    {
        if (!SpecDocumentFetchOutcomeCodes.All.Contains(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome));
        }

        GetDocument(source, documentId).LastFetchOutcome = outcome;
    }

    // Kaynagi fiziksel silmeden yeni kontrollerden cikarir ve dokuman gecmisini korur.
    public void Passivate(SpecSource source)
    {
        source.IsActive = false;
    }

    // Birlikte degisen kaynak tanim alanlarini kanonik bicimde tek yerde yazar.
    private static void SetDetails(SpecSource source, string name, string baseUrl, string? vaultSecretPath)
    {
        source.Name = EntityCanonicalizer.NormalizeRequired(name);
        source.BaseUrl = NormalizeBaseUrl(baseUrl);
        source.VaultSecretPath = EntityCanonicalizer.NormalizeOptional(vaultSecretPath);
    }

    // Aggregate davranislarinin hedefledigi dokumani yuklu koleksiyondan zorunlu bulur.
    private static SpecDocument GetDocument(SpecSource source, Guid documentId)
    {
        return source.Documents.SingleOrDefault(document => document.Id == documentId)
               ?? throw new EntityNotFoundException(typeof(SpecDocument), documentId);
    }

    // Create modelinin kaynak ve dokuman metinlerini benzersizlikten once kanoniklestirir.
    private static void NormalizeCreateModel(CreateSpecSourceModel model)
    {
        model.Name = EntityCanonicalizer.NormalizeRequired(model.Name);
        model.BaseUrl = NormalizeBaseUrl(model.BaseUrl);
        model.VaultSecretPath = EntityCanonicalizer.NormalizeOptional(model.VaultSecretPath);
        foreach (var document in model.Documents)
        {
            NormalizeDocumentModel(document);
        }
    }

    // Update modelinin kaynak ve dokuman metinlerini benzersizlikten once kanoniklestirir.
    private static void NormalizeUpdateModel(UpdateSpecSourceModel model)
    {
        model.Name = EntityCanonicalizer.NormalizeRequired(model.Name);
        model.BaseUrl = NormalizeBaseUrl(model.BaseUrl);
        model.VaultSecretPath = EntityCanonicalizer.NormalizeOptional(model.VaultSecretPath);
        foreach (var document in model.Documents)
        {
            NormalizeDocumentModel(document);
        }
    }

    // Dokuman modelini aggregate mutasyonundan once kanonik ad ve goreli yola indirger.
    private static void NormalizeDocumentModel(SpecDocumentModel document)
    {
        document.DocumentName = EntityCanonicalizer.NormalizeRequired(document.DocumentName);
        document.Path = NormalizePath(document.Path);
    }

    // Yeni dokumani ekler veya mevcut dokumani yenileyip istenirse pasiflestirir.
    private void ApplyDocumentUpdate(SpecSource source, SpecDocumentModel document)
    {
        if (document.Id == Guid.Empty)
        {
            AddDocument(source, GuidGenerator.Create(), document.DocumentName, document.Path, document.IsActive);
            return;
        }

        UpdateDocument(source, document.Id, document.DocumentName, document.Path);
        if (!document.IsActive)
        {
            PassivateDocument(source, document.Id);
        }
    }

    // Aggregate icindeki dokuman adinin buyuk-kucuk harf duyarsiz tekilligini korur.
    private static void EnsureDocumentNameUnique(SpecSource source, string documentName, Guid? excludedId = null)
    {
        if (source.Documents.Any(document =>
                document.Id != excludedId &&
                string.Equals(document.DocumentName, documentName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessException(SpecSourceExceptionCodes.DocumentNameAlreadyExists);
        }
    }

    // Izleme araligini kalici sozlesmenin alt ve ust sinirlarinda tutar.
    private static int EnsureIntervalInRange(int? value)
    {
        return value is >= SpecDocumentConsts.MinCheckIntervalMinutes
                     and <= SpecDocumentConsts.MaxCheckIntervalMinutes
            ? value.Value
            : throw new ArgumentOutOfRangeException(nameof(value));
    }

    // Servis kokunu sonda ayirac olmadan kanoniklestirir ve ic cagri hatalarinda mutlak HTTP bicimini savunur.
    private static string NormalizeBaseUrl(string value)
    {
        var normalized = EntityCanonicalizer.NormalizeRequired(value)
            .TrimEnd(ApiContractCheckerRoutes.SeparatorCharacter);
        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException(null, nameof(value));
        }

        return normalized;
    }

    // Dokuman yolunu trimler ve ic cagri hatalarinda mutlak HTTP adresini reddeder.
    private static string NormalizePath(string value)
    {
        var normalized = EntityCanonicalizer.NormalizeRequired(value);
        if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            throw new ArgumentException(null, nameof(value));
        }

        return normalized;
    }

    // Aktif dokumanlari belirleyip erisilebilirlik kararini provider sinirina teslim eder.
    public async Task<SpecSourceReachabilityModel> TestReachabilityAsync(SpecSource source)
    {
        var documents = GetActiveDocuments(source);
        if (documents.Count == 0)
        {
            return new SpecSourceReachabilityModel
            {
                IsReachable = false,
                ErrorMessage = SpecSourceExceptionCodes.Validation.DocumentsRequired
            };
        }

        return await ReachabilityTester.TestAsync(source, documents);
    }

    // Aggregate icindeki aktif dokumanlari dis provider'a verilecek salt-okunur listeye toplar.
    private static List<SpecDocument> GetActiveDocuments(SpecSource source)
    {
        var documents = new List<SpecDocument>();
        foreach (var document in source.Documents)
        {
            if (document.IsActive)
            {
                documents.Add(document);
            }
        }

        return documents;
    }

    // Snapshot alinacak dokumani aggregate icinde bulur; pasif veya yabanci dokumani kabul etmez.
    public SpecDocument GetRequiredActiveDocument(SpecSource source, Guid documentId)
    {
        return source.Documents.SingleOrDefault(document => document.Id == documentId && document.IsActive)
               ?? throw new BusinessException(SpecSourceExceptionCodes.ActiveDocumentNotFound);
    }

    // Kaynagi detaylariyla yukler ve istenen aktif dokumanin bu kaynaga ait oldugunu dogrular.
    public async Task<SpecDocument> GetRequiredActiveDocumentAsync(Guid sourceId, Guid documentId)
    {
        var source = await SourceRepository.FindWithDetailsAsync(sourceId)
                     ?? throw new Volo.Abp.BusinessException(SpecSourceExceptionCodes.ActiveDocumentNotFound);
        return GetRequiredActiveDocument(source, documentId);
    }

    // Tek dokumanin canli govdesini kaynak taban adresi ve Vault yoluyla ceker.
    public Task<SpecFetchResultModel> FetchDocumentAsync(SpecSource source, SpecDocument document)
    {
        return FetcherClient.FetchAsync(new SpecFetchRequestModel(source.BaseUrl, document.Path, source.VaultSecretPath));
    }
}
