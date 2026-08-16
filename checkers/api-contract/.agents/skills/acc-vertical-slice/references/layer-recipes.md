# Katman tarifleri — birebir iskeletler

Her iskelet bu repodaki gerçek deseni yansıtır. Kopyala, `{Ad}` / `{Alan}` yerine
kendi adlarını koy, **yorumları alan niyetine göre yeniden yaz** (kopyalanan yorum
ihlaldir — yorum sözdizimi değil amaç anlatır).

Her tipte house yorum çifti zorunludur:

```csharp
// islevi: <bu tip ne yapar>
// sistemdeki gorevi: <sistemde hangi bosluğu doldurur, kime hizmet eder>
```

Her yazdığın metodun üstünde tek satır amaç yorumu bulunur.

---

## Entity

```csharp
namespace Ptn.ApiContractChecker.Entities.{Alan};

// islevi: <alan gerceginin ne oldugunu bir cumlede>
// sistemdeki gorevi: <kim buna FK verir, hangi akista okunur>
public class {Ad} : AuditedAggregateRoot<Guid>, IPassivable, IMultiTenant
{
    // <alan amaci: bu kolon hangi soruyu cevaplar>
    public string Name { get; internal set; } = default!;

    // Emekli edilen kayit silinmez pasife cekilir; gecmis kayitlar FK ile isaret etmeye devam eder.
    public bool IsActive { get; internal set; }

    // Kiraci izolasyonu; ABP sorgulari otomatik filtreler.
    public Guid? TenantId { get; internal set; }

    // EF Core materializasyonu icin parametresiz ctor; disaridan cagrilmaz.
    protected {Ad}() { }

    // Kalici alanlari yalniz atayan ctor; normalize, validate veya throw etmez.
    public {Ad}(Guid id, string name, Guid? tenantId, bool isActive = true) : base(id)
    {
        Name = name;
        TenantId = tenantId;
        IsActive = isActive;
    }
}
```

Kurallar:

- Taban tipi **gerçek** ihtiyaca göre seç: audit gerekmiyorsa `AggregateRoot<Guid>`,
  soft-delete gerekiyorsa `FullAuditedAggregateRoot<Guid>`.
- Sır (parola, token) **kolon olmaz** — Vault'a gider, entity yalnız secret yolunu
  tutar.
- Public setter yazma. Entity'ye `Check`, `Ensure`, `Normalize`, `Validate`, durum
  geçişi veya mutasyon metodu koyma; bütün karar ve yazmalar manager'da dursun.

---

## Manager

```csharp
namespace Ptn.ApiContractChecker.Managers.{Alan};

// islevi: {Ad} icin varlik, benzersizlik ve durum gecisi kurallarini isletir.
// sistemdeki gorevi: AppService'i is kuralindan, repository'yi karardan uzak tutar.
public class {Ad}Manager : BaseManager<{Ad}>
{
    protected override string AlreadyExistsErrorCode => {Ad}ExceptionCodes.NameAlreadyExists;

    public {Ad}Manager(IBaseRepository<{Ad}> repository, IAbpLazyServiceProvider provider)
        : base(repository, provider) { }

    // islevi: Olusturma modelini kiraci icinde ad benzersizligi kuralindan gecirir.
    public async Task<Create{Ad}Model> ValidateCreateAsync(Create{Ad}Model model)
    {
        await EnsureUniqueAsync(x => x.Name == model.Name);
        return model;
    }

    // islevi: Guncelleme modelini, kaydin kendisi haric ad benzersizligi kuralindan gecirir.
    public async Task<Update{Ad}Model> ValidateUpdateAsync({Ad} entity, Update{Ad}Model model)
    {
        if (entity.Name != model.Name)
        {
            await EnsureUniqueAsync(x => x.Name == model.Name, entity.Id);
        }

        return model;
    }

    // islevi: Dogrulanmis modelden yalniz alan atamasi yapan entity'yi kurar.
    public {Ad} Create(Guid id, Create{Ad}Model model, Guid? tenantId)
        => new(id, model.Name, tenantId);

    // islevi: Dogrulanmis model alanlarini entity'ye uygular.
    public void Update({Ad} entity, Update{Ad}Model model)
    {
        entity.Name = model.Name;
    }

    // islevi: Kaydi emekli eder; silmez, gecmis referanslar korunur.
    public void Passivate({Ad} entity) => entity.IsActive = false;
}
```

Kurallar:

- **Döngü içinde repository çağrısı yok.** Toplu doğrulama gerekiyorsa
  `EnsureAllExistInAsync` / `GetRequiredEntitiesInAsync` kullan.
- Manager DTO/transport modeli görmez, `Result<T>` döndürmez.
- Bir işbirlikçiyi yalnız dış sistemle ilgili diye manager'dan atma. Burada bulunması
  use-case içindeki iş kararına gerçekten hizmet etmesine ve repo bağımlılık yönüne
  uymasına bağlıdır; kontrat mekanizmayı değil gereken yeteneği anlatır. Yalnız
  taşıma/SDK mekaniğini, wire tipini veya yabancı exception'ı manager'a yığma.
- Benzersizlik/varlık kontrolü için elle sorgu yazma — `BaseManager` metotları var.

---

## Repository

```csharp
namespace Ptn.ApiContractChecker.Repository.{Alan};

// islevi: {Ad} icin detay okuma, sayfalama ve erisilebilirlik sorgularini kurar.
// sistemdeki gorevi: Tum LINQ'i tek katmanda tutar; ustteki katmanlar sorgu bilmez.
public class {Ad}Repository : BaseRepository<{Ad}>, I{Ad}Repository
{
    public {Ad}Repository(IDbContextProvider<ApiContractCheckerDbContext> provider)
        : base(provider) { }

    // islevi: Tek kaydi gorunurluk kuralina uyarak navigation'lariyla okur.
    public async Task<{Ad}?> FindWithDetailsAsync(Guid id)
    {
        var query = await BuildAccessibleQueryAsync();
        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    // islevi: Sayfalamayi ve siralamayi veritabani tarafinda yapar.
    public async Task<List<{Ad}>> GetPagedWithDetailsAsync(int skipCount, int maxResultCount)
    {
        var query = await BuildAccessibleQueryAsync();
        return await query.OrderBy(x => x.Name).Skip(skipCount).Take(maxResultCount).ToListAsync();
    }

    // islevi: Kiraci ve sahiplik gorunurluk kuralini tek yerde kurar; her okuma yolu buradan gecer.
    private async Task<IQueryable<{Ad}>> BuildAccessibleQueryAsync()
    {
        var query = await GetQueryableAsync();
        // Kiraci baglaminda kayitlar kiraci-paylasimlidir; host/kiracisiz baglamda kullanici bazlidir.
        return CurrentTenant.Id.HasValue
            ? query
            : query.Where(x => x.CreatorId == null || x.CreatorId == CurrentUser.Id);
    }
}
```

Kurallar:

- **`.Where(x => x.IsActive)` yazma** — `IPassivable` global filtresi zaten yapıyor.
- **`.Where(x => !x.IsDeleted)` yazma** — ABP `ISoftDelete` filtresi zaten yapıyor.
- `ToListAsync()` sonrası `.Where/.Skip/.Take` yasak; filtre DB'de.
- Görünürlük kuralını her metoda kopyalama — tek private metotta topla.
- DTO şekilli okuma gerekiyorsa `select new Model { … }` projeksiyonu kullan
  (nesne başlatıcısının meşru olduğu tek yer).

---

## DTO ve validator

```csharp
// Application.Contracts/Dtos/{Alan}/Create{Ad}Dto.cs
// islevi: {Ad} olusturma istegi govdesini tasir.
// sistemdeki gorevi: HTTP sinirindaki tek girdi sozlesmesi; sir alanlari yalniz istekte bulunur, yanitta donmez.
public class Create{Ad}Dto
{
    public string Name { get; set; } = default!;
}
```

```csharp
// Application.Contracts/FluentValidation/{Alan}/Create{Ad}DtoValidator.cs
// islevi: {Ad} olusturma istegini sekil, uzunluk ve format acisindan dogrular.
// sistemdeki gorevi: Veritabanina dayali kurallari manager'a birakir; burada yalniz sinir dogrulamasi vardir.
public class Create{Ad}DtoValidator : AbstractValidator<Create{Ad}Dto>
{
    public Create{Ad}DtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode({Ad}ExceptionCodes.NameRequired)
            .MaximumLength({Ad}Consts.MaxNameLength).WithErrorCode({Ad}ExceptionCodes.NameTooLong);
    }
}
```

Sınır: **validator DB'ye bakmaz.** Benzersizlik, varlık, sahiplik → manager.

---

## Mapperly eşleyici

```csharp
// islevi: {Ad} icin entity <-> model <-> DTO donusumlerini kaynak-uretecle kurar.
// sistemdeki gorevi: Elle alan kopyalamayi ortadan kaldirir; eksik alan derleme zamaninda bildirilir.
[Mapper]
public partial class {Ad}Mapper
{
    public partial {Ad}Dto MapToDto({Ad} entity);
    public partial List<{Ad}Dto> MapToDto(List<{Ad}> entities);
    public partial Create{Ad}Model MapToCreateModel(Create{Ad}Dto input);
    public partial Update{Ad}Model MapToUpdateModel(Update{Ad}Dto input);
}
```

AppService'te tek statik örnek yeterlidir (stateless):

```csharp
private static readonly {Ad}Mapper Mapper = new();
```

Yeni RMG uyarısı **üretme**; dokunduğun eşleyicide varsa temizle.

`MapperIgnoreSource` / `MapperIgnoreTarget` susturma dekorasyonu değildir. Önce
attributesiz derle ve mapper'a ait RMG tanısını oku. Yalnız hedef sözleşmenin
bilinçli olarak taşımadığı alan için, somut tanı varsa ignore ekle; tanıyı anlamadan
toplu ignore listesi kurma.

Assembly seviyesinde hedef tamlığını aç:

```csharp
[assembly: MapperDefaults(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
```

Bu ayar hedef alanları sıkı tutar; ABP audit alanlarını, `ExtraProperties`,
`ConcurrencyStamp` ve `TenantId` alanlarını topluca `MapperIgnoreSource` yazmayı
gereksiz kılar.

---

## AppService

```csharp
// islevi: {Ad} icin CRUD kullanim senaryolarini orkestre eder.
// sistemdeki gorevi: Kurali manager'a, sorguyu repository'ye birakir; kendisi ince kalir.
[RemoteService(IsEnabled = false)]
public class {Ad}AppService
    : EntityCrudAppServiceBase<{Ad}, {Ad}, {Ad}Dto, Create{Ad}Dto, Update{Ad}Dto, Create{Ad}Model, Update{Ad}Model>,
      I{Ad}AppService
{
    private static readonly {Ad}Mapper Mapper = new();

    // Is kurallari manager katmaninda isletilir.
    private {Ad}Manager Manager => LazyGetRequiredService<{Ad}Manager>();

    // Detay okumalar repository katmaninda yapilir.
    private I{Ad}Repository {Ad}Repository => LazyGetRequiredService<I{Ad}Repository>();

    public {Ad}AppService(IAbpLazyServiceProvider provider, I{Ad}Repository repository)
        : base(provider, repository) { }

    // islevi: Kaydi emekli eder; pasif satir kendi okuma sorgusuna gorunmedigi icin cevap yuklu entity'den uretilir.
    public async Task<{Ad}Dto> PassivateAsync(Guid id)
    {
        var entity = await GetEntityForMutationAsync(id);
        Manager.Passivate(entity);
        var saved = await Repository.UpdateAsync(entity, autoSave: true);
        return Mapper.MapToDto(saved);   // yeniden OKUMA yok -> failure-catalog #2
    }

    protected override async Task<{Ad}> GetReadModelAsync(Guid id)
        => EnsureFound(await {Ad}Repository.FindWithDetailsAsync(id), id);

    protected override {Ad}Dto MapToDto({Ad} readModel) => Mapper.MapToDto(readModel);
    protected override Create{Ad}Model MapToCreateModel(Create{Ad}Dto input) => Mapper.MapToCreateModel(input);
    protected override Task<Create{Ad}Model> ValidateCreateModelAsync(Create{Ad}Model model)
        => Manager.ValidateCreateAsync(model);
    protected override {Ad} BuildEntity(Create{Ad}Model model)
        => Manager.Create(GuidGenerator.Create(), model, CurrentTenant.Id);
    protected override void MapToEntity(Update{Ad}Model model, {Ad} entity)
        => Manager.Update(entity, model);
}
```

Kurallar:

- En yakın tamamlanmış kardeşin base ve hook yüzeyi varsayılandır. Farklı base'i
  yalnız use-case davranışı gerçekten farklıysa ve bu fark açıkça gerekçeliyse seç.
- Entity data-shell constructor'u yüzünden generic entity kurma hook'u
  uymuyorsa concrete serviste CRUD akışını kopyalama. İkinci kullanımda mevcut
  base'in `BuildEntity` benzeri hook'unu aggregate-safe hale getir ve bütün mevcut
  tüketicileri aynı değişiklikte taşı.
- Base'in sağladığı `CreateValidator`, `UpdateValidator`, load, persist ve map
  adımlarını concrete serviste ikinci kez kurma. Yalnız dış etki sırası gibi gerçek
  varyasyonu override et ve küçük adlandırılmış metoda ayır.
- Public metod tek use-case'i düz akışta orkestre etsin. Ayrı sorumluluğu olmayan
  helper, aynı işi iki kez yapan recovery yolu veya bütün ayrıntıları taşıyan god
  service üretme.
- Kimlik **ABP `GuidGenerator`**'dan gelir, `Guid.NewGuid()` değil.
- Zaman **ABP `IClock`**'tan gelir, `DateTime.Now` değil.
- `try/catch + log` yok — exception middleware sınırı sahiplenir.
- Gereksinim/Wiki/kardeş akışında olmayan repair, compensation veya fallback'i
  “daha güvenli” varsayımıyla ekleme; davranış çatışmasını önce bildir.

---

## EF configuration

```csharp
// islevi: {Ad} entity'sinin tablo, kolon ve index eslemesini tanimlar.
// sistemdeki gorevi: DbContext'i esleme detayindan arindirir; ApplyConfigurationsFromAssembly otomatik bulur.
public class {Ad}Configuration : IEntityTypeConfiguration<{Ad}>
{
    public void Configure(EntityTypeBuilder<{Ad}> builder)
    {
        builder.ToTable(ApiContractCheckerTableNames.{Ad}s, ApiContractCheckerDbProperties.CheckerSchema);
        builder.ConfigureByConvention();

        builder.Property(x => x.Name).IsRequired().HasMaxLength({Ad}Consts.MaxNameLength);
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
    }
}
```

- `ConfigureByConvention()` **çağrılmazsa** ABP audit/tenant kolonları eşlenmez.
- DbContext içinde elle `builder.ConfigureX<T>()` yazma — otomatik tarama var.
- `ToJson`, `ToTable`, kolon adı/tipi gibi EF'e verilen kararlı stringler tek
  kullanımlı olsa da `Domain.Shared` sabitinden gelir; eşleme içinde literal kalmaz.
- EF modeli değiştiyse migration **üret ve oku**; `Up()` içinde beklemediğin tablo
  varsa dur.

---

## Controller

```csharp
// islevi: {Ad} okuma ve yazma endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Istekleri I{Ad}AppService'e yonlendiren ince kopru; is karari tasimaz.
[Route(ApiContractCheckerRoutes.{Alan}.{Ad}s)]
[ApiExplorerSettings(GroupName = ApiContractCheckerRoutes.Groups.{Alan})]
[Authorize(ApiContractCheckerPermissions.{Alan}.View)]
public class {Ad}Controller : EntityReadControllerBase<I{Ad}AppService, {Ad}Dto>
{
    /// <summary>Yeni {ad} olusturur.</summary>
    [HttpPost]
    [Authorize(ApiContractCheckerPermissions.{Alan}.Manage)]
    public async Task<Result<{Ad}Dto>> Create([FromBody] Create{Ad}Dto input)
    {
        var result = await AppService.CreateAsync(input);
        return result;
    }
}
```

`Get` ve `GetList` tabandan gelir — **yeniden yazma**.

---

## Arka plan işi

```csharp
// islevi: {Ad} calistirmasini kuyruktan alip tenant baglaminda yurutur.
// sistemdeki gorevi: Uzun dis I/O'yu HTTP isteginden ve uygulama transaction'indan ayirir.
public class {Ad}Job : ApiContractCheckerTenantBackgroundJob<{Ad}JobArgs>, ITransientDependency
{
    private {Ad}ExecutionManager Manager => LazyGetRequiredService<{Ad}ExecutionManager>();

    protected override async Task ExecuteInTenantAsync({Ad}JobArgs args)
    {
        // 1) Kisa UOW: kalici veriden salt-okunur baglam uret.
        var context = await RunInUnitOfWorkAsync(() => Manager.BuildExecutionContextAsync(args.Id, args.ScopeRules));

        // 2) UOW YOK: uzun dis I/O burada, hicbir transaction acik degil.
        var result = await Manager.ExecuteAsync(context);

        // 3) Kisa UOW: terminal durum + sonuc tek yazimda; zaten terminalse dokunma (idempotans).
        await RunInUnitOfWorkAsync(() => Manager.CompleteAsync(result));
    }
}
```

Kurallar:

- Kiracı açma, iptal kontrolü ve UOW **tabandan gelir** — somut işte yeniden yazma.
- Tamamlama **idempotent** olmak zorunda: job en az bir kez teslim edilir.
- 2. adımda repository çağırma; bağlam modeli her şeyi taşımalı.
