---
id: ADR-0016
type: decision
status: accepted
title: Kayit ve teshis veri modeli — 4 ana tablo + 5 lookup
created: 2026-08-13
updated: 2026-08-13
owners:
  - mertbyd
supersedes:
superseded_by: null
decision_refs:
  - ADR-0007
  - ADR-0014
  - ADR-0015
rule_refs:
  - RULE-0002
  - RULE-0006
---

# ADR-0016 — Kayıt ve teşhis veri modeli

> Silinen ADR-0011'in şema sahipliği ve veri modeli bölümlerini yerine geçirir. **9 ana tablo + 14 lookup →
> 4 ana tablo + 5 lookup.** Şema kaynağı: `04-Architecture/Test-Platform-Schema.dbml`.

## Bağlam

Silinen ADR-0011, 28 tablolu bir taslağı 9 ana + 14 lookup'a indirmişti. Sistem
[[03-Decisions/ADR-0014-Senaryo-Yazarlik-Modeli-Ve-Turetilebilirlik-Kapisi|ADR-0014]] ve
[[03-Decisions/ADR-0015-Kosum-Siniri-Dis-Arazzo-Runner|ADR-0015]] ile sadeleşince model de
sadeleşti: kendi koşum motorumuz olmadığı için adım kaydı tablosu gerekmiyor, ajan oturumu
ölçümü iki kolona iniyor, plan/etiket/sağlık kavramları ölçülene kadar açılmıyor.

**Ayrı tablo kuralı** korunur. Bir kavram yalnız şu üçünden **en az biri** doğruysa ayrı tablo
olur:

1. Başka yerden FK ile gösteriliyor
2. Parent'ından bağımsız sorgulanıyor (`WHERE`/`JOIN` girişi)
3. Parent'tan bağımsız tekilleştiriliyor

## Karar

### A. Şema sahipliği

İki şema: **`test_lookup`** ve **`test_run`**, artı tek tablolu **`test_catalog`**.
Eski `test_catalog` şeması korunur ama içinde tek tablo kalır.

Database Checker `lookup/connection/definition/run/comparison`, API Checker `checker`
şemalarını sahiplenmiştir; çakışma yoktur. Şema adları `TestModuleDbProperties` üzerinden
configuration'dan ezilebilir (RULE-0002).

### B. Model — 4 ana tablo + 5 lookup

```
test_lookup (5)              test_catalog (1)        test_run (3)
├ test_run_statuses          └ test_scenarios   ──►  ├ test_runs
├ test_outcome_statuses         (her satır bir       ├ test_run_results
├ test_failure_categories        sürümdür)           └ test_result_findings
├ test_trigger_kinds
└ test_scenario_states
```

**Global dayanak.** Altı bağımsız standart aynı üç seviyede yakınsıyor
(RESEARCH-0013 §2, ve JUnit/SARIF/RFC 9457/Allure/CTRF/OTel taraması):

| Standart | Çağrı bağlamı | Hüküm + sorun | Konum/ayrıntı |
|---|---|---|---|
| JUnit XML | `<testsuite>` | `<testcase>` + `<failure>`/`<error>` | — |
| SARIF 2.1.0 | `run` | `results[]` (`ruleId`, `level`) | `locations[]` |
| RFC 9457 | — | problem nesnesi | `errors[]` + `pointer` |
| Allure | sonuç başlığı | `status` + `statusDetails` | `steps[]` |
| CTRF | `tool` + `environment` | `tests[]` | — |
| OpenTelemetry | `test.suite.run.status` | `test.case.result.status` | — |

Üç kritik çıkarım:

1. **Koşu durumu ile test hükmü ayrı sözlüklerdir.** OTel bunu şart koşuyor:
   `test.suite.run.status` = `in_progress|success|failure|skipped|aborted|timed_out`,
   `test.case.result.status` = `pass|fail`. İki ayrı lookup.
2. **JUnit `<failure>` ile `<error>`'ı şemada ayırıyor** (assertion ile beklenmeyen sorun);
   Allure `failed`/`broken` der. Bizde `Failed` / `Broken`.
3. **Hiçbir standart başarılı adım kanıtı saklamıyor.** Çocuksuz `<testcase>` = geçti.

### C. Aggregate sınırları

```
Aggregate 1        Aggregate 2       Aggregate 3
TestScenario       TestRun           TestRunResult
                                     └── TestResultFinding (çocuk entity)
```

Aggregate'ler arası bağ **kimlikle** kurulur — navigation property yoktur, yalnız `Guid` ve
DB tarafında FK. Ev precedent'i: `ContractCheckRun.BaseSnapshotId` +
`HasOne<SpecSnapshot>().WithMany().HasForeignKey(...)`.

`TestResultFinding` kök **değildir**: kendi başına yaratılmaz, hep kökle birlikte yazılır.
Doğrudan sorgulanması aggregate ihlali değil, **read model sorgusudur**.

### D. Çok kiracılılık

ABP tenant filtresi **entity tipi bazında** uygulanır ve **miras alınmaz**. Çocuk entity olsa
bile doğrudan sorgulanan her tip `IMultiTenant` taşımak zorundadır.

- 4 ana tablo → `IMultiTenant` **zorunlu** (çocuk `test_result_findings` dahil)
- 5 lookup → `IMultiTenant` **yok** (global referans verisi)

Ev precedent'i: `SpecDocument` ve `SpecContent` çocuk entity oldukları halde `IMultiTenant`
taşır. Kiracı kapsamı **veritabanı katmanındadır**.

### E. Denetim alanı seçimi

Yazılıp bir daha değişmeyen kayıt `CreationAudited*`, durumu değişen kayıt `Audited*`
(`SpecContent` ↔ `ComparisonRun` precedent'i):

| Tablo | Taban sınıf | Arayüz |
|---|---|---|
| 5 lookup | `LookupEntity : Entity<Guid>` | `IPassivable` |
| `test_scenarios` | `AuditedAggregateRoot<Guid>` | `IMultiTenant` |
| `test_runs` | `AuditedAggregateRoot<Guid>` | `IMultiTenant` |
| `test_run_results` | `CreationAuditedAggregateRoot<Guid>` | `IMultiTenant` |
| `test_result_findings` | `CreationAuditedEntity<Guid>` | `IMultiTenant` |

**Taban sınıfın verdiği alan tekrar tanımlanmaz.** "Kim başlattı" sorusunun cevabı
`CreatorId`'dir; ayrı kolon açılmaz. Zamanlanmış koşuda oturum açmış kullanıcı olmadığı için
"nasıl başlatıldı" ayrı bir lookup'tır (`trigger_kind_id`).

### F. Lookup ve kod ayrımı

| Tablo | Kodlar |
|---|---|
| `test_run_statuses` | `Pending` `Running` `Completed` `Cancelled` `Aborted` `TimedOut` |
| `test_outcome_statuses` | `Passed` `Failed` `Broken` `Skipped` `Inconclusive` — **+ `breaks_build`** |
| `test_failure_categories` | `Contract` `Persistence` `Business` `Transport` `Technical` |
| `test_trigger_kinds` | `Manual` `Scheduled` `Api` `Webhook` `ContractChange` |
| `test_scenario_states` | `Draft` `PendingApproval` `Published` `Deprecated` |

`Inconclusive` = ön koşul sağlanmadı, ana yol hiç koşmadı, hiçbir şey doğrulanmadı. `Failed`
saymak yanlış alarm, `Skipped` saymak sessiz kapsam kaybıdır. SARIF `result.kind` sözlüğündeki
`notApplicable` tam olarak bu değerdir.

`breaks_build` kolonu politikayı koddan çıkarır: `if (code == "Failed")` yazılırsa `Broken`
eklendiğinde her yer bozulur.

**Enum kullanılmaz.** Ama her kod da lookup değildir:

> **Ayırt edici kural:** küme kapalı **ve** sözlüğün sahibi biz miyiz → lookup.
> Açık uçlu veya sahibi başka modül → `varchar` + `Domain.Shared` sabiti.

Bu yüzden `error_code`, `comparison_kind_code`, `source_checker_code`,
`diagnosis_*_code` **lookup değildir**. SARIF de `ruleId`'yi bilinçli olarak serbest string
tutar; kural kataloğu ayrı yerdedir. Checker sözlüğünü kendi seed migration'ımızda tutmak
ADR-0015 §F'nin modül sınırı yasağını deler.

### G. Ortam bağlaması tablo değildir

`test_environments` **açılmaz.** Mantıksal ad → adres eşlemesi ABP tenant-scoped `Setting`
olarak tutulur; koşum anında çözülür ve `test_runs` satırına snapshot olarak düşer
(`environment_key`, `spec_snapshot_id`, `db_connection_id`).

Gerekçe: v1'de az sayıda ortam olur, ABP ayar sistemi zaten kiracı kapsamlıdır ve tablo
sonradan eklenmesi kolay bir şeydir. Kiracıların kendi UI'sinden ortam tanımlaması **ölçülmüş**
bir ihtiyaç haline gelirse ayrı ADR ile açılır.

### H. Silme ve saklama

Varsayılan `Restrict`. `Cascade` yalnız `test_runs → test_run_results → test_result_findings`
zincirindedir. `test_scenarios` asla silinmez (`Restrict`), bu yüzden koşu satırında
sürüm snapshot'ı tekrarlanmaz — join her zaman çalışır.

**Bölümleme (partition) yoktur.** PostgreSQL'de bölümlenmiş tablonun birincil anahtarı
bölümleme kolonunu içermek zorundadır; bu ABP'nin tek kolonlu `Guid` anahtar sözleşmesini
kırar. Yerine zamanlanmış **parçalı silme** (10.000'lik partiler); `(tenant_id, creation_time DESC)`
indeksi bunun için. **Geçiş eşiği:** 50 milyon satır **veya** günlük silme penceresinin
10 dakikayı aşması.

**Büyük içerik nesne deposundadır.** HAR artefaktı ABP BLOB Storing'e gider, satırda yalnız
`har_blob_name` kalır. Kanıt ≤ 4 KB satır içi, üstü blob. **Sağlayıcı: S3-uyumlu**
(AWS provider + `ServiceURL`); Database provider yalnız geliştirmede kullanılır.

**`history_id` formülü:** `SHA-256(test_key ¦ environment_key ¦ kanonik girdiler)`.
MD5 **kullanılmaz** (FIPS); değişken girdiler `x-checknexus-history: exclude` ile dışlanır.

### I. Güvenli veri sınırı

OWASP Logging Cheat Sheet'e göre "ne zaman, nerede, kim, ne yaptı" tutulur; token, parola,
connection string ve hassas kişisel veri **tutulmaz**. Bu yüzden:

- Redaction **ACL adapter'ında** yapılır, manager'da değil — ham secret domain'e hiç girmez.
  Ev precedent'i `FindingValueRedactor` (None/Hashed/Masked/Full politikası).
- Manager yalnız uzunluk sınırlarını uygular; her sınır hem `Domain.Shared` sabiti hem
  EF `HasMaxLength`'tir (precedent: `ContractCheckRunConsts.MaxErrorMessageLength`).
- **Ham stack trace tabloya yazılmaz**; `error_code` + `test_runs.trace_id` yazılır, iz
  operasyonel logdadır.
- Satır sonu temizliği **log satırında** yapılır, DB kolonunda değil — çok satırlı teşhis
  değeri korunur.

### J. Unit of Work sınırları

Üç kısa UoW; checker'ın uzak çağrısıyla transaction açık tutulmaz (ADR-0015 §B):

1. **Başlangıç:** `test_runs` satırı `Pending`, commit.
2. **Claim:** `Running`, idempotent (`StartAsync → bool`); tekrar teslimde no-op.
3. **Terminal:** `test_runs` durumu + `test_run_results` + `test_result_findings` **tek atomik
   yazım**.

`UNIQUE (test_run_id, attempt)` çift yazımı sessiz ikinci satır değil, gürültülü hata yapar.
Asılı `Running` koşuları için `(run_status_id, started_at)` indeksi üzerinde süpürücü çalışır
(`RecoverStaleRunningAsync` precedent'i).

### K. Rapor read model'dir

Ayrı `reports` tablosu yoktur. `ITestRunRepository` tek sorguda (findings `Include`)
repository-native model döndürür, Mapperly DTO'ya çevirir. Liste ucu findings ve
`diagnosis_report` kolonunu **projekte etmez**.

`title` kalıcı yazılmaz; `error_code` + yerelleştirmeden üretilir — saklanan Türkçe başlık
İngilizce gösterilemez.

## Alternatifler

- **Eski 9 + 14 tablo modeli:** kendi koşum motorumuz olmadığı için adım tablosu ve plan/
  sağlık/ajan-oturumu kavramları karşılıksız kaldı.
- **Ayrı `test_scenarios` + `test_scenario_versions`:** başlık tablosu hiçbir yerden FK
  almıyor (koşu **sürüme** bağlanıyor) ve tuttuğu her alan türetilebilir. Üç kriterin üçü de
  düşüyor.
- **`comparison_kind` lookup'ı:** `Equals`/`Range`/`Exists` DB Checker'ın `matcherCode`
  sözlüğüdür, bizim değil.
- **Adımları ayrı tablo yapmak:** hiçbir standartta ilişkisel karşılığı yok; Allure ve CTRF
  gömülü dizi kullanıyor. Ayrıca adım seviyesinde devam yanılsaması yaratır.
- **Teşhis raporunu ayrı tabloya açmak:** FK almıyor, bağımsız sorgulanmıyor, checker
  sözleşmesi gereği ≤ 4 KB. Owned jsonb yeterli.
- **`diagnosis_hypothesis_code`/`confidence_code` kolonlarını şimdi açmak:** jsonb içinde
  zaten var; projeksiyon **ölçüldüğünde** eklenir, önceden değil.

## Sonuçlar ve riskler

**23 → 9 tablo.** 14 migration yüzeyi, 14 repository ve 14 test kümesi eksilir.

| Risk | Önlem |
|---|---|
| Çapraz koşum analitiği jsonb sorgusu ister | Performans **ölçüldüğünde** projeksiyon eklenir |
| Kiracılar arası sızıntı | 4 tabloda `IMultiTenant`; tenancy testi ilk gün yazılır |
| Üç hakem çelişir | `source_checker_code` her bulgunun kaynağını taşır; kayıt sahibi ADR-0015 §E'de tanımlı |
| `test_environments` ihtiyacı doğar | Ayar → tablo geçişi ek ADR ile; koşu satırındaki snapshot alanları zaten yerinde |
| Ortam kayması yanlış alarma dönüşür | `spec_fingerprint` + `db_schema_fingerprint`; CI pass→fail geçişlerinin %84'ü flaky (Google) |
