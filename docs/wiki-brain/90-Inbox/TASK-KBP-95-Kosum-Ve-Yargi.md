# AJAN GÖREVİ — KBP-95 · An 5 koşum ve An 6 yargı: runner, job, HAR ve oracle dağıtıcısı

Tek görev, **dört derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

Bu görev ürünün **çalıştığı yeri** kurar. KBP-93 kayıt modelini bıraktı; bu görev o modele
gerçekten yazan motoru getirir: dış runner'ı çalıştırır, HAR üretir, HAR'ı checker'lara verir,
hükmü ve teşhisi yazar. **Bitişinde elle yazılmış bir Arazzo senaryosu uçtan uca koşar.**

> **Numaralandırma kaydı.** `KBP-94` **kullanılmadı**: planlanan kapsamı (Application yüzeyi,
> iki controller, rapor read model) `KBP-93` içinde inmiştir. Bu satır silinmez.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-95   (KBP-93 üzerinden)
Motor   : PostgreSQL
Runner  : redocly/cli  (Docker, SABIT surum)
Commit  : #KBP-95 <type>: <past-tense English description>
```

| Ön koşul | Durum |
|---|---|
| KBP-93 commit edilmiş, build/test yeşil | ✅ `48b2fcd` |
| `test_runs` / `test_run_results` / `test_result_findings` + managers | ✅ KBP-93 |
| `TestRunManager.RecoverStaleRunningAsync` | ✅ **var ama çağıran yok** |
| `ITestRunRepository.ExistsActiveForEnvironmentAsync` | ✅ **var ama kullanan yok** |
| `TestRun.HarBlobName` kolonu | ✅ **var ama depo yok** |
| `AbpBackgroundJobsModule` `DependsOn`'da | ✅ **var ama job yok** |
| `RunMaterialDriftManager` | ✅ **var ama çağıran yok** |
| Bridge yüzeyleri (`ApiOracleAppService`, `FailureDiagnosisAppService`, `DatabaseOracleAppService`) | ✅ KBP-88/91 |
| **KBP-100** (Arazzo derleyicisi) | ⚠️ **paralel** — bu görev **elle yazılmış** belgeyle çalışır, derleyiciyi beklemez |

**Dosya bütçesi ≈50.** Dört dilim, dilim başına bir commit. Testler son dilimde.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| Background job | `layers-and-files.md` | `checkers/api-contract/src/*.Application/BackgroundJobs/Shared/ApiContractCheckerTenantBackgroundJob.cs` **ve** `BackgroundJobs/Runs/ContractCheckExecutionBackgroundJob.cs` |
| Job args sözleşmesi | `mapping.md` | `checkers/api-contract/src/*.Application.Contracts/BackgroundJobs/ITenantBackgroundJobArgs.cs` |
| Dış sistemi çağıran servis | `house-profile.md` → *AppService has no private helpers* | `src/Ptn.TestModule.Application/Services/Bridge/WriteSetCapabilityAppService.cs` |
| Manager | `house-profile.md` → *Base classes* | `src/Ptn.TestModule.Domain/Managers/Runs/TestRunManager.cs` |
| Port arayüzü | `layers-and-files.md` | `src/Ptn.TestModule.Domain/Interface/Runs/ITestRunRepository.cs` (biçim) |
| Sabit / kod kümesi | `house-profile.md` → *Stable strings* | `src/Ptn.TestModule.Domain.Shared/Constants/Runs/TestRunConsts.cs` |

**Kanonik kararlar:** `ADR-0015` (**bu görevin anayasası** — §A runner dışarıda, §B akış ve UoW,
§C DB adımı, §D zamanlama, §E kayıt sahibi, §G yasaklar), `ADR-0016 §H/§I/§J`,
`ADR-0021` (korelasyon), `ADR-0020 §C` (kayma), `RULE-0005` (koşumda model yok).

**Denetim düzeltmeleri — ADR metnine değil bunlara uy:** `AUDIT-0002` BULGU-07/08/09/10.

---

## 2. Sabitlenen kararlar — tartışmaya açık değil

### 2.1 Runner çağrı sözleşmesi

| Konu | Değer | Kaynak |
|---|---|---|
| Arazzo hedef sürümü | **`1.0.1`** — 1.1 **değil** | BULGU-07 |
| Severity | **`--severity` CLI bayrağı, JSON nesnesi.** `REDOCLY_CLI_RESPECT_SEVERITY` diye bir şey **yok** | BULGU-08 |
| Dört kontrolün severity'si | **Her koşumda açıkça** set edilir; varsayılana güvenilmez | BULGU-08 |
| Girdiler | **`REDOCLY_CLI_RESPECT_INPUT` ortam değişkeni.** *"Girdi dosyası"* yolu **yoktur** | BULGU-09 |
| Maskeleme | **`--no-secrets-masking` asla açılmaz** | ADR-0015 §G |
| Çıktı | `--har-output` (HAR 1.2) + `--json-output` | ADR-0015 §A |
| Timeout | `--execution-timeout` (vars. 1 sa) + `--max-fetch-timeout` (vars. 40 sn) + **job seviyesinde sert kill** | ADR-0015 risk tablosu |

Severity haritası **birebir budur** (ADR-0015 §E):

```
STATUS_CODE_CHECK      error    ← akış kontrolü
SUCCESS_CRITERIA_CHECK error    ← akış kontrolü
SCHEMA_CHECK           warn     ← kalıcı hükmü BİZİM checker'ımız verir
CONTENT_TYPE_CHECK     warn     ← aynı
```

`SCHEMA_CHECK`'i `error` bırakmak, Respect'in API Contract Checker'ın kayıt sahibi olduğu hükmü
kendi başına vermesi demektir — §E'nin engellemek istediği şeyin ta kendisi.

### 2.2 Akış (ADR-0015 §B) — UoW sınırları serttir

```
ExecuteTestRunJob (tenant scope, AsyncBackgroundJob)
  ├─ [UoW] Running'e idempotent claim        ← KBP-93'ün StartAsync'i, no-op olabilir
  ├─ HAZIRLIK (UoW DIŞI)  ortam çözümü · malzeme mührü kontrolü · kısa ömürlü token
  ├─ İCRA    (UoW DIŞI)  respect → HAR + JSON        ← süreç sınırı
  ├─ YARGI   (UoW DIŞI)  HAR → checker'lar
  └─ [UoW] TERMİNAL YAZIM (tek atomik, YENİ UoW)
```

**Checker'ın uzak çağrısıyla DB transaction açık tutulmaz.** Terminal yazım **ayrı yeni UoW**'dadır.

### 2.3 Zamanlama kuralı (ADR-0015 §D) — en çok kaçırılan madde

| Kontrol | Nerede |
|---|---|
| Response uygunluğu | **Koşum sonrası, HAR'dan** — saf fonksiyon, birebir aynı sonuç |
| DB assertion | **Koşum sırasında, Arazzo adımı olarak** — zaten belgede |

İki sonucu var:

1. **Response uygunluğu HAR'ın HER entry'si için çalışır**, yalnız kırmızılar için değil. Bir adım
   `$statusCode == 200`'ü geçmiş ama gövdesi şemaya uymuyor olabilir.
2. **DB assertion'ı HAR'dan tekrar çalıştırmak YASAKTIR** — sonraki adımlar durumu değiştirmiş
   olabilir. Dağıtıcı DB assertion adımlarını **yanıt olarak okur**, yeniden çağırmaz.

### 2.4 HAR entry ↔ senaryo adımı bağı **kimlikle** kurulur

`ADR-0021`: her checker giriş DTO'su opsiyonel `CorrelationRef { TraceId, StepKey }` taşır ve
sonuç DTO'su **aynen geri yansıtır**. Echo edilen `StepKey` **HAR gövdesine** düşer.

**Bu yüzden dağıtıcı HAR entry'sini adım sırasına/adına göre değil, `StepKey` ile bağlar.**
`AUDIT-0001` BULGU-01 tam olarak bu yolun kırıldığını kaydetmişti; alan artık iki checker'da da
mevcut (KBP-628/711). Konumla eşleme **yasaktır**; `StepKey` yoksa o bulgu `Inconclusive`
gerekçesi taşır.

### 2.5 Üç hakem, tek kayıt sahibi

`test_result_findings.source_checker_code` ∈ `{ApiContract, DatabaseComparison, Runner}`
— sabit KBP-93'te **zaten var** (`TestSourceCheckerCodes`).

| Kaynak | Rolü |
|---|---|
| `Runner` | Hızlı ön kapı — akış kontrolü |
| `ApiContract` | **Sözleşme hükmünün kayıt sahibi** |
| `DatabaseComparison` | **Kalıcılık hükmünün kayıt sahibi** |

### 2.6 Hata sınıflandırması

| Durum | Sonuç |
|---|---|
| `OperationCanceledException` | **`Cancelled`** — `Technical` değil |
| Bilinen `BusinessException` | `Code` **korunur** |
| Bilinmeyen exception | Kararlı **`Technical`** kategorisine indirgenir, yutulmaz |
| Runner süreci çöktü | **`Broken`** — adım seviyesinde devam **yoktur**, senaryo baştan koşar |
| Malzeme mührü tutmuyor | **`Inconclusive`** + `Technical` (ADR-0020 §C) |
| Ortam eşleşmesi yanlış | Koşum **başlamaz** — reddedilir (AUDIT-0001 BULGU-05, KBP-93'te kurulu) |

### 2.7 HAR deposu

`ADR-0016 §H`: HAR artefaktı **ABP BLOB Storing**'e gider, satırda yalnız `har_blob_name` kalır.
Kanıt ≤ 4 KB satır içi, üstü blob. **Sağlayıcı S3-uyumlu** (AWS provider + `ServiceURL`);
**Database provider yalnız geliştirmede.**

> **Bu depoda BLOB Storing precedent'i YOK.** İlk entegrasyon bu görevle geliyor; container
> tanımı, TTL ayarı ve sağlayıcı seçimi `ADR-0016 §H`'nin **açık gereksinimi** olarak kabul edilir.

Maskeleme notu (BULGU-10): Respect `format: password` alanlarını ve `x-security` başlıklarını
**dosya çıktısında da** maskeler — yani HAR'ımız maskeli gelir. Bu **tek savunma hattı değildir**;
spec'te bildirilmemiş secret maskelenmez, ACL redaksiyonu yerinde kalır.

### 2.8 Runner adapter'ı nereye yazılır — **karar**

`ADR-0015 §A` *"`Ptn.TestModule.EntityFrameworkCore/Adapters/` altında yaşar"* diyor. **Ama
`Adapters/` klasörü bu depoda hiç açılmadı**; KBP-88/91'in kurduğu ve kabul edilen desen
`Application/Services/Bridge/*AppService.cs` → checker çağrısı, `Domain/Managers/Bridge/*` → karar.

**Karar: uygulanan desen izlenir.** Runner bir veritabanı bileşeni değildir; EF Core projesine
koymanın tek gerekçesi *"yeni proje açılmaz"*dı ve o kısıt zaten korunuyor.

```
Domain/Interface/Runs/IWorkflowRunnerPort.cs        ← sözleşme
Application/Services/Runs/WorkflowRunnerService.cs  ← süreç sınırı (Bridge servis deseni)
```

`ADR-0015 §A`'nın klasör cümlesi **eskimiştir**; düzeltme notu `KBP-97`'ye eklenir.
**Yeni proje veya katman yine açılmaz.**

---

## 3. Dilimler ve dosya manifestosu

### Dilim 1 — Runner sınırı ve HAR deposu (≈14 dosya)

**`Domain.Shared/Constants/Runs/`**

| # | Dosya | İçerik |
|---|---|---|
| 1 | `WorkflowRunnerConsts.cs` | Pinlenmiş imaj + sürüm, `ArazzoTargetVersion = "1.0.1"`, timeout varsayılanları, `RunnerRefFormat` |
| 2 | `Lookups/RespectCheckCodes.cs` | `STATUS_CODE_CHECK` `SUCCESS_CRITERIA_CHECK` `SCHEMA_CHECK` `CONTENT_TYPE_CHECK` |
| 3 | `Lookups/RespectSeverityCodes.cs` | `error` `warn` `off` |
| 4 | `HarArtifactConsts.cs` | Container adı, TTL ayar anahtarı, azami boyut |

**`Domain.Shared/ExceptionCodes/Runs/`** — mevcut `TestModuleRunErrorCodes` **genişletilir**:
`RunnerImageUnavailable` `RunnerTimedOut` `RunnerExitedNonZero` `HarNotProduced` `HarTooLarge`
`ArazzoVersionUnsupported` `XPathCriteriaRejected`

**`Domain/Models/Runs/`**

| # | Dosya | İçerik |
|---|---|---|
| 5 | `WorkflowRunRequest.cs` | Belge, girdi sözlüğü, severity haritası, timeout'lar, `TraceId` |
| 6 | `WorkflowRunOutcome.cs` | Çıkış kodu, HAR içeriği, JSON özeti, süre, `RunnerRef` |
| 7 | `HarEntryModel.cs` | İstek/yanıt/zamanlama + **`StepKey`** (echo'dan çözülür) |
| 8 | `HarDocument.cs` | `Entries[]` + üretim meta verisi |

**`Domain/Interface/Runs/`**

| # | Dosya | Üyeler |
|---|---|---|
| 9 | `IWorkflowRunnerPort.cs` | `ExecuteAsync(WorkflowRunRequest, CancellationToken)` |
| 10 | `IHarArtifactStore.cs` | `SaveAsync → blobName`, `ReadAsync`, `DeleteAsync` |

**`Domain/Managers/Runs/`**

| # | Dosya | Sorumluluk |
|---|---|---|
| 11 | `WorkflowRunPlanner.cs` | Severity haritasını kur · girdi sözlüğünü hazırla · **XPath criteria içeren belgeyi reddet** · Arazzo sürümünü doğrula · timeout bütçesi |
| 12 | `HarInterpreter.cs` | HAR'ı `HarDocument`'e çevir · **`StepKey` echo'sunu çöz** · DB assertion adımlarını **işaretle** (yeniden çağrılmayacak) |

**`Application/Services/Runs/`**

| # | Dosya | Sorumluluk |
|---|---|---|
| 13 | `WorkflowRunnerService.cs` | Süreç sınırı: `redocly/cli` sabit imaj · **girdiler env ile** · `--severity` JSON bayrağı · `--har-output`/`--json-output` · sert kill · **`--no-secrets-masking` asla** |
| 14 | `HarArtifactService.cs` | ABP BLOB Storing container'ı; satıra `har_blob_name` |

**Değişecek:** `Ptn.TestModule.Application.csproj` (`Volo.Abp.BlobStoring` + AWS provider) ·
`TestModuleApplicationModule.cs` (container + provider kaydı) · `TestModuleSettings` +
`TestModuleSettingDefinitionProvider` (runner imajı, timeout, HAR TTL).

**Commit:** `#KBP-95 feat: created the arazzo runner process boundary and har artifact store`

---

### Dilim 2 — Dayanıklı koşum: job, eşzamanlılık, süpürücü (≈9 dosya)

**`Application.Contracts/BackgroundJobs/`**

| # | Dosya | Not |
|---|---|---|
| 15 | `ITestModuleTenantBackgroundJobArgs.cs` | Precedent: api-contract `ITenantBackgroundJobArgs` |
| 16 | `Runs/ExecuteTestRunArgs.cs` | `TestRunId`, `TenantId`, `TraceId` |
| 17 | `Runs/RecoverStaleRunsArgs.cs` | Eşik süresi |

**`Application/BackgroundJobs/`**

| # | Dosya | Sorumluluk |
|---|---|---|
| 18 | `Shared/TestModuleTenantBackgroundJob.cs` | Taban: tenant scope + cancellation + `Begin(requiresNew: true)` kabuğu |
| 19 | `Runs/ExecuteTestRunJob.cs` | **§2.2 akışının tamamı.** Claim → hazırlık → icra → yargı → terminal |
| 20 | `Runs/RecoverStaleRunsJob.cs` | `TestRunManager.RecoverStaleRunningAsync`'i **çağırır** (bugün çağıran yok) |

**`Domain/Managers/Runs/`** — mevcut `TestRunManager` **genişletilir**:

| # | Ne | Neden |
|---|---|---|
| 21 | **TM-11 eşzamanlılık zorlaması** | `ExistsActiveForEnvironmentAsync` bugün **hiçbir yerden çağrılmıyor**; aynı ortamda ikinci koşum sıraya alınır/reddedilir |

**`Application/Services/Runs/`** — mevcut `TestRunAppService` **genişletilir**:

| # | Ne |
|---|---|
| 22 | `CreateAsync` sonrası `IBackgroundJobManager.EnqueueAsync` → **202 + runId** |
| 23 | `TestRunController`'a tetikleme ucu (transport sarmalayıcı, tek AppService çağrısı) |

**Commit:** `#KBP-95 feat: created the durable test run job with concurrency guard and stale recovery`

---

### Dilim 3 — An 6: oracle dağıtıcısı ve teşhis (≈12 dosya)

**`Domain/Models/Runs/`**

| # | Dosya | İçerik |
|---|---|---|
| 24 | `OracleDispatchResult.cs` | Hüküm + kategori + `Findings[]` + teşhis raporu |
| 25 | `StepJudgement.cs` | `StepKey`, kaynak hakem, outcome, kanıt |

**`Domain/Managers/Runs/`**

| # | Dosya | Sorumluluk |
|---|---|---|
| 26 | `OracleDispatchManager.cs` | **HAR'ın HER entry'si** → response uygunluğu · kırmızılar → teşhis · DB adımlarını **yeniden çağırmadan** oku · üç hakemin bulgularını `source_checker_code` ile birleştir |
| 27 | `RunOutcomeResolver.cs` | Adım hükümlerinden koşu hükmünü türet: `Passed`/`Failed`/`Broken`/`Inconclusive` + `failure_category` |

**`Application/Services/Runs/`**

| # | Dosya | Sorumluluk |
|---|---|---|
| 28 | `OracleDispatchService.cs` | Mevcut Bridge yüzeylerini çağırır: `ApiOracleAppService` (uygunluk), `FailureDiagnosisAppService` (teşhis). **Checker AppService'ini doğrudan çağırmaz** |

**Diğer:** teşhis raporu ≤ 4 KB sınırı (KBP-93'ün `TestRunResultManager`'ında zaten var, **bağlanır**);
`rule_ref` bulguya taşınır; `CorrelationRef` üretimi ve echo doğrulaması.

**Commit:** `#KBP-95 feat: created the har oracle dispatcher and diagnosis binding`

---

### Dilim 4 — Testler ve uçtan uca (≈13 dosya)

| # | Test | Doğruladığı |
|---|---|---|
| 29 | `WorkflowRunPlannerTests` | Severity haritası **dört kontrolü de açıkça** set ediyor |
| 30 | `WorkflowRunPlannerTests` | **XPath criteria içeren belge reddediliyor** |
| 31 | `WorkflowRunPlannerTests` | Arazzo `1.1` belgesi reddediliyor, `1.0.1` kabul ediliyor |
| 32 | `WorkflowRunnerServiceTests` | Girdiler **env ile** geçiyor; CLI bayrağında secret **yok** |
| 33 | `WorkflowRunnerServiceTests` | `--no-secrets-masking` **hiçbir kod yolunda** üretilmiyor |
| 34 | `WorkflowRunnerServiceTests` | Timeout aşımında süreç öldürülüyor, koşum `Broken` |
| 35 | `HarInterpreterTests` | Entry ↔ adım bağı **`StepKey` ile**; konumla eşleme yok |
| 36 | `HarInterpreterTests` | `StepKey` yoksa sonuç `Inconclusive` gerekçesi taşıyor |
| 37 | `OracleDispatchManagerTests` | **Yeşil adımlar da** uygunluk kontrolünden geçiyor |
| 38 | `OracleDispatchManagerTests` | **DB assertion adımı HAR'dan yeniden çağrılmıyor** |
| 39 | `OracleDispatchManagerTests` | Bulgular `source_checker_code` taşıyor |
| 40 | `ExecuteTestRunJobTests` | Malzeme kayması → `Inconclusive` + `Technical`; `OperationCanceledException` → `Cancelled` |
| 41 | `ExecuteTestRunJobTests` | Aynı ortamda ikinci koşum eşzamanlı **başlamıyor** |
| 42 | `TestRunEndToEndTests` | **Elle yazılmış Arazzo belgesi uçtan uca koşuyor; tek satır model çağrısı yok** |

**Commit:** `#KBP-95 test: created runner boundary oracle dispatch and end to end run coverage`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **TM-10 test verisi sandbox'ı** (`ITestDataSandbox` portu ve reset stratejisi)
bir sonraki task'a devredilir. Bu görevin kabul kriteri **tek koşum** olduğu için sandbox
zorunlu değildir; ardışık koşumların birbirini bozması **bilinen ve kayıtlı** bir sınır olarak
raporlanır.

Kesilmeyecekler: dilim 1-2-3'ün hiçbir maddesi ve **#33, #38, #42** testleri.

---

## 5. Yasaklar

1. **Kendi HTTP adım koşum motorunu yazma** — TM-07 iptal (ADR-0015 §A).
2. **Runner'ı fork'lama, plugin yazma** — DB oracle zaten HTTP (§C).
3. **DB assertion'ını HAR'dan yeniden çalıştırma** — durum değişmiş olabilir (§D).
4. **HAR entry'sini konumla/adla eşleme** — `StepKey` ile (ADR-0021).
5. **Yalnız kırmızı adımlara uygunluk uygulama** — her entry (§D).
6. `--no-secrets-masking` **açma**.
7. Secret'ı **CLI bayrağına** koyma — `REDOCLY_CLI_RESPECT_INPUT` (BULGU-09).
8. `SCHEMA_CHECK`/`CONTENT_TYPE_CHECK`'i `error` bırakma (§E).
9. Severity'yi **set etmeden** koşma — varsayılan dokümante değil (BULGU-08).
10. **Adım seviyesinde devam** yolu açma — çökmede `Broken`, baştan koşum (§G).
11. Checker'ın **uzak çağrısıyla transaction açık tutma** (§B).
12. Terminal yazımı claim UoW'una **koyma** — ayrı yeni UoW.
13. **Model/LLM çağrısı ekleme** — An 5 ve An 6'da model **yoktur** (RULE-0005).
14. Checker AppService'ini **doğrudan** çağırma — mevcut Bridge servisleri üzerinden.
15. Checker tablosu okuma, FK verme, ortak transaction (ADR-0015 §F).
16. Yeni proje, yeni katman, `Infrastructure/`, `Engines/`, `Handlers/` açma.
17. Ham stack trace'i tabloya yazma — `error_code` + `trace_id` (ADR-0016 §I).
18. Ara dilimlerde build/test.

---

## 6. Kabul kriterleri

- **Elle yazılmış bir Arazzo `1.0.1` senaryosu uçtan uca yeşil koşuyor ve tek satır model
  çağrısı yok.** *(PLAN-0003 Blok 1'in resmî kabul ölçütü.)*
- Dört Respect kontrolünün severity'si her koşumda açıkça set ediliyor; `SCHEMA_CHECK` `warn`.
- Girdiler env değişkeniyle geçiyor; süreç listesinde secret görünmüyor.
- HAR blob'a yazılıyor, satırda yalnız `har_blob_name` var.
- **HAR'ın her entry'si** uygunluk kontrolünden geçiyor; DB adımları yeniden çağrılmıyor.
- Entry ↔ adım bağı **`StepKey`** ile kuruluyor.
- Bulgular üç hakem arasında `source_checker_code` ile ayrışıyor.
- Aynı ortamda eşzamanlı ikinci koşum başlamıyor.
- Asılı `Running` koşuları süpürücü tarafından toparlanıyor.
- Malzeme kayması `Inconclusive`, iptal `Cancelled`, runner çöküşü `Broken`.
- `test_runs.runner_ref` hangi runner sürümüyle koşulduğunu taşıyor.
- Migration **üretilmiyor** — bu görev şema değiştirmez.

---

## 7. Bitiş

1. §5'in 18 maddesini kendi kodunda tek tek kontrol et.
2. Dört dilimi sırayla commit et.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → `dotnet test Ptn.TestModule.slnx --no-restore`
4. `/abp-backend-dev` mimari incelemesi + `/backend-verify` gate'i.
5. Raporda: dosya listesi, **runner imaj sürümü**, sandbox kesildiyse etkisi, yaptığın **her varsayım**.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → bir kez; döngüde tekrar etme.

---

## 8. Kapattığı wiki borcu

| Kayıt | Madde |
|---|---|
| `PLAN-0003 TM-60` | Runner adapter'ı ve süreç sınırı |
| `PLAN-0003 TM-08` | Oracle dağıtıcısı |
| `PLAN-0003 TM-09` | Dayanıklı koşum — job, claim, stale süpürücü |
| `PLAN-0003 TM-11` | Eşzamanlılık |
| `PLAN-0003 TM-12` | HAR artefaktı ve BLOB Storing |
| `PLAN-0003 TM-21` | Teşhis bağlama |
| `ADR-0015` | **Koşum sınırının ilk gerçek uygulaması** |
| `ADR-0021` | Korelasyonun HAR yolunda **fiilen kullanılması** |
| `AUDIT-0002` BULGU-07/08/09/10 | Dördü de koda uygulanır |

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| TM-10 test verisi sandbox'ı | Kesme bölgesindeyse bir sonraki task |
| TM-13/14 CTRF · JUnit · SARIF dışa aktarımı | Sonraki |
| TM-15 saklama, parçalı silme, blob TTL koşumu | Sonraki |
| TM-16 OTel telemetrisi | Sonraki |
| Arazzo derleyicisi (TM-05) | **KBP-100** — paralel |
| Yazarlık ajanı (Blok 3) | *"Ancak koşum ve yargı kanıtlandıktan sonra anlamlı"* — PLAN-0003 |
