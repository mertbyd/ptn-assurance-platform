# AJAN GÖREVİ — KBP-103 · Canlı koşum kanıtı ve koşum hattının kapanışı

> [!IMPORTANT] KAPANIŞ KAYDI — 2026-08-15
> **Bu görev iki dilimle kapandı; kalan üç dilim yeniden dağıtıldı.**
>
> | Dilim | Durum |
> |---|---|
> | 1 — `alpha.8` sürüm bumpı | ✅ `349a4d6` |
> | 2 — canlı redocly lint turu | ✅ `14ff49d` — gerçek konteyner, XPath reddi, canlı 2/2 |
> | 3 — yeşil uçtan uca koşum | ⛔ **iki mimari blokajla durduruldu** → **[[90-Inbox/TASK-KBP-107-Composition-Host-Gerceklik-Turu\|KBP-107]]** |
> | 4–5 — sandbox, eşzamanlılık, saklama | ↪ **serbest bırakıldı** → **[[90-Inbox/TASK-KBP-108-Kosum-Hatti-Sandbox-Eszamanlilik-Saklama\|KBP-108]]** |
>
> **Blokaj 1:** host `autoMigrate`'i tek context'e kapsıyor ama `seedOnStartup` tüm graph'a
> yayılıyor; temiz PostgreSQL'de `42P01`. Consumer kabul kapısı 6 ve 7 hiç koşmamış.
>
> **Blokaj 2:** `HarInterpreter` `stepKey`'i yalnız yanıt gövdesinden okuyor; sıradan SUT
> yanıtı echo etmez. **ADR-0021 bu konuyu hiç kapsamıyordu** — çelişki değil, boşluk.
> **[[03-Decisions/ADR-0022-Sut-Adim-Korelasyonu-Derleme-Aninda-Istek-Basligi|ADR-0022]]**
> boşluğu derleme anında enjekte edilen istek header'ıyla kapattı.
>
> **Spec kusuru — kaydedilmiştir:** §2.1 lookup uçlarını SUT seçerken bu uçların
> `TestModulePermissions.Lookups.Default` ile **izinli** olduğu gözden kaçtı. Yeşil koşum
> gerçek token istiyor; kapı 10 da açık. KBP-107 üçünü birden kapatıyor.
>
> Ajan sahte E2E kanıtı üretmedi ve kapıda durdu — **doğru davranış** (§5.13).

Tek görev, **beş derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

Bu görev modülün **en büyük kanıt boşluğunu** kapatır. Bugün 170 unit testi geçiyor ama
`redocly/cli` **hiç çalıştırılmadı** ve hiçbir Arazzo belgesi uçtan uca **yeşil** koşmadı.
Blok 1'in üstündeki her şey icra edilmemiş bir runner sınırının üzerine inşa edildi.

Ardından koşum hattının kalan üç maddesi kapanır: **TM-10**, **TM-11**, **TM-15**.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-103   (KBP-102 üzerinden)
Motor   : PostgreSQL
Commit  : #KBP-103 <type>: <past-tense English description>
```

> **Numara uyarısı.** `KBP-103/104/105` önerilir; YouTrack'te teyit edilmelidir (PLAN-0005 §0 kalıbı).

| Ön koşul | Durum |
|---|---|
| KBP-102 beş dilimi commit edilmiş, build/test yeşil | ✅ `a5b089e`, 170/170 doğrulandı 2026-08-15 |
| `redocly/cli:2.14.0` imajı yerelde | ✅ çekildi 2026-08-15, digest `sha256:f96b920a…` |
| Docker engine · PostgreSQL · Vault · Redis · Mailpit | ✅ HANDOFF §5 envanteri |
| `ServiceShapeTests` · `ManagerReachabilityTests` · `ServiceContractTests` | ✅ KBP-101/102 — bu görev onları **bozmamalı** |

### 0.1 Görev öncesi hijyen — ilk commit bu

Çalışma ağacında **sahipsiz** bir değişiklik var ve wiki onu kanon yazmış durumda:

```
ptn-test-module/common.props                 CheckNexusDatabaseComparisonVersion 0.2.0-alpha.6 -> 0.2.0-alpha.8
test/.../Services/Bridge/PackageBoundaryTests.cs   assertion aynı sürüme hizalı
```

Bu ikisi tutarlı ve build/test yeşil, ama **hiçbir ticket'a bağlı değil**; HEAD'i klonlayan
hâlâ `alpha.6` alır. İlk commit bunları sahiplenir.

`OracleDispatchService.cs`'deki boş satır kaybı **paralel formatter artefaktıdır** — alan
yorumları arasındaki boşluk modülün her yerinde var. `git checkout -- ` ile **geri al**,
commit'e alma.

**Commit:** `#KBP-103 chore: adopted the database checker alpha.8 consumer version bump`

**Dosya bütçesi ≈35.** Beş dilim, dilim başına bir commit.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| Canlı altyapı testi | `verification.md` → *integration test host* | `test/Ptn.TestModule.EntityFrameworkCore.Tests/Runs/TestRunPersistenceTests.cs` |
| Domain port | `house-profile.md` → *Ports live in Domain, adapters in Application* | `src/Ptn.TestModule.Domain/Interface/Runs/IWorkflowRunnerPort.cs` |
| Port uygulaması (servis) | `house-profile.md` → *An AppService has no private business helpers* | `src/Ptn.TestModule.Application/Services/Runs/WorkflowRunnerService.cs` |
| Manager | `house-profile.md` → *Base classes* | `src/Ptn.TestModule.Domain/Managers/Runs/WorkflowRunPlanner.cs` |
| Background job | mevcut kalıp | `src/Ptn.TestModule.Application/BackgroundJobs/Runs/RecoverStaleRunsJob.cs` |
| ABP Setting | `house-profile.md` → *Stable string ownership* | `src/Ptn.TestModule.Domain/Settings/TestModuleSettingDefinitionProvider.cs` |
| Hata kodu / lokalizasyon | aynı bölüm | `Domain.Shared/ExceptionCodes/Runs/TestModuleRunErrorCodes.cs` + `Localization/TestModule/{en,tr}.json` |

**Kanonik kararlar:** ADR-0015 (koşum sınırı, dış runner, pinli imaj), ADR-0007 (checker hedefe
**yazmaz**), ADR-0016 §G/§H (ayar ve saklama), RULE-0005 (koşumda model yok),
PLAN-0003 Blok 1 kabul ölçütü.

---

## 2. Sabitlenen kararlar — tartışmaya açık değil

### 2.1 SUT kararı — Test Module kendi host'udur

Canlı koşum için **harici bir SUT beklenmez.** KBP-101 altı gerçek okuma ucu açtı ve lookup
satırları `TestModuleLookupDataSeedContributor` ile **deterministik** geliyor:

```
GET /api/test-module/test-run-statuses
GET /api/test-module/test-outcome-statuses
GET /api/test-module/test-failure-categories
GET /api/test-module/test-trigger-kinds
GET /api/test-module/test-scenario-states
```

Bu uçlara karşı yazılan bir Arazzo belgesi PLAN-0003 Blok 1'in kabul ölçütünü **birebir**
karşılar: *"elle yazılmış bir Arazzo senaryosu uçtan uca yeşil koşuyor ve tek satır model
çağrısı yok."* Dogfooding meşru bir SUT'tur; modülün kendi sözleşmesini test etmesi
ADR-0015'i ihlal etmez.

**Checker host'ları:** `checkers/api-contract` ve `checkers/database-comparison` ince
host'ları source tree'de duruyor. Dilim 2'de **API checker host'u ayağa kaldırılır**;
DB checker adımı Dilim 2'nin kapsamında **değildir** (kendi bağlantı profilini ister).

### 2.2 Canlı testler ayrı bir kategori — unit suite'i kirletmez

Canlı altyapı testleri **`[Trait("Category", "LiveInfrastructure")]`** ile işaretlenir ve
varsayılan `dotnet test` koşusundan **filtrelenir**. Gerekçe: CI'da Docker garanti değil;
170 unit testinin determinizmi korunur.

```
dotnet test Ptn.TestModule.slnx --filter "Category!=LiveInfrastructure"   → varsayılan kapı
dotnet test Ptn.TestModule.slnx --filter "Category=LiveInfrastructure"    → bu görevin kanıtı
```

Yeni test projesi **açılmaz**. Canlı testler mevcut `Ptn.TestModule.Application.Tests` içinde
`LiveInfrastructure/` klasöründe yaşar — precedent: `Composition/` klasörü (KBP-92/101).

### 2.3 TM-10 — sandbox bir **port**tur, checker'a yazma yetkisi değildir

`ITestDataSandbox` **Domain/Interface/Runs/** altında doğar; uygulaması
**Application/Services/Runs/** altında. Ayrı ve **açıkça yetkilendirilmiş** bağlantı kullanır
— checker'ın hedef bağlantısı **değil**. ADR-0007'nin salt-okunur invariant'ı checker içindir;
sandbox SUT'un kendi test verisini kurar.

Reset stratejisi **ayar** olarak taşınır (`TestModule.Runs.SandboxResetStrategy`), tabloya
yazılmaz. Rollback stratejisi **yasak** — SUT kendi bağlantısını açtığında çalışmaz (PLAN-0003 TM-10).

### 2.4 TM-11 — eşzamanlılık ABP'nin kendi kilidiyle

Aynı ortamda çakışan koşular **`IDistributedLock`** (ABP `Volo.Abp.DistributedLocking`) ile
sıraya alınır. Yeni orkestrasyon kümesi, Temporal veya kendi kuyruğumuz **açılmaz** —
PLAN-0003 "Kapsam dışı" tablosu bunu adıyla yasaklıyor.

Kilit anahtarı `TestModuleRunSettingNames` sahipliğinde bir sabitten türer; inline string yazılmaz.

### 2.5 TM-15 — saklama planın değeriyle hizalanır

Bugün `DefaultHarRetentionDays = "30"`; PLAN-0003 TM-15 **90 gün** diyor. Varsayılan
**90'a çekilir** ve koşu satırları için parçalı silme job'ı eklenir. **Partition açılmaz**
(ADR-0016 §H — ABP'nin tek kolonlu `Guid` anahtar sözleşmesini kırar).

---

## 3. Dilimler

### Dilim 1 — Sahipsiz sürüm bumpı (§0.1) · 2 dosya

`common.props` + `PackageBoundaryTests.cs` commit edilir; `OracleDispatchService.cs` geri alınır.

**Commit:** `#KBP-103 chore: adopted the database checker alpha.8 consumer version bump`

---

### Dilim 2 — Canlı lint turu (≈5 dosya) · **KBP-100'ün kanıtı**

| # | Dosya | Ne |
|---|---|---|
| 1 | `test/.../LiveInfrastructure/RedoclyLintLiveTests.cs` | **yeni** — gerçek `redocly/cli:2.14.0` konteyneri |
| 2 | `test/.../LiveInfrastructure/Fixtures/valid-lookup-scenario.arazzo.yaml` | **yeni** — elle yazılmış, geçerli Arazzo `1.0.1` |
| 3 | `test/.../LiveInfrastructure/Fixtures/xpath-criteria.arazzo.yaml` | **yeni** — kapıda **reddedilmesi** gereken belge |
| 4 | `test/.../LiveInfrastructure/LiveInfrastructureCollection.cs` | **yeni** — Docker preflight; imaj yoksa `Skip` |

Kanıtlanacak: `IArazzoDocumentLinter` **stub değil gerçek** konteynerle temiz sonuç veriyor;
XPath criteria taşıyan belge **reddediliyor** (ADR-0015 §G); `ProcessBoundaryService`'in
timeout ve `StartFailure` yolları gerçek process'le davranışını koruyor.

**Commit:** `#KBP-103 test: created the live redocly lint round against the pinned container`

---

### Dilim 3 — Uçtan uca yeşil koşum (≈7 dosya) · **KBP-95'in kabul ölçütü**

| # | Dosya | Ne |
|---|---|---|
| 1 | `test/.../LiveInfrastructure/TestRunGreenPathLiveTests.cs` | **yeni** — host ayağa kalkar, senaryo koşar, **yeşil** biter |
| 2 | `test/.../LiveInfrastructure/Fixtures/lookup-readback.arazzo.yaml` | **yeni** — §2.1'in uçlarına karşı elle yazılmış senaryo |
| 3 | `test/.../LiveInfrastructure/LiveHostFixture.cs` | **yeni** — `HttpApi.Host` + PostgreSQL konteyneri |

Zincir **eksiksiz** koşar: `ScenarioCompilationService` → `WorkflowRunnerService` → gerçek
runner → HAR → `HarInterpreter` → `OracleDispatchManager` → `RunOutcomeResolver` →
`TestRunResultManager` → `test_run_results` satırı.

**Kabul:** `TestRun.StatusCode` terminalde **Passed**; `OutcomeCode` **Passed**; HAR blob'da;
`ExecuteTestRunJob` gerçekten kuyruktan koştu; **sıfır model çağrısı** (mevcut reflection testi
korunur). Bu dilim yeşil kapanmadan Dilim 4'e geçilmez.

**Commit:** `#KBP-103 test: created the end to end green run against the live runner and host`

---

### Dilim 4 — TM-10 sandbox + TM-11 eşzamanlılık (≈10 dosya)

| # | Dosya | Ne |
|---|---|---|
| 1 | `Domain/Interface/Runs/ITestDataSandbox.cs` | **yeni** — port |
| 2 | `Domain/Models/Runs/SandboxResetPlan.cs` | **yeni** |
| 3 | `Domain/Managers/Runs/SandboxResetPlanner.cs` | **yeni** — strateji seçimi, doğrulama |
| 4 | `Application/Services/Runs/TestDataSandboxService.cs` | **yeni** — çıplak I/O |
| 5 | `Domain/Managers/Runs/RunConcurrencyManager.cs` | **yeni** — kilit anahtarı üretimi, çakışma kararı |
| 6 | `Application/BackgroundJobs/Runs/ExecuteTestRunJob.cs` | **düzenle** — kilidi al/bırak |
| 7 | `Domain.Shared/Constants/Runs/TestModuleRunSettingNames.cs` | **düzenle** — sandbox + kilit ayarları |
| 8 | `Domain/Settings/TestModuleSettingDefinitionProvider.cs` | **düzenle** |
| 9 | `Domain.Shared/ExceptionCodes/Runs/TestModuleRunErrorCodes.cs` + `{en,tr}.json` | **düzenle** |

**Commit:** `#KBP-103 feat: created the test data sandbox port and the run concurrency gate`

---

### Dilim 5 — TM-15 saklama + testler (≈8 dosya)

`DefaultHarRetentionDays` **90**; koşu satırları için parçalı silme job'ı
(`PurgeExpiredRunsJob`, `RecoverStaleRunsJob` kalıbı); blob TTL. Hepsi ABP setting'i.

Unit testler: `SandboxResetPlannerTests`, `RunConcurrencyManagerTests`, `PurgeExpiredRunsTests`.

**Commit:** `#KBP-103 feat: created the run retention purge job and its coverage`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **Dilim 4 ve 5** KBP-104'e devredilir. **Kesilmeyecekler: Dilim 1, 2, 3.**
Dilim 2 ve 3 bu görevin varlık sebebidir.

---

## 5. Yasaklar

1. Kendi Arazzo parser'ımızı veya koşum motorumuzu yazma (ADR-0015 §A).
2. Runner'ı fork'lama, plugin yazma, imaj sürümünü pinden çıkarma (§C).
3. Canlı testi varsayılan `dotnet test` koşusuna sızdırma (§2.2).
4. Yeni test projesi, yeni katman, `Infrastructure/`, `Helpers/`, `Utils/` açma.
5. `Domain/Managers/**` içine `Process`/`File`/`Directory`/`Path.GetTempPath` yazma (KBP-102 kuralı).
6. Application servisine private iş metodu veya guard koyma — `ServiceShapeTests` **kapıdır**.
7. Checker'a yazma yetkisi verme (ADR-0007).
8. Koşum hattına **model çağrısı** ekleme (RULE-0005).
9. Partition açma (ADR-0016 §H).
10. Kırılan testi silme, `Skip` etme, assertion zayıflatma. *(Docker yoksa `Skip` yalnız §2.2'nin preflight'ında meşrudur.)*
11. `KBP-95/99/100/101/102` dallarına commit; force-push, rebase, amend.
12. Ara dilimlerde build/test atlama.

---

## 6. Kabul kriterleri

- Gerçek `redocly/cli:2.14.0` konteyneri koştu; lint temiz; XPath belgesi **reddedildi**.
- Elle yazılmış Arazzo senaryosu **uçtan uca yeşil** koştu; `test_run_results` satırı `Passed`.
- **Sıfır model çağrısı** — mevcut reflection testi korunuyor.
- `ITestDataSandbox` portu var, ayrı bağlantı kullanıyor, rollback stratejisi **yok**.
- Aynı ortamda iki eşzamanlı koşu **sıraya giriyor**, birbirinin verisini bozmuyor.
- HAR ve koşu saklaması 90 gün; purge job'ı koşuyor; partition **yok**.
- `dotnet build Ptn.TestModule.slnx -m:1` → 0 hata.
- `dotnet test --filter "Category!=LiveInfrastructure"` → **170+ geçiyor, 0 başarısız**.
- `dotnet test --filter "Category=LiveInfrastructure"` → **hepsi geçiyor**.
- Migration: **sandbox/kilit şema değişikliği gerektirmiyorsa üretilmez.** Üretilirse tam okunur.

---

## 7. Bitiş

1. §5'in 12 maddesini kendi kodunda tek tek kontrol et.
2. Beş dilimi sırayla commit et.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → iki filtreli `dotnet test`.
4. `/backend-verify` gate'i; her commit öncesi başlık doğrulayıcısını koştur.
5. Raporda **zorunlu**: canlı koşumun HAR çıktısından alıntı; terminal satırın gerçek değerleri;
   runner'ın gerçek exit code'u; hangi konteynerlerin ayağa kalktığı; `Skip` edilen her test ve
   sebebi; her varsayım.

**Bilinen tarayıcı false positive'i:** `check-backend-diff.ps1`, `TestScenarioManager.cs`
içindeki `Ensure*`/`Normalize*` için `[ENTITY]` üretir. O bir Manager'dır. Raporla, refactor etme.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → bir kez.

---

## 8. Kapattığı borç

| Kayıt | Madde |
|---|---|
| PLAN-0003 Blok 1 kabul ölçütü | *"elle yazılmış Arazzo senaryosu uçtan uca yeşil koşuyor"* |
| HANDOFF §5 | KBP-95 ve KBP-100'ün iki kanıtsız kabul kriteri |
| Roadmap *"T1 dikey dilimi"* | aynı ölçüt |
| PLAN-0003 | **TM-10**, **TM-11**, **TM-15** |
| — | Sahipsiz `alpha.8` bumpı ticket'a bağlandı |

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| CTRF / JUnit / SARIF ihracatı, OTel, sağlık view'ı, zamanlama | **KBP-104** |
| MCP yüzeyi, ajan profilleri, Overlay yaması | **KBP-105** |
| DB checker adımının canlı koşumu | Ayrı iş — kendi bağlantı profilini ister |
| LLM / model sağlayıcı seçimi | Kod tarafı bittikten sonraki karar |
| Wiki senkronu | Her task'ın §7'sinde kendi sayfasını günceller |
