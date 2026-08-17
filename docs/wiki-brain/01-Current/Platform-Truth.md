---
id: CURRENT-0001
type: current
status: active
title: PTN Assurance workspace current truth
updated: 2026-08-16
decision_refs:
  - ADR-0001
  - ADR-0002
  - ADR-0003
  - ADR-0004
  - ADR-0005
  - ADR-0006
  - ADR-0009
  - ADR-0012
  - ADR-0013
  - ADR-0016
  - ADR-0017
  - ADR-0023
  - ADR-0024
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0003
  - RULE-0004
---

# Workspace güncel gerçekleri

## Fiziksel gerçek

Kanonik çalışma alanı `C:\Users\mertb\RiderProjects\ptn-assurance-platform` dizinidir. Kökünde
`checkers`, `ptn-test-module`, `vault` ve `docs` vardır.

| Yol | İçerik |
|---|---|
| `checkers/api-contract` | API Contract Checker source, ince host ve test katmanları |
| `checkers/database-comparison` | Database Checker source, ince host ve test katmanları |
| `ptn-test-module` | **Test Module composition host ve modül katmanları** (2026-08-13'te ABP CLI 10.6.0 `module` şablonundan kuruldu; `host/`, `src/`, `test/`) |
| `vault` | İki checker secret portunu uygulayan ortak KV v2 adapteri, yerel config ve doğrulama araçları |
| `docs/wiki-brain` | Bu kanonik Obsidian vault’u |

Kökte root solution, root `src/test` veya `eng` yoktur; her modül kendi solution dosyasını
taşır. Test Module composition hostu 2026-08-13'ten itibaren bu çalışma alanındadır ve
`Ptn.TestModule.slnx` ile derlenir.

### Sürüm kontrolü sınırı — ürün deposu yalnız çalışan kaynağı taşır

> [!IMPORTANT] Kök depo 2026-08-16'da daraltıldı — [[03-Decisions/ADR-0024-Depo-Siniri-Ve-Wiki-Yayin-Yeri|ADR-0024]]
> Kök depo yalnız `ptn-test-module/` kaynağını, `.gitignore`, `NuGet.Config` ve `README.md`
> dosyalarını izler. `docs/` ürün kaynak deposuna eklenmez; `docs/.git` kendi `main` dalı ve
> geçmişi olan **ayrı wiki deposudur** ve yayın yeri **GitHub Wiki**'dir. Kök depoda
> `git add -f docs` çalıştırılmaz.

Sürüm kontrolü dışında tutulanlar:

| Yol | Durum |
|---|---|
| `docs/` | Kök depodan bağımsız Git deposu — kanonik Obsidian vault'u; yayını GitHub Wiki (ADR-0024) |
| `vault/` | **2026-08-16'da takipten çıkarıldı** (`23dd372`). Kaynak diskte durur, kök depo izlemez; tüketim yalnız `CheckNexus.Vault` NuGet paketi üzerinden **PackageReference** iledir |
| `scripts/` | Ignored — release/publish betikleri yerel kalır |
| `AGENTS.md` · `CLAUDE.md` | Ignored — ajan yönerge belgeleri |
| `checkers/api-contract` · `checkers/database-comparison` | **Ayrı ve bağımsız Git depoları**; submodule değil, kök depo tarafından izlenmiyor (her birinin kendi dalları, etiketleri ve `PROVENANCE.md` dosyası var) |

Kök depoda izlenen tek kaynak ağacı `ptn-test-module/`tir (`host/`, `src/`, `test/` ve host
`Dockerfile`'ı). Kök `README.md` sürüm kontrolündeki tek prose dosyasıdır.

> [!WARNING] Geçmiş temizlenmedi
> `vault/` ve eski wiki denemesi Git geçmişinde ve daha önce push edilmiş dallarda durur.
> Depo public olduğu için bu commit'ler hâlâ okunabilir. Geçmiş yeniden yazımı (filter-repo +
> force-push) alınmış bir karar **değildir**.

## Yeteneklerin durumu

| Yetenek | Durum |
|---|---|
| API Contract Checker | NuGet'te sekiz public paket **`0.2.0-alpha.7`** (2026-08-16); kaynak `common.props` ve Test Module consumer sürümü de `0.2.0-alpha.7`, PackageValidation baseline `0.2.0-alpha.2`; 8/8 PackageId registry'de doğrulandı; host/test mevcut |
| Database Checker | NuGet'te sekiz public paket **`0.2.0-alpha.8`** (2026-08-15); kaynak `common.props` ve Test Module consumer sürümü de `0.2.0-alpha.8`, PackageValidation baseline `0.2.0-alpha.7`; 8/8 PackageId registry'de doğrulandı; host/test mevcut |
| CheckNexus.Vault | `0.2.0-alpha.2` NuGet.org'da public; Test Module hostunda compose edildi; iki checker portu aynı singleton adaptere çözülüyor. **Kaynağı 2026-08-16'da kök depodan çıkarıldı (ADR-0024)**; tüketim yalnız PackageReference iledir |
| Test Module tasarımı | **Karara bağlandı (2026-08-13)** — akış [[04-Architecture/Alti-An\|ARCH-0004]]; yazarlık **ADR-0014 + ADR-0017**, koşum **ADR-0015**, kayıt ve teşhis **ADR-0016**; iş listesi PLAN-0003 |
| Test Module kodu | **Üç şema da kuruldu; koşum ve yargı hattı kaynakta** (`ptn-test-module`): 7 src + 2 host + 5 test projesi. Üç migration: **`Initial_TestModuleSchema`** (KBP-90, `test_lookup` — 5 tablo, `code` unique, `breaks_build`, idempotent seed), **`TestScenarioCatalog`** (KBP-92, `test_catalog.test_scenarios`), **`TestRunRecords`** (KBP-93, `test_run.test_runs` / `test_run_results` / `test_result_findings`). KBP-95 runner sınırını, HAR deposunu, dayanıklı job'ı ve oracle dağıtıcısını ekledi (`WorkflowRunnerService`, `HarArtifactService`, `OracleDispatchService`, `ExecuteTestRunJob`). Checker consumer sürümleri API `alpha.7`, Database `alpha.8`, Vault `alpha.2` ile registry'ye hizalıdır; 2026-08-16 Release build 0 hata ve test **316/316** geçti |
| Test Module özerkliği | **KBP-110 ile kapandı (2026-08-15)** — modül artık kendi kendine dönüyor. Üç periyodik ABP worker kayıtlı: `ExpiredQuarantineSweepWorker` (TM-28, süresi dolan karantinayı elle müdahale olmadan temizler), `ScenarioHealthRefreshWorker` (TM-27), `DueScenarioRunWorker` (TM-29). İki yeni migration: **`ScenarioHealthView`** (`test_run.scenario_health` materialized view + `ux_scenario_health` unique index; pass/fail/flaky ve `percentile_cont(0.95)` veritabanında hesaplanır, `is_dry_run` satırlar hariç) ve **`ScenarioSchedule`** (`test_scenarios` + `schedule_cron` / `schedule_enabled` / `next_run_at`). Zamanlama cron'dur (`Cronos` 0.13.0, MIT) ve yalnız yayınlanmış sürümün alanıdır; yeni sürüm yayınlanınca Manager zamanlamayı öncekinden taşır. `ContractChangeTriggerHandler` API Checker'ın `ContractCheckRunStatusChangedEto` olayını `ILocalEventBus` üzerinden dinler; eşleme **snapshot seviyesindedir** (operasyon seviyesi TM-22b, kapalı). Webhook ucu paylaşılan sırla korunur, sır ayarı boşken **403**'tür ve aynı `deliveryId` ikinci koşu üretmez. Uç sayısı **48 → 53**. `REFRESH MATERIALIZED VIEW CONCURRENTLY` gerçek PostgreSQL'de doğrulandı; build 0 hata, non-live test **295/295**, live **2/2** |
| Test Module yazarlık hattı | **KBP-111 ile gerçek verilere bağlandı (2026-08-16)** — dört faz `predev`'de (`79e6758`, `3e60165`, `225ad76`, `45f53b2`). **Grounding**: aday operasyonlar checker envanterinin tüm sayfaları tüketilerek gerçek satırlardan üretilir; serbest operasyon adı girdiye açılmaz, seçim opak `ReferenceId` ile yapılır, kanıt yoksa `PtnOpenQuestionCodes` kararlı kodlarıyla (`OPERATION_REFERENCE_REQUIRED`, `TABLE_SELECTION_REQUIRED`, `EVIDENCE_UNAVAILABLE` …) açık soru döner. **Yayın kapısı**: `ptn_validate` artık `ScenarioPublicationGateManager`'ın beş gate'ini çalıştırır — `SchemaValidity`, `Derivability`, `AssertionCount`, `MaterialIntegrity`, `SourceDescriptionConsistency`; sonuç `Confirmed` ya da gate kodlarıyla `RuledOut`, kaynak kanıtı yoksa `Inconclusive`. **Yazarlık oturumu**: dört uç `api/test-module/authoring/sessions` (`POST` · `GET {id}` · `POST {id}/answer` · `POST {id}/step`), Swagger grubu `test-module-authoring`; durum ABP tipli distributed cache'te (`TestModuleAuthoringSessions`, TTL **30 dk**, tenant anahtarı otomatik ayrık), Application `IAuthoringSessionStore` domain portuna bağlanır — **yeni tablo, repository ve migration yoktur**; Arazzo belgesi her adımda Manager tarafından mekanik yeniden üretilir. **Kapsam**: payda artık gerçek ve tam sayfalanmış snapshot operasyon envanteridir; eksikte yanlış `0` yerine `Unknown` + kararlı gerekçe (`SnapshotNotFound`, `SnapshotIdentityMissing`, `SnapshotOperationInventoryUnavailable`), rapor başına en çok `1.000` yayınlanmış senaryo. **İş değişmezi** ucu `api/test-module/invariants/check`. Uç sayısı **53 → 58** (`OutwardSurfaceTests.ExpectedControllerActionCount`). Faz kapılarında son ölçüm build 0 hata / non-live **339/339**; `predev` merge'ü sonrası regresyon kapısı **yeniden çalıştırılmadı** |
| Test Module ortam ve kaynak yüzeyi | **KBP-112 ile hedefe koşturulabilir hâle geldi (2026-08-16)** — dört dilim `KBP-112` dalında (`06bc2d3`, `89d4d29`, `f267a07`, `7fa3aed`), `predev`'e **henüz merge edilmedi**. **Ayar**: host `AbpSettingManagement` Application/EFCore/HttpApi modüllerini compose eder; `abp.AbpSettings` sahibi Authenticator'dır (`20260809140749_Initial.cs:165`), Test Module `ConfigureSettingManagement()` çağırmaz ve **migration üretmez — sayı 8'de sabit**. Paralel ayar CRUD'u yazılmadı; ABP'nin `/api/setting-management/*` uçları kullanılır. **Ortam bağlaması**: `POST` · `PUT {key}` · `DELETE {key}`; değer tenant-scoped ayara yazılır (yeni tablo yok, ADR-0016 §G korundu), karar `RunEnvironmentBindingManager`'da, `ISettingManager` I/O'su AppService'te; `environmentKey` eşleşme kapısı negatif testte tutuyor. Yeni izin `Runs.ManageEnvironments`. **Koşum kimliği**: `api.secretRef` mevcut Vault portundan çözülür ve runner'a **yalnız tek ortam değişkeni** üzerinden geçer (`WorkflowRunnerConsts.Inputs.AuthHeaderName/AuthHeaderValue`); değer DTO/log/`RunnerRef`/HAR'a girmez. **HAR redaksiyonu** `HarInterpreter.Redact`'tedir. **Ağ sınırı**: `RunnerNetworkMode` / `RunnerExtraHosts` ayarları `--network` ve `--add-host`'a çevrilir; **ayar boşken argüman listesi birebir eskisidir** ve bunu test korur. **Kaynak tekliği**: host `.csproj`'daki `EmbeddedResource` satırları kaldırıldı, yerine `Content Include="Authoring\**"` geldi; `BusinessRulesResource` ve `AgentPolicyResource` artık DI ile porttan okur; `samples/profiles/acme-ticketing.yaml` `git mv` ile `host/…/Authoring/profiles/` altına taşındı ve varsayılan `Authoring/profiles` oldu. Yükleme/listeleme uçları `POST authoring/business-rules` · `POST authoring/profile-packs` · `GET authoring/profile-packs`; yeni izin `Bridge.ManageSources`. **Ajan döngüsü**: oturum `ptn_ground`'un arkasına bağlandı (`GroundRequestDto.SessionId` + tek `ProposedStep`), **yeni MCP tool açılmadı**. Uç sayısı **58 → 64** (`OutwardSurfaceTests.ExpectedControllerActionCount`). Ölçüm: Release build **0 hata / 3 uyarı**, non-live **358/358**, live **2/2**, `check-backend-diff` 72 dosyada tek gerekçelendirilmiş bulgu |
| Test Module yayın sözleşmesi | **KBP-116 ile mühür ve yetki sınırı kapandı (2026-08-16)** — beş commit `KBP-116` dalında (`e5ba0f0`, `9fa6d7e`, `2662769`, `263eee0`, `c7b5208`), `predev`'e **merge edilmedi**. **Yetki**: `PtnBridgeAppService`'in dokuz metodunun tamamı `CheckPolicyAsync` ile korunuyor; MCP artık AppService kapısından geçiyor, controller `[Authorize]` attribute'ları duruyor. **Fingerprint tel biçimi**: `TestScenarioConsts.FingerprintHashPattern = "^(sha256:)?[a-fA-F0-9]{64}$"`; mühür sınırı prefiksi kabul eder, `TestScenarioManager.StripFingerprintPrefix` soyar, veritabanına **düz 64-hex** yazılır. **`SourceHash`**: artık opsiyoneldir ve sunucu hesaplar — kanonikleştirme `ptn-source-canonical-v1` (UTF-8, BOM kırpılır, CRLF→LF, satır sonu boşlukları ve sondaki boş satırlar kırpılır); istemci gönderirse hesaplananla eşleşmelidir. **Kural mührü**: `IBusinessRuleSourcePort` okuması AppService'te, karşılaştırma `TestScenarioManager.ApplyRulesFingerprint`'te — `ApplyDbSchemaFingerprint` ile aynı desen; bayat değer `InvalidHash` alır. `ProfileFingerprint` **bilinçli olarak boş bırakılır** (mühürde profil anahtarı yok; başka bir belgenin hash'i konmaz — ADR-0020). Ölçüm: Release **0 hata / 3 uyarı**, non-live **373/373**, live **2/2** |
| Test Module DB yazarlık köprüsü | **KBP-118 ile kapandı (2026-08-16)** — `71a8183`, `predev`'de. Compiler `x-checknexus-db`'yi zaten derleyebiliyordu ama yazarlık oturumunun tipli DB adımı üretecek yolu yoktu; ajan `databaseAssertions: []` göndermek zorundaydı. Yeni uç `POST authoring/sessions/{id}/database-step`, DTO `AddDatabaseAuthoringStepDto` + `AuthoringExpectationDto`, kapalı matcher kümesi `PtnDatabaseMatcherCodes` (11 kod). Seçim **kapalı kümeden**: opak `TableReferenceId` ve kümedeki matcher kodu, ikisi de validator'da zorlanır (RULE-0007). `TableDescriptionDto` opak referans + kapalı matcher listesi taşır. **Landing sırasında düzeltildi:** validator yanlış projeye (`Application/Validators/`) konmuştu → 45 kardeşinin yanına `Application.Contracts/FluentValidation/Authoring/`'e taşındı; `AuthoringDatabaseStepModel`/`AuthoringExpectationModel` ikiz tipleri silindi ve Manager içindeki elle kopyalama Mapperly'ye devredildi (API adımının aksine burada grounding zenginleşmesi yok, yani ara tip kazanç sağlamıyordu); kapalı küme ve mükerrer adım için **6 test yazıldı** (KBP-118 hiç test getirmemişti). Uç sayısı **64 → 65**, non-live test **375 → 381** |
| Yazarlık ajanı (`ptn-test-agent`) | **Karar var, kod henüz depoda değil.** Runtime/model sınırı [[03-Decisions/ADR-0023-TypeScript-Yazarlik-Ajani-Runtime-Ve-Model-Siniri\|ADR-0023]] ile sabitlendi (Node 24 LTS, pnpm 11, ESM, provider-neutral port, ilk adapter OpenAI Responses). Kök depoda yalnız `ptn-test-agent/ADR-0001-...md` izlenir; paket kaynağı (`src/`, `package.json`, lockfile) **commit edilmedi**. Doğrulama durumu: typecheck ve build geçti, **lint çalıştırılamadı** (yerel Node 20, paket Node ≥24 istiyor), **test yok** (`tests/` klasörü hiç oluşturulmadı) |
| Foundation | Yedi public paket `1.0.0`; Authenticator `2.0.0` katmanları tarafından transitif taşınan ortak ABP tabanı |
| Authenticator | **Sekizli `2.0.0` ailesi nuget.org'da yayımlandı ve 8/8 registry'den doğrulandı (2026-08-13)**; ABP 10.6.0 / EF Core 10.0.10 tabanlıdır. Public `1.x` ailesi kullanılmaz |
| Notifications | Altılı `0.1.0-alpha.1` ailesi nuget.org'da yayımlı ve 6/6 doğrulandı; ayrı yetenek, checker paketleri içine gömülmez. **Açık:** `Pintern.SaaS.Notifications.Domain` ve `.HttpApi`, nuget.org'da **hiç yayımlanmamış** `SystemStandards.Abp.Authorization 1.0.0`'a bağımlıdır ve tiplerini gerçekten kullanır. Önce o paket (ve ProjectReference verdiği `SystemStandards.Authorization.Contracts`) yayımlanmalı, ancak sonra Notifications yeni sürüme repack edilebilir — blokaj 6 |
| MCP ve ajan | Yüzey ve sınırlar uygulandı (ADR-0008 + RULE-0005, KBP-105): MCP yalnız composition host'ta, dört kademeli izin ve koşumda model istemcisi yok. `ManagerReachabilityTests` ve `PackageBoundaryTests` bu sınırı korur. **İki ayrı sayı karıştırılmamalıdır:** `PtnToolCodes.Governed` **12** kod taşır (`ProtocolMax = 12`, `ToolCatalogTests` doludur der) ama `PtnMcpTools` bunlardan **10**'unu `[McpServerTool]` olarak register eder; `PatchSuggest` ve `PatchReview` `ReviewOnly` kümesindedir ve `tools/list`'te görünmez. Ajan istemcisi tool yüzeyini `tools/list ∩ ptn_profile.allowedToolCodes` olarak kurmalıdır |

### Açık backend blokajları — 2026-08-16 doğrulaması

**KBP-116 ile kapananlar:** MCP yetki asimetrisi, fingerprint tel biçimi ikiliği ve `SourceHash`
üretim sözleşmesinin yokluğu.

> [!NOTE] Satır durumları — 2026-08-16 akşamı
> **Kapandı:** 1–4 (KBP-117, `6d917fc`) ve 8 (predev merge, `6e9b657`). Bu satırlar aşağıda
> hâlâ eski metinleriyle duruyor; 8'in üstü çizildi.
> **Açık:** 5 (RULE-0008 DMN), 6 (Notifications alpha.2 — SystemStandards tarafı çözüldü),
> 7 (NuGet.Config parolası), 9 (`ProfileFingerprint`), 10 (alpha.2 üretilemiyor), 11 (cache).
> Güncel ölçüm: **65 uç**, non-live **381 test**, 8 migration.

| # | Konu | Kanıt | Sınıf |
|---|---|---|---|
| 1 | **İzin verilebilecek yer yok** | `TestModuleHttpApiHostModule` `DependsOn` listesinde (satır 49-72) hiçbir `AbpPermissionManagement*` modülü yok; buna karşılık `appsettings.json:38` `"Volo.Abp.PermissionManagement": "abp"` şema eşlemesini **zaten taşıyor**. KBP-116 MCP'yi `Bridge.*` iznine bağladığı için ajan bugün **hiçbir tool çağıramaz** | **Kurulum blocker** |
| 2 | **`abp.*` tablolarını kimse uygulamıyor** | `TestModuleEntityFrameworkCoreModule.MigrateAndSeedAsync` yalnız `TestModuleDbContext.Database.MigrateAsync()` çağırır (8 migration: `test_lookup`, `test_catalog`, `test_run`). `abp.AbpSettings` ve `abp.AbpPermissionGrants` Authenticator'ın DbContext'ine aittir ve bu host'ta uygulanmaz. Boş veritabanında ayar ve izin yüzeyi çalışmaz | **Kurulum blocker** |
| 3 | **Davranışsal yetki testi yok** | `BridgeAuthorizationTests` bir **kaynak-metin taramasıdır** (regex ile `await CheckPolicyAsync(` arar); `TestModuleTestBaseModule.cs:27` `AddAlwaysAllowAuthorization()` çağırdığı için tüm test süreci yetkiyi baypas eder. Ayrıca `Bridge_app_service_method_count_should_match_controller_authorize_count` adına rağmen controller'ı hiç okumaz, sabit `9` bekler | **Test borcu** |
| 4 | **`SpecFingerprint` mühre bağlanmamış** | `IsMaterialSealComplete` altı alanın hepsini dolu ister; `SpecFingerprint` bugün yalnız çağıranın gönderdiği değerdir (`TestRunAppService.cs:210`), yani drift koruması kâğıt üstündedir. **Kaynak checker'da zaten public:** `ISpecSnapshotAppService.GetAsync(snapshotId)` → `SpecSnapshotDetailDto.SpecContent.CanonicalHash` ("biçim gürültüsü elenmiş kanonik metnin SHA-256 kimliği"); ayrıca `RawHash` var. Test Module `CheckNexus.ApiContracts.Application.Contracts`'ı referanslıyor ve host `ApiContractCheckerModule`'ü compose ediyor. **Düzeltme (2026-08-16):** bu satır önce "kaynak yok, ürün kararı gerekir" diye kaydedilmişti — yanlıştı; yalnız Test Module'ün dar tüketici portu `IApiOracleAppService` incelenmiş, checker'ın kendi snapshot servisi atlanmıştı. Karar değil, kod dilimidir | **Publication blocker (kod)** |
| 5 | **RULE-0008 DMN kapsam şartı kodda yok** | `ScenarioPublicationGateManager` beş gate çalıştırır: `SchemaValidity`, `Derivability`, `AssertionCount`, `MaterialIntegrity`, `SourceDescriptionConsistency`. DMN satır kapsamı ölçülmez; wiki kuralı uygulanmıyor | **Ürün kararı** |
| 6 | **Notifications hâlâ yayımlanmamış bir bağımlılığa bağlı** — SystemStandards tarafı çözüldü | **SystemStandards tarafı kapandı (2026-08-16):** aile **lockstep `2.1.0`** olarak nuget.org'a yayımlandı — `Core`, `Validation`, `AspNetCore`, `Abp`, ve ilk kez `Abp.Authorization` + `Authorization.Contracts`. Lockstep zorunluydu: release engine `-p:Version`'ı çözüm genelinde ezdiği için ailenin bir kısmını yayınlamak ProjectReference bağımlılıklarını var olmayan sürümlere pinliyordu. Manifest `nuget-release.json`, iç bağımlılık sürümleri `requiredDependencies` ile kilitli. **Kalan blokaj Notifications'tadır:** yayımlanmış `Pintern.SaaS.Notifications.Domain 0.1.0-alpha.1` hâlâ `SystemStandards.Abp.Authorization` **1.0.0**'a bağlıdır ve o sürüm nuget.org'da yoktur → temiz klon/CI hâlâ NU1101 alır. Çözüm `0.1.0-alpha.2`'dir; engeli için blokaj 10 | **Live integration blocker** |
| 7 | **`NuGet.Config` düz metin kimlik bilgisi taşıyor** | Kök `NuGet.Config` içinde `CustomPackageFeed` için `ClearTextPassword` var ve depoda izleniyor | **Security** |
| 8 | ~~**`KBP-112` ve `KBP-116` `predev`'e merge edilmedi**~~ **Kapandı (2026-08-16).** `predev` artık `6e9b657`; KBP-112 · 116 · 117 · 118 tek merge ile girdi (`#KBP-118 chore: merged the completed backend branches into predev`) ve `origin/predev`'e push edildi | **Kapandı** |
| ~~9~~ ✅ | **Kapandı 2026-08-17 (`c7c7773`):** sunucu `validate` isteğinin adlandırdığı profil paketinin `ContentFingerprint`'ini mühre bağlar; istemcinin farklı değeri `ProfileFingerprintMismatch` alır. Böylece "başka belgenin hash'i konmaz" kararı korunur, kapı da elle değer istemez. Aşağısı tarihsel kayıttır. **`ProfileFingerprint` mühürde zorunlu ama kimse üretmiyor** | `ScenarioPublicationGateManager.cs:57` `MaterialIntegrity` için `ProfileFingerprint`'i **dolu ister**. Sunucuda onu türeten hiçbir yol yok: değer yalnız çağıranın `TestScenarioMaterialSealDto`'sundan `TestScenarioManager.cs:404` üzerinden geçer, `GroundingManager.cs:162` de isteği aynen yansıtır. Buna karşılık KBP-116 profil parmak izini **bilinçli olarak boş bırakmaya** karar verdi (satır 85). İki karar birbirini kesiyor: yazarlık tarafı boş bırakıyor, kapı dolu istiyor → **`MaterialIntegrity` elle değer verilmeden geçilemez**. Bu, KBP-117 Dilim 4'ün `SpecFingerprint` için kapattığı "kâğıt üstünde mühür" sorununun ikizidir | **Publication blocker (açık)** |
| 10 | **Notifications `0.1.0-alpha.2` commit'lenmiş koddan üretilemiyor** | Yayımlanmış `alpha.1`'in nuspec'i `repository commit="621b6da"` der, ama o commit'teki `Pintern.Notifications.Domain.csproj` yalnız `Volo.Abp.Ddd.Domain` referansı taşır; nuspec'teki `Piton.Emailing.Domain`, `Scriban`, `SystemStandards.Abp.Authorization` ve `Volo.Abp.TextTemplating.Core` bağımlılıklarının dördü de **commit edilmemiş `KBP-N06` çalışmasındadır**. `PackageId` (`Pintern.SaaS.Notifications.*`), `Authors`, lisans ve README metadata'sı da yalnız o kirli ağaçta yaşar. Yani alpha.1 kirli bir worktree'den yayımlanmıştır ve "alpha.1 + bağımlılık bump'ı" yeniden üretilemez. **Doğrulanan:** `621b6da` + `SystemStandards 2.1.0` build 0 hata / **53 test** yeşil, sürüm pini düzeltildi; hazır dal `KBP-N07` (`eec06e5`). **Gereken:** KBP-N06'nın yayınlanabilir hâle getirilip commit'lenmesi, sonra o ağaçtan alpha.2 | **Live integration blocker (sahip eylemi)** |
| 11 | **Yerel publish klasörleri gerçek NuGet sürümlerini gölgeliyor** | Bu makinenin global cache'inde `systemstandards.abp/2.1.0` ve `systemstandards.core/2.1.0` girdileri `…\SystemStandards-Main-Publish\publish-artifacts-consolidated` **yerel klasöründen** gelmişti ve nuget.org'daki aynı sürümlerden **farklı içerik** taşıyordu (Authorization tipleri Abp'ye, Contracts tipleri Core'a birleştirilmiş). NuGet birebir sürüm eşleşmesinde cache'i tercih ettiği için build yayımlanan paketi değil bayat kopyayı görüyor ve `CS0433` tip çakışması veriyor. İkisi `C:\Users\mertb\nrel-cache-backup\` altına taşındı, gerçek paketler indi. **Aynı desen `SystemStandards.Abp.Authorization 1.0.0`'da da vardı.** Sürüm seçerken registry preflight'ı tek başına yetmez; yerel cache'in `.nupkg.metadata` `source` alanı da kontrol edilmelidir | **Ortam tuzağı** |

### Bilinen tarayıcı false positive'i

`check-backend-diff.ps1`, `TestScenarioManager.cs` üzerinde **13 adet `[ENTITY]`** bulgusu verir.
İşaretlenen satırlar `EnsureCanDeleteAsync`, `EnsureEditableStateAsync`, `EnsureApprovalIsCurrent`,
`Normalize`, `NormalizeScenarioKey`, `NormalizeOptionalHash` — hepsi **zaten Manager'ın içindedir**.
Heuristik, dosyanın Manager olduğunu kontrol etmeden isim önekiyle eşleşiyor. Yeni bir değişikliğin
bulgu ekleyip eklemediğini bu **13** sayısıyla karşılaştır.

### Lookup şeması ile DBML arasındaki bilinçli sapma

`04-Architecture/Test-Platform-Schema.dbml` beş lookup tablosunu `code varchar(64)`,
`name varchar(128)`, `description varchar(512)` ve `is_active` ile tarif eder. Üretilen şema
**bilerek** farklıdır; kod DBML'e uydurulmamalıdır:

| Konu | DBML | Kurulan şema | Gerekçe |
|---|---|---|---|
| Kolon uzunlukları | 64 / 128 / 512 | **128 / 256 / 1024** | Foundation'ın `ConfigureLookup` eşlemesi ve `LookupManager` normalizasyonu bu sınırları sabit taşır. DBML uygulansaydı veritabanı 64'te keser, validator 128'e izin verirdi; fark çalışma anında `INSERT` hatası olurdu |
| `is_active` | Var | **Yok** | Foundation `LookupEntity<TKey>` `IPassivable` taşımaz; kullanıcı 2026-08-14'te Foundation tabanının olduğu gibi kullanılmasına karar verdi. ADR-0016 §E'nin `LookupEntity : Entity<Guid>` + `IPassivable` satırı bu modül için geçerli değildir |

Lookup vokabülerini elle değiştirmeden önce bu tablo okunur. DBML'i şemaya hizalamak ayrı bir
karardır ve alınmamıştır.

## Upstream doğrulama kaynakları

- API Contract Checker ayrıntılı motor geçmişi: `C:\Users\mertb\RiderProjects\ptn-api-contract-checker`
- Database Checker ayrıntılı motor/T12 geçmişi: `C:\Users\mertb\Documents\Codex\2026-07-06\bi\ptn-database-comparison-api`
- Authenticator: `C:\Users\mertb\RiderProjects\pintern-authenticator-latest-api`
- Foundation: `C:\Users\mertb\RiderProjects\nexum-abp-foundation`
- Notifications: `C:\Users\mertb\RiderProjects\pintern-notifications`

Bu kaynaklar kendi işlerinde değişebilir. Merkezi paket gerçeği için önce bu workspace’in kodu, üretilmiş `.nupkg` içeriği ve resmî NuGet kaydı kontrol edilir.
