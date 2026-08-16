using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Interface.Connections;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Connections;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Ptn.DatabaseChecker.Managers.Connections;

// islevi: Veritabani baglantisi kayitlarinin motor varligi ve ad benzersizligi kurallarini isletir.
// sistemdeki gorevi: Baglanti create/update akisini DTO ve EF ayrintilarindan bagimsiz domain kurallariyla korur. Sema kesfi (schemas/objects/snapshot) ayri SchemaDiscoveryManager'in isidir; burada yalnizca adres defteri kurallari + baglanti gecerlilik testi kalir.
public class DatabaseConnectionManager : BaseManager<DatabaseConnection>
{
    // Baglanti ad benzersizligi ihlalinde kullanilacak hata kodu.
    protected override string AlreadyExistsErrorCode => DatabaseConnectionExceptionCodes.NameAlreadyExists;

    // EngineId FK varligi lookup repository uzerinden dogrulanir.
    private IRepository<DatabaseEngine, System.Guid> EngineRepository => LazyGetRequiredService<IRepository<DatabaseEngine, System.Guid>>();

    // Entity -> secret cozulmus baglanti modeli; sema kesfiyle paylasilan tek kaynak.
    private DatabaseConnectionInfoFactory ConnectionInfoFactory => LazyGetRequiredService<DatabaseConnectionInfoFactory>();

    // Motor koduna uygun baglanti test edicisini secer.
    private IEngineComponentResolver<IDatabaseConnectionTester> ConnectionTesterResolver
        => LazyGetRequiredService<IEngineComponentResolver<IDatabaseConnectionTester>>();

    // islevi: Baglanti ad tekilligi icin aktif kullanici kimligini okur.
    // sistemdeki gorevi: Host kapsamindaki kisisel baglantilarin kullanici bazinda ayrilmasini saglar.
    private ICurrentUser CurrentUser => LazyGetRequiredService<ICurrentUser>();

    public DatabaseConnectionManager(
        IDatabaseConnectionRepository repository,
        IAbpLazyServiceProvider abpLazyServiceProvider)
        : base(repository, abpLazyServiceProvider)
    {
    }

    // islevi: Yeni baglanti modelinde ad tekilligi ve motor varligini dogrular.
    public async Task<CreateDatabaseConnectionModel> ValidateCreateAsync(CreateDatabaseConnectionModel model)
    {
        EnsureTlsMode(model.TlsModeCode);
        await EnsureNameUniqueForCreateAsync(model.Name);
        await EnsureEngineExistsAsync(model.EngineId);
        return model;
    }

    // islevi: Toplu baglanti olusturmada ad ve motor kurallarini tekil sorgularla dogrular.
    public async Task<List<CreateDatabaseConnectionModel>> ValidateCreateManyAsync(List<CreateDatabaseConnectionModel> models)
    {
        foreach (var model in models)
        {
            EnsureTlsMode(model.TlsModeCode);
        }

        await EnsureNamesUniqueForCreateManyAsync(models);
        await EnsureEnginesExistAsync(models.Select(x => x.EngineId));
        return models;
    }

    // islevi: Baglanti guncellemesinde ad degistiyse benzersizligi ve motor varligini dogrular.
    public async Task<UpdateDatabaseConnectionModel> ValidateUpdateAsync(DatabaseConnection existing, UpdateDatabaseConnectionModel model)
    {
        EnsureTlsMode(model.TlsModeCode);
        await EnsureNameUniqueForUpdateAsync(existing, model.Name);
        await EnsureEngineExistsAsync(model.EngineId);
        return model;
    }

    // islevi: Baglantiyi emekli eder; silme yerine pasife ceker, gecmis Run FK'lari kirilmaz (entity invariant'i: silinmez pasife cekilir).
    public void Passivate(DatabaseConnection connection)
    {
        connection.IsActive = false;
    }

    // islevi: Baglantiya gercekten baglanmayi dener; secret cozumleme ortak factory'de, motor secimi burada domain ara akisi olarak kalir.
    public async Task<ConnectionTestResult> TestConnectionAsync(DatabaseConnection connection)
    {
        var info = await ConnectionInfoFactory.BuildAsync(connection);
        var tester = ConnectionTesterResolver.Resolve(connection.Engine.Code);
        return await tester.TestAsync(info);
    }

    // islevi: Mevcut kiraci icinde yeni baglanti adinin bosta oldugunu dogrular.
    private async Task EnsureNameUniqueForCreateAsync(string name)
    {
        await EnsureUniqueAsync(BuildNameScope(name));
    }

    // islevi: Toplu create'te baglanti adlarini istek-ici ve DB tekrarlarina karsi dogrular.
    private async Task EnsureNamesUniqueForCreateManyAsync(List<CreateDatabaseConnectionModel> models)
    {
        await EnsureUniqueBulkAsync(
            models.Select(x => x.Name),
            x => x.Name,
            BuildNameScope());
    }

    // islevi: Guncellemede ad degismediyse sorgu yapmadan, degistiyse mevcut kayit haric tekilligi dogrular.
    private async Task EnsureNameUniqueForUpdateAsync(DatabaseConnection existing, string name)
    {
        if (existing.Name != name)
        {
            await EnsureUniqueAsync(BuildNameScope(name), existing.Id);
        }
    }

    // islevi: Baglanti adinin tenant veya host kullanicisi kapsamindaki tekillik ifadesini uretir.
    // sistemdeki gorevi: Host kullanicilarinin kisisel baglantilarinda ayni adin baska kullanicilarla gereksiz yere cakismasini onler.
    private Expression<Func<DatabaseConnection, bool>> BuildNameScope(string name)
    {
        if (CurrentTenant.Id.HasValue)
        {
            var tenantId = CurrentTenant.Id.Value;
            return connection => connection.TenantId == tenantId && connection.Name == name;
        }

        var userId = CurrentUser.Id;
        return connection => connection.TenantId == null && connection.CreatorId == userId && connection.Name == name;
    }

    // islevi: Toplu baglanti ad kontrolu icin isimden bagimsiz tenant/kullanici scope ifadesini uretir.
    // sistemdeki gorevi: CreateMany akisinda ayni gorunurluk kuraliyla tek sorgulu duplicate kontrolu yapar.
    private Expression<Func<DatabaseConnection, bool>> BuildNameScope()
    {
        if (CurrentTenant.Id.HasValue)
        {
            var tenantId = CurrentTenant.Id.Value;
            return connection => connection.TenantId == tenantId;
        }

        var userId = CurrentUser.Id;
        return connection => connection.TenantId == null && connection.CreatorId == userId;
    }

    // islevi: Tek motor lookup kaydinin varligini dogrular.
    private async Task EnsureEngineExistsAsync(System.Guid engineId)
    {
        await EnsureExistsInAsync(EngineRepository, engineId);
    }

    // islevi: Toplu baglanti create'te motor lookup kayitlarini tek sorguyla dogrular.
    private async Task EnsureEnginesExistAsync(IEnumerable<System.Guid> engineIds)
    {
        await EnsureAllExistInAsync(EngineRepository, engineIds);
    }

    // islevi: TLS kodunun Domain.Shared'daki kapali kararli kod kumesinde oldugunu domain sinirinda dogrular.
    private static void EnsureTlsMode(string tlsModeCode)
    {
        if (!TlsModeCodes.IsDefined(tlsModeCode))
        {
            throw new BusinessException(DatabaseConnectionExceptionCodes.InvalidTlsMode);
        }
    }
}
