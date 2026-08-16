# AJAN GÖREVİ — KBP-101 · Dış yüzey tamamlama: ulaşılamayan Manager'lar ve sözleşmesiz servisler

Tek görev, **dört derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

Bu görev yeni iş mantığı yazmaz. Yazılmış ama **dışarıdan ulaşılamayan** domain davranışını
house profilinin zincirine bağlar. Gerekçe `house-profile.md` → *Wire the chain to the outside*:

> *"A manager is not finished while it is unreachable. Every use case completes the full house
> chain in the same slice of work: `Manager -> AppService -> Dto/Model/Mapper -> FluentValidation
> -> Controller`. Leaving domain behavior with no AppService and no controller route is an
> **incomplete task**, not a smaller task."*

Bu modül dışarıdan **ajanlar** tarafından tüketilecek. Ajan bir kodu HTTP'den keşfedemiyorsa
tahmin eder — `RULE-0007` bunu yasaklıyor.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-101   (KBP-99 üzerinden)
Motor   : PostgreSQL
Commit  : #KBP-101 <type>: <past-tense English description>
```

| Ön koşul | Durum |
|---|---|
| KBP-99 üç dilimi commit edilmiş, build/test yeşil | ⚠️ **doğrula** (`e374aff`, `a501f50`, `ad6aaa8`) |
| 5 lookup entity + repo + EF repo + configuration | ✅ KBP-90 |
| 5 lookup Manager'ı `LookupManager<TEntity, Guid>` tabanında | ✅ KBP-90 |
| `TestScenarioAppService` = `BaseApplicationService<...>` kalıbı | ✅ KBP-92 |
| `TestRunResult.Findings` aggregate child koleksiyonu | ✅ KBP-93 |

**Dosya bütçesi ≈40.** Dört dilim, dilim başına bir commit. Her dilim yeşil kapanır.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| Lookup AppService | `house-profile.md` → *Base classes and dependency registration* | Foundation paketindeki `LookupAppService<...>` **imzasını paketten çöz** (§2.2) |
| CRUD AppService kalıbı | `house-profile.md` → *Architectural spine* | `src/Ptn.TestModule.Application/Services/Catalog/TestScenarioAppService.cs` |
| AppService arayüzü | `house-profile.md` → *Contracts live in Application.Contracts* | `src/Ptn.TestModule.Application.Contracts/Services/Catalog/ITestScenarioAppService.cs` |
| Controller | `house-profile.md` → *Architectural spine* | `src/Ptn.TestModule.HttpApi/Controllers/Catalog/TestScenarioController.cs` |
| DTO + validator | `contracts-mapping.md` | `Application.Contracts/Dtos/Catalog/*` + `FluentValidation/Catalog/*` |
| Mapperly | `house-profile.md` → *Mapper files contain declarations only* | `src/Ptn.TestModule.Application/Mappers/Catalog/TestScenarioMapper.cs` |
| Rota / izin sabiti | `house-profile.md` → *Stable string ownership* | `Domain.Shared/Constants/Catalog/TestScenarioRoutes.cs` + `Permissions/TestModulePermissions.Scenarios.cs` |

**Kanonik kararlar:** `house-profile.md` §*Wire the chain to the outside*, §*Contracts live in
Application.Contracts*, §*Base classes*, §*One type, one file*; `RULE-0007` (ajan tahmin etmez);
`ADR-0016` (kayıt modeli); `PLAN-0003 TM-22` (rapor read model'i).

---

## 2. Sabitlenen kararlar — tartışmaya açık değil

### 2.1 Ölçülen boşluk — bu görevin tamamı budur

**Manager ulaşılabilirliği** (kendi klasörü dışından çağrılan dosya sayısı, 2026-08-15 ölçümü):

| Manager | Çağıran | Sonuç |
|---|---|---|
| `TestRunStatusManager` | **0** | ❌ ulaşılamaz |
| `TestOutcomeStatusManager` | **0** | ❌ ulaşılamaz |
| `TestFailureCategoryManager` | **0** | ❌ ulaşılamaz |
| `TestTriggerKindManager` | **0** | ❌ ulaşılamaz |
| `TestScenarioStateManager` | **0** | ❌ ulaşılamaz |
| diğer 23 Manager | 1–6 | ✅ bağlı |

**Sözleşme arayüzü eksik Application servisleri:**

| Servis | Durum |
|---|---|
| `OracleDispatchService` | ❌ ne `Application.Contracts` arayüzü ne Domain port'u var |
| `ScenarioCompilationService` | ❌ aynı |
| `RedoclyArazzoDocumentLinter` | ✅ `IArazzoDocumentLinter` Domain port'unu uygular — **eksik değil** |
| `HarArtifactService` · `ProcessBoundaryService` · `WorkflowRunnerService` | ✅ Domain port'u uygular |

`house-profile.md` → *Contracts live in Application.Contracts*: *"Every public service gets its
interface in `Application.Contracts/Services/`; a service with no contract interface cannot be
consumed or replaced."*

**Okuma yüzeyi eksiği:** `ITestRunAppService` yalnız `GetAsync(id)` ve `GetResultAsync(id)`
taşıyor — **liste ucu yok**, **bulgular ve teşhis raporu okunamıyor**. TM-22 açık.

### 2.2 Lookup yüzeyi Foundation tabanıyla yazılır — elle CRUD yazılmaz

Beş Manager **zaten** `LookupManager<TEntity, Guid>` tabanında. `house-profile.md` →
*Base classes* tablosu eşleşen AppService tabanını da adlandırıyor: **`LookupAppService<...>`**
(`Nexum.Abp.Foundation.Application.Services`).

**Bu depoda `LookupAppService` kullanan bir kardeş YOK.** İlk iş onun **gerçek generic imzasını
Foundation paketinden çözmektir** — assembly'yi incele, `LookupManager` ile aynı namespace
ailesine bak, `TestScenarioAppService`'in `BaseApplicationService` kullanımını biçim referansı al.
**İmzayı tahmin etme.** Çözemezsen dur ve raporla; elle CRUD AppService yazarak geçiştirme.

Lookup yüzeyi **salt-okunurdur**: liste + detay. Create/Update/Delete **açılmaz** — lookup satırları
`DataSeedContributor` ile gelir (KBP-90) ve dışarıdan yazılamaz.

### 2.3 Bu görevde **olmayan** iş — `ProcessBoundaryService`

`ProcessBoundaryService`'in on private metodu **yerinde kalır**. `house-profile.md` →
*An AppService has no private business helpers* bölümü sınırı açıkça çiziyor:

> *"What legitimately stays in an Application service is the **boundary mechanic itself**: process
> start/kill, file and temp-directory handling, stream reading, HTTP transport, BLOB container
> access, cancellation plumbing. Deciding **what** to run, **which** flags, **which** severity, and
> **what the result means** is Manager work even when it feeds a process call."*

O on metot birebir bu listedir: geçici klasör, dosya yazma, process başlatma/öldürme, stream
okuma, iptal plumbing'i. Kararların sahibi **zaten** `ArazzoLintManager` (`CreatePlan` / `Interpret`)
ve `WorkflowRunPlanner`. `abp-coding-standards` §4 ayrıca uyarıyor: *"Never apply 'move private
methods to Manager' mechanically."*

**Tek istisna — gerçek bir kusur, §3 Dilim 2'de düzeltilir:** timeout'ta önce process öldürülür,
sonra `finally` içindeki `DeleteWorkspace` koşar. Windows'ta öldürülen process dosya handle'ını
hemen bırakmayabilir; `Directory.Delete` `IOException` atarsa asıl `BusinessException(TimeoutErrorCode)`
**maskelenir** ve çağıran yanlış hatayı görür.

### 2.4 `TestResultFinding`'e repository **açılmaz**

`CreationAuditedEntity<Guid>`'dir, aggregate root değildir; `TestRunResult` onu
`private List<TestResultFinding> _findings` ile sahiplenir. `house-profile.md` →
*Entity profile: data shell* bunu zaten öngörüyor: *"child collection exposure required by
EF/aggregate persistence"*.

Bulgular **parent üzerinden** okunur — yani TM-22 rapor read model'i ile (§3 Dilim 3).
Ayrı `ITestResultFindingRepository` veya ayrı controller **açılmaz**.

---

## 3. Dilimler ve dosya manifestosu

### Dilim 1 — Beş lookup okuma yüzeyi (≈22 dosya)

Her lookup için, `TestScenario` kalıbını izleyerek:

**`Domain.Shared/`**

| # | Ne |
|---|---|
| 1 | `Constants/Lookups/TestLookupRoutes.cs` — beş rota, tek sahip |
| 2 | `Permissions/TestModulePermissions.Lookups.cs` — partial dosya kalıbı (`.Scenarios.cs` gibi) |

**`Application.Contracts/`** — beş lookup × (DTO + liste input + AppService arayüzü)

| # | Ne | Not |
|---|---|---|
| 3 | `Dtos/Lookups/*Dto.cs` | Foundation lookup DTO tabanı varsa **onu kullan** |
| 4 | `Dtos/Lookups/*ListInput.cs` | yalnız gerekiyorsa; taban zaten sağlıyorsa **yazma** |
| 5 | `Services/Lookups/I*AppService.cs` | beş arayüz |
| 6 | `Permissions/Definitions/Lookups/TestModulePermissionDefinitionProvider.Lookups.cs` | |

**`Application/`**

| # | Ne |
|---|---|
| 7 | `Services/Lookups/*AppService.cs` — beş servis, **`LookupAppService<...>` tabanında** |
| 8 | `Mappers/Lookups/*Mapper.cs` — Mapperly, **yalnız bildirim** |

**`HttpApi/`**

| # | Ne |
|---|---|
| 9 | `Controllers/Lookups/*Controller.cs` — beş controller, salt-okunur liste + detay |

**Yasak:** Create/Update/Delete ucu açmak. Elle CRUD AppService yazmak (§2.2). Validator yazmak —
salt-okunur yüzeyin public input DTO'su yoksa validator da yoktur.

**Commit:** `#KBP-101 feat: created the read surface for the five test lookups`

---

### Dilim 2 — Sözleşmesiz servisler ve süreç sınırı kusuru (≈5 dosya)

| # | Dosya | Değişiklik |
|---|---|---|
| 10 | `Application.Contracts/Services/Runs/IOracleDispatchService.cs` | **yeni** — `OracleDispatchService`'in public yüzeyi |
| 11 | `Application/Services/Runs/OracleDispatchService.cs` | arayüzü uygular |
| 12 | `Application.Contracts/Services/Compilation/IScenarioCompilationService.cs` | **yeni** |
| 13 | `Application/Services/Compilation/ScenarioCompilationService.cs` | arayüzü uygular |
| 14 | `Application/Services/Shared/ProcessBoundaryService.cs` | `finally` temizliği asıl exception'ı **maskelemez** (§2.3) |

14 numara için: temizlik hatası yutulmaz da maskelemez de — asıl hata çağırana ulaşır, temizlik
hatası ayrı kanaldan görünür. Çözümü **depodaki mevcut kalıptan** seç; yeni bir loglama
mekanizması icat etme.

**Yasak:** Bu iki servise controller açmak — ikisi de iç orkestrasyondur, HTTP yüzeyi değildir.
Arayüz **tüketilebilirlik ve test edilebilirlik** içindir.

**Commit:** `#KBP-101 refactor: created the missing application service contracts and hardened the process workspace cleanup`

---

### Dilim 3 — Koşum okuma yüzeyi, TM-22 (≈8 dosya)

`ITestRunAppService` bugün liste ucu taşımıyor ve bulgular/teşhis okunamıyor.

| # | Ne | Not |
|---|---|---|
| 15 | `Dtos/Runs/TestRunListInput.cs` | liste sorgusu |
| 16 | `Dtos/Runs/TestReportDetailDto.cs` | findings + `diagnosis_report` **dahil** |
| 17 | `ITestRunAppService` | `GetListAsync` + `GetReportAsync` |
| 18 | `TestRunAppService` | iki uç; rapor **tek sorguda** findings `Include` eder |
| 19 | `ITestRunRepository` + `EfCoreTestRunRepository` | rapor sorgusu — **tüm EF/LINQ burada** |
| 20 | `TestRunMapper` | yeni DTO eşlemeleri, ignore eklemeden |
| 21 | `TestRunController` | iki rota |
| 22 | `TestRunRoutes` / permissions | sabitler tek sahipte |

**Kural (PLAN-0003 TM-22):** liste ucu findings ve `diagnosis_report` **projekte etmez** — ağır
kolonlar liste sorgusuna girmez. Yalnız detay ucu taşır.

**Commit:** `#KBP-101 feat: created the test run list and report read model`

---

### Dilim 4 — Testler (≈6 test)

| # | Test | Doğruladığı |
|---|---|---|
| 23 | `LookupSurfaceTests` | Beş lookup ucu seed edilmiş kodları **HTTP yüzeyinden** döndürüyor |
| 24 | `LookupSurfaceTests` | Create/Update/Delete ucu **yok** |
| 25 | `ManagerReachabilityTests` | Her `*Manager` en az bir AppService/servis tarafından çağrılıyor — **regresyon kapısı** |
| 26 | `ServiceContractTests` | `Application/Services/**` altındaki her public servisin sözleşme arayüzü veya Domain port'u var |
| 27 | `TestRunReportTests` | Rapor ucu findings + `diagnosis_report` taşıyor; **liste ucu taşımıyor** |
| 28 | `ProcessBoundaryServiceTests` | Timeout + temizlik hatası birlikte olduğunda çağıran **`TimeoutErrorCode`** görüyor |

25 ve 26 bu görevin asıl kalıcı değeridir: bir daha ulaşılamaz Manager veya sözleşmesiz servis
**derlenmeden** yakalanır.

**Commit:** `#KBP-101 test: created the outward surface and manager reachability coverage`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **Dilim 3** (TM-22) bir sonraki göreve devredilir — PLAN-0003'te zaten kayıtlı bir
kalemdir. Kesilmeyecekler: **Dilim 1, Dilim 2 ve #25, #26 testleri.**

---

## 5. Yasaklar

1. Lookup'lara **yazma ucu** açma — seed ile gelirler.
2. `LookupAppService` imzasını **tahmin etme**; paketten çöz, çözemezsen dur ve raporla (§2.2).
3. Elle CRUD AppService yazarak Foundation tabanını **atlama**.
4. `ProcessBoundaryService`'in boundary mechanic private metotlarını **Manager'a taşıma** (§2.3).
5. `TestResultFinding`'e repository, manager veya controller **açma** (§2.4).
6. `OracleDispatchService` / `ScenarioCompilationService`'e **controller** açma.
7. Yeni proje, yeni katman, `Infrastructure/`, `Handlers/`, `Providers/` açma.
8. Nested tip yazma — **bir tip bir dosya** (`house-profile.md`).
9. Mapper dosyasına gövde, `[MapProperty]`, LINQ veya kanıtlanmamış ignore koyma.
10. EF/LINQ'i AppService'e yazma — **repository'ye**.
11. Liste ucuna findings/`diagnosis_report` projekte etme (§Dilim 3).
12. Sabit rota/izin/hata kodunu Domain.Shared dışında tanımlama.
13. Elle DI kaydı yazma — taban veya `ITransientDependency` konvansiyoneldir.
14. Migration üretme — **bu görev şema değiştirmez.**
15. Kırılan testi silme veya `Skip` etme.
16. `KBP-95` / `KBP-99` / `KBP-100` dallarına commit atma; force-push, rebase, amend.
17. Ara dilimlerde build/test atlama — **her dilim yeşil kapanır.**

---

## 6. Kabul kriterleri

- **Sıfır ulaşılamaz Manager**: her `*Manager` kendi klasörü dışından en az bir kez çağrılıyor ve
  bu bir testle korunuyor (#25).
- `Application/Services/**` altındaki her public servisin sözleşme arayüzü veya Domain port'u var,
  testle korunuyor (#26).
- Beş lookup kodu **HTTP'den keşfedilebiliyor**; ajan artık `Domain.Shared` kaynağını okumak
  zorunda değil (`RULE-0007`).
- Lookup uçlarında yazma yok.
- Koşum raporu findings ve `diagnosis_report` taşıyor; liste ucu taşımıyor.
- Timeout + temizlik hatası birlikte olduğunda çağıran `TimeoutErrorCode` görüyor.
- `dotnet build Ptn.TestModule.slnx -m:1` → 0 hata; `dotnet test` → 0 başarısız.
- Migration **üretilmiyor**.

---

## 7. Bitiş

1. §5'in 17 maddesini kendi kodunda tek tek kontrol et.
2. Dört dilimi sırayla commit et.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → `dotnet test Ptn.TestModule.slnx --no-restore`
4. `/backend-verify` gate'i; her commit öncesi başlık doğrulayıcısını koştur.
5. Raporda: dosya listesi, **`LookupAppService` imzasını nasıl çözdüğün**, kesilen madde varsa
   etkisi, `ProcessBoundaryService` temizlik düzeltmesinin hangi depo kalıbını izlediği,
   yaptığın **her varsayım**.

**Bilinen tarayıcı false positive'i — düzeltmeye kalkma:** `check-backend-diff.ps1`,
`Domain/Managers/Catalog/TestScenarioManager.cs` içindeki `Ensure*`/`Normalize*` metotları için
`[ENTITY]` bulgusu üretir. O dosya bir Manager'dır ve metotlar tam yerindedir (2026-08-15
doğrulandı). Sayıyı raporla, refactor etme.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → bir kez; döngüde tekrar etme.

---

## 8. Kapattığı wiki borcu

| Kayıt | Madde |
|---|---|
| `PLAN-0003 TM-22` | Rapor read model'i (Dilim 3) |
| `house-profile.md` *Wire the chain to the outside* | Beş ulaşılamaz Manager |
| `house-profile.md` *Contracts live in Application.Contracts* | İki sözleşmesiz servis |
| `RULE-0007` | Ajanın lookup kodlarını tahmin etmek zorunda kalmaması |
| Kullanıcı talebi (2026-08-15) | Dışarıdan kullanılacak her yüzeyin tam zinciri |

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| `ProcessBoundaryService` private metotlarının taşınması | **Yapılmayacak** — §2.3 |
| `TestResultFinding` repository'si | **Yapılmayacak** — §2.4 |
| TM-13/14 CTRF · JUnit · SARIF dışa aktarımı | Sonraki |
| TM-15 saklama, blob TTL | Sonraki |
| TM-16 OTel telemetrisi | Sonraki |
| MCP yüzeyi | Blok 3 — yalnız `Application.Contracts` sınırında (ADR-0008) |
| Canlı altyapı smoke | Ayrı iş |
