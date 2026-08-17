---
id: AUDIT-0005
type: audit
status: draft
title: Backend teslim denetimi — UI ve ajan gelistiricisine devir oncesi kod incelemesi
updated: 2026-08-17
decision_refs:
  - ADR-0013
  - ADR-0016
  - ADR-0020
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0005
---

# Backend teslim denetimi

> [!IMPORTANT] Kapanış durumu — 2026-08-17
> Bu denetimin **blocker'ı ve risklerinin çoğu kapandı.** Aşağıdaki gövde denetim anındaki
> hâlini korur; her maddenin güncel durumu burada:
>
> | Madde | Durum | Kanıt |
> |---|---|---|
> | B-1 capability port'ları DI'da yok | ✅ kapandı — **on bir** port (`[ExposeServices]`); test iki adapter'ı daha yakaladı | `f23ee3c` · `CapabilityPortWiringTests` |
> | R-1 `ProfileFingerprint` üretilmiyor | ✅ kapandı — sunucu `ProfilePack.ContentFingerprint`'i mühürlüyor, uyuşmazlık reddediliyor | `c7c7773` |
> | R-2 davranışsal yetki kapsamı | ✅ genişletildi — `Scenarios.*` ve `Runs.*` gerçek modül grafiğinde | `f68d2c4` |
> | R-3 `EnsureSharedAbpSchema` doğrulamıyor | ⚪ hâlâ niyet beyanı; kurulum sırası runbook'ta | GUIDE-0007 |
> | R-4 `SQLitePCLRaw` NU1903 | ✅ kapandı — yamalı 2.1.13 | `3197d42` |
> | R-5 `NuGet.Config` düz metin kimlik | 🔴 açık — HEAD temiz ama geçmişte duruyor; **feed sahibi iptal etmeli** | ölçüldü: 4 eşleşme |
> | R-6 ajan yüzeyinde kimlik doğrulama | 🔴 açık — `ptn-test-agent` tarafı | — |
> | N-1 list input validator'ı | ⚪ yanlış pozitif — `TestScenarioListInput` boş paging DTO'su, doğrulanacak alanı yok | kaynak |
> | N-3 `new const string Equals` | ⚪ kasıtlı — `CS0108` susturması; değer wire kodu | kaynak |
> | N-5 izin asimetrisi | 🔴 açık — ürün kararı |  |
>
> Denetimden sonra bulunan ve kapatılan ek açık: **`POST /api/emailing/emails` yetkisizdi**
> (controller ve AppService'te hiç kontrol yok) — Emailing HTTP yüzeyi artık compose edilmiyor
> (`3fd78aa`).

> Kapsam: `ptn-test-module` (65 uç, 7 src + 2 host + 5 test projesi) ve `ptn-test-agent`.
> Yöntem: `backend-standards-router` → `dotnet-clean-code-standards` + `abp-coding-standards`
> + `backend-verify` zinciri; kaynak okuma, Release build ve tam test koşumu.
> Tarih: **2026-08-17**. Bu denetim **hiçbir üretim kodunu değiştirmedi**.

## 0. Ölçüm

| Kapı | Komut | Sonuç |
|---|---|---|
| Değişiklik tarayıcısı | `check-backend-diff.ps1` | **Temiz** — değişmiş C# dosyası yok |
| Derleme | `dotnet build Ptn.TestModule.slnx -c Release` | **0 hata / 3 uyarı** |
| Test | `dotnet test Ptn.TestModule.slnx -c Release --no-build` | **383/383 geçti** (Domain 256 · EFCore 29 · Application 98) |

Yeşil build ve yeşil test **mimari doğruluğu kanıtlamaz**. Aşağıdaki B-1 tam olarak bunun
örneğidir: derleyici görmez, mevcut testler dokunmaz, ilk HTTP isteğinde patlar.

---

## A. Blocker

### B-1 — Dokuz capability port'u üretim DI'ında kayıtlı değil

**Kanıt zinciri:**

1. ABP'nin varsayılan kuralı (`ExposeServicesAttribute.GetDefaultServices`, `dev` dalı):
   arayüz adının başındaki `I` atılır ve **`type.Name.EndsWith(interfaceName, OrdinalIgnoreCase)`**
   koşulu aranır. Koşul tutmazsa arayüz **expose edilmez**.
2. `src` ve `host` altında `AddTransient` / `AddScoped` / `AddSingleton` ile yapılmış
   **hiçbir manuel kayıt yoktur** (tarama sonucu: sıfır eşleşme).
3. Şu dokuz sınıf `ITransientDependency` taşır ama `[ExposeServices]` **taşımaz** ve ad kuralı
   hiçbirinde tutmaz:

| Sınıf | Arayüz | `EndsWith` tutuyor mu | Kayıtlı mı |
|---|---|---|---|
| `AgentPolicySourceService` | `IAgentPolicySourcePort` | hayır | ❌ |
| `AuthoringSessionCacheService` | `IAuthoringSessionStore` | hayır | ❌ |
| `BusinessRuleSourceService` | `IBusinessRuleSourcePort` | hayır | ❌ |
| `OracleDispatchService` | `IOracleDispatchPort` | hayır | ❌ |
| `ProcessBoundaryService` | `IProcessBoundaryPort` | hayır | ❌ |
| `ProfilePackSourceService` | `IProfilePackSourcePort` | hayır | ❌ |
| `ScenarioCompilationService` | `IScenarioCompilationPort` | hayır | ❌ |
| `TestDataSandboxService` | `ITestDataSandbox` | hayır | ❌ |
| `WorkflowRunnerService` | `IWorkflowRunnerPort` | hayır | ❌ |

4. **Deponun kendi çözümü zaten var:** aynı ad uyuşmazlığını taşıyan iki kardeş
   `[ExposeServices(typeof(IRunArtifactStore))]` ve `[ExposeServices(typeof(IHarArtifactStore))]`
   attribute'unu taşır ve doğru çalışır. Yani doğru kalıp deponun içinde kanıtlıdır.
5. **Yazar bunu zaten fark etmiş ama testte örtmüş:**
   `test/…/Composition/TestModuleIsolatedAuthTestModule.cs:22-25`, üç port'u elle kaydeder ve
   yorumu aynen şudur: *"KBP-117 Test DI kısıtı: **Prod tarafında `[ExposeServices]` eksiği olan**
   test bağımlılıklarını burada karşıla"*. Test yeşile döndü, üretim hatası kaldı.

**Etkilenen tüketiciler** (constructor injection ile bu port'ları isteyen tipler):

| Tüketici | İstediği port | Düşen yüzey |
|---|---|---|
| `AuthoringSessionAppService` | `IAuthoringSessionStore` | `authoring/sessions` — **5 uç** |
| `PtnBridgeAppService` | `IScenarioCompilationPort`, `IAuthoringSessionStore` | `bridge/*` — **9 uç** + MCP tool yüzeyinin tamamı |
| `TestScenarioAppService` | `IScenarioCompilationPort`, `IBusinessRuleSourcePort` | `scenarios/*` — **13 uç** |
| `AuthoringSourceAppService` | `IBusinessRuleSourcePort`, `IProfilePackSourcePort` | `authoring/business-rules` · `profile-packs` — **3 uç** |
| `TestEnvironmentAppService` | `ITestDataSandbox` | `environments/{key}/sandbox/reset` |
| `ExecuteTestRunJob` | `IWorkflowRunnerPort`, `IOracleDispatchPort`, `ITestDataSandbox` | **Koşum hattının tamamı** |
| `TestRunExecutionManager` | `IWorkflowRunnerPort` | koşum tetikleme |
| `RedoclyArazzoDocumentLinter` · `WorkflowRunnerService` | `IProcessBoundaryPort` | lint ve runner |
| `AgentPolicyResource` · `BusinessRulesResource` (host MCP) | `IAgentPolicySourcePort`, `IBusinessRuleSourcePort` | MCP `Resource` okuması |

Yani **65 ucun yaklaşık 30'u ve MCP yüzeyinin tamamı** ilk çağrıda DI çözümleme hatası verir.
Uygulama açılır (transient kayıtlar istek anında çözülür), hata **çalışma zamanında 500**
olarak görünür.

**Neden mevcut testler yakalamadı:** yazarlık testleri (`AuthoringSessionCacheServiceTests`)
servisi `new AuthoringSessionCacheService(...)` ile doğrudan kurar, container'dan **çözmez**.
Container'dan çözen tek yer, üç port'u elle kaydeden izole test modülüdür.

**Düzeltme:** dokuz sınıfa `[ExposeServices(typeof(I…))]` eklenir ve test modülündeki üç manuel
kayıt satırı silinir. Deponun `RunArtifactService` kardeşi birebir örnektir.

**Regresyon kapısı önerisi:** `OutwardSurfaceTests` yalnız controller/action sayar. Her port
arayüzünün gerçekten çözülebildiğini doğrulayan bir kompozisyon testi eklenmelidir; aksi hâlde
aynı hata sessizce geri gelir.

> **Not:** Host canlı ayağa kaldırılıp 500 gözlemlenmedi (PostgreSQL + Authenticator
> migration'ları gerekiyor). Bulgu **statik kanıta** dayanıyor: ABP kaynak kuralı + attribute
> yokluğu + manuel kayıt yokluğu + yazarın kendi yorumu.

---

## B. Risk

### R-1 — `ProfileFingerprint` üretilmiyor, ama yayın kapısı zorunlu tutuyor

`ScenarioPublicationGateManager.cs:57` `MaterialIntegrity` için `ProfileFingerprint`'i **dolu**
ister. Değeri türeten hiçbir sunucu yolu yoktur: yalnız çağıranın DTO'sundan
`TestScenarioManager.cs:404` üzerinden geçer, `GroundingManager.cs:162` isteği aynen yansıtır.
`SpecFingerprint` için KBP-117'nin kapattığı boşluğun ikizidir ve **açıktır**.

Sonuç: senaryo yayınlamak, hiçbir bileşenin üretmediği bir değerin elle girilmesini gerektirir.
UI ve ajan için bu, yayın hattının çalışmaması demektir.

### R-2 — Davranışsal yetki testi yalnız tek metodu kapsıyor

`BehavioralAuthorizationTests` doğru kurulmuş: `TestModuleIsolatedAuthTestModule`
`AlwaysAllowAuthorizationService`'i kaldırıyor, `FakePermissionChecker` claim tabanlı gerçek
karar veriyor. Ancak kapsam **`IPtnBridgeAppService.GroundAsync` tek metodu**dur.
`Scenarios.*` (13 uç), `Runs.*` (15 uç) ve `Bridge.*`'ın kalan sekiz metodu davranışsal olarak
denetlenmiyor.

`BridgeAuthorizationTests.Every_bridge_app_service_method_should_start_with_check_policy` bir
**regex kaynak taramasıdır**; gövde deseni ilk `}` karakterinde durur (`[^}]+`), yani içinde
süslü parantez bulunan bir metot gövdesi yanlış ayrıştırılır. Regresyon değeri sınırlıdır.

### R-3 — `EnsureSharedAbpSchema` bayrağı şemayı doğrulamıyor

`TestModuleHttpApiHostModule.cs:256-259` bayrak `false` ise host'u `InvalidOperationException`
ile durdurur — bu **doğru bir fail-fast**tır. Ancak bayrak yalnız bir **niyet beyanıdır**:
`true` verildiğinde Authenticator migration'larının gerçekten uygulandığı denetlenmez.
Yanlış kurulumda hata, açılışta değil ilk ayar/izin isteğinde 500 olarak görünür.

### R-4 — `SQLitePCLRaw.lib.e_sqlite3 2.1.11` yüksek önem dereceli güvenlik açığı

Build çıktısı `NU1903` veriyor (GHSA-2m69-gcr7-jv3q). Yalnız `EntityFrameworkCore.Tests`
bağımlılığıdır, üretime çıkmaz; yine de bakım borcudur ve CI'da uyarı-hata kapısı varsa
pipeline'ı kırar.

### R-5 — `NuGet.Config` düz metin kimlik bilgisi

Kök `NuGet.Config` `CustomPackageFeed` için `ClearTextPassword` taşır ve izlenir. Depo
genişleyen izleme kapsamıyla birlikte (bkz. [[90-Inbox/AUDIT-0004-Ui-Oncesi-Wiki-Gerceklik-Denetimi|AUDIT-0004]] §A)
maruziyet arttı.

### R-6 — Ajan yüzeyinde kimlik doğrulama yok

`ptn-test-agent/src/http/create-server.ts` beş ucun hiçbirinde authorization okumaz;
`config.ts` tek bir `PTN_MCP_BEARER_TOKEN` kullanır. Tenant izolasyonu ajan sınırında düşer.
Ayrıntı [[04-Architecture/UI-Agent-Experience|ARCH-0005]] §8.

---

## C. Nit

| # | Bulgu | Yer |
|---|---|---|
| N-1 | `TestScenarioListInput` tek başına **FluentValidation validator'ı olmayan** list input'tur; üç kardeşi (`TestRunListInput`, `TestFindingListInput`, `ScenarioHealthListInput`) validator taşır | `Application.Contracts/Dtos/Catalog/` |
| N-2 | `TestModuleIsolatedAuthTestModule.cs` **üç top-level tip** taşır (`FakeHostEnvironment`, `FakePermissionChecker` aynı dosyada) ve tip adlarını `using` yerine tam nitelikli yazar — ev kuralı "tek tip, tek dosya" | test projesi |
| N-3 | `PtnDatabaseMatcherCodes` içinde `public new const string Equals` — `object.Equals` statik metodunu gizler; derlenir ama sınıf üzerinden `Equals(a,b)` çağrılamaz hâle gelir | `Domain.Shared/Constants/Bridge/Vocabulary/` |
| N-4 | `ScenarioCoverageController` rotasını `ScenarioCoverageConsts.Root`'tan alır; diğer 16 controller `*Routes` sınıfı kullanır | `HttpApi/Controllers/Catalog/` |
| N-5 | `AuthoringSessionController.Create` `Scenarios.Create`, ama `Get`/`Answer`/`Step` `Scenarios.Update` ister; yalnız `Create` izni olan kullanıcı başlattığı oturumu **okuyamaz** | `HttpApi/Controllers/Authoring/` |
| N-6 | `RunOutcomeResolverTests.cs:64` `CS8604` olası null uyarısı | Domain.Tests |

---

## D. Doğru bulunanlar — teslim edilebilir kısım

Denetimin bulmadığı şeyler de kayıttır:

| Konu | Durum |
|---|---|
| `Controller -> AppService -> Manager -> Repository` zinciri | Korunmuş; 17 controller'ın hepsi tek AppService çağrısına delege ediyor |
| Entity veri kabuğu profili | `internal set`, EF ctor, atama-only ctor korunmuş |
| Validator kapsamı | **15 public input DTO'nun 15'inde** FluentValidation validator'ı var (N-1 list input hariç) |
| Sabit metin sahipliği | Route, permission, hata kodu, lookup kodu, matcher kümesi hepsi `Domain.Shared`'da |
| CORS | `App:CorsOrigins` ayarından okunuyor; wildcard origin **yok**, `AllowCredentials` açık origin listesiyle birlikte — doğru kombinasyon |
| Bearer doğrulama | `AddAbpJwtBearer` ile Authority/Audience config'ten; host resource server olarak doğru kurulmuş (ADR-0013) |
| İzin yüzeyi | `AbpPermissionManagement` Application/EFCore/HttpApi compose edilmiş — CURRENT-0001 blokaj 1 **kapanmış** |
| `SpecFingerprint` | Sunucu `snapshot.SpecContent.CanonicalHash`'ten hesaplıyor — blokaj 4 **kapanmış** |
| Sır sınırı | Ortam DTO'su sır taşımıyor; `api.secretRef` Vault'tan çözülüp runner'a tek env değişkeniyle geçiyor; HAR redaksiyonu var |
| Hata sözleşmesi | Tutarlı: 2xx `Result<T>`, 2xx dışı ABP `RemoteServiceErrorResponse`; `Result.Fail` karışımı **yok** |
| Migration sahipliği | Yalnız `test_lookup` / `test_catalog` / `test_run`; `abp.*` için migration üretilmemiş (RULE-0002) |

---

## E. Teslimden önce yapılacaklar sırası

| Sıra | İş | Sınıf | Kime |
|---|---|---|---|
| 1 | Dokuz port'a `[ExposeServices]`; test modülündeki üç manuel kaydı sil; port çözümleme kompozisyon testi ekle | **Blocker** | Backend |
| 2 | `ProfileFingerprint` üretim yolu ya da kapıdan çıkarma kararı | **Blocker** (yayın hattı) | Ürün + backend |
| 3 | Ajan yüzeyine kimlik doğrulama (ajanda veya ters vekilde) | **Blocker** (ajan ekranı) | Backend/ajan |
| 4 | Davranışsal yetki testini `Scenarios.*` ve `Runs.*`'a genişlet | Risk | Backend |
| 5 | Canlı uçtan uca koşum kanıtı (KBP-115) | Risk | Backend |
| 6 | `NuGet.Config` sırrı, `SQLitePCLRaw` yükseltmesi | Risk | Bakım |
| 7 | N-1..N-6 | Nit | Backend |

**1, 2 ve 3 kapanmadan** UI ve ajan geliştiricisine devir, çalışmayan bir yüzeyin devri olur.
1 numaralı iş dokuz dosyada birer satırdır ve deponun kendi kardeş dosyasında kanıtlı bir
kalıbı vardır.
