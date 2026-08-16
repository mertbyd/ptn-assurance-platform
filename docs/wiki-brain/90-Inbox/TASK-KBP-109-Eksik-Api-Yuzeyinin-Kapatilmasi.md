# AJAN GÖREVİ — KBP-109 · Eksik API yüzeyinin kapatılması

> **KAPANDI — 2026-08-15.** `predev` HEAD `a3bc92c`. Uç sayısı **41 → 48**, migration üretilmedi.
> Commitler: `8759472` (blob provider), `6ee3de4` (yüzey), `a3bc92c` (merge).
> Build 0 hata / 3 uyarı (yalnız NU1903); non-live **257/257**; live **2/2**.
> Teslim edilen kod derlenmiyordu; kapanışta düzeltilenler: `Result<IReadOnlyList<T>>` dönüşümü,
> ifade ağacında `is`/`?.` kullanan Shouldly assertion'ı, `sealed` AppService'ler (ABP proxy'leyemez).
> Ayrıca KBP-107'den beri gizli duran iki kompozisyon kusuru kapatıldı: `IRunArtifactStore` /
> `IHarArtifactStore` DI'da kayıtlı değildi (`[ExposeServices]`), ve hiçbir BLOB provider
> kayıtlı değildi (host + test tabanına FileSystem provider). Canlı uçtan uca doğrulama
> (gerçek Running koşusunu iptal, gerçek indirme) **yapılmadı** — PostgreSQL + runner ister.

Tek görev, **sekiz derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

Bugün modülde **41 HTTP ucu** var ve iş mantığının çoğu yerinde. Bu görev yeni yetenek
yazmaz; **dışarıdan ulaşılamayan** sekiz davranışı UI'ın ve ajanın tüketebileceği hâle getirir.

Gerekçe `house-profile.md` → *Wire the chain to the outside*: *"A manager is not finished while
it is unreachable."* Aynı gerekçe KBP-101'i doğurmuştu; KBP-104/105/107/108 yeni davranış
getirdi ve bir kısmı yine dışarı bağlanmadan kaldı.

> **Revizyon 2 — 2026-08-15.** §2 kaynak taramasıyla yeniden doğrulandı. Üç değişiklik:
> (a) Dilim 6'nın *"önce doğrula"* maddesi **kapandı** — depoda byte servis eden hiçbir uç yok;
> (b) **iki yeni boşluk** ölçüldü (liste filtresi ve liste başlık projeksiyonu) — UI'ın koşum
> ekranı bugün kurulamıyor; (c) Dilim 1'in kapısı 18 metotta **kırmızı açılıyor**, kararı §2.2'de
> sabitlendi. Revizyon 1'in rota adları yanlıştı (`/test-runs`, `/test-scenarios`); gerçek kökler
> `api/test-module/...`, §3'te düzeltildi.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform   (tek klasör, branch predev)
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-109   (predev üzerinden)
Motor   : PostgreSQL
Commit  : #KBP-109 <type>: <past-tense English description>
```

| Ön koşul | Durum |
|---|---|
| `predev` HEAD `fb2b90c`; KBP-104/105/106/107/108 merge edilmiş | ✅ doğrulandı 2026-08-15 |
| Build 0 hata; non-live 255/255; live 2/2 | ✅ |
| `ServiceShapeTests` · `ManagerReachabilityTests` · `ServiceContractTests` | ✅ **bu görev onları bozmaz — tam tersine besler** |
| 41 mevcut uç | ✅ sayıldı: Runs 11 · Scenarios 11 · Bridge 9 · Lookups 5×2 |
| Tek worktree; `git worktree list` → yalnız ana klasör | ✅ |
| Çalışma ağacında yalnız `TestRunAppService.cs` üzerinde **biçimsel** (whitespace) kullanıcı değişikliği var | ⚠️ **dokunma, commit'e alma** |

**Dosya bütçesi ≈60.** Sekiz dilim, dilim başına bir commit. **Migration üretilmez.**

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| AppService arayüzü | `house-profile.md` → *Contracts live in Application.Contracts* | `Application.Contracts/Services/Runs/ITestRunAppService.cs` |
| AppService | `house-profile.md` → *An AppService has no private business helpers* | `Application/Services/Runs/TestRunAppService.cs` |
| Controller | `house-profile.md` → *Architectural spine* | `HttpApi/Controllers/Runs/TestRunController.cs` |
| Filtreli sayfalı sorgu | `data-access.md` | **`checkers/api-contract/.../Dtos/Runs/GetContractCheckRunsInput.cs`** + `GetContractCheckFindingsInput.cs` |
| Liste başlık DTO'su | `contracts-mapping.md` | **`checkers/api-contract/.../Dtos/Runs/ContractCheckRunHeaderDto.cs`** |
| DTO + validator | `contracts-mapping.md` | `Dtos/Runs/*` + `FluentValidation/Runs/*` |
| Mapperly | `house-profile.md` → *Mapper files contain declarations only* | `Application/Mappers/Runs/TestRunMapper.cs` |
| Rota / izin sabiti | `house-profile.md` → *Stable string ownership* | `Domain.Shared/Constants/Runs/TestRunRoutes.cs` · `Permissions/TestModulePermissions.Runs.cs` |

**Kanonik kararlar:** ADR-0016 (kayıt modeli), ADR-0015 §F (checker iletişimi), ADR-0007
(checker oracle yüzeyi), RULE-0005 (ajan hakem değildir), RULE-0007 (ajan tahmin etmez).

**Referans notu:** `ptn-payment-management-api` **kalıp** referansıdır (modül şekli, host
kompozisyonu, `common.props`), **uç envanteri referansı değildir** — doğrulandı: `src/` altında
yalnız taban `PaymentManagementController` var, tek bir `I*AppService` bile yok; orası henüz
dikey dilim yazılmamış bir iskelet. Uç şekli için **bu depodaki checker kardeşlerini** kullan.

**UI referans notu:** `ptn-api-contract-checker-admin-ui` ilkel mantığın referansıdır. Ölçüldü:
dashboard'u **ayrı bir istatistik ucundan beslenmiyor**, liste uçlarını çekip istemcide
topluyor (`features/dashboard/use-dashboard-data.ts`). Bu yüzden bu görev **istatistik/özet ucu
açmaz**; bunun yerine liste uçlarını UI'ın ekranı kuracak kadar zenginleştirir (Dilim 2).
Aynı UI lookup'ları `create/update/passivate` ile yönetiyor; **Test Module lookup'ları
seed'lidir ve yazma ucu açılmaz** — bu bilinçli farktır, boşluk değildir.

---

## 2. Ölçülen boşluk — 2026-08-15 kaynak taraması

### 2.1 Dışarıdan ulaşılamayan sekiz davranış

| # | Ne | Bugünkü kanıt | Kim ister |
|---|---|---|---|
| **1** | **Koşum listesi filtresiz** | `TestRunListInput : PagedResultRequestDto` — **gövdesi boş**. Durum, ortam, senaryo, tetikleyici, tarih aralığı, dry-run filtresi **yok**; sıralama yok. Kardeşte `GetContractCheckRunsInput` filtre taşıyor. UI'ın koşum ekranı bugün "son N kaydı" listelemekten fazlasını yapamaz | UI'ın **ana ekranı** |
| **2** | **Liste başlık projeksiyonu** | `TestRunDto` yalnız `RunStatusId` / `TriggerKindId` **GUID**'i taşıyor; hüküm, süre, bulgu sayısı yok. Hüküm yalnız `TestReportDetailDto.OutcomeCode` içinde — yani **koşum başına ayrı istek**. Kardeş `ContractCheckRunHeaderDto` hem `StatusCode` hem sayaç taşıyor | UI, CI |
| **3** | **Bulgu sorgusu** | `TestResultFinding` yalnız `GetReportAsync(runId)` içinde **nested** geliyor. Koşumlar arası sayfalı/filtreli bulgu listesi **yok**. İki checker'da `FindingQueryInput` + sayfalı sonuç var; consumer'da yok | UI'ın **ikinci ekranı** |
| **4** | **Koşum iptali** | `TestRunStatusCodes.Cancelled` ve `TestModuleRunErrorCodes.RunCancelled` var; `RunOutcomeResolver` `OperationCanceledException`'ı `Cancelled`'a çeviriyor. **`ITestRunAppService`'te iptal yok, rota yok.** TM-09'un *"kooperatif iptal"*i ulaşılamaz | UI, ajan, CI |
| **5** | **Derleme/lint önizleme** | `ScenarioCompilationService` yalnız Domain portu `IScenarioCompilationPort`'u uyguluyor. **Application.Contracts arayüzü yok, rota yok.** UI bir taslağı yayımlamadan lint edip hatayı gösteremiyor. `evaluate-publication` var ama **kalıcılaşmış** senaryo üstünde çalışır | UI yazarlık ekranı, ajan |
| **6** | **Ortam bağlamaları** | `RunEnvironmentBindingManager` ABP `Setting`'ten çözüyor; **listeyi veren uç yok**. Koşum tetikleme diyaloğu ortam dropdown'ını dolduramıyor | UI |
| **7** | **Sandbox** | `TestDataSandboxService` yalnız `ITestDataSandbox` Domain portunu uyguluyor. Application.Contracts arayüzü yok, rota yok (KBP-108) | UI, CI |
| **8** | **Artefakt indirme** | ✅ **doğrulandı:** `GetArtifactLinksAsync` yalnız **blob adı** döndürüyor (`RunArtifactLinksDto` = üç `string?`). Depo genelinde `File(`, `FileResult`, `FileStreamResult` **sıfır eşleşme** — byte/gövde servis eden uç **yok**. `IRunArtifactStore.ReadAsync` ve `HarArtifactService` içeriği okuyor ama dışarı vermiyor. UI'ın "İndir" butonu ve HAR kanıt görünümü kurulamıyor | UI, CI |

### 2.2 Sabitlenen kararlar

- **Bridge alt-AppService'leri içeride kalır.** `IApiOracleAppService` (5), `IDatabaseOracleAppService`
  (6), `ISchemaKnowledgeAppService` (3), `IFailureDiagnosisAppService` (2),
  `IWriteSetCapabilityAppService` (2) = **18 public metot** bugün controller'sız. Bunlar **açılmaz**:
  aynı composition host'ta checker'lar bu yetenekleri zaten kendi uçlarından veriyor
  (`ResponseConformanceController` → AssertResponse/BuildRequestExample/SuggestOperationBindings/
  SuggestOperationLinks/ValidateScenarioAssertions; DB Checker → `AssertionController`,
  `ProjectionController`, `SchemaDiscoveryController`, `WriteSetCapabilityController`,
  `DiagnosisController`). Test Module tarafı **tüketici portudur** (ADR-0007, ADR-0015 §F);
  ikinci kez yayımlamak tek host'ta çift yüzey üretir. Dilim 1'in kapısı bu 18 metodu
  **gerekçeli allowlist** ile tanır; allowlist **isim listesi değil**, `Services/Bridge`
  namespace'i + `IPtnBridgeAppService` istisnasıdır.
- **İptal kooperatiftir.** Uç `Running` koşuya iptal **talebi** yazar; job kooperatif olarak
  görür ve `RunOutcomeResolver` mevcut yolundan `Cancelled`'a düşürür. Process `Kill`
  edilmez, durum dışarıdan `Cancelled`'a **zorlanmaz**. Zaten terminal olan koşu idempotent
  biçimde reddedilir.
- **Liste ucu ağır kolon projekte etmez.** Ne koşum ne bulgu listesi `diagnosis_report`'u,
  HAR gövdesini veya bulgu gövdesini taşır — TM-22'nin kuralı. Başlık projeksiyonu yalnız
  **kod + sayaç + zaman**'dır: `RunStatusCode`, `OutcomeCode`, `TriggerKindCode`, `DurationMs`,
  `FindingCount`, `Attempt`, `StartedAt`, `CompletedAt`, `IsDryRun`. Kalıp: `ContractCheckRunHeaderDto`.
- **Ayrı istatistik/dashboard ucu açılmaz.** Referans UI özetini liste uçlarından hesaplıyor
  (§1). Başlık projeksiyonu geldiğinde aynı hesap burada da kurulabilir.
- **Ayrı `status` ucu açılmaz.** Kardeşte `GET /checks/{id}/status` var çünkü orada detay ucu
  findings gövdesini taşıyor. Burada `TestRunDto` zaten ağır kolon taşımıyor; `GET runs/{id}`
  polling için yeterlidir. Uç sayısını gereksiz şişirme.
- **Filtre kümesi bounded'dır.** Bulgu filtresi: koşu, senaryo, severity, outcome, checker kodu,
  `rule_ref`, fingerprint, tarih aralığı. Fingerprint filtresi **sınırlı** (checker'ların 100
  SHA-256 sınırı kalıbı). Serbest metin arama **yok** — kod alanları üzerinden filtrelenir.
- **Önizleme kalıcılaştırmaz.** Derleme/lint önizlemesi **salt hesap**tır; `test_scenarios`'a
  satır yazmaz, `compiled_hash` mühürlemez. Yayın kapısı ayrı ve değişmeden kalır.
- **Ortam listesi salt-okunurdur.** Ayar yazma ucu **açılmaz**; ABP'nin kendi setting yüzeyi
  zaten var. Sır değeri (`secretRef` çözümü) **döndürülmez**.
- **Sandbox ucu açıkça yetkilendirilir.** Kendi izin kodunu alır; `Runs.Trigger` ile
  paylaşılmaz — ayrı ve tehlikeli bir yetenektir (KBP-108 §2.1).
- **İndirme metin gövdesi döndürür, stream değil.** Dört artefaktın dördü de UTF-8 metindir
  (CTRF JSON, JUnit XML, SARIF JSON, HAR JSON) ve `IRunArtifactStore.ReadAsync` zaten `string`
  veriyor. Depoda `File()` **precedent'i yok**; tek `Result<T>` sözleşmesini bozmamak için uç
  `Result<RunArtifactContentDto>` döndürür (`Format`, `BlobName`, `ContentType`, `Content`),
  indirmeyi istemci Blob'la yapar. Boyut kapısı zaten var: `RunArtifactConsts.MaxArtifactBytes`
  = 32 MiB. **İkili (binary) artefakt veya 32 MiB üstü gelirse dur ve raporla** — o an stream
  kararı ayrı bir işe düşer.
- **Migration üretilmez.** Sekiz boşluğun hiçbiri şema değiştirmez; hepsi mevcut kolonların
  projeksiyonu, filtresi veya okunmasıdır. Gerektiğini düşünüyorsan **dur ve raporla**.

---

## 3. Dilimler

Rota kökleri **mevcut sabitlerdir**: `TestRunRoutes.Root` = `api/test-module/runs`,
`TestScenarioRoutes.Root` = `api/test-module/scenarios`, `TestLookupRoutes.*` =
`api/test-module/lookups/...`. Yeni kökler aynı ailede kalır.

### Dilim 1 — Envanter ve boşluk kapısı (≈4 dosya)

Önce **ölç**, sonra yaz. `OutwardSurfaceTests`: `Application.Contracts/Services/**` altındaki
her public metodun bir controller action'ına karşılık geldiğini reflection ile tarar.
§2.2'nin Bridge allowlist'i gerekçesiyle birlikte teste yazılır; allowlist genişlerse test
**kırmızıya döner**. Bugünkü 41 uç taban çizgisidir; her dilim bu sayıyı **bilinçli** artırır.

Bu test kalıcı kapıdır: bir daha sözleşmesiz AppService metodu **derlenmeden** yakalanır.
`ManagerReachabilityTests` ve `ServiceContractTests`'in eksik üçüncü ayağıdır.

**Commit:** `#KBP-109 test: created the outward surface coverage gate`

---

### Dilim 2 — Koşum sorgu yüzeyi (≈9 dosya) · **UI'ın ana ekranı**

`TestRunListInput`'a filtre + sıralama; `TestRunHeaderDto` (kod + sayaç projeksiyonu);
repository sorgusu; Mapperly. **Yeni rota yok** — `GET api/test-module/runs` zenginleşir.
Kalıp birebir `GetContractCheckRunsInput` + `ContractCheckRunHeaderDto`.

Sıralama ve filtre alan adları `Domain.Shared` sabitlerinden gelir; inline string yasak.
Sayfalama sınırı ve varsayılanı mevcut Foundation akışıyla aynı kalır.

**Commit:** `#KBP-109 feat: created the filtered run query surface`

---

### Dilim 3 — Bulgu sorgu yüzeyi (≈10 dosya) · **UI'ın ikinci ekranı**

`ITestFindingAppService` + `TestFindingController` (`GET api/test-module/findings`) +
`TestFindingListInput` + repository sorgusu + Mapperly. Sayfalı, filtreli, ağır kolonsuz.
Koşum kimliği bir **filtredir**, ayrı rota değil.

**Commit:** `#KBP-109 feat: created the paged finding query surface`

---

### Dilim 4 — Koşum iptali (≈8 dosya)

`RunCancellationManager` (Domain) + `ITestRunAppService.CancelAsync` +
`POST api/test-module/runs/{id}/cancel` + `TestModulePermissions.Runs.Cancel` + validator.
Kooperatif; §2.2'nin kuralı birebir.

**Commit:** `#KBP-109 feat: created the cooperative run cancellation endpoint`

---

### Dilim 5 — Derleme/lint önizleme (≈8 dosya)

`IScenarioCompilationAppService` (Application.Contracts) +
`POST api/test-module/scenarios/compile-preview`. Salt hesap; kalıcılaştırmaz.
`ArazzoLintManager` ve `ArazzoCompilerManager` çıktısı UI'a okunur hata listesi olarak döner.

**Commit:** `#KBP-109 feat: created the scenario compile and lint preview endpoint`

---

### Dilim 6 — Ortam bağlamaları ve sandbox (≈9 dosya)

`GET api/test-module/environments` — salt-okunur, sırsız.
`ITestDataSandboxAppService` + `POST api/test-module/environments/{key}/sandbox/reset` +
kendi izin kodu (`TestModulePermissions.Runs.SandboxReset`, `Trigger`'dan ayrı).

**Commit:** `#KBP-109 feat: created the environment binding and sandbox reset surface`

---

### Dilim 7 — Artefakt ve HAR indirme (≈7 dosya)

Doğrulama **kapandı**: byte servis eden uç yok (§2.1 #8). İki uç açılır:

- `GET api/test-module/runs/results/{id}/artifacts/{format}` — `format` ∈ `RunArtifactFormatCodes`;
- `GET api/test-module/runs/{id}/har` — `HarBlobName` boşsa 404 semantiği ev standardıyla.

`Result<RunArtifactContentDto>`; boyut kapısı `RunArtifactConsts.MaxArtifactBytes` (§2.2).
`HarArtifactService` ve `RunArtifactService` **yeniden yazılmaz**, okunur.

**Commit:** `#KBP-109 feat: created the run artifact and har download endpoints`

---

### Dilim 8 — İstemci proxy'si ve Swagger bütünlüğü (≈5 dosya)

UI generated client kullanıyor (`ptn-api-contract-checker-admin-ui/src/api/generated/schema.d.ts`
kalıbı). `Ptn.TestModule.HttpApi.Client` proxy'lerinin ve **Swagger gruplarının** yeni uçları
kapsadığını doğrula; yeni controller'lar kendi `SwaggerGroupName` sabitini alsın.

Kapı: Swagger dokümanında **her** controller action'ı görünüyor.

**Commit:** `#KBP-109 test: created the swagger and client proxy completeness gate`

---

## 4. Kesme bölgesi

Bütçe aşılırsa sırayla **Dilim 5** ve **Dilim 6'nın sandbox yarısı** devredilir (ortam listesi
kalır — tetikleme diyaloğu onsuz kurulamaz).
**Kesilmeyecekler: Dilim 1, 2, 3, 4, 7.** Dilim 1 kapısı olmadan bu görev tekrar eder;
2-3-7 olmadan UI ekranı kurulamaz.

---

## 5. Yasaklar

1. Yeni iş mantığı yazma — bu görev **ulaşılabilirlik** görevidir; karar Manager'da zaten var.
2. İptali zorlayıcı yapma: process `Kill`, durumu dışarıdan `Cancelled`'a zorlama (§2.2).
3. Liste uçlarına `diagnosis_report`, HAR gövdesi veya bulgu gövdesi projekte etme (§2.2).
4. Fingerprint filtresini sınırsız bırakma; liste uçlarına serbest metin arama ekleme (§2.2).
5. Önizleme ucunda `test_scenarios`'a yazma veya `compiled_hash` mühürleme (§2.2).
6. Ortam ucundan sır değeri döndürme; ayar **yazma** ucu açma (§2.2).
7. Sandbox iznini `Runs.Trigger` ile paylaştırma (§2.2).
8. Bridge alt-AppService'lerini controller'a bağlama veya allowlist'i isim isim şişirme (§2.2).
9. Ayrı istatistik/dashboard ya da `status` ucu açma (§2.2).
10. Application servisine private iş metodu veya guard koyma — `ServiceShapeTests` **kapıdır**.
11. `Domain/Managers/**` içine `Process`/`File`/`Directory` yazma (KBP-102 kuralı).
12. Yeni proje, yeni katman, `Infrastructure/`, `Helpers/`, `Utils/` açma.
13. **Migration üretme** — gerekiyorsa dur ve raporla (§2.2).
14. Rota, izin, hata kodu, filtre/sıralama alan adı için inline string yazma — `Domain.Shared` sahibidir.
15. Koşum hattına model çağrısı ekleme (RULE-0005); KBP-105'in kapısını bozma.
16. Kırılan testi silme, `Skip` etme, assertion zayıflatma.
17. Ara dilimlerde build/test atlama.
18. Çalışma ağacındaki kullanıcıya ait `TestRunAppService.cs` biçim değişikliğini commit'e alma (§0).

---

## 6. Kabul kriterleri

- `OutwardSurfaceTests` yeşil: allowlist dışında **hiçbir** public AppService metodu controller'sız değil.
- Koşum listesi durum, ortam, senaryo, tetikleyici ve tarih aralığına göre filtreleniyor; sıralanabiliyor.
- Koşum listesi tek istekte hüküm kodunu, süreyi ve bulgu sayısını taşıyor; **ağır kolon yok**;
  UI listeyi kurmak için koşum başına ikinci istek atmıyor.
- Bulgular koşumlar arası sayfalı ve filtreli okunuyor; ağır kolon projekte edilmiyor.
- `Running` koşu dışarıdan iptal ediliyor; terminal koşumda iptal idempotent reddediliyor.
- Bir Arazzo taslağı **yayımlanmadan** lint ediliyor ve hata listesi dönüyor; `test_scenarios`
  satır sayısı değişmiyor.
- Ortam listesi dönüyor; **hiçbir sır değeri** taşımıyor.
- Sandbox reset kendi izin koduyla korunuyor.
- Üç ihracat formatı ve HAR **gövdesiyle** indiriliyor; 32 MiB kapısı test ediliyor.
- Swagger dokümanında her action görünüyor; client proxy'leri derleniyor.
- Migration **üretilmedi**.
- `dotnet build Ptn.TestModule.slnx -m:1` → 0 hata.
- `dotnet test --filter "Category!=LiveInfrastructure"` → 0 başarısız (taban 255).
- `dotnet test --filter "Category=LiveInfrastructure"` → 2/2 **hâlâ yeşil**.

**Beklenen uç sayısı: 41 → 48** (findings 1, cancel 1, compile-preview 1, environments 2,
download 2). Dilim 2 yeni rota **eklemez**.

---

## 7. Bitiş

1. §5'in 18 maddesini kendi kodunda tek tek kontrol et.
2. Sekiz dilimi sırayla commit et.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → iki filtreli `dotnet test`.
4. `/backend-verify` gate'i; her commit öncesi başlık doğrulayıcısını koştur.
5. Raporda **zorunlu**: **öncesi/sonrası uç sayısı** ve tam liste; Bridge allowlist'inin son
   hâli; şema değişikliği gerektiğini düşündüğün her nokta; her varsayım.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → bir kez.

---

## 8. Kapattığı borç

| Kayıt | Madde |
|---|---|
| `house-profile.md` *Wire the chain to the outside* | Sekiz ulaşılamaz davranış |
| PLAN-0003 TM-09 | *"kooperatif iptal"* — kod vardı, uç yoktu |
| PLAN-0003 TM-13 | `resource_link` vardı, gövde servis eden uç yoktu |
| Kullanıcı hedefi (2026-08-15) | *"test tarafında eksik api ucu servis kalmasın"* |
| — | `OutwardSurfaceTests` ile kalıcı regresyon kapısı |

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| Senaryo sağlığı, kapsam raporu, zamanlama, sözleşme tetikleyicisi, karantina süpürme | **KBP-110** — KBP-104'ün ertelenen Dilim 6-7'si |
| Eşzamanlılık kuyruğu görünürlüğü, elle purge tetikleme | **KBP-110** — operasyon yüzeyi |
| Auth / team / permission uçları | Ayrı deploy: `pintern-authenticator-latest-api` (11 controller hazır) |
| E-posta, şablon, alıcı uçları | Host'ta zaten compose: `Pintern.Notifications` + `Piton.Emailing` |
| Checker uçları (checks, sources, snapshots, connections, assertions…) | Host'ta zaten compose: iki checker kendi yüzeyini veriyor |
| UI'ın kendisi | `ptn-assurance-platform-ui` — ayrı depo, ayrı iş kolu |
| LLM / model sağlayıcı seçimi | Kod tarafı bittikten sonraki karar |
