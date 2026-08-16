---
id: BACKLOG-0001
type: backlog
status: draft
title: Test Module'un checker'lardan istedigi ek gelistirmeler — siniflandirilmis talep defteri
updated: 2026-08-13
decision_refs:
  - ADR-0002
  - ADR-0007
  - ADR-0008
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0003
  - RULE-0004
---

# Checker ek geliştirme talep defteri

Bu sayfa, [[90-Inbox/RESEARCH-0006-TestModule-Global-Tarama-Ve-Veri-Modeli|RESEARCH-0006]] taraması
sırasında ortaya çıkan ve **Test Module'ün ihtiyacı olduğu halde checker paketlerinde bulunmayan**
maddelerin kayıt yeridir. Kanonik değildir; bir madde kabul edildiğinde ilgili plana
(PLAN-0001 `DBC-xx` / PLAN-0002 `ACC-xx`) ve Roadmap'e taşınır, buradan kaldırılır.

**Neden ayrı sayfa:** PLAN-0001/PLAN-0002 checker'ların **kendi** yol haritasıdır. Bu defter
consumer'ın talebidir; iki listeyi karıştırmak "kimin işi" sorusunu bulanıklaştırır.

## Sınıflandırma

| Sınıf | Anlamı | Kural |
|---|---|---|
| **A — Bloklayıcı** | Bu madde olmadan bir Test Module fazı **başlayamaz** | Faz planına girer, tarih taahhüdü ister |
| **B — Maliyet** | Çalışır ama fazladan round-trip / token / süre yakar | İlk sürümde geçici çözümle idare edilir, gerekçesi yazılır |
| **C — Fırsat** | Kaliteyi artırır, zorunlu değildir | Motor genişleme dalgasına eklenir |

## Kanıt notu (2026-08-12, K1)

Bu defter yazılırken kaynak tekrar tarandı. **PLAN-0001/PLAN-0002'de "yok" diye işaretlenen
bazı maddeler artık kaynakta mevcuttur** ve buraya talep olarak yazılmamıştır:

| Yetenek | Bugünkü kaynak gerçeği |
|---|---|
| Bulgu parmak izi | `FindingFingerprintCalculator` **her iki checker'da da** mevcut; `FindingDto.Fingerprint` yayınlanıyor |
| Fark şiddeti | `DifferenceSeverityClassifier` (DB) ve `SpecDifferenceSeverityClassifier` (API) mevcut |
| Sayfalı bulgu okuma | `FindingQueryInput` (DB) ve `FindingPagedResultDto` (API) mevcut |
| Değer saklama politikası | `ValueRetentionPolicyResolver` + `FindingValueRedactor` **her iki tarafta** mevcut |
| Assertion yüzeyi | `IDatabaseAssertionAppService` (`Row`/`Count`/`Absent`/`Batch`), sonuçta `ObservedAtMs` + `AttemptCount` mevcut |
| Uygunluk yüzeyi | `IResponseConformanceAppService` (`AssertResponse`/`AssertRequest`/`BuildRequestExample`/`SuggestOperationBindings`/`ValidateScenarioAssertions`) mevcut |
| Teşhis yüzeyi | İki tarafta da `IDiagnosisAppService.DiagnoseAsync` mevcut |
| Run durum olayı | `ComparisonRunStatusChangedEto` ve `ContractCheckRunStatusChangedEto` mevcut |
| Şema çözümleyici | `ISpecSchemaResolver` + `SpecSchemaResolver` mevcut |

**Sonuç:** PLAN-0001 ve PLAN-0002'nin durum satırları güncellenmelidir; bu defter yalnız
**gerçekten kalan** boşlukları taşır.

---

## Sınıf A — Bloklayıcı talepler

Açık Sınıf A talep kalmadı. Dört talep aşağıdaki kapanış kaydına taşındı ve
**`0.2.0-alpha.2` ile publictir** (2026-08-12; push sonrası 16/16 PackageId registry'de
doğrulandı — [[05-Operations/Package-Release-Ledger|LEDGER-0001]]). Test Module bu
yüzeyleri artık paket restore ederek tüketebilir.

---

## Kapanan talepler

| # | Sahip | Talep | Kapanış |
|---|---|---|---|
| **DBX-01** | Database Checker | Kararlı DB finding adres sözleşmesi | **Kapandı ve yayımlandı (2026-08-12, `0.2.0-alpha.2`).** `FindingAddressDto`, `FindingAddressGrammar`, Mapperly projection ve `PACKAGE-README` exact `schema.object[.child]`/fingerprint sırasını yayınlıyor |
| **DBX-02** | Database Checker | `SinceRunId` + bounded `Fingerprints` | **Kapandı ve yayımlandı (2026-08-12, `0.2.0-alpha.2`).** Validator, Manager reference kuralı ve repository scalar-set/IN filtreleri count/page ile aynı predicate'i kullanıyor; legacy null fingerprint New sayılmıyor |
| **ACX-01** | API Contract Checker | Typed operation finding adresi | **Kapandı ve yayımlandı (2026-08-12, `0.2.0-alpha.2`).** Sekiz bileşenli `FindingAddressDto` Mapperly ile yayınlanıyor; `FindingAddressGrammar` ve `PACKAGE-README` tam hash sırasını/normalizasyonu sabitliyor |
| **ACX-02** | API Contract Checker | `SinceRunId` + bounded `Fingerprints` | **Kapandı ve yayımlandı (2026-08-12, `0.2.0-alpha.2`).** Aynı base/target document çifti içindeki eski Completed run Manager/Repository zincirinde doğrulanıyor; change-state, explicit fingerprint ve alan filtreleri tek selection'da kesişiyor |
| **DBX-03** | Database Checker | `0.2.0-alpha.1` yayını (assertion + teşhis yüzeylerinin public olması) | **Kapandı (2026-08-12).** Sekizli aile NuGet.org'da `0.2.0-alpha.1` ([[01-Current/Checker-Packages-Truth|CURRENT-0002]]) |
| **ACX-03** | API Contract Checker | `0.2.0-alpha.1` yayını | **Kapandı (2026-08-12).** Aynı kayıt |
| **DBX-04** | Database Checker | `AssertBatchAsync` üst sınırı ve süre bütçesi | **Kapandı, kaynakta (2026-08-13, `KBP-708`).** `PACKAGE-README` batch boyutunun kısmi sonuç vermeden reddedildiğini (`BatchRequired`/`BatchTooLarge`), timeout'un sessizce kırpıldığını ve **toplam batch bütçesi olmadığını** (en kötü hal `MaxBatchSize × MaxTimeoutMs` = 600 s) yazıyor |
| **ACX-04** | API Contract Checker | Uygunluk çıktı tavanı ve ihlal kırpma kuralı | **Kapandı, kaynakta (2026-08-13, `KBP-625`).** İki aşamalı kırpma (önce `MaxViolations`, sonra 128 baytlık transport payı düşülmüş UTF-8 bütçesi), kuyruktan atma sırası, kırpmadan **önce** hesaplanan `OutcomeCode` ve kırpma bayrağının olmadığı `PACKAGE-README`'de |
| **DBX-05** | Database Checker | Run olayına bulgu özeti (`NewFindingCount`, `MaxSeverityCode`) | **Kapandı, kaynakta (2026-08-13, `KBP-709`).** `ComparisonRunStatusChangedEto` iki alanı taşıyor; şiddet sırası `DifferenceSeverityCodes.Ranked`'a, New sayımı önceki Completed koşuya dayanıyor. Üç argümanlı ctor korundu |
| **ACX-05** | API Contract Checker | Aynı özet alanları `ContractCheckRunStatusChangedEto` için | **Kapandı, kaynakta (2026-08-13, `KBP-626`).** Mevcut `FindingChangeStateManager` sınıflandırması yeniden kullanıldı; ikinci bir sınıflandırma yolu açılmadı |

> **Yayın notu:** DBX-04/05 ve ACX-04/05 **kaynakta** kapalıdır, **yayımlanmış değildir**.
> Tüketici bu alanları paket restore ederek almadan önce yeni bir sürüm çıkmalıdır; iki modülün
> `common.props` sürümü hâlâ `0.2.0-alpha.2`'dir ve o sürüm immutable olarak yayımlanmıştır
> ([[05-Operations/Package-Release-Ledger|LEDGER-0001]]).

> **Wiki tutarsızlığı (bulgu, 2026-08-12):** `CURRENT-0002` içinde "Database Checker — oracle yüzeyi
> (kaynakta hazır, yayımlanmadı)" başlığı, aynı bölümün son cümlesiyle ("bu yüzeyler `0.2.0-alpha.1`
> paket ailesiyle NuGet.org'da yayımlıdır") çelişiyordu. Yetki sırası gereği paket kaydı esas alınmış
> ve başlık düzeltilmiştir.

---

## Sınıf B — Maliyet talepleri

| # | Sahip | Talep | Bugünkü kod gerçeği (K1) | Geçici çözüm | İlgili |
|---|---|---|---|---|---|
| **ACX-06** | API Contract Checker | **MCP bütçe ve doğruluk kapıları** (Roadmap ACC-18..22): statik katalog bütçesi, çıktı tavanları, mutasyon skoru | Telemetri altyapısı (`ApiContractCheckerActivity`) var; bütçe kapıları yok. **Checker deposunda açılamaz — aşağıdaki engel notuna bak** | Test Module kendi tool kataloğunda bütçe testi yazar | TM-20, TM-31 |

### ACX-06 engel notu (2026-08-13)

ACX-06 checker deposunda **kapatılamaz**; kalan iş sahibi yanlış yerde aranıyordu.

| Madde | Durum | Kanıt |
|---|---|---|
| ACC-18 statik katalog bütçesi | **Checker'ın işi değil** | Ölçüm MCP yüzeyini ayağa kaldırıp `tools/list` çağırmayı gerektirir. [[03-Decisions/ADR-0008-Mcp-Surface-Placement\|ADR-0008]] (accepted): "Checker paketleri MCP'ye dair hiçbir tip, bağımlılık veya endpoint taşımaz." Bu workspace'te composition host yok (`checkers/` altında yalnız iki modül var) |
| ACC-19 dinamik çıktı bütçesi | **Zaten kapalı** | `TrimToBudget` + public tavanlar; PLAN-0002 durum tablosu da "Tamamlandı" diyor. Kırpma kuralı artık `PACKAGE-README`'de yazılı (ACX-04, KBP-625) |
| ACC-20 G1/G2 doğruluk kapısı | **Kısmen checker'ın işi** | Derivability yüzeyi (`AssertionDerivabilityManager`) mevcut. G1'in Arazzo senaryo doğrulaması senaryo modeline dayanır; senaryo checker'ın nesnesi değildir ([[03-Decisions/ADR-0002-Checker-Packaging-Boundary\|ADR-0002]]) |
| ACC-21 G3 mutasyon kapısı | **Senaryo koşucusuna bağımlı** | "Mutant spec'ten stub üret → senaryoyu koştur → kırmızıya dönmesi şart" adımı bir senaryo koşucusu ister; o Test Module'dedir |
| ACC-22 G4 + tool golden eval | **Checker'ın işi değil** | "Her MCP tool'u için golden vaka seti" — tool kataloğu composition host'ta küratörlenir (ADR-0008) |

ADR-0008 checker'ın MCP'ye borcunu üç protokolden bağımsız maddeye indirger (kararlı kod
kümeleri, sınırlı çıktı, sayfalama/filtreleme) ve "**hepsi zaten karşılanır**" der. Dolayısıyla
ACX-06'nın checker tarafındaki payı bugün kapalıdır; kalan G1–G4 kapıları composition host
açıldığında **TM-20/TM-31 altında** açılmalıdır.

**Öneri:** ACX-06 bu defterden Test Module iş listesine taşınsın; checker'a düşen bir borç
olarak burada durması "kimin işi" sorusunu yanlış cevaplıyor.

---

## Sınıf C — Fırsat talepleri

| # | Sahip | Talep | Neden değerli | İlgili |
|---|---|---|---|---|
| **DBX-06** | Database Checker | **FK komşuluk grafiği yüzeyi** (RESEARCH-0002 M-12): tablo → 1 seviye komşu + FK yönü | `db.binding.suggest` önerisinin kalitesi doğrudan buna bağlı; yazım anı tokenini düşürür | TM-17 |
| **DBX-07** | Database Checker | **Şema lint yüzeyi** (RESEARCH-0002 M-08): "PK yok", "unique yok", "generated kolon" uyarıları | Yayın kapısında "bu tabloya anahtarla assertion yazamazsın" uyarısı, koşumda `KeyNotUnique` almaktan ucuzdur | TM-17 |
| **ACX-07** | API Contract Checker | **OpenAPI `links` tabanlı üretici→tüketici zinciri önerisi** (Schemathesis deseni) | Çok adımlı senaryonun iskeletini ajanın tahmin etmesi yerine spec'ten türetir | TM-17 |
| **ACX-08** | API Contract Checker | **Uygunluk profilinin senaryo başına geçersiz kılınması** (`Strict`/`Runtime`/`Lenient`) | Kritik senaryoda sıkı, keşif senaryosunda gevşek profil; gürültü yönetimi | TM-07 |

---

## Bilinçli olarak checker'dan **istenmeyecek** olanlar

| Ne | Neden Test Module'ün işi |
|---|---|
| Mantıksal `ConnectionRef` desteği (RESEARCH-0003 S-05) | Checker `ConnectionId` (Guid) ister; mantıksal ad → Guid çözümü `environment_bindings` tablosunda consumer'ın işidir. Checker sözleşmesi değişmez |
| Test verisi seed/cleanup | ADR-0007 salt-okunur invariant'ı; `ITestDataSandbox` ayrı ve açıkça yetkilendirilmiş bağlantı kullanır |
| Senaryo saklama / sürümleme | Senaryo checker'ın nesnesi değildir (ADR-0002) |
| MCP tool yayınlama | ADR-0008: tool kataloğu ürün başına küratörlenir, capability başına değil |
| Bulgu ↔ senaryo eşlemesinin checker'da tutulması | Checker senaryoyu bilmez; eşleme `finding_links` tablosunda consumer'dadır (ACC-10 ile aynı sınır) |
| LLM tabanlı teşhis/onarım | Oracle deterministik kalır; model yalnız öneri üretir |
</content>
