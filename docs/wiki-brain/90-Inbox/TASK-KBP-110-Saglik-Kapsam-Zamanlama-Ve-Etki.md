# AJAN GÖREVİ — KBP-110 · Senaryo sağlığı, kapsam, zamanlama ve etki

Tek görev, **altı derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

Bu görev Test Module'ün **son iş bloğudur**: PLAN-0003'ün Blok 4-5'inden geriye kalan beş
madde (TM-26, TM-27, TM-28'in süpürücüsü, TM-29, TM-61). KBP-109 modülü *ulaşılabilir* yaptı;
bu görev *kendi kendine dönen* yapar — zamanında koşan, sağlığını bilen, sözleşme değişince
uyanan bir modül.

KBP-104..109'dan farkı: **bu görev migration üretir.** İki tane. Sebebi §2.2'de.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform   (tek klasör, branch predev)
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-110   (KBP-109 merge edildikten sonra predev üzerinden)
Motor   : PostgreSQL
Commit  : #KBP-110 <type>: <past-tense English description>
```

| Ön koşul | Durum |
|---|---|
| **KBP-109 kapanmış ve merge edilmiş** | ⛔ **zorunlu** — 48 uç taban çizgisi |
| `OutwardSurfaceTests` yeşil | ⛔ bu görevin yeni uçları da o kapıdan geçer |
| Build 0 hata; non-live testler yeşil; live 2/2 | ⛔ |
| PostgreSQL erişilebilir (matview ve migration doğrulaması için) | ⛔ |
| ADR-0016 · 4 tablo + 5 lookup modeli | ✅ bu görev **yeni tablo açmaz** |

**Dosya bütçesi ≈65.** Altı dilim, dilim başına bir commit. **İki migration üretilir.**

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| Periyodik worker | `house-profile.md` → *Reuse the nearest sibling base* | **`checkers/api-contract/.../Application/BackgroundWorkers/Sources/DueSpecDocumentCheckWorker.cs`** |
| Kuyruklanan job | `architecture.md` | `Application/BackgroundJobs/Runs/PurgeExpiredRunsJob.cs` |
| Vade alanı taşıyan entity | `architecture.md` → entity veri kabuğudur | **`checkers/api-contract/.../Entities/Sources/SpecDocument.cs`** (`IsMonitored` · `CheckIntervalMinutes` · `NextCheckAt`) |
| Olay dinleme | ADR-0015 §F → *olay ile olgu dinlenir* | **`checkers/api-contract/.../Events/Runs/ContractCheckRunStatusChangedEto.cs`** + `ContractCheckRunManager` `PublishAsync` |
| Ham SQL taşıyan migration | `data-access.md` | **`EntityFrameworkCore/Migrations/20260815135617_RunFindingFingerprint.cs`** (`migrationBuilder.Sql`) |
| AppService / Controller / DTO / validator / Mapperly | KBP-109 §1'in aynısı | `Services/Runs/*` · `Controllers/Runs/*` |
| Rota / izin / kod sabiti | `house-profile.md` → *Stable string ownership* | `Domain.Shared/Constants/**` |

**Kanonik kararlar:** ADR-0016 (kayıt modeli ve tablo sayısı), ADR-0015 §F (checker iletişimi),
ADR-0007 (checker salt-okunur), RULE-0002 (şema sahipliği), RULE-0005 (ajan hakem değildir).

---

## 2. Ölçülen boşluk — 2026-08-15 kaynak taraması

### 2.1 Beş açık madde

| # | TM | Ne | Bugünkü kanıt |
|---|---|---|---|
| **1** | TM-28 | **Karantina süpürücüsü** | `ScenarioQuarantineManager` + `QuarantineUntil` var; `IsQuarantined` **okuma anında** süreye bakıyor ve `POST .../quarantine/release` elle temizliyor. **Zamanla kendiliğinden temizleyen hiçbir şey yok** — KBP-107 bunu açıkça devretti |
| **2** | TM-27 | **Senaryo sağlığı** | `history_id`, `is_dry_run`, `attempt`, `duration_ms` kolonları **duruyor**; pass/fail/flaky oranı ve p95'i hesaplayan hiçbir sorgu, view veya uç yok |
| **3** | TM-29 | **Zamanlama** | `TestTriggerKindCodes` içinde `Scheduled` ve `Webhook` **zaten tanımlı ve seed'li**; `test_scenarios`'ta hiçbir zamanlama alanı yok, vade tarayan worker yok, webhook ucu yok. Bugün koşu yalnız elle veya API ile başlıyor |
| **4** | TM-26 | **Sözleşme değişikliği tetikleyicisi** | `TestTriggerKindCodes.ContractChange` **tanımlı ve seed'li**; `test_runs.trigger_ref` kolonu **var**. Checker olayı `ContractCheckRunStatusChangedEto` `NewFindingCount` + `MaxSeverityCode` taşıyor ve **tam bu amaçla** yazılmış (dosya yorumu: *"modul disi ... adapterlerin checker internallerine baglanmadan ... bulgu agirligini tek olayda okumasi"*). **Test Module'de tek bir olay abonesi yok** |
| **5** | TM-61 | **Kapsam raporu** | `compiled_document`, `spec_snapshot_id`, `rule_ref` **duruyor**; kapsam hesabı yok. ⚠️ **Payda eksik** — §2.2'ye bak |

### 2.2 Sabitlenen kararlar

- **Yeni tablo açılmaz; iki migration üretilir.** ADR-0016'nın 4 tablo + 5 lookup modeli
  korunur. Üretilecekler: (a) `test_run` şemasında **materialized view**; (b) `test_scenarios`
  üzerinde **üç zamanlama kolonu**. İkisi de mevcut şema sahipliği içindedir (RULE-0002).
  Başka hiçbir şema değişikliği yapılmaz.
- **Sağlık materialized view'dir, tablo değil.** PLAN-0003 TM-27 aynen: *"Tablo değil view
  olarak başlar; ölçülmüş performans sorunu çıkarsa tabloya iner."* Kurallar:
  - view `migrationBuilder.Sql` ile kurulur, `Down()` `DROP MATERIALIZED VIEW` ile geri alır;
  - EF tarafında **keyless** entity + `ToView(...)`; repository salt-okunur;
  - `REFRESH MATERIALIZED VIEW CONCURRENTLY` **unique index ister** — view'a `(tenant_id,
    scenario_key)` unique index'i migration içinde kurulur, aksi hâlde refresh çalışmaz;
  - `CONCURRENTLY` **transaction içinde koşamaz** — refresh worker kendi UOW'unun **dışında**,
    ayrı komutla çalışır. Bunu atlarsan çalışma anında patlar.
  - p95 `percentile_cont(0.95)` ile view içinde hesaplanır; **uygulama tarafında satır
    toplayıp bellekte yüzdelik hesaplama yasaktır**.
  - `is_dry_run = true` satırlar sağlık hesabına **girmez** (TM-18).
- **Zamanlama cron'dur, aralık değil.** PLAN-0003'ün gerekçesi *"gece koşusu"*dur; dakika
  aralığı "her gece 02:00"ı ifade edemez. Kolonlar `test_scenarios` üzerinde:
  `schedule_cron` (nullable), `schedule_enabled` (bool), `next_run_at` (nullable, indexli).
  Vade deseni `SpecDocument.NextCheckAt` kalıbının birebir aynısıdır.
  - **Tek yeni bağımlılık:** cron ayrıştırıcı `Cronos` (MIT). Sürüm `common.props` içinde
    değişken olarak tanımlanır; csproj'a sabit sürüm yazılmaz. **Bağımlılık reddedilirse**
    yedek plan: `schedule_cron` yerine `schedule_time_utc` (günlük saat) + `schedule_days`
    (haftanın günleri maskesi) — aynı use-case, sıfır bağımlılık. Kararı uygulamadan önce sor.
  - Zamanlama **yayınlanmış** sürümün alanıdır. Yeni sürüm yayınlandığında Manager zamanlamayı
    **önceki yayınlanmış sürümden taşır**; iki sürüm aynı anda vadeli olamaz.
  - Worker tik başına **tavanlı** okur (`MaxScenariosPerTick` ayarı), tek kısa UOW açar, her
    senaryoyu kendi tenant bağlamında kuyruklar — `DueSpecDocumentCheckWorker` birebir.
- **Webhook ucu idempotenttir ve doğrulanır.** Uç `POST api/test-module/runs/webhook`
  paylaşılan sırla doğrulanır (ABP `Setting`, sır **loglanmaz ve dönülmez**), gelen
  `deliveryId` tekrarları **yeni koşu üretmez** — `trigger_ref` üzerinden idempotent reddedilir.
  Sır ayarı yoksa uç **kapalıdır** (403), açık uç bırakılmaz.
- **Sözleşme tetikleyicisi olayla başlar, çağrıyla derinleşir.** ADR-0015 §F'nin harfiyen
  uygulanışı:
  1. `ILocalEventBus` üzerinden `ContractCheckRunStatusChangedEto` dinlenir (tek host, tek
     süreç — dağıtık bus'a gerek yok);
  2. `NewFindingCount > 0` **ve** `MaxSeverityCode` breaking değilse **hiçbir şey yapılmaz**;
  3. detay checker'ın **kendi AppService'inden** çekilir (`GetFindingsAsync`, filtre
     `ChangeStateCode = New` + breaking severity) — checker tablosu **okunmaz**, FK verilmez;
  4. etkilenen senaryolar **`spec_snapshot_id == BaseSnapshotId`** olan **yayınlanmış**
     senaryolardır: eski sözleşmeye mühürlenmiş olan onlardır;
  5. her biri için `trigger_kind = ContractChange`, `trigger_ref = <checkRunId>` ile koşu
     kuyruklanır; **aynı (checkRunId, scenarioId) çifti ikinci kez koşu üretmez**.
  - Karantinadaki ve `is_dry_run` senaryolar tetiklenmez.
  - **Operasyon seviyesinde eşleme yapılmaz.** O TM-22b'dir ve PLAN-0003'te *"ölçülene kadar
    açılmaz"* der. Snapshot seviyesinde eşleme kabalık yapar; bu bilinçlidir ve raporlanır.
  - DB Checker'ın `ComparisonRunStatusChangedEto`'su da vardır; **v1'de bağlanmaz** — simetrik
    iş, ayrı ölçüm ister.
- **Kapsam raporunun paydası bu depoda yok.** ⚠️ Doğrulandı: api-contract checker'da bir
  snapshot'ın **operasyon envanterini** veren uç **yok** — `ISpecSnapshotAppService` yalnız
  `FindOperationAsync` (tek operasyon), `DescribeSchemaAsync`, `GetAuthoringResultAsync` sunuyor.
  Bu yüzden TM-61 ikiye bölünür:
  - **bu görevde:** *pay* — yayınlanmış senaryoların `compiled_document`'ından dokunulan
    operasyon kümesi ve `rule_ref` kümesi, snapshot ve kural bazında gruplanmış;
  - **bu görevde değil:** *payda* — "140 operasyonun kaçı" cevabı checker'da
    `ListOperationsAsync(snapshotId)` açılmasını bekler. Bu bir **ACC-xx checker işidir**,
    paket sınırını (RULE-0001) ve sürüm pinini ilgilendirir. **Test Module'den checker'a
    operasyon envanteri yazılmaz, checker tablosu okunmaz.** Rapor DTO'su paydayı
    `null`/`Unknown` olarak taşır ve neden bilinmediğini kodla söyler.
- **Sağlık ve kapsam uçları ağır kolon projekte etmez.** `compiled_document`, `diagnosis_report`
  ve HAR gövdesi hiçbir yanıtta yer almaz (TM-22 kuralı).
- **Model çağrısı yok.** Hiçbir dilim `IChatClient`/model sağlayıcı getirmez;
  `PackageBoundaryTests` kapısı bozulmaz (RULE-0005).

---

## 3. Dilimler

### Dilim 1 — Karantina süpürücüsü (≈5 dosya) · **ısınma**

`ExpiredQuarantineSweepWorker` (`AsyncPeriodicBackgroundWorkerBase`), tavanlı ve tenant-aware.
Süresi dolmuş `QuarantineUntil` satırlarını **mevcut** `ScenarioQuarantineManager` yoluyla
temizler; yeni karar mantığı yazılmaz. Periyot ve tavan ABP `Setting`'inden.

Bu dilim worker desenini modüle sokar; sonraki iki dilim aynı tabana biner.

**Commit:** `#KBP-110 feat: created the expired quarantine sweep worker`

---

### Dilim 2 — Senaryo sağlığı (≈12 dosya + 1 migration)

`test_run` şemasında materialized view: `scenario_key` bazında toplam/başarılı/başarısız koşu,
flaky oranı (aynı `history_id` içinde hem yeşil hem kırmızı), p95 `duration_ms`, son koşu anı.
Keyless entity + `ToView` + salt-okunur repository + `IScenarioHealthAppService` +
`GET api/test-module/scenario-health` (sayfalı, filtreli) + `.../{scenarioKey}`.
Refresh: `ScenarioHealthRefreshWorker`, `CONCURRENTLY`, §2.2'nin transaction kuralıyla.

Migration **tam okunur**; `Up`/`Down` gövdesi rapora yazılır.

**Commit:** `#KBP-110 feat: created the scenario health view and its read surface`

---

### Dilim 3 — Zamanlama ve webhook (≈16 dosya + 1 migration)

`test_scenarios` + üç kolon; `TestScenarioManager` zamanlama normalizasyonu ve sürüm taşıması;
`DueScenarioRunWorker`; `PUT api/test-module/scenarios/{id}/schedule` (+ izin kodu);
`POST api/test-module/runs/webhook` (idempotent, sırlı, §2.2).
Kuyruklanan koşu `trigger_kind = Scheduled` / `Webhook` taşır.

**Commit:** `#KBP-110 feat: created the scenario schedule and webhook trigger surface`

---

### Dilim 4 — Sözleşme değişikliği tetikleyicisi (≈10 dosya)

`ContractChangeTriggerHandler` (`ILocalEventBusHandler`) + `ContractChangeImpactManager`
(etkilenen senaryo çözümü ve idempotency kararı) + kuyruklama.
§2.2'nin beş adımı birebir; kaba eşleme raporlanır.

**Commit:** `#KBP-110 feat: created the contract change run trigger`

---

### Dilim 5 — Kapsam raporu, pay tarafı (≈9 dosya)

`IScenarioCoverageAppService` + `GET api/test-module/coverage`:
snapshot bazında dokunulan operasyon kümesi, kural bazında `rule_ref` kümesi, senaryo sayıları.
Payda `null` ve gerekçe koduyla döner (§2.2). `compiled_document` ayrıştırması **Manager**'da,
JSON okuma AppService'e sızmaz.

**Commit:** `#KBP-110 feat: created the scenario coverage report surface`

---

### Dilim 6 — Kapanış kapısı ve doğruluk kaydı (≈6 dosya)

`OutwardSurfaceTests` yeni uçlarla yeşil; worker'ların kayıtlı ve tik davranışının test edildiği
`BackgroundWorkerRegistrationTests`; `PackageBoundaryTests` hâlâ modelsiz.
`docs/wiki-brain/01-Current/Platform-Truth.md` ve PLAN-0003 durum sütunu güncellenir:
TM-26/27/28/29/61 kapandı, TM-22b/23 ve Blok 8 bilinçli açık.

**Commit:** `#KBP-110 test: created the closing surface and worker registration gates`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **Dilim 5** devredilir (paydası zaten checker'a bağlı, tek başına yarım değer).
**Kesilmeyecekler: Dilim 1, 2, 3, 4.** Bunlar modülün "kendi kendine dönmesi"ni tanımlar.

Migration'lardan biri sorun çıkarırsa **o dilim durur**, sonraki dilime geçilmez (faz kuralı).

---

## 5. Yasaklar

1. Yeni tablo açma — ADR-0016'nın 4 tablo + 5 lookup modeli bağlayıcıdır (§2.2).
2. Sağlığı tablo olarak kurma veya p95'i bellekte hesaplama (§2.2).
3. `REFRESH ... CONCURRENTLY`'yi UOW/transaction içine koyma, unique index'i atlama (§2.2).
4. Checker tablosunu okuma, checker'a FK verme, ortak transaction açma (ADR-0015 §F, ADR-0007).
5. Checker paketine kod yazma veya sürüm pinini oynatma — payda işi ACC-xx'tir (§2.2).
6. Operasyon seviyesinde etki eşlemesi kurma — o TM-22b, kapalı (§2.2).
7. Webhook ucunu sırsız açma, sırrı loglama veya yanıtta döndürme (§2.2).
8. Aynı `(checkRunId, scenarioId)` için ikinci koşu üretme (§2.2).
9. Karantinadaki veya dry-run senaryoyu otomatik tetikleme (§2.2).
10. `Cronos` dışında yeni paket getirme; csproj'a sabit sürüm yazma.
11. Application servisine private iş metodu veya guard koyma — `ServiceShapeTests` kapıdır.
12. `Domain/Managers/**` içine `Process`/`File`/`Directory` yazma (KBP-102 kuralı).
13. Yeni proje, yeni katman, `Infrastructure/`, `Helpers/`, `Utils/` açma.
14. Rota, izin, hata kodu, ayar adı için inline string yazma — `Domain.Shared` sahibidir.
15. Koşum hattına model çağrısı ekleme (RULE-0005).
16. Kırılan testi silme, `Skip` etme, assertion zayıflatma.
17. Migration'ı okumadan kabul etme; `Down()` gövdesi boş bırakma.
18. Ara dilimlerde build/test atlama; başarısız kapıdan sonra sonraki dilime geçme.

---

## 6. Kabul kriterleri

- Süresi dolmuş karantina **elle müdahale olmadan** temizleniyor; süresi dolmamış olan duruyor.
- Sağlık view'i pass/fail/flaky ve p95 döndürüyor; dry-run koşular hesaba girmiyor;
  `CONCURRENTLY` refresh gerçek PostgreSQL'de çalışıyor.
- Zamanlanmış senaryo vadesi gelince **kendiliğinden** koşuyor; `trigger_kind = Scheduled`.
- Webhook aynı `deliveryId` ile iki kez çağrıldığında **tek** koşu üretiyor; sırsız çağrı reddediliyor.
- Breaking + New bulgu üreten bir checker koşusu, eski snapshot'a mühürlü yayınlanmış
  senaryoları `trigger_kind = ContractChange` ve `trigger_ref = checkRunId` ile tetikliyor;
  aynı olay ikinci kez tetiklemiyor; karantinadaki senaryo tetiklenmiyor.
- Kapsam raporu dokunulan operasyon ve kural kümelerini döndürüyor; payda **açıkça bilinmiyor**
  olarak işaretleniyor.
- `OutwardSurfaceTests`, `ServiceShapeTests`, `ManagerReachabilityTests`, `ServiceContractTests`,
  `PackageBoundaryTests` **yeşil**.
- İki migration da tam okundu; `Down()` her ikisinde de tersini yapıyor; yıkıcı işlem yok.
- `dotnet build Ptn.TestModule.slnx -m:1` → 0 hata.
- `dotnet test --filter "Category!=LiveInfrastructure"` → 0 başarısız.
- `dotnet test --filter "Category=LiveInfrastructure"` → **hâlâ yeşil**.

**Beklenen uç sayısı: 48 → 53** (scenario-health 2, schedule 1, webhook 1, coverage 1).
Dilim 1 ve 4 uç eklemez — worker ve olay abonesidir.

---

## 7. Bitiş

1. §5'in 18 maddesini kendi kodunda tek tek kontrol et.
2. Altı dilimi sırayla commit et; migration üreten iki dilimde `Up`/`Down`'ı **tam** oku.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → iki filtreli `dotnet test`.
4. `/backend-verify` gate'i; her commit öncesi başlık doğrulayıcısını koştur.
5. Raporda **zorunlu**: iki migration'ın tam gövdesi; `Cronos` kararının sonucu; sözleşme
   tetikleyicisinin kaba eşleme etkisi (kaç senaryo tetiklendi, kaçı gereksizdi); kapsam
   raporunun eksik paydası; her varsayım.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → bir kez.

---

## 8. Kapattığı borç

| Kayıt | Madde |
|---|---|
| PLAN-0003 TM-27 | Senaryo sağlığı — materialized view |
| PLAN-0003 TM-28 | Karantinanın **kendiliğinden** bitmesi |
| PLAN-0003 TM-29 | Zamanlama, webhook, manuel tetikleyicilerin tek modelden doğması |
| PLAN-0003 TM-26 | Sözleşme değişikliği tetikleyicisi |
| PLAN-0003 TM-61 | Kapsam raporu — pay tarafı |
| KBP-104 §4 | Ertelenen Dilim 6-7 |
| KBP-107 | Karantina süpürücüsünün devredilmesi |

## 9. Bu görevde olmayan iş

| Ne | Nereye | Neden |
|---|---|---|
| Kapsam raporunun **paydası** | **ACC-xx** — api-contract checker | Snapshot operasyon envanteri ucu checker'da yok (§2.2) |
| TM-22b adım adres indeksi, TM-23 etki analizi | Ölçüldüğünde ayrı görev | PLAN-0003: *"ölçülene kadar açılmaz"* |
| DB Checker olayıyla şema değişikliği tetikleyicisi | v2 | Simetrik iş, ayrı ölçüm ister (§2.2) |
| Blok 8 — iş bilgisi tabloları | Açılmayacak | Git + MCP `Resource` (ADR-0014 §A) |
| Kademe-4 yama **uygulaması** | Açılmayacak | RULE-0005 |
| Canlı yeşil koşumla üç formatın kanıtı | İlk gerçek kullanım | KBP-107'nin açık kabul maddesi |
| UI'ın kendisi, LLM sağlayıcı seçimi | Ayrı iş kolu | — |
