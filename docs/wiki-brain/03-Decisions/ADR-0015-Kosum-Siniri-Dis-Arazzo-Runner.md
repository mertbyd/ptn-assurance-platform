---
id: ADR-0015
type: decision
status: accepted
title: Kosum siniri — Arazzo runner disarida, hakem iceride
created: 2026-08-13
updated: 2026-08-13
owners:
  - mertbyd
supersedes:
superseded_by: null
decision_refs:
  - ADR-0007
  - ADR-0008
  - ADR-0014
rule_refs:
  - RULE-0002
  - RULE-0005
---

# ADR-0015 — Koşum sınırı: Arazzo runner dışarıda, hakem içeride

> Silinen ADR-0011'in **modül entegrasyonu** bölümünü devralır (§F) ve genişletir.
> PLAN-0003'ün TM-07 maddesini (kendi adım koşum motorumuz) **iptal eder**.

## Bağlam

Önceki plan kendi HTTP adım koşum motorumuzu yazmayı öngörüyordu (TM-07, boyut **L**):
`successCriteria` değerlendirmesi, `retryLimit`/`retryAfter`, `timeout`, runtime expression
çözümü. Üstüne süresiz Arazzo spec-uyum bakımı.

Tarama sonucu ([[90-Inbox/RESEARCH-0013-Runner-Oracle-Ayrimi-Ve-Ajan-Yazarlik-Kaniti|RESEARCH-0013]] §1-2):

- **.NET/C# Arazzo runner yoktur** — ne `awesome-arazzo`'da, ne `openapi.tools`'da, ne
  Microsoft.OpenApi'de. Parser bile yalnız Java ve Python.
- **Respect (Redocly CLI) MIT'tir**, olgundur, adım başına OpenAPI kontrolü yapar ve
  **HAR + JSON** üretir.
- Global desen zaten "icra eden ≠ hakem"dir: Tracetest, Schemathesis, Dredd, Citrus, Microcks.

## Karar

### A. Runner dış bileşendir

> [!WARNING] Açık bulgu (2026-08-14, AUDIT-0002 / BULGU-07) — **Arazzo sürümü**
> Redocly CLI README'si **"Arazzo 1.0"** diyor; `generate-arazzo` **`arazzo: 1.0.1`** üretiyor;
> changelog `lint`'e **1.1.0 sözdizimi doğrulama** eklendiğini söylüyor. **`respect`'in bir
> 1.1 belgesini koştuğu doğrulanamadı.** ADR-0014 §C ve ARCH-0004 "Arazzo 1.1" diyor —
> bu **doğrulanmamış bir varsayımdır**. Ölçülene kadar üretim hedefi **`1.0.1`** olmalıdır;
> bugün 1.1'e ihtiyaç duyan tek şey ertelenmiş olan async adımdır.

Test Module **kendi HTTP koşum motorunu yazmaz.** Seçilen runner **Redocly Respect**'tir.

| Ölçüt | Gerekçe |
|---|---|
| Lisans | MIT |
| Dağıtım | `redocly/cli` Docker imajı, sabit sürümle pinlenir |
| Adım kontrolü | status / content-type / schema / successCriteria |
| Çıktı | `--har-output` (HAR 1.2) + `--json-output` |
| Ek | Arazzo lint'i aynı CLI'da |

Runner `IWorkflowRunnerPort` arkasındadır; adapter süreç sınırını yönetir ve
`Ptn.TestModule.EntityFrameworkCore/Adapters/` altında yaşar. **Yeni proje veya katman
açılmaz** (§F deseni korunur).

Ölçülmüş bir olay tabanlı test ihtiyacı doğarsa ikinci adapter eklenir; Arazzo 1.1
`channelPath`/`action`/`correlationId` ile async adımı zaten tanımlamıştır.

### B. Akış

```
POST /api/test-runs → Controller → AppService
   └─ test_runs satırı: Pending   (kısa UoW, commit)
   └─ IBackgroundJobManager.EnqueueAsync → 202 + runId

ExecuteTestRunJob (AsyncBackgroundJob, tenant scope)
   ├─ [UoW] Running'e idempotent claim (StartAsync → bool)
   ├─ HAZIRLIK (UoW dışı): ortam çözümü, derleme, kısa ömürlü token, sandbox reset
   ├─ İCRA    (UoW dışı, süreç sınırı): respect → HAR + JSON
   ├─ YARGI   (UoW dışı): HAR → checker'lar
   └─ [UoW] TERMİNAL YAZIM (tek atomik)
```

**Uzun süren iş HTTP isteği içinde yaşamaz; checker'ın uzak çağrısıyla DB transaction açık
tutulmaz.** Ev precedent'i `ContractCheckExecutionBackgroundJob` ve
`ApiContractCheckerTenantBackgroundJob<TArgs>`'tır; taban sınıf tenant + cancellation +
`Begin(requiresNew: true)` kabuğunu verir.

Beklenmeyen exception yutulmaz: `BusinessException.Code` korunur, bilinmeyen kod kararlı
`Technical` kategorisine indirgenir ve terminal yazım **ayrı yeni UoW'da** yapılır.
`OperationCanceledException` `Technical` değil, `Cancelled`'dır.

### C. Veritabanı doğrulaması bir Arazzo adımıdır

`x-checknexus-db` için runner'a plugin yazılmaz, fork'lanmaz. Database Checker'ın assertion
yüzeyi zaten gerçek HTTP endpoint'leridir (`POST /assertions/row|count|absent|batch`).

Yayın anında Test Module `x-checknexus-db` uzantısını gerçek bir Arazzo adımına derler.
Jenerik runner sıradan bir POST atar.

Kazançlar:

- **Zamanlama doğru** — adım, mutasyon adımından hemen sonra sırayla koşar.
- **Eventual consistency çözülür** — `timeoutMs`/`pollIntervalMs` DB Checker'ın kendi polling
  çekirdeğindedir, runner'ın retry'ında değil.
- **Bulgu ayrıntısı HAR'a düşer** — `FailedExpectations[]` response gövdesindedir.
- **Ham SQL ve secret yoktur** — `RowAssertionRequestDto` serbest SQL taşımaz.

### D. Zamanlama kuralı — hangi kontrol nerede

| Kontrol | Girdi | Yer |
|---|---|---|
| Response uygunluğu | (istek, yanıt, spec) — saf fonksiyon | **Koşum sonrası**, HAR'dan replay; birebir aynı sonuç |
| DB assertion | O andaki veritabanı durumu | **Koşum sırasında**, Arazzo adımı olarak |

DB assertion'ı HAR'dan çalıştırmak yasaktır: sonraki adımlar durumu değiştirmiş olabilir ve
"geçti" diyen test hiçbir şey doğrulamamış olur.

**Response uygunluğu HAR'daki HER adım için çalışır, yalnız kırmızılar için değil.** Bir adım
`$statusCode == 200` şartını geçmiş ama gövdesi şemaya uymuyor olabilir; yalnız kırmızılara
bakmak bunu kaçırır.

### E. Kayıt sahibi tektir

> [!WARNING] Düzeltme (2026-08-14, AUDIT-0002 / BULGU-08)
> `REDOCLY_CLI_RESPECT_SEVERITY` diye bir ortam değişkeni **yoktur**. Severity **CLI
> bayrağıyla ve JSON nesnesi olarak** verilir:
> `respect test.yaml --severity='{"STATUS_CODE_CHECK":"warn"}'`
> Ayrıca dokümantasyon **varsayılan severity'leri belirtmiyor**; dört kontrolün severity'si
> **her koşumda açıkça** set edilir. Aksi hâlde `SCHEMA_CHECK` `error` koşabilir ve Respect,
> API Contract Checker'ın kayıt sahibi olduğu hükmü kendi başına verir — bu bölümün
> engellemek istediği şeyin ta kendisi.

Respect'in kendi kontrolleri `--severity` bayrağıyla ayarlanır:

| Kontrol | Seviye | Neden |
|---|---|---|
| `STATUS_CODE_CHECK` | `error` | Akış kontrolü — yanlış durumda devam etmek anlamsız |
| `SUCCESS_CRITERIA_CHECK` | `error` | Akış kontrolü |
| `SCHEMA_CHECK` | `warn` | **Kalıcı hükmü bizim checker'ımız verir** |
| `CONTENT_TYPE_CHECK` | `warn` | Aynı |

İki hakem birden koşuyu düşürürse kayıt sahibi belirsizleşir. Respect hızlı ön kapıdır;
sistem kaydı API Contract Checker'dır.

Her bulgu hangi hakemden geldiğini taşır: `test_result_findings.source_checker_code`
∈ `{ApiContract, DatabaseComparison, Runner}`.

### F. Modül entegrasyon kuralları

**Kural: sorular için doğrudan çağrı, olgular için olay.** Paylaşılan veri üzerinden entegrasyon,
modüller arası anahtar, join ve transaction **yasaktır**.

| Kullanım | Desen |
|---|---|
| Assertion, uygunluk, teşhis | Doğrudan çağrı — `*.Application.Contracts` arayüzleri |
| Tablo/operasyon bilgisi | Doğrudan çağrı + **önbellek** (anahtar: `CanonicalHash`) |
| Checker koşusu bitti | Olay (`*RunStatusChangedEto`) |
| Checker tablosu okuma / FK / ortak transaction | **Yasak** |

**Anti-corruption layer zorunludur.** Test Module checker arayüzlerini doğrudan çağırmaz; kendi
portlarını çağırır: `IDatabaseOraclePort`, `IApiOraclePort`, `IFailureDiagnosisPort`,
`ICheckerFindingsPort`, `ISchemaKnowledgePort`, `IWorkflowRunnerPort`, `IBusinessInvariantPort`.
Adapter'lar altyapı projesinin `Adapters/` klasöründe yaşar; **yeni proje veya katman açılmaz.**

Adapter üç işi yapar: DTO çevirisi, **tek ajan sözlüğüne normalizasyon**, hata çevirisi.

**Modül dışı kimlikler** (`db_connection_id`, `spec_snapshot_id`) kimlikle referans olarak
tutulur. Doğrulama bağlama kurulurken bir kez yapılır; görüntüleme için snapshot kopyalanır.
**Snapshot karar için kullanılmaz.**

**`[IntegrationService]` kullanılmaz.** Checker yüzeyleri ADR-0008 uyarınca bilinçli olarak
public AppService'tir; MCP olmadan doğrudan HTTP ile tüketilebilmeleri zorunludur — ADR-0015 §C
(DB assertion'ın Arazzo adımı olması) bu karara dayanır.

**Dağıtık transaction, saga ve telafi mekanizması yoktur** — checker'lar yazma yapmadığı için
(ADR-0007) gerekmez.

### G. Yasaklar

- **XPath criteria yasaktır** — Respect desteklemiyor; yayın kapısında lint ile engellenir.
  `simple` / `regex` / `jsonpath` yeterlidir.
- **`--no-secrets-masking` asla açılmaz.**
- **Girdiler CLI bayrağıyla değil**, **`REDOCLY_CLI_RESPECT_INPUT` ortam değişkeniyle**
  verilir; secret process listesinde görünmez. *(AUDIT-0002 / BULGU-09: "girdi dosyası" yolu
  dokümante değildir; `--input` bayrağı vardır ama secret için kullanılmaz.)*
- **Adım seviyesinde devam yoktur.** Çökmede koşum `Broken` işaretlenir, sandbox sıfırlanır,
  senaryo baştan koşar. Yarım kalmış bir iş işleminin ortasından devam etmek doğru değildir.

## Alternatifler

- **Kendi runner'ımızı C#'ta yazmak (TM-07):** L boyutunda iş + süresiz spec-uyum bakımı.
  Silinen şey rekabet avantajı değil, MIT lisanslı hazırı olan jenerik bir HTTP motoru.
- **Runner'ı fork'layıp plugin eklemek:** §C sayesinde gereksiz; DB oracle zaten HTTP.
- **Specmatic-Arazzo:** olay tabanlı adımı destekleyen tek araç ama **ticari (Enterprise)**.
  Ölçülmüş async ihtiyacı doğduğunda ikinci adapter olarak değerlendirilir.
- **Jentic arazzo-runner (Apache-2.0, Python):** olgunluk düşük, adım başına OpenAPI kontrolü
  yok, HAR üretmiyor. Runner projesi ölürse geçiş hedefi olarak not edilir.
- **Respect kontrollerini `error` bırakmak:** iki hakem çelişince kayıt sahibi belirsizleşir.

## Sonuçlar ve riskler

PLAN-0003 etkisi: **TM-07 düşer**, TM-05 L→S küçülür, TM-09 sadeleşir, TM-12 tablodan
HAR artefaktına iner, TM-08 şekil değiştirir, TM-21 aynen kalır.

| Risk | Önlem |
|---|---|
| Node bağımlılığı | Ayrı konteyner, sabit sürüm; host imajı temiz kalır |
| Süreç sınırında iptal/timeout | `--execution-timeout` + `--max-fetch-timeout` runner'a; üstüne job seviyesinde sert kill |
| Akış yok — HAR koşum bitince yazılır | Kabul edilir; adım seviyesinde devam zaten yok |
| `Result<T>` zarfı — `successCriteria` zarfın şeklini bilmeli | `$response.body#/data/passed` ifadesi **ilk gün** sözleşme testiyle sabitlenir |
| Checker'a çağrı yetkisi | Koşum kimliğine `DatabaseCheckerPermissions.Assertions.Execute`; token kısa ömürlü |
| Runner sürümü davranışı değiştirir | `test_runs.runner_ref` her koşuda hangi runner sürümüyle koşulduğunu tutar |
| Runner projesi ölürse | `IWorkflowRunnerPort` arkasında; Arazzo standart olduğu için doküman taşınabilir |
