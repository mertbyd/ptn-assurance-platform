> [!WARNING] ASKIYA ALINDI (2026-08-14)
> Bu blok **iptal edildi**. Önce wiki denetimi (AUDIT-0001 ve sonraki turlar) kapanacak,
> sonra bu plan bulgulara göre yeniden yazılacak. Bu hâliyle **uygulanmaz**.

# AJAN GÖREVİ — KBP-90 … KBP-94 · Veri modeli, senaryo kataloğu, koşum, runner ve yargı

Bu tek belge **beş fazlık** bir iştir. Beş ayrı branch, beş ayrı teslim, tek mimari.
Fazlar **sırayla** yapılır; her faz tek başına derlenebilir olmalıdır.

Köprü (KBP-88/89) bittikten sonra başlar. Bu blok bittiğinde ürün **model olmadan**
uçtan uca çalışır: elle yazılmış bir Arazzo belgesi koşar, hüküm verilir, teşhis raporlanır.

---

## 0. Faz haritası

| Faz | Branch | Ne | Boyut |
|---|---|---|---|
| **F1** | `KBP-90` | Şema sahipliği, 5 lookup, sözlük, DbContext, ilk migration | ~30 dosya |
| **F2** | `KBP-91` | `test_scenarios` aggregate + CRUD dikey dilimi + **malzeme mührü** + yayın kapıları | ~34 dosya |
| **F3** | `KBP-92` | `test_runs` + tetikleyiciler + ortam çözümü + background job + idempotent claim + stale süpürücü | ~32 dosya |
| **F4** | `KBP-93` | Runner adapter (Redocly Respect) + HAR artefaktı + BLOB deposu | ~26 dosya |
| **F5** | `KBP-94` | Yargı: oracle dağıtıcısı + `test_run_results` + `test_result_findings` + teşhis + rapor read model | ~34 dosya |

```
Depo   : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül  : ptn-test-module   (solution: Ptn.TestModule.slnx)
Motor  : PostgreSQL
Commit : #KBP-9x <type>: <past-tense English description>
```

Faz başına **derlenebilir dilimler** hâlinde commit; faz başına **en fazla 6 commit**.

---

## 1. YAZMA KAPISI — her dosya tipi için zorunlu

Dosya açmadan önce o satırdaki referansı **oku**. Kural hatırlamıyorsan yazma.

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| Entity | `house-profile.md` → *Entity data shell* | `nexum-abp-filemodule/src/*.Domain/Entities/Files/*.cs` |
| Lookup entity | aynı + §3.3 | Foundation `LookupEntity<TKey>` |
| Manager | `house-profile.md` → *Base classes* | `nexum-abp-filemodule/.../Managers/Files/FileCategoryManager.cs` |
| Repository | `data-access.md` | `nexum-abp-filemodule/.../Repository/Files/EfCoreFileEntryRepository.cs` |
| EF Configuration | `data-access.md` | `ptn-api-contract-checker/src/*.EntityFrameworkCore/Configurations/**` |
| AppService | `house-profile.md` → *Contracts live in Application.Contracts* | `ptn-api-contract-checker/.../Services/Sources/SpecSourceAppService.cs` |
| Servis arayüzü | aynı | `ptn-api-contract-checker/.../Application.Contracts/Services/**` |
| DTO | `mapping.md` → *DTOs* | `ptn-api-contract-checker/.../Application.Contracts/Dtos/**` |
| Validator | `mapping.md` → *Validation* | `ptn-api-contract-checker/.../FluentValidation/**` |
| Mapper | `house-profile.md` → *Mapper files contain declarations only* | `ptn-api-contract-checker/.../Mappers/Diagnosis/DiagnosisMapper.cs` |
| Controller | `layers-and-files.md` → *Controller* | `ptn-api-contract-checker/src/*.HttpApi/Controllers/**` |
| Background job | `layers-and-files.md` | `ptn-api-contract-checker/.../BackgroundJobs/**` |
| Sabit / kod kümesi | `house-profile.md` → *Stable strings* | `ptn-api-contract-checker/.../Constants/**/*Codes.cs` |

**Kanonik kararlar (bu blokta bağlayıcı):** `ADR-0014` (yazarlık), `ADR-0015` (koşum sınırı,
UoW, modül entegrasyonu), **`ADR-0016`** (veri modeli — bu bloğun anayasası), `ADR-0017`
(assertion kaynakları), `ADR-0018`/`ADR-0019` (köprü), **`ADR-0020`** (malzeme mührü),
`RULE-0002` (şema/migration sahipliği), `RULE-0005`, `RULE-0006`, `RULE-0008`.
Şema kaynağı: **`docs/wiki-brain/04-Architecture/Test-Platform-Schema.dbml`** — kolon adları
oradan alınır, uydurulmaz.

---

## 2. Bloğun değişmezleri

1. **4 ana tablo + 5 lookup. Başka tablo açılmaz.** `test_environments`, `run_steps`,
   `reports`, `business_rules`, `operation_links`, `effect_footprints` **yoktur** (ADR-0016).
2. **Kendi koşum motorumuz yazılmaz.** Arazzo'yu dış runner (Redocly Respect) icra eder.
3. **Koşum ve yargı anlarında model/LLM çağrısı yoktur** (RULE-0005).
4. **Checker tablosuna FK verilmez, ortak transaction açılmaz.** Modül dışı kimlikler düz
   `uuid`'dir.
5. **Ortam bağlaması tablo değil, ABP `Setting`'dir**; koşumda çözülür ve `test_runs` satırına
   snapshot'lanır.
6. **Migration yalnız kendi üç şemamız için üretilir**: `test_lookup`, `test_catalog`,
   `test_run`. Auth/Notification/checker tablosu için migration **üretilmez**.
7. **PostgreSQL.**

---

## 3. Base zinciri — bu blokta Foundation gerçekten kullanılıyor

Köprüde aggregate yoktu; burada var. **Hiçbir sınıf doğrudan `DomainService`,
`ApplicationService` veya `EfCoreRepository<>` türetmez.**

### 3.1 Aggregate'li iş

```csharp
using Nexum.Abp.Foundation.Managers;                 // FoundationManager<TEntity, TKey>
using Nexum.Abp.Foundation.Application.Services;     // BaseApplicationService<...10 arg>
using Nexum.Abp.Foundation.EntityFrameworkCore;      // BaseEfCoreRepository<TDbContext, TEntity, TKey>
using Nexum.Abp.Foundation.Repositories;             // IBaseRepository<TEntity, TKey>
using Nexum.Abp.Foundation.Querying;                 // RepositoryQuery/Page/Order<T>
```

`BaseApplicationService` generic sırası **birebir**:
`<TEntity, TKey, TDto, TListInput, TCreateDto, TUpdateDto, TCreateModel, TUpdateModel, TManager, TRepository>`

`FoundationManager` hazır veriyor — **yeniden yazma**: `EnsureExistsAsync`,
`EnsureAllExistAsync`, `EnsureUniqueAsync` (4 aşırı yükleme), `EnsureUniqueValuesAsync`,
`EnsureDistinctValues`, `NormalizeRequiredText`, `NormalizeOptionalText`, `EnsureEnumDefined`,
`AlreadyExistsErrorCode`.

### 3.2 Aggregate'siz iş (dağıtıcı, derleyici, çözümleyici)

Modül base'i: `TestModuleDomainService` (manager) ve `TestModuleAppService` (servis).

### 3.3 Lookup yığını **hazır** — elle CRUD yazma

Foundation şunları veriyor: `LookupEntity<TKey>` (`Code`, `Name`, `Description`),
`LookupManager<TEntity,TKey>`, `LookupAppService<TEntity,TKey,TDto,TCreateDto,TUpdateDto,TManager,TRepository>`,
`ILookupAppService`, `LookupDto<TKey>`, `LookupCreateDto`, `LookupUpdateDto`, `LookupListInput`,
`LookupCreateModel`, `LookupUpdateModel`, validator'lar ve mapper'lar,
`EfCoreLookupRepository<TDbContext,TEntity,TKey>`, `ILookupRepository<TEntity,TKey>`.

> **Beş lookup için yazılacak tek şey:** entity (varsa feature alanı), repository, manager,
> DTO (yalnız ek alan), servis, controller, configuration. **Liste/sayfalama/CRUD gövdesi
> yazılmaz.** Bir lookup'ın CRUD'ını elle yazarsan iş reddedilir.

---

## 4. Ortak kurallar — beş fazda da geçerli

| Konu | Kural |
|---|---|
| Dosya bütçesi | Faz başına **≤ 35 dosya**. Aşarsa listenin **sonundan** kes, bir sonraki faza taşı |
| Katman | `Controller → AppService → Manager → Repository`. ABP'de olmayan katman açılmaz |
| Klasör | `Entities/`, `Managers/`, `Interface/`, `Models/`, `Dtos/`, `Services/`, `FluentValidation/`, `Permissions/`, `Mappers/`, `Configurations/`, `Repository/`, `Controllers/`, `Constants/`, `ExceptionCodes/`, `Localization/`, `BackgroundJobs/` |
| Contracts | **DTO, servis arayüzü, validator, permission → `Application.Contracts`**. Uygulama ve mapper → `Application` |
| EF Core | **Yalnız** `Configurations/`, `Repository/`, `EntityFrameworkCore/`, `Migrations/`. Mapperly bağımlılığı **yok** |
| Entity | Veri kabuğu: alanlar, `internal set`, protected EF ctor, atama-only public ctor. `Check`/`Ensure`/`Normalize`/`Validate`/geçiş metodu **yok** — hepsi manager'da |
| Servis | Karar vermez, hesap yapmaz, çevirmez. `private` iş metodu, eşleme sözlüğü, fingerprint, redaksiyon **yok** |
| Mapper | Yalnız `public partial X MapToY(Z input);`. `[MapProperty]`, gövde, `.Select`, `if` **yok** |
| Tip | Her tip kendi dosyasında. **Nested tip yok**, dosya içinde ikinci tip yok |
| Sabit | Her anlamlı string `Domain.Shared` sahipli. `Lookups/` klasörü **yalnız** gerçek lookup tablosu olan kodlar için |
| Yorum | Her tip `// islevi:` + `// sistemdeki gorevi:`; her authored metot tek satır niyet yorumu |
| Metot | ≤ 25 satır, ≤ 2 iç içe seviye. 3'ten fazla dönüş değeri → adlandırılmış model (**tuple yasak**) |
| Commit | Boş dosya, yer tutucu, kullanılmayan using girmez |
| Arkeoloji | `git log`, eski revizyon diff'i, ilgisiz ağaç gezintisi **yok** |
| Build/test | Faz içinde **ara adımda çalıştırma**; faz sonunda **bir kez** |

---

# FAZ 1 — `KBP-90` · Şema, lookup'lar ve sözlük

**Amaç:** Modülün üç şemasını sahiplenmek, beş lookup'ı Foundation yığınıyla kurmak, kod
sözlüğünü `Domain.Shared`'da sabitlemek ve ilk migration'ı üretmek.

**Neden önce:** yanlış kurulan şema her fazda migration borcu yaratır (RULE-0002).

### Dosyalar

**`Domain.Shared/Constants/Runs/`** — kod sabitleri (lookup tablosu **var**, o yüzden
`Lookups/` alt klasörü **doğru** kullanım)
1. `Lookups/TestRunStatusCodes.cs` — `Pending` `Running` `Completed` `Cancelled` `Aborted` `TimedOut` + `All`
2. `Lookups/TestOutcomeStatusCodes.cs` — `Passed` `Failed` `Broken` `Skipped` `Inconclusive` + `All`
3. `Lookups/TestFailureCategoryCodes.cs` — `Contract` `Persistence` `Business` `Transport` `Technical` + `All`
4. `Lookups/TestTriggerKindCodes.cs` — `Manual` `Scheduled` `Api` `Webhook` `ContractChange` + `All`
5. `Lookups/TestScenarioStateCodes.cs` — `Draft` `PendingApproval` `Published` `Deprecated` + `All`
6. `TestModuleTableNames.cs` — tablo adları
7. `TestModuleRoutes.cs` — route şablonları + swagger grubu
8. `TestRunConsts.cs` — uzunluk sınırları (`MaxTitleLength`, `MaxErrorMessageLength`, `MaxTraceIdLength = 32`, `MaxHashLength = 64`, `MaxDiagnosisReportBytes = 4096`)
9. `ExceptionCodes/Runs/TestModuleRunErrorCodes.cs`

**`Domain/Entities/Lookups/`** — beşi de `LookupEntity<Guid>` türer
10. `TestRunStatus.cs`
11. `TestOutcomeStatus.cs` — **ek alan `BreaksBuild`** (bool, ADR-0016 §F)
12. `TestFailureCategory.cs`
13. `TestTriggerKind.cs`
14. `TestScenarioState.cs`

> Beş lookup **`IMultiTenant` taşımaz** (global referans verisi). `IPassivable` taşır.

**`Domain/Interface/Lookups/`**
15–19. Beş repository arayüzü (`ITestRunStatusRepository` …) — `ILookupRepository<T,Guid>` türer

**`Domain/Managers/Lookups/`**
20–24. Beş manager — `LookupManager<T, Guid>` türer, gövde **boş** (taban yeterli)

**`EntityFrameworkCore/`**
25. `EntityFrameworkCore/TestModuleDbContext.cs` **güncelle** — beş `DbSet`
26. `EntityFrameworkCore/TestModuleDbContextModelCreatingExtensions.cs` **güncelle** — şema adları `TestModuleDbProperties`'ten
27. `Configurations/Lookups/LookupEntityConfigurationBase.cs` — ortak `Code`/`Name`/`Description` + unique `Code`
28. `Configurations/Lookups/TestOutcomeStatusConfiguration.cs` — `breaks_build`
29. `Repository/Lookups/EfCoreLookupRepositoryBase.cs` (gerekiyorsa) + beş somut repository
30. `TestModuleDataSeedContributor.cs` — beş lookup'ın kod seti; **idempotent**

**Migration:** `dotnet ef migrations add Initial_TestModuleSchema` — yalnız üç şema.

### Kabul kriterleri
- Üç şema (`test_lookup`, `test_catalog`, `test_run`) `TestModuleDbProperties` üzerinden
  configuration'dan ezilebiliyor.
- Beş lookup tablosu oluşuyor, seed idempotent (iki kez koşunca çift satır yok).
- `test_outcome_statuses.breaks_build` dolu: `Failed`/`Broken` → `true`, diğerleri → `false`.
- Lookup CRUD'ı için **elle yazılmış tek satır gövde yok**; hepsi Foundation tabanından.
- Migration yalnız kendi üç şemamıza dokunuyor.

**Commit:** `#KBP-90 feat: created the test module schema ownership, lookup catalog and seed data`

---

# FAZ 2 — `KBP-91` · `test_scenarios` aggregate ve yayın kapıları

**Amaç:** Senaryo sürümü aggregate'ini, CRUD dikey dilimini, **ADR-0020 malzeme mührünü** ve
yayın kapılarını kurmak.

### Dosyalar

**`Domain/Entities/Catalog/`**
1. `TestScenario.cs` — `AuditedAggregateRoot<Guid>`, `IMultiTenant`. Alanlar **DBML'den**:
   `ScenarioKey`, `VersionNo`, `Title`, `Description`, `StateId`, `SourceDocument`, `SourceHash`,
   `CompiledDocument`, `CompiledHash`, **malzeme mührü:** `RulesFingerprint`, `SpecSnapshotId`,
   `SpecFingerprint`, `DbConnectionId`, `DbSchemaFingerprint`, `ProfileFingerprint`,
   `AssertionCount`, `DerivabilityCode`, `AuthoredByAgent`, `AgentModelRef`, `ApprovedBy`,
   `ApprovedAt`, `ApprovalBoundToHash`, `Notes`. **Tümü `internal set`; metot yok.**

**`Domain/Models/Catalog/`**
2. `TestScenarioCreateModel.cs`
3. `TestScenarioUpdateModel.cs`
4. `TestScenarioPublishModel.cs`
5. `TestScenarioMaterialSeal.cs` — altı mühür alanı + `IsComplete`
6. `TestScenarioPublishDecision.cs` — `IsPublishable`, `FailedGateCodes[]`, `Warnings[]`

**`Domain/Interface/Catalog/`**
7. `ITestScenarioRepository.cs` — `IBaseRepository<TestScenario, Guid>` türer; `FindLatestVersionAsync`, `FindPublishedAsync`, `GetNextVersionNoAsync`

**`Domain/Managers/Catalog/`**
8. `TestScenarioManager.cs` — `FoundationManager<TestScenario, Guid>`. Normalizasyon, sürüm
   numarası üretimi, `(scenario_key, version_no)` ve `(scenario_key, source_hash)` benzersizliği,
   durum geçişi `Draft → PendingApproval → Published → Deprecated`, onay hash bağlama
9. `ScenarioPublicationGateManager.cs` — **beş kapı**:
   (1) şema geçerliliği · (2) türetilebilirlik · (3) `AssertionCount > 0` ·
   (4) **malzeme bütünlüğü** (ADR-0020 §B) · (5) **`sourceDescriptions` ↔ `SpecSnapshotId`
   tutarlılığı**. Kapı düşerse `FailedGateCodes` dolar, `Published`'a geçilmez

**`EntityFrameworkCore/`**
10. `Configurations/Catalog/TestScenarioConfiguration.cs` — iki unique index, `HasMaxLength` sınırları `Domain.Shared` sabitlerinden, `Restrict`
11. `Repository/Catalog/EfCoreTestScenarioRepository.cs` — `BaseEfCoreRepository<TestModuleDbContext, TestScenario, Guid>`

**`Application.Contracts/Dtos/Catalog/`**
12–18. `TestScenarioDto`, `TestScenarioListInput` (`PagedResultRequestDto` türer),
`CreateTestScenarioDto`, `UpdateTestScenarioDto`, `PublishTestScenarioDto`,
`TestScenarioMaterialSealDto`, `TestScenarioPublishDecisionDto`

**`Application.Contracts/Services/Catalog/`**
19. `ITestScenarioAppService.cs` — CRUD + `SubmitForApprovalAsync`, `PublishAsync`, `DeprecateAsync`, `EvaluatePublicationAsync`

**`Application.Contracts/FluentValidation/Catalog/`**
20–23. Dört validator. Hash alanları 64 hex; `ScenarioKey` formatı; `ResponseFormat` yok

**`Application.Contracts/Permissions/`** (güncelle)
24. `TestModulePermissions.cs` — `Scenarios.Default|Create|Update|Delete|Publish|Approve`
25. `TestModulePermissionDefinitionProvider.cs`

**`Application/Services/Catalog/`**
26. `TestScenarioAppService.cs` — `BaseApplicationService<TestScenario, Guid, TestScenarioDto, TestScenarioListInput, CreateTestScenarioDto, UpdateTestScenarioDto, TestScenarioCreateModel, TestScenarioUpdateModel, TestScenarioManager, ITestScenarioRepository>`. Policy adları override edilir

**`Application/Mappers/Catalog/`**
27. `TestScenarioMapper.cs` — yalnız partial bildirimler

**`HttpApi/Controllers/Catalog/`**
28. `TestScenarioController.cs`

**Testler**
29–32. Yayın kapısı testleri (aşağıdaki kabul kriterlerinin birebir karşılığı)

### Kabul kriterleri
- Türetilemeyen assertion içeren sürüm `Published` **olamıyor** (RULE-0006).
- `AssertionCount = 0` olan sürüm reddediliyor.
- **Dört malzemeden biri eksikse yayın reddediliyor** (ADR-0020 §B/4).
- **`sourceDescriptions` `SpecSnapshotId`'ye çözülmüyorsa yayın reddediliyor** (ADR-0020 §B/5).
- Onay `ApprovalBoundToHash`'e bağlı; belge değişince onay geçersiz.
- `(scenario_key, version_no)` ve `(scenario_key, source_hash)` unique çalışıyor.
- Senaryo **silinmiyor** (`Restrict`); `DeleteAsync` reddediyor.

**Commit:** `#KBP-91 feat: created the scenario catalog aggregate with material sealing and publication gates`

---

# FAZ 3 — `KBP-92` · Koşum kaydı, tetikleyiciler ve dayanıklı job

**Amaç:** `test_runs` aggregate'i, ortam çözümü, tetikleyici sözlüğü, ABP background job,
idempotent claim ve asılı koşum süpürücüsü.

### Dosyalar

**`Domain/Entities/Runs/`**
1. `TestRun.cs` — `AuditedAggregateRoot<Guid>`, `IMultiTenant`. `ScenarioId` (nullable, ad-hoc/dryRun),
   `TestKey`, `HistoryId`, `EnvironmentKey`, `SpecSnapshotId`, `DbConnectionId`, `RunStatusId`,
   `TriggerKindId`, `TriggerRef`, `TraceId` (**32 küçük harf hex, `Guid` değil**),
   `SpecFingerprint`, `DbSchemaFingerprint`, `ProfileFingerprint`, `RunnerRef`, `IsDryRun`,
   `Attempt`, `HarBlobName`, `StartedAt`, `CompletedAt`, `ErrorCode`

**`Domain/Models/Runs/`**
2. `TestRunStartModel.cs`
3. `TestRunEnvironment.cs` — çözülmüş ortam: `BaseUrl`, `SpecSnapshotId`, `DbConnectionId`, `SecretRef`
4. `TestRunClaimResult.cs`
5. `MaterialDriftResult.cs` — hangi mühür kaydı, `IsDrifted`, `DriftedMaterialCodes[]`

**`Domain/Interface/Runs/`**
6. `ITestRunRepository.cs` — `FindStaleRunningAsync`, `ExistsAttemptAsync`
7. `ITestDataSandbox.cs` — sandbox portu (reset, tekillik)

**`Domain/Managers/Runs/`**
8. `TestRunManager.cs` — `FoundationManager<TestRun, Guid>`. `HistoryId` = **SHA-256**
   (`test_key ¦ environment_key ¦ kanonik girdiler`) — **MD5 yasak**; `Attempt` artışı;
   `UNIQUE (test_run_id, attempt)` ihlalini gürültülü hataya çevirme
9. `TestRunStateManager.cs` — durum geçişleri + **idempotent claim** (`StartAsync → bool`);
   tekrar teslimde no-op
10. `TestRunEnvironmentManager.cs` — ABP `Setting`'ten ortam çözümü, koşu satırına snapshot
11. `MaterialDriftManager.cs` — **ADR-0020 §C**: dört mührü yeniden hesapla, karşılaştır,
    kayma varsa `Inconclusive` + `Technical`, kayan malzemeyi adıyla döndür
12. `StaleRunRecoveryManager.cs` — `(run_status_id, started_at)` indeksi üzerinden asılı
    `Running` süpürücüsü

**`Domain/Settings/`** (güncelle)
13. `TestModuleSettings.cs` — `Environment.*`, `Retention.Days`, `Run.StaleTimeoutMinutes`, `Run.MaxParallelPerEnvironment`
14. `TestModuleSettingDefinitionProvider.cs`

**`EntityFrameworkCore/`**
15. `Configurations/Runs/TestRunConfiguration.cs` — `UNIQUE (test_run_id, attempt)`,
    `(tenant_id, creation_time DESC)`, `(run_status_id, started_at)`, `Restrict`
16. `Repository/Runs/EfCoreTestRunRepository.cs`

**`Application.Contracts/`**
17–21. `TestRunDto`, `TestRunListInput`, `StartTestRunDto`, `TestRunClaimResultDto`, `MaterialDriftResultDto`
22. `Services/Runs/ITestRunAppService.cs` — `StartAsync`, `GetAsync`, `GetListAsync`, `CancelAsync`
23. `FluentValidation/Runs/StartTestRunDtoValidator.cs`
24. `Permissions/` güncelle — `Runs.Default|Trigger|Cancel`

**`Application/`**
25. `Services/Runs/TestRunAppService.cs` — `BaseApplicationService<...>`; `StartAsync`
    **kısa UoW**: satır `Pending`, commit, `IBackgroundJobManager.EnqueueAsync`, `202 + runId`
26. `BackgroundJobs/ExecuteTestRunJob.cs` — `AsyncBackgroundJob`, tenant scope, cancellation.
    **UoW sınırları (ADR-0015 §B):** [UoW] claim → **UoW dışı** hazırlık/icra/yargı →
    [UoW] terminal yazım. `OperationCanceledException` → `Cancelled`, `Technical` **değil**
27. `BackgroundWorkers/StaleRunSweeperWorker.cs`
28. `Mappers/Runs/TestRunMapper.cs`

**`HttpApi/Controllers/Runs/`**
29. `TestRunController.cs` — `POST /api/test-runs` → 202

### Kabul kriterleri
- Uzun koşu **HTTP isteği içinde yaşamıyor**; uç 202 + runId dönüyor.
- Aynı job iki kez teslim edilirse ikinci claim **no-op**; çift satır yok.
- Asılı `Running` koşu süpürücüyle `Aborted`'a düşüyor.
- `trace_id` 32 küçük harf hex; `Guid` **değil**.
- Malzeme kayması `Failed` değil **`Inconclusive`** üretiyor ve kayan malzemeyi adıyla
  raporluyor (ADR-0020 §C).
- Aynı ortamda çakışan koşum sıraya alınıyor.
- Checker'ın uzak çağrısıyla **DB transaction açık tutulmuyor**.

**Commit:** `#KBP-92 feat: created durable test run execution with environment resolution and drift detection`

---

# FAZ 4 — `KBP-93` · Runner adapter, HAR artefaktı ve BLOB deposu

**Amaç:** Redocly Respect'i süreç sınırı arkasından çalıştırmak, HAR + JSON çıktısını almak,
ağır artefaktı BLOB'a yazmak.

**Değişmez:** **kendi HTTP koşum motorumuz yazılmaz** (ADR-0015 §A).

### Dosyalar

**`Domain.Shared/Constants/Runner/`**
1. `RunnerConsts.cs` — sabit imaj sürümü, `ExecutionTimeoutMs`, `MaxFetchTimeoutMs`, `MaxHarBytes`
2. `RunnerSeverityCodes.cs` — `STATUS_CODE_CHECK=error`, `SUCCESS_CRITERIA_CHECK=error`,
   `SCHEMA_CHECK=warn`, `CONTENT_TYPE_CHECK=warn` (ADR-0015 §E)
3. `ExceptionCodes/Runner/TestModuleRunnerErrorCodes.cs`

**`Domain/Models/Runner/`**
4. `WorkflowRunRequest.cs` — belge, girdiler, ortam, timeout
5. `WorkflowRunResult.cs` — `ExitCode`, `HarContent`, `JsonSummary`, `RunnerRef`, `IsBroken`
6. `HarEntry.cs`
7. `HarRequest.cs`
8. `HarResponse.cs`
9. `WorkflowStepOutcome.cs`

**`Domain/Interface/Runner/`**
10. `IWorkflowRunnerPort.cs` — `RunAsync(WorkflowRunRequest, CancellationToken)`
11. `IArtifactStore.cs` — `SaveAsync`, `GetLinkAsync`, `DeleteAsync`

**`Domain/Managers/Runner/`**
12. `ArazzoCompilerManager.cs` — **`x-checknexus-db` → gerçek Arazzo adımı** (DB Checker
    `POST /assertions/row|count|absent` ucuna giden sıradan HTTP adımı). **XPath criteria
    yasağı** burada uygulanır
13. `HarParserManager.cs` — HAR 1.2 → `HarEntry` listesi
14. `RunnerInvocationManager.cs` — timeout, iptal, `Broken` işaretleme, sandbox reset kararı

**`Application/Services/Runner/`**
15. `WorkflowRunnerService.cs` — `IWorkflowRunnerPort` uygular; **süreç sınırı**:
    `redocly/cli` sabit sürüm, **girdiler env değişkeni/dosya ile** (CLI bayrağıyla **değil**),
    `--har-output` + `--json-output`, `--execution-timeout`, `--max-fetch-timeout`,
    üstüne **sert kill**. `--no-secrets-masking` **asla açılmaz**
16. `ArtifactStoreService.cs` — ABP BLOB Storing; **S3 uyumlu** sağlayıcı; satırda yalnız
    `HarBlobName`; TTL

**`Application.Contracts/`**
17–19. `WorkflowRunResultDto`, `ArtifactLinkDto`, `Services/Runner/IArtifactAppService.cs`
20. `Permissions/` güncelle — `Artifacts.Default|Download`

**`Application/`**
21. `Services/Runner/ArtifactAppService.cs`
22. `Mappers/Runner/RunnerMapper.cs`

**`HttpApi/Controllers/Runner/`**
23. `ArtifactController.cs` — `resource_link` döner, gövde döndürmez

**Testler**
24–26. Derleyici testi (`x-checknexus-db` → HTTP adımı), XPath reddi, timeout/kill

### Kabul kriterleri
- `x-checknexus-db` uzantısı **gerçek bir Arazzo adımına** derleniyor; runner'a plugin
  yazılmadı, fork yok.
- XPath criteria içeren belge yayın/derleme kapısında **engelleniyor**.
- Girdiler süreç listesinde **görünmüyor** (env/dosya ile veriliyor).
- Runner çökerse koşum `Broken`; sandbox sıfırlanıyor; **adım seviyesinde devam yok**.
- HAR BLOB'a yazılıyor, satırda yalnız `HarBlobName` var.
- `RunnerRef` her koşuda hangi runner sürümüyle koşulduğunu tutuyor.

**Commit:** `#KBP-93 feat: created the external workflow runner boundary with HAR capture and artifact storage`

---

# FAZ 5 — `KBP-94` · Yargı, bulgu kaydı ve teşhis

**Amaç:** HAR'ın **her** girdisini uygunluk kontrolünden geçirmek, kırmızıları teşhise
göndermek, hüküm ve bulguları tek atomik yazımla kaydetmek, raporu read model olarak sunmak.

**Değişmez:** hüküm **yalnız checker'ındır**; bu fazda model çağrısı **yoktur** (RULE-0005).

### Dosyalar

**`Domain/Entities/Runs/`**
1. `TestRunResult.cs` — `CreationAuditedAggregateRoot<Guid>`, `IMultiTenant`. `TestRunId`,
   `StepKey`, `OutcomeStatusId`, `FailureCategoryId`, `DurationMs`, `TakenBranchPath`,
   `DiagnosisReport` (**owned jsonb, ≤ 4 KB**), `ErrorCode`
2. `TestResultFinding.cs` — `CreationAuditedEntity<Guid>`, **`IMultiTenant` zorunlu**
   (ABP tenant filtresi miras alınmaz — ADR-0016 §D). `TestRunResultId`,
   `SourceCheckerCode`, `RuleRef`, `Fingerprint`, `Severity`, `Message`, `Location`

**`Domain/Models/Judgment/`**
3. `StepJudgment.cs`
4. `ConformanceOutcome.cs`
5. `DiagnosisOutcome.cs`
6. `RunJudgmentResult.cs`
7. `FindingRecord.cs`

**`Domain/Interface/Judgment/`**
8. `ITestRunResultRepository.cs` — findings `Include` ile **tek sorguda** detay
9. `IConformanceOraclePort.cs`
10. `IPersistenceOraclePort.cs`

**`Domain/Managers/Judgment/`**
11. `OracleDispatchManager.cs` — **HAR'ın HER entry'si** → `AssertResponseAsync`
    (yeşiller dâhil, ADR-0015 §D); kırmızılar → `DiagnoseAsync`
12. `JudgmentManager.cs` — outcome + `failure_category` ataması; `breaks_build` kolonundan
    politika okuma (**`if (code == "Failed")` yazma**)
13. `FindingRecordManager.cs` — `source_checker_code` her bulguda dolu; redaksiyon
    **ACL sınırında** yapılmış olarak gelir; uzunluk sınırları uygulanır
14. `TerminalWriteManager.cs` — **tek atomik yazım**: koşu durumu + results + findings;
    **ayrı yeni UoW** (ADR-0016 §J)

**`EntityFrameworkCore/`**
15. `Configurations/Runs/TestRunResultConfiguration.cs` — owned jsonb, `Cascade`
16. `Configurations/Runs/TestResultFindingConfiguration.cs` — `Cascade`
17. `Repository/Runs/EfCoreTestRunResultRepository.cs`

**`Application.Contracts/Dtos/Judgment/`**
18–24. `TestReportDetailDto`, `TestReportListItemDto` (**findings ve `diagnosis_report`
projekte etmez**), `TestRunResultDto`, `TestResultFindingDto`, `DiagnosisReportDto`,
`RunSummaryDto`, `CoverageReportDto`

**`Application.Contracts/Services/Judgment/`**
25. `ITestReportAppService.cs` — `GetDetailAsync`, `GetListAsync`, `GetCoverageAsync`

**`Application.Contracts/`**
26. `FluentValidation/Judgment/TestReportListInputValidator.cs` — sıralama **allowlist**
27. `Permissions/` güncelle — `Reports.Default|Export`

**`Application/`**
28. `Services/Judgment/TestReportAppService.cs`
29. `Services/Judgment/JudgmentDispatchService.cs` — job'dan çağrılır
30. `Mappers/Judgment/TestReportMapper.cs`
31. `Mappers/Judgment/FindingMapper.cs`

**`HttpApi/Controllers/Judgment/`**
32. `TestReportController.cs`

**Testler**
33–35. Yeşil adımın şema ihlali yakalanıyor mu · üç hakem çelişkisi · tenancy sızıntısı

### Kabul kriterleri
- **HAR'ın her entry'si** uygunluk kontrolünden geçiyor; `200` dönen ama şemayı ihlal eden
  adım yakalanıyor.
- Kırmızı adım teşhise gidiyor; RFC 9457 raporu ≤ 4 KB, `diagnosis_report` jsonb'sine yazılıyor.
- Her bulgu `source_checker_code` taşıyor (`ApiContract` / `DatabaseComparison` / `Runner`).
- `breaks_build` politikası koddan değil **kolondan** okunuyor.
- Terminal yazım **tek atomik**; koşu durumu + results + findings birlikte.
- Liste ucu findings ve `diagnosis_report` **projekte etmiyor**.
- `title` kalıcı yazılmıyor; `error_code` + yerelleştirmeden üretiliyor.
- Tenancy testi: başka kiracının bulgusu görünmüyor (4 tabloda da `IMultiTenant`).
- **Ham stack trace tabloya yazılmıyor**; `error_code` + `trace_id` yazılıyor.

**Commit:** `#KBP-94 feat: created the judgment pipeline with oracle dispatch, finding records and report read model`

---

## 5. Komut hijyeni

- build/test çağrılarına **en az 600000 ms** timeout; kısa timeout MSBuild sürecini canlı
  bırakır ve Fody DLL kilidi doğurur.
- `dotnet build Ptn.TestModule.slnx -m:1`; ilk build restore etsin, sonrakiler `--no-restore`.
- Kilit hatasında: `dotnet build-server shutdown` → kalan süreçleri kapat → **bir kez** dene.
- Aynı komutu **döngüde tekrarlama**; iki denemede geçmiyorsa dur.
- Kilit/timeout'u **kod hatası sanma**.
- Tek engelde **10 dakikadan fazla** harcama; dur, tek paragraf rapor et, devam et.
- Migration üretirken `--project` ve `--startup-project` açıkça verilir; başka modülün
  migration'ı **üretilmez**.

---

## 6. Faz geçiş kapısı

Her fazın sonunda, **bir kez**:

1. `dotnet build Ptn.TestModule.slnx -m:1`
2. `dotnet test Ptn.TestModule.slnx --no-restore`
3. `/abp-backend-dev` mimari incelemesi — katman zinciri, base zinciri, klasör düzeni,
   mapper saflığı, servis saflığı (private iş metodu yok), contracts yerleşimi, nested tip yok,
   yorum çifti, metot sınırları
4. `/backend-verify` gate'i — diff taraması, commit grameri, güvenlik ve veri sınırı
5. **Gate kırmızıysa bir sonraki faza geçilmez.** Düzeltme aynı branch'te yapılır.
6. Raporda: dosya listesi, bir sonraki faza devredilenler, yapılan **her varsayım**.
