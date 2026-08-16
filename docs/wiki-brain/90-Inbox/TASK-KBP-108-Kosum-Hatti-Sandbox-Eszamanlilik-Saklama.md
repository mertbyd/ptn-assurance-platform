# AJAN GÖREVİ — KBP-108 · Koşum hattı: sandbox, eşzamanlılık ve saklama

Tek görev, **üç derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

Bu görev KBP-103'ün Dilim 4 ve 5'idir. KBP-103 Dilim 3'te mimari blokajla durdu; bu üç madde
**Dilim 3'e bağlı değildir** ve beklemesi için sebep yoktur. **TM-10**, **TM-11**, **TM-15**.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-108   (KBP-103 üzerinden)
Motor   : PostgreSQL
Commit  : #KBP-108 <type>: <past-tense English description>
```

> **Paralellik.** Bu görev `**/Runs/**` ve `Settings/` dokunur. KBP-107 `host/`, `Compilation/`
> ve `HarInterpreter` dokunur; KBP-106 `**/Bridge/**` dokunur. **Üçü de paralel güvenlidir** —
> ama aynı checkout'ta iki ajan `git commit` çalıştıramaz (HANDOFF §0). Ayrı worktree kullan.

| Ön koşul | Durum |
|---|---|
| KBP-103 Dilim 1–2 commit edilmiş | ✅ `349a4d6`, `14ff49d` |
| `ExecuteTestRunJob` + `RecoverStaleRunsJob` kalıbı | ✅ KBP-95 |
| `HarRetentionDays` ayarı (bugün **30**) | ✅ KBP-95 — bu görev **90**'a çeker |
| `ServiceShapeTests` · `ManagerReachabilityTests` | ✅ KBP-101/102 — **bozulmaz** |

**Dosya bütçesi ≈20.** Üç dilim, dilim başına bir commit.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| Domain port | `house-profile.md` → *Ports live in Domain, adapters in Application* | `src/Ptn.TestModule.Domain/Interface/Runs/IWorkflowRunnerPort.cs` |
| Port uygulaması | `house-profile.md` → *An AppService has no private business helpers* | `src/Ptn.TestModule.Application/Services/Runs/HarArtifactService.cs` |
| Manager | `house-profile.md` → *Base classes* | `src/Ptn.TestModule.Domain/Managers/Runs/WorkflowRunPlanner.cs` |
| Background job | mevcut kalıp | `src/Ptn.TestModule.Application/BackgroundJobs/Runs/RecoverStaleRunsJob.cs` |
| ABP Setting | `house-profile.md` → *Stable string ownership* | `src/Ptn.TestModule.Domain/Settings/TestModuleSettingDefinitionProvider.cs` |
| Hata kodu / lokalizasyon | aynı bölüm | `Domain.Shared/ExceptionCodes/Runs/TestModuleRunErrorCodes.cs` + `Localization/TestModule/{en,tr}.json` |

**Kanonik kararlar:** ADR-0007 (checker hedefe **yazmaz**), ADR-0016 §G (ayar) ve **§H**
(partition yasağı), PLAN-0003 TM-10 / TM-11 / TM-15.

---

## 2. Sabitlenen kararlar — tartışmaya açık değil

### 2.1 TM-10 — sandbox bir **port**tur, checker'a yazma yetkisi değildir

`ITestDataSandbox` **`Domain/Interface/Runs/`** altında doğar; uygulaması
**`Application/Services/Runs/`** altında. **Ayrı ve açıkça yetkilendirilmiş** bağlantı
kullanır — checker'ın hedef bağlantısı **değil**. ADR-0007'nin salt-okunur invariant'ı
checker içindir; sandbox SUT'un kendi test verisini kurar.

Reset stratejisi **ayar**dır (`TestModule.Runs.SandboxResetStrategy`), tablo değil.
**Rollback stratejisi yasak** — SUT kendi bağlantısını açtığında çalışmaz (PLAN-0003 TM-10).

### 2.2 TM-11 — eşzamanlılık ABP'nin kendi kilidiyle

Aynı ortamda çakışan koşular **`IDistributedLock`** (`Volo.Abp.DistributedLocking`) ile
sıraya alınır. Kendi kuyruğumuz, Temporal veya ikinci bir durum sahibi **açılmaz** —
PLAN-0003 "Kapsam dışı" tablosu bunu adıyla yasaklıyor.

Kilit anahtarı `TestModuleRunSettingNames` sahipliğinde bir sabitten türer; inline string yok.
Anahtar **ortam bazlıdır** — farklı ortamlardaki koşular birbirini beklemez.

### 2.3 TM-15 — saklama planın değeriyle hizalanır

`DefaultHarRetentionDays` bugün `"30"`; PLAN-0003 TM-15 **90 gün** diyor. Varsayılan **90**'a
çekilir. Koşu satırları için **parçalı silme** job'ı eklenir; blob TTL'i ayarla sürülür.

**Partition açılmaz** — ADR-0016 §H, ABP'nin tek kolonlu `Guid` anahtar sözleşmesini kırar.

### 2.4 Migration

Sandbox ve kilit **şema değişikliği gerektirmemelidir**. Gerektiğini düşünüyorsan **dur ve
raporla**; şema kararı bu görevin dışıdır.

---

## 3. Dilimler

### Dilim 1 — TM-10 test verisi sandbox'ı (≈8 dosya)

| # | Dosya | Ne |
|---|---|---|
| 1 | `Domain/Interface/Runs/ITestDataSandbox.cs` | **yeni** — port |
| 2 | `Domain/Models/Runs/SandboxResetPlan.cs` | **yeni** |
| 3 | `Domain/Managers/Runs/SandboxResetPlanner.cs` | **yeni** — strateji seçimi ve doğrulama |
| 4 | `Application/Services/Runs/TestDataSandboxService.cs` | **yeni** — çıplak I/O, sıfır private iş metodu |
| 5 | `Domain.Shared/Constants/Runs/TestModuleRunSettingNames.cs` + `Settings/*` | **düzenle** |
| 6 | `Domain.Shared/ExceptionCodes/Runs/*` + `{en,tr}.json` | **düzenle** |

**Commit:** `#KBP-108 feat: created the test data sandbox port and its reset planner`

---

### Dilim 2 — TM-11 eşzamanlılık (≈6 dosya)

| # | Dosya | Ne |
|---|---|---|
| 1 | `Domain/Managers/Runs/RunConcurrencyManager.cs` | **yeni** — kilit anahtarı üretimi, çakışma kararı |
| 2 | `Application/BackgroundJobs/Runs/ExecuteTestRunJob.cs` | **düzenle** — kilidi al/bırak |
| 3 | `Domain.Shared/Constants/Runs/*` + `Settings/*` | **düzenle** |

**Kabul:** aynı ortamda iki eşzamanlı koşu **sıraya giriyor**; farklı ortamlardakiler
birbirini **beklemiyor**.

**Commit:** `#KBP-108 feat: created the run concurrency gate for shared environments`

---

### Dilim 3 — TM-15 saklama + testler (≈8 dosya)

`DefaultHarRetentionDays` **90**; `PurgeExpiredRunsJob` (`RecoverStaleRunsJob` kalıbı);
blob TTL. Hepsi ABP setting'i, partition yok.

Testler: `SandboxResetPlannerTests`, `RunConcurrencyManagerTests`, `PurgeExpiredRunsTests`.

**Commit:** `#KBP-108 feat: created the run retention purge job and its coverage`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **Dilim 1** devredilir — üçü içinde en büyük olan odur.
**Kesilmeyecekler: Dilim 2 ve 3.**

---

## 5. Yasaklar

1. Checker'a yazma yetkisi verme (ADR-0007).
2. Rollback reset stratejisi yazma (§2.1).
3. Kendi kuyruğumuzu, Temporal'ı veya ikinci durum sahibini açma (§2.2).
4. Partition açma (ADR-0016 §H).
5. Sandbox/kilit ayarlarını tablo olarak açma (§2.1, §2.2).
6. Inline kilit anahtarı veya ayar adı yazma — `Domain.Shared` sahibidir.
7. Application servisine private iş metodu veya guard koyma — `ServiceShapeTests` **kapıdır**.
8. `Domain/Managers/**` içine `Process`/`File`/`Directory` yazma (KBP-102 kuralı).
9. Yeni proje, yeni katman, `Infrastructure/`, `Helpers/`, `Utils/` açma.
10. **Migration üretme** — gerekiyorsa dur ve raporla (§2.4).
11. Koşum hattına model çağrısı ekleme (RULE-0005).
12. Kırılan testi silme, `Skip` etme, assertion zayıflatma.
13. `KBP-95..107` dallarına commit; force-push, rebase, amend.
14. Ara dilimlerde build/test atlama.

---

## 6. Kabul kriterleri

- `ITestDataSandbox` portu var; **ayrı bağlantı** kullanıyor; rollback stratejisi **yok**.
- Aynı ortamda iki eşzamanlı koşu sıraya giriyor; farklı ortamlar birbirini beklemiyor.
- HAR ve koşu saklaması **90 gün**; purge job'ı koşuyor; **partition yok**.
- Migration **üretilmedi**.
- `dotnet build Ptn.TestModule.slnx -m:1` → 0 hata.
- `dotnet test --filter "Category!=LiveInfrastructure"` → 0 başarısız.
- `dotnet test --filter "Category=LiveInfrastructure"` → KBP-103'ün lint kanıtı **hâlâ yeşil**.

---

## 7. Bitiş

1. §5'in 14 maddesini kendi kodunda tek tek kontrol et.
2. Üç dilimi sırayla commit et.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → iki filtreli `dotnet test`.
4. `/backend-verify` gate'i; her commit öncesi başlık doğrulayıcısını koştur.
5. Raporda **zorunlu**: sandbox'ın kullandığı bağlantının nasıl ayrıldığı; kilit anahtarının
   gerçek biçimi; iki eşzamanlı koşumun sıraya girdiğinin kanıtı; şema değişikliği gerektiğini
   düşündüğün her nokta; her varsayım.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → bir kez.

---

## 8. Kapattığı borç

| Kayıt | Madde |
|---|---|
| PLAN-0003 Blok 1 | **TM-10**, **TM-11** |
| PLAN-0003 Blok 2 | **TM-15** |
| KBP-103 | Serbest bırakılan Dilim 4 ve 5 |

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| Migration orkestrasyonu, token turu, yeşil E2E | **KBP-107** |
| Rapor, artefakt, ihracat, operasyon | **KBP-104** |
| Checker yazarlık yüzeyleri | **KBP-106** |
| Ajan yüzeyi, MCP, Overlay | **KBP-105** |
| LLM / model sağlayıcı seçimi | Kod tarafı bittikten sonraki karar |
