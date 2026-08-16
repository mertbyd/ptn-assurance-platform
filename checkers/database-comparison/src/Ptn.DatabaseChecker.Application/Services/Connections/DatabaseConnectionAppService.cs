using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Ptn.DatabaseChecker.Application.Mappers.Connections;
using Ptn.DatabaseChecker.Application.Services;
using Ptn.DatabaseChecker.Dtos.Connections;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Interface.Connections;
using Ptn.DatabaseChecker.Interface.Secrets;
using Ptn.DatabaseChecker.Managers.Connections;
using Ptn.DatabaseChecker.Models.Connections;
using Ptn.DatabaseChecker.Models.Secrets;
using Ptn.DatabaseChecker.Secrets;
using Ptn.DatabaseChecker.Services.Connections;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Application.Services.Connections;

// islevi: Veritabani baglantilari icin temel CRUD Application akisini yonetir.
// sistemdeki gorevi: Baglanti adres defterinde is kurali manager'da, detayli okuma repository'de kalacak sekilde orkestrasyon yapar.
[RemoteService(IsEnabled = false)]
public class DatabaseConnectionAppService
    : EntityCrudAppServiceBase<DatabaseConnection, DatabaseConnection, DatabaseConnectionDto, CreateDatabaseConnectionDto, UpdateDatabaseConnectionDto, CreateDatabaseConnectionModel, UpdateDatabaseConnectionModel>,
        IDatabaseConnectionAppService
{
    // Mapperly source-generated mapper; stateless oldugu icin tek statik ornek yeterli.
    private static readonly DatabaseConnectionMapper Mapper = new();

    // Baglanti domain kurallari manager katmaninda isletilir.
    private DatabaseConnectionManager Manager => LazyGetRequiredService<DatabaseConnectionManager>();

    // Baglanti detay okumalari Engine Include ile repository katmaninda yapilir.
    private IDatabaseConnectionRepository ConnectionRepository => LazyGetRequiredService<IDatabaseConnectionRepository>();

    // Kimlik bilgileri (kullanici adi + sifre) yalnizca Vault'a yazilir; sifre hicbir katmanda DB'ye/DTO'ya gitmez.
    private ISecretProvider SecretProvider => LazyGetRequiredService<ISecretProvider>();

    public DatabaseConnectionAppService(
        IAbpLazyServiceProvider abpLazyServiceProvider,
        IDatabaseConnectionRepository repository)
        : base(abpLazyServiceProvider, repository)
    {
    }

    // islevi: Baglantiyi olusturur; once dogrular, sonra kimlik bilgisini Vault'a yazar, DB'ye yalnizca hesaplanmis vault_secret_path'i persist eder.
    // sistemdeki gorevi: Secret path baglanti Id'sine baglidir; bu yuzden Id uretildikten sonra Vault'a yazilir, ardindan entity kaydedilir. Sifre entity'ye/DTO'ya asla girmez.
    public override async Task<DatabaseConnectionDto> CreateAsync(CreateDatabaseConnectionDto input)
    {
        await CreateValidator.ValidateAndThrowAsync(input);
        var model = MapToCreateModel(input);
        await ValidateCreateModelAsync(model);

        var entity = CreateEmptyEntity();
        var secretPath = SecretPathBuilder.Build(CurrentTenant.Id, entity.Id);
        await SecretProvider.SetAsync(secretPath, new DatabaseCredentialModel { Username = input.Username, Password = input.Password });

        model.VaultSecretPath = secretPath;
        MapToEntity(model, entity);

        var inserted = await Repository.InsertAsync(entity, autoSave: true);
        return await MapSavedEntityAsync(inserted.Id);
    }

    // islevi: Baglantiyi gunceller; adres alanlarini her zaman, secret'i yalnizca yeni parola verilince Vault'ta yeniden yazar.
    // sistemdeki gorevi: Secret path baglanti Id'sine bagli sabittir; degismez. Parola bos gelirse mevcut secret'a dokunulmaz.
    public override async Task<DatabaseConnectionDto> UpdateAsync(Guid id, UpdateDatabaseConnectionDto input)
    {
        await UpdateValidator.ValidateAndThrowAsync(input);
        var entity = await GetEntityForMutationAsync(id);
        var model = MapToUpdateModel(input);
        await ValidateUpdateModelAsync(entity, model);

        model.VaultSecretPath = entity.VaultSecretPath;
        if (!string.IsNullOrWhiteSpace(input.Password))
        {
            await SecretProvider.SetAsync(entity.VaultSecretPath, new DatabaseCredentialModel { Username = input.Username!, Password = input.Password! });
        }

        MapToEntity(model, entity);
        var saved = await Repository.UpdateAsync(entity, autoSave: true);
        return await MapSavedEntityAsync(saved.Id);
    }

    // islevi: Baglantiyi emekli eder (pasife ceker); silmez, gecmis Run FK'lari korunur.
    // sistemdeki gorevi: Kayit pasiflestikten sonra IPassivable global filtresi onu her sorgudan disladigi icin geri okunamaz;
    // yeniden okumak basarili islemi 404'e cevirirdi. Entity zaten Engine Include'uyla yuklu oldugundan cevap ondan uretilir.
    public async Task<DatabaseConnectionDto> PassivateAsync(Guid id)
    {
        var entity = await GetEntityForMutationAsync(id);
        Manager.Passivate(entity);
        var saved = await Repository.UpdateAsync(entity, autoSave: true);
        return Mapper.MapToDto(saved);
    }

    // islevi: Baglantiya gercekten baglanmayi dener; kimligi Vault'tan cozer, motora uygun test edicisi ile hedefe baglanir, sonucu doner.
    // sistemdeki gorevi: Sifre Vault'tan yalniz bellege cozulur; connection string test edicide kurulur, cevaba yalniz durum + surum + hata mesaji gider.
    public async Task<TestConnectionResultDto> TestConnectionAsync(Guid id)
    {
        var entity = EnsureFound(await ConnectionRepository.FindWithDetailsAsync(id), id);
        var result = await Manager.TestConnectionAsync(entity);
        return Mapper.MapToTestResultDto(result);
    }

    // islevi: Tek baglantiyi Engine detaylariyla repository'den okur.
    protected override async Task<DatabaseConnection> GetReadModelAsync(Guid id)
        => EnsureFound(await ConnectionRepository.FindWithDetailsAsync(id), id);

    // islevi: Update/passivate islemlerinde de okuma ile ayni tenant + CreatorId gorunurluk kuralini uygular.
    // sistemdeki gorevi: Kullanici baska bir kullanicinin host baglantisini veya baska tenant'in baglantisini kimlikle degistiremesin.
    protected override async Task<DatabaseConnection> GetEntityForMutationAsync(Guid id)
        => EnsureFound(await ConnectionRepository.FindWithDetailsAsync(id), id);

    // islevi: Baglanti listesini Engine detaylariyla sayfali okur.
    protected override Task<List<DatabaseConnection>> GetPagedReadModelsAsync(PagedResultRequestDto input)
        => ConnectionRepository.GetPagedWithDetailsAsync(input.SkipCount, input.MaxResultCount);

    // islevi: Kayit sonrasi baglantilari Engine detaylariyla tek batch sorguda okur.
    protected override Task<List<DatabaseConnection>> GetReadModelsByIdsAsync(List<Guid> ids)
        => ConnectionRepository.GetWithDetailsByIdsAsync(ids);

    // islevi: Liste toplam sayisini detayli liste sorgusuyla ayni erisim kuraliyla hesaplar.
    // sistemdeki gorevi: UserId + tenant birlesik gorunurlugunde sayfalama metadatasinin yanlis olmasini engeller.
    protected override Task<long> GetTotalCountAsync(PagedResultRequestDto input)
        => ConnectionRepository.GetAccessibleCountAsync();

    // islevi: Baglanti entity'sini API cevap DTO'suna tasir.
    protected override DatabaseConnectionDto MapToDto(DatabaseConnection readModel) => Mapper.MapToDto(readModel);

    // islevi: Baglanti entity listesini API cevap DTO listesine tasir.
    protected override List<DatabaseConnectionDto> MapToDto(List<DatabaseConnection> readModels) => Mapper.MapToDto(readModels);

    // islevi: Baglanti create DTO'sunu domain manager modeline tasir.
    protected override CreateDatabaseConnectionModel MapToCreateModel(CreateDatabaseConnectionDto input) => Mapper.MapToCreateModel(input);

    // islevi: Baglanti update DTO'sunu domain manager modeline tasir.
    protected override UpdateDatabaseConnectionModel MapToUpdateModel(UpdateDatabaseConnectionDto input) => Mapper.MapToUpdateModel(input);

    // islevi: Baglanti create modelini manager kurallarindan gecirir.
    protected override Task<CreateDatabaseConnectionModel> ValidateCreateModelAsync(CreateDatabaseConnectionModel model)
        => Manager.ValidateCreateAsync(model);

    // islevi: Baglanti create model listesini manager batch kurallarindan gecirir.
    protected override Task<List<CreateDatabaseConnectionModel>> ValidateCreateModelsAsync(List<CreateDatabaseConnectionModel> models)
        => Manager.ValidateCreateManyAsync(models);

    // islevi: Baglanti update modelini manager kurallarindan gecirir.
    protected override Task<UpdateDatabaseConnectionModel> ValidateUpdateModelAsync(DatabaseConnection entity, UpdateDatabaseConnectionModel model)
        => Manager.ValidateUpdateAsync(entity, model);

    // islevi: Yeni DatabaseConnection entity'sini ABP GuidGenerator kimligiyle kurar.
    protected override DatabaseConnection CreateEmptyEntity() => new(GuidGenerator.Create());

    // islevi: Dogrulanmis baglanti create modelini entity uzerine uygular.
    protected override void MapToEntity(CreateDatabaseConnectionModel model, DatabaseConnection entity) => Mapper.MapToEntity(model, entity);

    // islevi: Dogrulanmis baglanti update modelini entity uzerine uygular.
    protected override void MapToEntity(UpdateDatabaseConnectionModel model, DatabaseConnection entity) => Mapper.MapToEntity(model, entity);
}
