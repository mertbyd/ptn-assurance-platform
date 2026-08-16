# Artifact manifesti — gereksinimden dosya listesine

Gereksinimi aşağıdaki tiplerden birine oturt, manifesti aynen uygula. Klasör
yolları sabittir ([kanonik checker kuralları](../../../../../../docs/PTN-ASSURANCE-PLATFORM-WIKI.md#13-checker-gelistirme-kurallari));
`{Alan}` = iş alanı klasörü (`Sources`, `Snapshots`, `Runs`, `Recipients` …).

## Tip A — Kiracıya ait yeni ana varlık, tam CRUD

> "Kaynak tanımlarını ekleyip listeleyebilelim, güncelleyebilelim, pasife çekebilelim."

| # | Dosya | Not |
|---|---|---|
| 1 | `Domain.Shared/Constants/{Alan}/{Ad}Consts.cs` | Alan uzunlukları, sınırlar |
| 2 | `Domain.Shared/ExceptionCodes/{Alan}/{Ad}ExceptionCodes.cs` | Her iş hatası bir kod |
| 3 | `Domain.Shared/Permissions/ApiContractCheckerPermissions.{Alan}.cs` | `View` / `Manage` |
| 4 | `Domain/Entities/{Alan}/{Ad}.cs` | `AuditedAggregateRoot<Guid>`, `IPassivable`, `IMultiTenant` |
| 5 | `Domain/Models/{Alan}/Create{Ad}Model.cs` + `Update{Ad}Model.cs` | Manager girdisi |
| 6 | `Domain/Interface/{Alan}/I{Ad}Repository.cs` | `IBaseRepository<{Ad}>` türer |
| 7 | `Domain/Managers/{Alan}/{Ad}Manager.cs` | `BaseManager<{Ad}>` türer |
| 8 | `Application.Contracts/Dtos/{Alan}/{Ad}Dto.cs` + `Create{Ad}Dto.cs` + `Update{Ad}Dto.cs` | Sır **asla** yanıt DTO'sunda |
| 9 | `Application.Contracts/FluentValidation/{Alan}/Create{Ad}DtoValidator.cs` + `Update…` | Şekil/format/uzunluk/aralık |
| 10 | `Application.Contracts/Services/{Alan}/I{Ad}AppService.cs` | |
| 11 | `Application.Contracts/Permissions/Definitions/{Alan}/…PermissionDefinitionProvider.{Alan}.cs` | partial metod |
| 12 | `Application/Mappers/{Alan}/{Ad}Mapper.cs` | `[Mapper]` partial |
| 13 | `Application/Services/{Alan}/{Ad}AppService.cs` | `EntityCrudAppServiceBase<…>` türer |
| 14 | `EntityFrameworkCore/Configurations/{Alan}/{Ad}Configuration.cs` | `IEntityTypeConfiguration<{Ad}>` |
| 15 | `EntityFrameworkCore/Repository/{Alan}/{Ad}Repository.cs` | `BaseRepository<{Ad}>` türer |
| 16 | `EntityFrameworkCore/…DbContext.cs` + `I…DbContext.cs` | `DbSet<{Ad}>` ekle |
| 17 | **Migration** | `dotnet ef migrations add` + **üretileni oku** |
| 18 | `HttpApi/Controllers/{Alan}/{Ad}Controller.cs` | `EntityReadControllerBase<…>` türer |
| 19 | Test: repository + validator + (varsa) durum geçişi | |

Ana varlık controller'ı **yalnız istenen endpoint'leri** açar. Pasife çekme varsa
`DELETE` **yoktur**.

## Tip B — Yeni lookup

Ayrı tarif: [`../../acc-lookup-recipe/SKILL.md`](../../acc-lookup-recipe/SKILL.md).
Manifest 6 dosyaya iner çünkü CRUD tabandan gelir.

## Tip C — Mevcut varlığa yeni işlem (endpoint)

> "Bir kaynağa erişilebiliyor mu diye test edebilelim."

| # | Dosya |
|---|---|
| 1 | Gerekiyorsa yeni `ExceptionCodes` sabiti |
| 2 | Gerekiyorsa yeni izin sabiti + tanım satırı |
| 3 | Sonuç DTO'su (`Application.Contracts/Dtos/{Alan}/`) |
| 4 | Girdi varsa DTO + validator |
| 5 | `Mapper`'a yeni eşleme metodu — **yeni mapper sınıfı açma** |
| 6 | Manager'a iş metodu |
| 7 | AppService'e senaryo metodu |
| 8 | Controller'a rota |
| 9 | Test |

Entity, EF configuration ve migration **değişmez**. Değişiyorsa gereksinim Tip A
veya D'dir, C değildir.

## Tip D — Uzun süren, asenkron çalıştırılan iş

> "Kontrolü tetikleyelim, durumunu sorabilelim."

| # | Dosya | Not |
|---|---|---|
| 1 | `Application.Contracts/BackgroundJobs/{Alan}/{Ad}JobArgs.cs` | `ITenantBackgroundJobArgs` uygular |
| 2 | `Domain/Models/{Alan}/{Ad}ExecutionContextModel.cs` | Salt-okunur bağlam |
| 3 | `Domain/Models/{Alan}/{Ad}ExecutionResultModel.cs` | Terminal sonuç |
| 4 | `Domain/Managers/{Alan}/{Ad}ExecutionManager.cs` | `PrepareAsync` / `BuildExecutionContextAsync` / `ExecuteAsync` |
| 5 | `Application/BackgroundJobs/{Alan}/{Ad}Job.cs` | `ApiContractCheckerTenantBackgroundJob<TArgs>` türer |
| 6 | AppService: tetikleme + durum sorgulama |
| 7 | Controller: `POST` tetikle, `GET {id}/status` |
| 8 | Test: **idempotans** (aynı job iki kez → tek sonuç) |

İş birimi sınırı pazarlıksızdır: hazırlık kısa UOW → bağlam kısa UOW →
**çalıştırma UOW'suz** → tamamlama kısa UOW
([kanonik API Contract Checker gerçeği](../../../../../../docs/PTN-ASSURANCE-PLATFORM-WIKI.md#7-api-contract-checker-gercegi)).

## Tip E — Yeni spec formatı / motor bileşeni

Ayrı tarif: [`../../acc-comparison-engine/references/engine-component-recipe.md`](../../acc-comparison-engine/references/engine-component-recipe.md).
Manager, AppService ve controller **değişmez** — değişiyorsa soyutlama sızmıştır.

## Manifest dışına çıkma testi

Listede olmayan bir dosya yazmak üzereysen üç sorudan birine "evet" demelisin:

1. Gereksinim gerçekten yeni bir kavram getiriyor mu? → Önce
   [kanonik checker kuralları](../../../../../../docs/PTN-ASSURANCE-PLATFORM-WIKI.md#13-checker-gelistirme-kurallari)
   tablosunu güncelle, sonra yaz.
2. İkinci gerçek uygulama var mı? → Taban çıkarmak meşru.
3. ABP ve mevcut tabanlar gerçekten kapsamıyor mu? → Kanıtı yaz.

Üçü de hayırsa o dosyayı yazma.
