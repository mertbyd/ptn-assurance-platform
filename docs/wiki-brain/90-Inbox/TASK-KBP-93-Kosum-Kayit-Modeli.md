# AJAN GÖREVİ — KBP-93 · Koşum kayıt modeli, ortam bağlaması ve terminal yazım

Tek görev. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

Bu görev ADR-0016'nın **4 ana tablosundan kalan üçünü** kurar. KBP-92 `test_catalog`'u
kapattı; bu görev `test_run` dünyasını kapatır ve veri modelini bitirir. Koşum motoru,
runner adapter'ı, HAR ve oracle dağıtıcısı **bu görevde yoktur** — onlar bu modelin
içine yazacak olan sonraki task'lardır.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-93   (KBP-92 üzerinden)
Motor   : PostgreSQL
Commit  : #KBP-93 <type>: <past-tense English description>
```

| Ön koşul | Durum |
|---|---|
| **KBP-92 commit edilmiş ve build/test yeşil** | ✅ `84e5184` · senaryo aggregate'i ve beş yayın kapısı yerinde |
| `test_lookup` beş tablosu ve seed | ✅ KBP-90 |
| `TestModuleTableNames.Runs / RunResults / ResultFindings` | ✅ **Zaten tanımlı** — KBP-90'da ilan edildi, kullanılmayı bekliyor |
| `TestModuleDbProperties.RunSchema` | ✅ Zaten tanımlı |
| KBP-714 şema parmak izi ucu | ✅ Kapandı — `ISchemaKnowledgeAppService.GetSchemaFingerprintAsync` gerçek |

Derlenebilir dilimler, **en fazla 4 commit**, testler son dilimde. Boş dosya, yer tutucu,
kullanılmayan using girmez.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek — **bu depoda**) |
|---|---|---|
| Entity (veri kabuğu) | `house-profile.md` → *Entity data shell* | `src/Ptn.TestModule.Domain/Entities/Catalog/TestScenario.cs` |
| Manager | `house-profile.md` → *Base classes* | `src/Ptn.TestModule.Domain/Managers/Catalog/TestScenarioManager.cs` |
| Karar veren manager | `house-profile.md` → *Base classes* | `src/Ptn.TestModule.Domain/Managers/Catalog/ScenarioPublicationGateManager.cs` |
| Repository + arayüz | `data-access.md` | `src/Ptn.TestModule.EntityFrameworkCore/Repository/Catalog/EfCoreTestScenarioRepository.cs` · `Domain/Interface/Catalog/ITestScenarioRepository.cs` |
| EF Configuration | `data-access.md` | `src/Ptn.TestModule.EntityFrameworkCore/Configurations/Catalog/TestScenarioConfiguration.cs` |
| Sabit / kod kümesi | `house-profile.md` → *Stable strings* | `src/Ptn.TestModule.Domain.Shared/Constants/Catalog/TestScenarioConsts.cs` |
| Ayar adı | `house-profile.md` → *Stable strings* | `src/Ptn.TestModule.Domain.Shared/Constants/Bridge/PtnBridgeSettingNames.cs` · `Domain/Settings/TestModuleSettings.cs` |

**Kanonik kararlar:** `ADR-0016` (bu görevin anayasası — özellikle §C aggregate sınırları,
§D çok kiracılılık, §E denetim alanı, §G ortam bağlaması, §H silme/saklama, §J UoW sınırları),
`ADR-0020 §C` (koşum anı kayma kapısı), `ADR-0015 §B` (üç kısa UoW), `RULE-0002`.

**Şema kaynağı:** `docs/wiki-brain/04-Architecture/Test-Platform-Schema.dbml` →
`test_run.test_runs`, `test_run.test_run_results`, `test_run.test_result_findings`.
Kolon adları, tipler, uzunluklar ve indeksler **oradan alınır, uydurulmaz.**

> **DBML okuma uyarısı.** Dosyanın Project notu hâlâ *"Arazzo 1.1"* diyor; **geçerli hedef
> `1.0.1`**'dir (AUDIT-0002 / BULGU-07). Bu görevi etkilemez ama DBML'i tek doğru sanma.

---

## 2. Ne yapıyor

Üç tablo, üç ayrı soru:

| Tablo | Cevapladığı soru | Taban sınıf |
|---|---|---|
| `test_runs` | **Ne istendi** — düğmeye basıldığı an yazılır, hüküm değil | `AuditedAggregateRoot<Guid>`, `IMultiTenant` |
| `test_run_results` | **Ne oldu** — hüküm ve teşhis, denemeye bir satır | `CreationAuditedAggregateRoot<Guid>`, `IMultiTenant` |
| `test_result_findings` | **Farkın tam konumu** | `CreationAuditedEntity<Guid>`, `IMultiTenant` |

Artı iki davranış:

- **Ortam bağlaması** (TM-04): mantıksal ad → `baseUrl` / `specSnapshotId` / `dbConnectionId` /
  `secretRef` eşlemesi ABP tenant-scoped `Setting`'tir, **tablo değildir** (ADR-0016 §G).
  Koşum anında çözülür ve `test_runs` satırına snapshot olarak düşer.
- **Malzeme kayma kapısı** (ADR-0020 §C): koşum başlarken dört mühür yeniden hesaplanır;
  tutmuyorsa sonuç `Failed` değil **`Inconclusive`**'dir.

---

## 3. Sabitlenen tasarım kararları — bunlar tartışmaya açık değil

Aşağıdakiler ADR/DBML'den gelir. Farklı bir şey yapman gerekiyorsa **dur ve sor**; kodda
sessizce doğmasın.

### 3.1 Aggregate sınırları

```
Aggregate 1        Aggregate 2       Aggregate 3
TestScenario ✅    TestRun           TestRunResult
(KBP-92)                             └── TestResultFinding (çocuk entity)
```

Aggregate'ler arası bağ **kimlikledir** — navigation property **yoktur**, yalnız `Guid` ve
DB tarafında FK. Ev precedent'i `TestScenarioConfiguration`'ın
`HasOne<TestScenarioState>().WithMany().HasForeignKey(...)` satırıdır: **generic `HasOne<T>()`,
navigation property'siz.** Aynı biçim kullanılır.

`TestResultFinding` kök **değildir**: kendi başına yaratılmaz, hep `TestRunResult` ile birlikte
yazılır. Doğrudan sorgulanması aggregate ihlali değil, **read model sorgusudur**.

### 3.2 Çok kiracılılık — çocuk tabloda da zorunlu

ABP tenant filtresi **entity tipi bazında** uygulanır ve **miras alınmaz**. `test_result_findings`
çocuk entity olmasına rağmen `IMultiTenant` taşır; taşımazsa o tabloya doğrudan vuran her rapor
sorgusu **tüm kiracıların** satırını döndürür. Bu bir test maddesidir, yorum değil.

### 3.3 Terminal yazımın değişmez kuralı — **Manager uygular**

```
outcome = Passed  →  FailureCategoryId, ErrorCode, Detail, FailedStepOrdinal,
                     FailedStepName, FailedStepPath, TakenBranchPath,
                     LastCompletedOrdinal  ...  HEPSİ null
```

Bu kural entity'de değil `TestRunResultManager`'da yaşar (entity veri kabuğudur). İhlal
`BusinessException` üretir.

### 3.4 Üç kısa UoW (ADR-0016 §J, ADR-0015 §B)

| # | Adım | Kural |
|---|---|---|
| 1 | `test_runs` satırı `Pending`, commit | Kısa; checker çağrısı **yok** |
| 2 | `Running`'e **idempotent claim** — `StartAsync → bool` | Tekrar teslimde **no-op**, exception değil |
| 3 | Terminal yazım | `test_runs` durumu + `test_run_results` + `test_result_findings` **tek atomik yazım** |

`UNIQUE (test_run_id, attempt)` çift yazımı sessiz ikinci satır değil, **gürültülü hata** yapar.
Asılı `Running` koşuları için `(run_status_id, started_at)` indeksi üzerinde süpürücü:
`RecoverStaleRunningAsync`.

### 3.5 Türetilebilen alan tutulmaz

- `test_runs`'ta **`duration_ms` yok** — `completed_at - started_at`.
- `test_runs`'ta **`test_name` / `version_no` yok** — FK `Restrict`, join her zaman çalışır.
- `test_run_results`'ta **`title` yok** — `error_code` + yerelleştirmeden üretilir. Saklanan
  Türkçe başlık İngilizce gösterilemez.
- `test_run_results`'ta **`diagnosis_hypothesis_code` / `confidence_code` kolonu yok** — jsonb
  içinde zaten var; projeksiyon **ölçüldüğünde** eklenir, önceden değil.

### 3.6 Lookup mu `varchar` mı

> Küme kapalı **ve** sözlüğün sahibi biz miyiz → lookup FK.
> Açık uçlu **veya** sahibi başka modül → `varchar` + `Domain.Shared` sabiti.

| Alan | Karar |
|---|---|
| `run_status_id`, `outcome_status_id`, `failure_category_id`, `trigger_kind_id` | **Lookup FK** — beşi de KBP-90'da mevcut |
| `error_code` | **`varchar`** — SARIF `ruleId` gibi bilinçli serbest |
| `source_checker_code` | **`varchar` + sabit** — `ApiContract` / `DatabaseComparison` / `Runner` |
| `comparison_kind_code` | **`varchar`** — DB Checker'ın `matcherCode` sözlüğü, **bizim değil** |

Checker sözlüğünü kendi seed migration'ımızda tutmak ADR-0015 §F'nin modül sınırı yasağını
deler. **Yapma.**

### 3.7 Kimlik biçimleri

- `trace_id`: **32 küçük harf hex** (W3C trace-id, 16 bayt). **`Guid` değildir**, `Guid.ToString()`
  ile üretilmez.
- `history_id`: `SHA-256(test_key ¦ environment_key ¦ kanonik girdiler)`. **MD5 kullanılmaz** (FIPS).
- Uzunluk sınırları hem `Domain.Shared` sabiti hem EF `HasMaxLength`'tir — tek kaynak
  (`TestScenarioConsts` precedent'i).

### 3.8 Güvenli veri sınırı (ADR-0016 §I)

- **Ham stack trace tabloya yazılmaz** — `error_code` + `trace_id` yazılır, iz operasyonel logdadır.
- Token, parola, connection string, hassas kişisel veri **tutulmaz**.
- Satır sonu temizliği **log satırında** yapılır, DB kolonunda değil — çok satırlı teşhis
  değeri korunur.

### 3.9 Ortam eşleşmesi doğrulanır — AUDIT-0001 BULGU-05

Ayar `specSnapshotId` ile `dbConnectionId`'yi yan yana koyuyor ama *"bu ikisi aynı çalışan
sistemi mi tarif ediyor"* hiçbir yerde sorulmuyor. Yanlış eşleştirilmiş bir ortam (staging API +
prod DB) **sessizce** koşar.

Bağlama çözülürken **bir kez** doğrulanır: iki tarafın `environmentKey`'i aynı olmalıdır.
Eşleşmiyorsa koşum **başlamaz** — `Inconclusive` değil, **reddedilir**; çünkü bu bir
yapılandırma hatasıdır, bilgi eksikliği değil.

### 3.10 Kayma `Failed` değil `Inconclusive` — ADR-0020 §C

| Durum | Sonuç |
|---|---|
| Dört mühür de tutuyor | Normal koşum |
| API veya DB mührü tutmuyor | **`Inconclusive`** + `failure_category = Technical`, kayan malzeme raporda adıyla |
| `rules_fingerprint` tutmuyor | **`Inconclusive`** — kural değişti, senaryo bayat |
| Profil mührü tutmuyor | **`Inconclusive`** — adresler yanlışlanmış olabilir |

`Failed` saymak yanlış alarmdır (Google: CI pass→fail geçişlerinin **%84'ü flaky**).
`Skipped` saymak sessiz kapsam kaybıdır.

---

## 4. Dosya manifestosu (≈31 yeni, ≤8 değişen)

> Sıra bağlayıcıdır. 35'i aşarsan **listenin sonundan** kes ve KBP-94'e taşı; hiçbir dosyayı
> yarım bırakma. Kesme bölgesi §4.9'da işaretli.

### 4.1 `Domain.Shared/Constants/Runs/`

| # | Dosya | İçerik |
|---|---|---|
| 1 | `TestRunConsts.cs` | `MaxTestKeyLength=128`, `HashLength=64`, `TraceIdLength=32`, `MaxEnvironmentKeyLength=128`, `MaxTriggerRefLength=256`, `MaxRunnerRefLength=64`, `MaxHarBlobNameLength=256`, `TraceIdPattern`, dört indeks adı |
| 2 | `TestRunResultConsts.cs` | `MaxErrorCodeLength=128`, `MaxDetailLength=4000`, `MaxStepNameLength=256`, `MaxStepPathLength=1000`, `MaxBranchPathLength=256`, `MaxDiagnosisReportBytes=4096`, iki indeks adı |
| 3 | `TestResultFindingConsts.cs` | `MaxLocationLength=1000`, `MaxTargetDisplayNameLength=256`, `MaxMessageLength=1000`, `MaxValueLength=2000`, `MaxEvidenceSummaryLength=2000`, `MaxKindCodeLength=64`, `MaxRuleRefLength=64`, dört indeks adı |
| 4 | `Lookups/TestSourceCheckerCodes.cs` | `ApiContract` `DatabaseComparison` `Runner` + `All` — **lookup tablosu değil**, sabit |
| 5 | `TestModuleRunSettingNames.cs` | Ortam bağlaması ayar anahtarları (precedent: `PtnBridgeSettingNames.cs`) |

### 4.2 `Domain.Shared/ExceptionCodes/Runs/`

| # | Dosya | İçerik |
|---|---|---|
| 6 | `TestModuleRunErrorCodes.cs` | `EnvironmentNotBound` `EnvironmentMismatch` `RunAlreadyClaimed` `AttemptAlreadyWritten` `PassedOutcomeCarriesFailureData` `InvalidTraceId` `DiagnosisReportTooLarge` `RunDeletionNotAllowed` |

### 4.3 `Domain/Entities/Runs/`

*Hepsi veri kabuğu: alanlar `internal set`, EF kurucusu + atama-only kurucu. Metot, `if`, hesap, `throw` **yok**.*

| # | Dosya |
|---|---|
| 7 | `TestRun.cs` |
| 8 | `TestRunResult.cs` |
| 9 | `TestResultFinding.cs` |

### 4.4 `Domain/Models/Runs/`

| # | Dosya | İçerik |
|---|---|---|
| 10 | `TestRunCreateModel.cs` | `ScenarioId?`, `TestKey`, `TriggerKindCode`, `TriggerRef`, `IsDryRun` |
| 11 | `TestRunEnvironmentBinding.cs` | `EnvironmentKey`, `BaseUrl`, `SpecSnapshotId`, `DbConnectionId`, `SecretRef` |
| 12 | `TestRunTerminalModel.cs` | `OutcomeCode`, `FailureCategoryCode?`, `ErrorCode?`, `Detail?`, adım alanları, `DiagnosisReport?`, `Findings[]` |
| 13 | `TestResultFindingModel.cs` | `Ordinal`, `SourceCheckerCode`, `ComparisonKindCode`, `RuleRef?`, `Location`, `Message`, değer alanları |
| 14 | `TestRunMaterialDrift.cs` | `HasDrift`, `DriftedMaterialCodes[]` — dört mührün karşılaştırma sonucu |

### 4.5 `Domain/Interface/Runs/`

| # | Dosya | Üyeler |
|---|---|---|
| 15 | `ITestRunRepository.cs` | `FindByTraceIdAsync`, `GetStaleRunningAsync`, `ExistsActiveForEnvironmentAsync` |
| 16 | `ITestRunResultRepository.cs` | `FindByAttemptAsync`, `GetWithFindingsAsync` (findings `Include` — tek sorgu) |

### 4.6 `Domain/Managers/Runs/`

| # | Dosya | Sorumluluk |
|---|---|---|
| 17 | `TestRunManager.cs` | `Pending` satırı kur · `trace_id` ve `history_id` üret · **idempotent `StartAsync → bool`** · terminal durum geçişi · `RecoverStaleRunningAsync` · silmeyi reddet |
| 18 | `TestRunResultManager.cs` | Terminal yazım · **§3.3 Passed değişmezi** · `attempt` üretimi · `diagnosis_report` boyut sınırı · bulgu sırası (`ordinal`) |
| 19 | `RunEnvironmentBindingManager.cs` | Ayardan çöz · **§3.9 eşleşme doğrulaması** · bağlanmamışsa `EnvironmentNotBound` |
| 20 | `RunMaterialDriftManager.cs` | **§3.10** dört mührü yeniden hesapla, kayan malzemeyi adıyla döndür (precedent: `ScenarioPublicationGateManager`) |

### 4.7 `EntityFrameworkCore/`

| # | Dosya |
|---|---|
| 21 | `Configurations/Runs/TestRunConfiguration.cs` |
| 22 | `Configurations/Runs/TestRunResultConfiguration.cs` |
| 23 | `Configurations/Runs/TestResultFindingConfiguration.cs` |
| 24 | `Repository/Runs/EfCoreTestRunRepository.cs` |
| 25 | `Repository/Runs/EfCoreTestRunResultRepository.cs` |

**Migration:** `dotnet ef migrations add TestRunRecords` — **yalnız `test_run` şeması.**
`--project` ve `--startup-project` açıkça verilir.

Cascade zinciri **yalnız** `test_runs → test_run_results → test_result_findings`.
`test_runs → test_scenarios` **`Restrict`**; senaryo sürümü asla silinmez.
Varsayılan her yerde `Restrict`.

### 4.8 Testler (son dilim)

| # | Dosya | Doğruladığı |
|---|---|---|
| 26 | `Domain.Tests/Runs/TestRunLifecycleTests.cs` | `Pending → Running` claim **idempotent**; ikinci çağrı `false` döner, exception atmaz · `trace_id` 32 küçük harf hex · `history_id` kanonik |
| 27 | `Domain.Tests/Runs/TestRunResultInvariantTests.cs` | **Passed iken sekiz sorun alanı null olmalı** — dolu gelirse reddediliyor · `diagnosis_report` 4 KB üstü reddediliyor |
| 28 | `Domain.Tests/Runs/RunEnvironmentBindingTests.cs` | Bağlanmamış ortam `EnvironmentNotBound` · **`environmentKey` uyuşmazlığı koşumu başlatmıyor** |
| 29 | `Domain.Tests/Runs/RunMaterialDriftTests.cs` | Dört mühürden biri kaydığında sonuç **`Inconclusive` + `Technical`**, asla `Failed` |
| 30 | `EntityFrameworkCore.Tests/Runs/TestRunPersistenceTests.cs` | `UNIQUE(test_run_id, attempt)` çift yazımda patlıyor · cascade zinciri çalışıyor · senaryo `Restrict` · **çocuk `test_result_findings` başka kiracıya sızmıyor** |
| 31 | `EntityFrameworkCore.Tests/EntityFrameworkCore/MigrationScopeTests.cs` *(mevcut, genişlet)* | Yeni migration **yalnız `test_run`** şemasına dokunuyor |

### 4.9 Kesme bölgesi

Bütçe aşılırsa sırayla kesilecekler: **#14 + #20 + #29** (malzeme kayma kapısı).
Kesilirse KBP-94'e **açıkça** devredilir ve bu belgeye not düşülür. Kalan hiçbir madde
kesilmez — hepsi tablo veya değişmez.

### 4.10 Değişecek mevcut dosyalar (≤8)

`TestModuleDbContext.cs` + `ITestModuleDbContext.cs` (üç `DbSet`) ·
`Domain/Settings/TestModuleSettings.cs` + `TestModuleSettingDefinitionProvider.cs`
(ortam bağlaması ayarı) · `Localization/TestModule/{en,tr}.json` +
`TestModuleLocalizationKeys.cs` (hata mesajları) · `MigrationScopeTests.cs`.

---

## 5. Yasaklar

1. **`test_environments` tablosu açma** — ortam ABP `Setting`'tir (ADR-0016 §G).
2. **`run_steps` / adım tablosu açma** — adım kaydı yoktur, kanıt HAR artefaktıdır (ADR-0015).
3. **Partition kurma** — bölümlenmiş tablonun PK'sı bölümleme kolonunu içermek zorundadır ve bu
   ABP'nin tek kolonlu `Guid` anahtar sözleşmesini kırar (ADR-0016 §H).
4. **Checker tablosuna FK verme** — `spec_snapshot_id` ve `db_connection_id` düz `uuid`'dir.
5. **Navigation property yazma** — aggregate'ler arası bağ kimlikledir.
6. Entity'ye metot, `if`, normalizasyon, geçiş, `throw` koyma.
7. `duration_ms`, `title`, `test_name`, `version_no` gibi **türetilebilir kolon** açma.
8. Ham stack trace, token, parola veya connection string'i tabloya yazma.
9. `error_code` / `comparison_kind_code` / `source_checker_code` için **lookup tablosu** açma.
10. Enum kullanma; durum lookup'tan gelir.
11. Runner çağrısı, HTTP çağrısı, background job, HAR okuma, oracle çağrısı **ekleme** —
    bu görev **yalnız kayıt modelidir**.
12. Model/LLM çağrısı ekleme.
13. Yeni katman/klasör (`Helpers/`, `Engines/`, `Infrastructure/`, `Handlers/`).
14. `[MapProperty]`, mapper'da gövde, serviste `private` iş metodu, nested tip.
15. Ara dilimlerde build/test; geçmiş commit arkeolojisi.

---

## 6. Kabul kriterleri

- Üç tablo DBML'deki kolon, uzunluk, indeks ve cascade davranışıyla oluşuyor.
- Migration **yalnız `test_run`** şemasına dokunuyor; `test_catalog` ve `test_lookup` değişmiyor.
- Claim **idempotenttir**: ikinci çağrı `false` döner, satırı bozmaz, exception atmaz.
- `Passed` sonucu sekiz sorun alanının hepsini `null` bırakıyor; ihlal reddediliyor.
- `UNIQUE(test_run_id, attempt)` çift yazımda **gürültülü** hata veriyor.
- `test_result_findings` çocuk olmasına rağmen kiracı sızdırmıyor (testle kanıtlı).
- Ortam `environmentKey` uyuşmazlığı koşumu **başlatmıyor** (reddediliyor, `Inconclusive` değil).
- Malzeme kayması **`Inconclusive` + `Technical`** üretiyor, asla `Failed`.
- `trace_id` 32 küçük harf hex; `Guid.ToString()` hiçbir yerde üretmiyor.
- Taban sınıfın verdiği hiçbir gövde elle yazılmamış.

---

## 7. Bitiş

1. §5'in 15 maddesini kendi kodunda tek tek kontrol et.
2. Son dilimi commit et:
   `#KBP-93 feat: created the test run record model with environment binding and terminal write invariants`
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → `dotnet test Ptn.TestModule.slnx --no-restore`
4. `/abp-backend-dev` mimari incelemesi + `/backend-verify` gate'i.
5. Raporda: dosya listesi, migration adı, kesme bölgesinden bir şey kesildiyse **ne kesildi**,
   yaptığın **her varsayım**.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → **bir kez** dene; aynı komutu döngüde tekrarlama; tek engelde
10 dakikadan fazla harcama.

---

## 8. Bu görevin kapattığı wiki borcu

| Kayıt | Madde |
|---|---|
| `PLAN-0003 TM-04` | Ortam bağlaması ABP `Setting` olarak |
| `PLAN-0003 TM-06` | Koşu kayıt modeli (`test_runs`) |
| `PLAN-0003 TM-06b` | Hüküm ve bulgu modeli (`test_run_results`, `test_result_findings`) |
| `AUDIT-0001 BULGU-05` | Ortam eşleşmesi doğrulanmıyor — *"koşum task'ına düşecek"* |
| `ADR-0020 §C` | Koşum anı kayma tespiti — KBP-92'nin açıkça devrettiği madde |
| `ADR-0016` | **4 ana tablo + 5 lookup modeli tamamlanır** |

## 9. Bu görevde **olmayan** iş — sonraki task'lar

| Ne | Nereye |
|---|---|
| `TestScenarioController` (KBP-92'de dosya bütçesinden kesildi) | **KBP-94** |
| `ITestRunAppService` + DTO + validator + controller | **KBP-94** |
| Rapor read model'i (TM-22) | **KBP-94** |
| Runner adapter'ı, süreç sınırı (TM-60) | KBP-95 |
| HAR artefaktı ve BLOB Storing (TM-12) | KBP-95 |
| Oracle dağıtıcısı (TM-08) | KBP-96 |
| Background job + stale süpürücü koşumu (TM-09) | KBP-96 |
| Saklama/parçalı silme (TM-15) | Sonra |

> Bu görev **yazılabilir bir kayıt modeli** bırakır. Sonraki task'ların hepsi bu modelin
> içine yazar; model yanlış kurulursa her biri migration borcu üretir. PLAN-0003'ün
> *"önce Blok 0"* sırası tam olarak bunun içindir.
