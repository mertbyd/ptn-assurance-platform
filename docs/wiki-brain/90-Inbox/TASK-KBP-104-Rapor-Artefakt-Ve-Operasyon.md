# AJAN GÖREVİ — KBP-104 · Rapor, artefakt, ihracat ve operasyon yüzeyi

Tek görev, **yedi derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

Bu görev PLAN-0003'ün **Blok 2 kuyruğunu, Blok 4'ün iki maddesini ve Blok 5'in tamamını**
kapatır: **TM-13, TM-14, TM-16, TM-25, TM-26, TM-27, TM-28, TM-29, TM-30, TM-61** — on madde.

Bugün koşum yargısını üretiyor ve veritabanına yazıyor; **dışarıya hiçbir standart formatta
çıkaramıyor** ve hiçbir operasyon politikası taşımıyor. Bu görev bittiğinde bulgu CI'ya,
kod taramasına ve rapor yüzeyine ulaşır.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-104   (KBP-107 üzerinden)
Motor   : PostgreSQL
Commit  : #KBP-104 <type>: <past-tense English description>
```

| Ön koşul | Durum |
|---|---|
| KBP-103 Dilim 1–2 commit edilmiş | ✅ `349a4d6`, `14ff49d` |
| **KBP-107 commit edilmiş, yeşil E2E koşum kanıtlanmış** | ⚠️ **doğrula.** İhracat dilimleri **gerçek** koşu verisine yaslanır; kurgu fixture'dan CTRF üretmek bu görevin amacını boşa çıkarır |
| `test_runs` / `test_run_results` / `test_result_findings` şeması | ✅ KBP-93 |
| `TestRunResult.DiagnosisReport` jsonb + `Findings` koleksiyonu | ✅ KBP-93 |
| `ITestRunAppService.GetReportAsync` read model'i | ✅ KBP-101 |
| `HarArtifactService` + `IHarArtifactStore` (BLOB Storing) | ✅ KBP-95 |
| `breaks_build` kolonu ve `BreaksBuildPolicyTests` | ✅ KBP-90 |
| `TestTriggerKindCodes.ContractChange` lookup değeri | ✅ KBP-90 — **kod var, hat yok** |

**Dosya bütçesi ≈60.** Yedi dilim, dilim başına bir commit. **En az bir migration üretilir.**

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| Blob yazan servis | `house-profile.md` → *An AppService has no private business helpers* | `src/Ptn.TestModule.Application/Services/Runs/HarArtifactService.cs` |
| Artefakt portu | `house-profile.md` → *Ports live in Domain* | `src/Ptn.TestModule.Domain/Interface/Runs/IHarArtifactStore.cs` |
| İhracat Manager'ı | `house-profile.md` → *Base classes* | `src/Ptn.TestModule.Domain/Managers/Runs/RunOutcomeResolver.cs` |
| Read model / rapor DTO'su | `contracts-mapping.md` | `Application.Contracts/Dtos/Runs/TestReportDetailDto.cs` |
| Mapperly | `house-profile.md` → *Mapper files contain declarations only* | `src/Ptn.TestModule.Application/Mappers/Runs/TestRunMapper.cs` |
| Controller | `house-profile.md` → *Architectural spine* | `src/Ptn.TestModule.HttpApi/Controllers/Runs/TestRunController.cs` |
| EF configuration + migration | `data-access.md` | `EntityFrameworkCore/Configurations/Runs/TestRunResultConfiguration.cs` |
| Background job | mevcut kalıp | `Application/BackgroundJobs/Runs/RecoverStaleRunsJob.cs` |
| Ayar / hata kodu | `house-profile.md` → *Stable string ownership* | `Domain/Settings/TestModuleSettingDefinitionProvider.cs` |

**Kanonik kararlar:** ADR-0016 (kayıt ve teşhis modeli, §H partition yasağı), ADR-0007
(checker salt-okunur), RULE-0005 (ajan hakem değildir), RULE-0006 (türetilebilirlik),
PLAN-0003 Blok 2/4/5.

---

## 2. Sabitlenen kararlar — tartışmaya açık değil

### 2.1 Ağır çıktı **tabloya girmez**

`Ctrf` / `JUnit` / `Sarif` / `Report` çıktıları **BLOB Storing**'e yazılır; satırda yalnız
`resource_link` durur (PLAN-0003 TM-13). `HarArtifactService`'in kalıbı birebir kopyalanır:
Domain portu + Application servisi + blob adı üreten Manager.

**Yeni artefakt tablosu açılmaz.** Var olan `test_run_results` satırına kolon eklenir.

### 2.2 İhracat **deterministik ve modelsizdir**

CTRF, JUnit ve SARIF üretimi tamamen `compiled_document` + `test_run_results` +
`test_result_findings` üzerinden **saf hesaptır**. Tek satır model çağrısı, tek satır tahmin
yok (RULE-0005). Aynı koşu → aynı byte.

Eşlemeler sabittir ve **kayıpsızdır**:

| İç durum | CTRF | JUnit | SARIF |
|---|---|---|---|
| `Passed` | `passed` | *(yok)* | *(yok)* |
| `Failed` | `failed` | `<failure>` | `result.level = error` |
| `Broken` | **`other`** | **`<error>`** | `result.level = error` |
| `Skipped` | `skipped` | `<skipped>` | *(yok)* |

`Failed` (hakem hayır dedi) ile `Broken` (adım hiç koşamadı) ayrımı **korunur** — PLAN-0003
TM-02'nin varlık sebebi budur.

### 2.3 TM-27 sağlık — **materialized view**, tablo değil

`scenario_health` bir **view** olarak doğar (PLAN-0003 TM-27). `history_id` + `is_dry_run` +
`attempt` üzerinden pass/fail/flaky oranı ve p95. Migration view'ı `Sql()` ile kurar.

Tabloya inmek **ayrı bir karardır** ve ölçülmüş performans sorunu ister. Bu görevde alınmaz.

### 2.4 TM-26 tetikleyici **deterministiktir**

`New` + `Breaking` bulgu → etkilenen senaryolar → `trigger_kind = ContractChange` koşusu.
ML yok, model yok, tahmin yok. Etkilenen senaryo seçimi `compiled_document` üzerinden
**parmak izi eşleşmesiyle** yapılır.

`TestTriggerKindCodes.ContractChange` **zaten var**; eksik olan yalnız hattır.

### 2.5 TM-28 karantina — **süre zorunludur**

`breaks_build = false` olan outcome'a düşen senaryo karantinadadır ve **son kullanma tarihi
olmadan karantinaya alınamaz**. Süresiz karantina gerçek hatayı gizler (PLAN-0003 TM-28).
Süre dolduğunda senaryo otomatik olarak karantinadan **çıkar**.

### 2.6 TM-16 telemetri — ayrıntı trace'te, hüküm veritabanında

OTel semantic convention adları **aynen** kullanılır: `test.case.result.status`,
`test.suite.run.status`. `trace_id` köprüsü `test_runs` satırındaki mevcut alandan gelir.

**Ham log deposu açılmaz** (ClickHouse/OpenSearch — PLAN-0003 "Kapsam dışı").

### 2.7 Migration disiplini

Bu görev şema değiştirir. **Tek toplu migration değil**, dilim başına bir migration:
`RunArtifactLinks`, `ScenarioQuarantine`, `ScenarioHealthView`, `RunSchedules`.
Her biri **tam okunur**; destructive işlem varsa çözülür.

---

## 3. Dilimler

### Dilim 1 — TM-13 artefakt deposu (≈9 dosya)

`IRunArtifactStore` portu + `RunArtifactService` + `RunArtifactNameManager`;
`test_run_results`'a `ctrf_blob_name` / `junit_blob_name` / `sarif_blob_name` kolonları;
migration `RunArtifactLinks`; `resource_link` okuma ucu `TestRunController`'da.

**Commit:** `#KBP-104 feat: created the run artifact store and its resource link surface`

---

### Dilim 2 — TM-14 CTRF + JUnit ihracatı (≈10 dosya)

`CtrfReportManager` + `JUnitReportManager` (Domain); `RunReportExportService` (Application);
`summary` sayaçları, `tests[]`, `environment`; §2.2'nin eşleme tablosu birebir.

**Commit:** `#KBP-104 feat: created the ctrf and junit report exports`

---

### Dilim 3 — TM-30 SARIF ihracatı (≈6 dosya)

`SarifReportManager`; bulgu + severity → `results[]` + `partialFingerprints`.
Parmak izi checker'ın ürettiği `finding_fingerprint`'ten gelir — yeniden hesaplanmaz.

**Commit:** `#KBP-104 feat: created the sarif export for checker findings`

---

### Dilim 4 — TM-16 telemetri (≈6 dosya)

`ActivitySource` + `Meter`; §2.6'nın iki metriği; `trace_id` köprüsü.
Yeni katman açılmaz; mevcut `TestRunExecutionManager` ve `RunOutcomeResolver` sınırında yayılır.

**Commit:** `#KBP-104 feat: created the run telemetry spans and result metrics`

---

### Dilim 5 — TM-25 Healed + TM-28 karantina (≈9 dosya)

`Healed` etiketi: onarılmış senaryonun ilk yeşil koşusu raporda işaretlenir.
Karantina: `quarantine_until` kolonu (**NOT NULL** olduğunda karantina aktif), süre dolunca
otomatik çıkış; migration `ScenarioQuarantine`.

**Commit:** `#KBP-104 feat: created the healed marker and the bounded quarantine policy`

---

### Dilim 6 — TM-27 sağlık view'ı + TM-61 kapsam raporu (≈8 dosya)

`scenario_health` materialized view'ı (`Sql()` ile, migration `ScenarioHealthView`);
kapsam raporu `compiled_document` + `spec_snapshot_id` → operasyon kapsamı, `rule_ref` →
kural kapsamı. İkisi de **tek sorgudan** okunur.

**Commit:** `#KBP-104 feat: created the scenario health view and the coverage report surface`

---

### Dilim 7 — TM-29 zamanlama + TM-26 sözleşme tetikleyicisi (≈12 dosya)

`schedule_cron`, webhook ve manuel tetikleyici aynı modelden doğar; migration `RunSchedules`.
`ContractChangeTriggerManager`: `New` + `Breaking` bulgu → parmak izi eşleşmesi → etkilenen
senaryolar → `trigger_kind = ContractChange` koşusu kuyruğa.

**Commit:** `#KBP-104 feat: created the run scheduling surface and the contract change trigger`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **Dilim 6 ve 7** ayrı bir ticket'a devredilir.
**Kesilmeyecekler: Dilim 1, 2, 3** — ihracat yüzeyi bu görevin çekirdeğidir.

---

## 5. Yasaklar

1. Ağır çıktıyı ilişkisel tabloya yazma (§2.1).
2. Yeni artefakt tablosu, `reports` tablosu veya `finding_links` tablosu açma (ADR-0016; TM-23 ertelendi).
3. İhracata **model çağrısı** veya tahmin sokma (RULE-0005, §2.2).
4. `Failed` / `Broken` ayrımını ihracatta düzleştirme (§2.2).
5. Partition açma (ADR-0016 §H).
6. `scenario_health`'i tablo olarak açma (§2.3).
7. Süresiz karantinaya izin verme (§2.5).
8. Ham log deposu (ClickHouse/OpenSearch) ekleme.
9. ML tabanlı test seçimi (PLAN-0003 "Kapsam dışı").
10. Application servisine private iş metodu veya guard koyma — `ServiceShapeTests` **kapıdır**.
11. Yeni proje, yeni katman, `Infrastructure/`, `Helpers/`, `Utils/` açma.
12. Migration'ı okumadan commit etme; tek toplu migration üretme (§2.7).
13. Kırılan testi silme, `Skip` etme, assertion zayıflatma.
14. `KBP-95..103` dallarına commit; force-push, rebase, amend.
15. Ara dilimlerde build/test atlama.

---

## 6. Kabul kriterleri

- Gerçek bir koşu **CTRF**, **JUnit** ve **SARIF** üretiyor; üçü de kendi şemasına karşı doğrulanıyor.
- Aynı koşu iki kez ihraç edildiğinde **byte-eş** çıktı veriyor (determinizm).
- `Broken` → CTRF `other` / JUnit `<error>`; `Failed` → `<failure>`. Kayıpsız.
- Ağır çıktı blob'da; satırda yalnız `resource_link`.
- `test.case.result.status` ve `test.suite.run.status` metrikleri yayılıyor; `trace_id` köprüsü çalışıyor.
- Karantina **süresiz kurulamıyor**; süre dolunca otomatik çıkıyor.
- `Healed` yalnız onarım sonrası **ilk** yeşil koşuda işaretleniyor.
- `scenario_health` **view**; pass/fail/flaky ve p95 doğru.
- Kapsam raporu *"140 operasyonun kaçına dokunuluyor"* ve *"BR-015 test edilmiyor"* sorularını
  **tek sorgudan** cevaplıyor.
- `New` + `Breaking` bulgu, etkilenen senaryolar için `ContractChange` koşusu kuyruğa veriyor.
- Her migration okundu; destructive işlem yok.
- `dotnet build Ptn.TestModule.slnx -m:1` → 0 hata.
- `dotnet test --filter "Category!=LiveInfrastructure"` → 0 başarısız.
- `dotnet test --filter "Category=LiveInfrastructure"` → KBP-103'ün kanıtı **hâlâ yeşil**.

---

## 7. Bitiş

1. §5'in 15 maddesini kendi kodunda tek tek kontrol et.
2. Yedi dilimi sırayla commit et.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → iki filtreli `dotnet test`.
4. `/backend-verify` gate'i; her commit öncesi başlık doğrulayıcısını koştur.
5. Raporda **zorunlu**: üretilen her migration'ın tam okuması; üç ihracat formatından
   birer gerçek örnek; determinizm kanıtı (iki koşunun hash'i); `scenario_health` view'ının
   gerçek SQL'i; her varsayım.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → bir kez.

---

## 8. Kapattığı borç

| Kayıt | Madde |
|---|---|
| PLAN-0003 Blok 2 kuyruğu | **TM-13**, **TM-14**, **TM-16** |
| PLAN-0003 Blok 4 | **TM-25**, **TM-26** |
| PLAN-0003 Blok 5 | **TM-27**, **TM-28**, **TM-29**, **TM-30**, **TM-61** |

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| MCP yüzeyi, ajan profilleri, kuru koşum bildirimi, Overlay yaması | **KBP-105** |
| TM-22b adım adres indeksi · TM-23 etki analizi | **Ertelendi** — ölçülmüş ihtiyaç yok (PLAN-0003) |
| Blok 8 iş bilgisi tabloları | **Ertelendi** — ADR-0014 §A, yeni ADR ister |
| UI | Ayrı iş kolu |
| LLM / model sağlayıcı seçimi | Kod tarafı bittikten sonraki karar |
