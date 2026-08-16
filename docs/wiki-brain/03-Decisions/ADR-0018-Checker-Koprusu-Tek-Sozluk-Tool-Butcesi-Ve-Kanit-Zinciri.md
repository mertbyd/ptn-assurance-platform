---
id: ADR-0018
type: decision
status: accepted
title: Checker koprusu — tek sozluk, tool butcesi ve kanit zinciri
created: 2026-08-13
updated: 2026-08-15
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs:
  - ADR-0007
  - ADR-0008
  - ADR-0014
  - ADR-0015
  - ADR-0016
  - ADR-0017
rule_refs:
  - RULE-0005
  - RULE-0006
  - RULE-0007
  - RULE-0008
---

# ADR-0018 — Checker köprüsü: tek sözlük, tool bütçesi ve kanıt zinciri

> Dayanak: [[90-Inbox/RESEARCH-0015-Ajan-Gerceklikleri-Ve-Checker-Koprusu|RESEARCH-0015]].
> ADR-0015 §F'deki *"adapter tek ajan sözlüğüne normalize eder"* kuralının uygulama kararıdır.

## Bağlam

İki checker kod seviyesinde **mimari ikizdir** — `IDiagnosisRule` aynı beş üye, `DiagnosisReport`
aynı RFC 9457 şekli, `DiagnosisConfidenceCodes` byte-identical. Köprünün **%70'i bedavadır.**

Ama sözlük ayrışıyor ve ölçümler bu ayrışmanın ajanı doğrudan bozduğunu söylüyor
(RESEARCH-0015 §1-3):

- Model **varsayılan olarak tahmin eder**; bu eğitim teşvikidir, prompt ile kapatılamaz.
- Ajanın **sözel güveni kontrol sinyali değildir**.
- **18/18 frontier model** uzun bağlamda bozuluyor; **konuyla ilgili ama yanlış** bilgi
  ilgisizden daha çok zarar veriyor.
- Küçük model **çağrıyı biçimlendirmekte iyi, doğru tool'u seçmekte zayıf**.

## Karar

### A. Tek ajan sözlüğü — köprü sahibidir

Köprü **tek** kelime dağarcığı tanımlar; iki adapter ona eşler. Ajan **hiçbir zaman** checker'ın
ham kodunu görmez.

Kod seviyesinde tespit edilen dört çakışma ve çözümü:

| Çakışma | Bugün | Köprüde |
|---|---|---|
| Hipotez grameri | api: `H-CD-01` · db: `RowNeverCreated` | **`PtnHypothesisCodes`** — tek gramer |
| Casing | db: `Passed` · api: `passed` | **`PtnOutcomeCodes`** — tek casing |
| **`ObjectReference.SchemaName`** | api: **OpenAPI şeması** · db: **veritabanı şeması** | **`PtnLocation`** — `apiSchemaName` / `dbSchemaName` / `dbTableName`, **çakışan ad yok** |
| Probe kodu | api: `Match` · db: `Matches` | Tek form |

**Fingerprint'ler birleştirilmez.** İki checker'ın parmak izi grameri farklıdır (api: kind +
direction + 8 bileşenli adres + delta; db: motor çifti + şema/nesne/çocuk + kind + delta).
Köprü **çıplak fingerprint dışarı vermez**; her zaman `PtnFindingRef { sourceChecker, fingerprint }`
çifti olarak verir.

### B. Tool bütçesi ve toolset

Aktif tool sayısı **≤ 7**. Geri kalan yetenek **toolset** olarak gruplanır ve **dinamik keşifle**
açılır (GitHub MCP Server deseni).

| Tool | Ajanın sorusu | Arkasında birleşen |
|---|---|---|
| `ptn_ground` | "bu iş adımının zemini ne?" | `SuggestOperationBindings` + `BuildRequestExample` + `DescribeTable` — **tek çağrı** |
| `ptn_validate` | "yayınlanabilir mi?" | lint + `ValidateScenarioAssertions` + DMN kapsam |
| `ptn_run` | "koştur" | TestRun AppService |
| `ptn_result` | "ne oldu?" | rapor read model |
| `ptn_explain` | "neden patladı?" | **iki** `DiagnosisManager` + kanıt zinciri, tek raporda |
| `ptn_knowledge` | "kural ne diyor?" | MCP **Resource** |
| `ptn_impact` | "bu bulgu neyi bozar?" | adım indeksi (sonra) |

`ptn_ground`'un tek çağrı olması bilinçlidir: üç ayrı çağrının ara sonuçları ajanın bağlamına
girmez (context rot karşı önlemi).

**Yanıt şekillendirme zorunludur:** `responseFormat: concise | detailed` (ölçülmüş ≈ ⅓ token),
ağır gövde `resource_link`, öğreten hata mesajı.

### C. Kanıt zinciri — köprünün asıl işi

İki checker'ın `DiagnosisManager`'ı **kendi alanında** hipotez-kanıt-güven döngüsü çalıştırıyor.
Köprü **süpervizördür**: kanıtı **alanlar arasında** ilişkilendirir.

**Teşhis yönü (An 6):**
```
403 sinyali        (API Checker · FailureIdentity: ChallengeScheme, ChallengeScopes)
  → user_roles                    (DB Checker · AssertRow / probe)
  → role_permission_grants        (DB Checker)
  → operasyonun gerektirdiği scope (API Checker · SpecSnapshot)
  → DOĞRULANDI: gereken izin kullanıcıda yok
```

**Yazarlık yönü (An 2-3):**
```
"öğrenci 6 saatte bir bilet"
  → operasyon adayı (skorlu)      (API Checker)
  → geçerli istek örneği           (API Checker)
  → etkilenen tablo/kolon          (DB Checker · etki ayak izi)
  → anahtar PK/unique mi           (DB Checker)
  → assertion türetilebilir mi     (API Checker)
  → DOĞRULANDI: yazılabilir
```

**Aynı desen, iki yön.** Her adımda ajan bir checker'dan **olgu** alır; hiçbir adımda tahmin etmez.

Zincirin gerekliliği alan bilgisiyle sabit: *"aynı 403 kodu eksik IAM rolünden, kapalı API'den
veya organizasyon politikasından gelebilir — hepsi 403 döner."* Tek sinyal teşhis değildir.

### D. Kanıt-alıntı kapısı

RAPTOR deseni benimsenir: **alıntısız hipotez rapora giremez.**

- Her `HypothesisAssessment` en az bir `ProbeEvidence`'ı **kimliğiyle** alıntılar.
- Köprü, alıntısı olmayan veya kanıtı doğrulanmamış hipotezi **düşürür**.
- Rapor **destekleyen ve çelişen** kanıtı ayrı taşır (AgentRCA hipotez tablosu deseni).

Bu, halüsinasyona karşı mekanik kapıdır: model istediği kadar hipotez üretsin, kanıtsızı rapora
giremez.

### E. Sözlük drift'i derleme zamanında kırılır

Köprünün eşleme tablosu, checker'ların **kod sabitlerine karşı** test edilir:

```
AssertionOutcomeCodes üye kümesi        == köprünün beklediği küme
ConformanceOutcomeCodes üye kümesi      == köprünün beklediği küme
HypothesisKindCodes üye kümesi          == köprünün beklediği küme
DiagnosisConfidenceCodes üye kümesi     == köprünün beklediği küme
```

Checker yeni bir kod eklediğinde test **kırmızı olur** ve köprüde eşleme kararı vermeye zorlar.
**Sessiz drift imkânsız hâle gelir.** Çift taraflı doğrulamanın bizdeki karşılığı budur.

### F. Yazarlık soru kataloğu bağlayıcıdır

RESEARCH-0015 §11.1'deki on iki soru köprünün kapsam tanımıdır. Her soru için ya deterministik
kaynak vardır, ya insana sorulur (RULE-0007).

Bugün eksik olanlar ve nereden gelecekleri:

| Soru | Kaynak | Durum |
|---|---|---|
| Adım zincirleme | **OpenAPI `links`** + şema eşleşmesi + `Location` gözlemi | Schemathesis deseni; **aday üretilir, insan onaylar** |
| Sınır değerler | **JSON Schema kısıtları** (`minLength` 2/10 → 1,2,3,9,10,11) | Mekanik |
| Negatif vaka | Kısıtın sistematik ihlali | Mekanik |
| **Etki ayak izi** | **Önce/sonra DB farkı** — motorumuz var | Akış yazılacak |
| Kural sınırları | DMN karar tablosu | ADR-0017 |

**Etki ayak izi için telemetri değil fark yöntemi seçilir:** OTel veritabanı span'leri
(`db.operation`, `db.sql.table`) aynı bilgiyi verir ama **SUT'un enstrümante olmasını** şart
koşar. Önce/sonra farkı SUT'tan hiçbir şey istemez ve motoru (`TableDataComparisonManager`,
`DataRowCountComparisonManager`) elimizdedir.

**Ayak izi oracle değildir.** Gözlemden çıkar, yani uygulamadan öğrenme tuzağına açıktır
(RESEARCH-0013 B7). Yalnız **insana öneri** olarak sunulur; onaylanmadan assertion üretiminde
kullanılmaz.

### G. Spec boşluğu telafi edilmez, raporlanır

*"Spec kalitesi test kalitesini belirler."* Köprü eksik şemayı doldurmaya çalışmaz;
*"bu operasyonun yanıt şeması eksik, assertion türetilemez"* bir **kırmızı karttır**
(RULE-0006 kapsamında).

### H. Yerel model uygunluğu

Bu tasarım yerel modeli **mümkün kılar**: deterministik katman kalınlaştıkça modelin işi küçülür.
Ölçülmüş sınırlar (RESEARCH-0015 §3):

- Tool seçim F1: qwen3:14B **0,971** (gpt-4 0,974), qwen3:8B **0,933**, llama3.1:8B 0,835
- Üretim eşiği **F1 ≥ 0,70**
- Gecikme 84-142 sn → **yazarlık anı için kabul edilebilir**, etkileşimli sohbet için değil
- Nicemleme fark yaratmıyor; `num_ctx` > VRAM **sessiz CPU fallback** yapar ve doğruluğu düşürür

**Kabul kriteri:** köprü tool seti, hedef yerel modelde ölçülen tool-seçim F1 **≥ 0,90** olmadan
"yerel model destekleniyor" denmez.

### I. `SchemaName` yasağının kapsamı — **konum ve rapor tipleri** *(daraltma, 2026-08-15)*

§A'nın üçüncü çakışma satırı ve risk tablosunun son satırı yasağı *"köprü sözlüğünde"* diye
**geniş** yazmıştı. Uygulama onu **dar** yorumladı ve AUDIT-0003 **BULGU-13** bu farkı kayda
geçirdi. PLAN-0005 §4.2 ve TASK-KBP-99 §2.4 **seçenek (a)**'yı seçti: **kod olduğu gibi kalır,
ADR metni daraltılır.** Bu bir **kapsam açıklamasıdır**, karar değişikliği değildir — §A'nın
kararı (tek sözlük; `PtnLocation` üç ayrı adla) aynen durmaktadır.

**Yasak nerede geçerlidir.** Yalnız **birleştirilmiş, ajana giden** yüzeyde:

| Kapsam | Tip | Neden |
|---|---|---|
| ✅ **İçinde** | `PtnLocation` (`LocationDto`) | İki anlam (OpenAPI şeması · veritabanı şeması) **aynı anda** taşınır; çakışma yalnız burada gerçektir |
| ✅ **İçinde** | Bu konumu gömen rapor yüzeyi (`DiagnosisReportDto` → `DiagnosisHypothesisDto` → `EvidenceDto`) | Ajanın okuduğu nihai rapor; konum oradan görünür |

**Yasak nerede geçerli değildir.** İki aile bilinçli olarak dışarıdadır:

| Kapsam | Tip | Neden |
|---|---|---|
| ❌ **Dışında** | Checker tarafı **kaynak** modelleri: `ApiDiagnosisLocation`, `DatabaseDiagnosisLocation` | Bunlar checker'ın **kendi** alan şeklini taşır ve Manager onları `PtnLocation`'a **çevirir**; her biri kendi içinde tek anlamlıdır. Kodun kendi yorumu da bunu söyler: *"SchemaName'i ortak Location anlamına Manager'ın çevirmesini sağlar"* |
| ❌ **Dışında** | Tek yönlü, yalnız DB tarafına giden modeller: `PtnCheckerTableDescription`, `PtnDatabaseAssertionRequest`, `PtnDatabaseAssertionSignal`, `DatabaseDerivabilityAddress` | Tek anlam taşırlar; ad hizalaması Mapperly'yi `[MapProperty]`'siz tutar. (b) seçeneği bu üç tipi yeniden adlandırırdı ve **mapper saflığı kuralını delerdi** |

**Drift testinin kapsamı bu tablodur.** `VocabularyDriftTests` yalnız yukarıdaki *"içinde"*
ailesini tarar; kapsam içi sıkılık değişmez — orada `SchemaName` adında alan **bulunamaz**.
Testin kapsamı KBP-99'un işidir ve bu metinle **birebir** aynı olmalıdır; metin ile test
arasında geçici fark kalırsa kaynak **bu bölümdür**.

## Alternatifler

- **19 AppService'i bire bir tool olarak yansıtmak:** 40+ tool; belgelenmiş anti-pattern
  (*"model entegrasyon mühendisi gibi davranmak zorunda kalır"*).
- **İki sözlüğü ajana öğretmek:** `Passed`/`passed` gibi çakışmalar sessiz hata üretir.
- **Fingerprint'leri tek namespace saymak:** gramerler farklı; çakışma sessiz olur.
- **Ajanın kendi güvenine göre soru sormasına izin vermek:** sözel güven kontrol sinyali değil.
- **Etki ayak izini OTel telemetrisiyle çıkarmak:** SUT enstrümantasyonu şart koşar; fark
  yöntemi koşmaz.
- **Etki ayak izini doğrudan oracle yapmak:** gözlemden öğrenme tuzağı (B7).
- **Kanıtsız hipotezi raporda tutmak:** halüsinasyonun rapora sızdığı yer burasıdır.

## Sonuçlar ve riskler

Yeni yüzeyler: köprü sözlüğü (`PtnOutcomeCodes`, `PtnHypothesisCodes`, `PtnLocation`,
`PtnFindingRef`), 7 tool + toolset kaydı, kanıt zinciri orkestratörü, sözlük drift testi,
etki ayak izi akışı. **Yeni proje veya katman açılmaz** (ADR-0015 §F).

| Risk | Önlem |
|---|---|
| Checker yeni kod ekler, köprü sessizce eskir | **§E derleme zamanı drift testi** |
| Tool sayısı zamanla şişer | ≤7 sınırı RULE-0007'de; fazlası toolset + dinamik keşif |
| Kanıt zinciri pahalı olur | Probe bütçesi zaten iki checker'da var (`ProbeBudgetManager`); köprü toplam bütçe uygular |
| Etki ayak izi yanlış tabloyu işaret eder | Aday kümesi FK grafiğiyle daraltılır; sonuç **öneri**, insan onaylar |
| Ayak izi sessizce oracle'a dönüşür | Onaysız ayak izi assertion üretimine **giremez** — yayın kapısında kontrol |
| Yerel model beklenenden kötü | Kabul kriteri F1 ≥ 0,90; altında "destekleniyor" denmez |
| `PtnLocation` alan adları yine çakışır | Ad çakışması **testle** yasaklanır: **birleştirilmiş, ajana giden** konum ve rapor tiplerinde `SchemaName` adında alan bulunmamalı — kapsam §I'da tanımlıdır |
