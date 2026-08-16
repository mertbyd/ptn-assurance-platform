---
id: RESEARCH-0006
type: research
status: draft
title: Test Module global tarama — yasam dongusu, calisma motoru ve veritabani tasarimi
updated: 2026-08-12
decision_refs:
  - ADR-0002
  - ADR-0005
  - ADR-0007
  - ADR-0008
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0003
  - RULE-0004
---

# Test Module — global tarama, uçtan uca yaşam döngüsü ve veritabanı tasarımı

> [!WARNING] Bu belge KARARA BAĞLANDI — geçerli model ADR-0016'dadır
> §5'teki veri modeli **ADR-0016**
> §B ile **konsolide edilmiştir**. Farklar:
> **(1)** Şema adları `test_lookup` / `test_catalog` / `test_run` (burada `testlookup` vb. yazıyor).
> **(2)** 18+ tablo yerine **9 ana tablo + 14 lookup**; `run_scenarios`, `run_steps`,
> `scenario_versions`, `scenario_health`, `heal_proposals`, `finding_links` ayrı tablo değil,
> owned jsonb.
> **(3)** `run_steps` partition'ı **v1'de yok**; parçalı silme + 50M satır eşiği.
> Belge **gerekçe arşivi** olarak durur; §1–4 ve §6–16 hâlâ geçerli referanstır.

> Kanonik değildir. Bu belge [[90-Inbox/RESEARCH-0003-MCP-Senaryo-Testi-Mimarisi|RESEARCH-0003]]'ün
> devamıdır ve ondan **iki bakımdan** ayrılır:
> RESEARCH-0003 *"senaryo testi mimarisi ne olmalı"* sorusunu yanıtladı (tez düzeyi).
> Bu belge *"o mimariyi taşıyacak ürünün her aşaması ve **kalıcı veri modeli** nedir"*
> sorusunu yanıtlar (uygulama düzeyi, tablo/kolon/indeks/saklama kararlarına kadar).
>
> Kanıt sınıfları RESEARCH-0001 §0 ile aynıdır:
> **K1** = bu workspace'in çalışan kodu/migration/paket içeriği,
> **K2** = birincil spesifikasyon veya resmî ürün dokümantasyonu,
> **K3** = ikincil ölçüm/deneyim iddiası (karar gerekçesi olamaz, yalnız yön verir).

---

## 0. Tek paragrafta sonuç

Test Module'ün asıl ürünü "test koşan bir servis" değil, **iki deterministik motorun bilgisini
senaryoya bağlayan kalıcı bir kayıt sistemi**dir. Piyasadaki on iki sistem tek tek incelendiğinde
hepsinin aynı üç ayrımı yaptığı görülüyor: (1) **tanım** ile **koşum** ayrı kalıcı nesnelerdir,
(2) tanımın kimliği **içerik hash'i**yle, koşumun kimliği **zaman + ortam**la belirlenir,
(3) ağır kanıt (log, gövde, ekran görüntüsü) ilişkisel tablodan **çıkarılır**, referansla tutulur.
Bizde bu üç ayrımın ikisi zaten kodda var: `SpecContent` içerik-adresli hash ile saklanıyor ve
`ComparisonRun` snapshot + denormalize sayaç deseniyle yazılıyor (K1). Yani Test Module'ün veri
modeli sıfırdan icat edilmeyecek; **checker'ların kanıtlanmış deseni senaryo alanına taşınacak.**
Bunun üstüne piyasada olmayan tek şeyi ekliyoruz: **`scenario_step_index` + `finding_links`**,
yani "bir sözleşme değişikliği hangi senaryonun hangi adımını bozar" sorusunu tahminle değil
**indeksli join'le** cevaplayan tablo çifti.

---

## 1. Test Module nedir, ne değildir

| Sorumluluk | Sahibi |
|---|---|
| Senaryonun yazılması, sürümlenmesi, onaylanması | **Test Module** |
| Senaryonun koşulması, adım kanıtının üretilmesi | **Test Module** |
| HTTP yanıtının sözleşmeye uygunluğu | `CheckNexus.ApiContracts` (ADR-0007 API karşılığı) |
| Veritabanında beklenen satırın oluşup oluşmadığı | `CheckNexus.DatabaseComparison` (ADR-0007) |
| Başarısızlığın nedeni hakkında sıralı hipotez | İki checker'ın `IDiagnosisAppService`'i |
| Secret çözümü | `CheckNexus.Vault` (RULE-0003) |
| Kimlik ve tenant bağlamı | Authenticator (RULE-0004) |
| MCP tool yüzeyi | Composition host (ADR-0008) |
| **Test verisi seed/cleanup** | **Test Module** — checker'lar hedefe yazmaz (ADR-0007) |

**Test Module açıkça şunlar değildir:** ikinci bir karşılaştırma motoru, ikinci bir şema okuyucu,
ikinci bir identity owner, ve **koşum anında model çağıran bir ajan**.

---

## 2. Küresel cephe — aynı işi yapan on iki sistem tek tek

### 2.1 Playwright Test Agents (Microsoft, v1.56+) — **yazım/koşum ayrımının referansı**

Üç ajan: **planner** uygulamayı gezip `specs/*.md` insan-okur plan üretir; **generator** planı
`tests/*.spec.ts` çalıştırılabilir teste çevirir; **healer** kırık testi yeniden koşup locator/bekleme
onarımı önerir. Kritik ayrıntı: **`seed.spec.ts`** — ajanların bootstrap ettiği, global setup'ı çalıştıran
ve üretilen tüm testler için **stil örneği** olan tohum test. Ajanlar bağımsız binary değil, MCP tool
demeti + talimattır. (K2)

**Bizim karşılığımız:** `specs/*.md` → `scenario_versions` (Draft/PendingApproval), `tests/*.spec.ts`
→ yayımlanmış Arazzo dokümanı (`scenario_contents`), `seed.spec.ts` → **ortam bağlama profili**
(`environments` + `environment_bindings`) ve şablon senaryo.

### 2.2 Testkube — **deklaratif tanım ile koşum CRD'sinin ayrılması**

`TestWorkflow` (tanım) ile `TestWorkflowExecution` (koşum) **ayrı** kaynaklardır; controller execution
kaynağını izler, Job/Pod üretir, bittiğinde kaynakları siler; sonuç ve loglar control plane'e taşınır ve
agent yeniden başlasa bile toparlanır. Artefaktlar ayrı bir yüzeyden (CLI/REST) okunur. (K2)

**Ders:** tanım kaynağı ile koşum kaydını **aynı satırda tutma**. Bizde `scenario_versions` (tanım)
ile `run_scenarios` (koşum) ayrı tablodur ve koşum tanımın **sürüm kimliğini** taşır.

### 2.3 Allure TestOps — **kimlik üçlüsü ve launch kapanışı**

Üç ayrı kimlik katmanı: **AllureID** (açık, kalıcı kimlik), **testCaseId** = `md5(fullName + sort(params))`
(adaptör türetir; fonksiyon adı/parametre değişirse kimlik değişir ve **çift kayıt** doğar), **historyId**
(koşum bağlamı kimliği; trend/geçmiş buna bağlanır). Statüler: `passed | failed | broken | skipped | unknown`.
**Launch kapandığında** tüm sonuçlar işlenir ve test case'ler ile analitik veri **o anda** güncellenir;
retry aynı launch içinde kalır, parametre/ortam değişirse ayrışır. (K2)

**Ders 1 — bizde en kritik tasarım kararı:** kimlik **türetilmiş hash'e bırakılmaz**. Senaryonun
kalıcı `key`'i vardır (AllureID muadili); hash yalnız **içerik dedup**u içindir.
**Ders 2:** `failed` (oracle "hayır" dedi) ile `broken` (adım hiç koşamadı) ayrı statülerdir. Bu ayrım
teşhis motorunun girdisini belirler; tek "failed" statüsü ile ikisi karışır.

### 2.4 ReportPortal — **hiyerarşi + üç depolama katmanı**

Model: `Launch → TestItem (iç içe) → Log → Attachment`. İstatistik taşımayan iç adımlar
`hasStats=false` ile **nested step** sayılır (log gruplama amaçlı). Depolama üçe bölünür:
PostgreSQL (ilişkisel), MinIO/dosya sistemi (ek dosyalar), OpenSearch (log indeksleme/ML).
API attachment sınırı varsayılan 64 MB. (K2)

**Ders:** log/kanıt gövdesi ilişkisel tabloya konmaz. Bizde `run_steps` **satır**, kanıt **blob**tur
(ABP BLOB Storing). Bizim kanıt hacmimiz ReportPortal'dan çok daha küçüktür çünkü ham gövde
varsayılan olarak **hiç saklanmaz** (`ValueRetentionMode = None`, ADR-0007).

### 2.5 Kiwi TCMS — **açık kaynak, okunabilir ilişkisel model**

`TestPlan` (parent ile ağaç) → `TestCase` (geçmiş kaydı `KiwiHistoricalRecords`) → `TestRun`
(`plan`, `build`, `manager`, `default_tester`, `start_date`/`stop_date`/`planned_start`/`planned_stop`,
hepsi indeksli) → `TestExecution` (`run`, `case`, `build`, `status` FK, `assignee`, `tested_by`,
`sortkey`, **`case_text_version`**). Statü ayrı tablo: `TestExecutionStatus(name unique, weight, icon, color)`
— `weight` ile "tamamlanmış" ve "başarısız" ayrımı yapılır. Parametreler `TestExecutionProperty` /
`Property` name-value tablolarında. (K2)

**Ders 1 — doğrudan kopyalanacak:** `case_text_version`. Koşum, **o an geçerli tanım sürümünü**
satırında taşır; tanım sonradan değişse bile tarih doğru kalır. Bizde bu `run_scenarios.scenario_version_id`
olur ve zaten `ComparisonRun`'ın "snapshot connection id" deseninin aynısıdır (K1).
**Ders 2:** statü **lookup tablosu**dur, enum kolonu değil — checker'larımızdaki `ComparisonRunStatus`
lookup deseniyle birebir aynı (K1).

### 2.6 TestRail / Xray / Zephyr — **snapshot-on-run ve depolama sınırı**

TestRail: case'ler kendi veritabanında; **test run, case'leri o sürüm için snapshot alır**; milestone
run'ları gruplar. Xray: her test/precondition/test set/execution **Jira iş kaydıdır** — esneklik verir
ama ölçekte performans bedeli ödetir. Zephyr: kendi optimize deposu, Jira'yı şişirmez. (K2/K3)

**Ders:** "her şeyi ana iş takip sistemine yaz" yaklaşımı ölçekte cezalandırıyor. Test Module kendi
şemalarının sahibidir (ARCH-0003 modeli); Authenticator/Notifications tablolarına yazmaz.

### 2.7 Pact Broker — **bizim moment D'mizin kanıtlanmış atası**

`pacticipant` (uygulama) ↔ `pacticipant version` (dağıtılabilir sürüm) ↔ **pact içeriği**.
İçerik **hash'lenerek dedup** edilir: aynı sözleşme birden çok sürüme aitse doğrulama tekrar
kullanılır. Doğrulama sonuçları provider sürümüyle sözleşmeyi bağlar; **matrix** tüm consumer/provider
sürüm çiftlerinin doğrulama durumunu tutar ve `can-i-deploy` kararını bu matrisi okuyarak verir.
Webhook'lar "sözleşme değişti" ve "doğrulama yayımlandı" olaylarında tetiklenir — sağlayıcı
kırıcı değişiklik yaptığında **etkilenen consumer'lar görünür**. (K2)

**Ders — bu belgenin omurgası:** *sözleşme kimliği ile uygulama sürüm kimliği ayrılır.*
Bizde bunun yarısı zaten kodda: `SpecContent.RawHash` + `SpecContent.CanonicalHash`, tenant içinde
dedup anahtarı (K1). Test Module aynı ayrımı senaryoya uygular: `scenario_contents` (hash'li, değişmez)
≠ `scenario_versions` (yayın kaydı) ≠ `run_scenarios` (koşum). Pact'in "matrix"i bizde
**`finding_links`** tablosudur: bulgu parmak izi ↔ etkilenen senaryo adımı.

### 2.8 Datadog Test Optimization — **flaky yaşam döngüsü bir durum makinesidir**

Durumlar: **Active** (bilinen flaky, koşuyor), **Quarantined** (koşar ama başarısızlığı CI'ı kırmaz;
`is_quarantined:true`), **Disabled** (hiç koşmaz; `is_disabled:true`), **Fixed** (kararlı geçti,
remediation akışıyla doğrulandı). İzlenen öznitelikler: 7 günlük **failure rate**, etkilenen **pipeline
failure** sayısı, **boşa giden CI süresi**, ilk/son görülme, %100 başarısızlıkta "broken" işareti.
Geçişler politika ile otomatikleşir (süre, eşik, dal bazlı). (K2)

**Ders:** flaky yönetimi rapor değil **kalıcı durum**dur. Bizde `scenario_health` tablosu +
`health_states` lookup'ı bunun karşılığıdır; karantina kararı koşum sonucunu **değil**, koşumun
CI'a etkisini değiştirir.

### 2.9 Test Impact Analysis / Predictive Test Selection — **biz neden deterministik kalıyoruz**

Azure DevOps TIA çağrı grafiği + kod kapsamı haritasıyla **deterministik** seçim yapar; Launchable
tarzı yaklaşım geçmiş koşulardan öğrenen bir ML modeliyle **olasılıksal** seçim yapar. (K2/K3)

**Ders:** bizim seçim problemimiz "hangi test büyük ihtimalle kırılır" değil, **"hangi senaryo adımı
bu sözleşme farkına bağlı"**dır — ve bu, ML gerektirmeyen bir **join**'dir. İki checker parmak izini
zaten üretiyor (`FindingFingerprintCalculator` her iki tarafta da kaynakta mevcut, K1). ML'e
girmemek bilinçli bir karardır: açıklanabilirlik ve sıfır eğitim maliyeti.

### 2.10 Schemathesis — **senaryo üretiminde bağımlılık keşfi**

OpenAPI **links**, `Location` header'ları ve şema analiziyle "üretici" ve "tüketici" operasyonları
eşleyip durum makinesi kurar; oluştur → oku → sil zincirlerini gezer, "silinen kaynak hâlâ okunuyor"
gibi hataları yakalar. (K2)

**Ders:** yazım anındaki `contract.operation.find` + `db.binding.suggest` tool'larının öneri
kalitesi, spec'teki link/ilişki bilgisinden gelir. Bu, api-contract checker'ın
`SuggestOperationBindingsAsync` yüzeyinin (K1, kaynakta mevcut) neden doğru yerde durduğunu gösterir.

### 2.11 Arazzo 1.1.0 + Overlay 1.0/1.1 — **senaryo formatı ve yama formatı**

Arazzo 1.1.0 senaryo tarafını çözüyor (workflow/step/inputs/outputs/successCriteria/retry/timeout,
`dependsOn` + `correlationId` ile asenkron, AsyncAPI kaynakları, runtime expression için ABNF gramer
ve açık truthy/falsy semantiği). RESEARCH-0003 §5.2 bunu zaten seçti.

**Bu belgenin yeni katkısı: Overlay Specification.** Overlay, hedef dokümana uygulanacak **sıralı
Action listesi**dir; her action bir **JSONPath `target`** ve bir **`update`** (merge) veya **`remove: true`**
taşır. (K2)

**Ders — healer yaması artık kendi formatımız olmayacak:** `heal_proposals.patch_document` bir
**Overlay dokümanı** olarak saklanır. Kazanç üç kat: (1) model formatı zaten biliyor, anlatım tokeni
sıfır; (2) yama **incelenebilir** — hangi adımın hangi alanı değişiyor JSONPath ile görünür;
(3) uygulanmadan önce hedef sürüme karşı **kuru çalıştırılabilir**, yani "sessiz onarım" yasağını
(RESEARCH-0003 §5.4) mekanik olarak uygulayabiliriz.

### 2.12 CTRF + OpenTelemetry `test.*` + SARIF — **dışa aktarım üçlüsü**

**CTRF** (K2): `results.tool`, `results.summary` (`tests/passed/failed/skipped/pending/other/start/stop`,
opsiyonel `flaky`, `suites`, `duration`), `results.tests[]` (zorunlu: `name`, `status`, `duration`;
opsiyonel: `id`, `testId`, `executionId`, `suite[]`, `message`, `trace`, `snippet`, `line`, `filePath`,
`tags[]`, `labels{}`, `retries`, `flaky`, `stdout[]`, `stderr[]`, `steps[]`, `attachments[]`,
`retryAttempts[]`, `insights{passRate,failRate,flakyRate,averageTestDuration,p95TestDuration,executedInRuns}`),
`results.environment` (build/commit/branch/testEnvironment...), üst düzey `insights` ve **`baseline`**
(`reportId`, `commit`, `buildNumber`...). Statü enum'u: `passed | failed | skipped | pending | other`.

**OpenTelemetry semconv** (K2, Development seviyesinde): `test.case.name`,
`test.case.result.status` (`pass | fail`), `test.suite.name`,
`test.suite.run.status` (`success | failure | skipped | aborted | timed_out | in_progress`).
CI/CD span'lerinde `cicd.pipeline.*` ve `cicd.pipeline.task.run.result` mevcuttur.

**SARIF 2.1.0** (OASIS, K2): bulgu taşıma formatı; `fingerprints`/`partialFingerprints` ile bulgu
kimliği ve baseline/suppression kavramları. Checker bulgularının CI'a taşınması için doğru format.

**Ders:** üç formatın **üçü de** gerekir ve üçü de **türetilmiş çıktı**dır — hiçbiri kalıcı model
değildir. Kalıcı model tek: bizim tablolarımız. Statü sözlüğümüz beş değerli olmalı ki CTRF ve
Allure'a kayıpsız eşlensin (`broken → other`).

### 2.13 Temporal — **aldığımız fikir, almadığımız bağımlılık**

Temporal iş akışını **deterministik ve replay edilebilir** workflow ile **yeniden denenebilir** activity
olarak ikiye ayırır; geçmiş kaydını yeniden oynatarak çökme sonrası durumu kurtarır; replay testi
determinizm ihlalini yakalar. (K2/K3)

**Ders — fikri alıyoruz, altyapıyı almıyoruz:** "adım tamamlandığında kalıcı checkpoint yaz, çökme
sonrası oradan devam et" bizde `run_steps` satırının kendisidir. Ayrı bir orkestrasyon kümesi
işletmek, ABP'nin arka plan iş + outbox altyapısı varken **ikinci bir durum sahibi** yaratır ve
RULE-0001 sınırını zorlar. §7'de karar gerekçesiyle birlikte.

---

## 3. Uçtan uca yaşam döngüsü — dokuz faz

```text
[1] YAZIM        insan + ajan  -> Draft scenario_version (Arazzo)          moment A, yuksek token
[2] DOGRULAMA    scenario.validate + dryRun (canli)                        moment A
[3] ONAY         PendingApproval -> Published (insan)                      token yok
[4] YAYIN        content hash + version no + step index turetimi           token yok
[5] TETIK        manuel | zamanlanmis | sozlesme degisikligi | webhook     token yok
[6] KOSUM        runner: HTTP adimi + oracle cagrilari, MODEL YOK          moment B, SIFIR token
[7] KANIT        adim sonucu + redaction'li kanit + blob artefakt          token yok
[8] TESHIS       yalniz kirmizi kosuda, iki checker'in diagnosis yuzeyi    moment C, dusuk token
[9] BAKIM        bulgu -> etkilenen adim -> Overlay yama -> onay           moment D, dusuk token
```

Her fazın kalıcı izi vardır; hiçbir faz "bellekte" geçmez. Bu, RESEARCH-0003 §4'teki dört anın
(A yazım / B koşum / C teşhis / D bakım) tabloya oturmuş halidir.

---

## 4. Pazarlıksız değişmezler

| # | Değişmez | Gerekçe |
|---|---|---|
| I-01 | Runner hiçbir koşulda modele başvurmaz; karar veremeyen adım **başarısızdır** | Determinizm + B anı sıfır token (RESEARCH-0003 §5.1) |
| I-02 | Oracle deterministiktir; model yalnız **öneri** üretir, hakem değildir | LLM oracle kırılganlığı (RESEARCH-0003 §5.3) |
| I-03 | Checker hedef veritabanına **yazmaz**; seed/cleanup Test Module'ün `ITestDataSandbox`'ıdır | ADR-0007 |
| I-04 | Koşum kaydı, tanımın **sürüm kimliğini** taşır; tanım değişse tarih bozulmaz | Kiwi `case_text_version`, `ComparisonRun` snapshot deseni (K1/K2) |
| I-05 | Ham gövde/hücre varsayılan olarak **saklanmaz** (`ValueRetentionMode = None`) | ADR-0007 + prompt injection yüzeyini kesmek |
| I-06 | Yama gerekçesizse **uygulanmaz**; her yama bir bulgu parmak iziyle ilişkilidir | Sessiz self-heal riski (RESEARCH-0003 §5.4) |
| I-07 | Onarılmış senaryonun ilk yeşil koşusu raporda `Healed` etiketi taşır | Aynı |
| I-08 | Senaryo kimliği hash'ten **türetilmez**; kalıcı `key` taşır | Allure `testCaseId` çift-kayıt tuzağı (K2) |
| I-09 | Secret senaryoda **yer almaz**; yalnız mantıksal `secretRef` durur | RULE-0003 |
| I-10 | Test Module yalnız kendi şemalarının migration sahibidir | RULE-0002 / ARCH-0003 |

---

## 5. Veritabanı tasarımı

### 5.1 Modelleme ilkeleri (hepsi K1 precedent'inden)

| İlke | Kaynak precedent |
|---|---|
| `AuditedAggregateRoot<Guid>` + `IMultiTenant`; `TenantId` korumalı setter | `ComparisonRun` |
| Değişmez içerik ayrı entity, **SHA-256 hex** ile adreslenir, tenant içinde dedup | `SpecContent` |
| Yaşam döngüsü durumu **lookup tablosu** + `Domain.Shared` kod sabiti | `ComparisonRunStatus` + `ComparisonRunStatusCodes` |
| Ağır alan owned JSON kolon; liste sorgusu bu kolonu **projekte etmez** | `ComparisonRun.Findings` / `.Reports` |
| Özet sayaç bilinçli denormalize edilir, run biterken bir kez yazılır | `SchemaDifferenceCount` vb. |
| Tablo adları `snake_case`, `DbTablePrefix` boş ve configuration'dan gelir | `DatabaseCheckerTableNames`, `*DbProperties` |
| Entity veri kabuğudur; kural Manager'dadır | Repo AGENTS sözleşmesi |

### 5.2 Şema sahipliği — çakışmayan üç şema

ARCH-0003 gereği aynı tabloyu iki modül oluşturamaz ve DB checker `lookup`, `connection`,
`definition`, `run`, `comparison` şemalarını **zaten sahiplenmiştir**. Bu yüzden Test Module
o adları kullanamaz. Önerilen:

| Şema | İçerik | Hacim sınıfı |
|---|---|---|
| `testlookup` | Tüm lookup tabloları | Çok küçük, seed |
| `testcatalog` | Senaryo, içerik, sürüm, adım indeksi, ortam, plan | Küçük–orta, uzun ömürlü |
| `testrun` | Koşu, koşum, adım, artefakt | **Büyük, kısa ömürlü, partition'lı** |

Bakım/analitik tabloları (`finding_links`, `heal_proposals`, `scenario_health`) `testcatalog`
içinde yaşar; senaryo yaşam döngüsüne aittirler, koşum hacmine değil.

### 5.3 `testlookup` — lookup tabloları

Hepsi checker'daki `LookupEntity` desenini izler (`Code` unique, `Name`, `DisplayOrder`, `IsActive`)
ve `Domain.Shared`'da kod sabiti sınıfı vardır.

| Tablo | Kodlar |
|---|---|
| `test_run_statuses` | `Pending`, `Running`, `Completed`, `Failed`, `Cancelled` |
| `test_execution_statuses` | `Passed`, `Failed`, `Broken`, `Skipped`, `Quarantined` |
| `test_step_kinds` | `HttpCall`, `DbAssert`, `EventReceive`, `Delay`, `Setup`, `Teardown` |
| `test_oracle_layers` | `Transport`, `Contract`, `Domain`, `Persistence`, `Async`, `Security` |
| `test_trigger_kinds` | `Manual`, `Scheduled`, `ContractChange`, `Webhook` |
| `test_scenario_states` | `Draft`, `PendingApproval`, `Published`, `Deprecated` |
| `test_health_states` | `Healthy`, `Flaky`, `Quarantined`, `Disabled`, `Broken` |
| `test_heal_statuses` | `Pending`, `Approved`, `Rejected`, `Superseded` |

`Failed` / `Broken` ayrımı §2.3'ten gelir ve teşhis girdisini belirler: `Failed` → oracle "hayır" dedi
(assertion yüzeyine gidilir), `Broken` → adım hiç koşamadı (transport/bağlantı; diagnosis yüzeyine gidilir).
İnce taneli sonuç kodları (`AssertionOutcomeCodes` gibi) lookup **satırı değil**, `Domain.Shared`
sabiti + `varchar` kolondur; checker'lar da böyle yapar (K1).

### 5.4 `testcatalog` — tanım dünyası

```text
scenarios                       mantiksal kimlik, kalici key (I-08)
  id                PK  uuid
  scenario_key      varchar(128)     UNIQUE (tenant_id, scenario_key)
  title             varchar(256)
  description       text?
  owner_user_id     uuid?
  state_id          FK -> test_scenario_states
  current_version_id FK -> scenario_versions (nullable, yayimlanan surum)
  tags              jsonb            GIN index
  tenant_id         uuid?
  + Audited alanlar

scenario_contents               DEGISMEZ, icerik-adresli (SpecContent deseni)
  id                PK  uuid
  raw_hash          char(64)         UNIQUE (tenant_id, raw_hash)
  canonical_hash    char(64)         index  (anlamsal esitlik)
  content           text             Arazzo 1.1.0 dokumani
  byte_size         int
  media_type        varchar(128)
  tenant_id         uuid?
  + CreationAudited

scenario_versions               yayin kaydi
  id                PK  uuid
  scenario_id       FK -> scenarios
  content_id        FK -> scenario_contents
  version_number    int              UNIQUE (scenario_id, version_number)
  state_id          FK -> test_scenario_states
  approved_by       uuid?            approved_at timestamptz?
  healed_from_version_id  FK?        heal_proposal_id FK?   (I-06/I-07 izi)
  notes             text?
  + Audited

scenario_step_index             TURETILMIS; yayinda yeniden uretilebilir  << moment D'nin anahtari
  id                PK  uuid
  scenario_version_id FK -> scenario_versions   (ON DELETE CASCADE)
  step_id           varchar(128)     Arazzo stepId
  ordinal           int
  kind_id           FK -> test_step_kinds
  operation_fingerprint  char(64)?   API operasyon kimligi (api-contract ile ayni gramer)
  db_target_fingerprint  char(64)?   schema.table[.column] kimligi (db-checker ile ayni gramer)
  assertion_count   int
  UNIQUE (scenario_version_id, step_id)
  INDEX (operation_fingerprint)   INDEX (db_target_fingerprint)

environments
  id, environment_key UNIQUE(tenant), name, is_active, tenant_id

environment_bindings            senaryo ortamdan bagimsiz kalir (RESEARCH-0003 S-05)
  id                PK
  environment_id    FK -> environments
  binding_kind      varchar(32)      Http | Database | Event
  logical_ref       varchar(128)     ornek: "ordersApi", "sales-db"
  api_base_url      varchar(512)?
  spec_source_id    uuid?            api-contract SpecSource kimligi
  db_connection_id  uuid?            db-checker DatabaseConnection kimligi
  secret_ref        varchar(256)?    Vault yolu; deger DEGIL (I-09)
  UNIQUE (environment_id, logical_ref)

test_plans
  id, plan_key UNIQUE(tenant), name, environment_id FK,
  selection jsonb,            senaryo id listesi | etiket sorgusu
  schedule_cron varchar(64)?, trigger_flags jsonb,
  max_parallelism int, is_active bool, tenant_id, + Audited

plan_scenarios                  acik liste kullanildiginda
  plan_id FK, scenario_id FK, ordinal int,
  pinned_version_id FK?         null = en son Published
  UNIQUE (plan_id, scenario_id)
```

**Neden `scenario_step_index` ayrı bir tablo?** Çünkü moment D'nin sorusu şudur:
*"`operation_fingerprint = X` olan bulgu geldi; hangi senaryonun hangi adımı etkilenir?"*
Bu soru senaryo dokümanını JSON içinde tarayarak değil, **iki indeksli kolonla** cevaplanır.
Tablo türetilmiştir: bozulursa yayımlanmış içerikten yeniden üretilir, kanonik bilgi taşımaz.

### 5.5 `testrun` — koşum dünyası

```text
test_runs                       ComparisonRun deseninin birebir kardesi
  id                PK  uuid
  plan_id           FK?          null = ad-hoc kosum
  environment_id    FK           (snapshot: o an kullanilan ortam)
  status_id         FK -> test_run_statuses
  trigger_kind_id   FK -> test_trigger_kinds
  trigger_ref       varchar(256)?  bulgu parmak izi | kullanici | webhook kimligi
  correlation_id    uuid           MCP task / CI kosusu ile eslesme
  started_at, completed_at  timestamptz?
  total_count, passed_count, failed_count, broken_count,
  skipped_count, quarantined_count, healed_count   int   << bilincli denormalizasyon
  duration_ms       int?
  cancellation_requested bool     cancelled_at timestamptz?
  error_message     text?
  tenant_id, + Audited
  INDEX (tenant_id, creation_time DESC)   INDEX (status_id)  INDEX (correlation_id)

run_scenarios                   bir senaryonun tek kosumu (Allure "test result")
  id                PK  uuid
  run_id            FK -> test_runs (CASCADE)
  scenario_id       FK -> scenarios
  scenario_version_id FK -> scenario_versions     << I-04
  attempt           int                            retry ayni run icinde kalir
  history_id        char(64)   hash(scenario_key + environment_key + input params)  << Allure historyId
  status_id         FK -> test_execution_statuses
  started_at, completed_at, duration_ms
  failed_step_id    varchar(128)?
  was_healed        bool        was_quarantined bool
  tenant_id
  UNIQUE (run_id, scenario_id, attempt)
  INDEX (scenario_id, creation_time DESC)   INDEX (history_id, creation_time DESC)

run_steps                       EN BUYUK TABLO — partition adayi
  id                PK  uuid
  run_scenario_id   FK -> run_scenarios (CASCADE)
  step_id           varchar(128)      ordinal int
  kind_id           FK -> test_step_kinds
  oracle_layer_id   FK -> test_oracle_layers
  outcome_code      varchar(64)       Domain.Shared sabiti (AssertionOutcomeCodes muadili)
  http_status       smallint?
  observed_at_ms    int?              RESEARCH-0003 S-03: kac ms sonra gerceklesti
  duration_ms       int               retry_count int
  redaction_mode    varchar(16)       varsayilan None (I-05)
  evidence_inline   jsonb?            <= 4 KB sinirli ozet
  evidence_blob_id  uuid?             asildiginda ABP BLOB Storing referansi
  created_at        timestamptz       PARTITION KEY (aylik RANGE)
  INDEX (run_scenario_id, ordinal)    INDEX (outcome_code) WHERE outcome_code <> 'Passed'

run_artifacts
  id, run_id FK, kind varchar(16)  Ctrf | JUnit | Sarif | Report
  blob_name varchar(256), byte_size int, content_hash char(64), expires_at timestamptz?
```

### 5.6 `testcatalog` — bakım ve sağlık dünyası

```text
finding_links                   << piyasada olmayan tablo: Pact "matrix"in bizim karsiligimiz
  id                PK
  finding_fingerprint char(64)          checker'in urettigi parmak izi
  source_capability varchar(32)         ApiContract | DatabaseComparison
  severity_code     varchar(32)         DifferenceSeverityCodes
  scenario_version_id FK
  step_id           varchar(128)
  impact_state      varchar(16)         New | Acknowledged | Resolved
  first_seen_run_id uuid?               last_seen_at timestamptz
  UNIQUE (finding_fingerprint, scenario_version_id, step_id)
  INDEX (finding_fingerprint)  INDEX (impact_state) WHERE impact_state = 'New'

heal_proposals                  yama = Overlay dokumani (§2.11)
  id                PK
  scenario_id FK, base_version_id FK
  finding_fingerprint char(64)          NOT NULL  << I-06: gerekcesiz yama yok
  patch_document    jsonb               Overlay 1.x actions[]
  rationale         text                insan-okur gerekce
  status_id         FK -> test_heal_statuses
  proposed_by_agent varchar(64), proposed_token_cost int?
  reviewed_by uuid?, reviewed_at timestamptz?
  applied_version_id FK?
  INDEX (status_id) WHERE status_id = Pending

scenario_health                 Datadog durum makinesinin kalici hali (§2.8)
  id                PK
  scenario_id FK, environment_id FK      UNIQUE (scenario_id, environment_id)
  window_days       smallint             runs_analyzed int
  pass_rate, fail_rate, flaky_rate       numeric(5,4)
  avg_duration_ms int, p95_duration_ms int
  health_state_id   FK -> test_health_states
  quarantined_until timestamptz?
  first_flaky_at, last_state_change_at   timestamptz?
```

### 5.7 Hacim, saklama ve ağır veri stratejisi

| Karar | İçerik |
|---|---|
| **Partition** | `run_steps` (ve gerekirse `run_scenarios`) `created_at` üzerinde **aylık RANGE**; eski partition **DROP** edilir. Bu, satır silmekten kat kat ucuzdur ve retention politikasını tek komuta indirir. (K2/K3) |
| **Saklama varsayılanı** | `run_steps` + kanıt **90 gün**; `run_scenarios` **1 yıl**; `test_runs` + sayaçlar **süresiz**; `scenario_health` yuvarlanan pencere. Hepsi ABP setting'i olarak `Domain.Shared`'da tanımlanır. |
| **Satır içi vs blob** | Kanıt ≤ 4 KB ise `evidence_inline jsonb`; üstü **ABP BLOB Storing** (Database / FileSystem / S3-uyumlu / MinIO sağlayıcıları mevcut, K2). PostgreSQL büyük `jsonb` değerlerini zaten TOAST'a taşıyıp sıkıştırır; sınırı aşan gövdeyi tabloda tutmanın tek etkisi liste sorgularını yavaşlatmaktır. |
| **Kolonlar ne saklamaz** | Ham yanıt gövdesi, ham DB hücresi, secret, model prompt/yanıt metni. Model tarafında yalnız **token sayacı** ve **gerekçe metni** saklanır. (I-05, I-09) |
| **Kolon depoları** | ClickHouse/OpenSearch **v1'de gerekmez**. ReportPortal'ın OpenSearch ihtiyacı ham log indekslemekten doğuyor; biz ham log saklamıyoruz. Gerekirse artefakt dışa aktarımıyla sonradan eklenir; kalıcı model değişmez. |
| **EF Core 10 imkânı** | Complex type → JSON kolon eşlemesi ve JSON path sorgusu artık birinci sınıf; `evidence_inline` ve `patch_document` için owned/complex tip kullanımı `ComparisonRun.Findings` desenini bozmadan mümkündür. (K2) |

### 5.8 Migration ve tablo sahipliği

- Test Module kendi migration assembly'sini taşır; checker migration'larına dokunmaz (RULE-0002).
- `TestModuleDbProperties.DbTablePrefix` boş varsayılan + configuration'dan okunur (checker deseni, K1).
- Migration history tablo adı çakışmaz; consumer tek migrator ile deterministik sırada uygular (ARCH-0003).
- Lookup satırları `DataSeedContributor` ile gelir (checker deseni, K1).

---

## 6. Katman yerleşimi (ABP, RULE-0001 + repo sözleşmesi)

| Katman | Bileşen | Sorumluluk |
|---|---|---|
| HttpApi | `ScenarioController`, `TestRunController`, `HealProposalController`, `ExportController` | Route, binding, authorization, XML doc, **tek** AppService çağrısı |
| Application | `ScenarioAppService`, `ScenarioVersionAppService`, `EnvironmentAppService`, `TestPlanAppService`, `TestRunAppService`, `RunEvidenceAppService`, `HealProposalAppService`, `ScenarioHealthAppService`, `TestExportAppService` | Düz orkestrasyon |
| Domain (Manager) | `ScenarioAuthoringManager`, `ScenarioContentManager` (hash + dedup), `ScenarioIndexManager` (adım indeksi türetimi), `RunOrchestrationManager`, `HttpStepExecutionManager`, `OracleDispatchManager`, `EvidenceRetentionManager`, `ImpactAnalysisManager`, `HealProposalManager`, `HealthScoringManager` | Normalizasyon, doğrulama, durum geçişi, mutasyon |
| Domain (Port) | `ITestDataSandbox`, `IScenarioDocumentReader` (Arazzo), `IOverlayPatchApplier`, `ISecretProvider` | Dış dünya sözleşmeleri |
| EF Core | Aggregate başına repository + sayfalı `run_steps` repository'si | Tüm EF/LINQ/SQL |
| Composition host | MCP tool yüzeyi (ADR-0008) | Tool kataloğu, izin, tenant politikası |

`ITestDataSandbox` **ayrı ve açıkça yetkilendirilmiş** bir bağlantı kullanır; checker o bağlantıyı
hiç görmez (ADR-0007 + RESEARCH-0003 §7.3). Piyasa deseni burada nettir: konteyner başına tek
başlatma + test başına hızlı reset (Respawn tarzı) veya transaction rollback; rollback, SUT kendi
bağlantısını açtığında veya arka plan işçisi commit ettiğinde çalışmaz (K3) — bizim senaryolarımız
tam olarak bu sınıfta olduğundan **varsayılan reset stratejisi** seçilmelidir.

---

## 7. Koşum motoru — durable execution kararı

**İhtiyaç:** uzun süren koşu, çökme sonrası devam, adım seviyesinde yeniden deneme, iptal, paralellik.

**Seçenekler:**

| Seçenek | Artı | Eksi | Karar |
|---|---|---|---|
| Temporal | Kanıtlanmış determinist replay, saga, uzun ömürlü workflow (K2) | Ayrı küme, ikinci durum sahibi, RULE-0001 sınırını zorlar, ABP tenant/izin bağlamı dışında kalır | **v1'de hayır** |
| ABP `BackgroundJobs` + adım checkpoint'i + distributed lock + transactional outbox | ABP 10'da outbox/inbox ve distributed lock hazır (K2); tenant/izin bağlamı korunur; durum tek yerde | Replay garantisi bizim disiplinimize kalır | **Evet** |
| Sadece HTTP isteği içinde koşmak | Basit | Uzun koşu, iptal ve çökme kurtarma yok | Hayır |

**Uygulama disiplini:** her adım bittiğinde `run_steps` satırı yazılır (checkpoint budur);
yeniden başlatma son yazılan `ordinal`'dan devam eder; iptal `cancellation_requested` bayrağı ile
kooperatiftir (MCP `tasks/cancel` semantiğiyle aynı, §10); adım zaman aşımı ve retry **senaryo
dokümanındaki** `timeout`/`retryLimit`/`retryAfter` alanlarından okunur, runner kendi politikasını
uydurmaz.

---

## 8. Oracle çağrı haritası — kodda **bugün var olan** imzalarla (K1)

| Adım tipi | Oracle katmanı | Çağrılan yüzey | Not |
|---|---|---|---|
| `HttpCall` | Transport | Runner'ın kendisi | Durum kodu, header, content-type |
| `HttpCall` | Contract | `IResponseConformanceAppService.AssertResponseAsync(ResponseConformanceDto)` | Yanıtın spec'e uygunluğu |
| `HttpCall` (istek doğrulama) | Contract | `.AssertRequestAsync(RequestConformanceDto)` | İstek tarafı uygunluk |
| Yazım anı | — | `.BuildRequestExampleAsync(OperationSelectionDto)` | Örnek gövde üretimi |
| Yazım anı | — | `.SuggestOperationBindingsAsync(OperationSelectionDto)` | Operasyon eşleme önerisi |
| Yayın kapısı | — | `.ValidateScenarioAssertionsAsync(AssertionDerivabilityDto)` | **Assertion sözleşmeden türetilebilir mi** |
| `DbAssert` | Persistence | `IDatabaseAssertionAppService.AssertRowAsync / AssertCountAsync / AssertAbsentAsync(RowAssertionRequestDto)` | `TimeoutMs` + `PollIntervalMs` sunucu tarafında |
| Çoklu `DbAssert` | Persistence | `.AssertBatchAsync(List<RowAssertionRequestDto>)` | **Tek round-trip'te çoklu sonuç** |
| Kırmızı koşu | Teşhis | `Ptn.ApiContractChecker...IDiagnosisAppService.DiagnoseAsync` | RFC 9457 rapor |
| Kırmızı koşu | Teşhis | `Ptn.DatabaseChecker...IDiagnosisAppService.DiagnoseAsync` | RFC 9457 rapor |

**Kodda tespit edilen sınır (K1):** `RowAssertionRequestDto.ConnectionId` bir **Guid**'dir, mantıksal
ad değildir. Yani RESEARCH-0003'ün S-05 maddesi ("`ConnectionRef` mantıksal ad") checker'da
karşılanmamıştır — **ve karşılanmasına gerek yoktur**: mantıksal ad → Guid çözümü
`environment_bindings` tablosunda Test Module'ün işidir. Senaryo dokümanı ortamdan bağımsız kalır,
checker sözleşmesi değişmez. Bu, backlog'a **checker isteği olarak yazılmaz**.

---

## 9. Kanıt, gizlilik ve saklama

- Varsayılan `ValueRetentionMode = None`: kanıt "beklenen vs gerçek" **şeklini** taşır, değeri değil (ADR-0007).
- `IncludeRowOnFailure` gibi değer taşıyan bayraklar yalnız açık talep + izinle açılır ve
  `run_steps.redaction_mode` kolonunda **koşum başına kaydedilir** (denetlenebilirlik).
- Test verisi yaşam döngüsü yönetişimi (saklama takvimi, gerçek PII içeren test verisinin silinmesi,
  erişim kontrolü) GDPR md. 5 saklama sınırlaması ve veri minimizasyonu ile gerekçelenir; raporlar
  kapsam ve geç/kal bilgisine indirgenir, gövde gömülmez (K2/K3).
- Blob artefaktlarında `expires_at`; partition DROP ile satır tarafı, blob TTL ile dosya tarafı temizlenir.

---

## 10. MCP yüzeyi — an bazında ve **Tasks** ile

Tool kataloğu RESEARCH-0003 §6'da belirlendi (toplam ≤ 12, an bazında profil). Bu belgenin eklediği
somut protokol ayrıntısı:

**Tasks extension (K2).** Uzun koşu tool çağrısı bloklamaz; sunucu `resultType: "task"` ile
`taskId` + `ttlMs` + `pollIntervalMs` döndürür. Durumlar: **`working`**, **`input_required`**,
**`completed`**, **`failed`**, **`cancelled`** — son üçü terminal. `input_required`, insan onayı
gereken adımın (bizde: **yama onayı**, I-06) protokoldeki tam karşılığıdır ve `tasks/update` ile
yanıtlanır. İptal kooperatiftir. Bizim `test_run_statuses` sözlüğümüz bu beşliye kayıpsız eşlenir:
`Pending|Running → working`, `Completed → completed`, `Failed → failed`, `Cancelled → cancelled`.

| MCP tool (composition host) | Arkasındaki AppService | An |
|---|---|---|
| `contract.operation.find` | `ISpecSnapshotAppService` / conformance yüzeyi | A |
| `db.table.describe` | `ISchemaDiscoveryAppService` | A |
| `db.binding.suggest` | `ISchemaDiscoveryAppService` (+ FK grafiği) | A |
| `scenario.validate` | `ScenarioVersionAppService` + `ValidateScenarioAssertionsAsync` | A |
| `scenario.dryRun` | `TestRunAppService` (tek koşu, task) | A |
| `scenario.save` | `ScenarioAppService` | A |
| `run.get` | `TestRunAppService` | C |
| `run.step.evidence` | `RunEvidenceAppService` | C |
| `db.assert.explain` / `api.diagnose` | iki `IDiagnosisAppService` | C |
| `change.since` | `IComparisonRunAppService` / `IContractCheckRunAppService` bulgu sayfası | D |
| `scenario.impacted` | `ImpactAnalysisManager` → `finding_links` | D |
| `scenario.patch.propose` | `HealProposalAppService` | D |

---

## 11. Dışa aktarım — üç format, tek kaynak

| Bizim alan | CTRF | OpenTelemetry | SARIF |
|---|---|---|---|
| `scenarios.title` | `tests[].name` | `test.case.name` | — |
| `run_scenarios.status` | `tests[].status` (`Broken → other`) | `test.case.result.status` (`pass`/`fail`) | — |
| `run_scenarios.duration_ms` | `tests[].duration` | span süresi | — |
| `run_scenarios.attempt` | `tests[].retries` + `retryAttempts[]` | — | — |
| `scenario_health.flaky_rate` | `tests[].insights.flakyRate` | — | — |
| `run_steps` | `tests[].steps[]` | child span | — |
| `test_runs` sayaçları | `summary.*` | `test.suite.run.status` | — |
| `run_artifacts` | `tests[].attachments[]` | — | — |
| `environments` + commit/build | `environment.*` | `cicd.pipeline.*` | — |
| `finding_links` + severity | — | — | `results[].partialFingerprints` |
| Bir önceki koşu | `baseline.reportId` | — | baseline/suppression |

Üçü de **türetilmiş**tir; hiçbiri kalıcı model değildir.

---

## 12. Güvenlik

**OWASP GenAI/LLM Top 10 2026 (K2):** Prompt Injection **LLM01** olarak birinci sırada kalmış;
**Excessive Agency** altıncılıktan **üçüncülüğe** yükselmiş — listedeki en büyük sıçrama — ve
gerekçesi doğrudan bizim alanımız: model çıktısının kabuk komutu çalıştırdığı, dış API çağırdığı
veya veritabanı işlemi yönettiği **ajanik sistemler**. 2026 sürümü ilk kez gerçek olay verisiyle
ağırlıklandırılmış. Ayrıca ayrı bir **Agentic Applications Top 10** yayımlanmış (Agent Goal Hijack,
Tool Misuse, Agent Identity & Privilege Abuse).

Bizim mimarimiz bu iki maddeyi **tasarımla** kesiyor:

| Risk | Bizdeki kesici |
|---|---|
| LLM01 Prompt injection (SUT yanıtı/DB hücresi modele talimat taşır) | I-01 (koşumda model yok) + I-05 (ham değer saklanmaz/gösterilmez) |
| LLM03 Excessive agency | Model yalnız **öneri** üretir; yazma yolu **insan onayı** + `heal_proposals` durumu |
| Tool misuse | ≤ 12 tool, an bazında profil, `readOnly` işaretleri, ADR-0008 küratörlüğü |
| Privilege abuse | Checker kimliği salt-okunur; sandbox bağlantısı ayrı ve yetkilendirilmiş |

---

## 13. Riskler ve karşı önlemler

| Risk | Önlem |
|---|---|
| Senaryo kimliğinin hash'e bağlanması → çift kayıt | Kalıcı `scenario_key` (I-08); hash yalnız dedup |
| `run_steps` tablosunun şişmesi | Aylık partition + DROP; kanıt blob'a taşınır |
| Tanım değişince tarihin bozulması | `run_scenarios.scenario_version_id` (I-04) |
| Sessiz onarım | `finding_fingerprint` zorunlu + `PendingApproval` + `Healed` etiketi |
| Karantinanın gerçek hatayı gizlemesi | `scenario_health` durum geçişleri denetlenir; `quarantined_until` zorunlu son tarih |
| Checker sürüm kayması | İki checker `0.2.0-alpha.1` ile yayımlıdır (CURRENT-0002); Test Module tek ABP sürüm grafiğinde restore edip CURRENT-0004'teki sekiz kabul kapısını çalıştırmadan stable karar vermez |
| Standart kayması (Arazzo/Overlay/MCP/CTRF) | Sürüm sabitlenir, `SOURCE-0001`'e erişim tarihiyle yazılır, okuyucu/yazıcı adaptörü ince tutulur |
| Şema adı çakışması | `testlookup` / `testcatalog` / `testrun`; DB checker'ın beş şeması korunur (§5.2) |

---

## 14. Faz planı ve kabul ölçütleri

| Faz | İçerik | Kabul ölçütü |
|---|---|---|
| **T1 — İskelet** | `testlookup` + `testcatalog` + `testrun` şemaları, senaryo/sürüm/içerik, elle yazılmış Arazzo dokümanı, HTTP + `DbAssert` adımlı runner | Elle yazılmış bir senaryo uçtan uca yeşil koşar; **tek satır model çağrısı yok** |
| **T2 — Kanıt ve rapor** | `run_steps` + kanıt + blob artefakt + CTRF/JUnit dışa aktarımı | Kırmızı koşuda hangi adımın hangi oracle katmanında patladığı raporda görünür |
| **T3 — Yazım ajanı** | MCP §6.1 tool'ları, `scenario.dryRun`, onay akışı, `ValidateScenarioAssertionsAsync` kapısı | Ajanın ürettiği doküman T1 runner'ında **değişmeden** koşar |
| **T4 — Teşhis** | İki `IDiagnosisAppService` bağlanması, `run.step.evidence` | Kırmızı koşuda sıralı hipotez raporu ≤ 4 KB ile döner |
| **T5 — Bakım** | `scenario_step_index` + `finding_links` + `heal_proposals` (Overlay) + `Healed` etiketi | Bir API alanı opsiyonel yapıldığında etkilenen senaryolar bulunur ve ajanın bağlamına giren veri **< 2.000 token** kalır |
| **T6 — Sağlık** | `scenario_health`, karantina politikası, SARIF dışa aktarımı | Flaky senaryo CI'ı kırmadan izlenir ve durum geçişi denetlenebilir |

---

## 15. Checker'lardan istenen ek geliştirmeler

Bu taramada Test Module'ün ihtiyacı olup checker tarafında **eksik veya belirsiz** olan maddeler
ayrı bir sınıflandırılmış alana yazılmıştır:
[[90-Inbox/BACKLOG-0001-Checker-Ek-Gelistirme-Talepleri|BACKLOG-0001]].
Test Module'ün kendi özellik listesi: [[90-Inbox/PLAN-0003-TestModule-Ozellik-Listesi|PLAN-0003]].

---

## 16. Kaynaklar (bu belgeye özel; erişim 2026-08-12)

| Kaynak | Neyi kanıtlıyor | Sınıf |
|---|---|---|
| https://playwright.dev/docs/test-agents | planner/generator/healer; `seed.spec.ts`; ajan = talimat + MCP tool demeti | K2 |
| https://docs.testkube.io/articles/test-workflows-high-level-architecture | Tanım CRD ↔ Execution CRD ayrımı; efemer koşum kaynakları; sonuç/artefakt taşınması | K2 |
| https://docs.qameta.io/allure-testops/briefly/test-results/ | `testCaseId = md5(fullName + sort(params))`, `historyId`, launch kapanışında case upsert, beş statü | K2 |
| https://reportportal.io/docs/developers-guides/ReportingDevelopersGuide/ | `Launch → TestItem → Log → Attachment`; `hasStats=false` nested step; Postgres + MinIO + OpenSearch | K2 |
| https://kiwitcms.readthedocs.io/en/latest/_modules/tcms/testruns/models.html | `TestRun`/`TestExecution` alanları, `case_text_version`, statü lookup'ı (`weight`), property tabloları | K2 |
| https://docs.pact.io/getting_started/versioning_in_the_pact_broker | Pacticipant/version ayrımı, **pact içeriğinin hash ile dedup'u**, matrix, can-i-deploy, webhook | K2 |
| https://docs.datadoghq.com/tests/flaky_management/ | Flaky durum makinesi (Active/Quarantined/Disabled/Fixed) ve izlenen metrikler | K2 |
| https://learn.microsoft.com/azure/devops/pipelines/test/test-impact-analysis | Deterministik etki analizi (çağrı grafiği + kapsam) | K2 |
| https://schemathesis.readthedocs.io/en/stable/explanations/stateful/ | OpenAPI link'lerinden üretici/tüketici zinciri ve durum makinesi | K2 |
| https://spec.openapis.org/arazzo/latest.html | Arazzo 1.1.0: adım, `successCriteria`, runtime expression ABNF, retry, `dependsOn`, `correlationId` | K2 |
| https://spec.openapis.org/overlay/v1.0.0.html | Overlay: sıralı `actions[]`, JSONPath `target`, `update` merge / `remove: true` | K2 |
| https://ctrf.io/docs/full-schema | CTRF tam şeması: summary/tests/environment/insights/baseline alanları, statü enum'u | K2 |
| https://opentelemetry.io/docs/specs/semconv/registry/attributes/test/ | `test.case.name`, `test.case.result.status`, `test.suite.name`, `test.suite.run.status` | K2 |
| https://docs.oasis-open.org/sarif/sarif/v2.1.0/sarif-v2.1.0.html | SARIF 2.1.0 bulgu taşıma formatı, fingerprint/baseline kavramları | K2 |
| https://modelcontextprotocol.io/extensions/tasks/overview | Tasks: `working / input_required / completed / failed / cancelled`, `ttlMs`, `pollIntervalMs`, kooperatif iptal | K2 |
| https://genai.owasp.org/resource/owasp-genai-llm-top-10-2026/ | LLM01 Prompt Injection; **LLM03 Excessive Agency** yükselişi; olay verisiyle ağırlıklandırma | K2 |
| https://abp.io/docs/latest/framework/infrastructure/blob-storing | ABP BLOB Storing: container sistemi, Database/FileSystem/S3-uyumlu/MinIO sağlayıcıları | K2 |
| https://abp.io/docs/latest/framework/infrastructure/background-jobs | Arka plan iş yöneticisi + distributed lock gereksinimi | K2 |
| https://docs.abp.io/en/abp/latest/Distributed-Event-Bus | Transactional outbox/inbox ve distributed lock ile eşzamanlılık | K2 |
| https://learn.microsoft.com/ef/core/what-is-new/ | EF Core 10: complex type → JSON kolon eşlemesi, JSON path sorgusu | K2 |
| https://www.crunchydata.com/blog/five-great-features-of-postgres-partition-manager | Zaman bazlı partition + `retention` ile otomatik DROP | K3 |
| https://dataegret.com/2025/05/data-archiving-and-retention-in-postgresql-best-practices-for-large-datasets/ | Büyük veri setlerinde arşiv/saklama pratiği; TOAST davranışı | K3 |
| https://milanjovanovic.tech/blog/testcontainers-best-practices-dotnet-integration-testing | Konteyner başına tek başlatma + test başına hızlı reset; rollback'in kırıldığı durumlar | K3 |
| https://learn.temporal.io/tutorials/typescript/background-check/durable-execution/ | Deterministik replay, activity retry, replay testi | K2 |
</content>
</invoke>
