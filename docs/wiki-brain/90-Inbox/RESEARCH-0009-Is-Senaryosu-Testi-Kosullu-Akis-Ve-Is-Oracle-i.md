---
id: RESEARCH-0009
type: research
status: draft
title: Is senaryosu testi — kosullu akis, onkosul, is degismezleri ve dogal dilden senaryo uretimi
updated: 2026-08-12
decision_refs:
  - ADR-0007
  - ADR-0008
rule_refs:
  - RULE-0004
---

# İş senaryosu testi — teknik doğrulamanın ötesi

> Kanonik değildir. Bu belge önceki tasarımın **kabul edilmiş bir eksiğini** kapatır.
> RESEARCH-0003..0008 zinciri "bir adımın doğruluğu nasıl kanıtlanır" sorusunu çözdü.
> Bu belge şu soruyu çözer: **"A saatinde bilet varsa bilet al" gibi bir iş senaryosu nasıl
> çalıştırılabilir ve doğrulanabilir hale gelir?**
>
> Kanıt sınıfları: **K1** çalışan kod · **K2** birincil/akademik kaynak · **K3** sektör ölçümü.

---

## 0. Kabul edilen eksik

Şimdiye kadarki tasarım şunu iyi yapıyordu:

- Adım koştur → yanıtı sözleşmeye karşı doğrula → veritabanında satırı doğrula

Şunu **yapmıyordu**:

- Koşullu akış (*"bilet varsa al, yoksa ne olacak?"*)
- Önkoşul kavramı (*"A saatinde bilet olmalı"* — kim sağlayacak?)
- İş kuralı doğrulaması (*"koltuk sayısı tam 1 azalmalı"*)
- "Hiçbir şey test edilmedi" durumunun ayrı bir sonuç olması
- Doğal dilden senaryo üretimi için gereken iş sözlüğü

Bu eksik giderilmezse ürün, MCP'siz de yapılabilecek teknik doğrulamalardan ibaret kalır.

**Kritik tespit:** Bu eksiğin hiçbiri "koşumda model yok" kararını bozmuyor. Koşullu akış,
önkoşul ve iş değişmezleri **deklaratif ve deterministik** ifade edilebilir. Model yalnız
bunları **yazarken** gerekir — ki zaten moment A budur.

---

## 1. Kavram ayrımı: test senaryosu ≠ test durumu

| | Test durumu (test case) | **İş senaryosu (scenario)** |
|---|---|---|
| Odak | *Nasıl* test edilir | ***Ne* test edilir** |
| Kapsam | Tek fonksiyon | Uçtan uca iş süreci / kullanıcı yolculuğu |
| Yazım | Adım adım kesin talimat | İş diliyle amaç |
| Örnek | "`POST /orders` çağır, 201 bekle" | "Kullanıcı A saatinde bilet bulup satın alabilmeli" |

Sektör tanımı bunu doğruluyor: senaryo *"adım adım nasıl test edileceğinden çok, neyin test
edileceğine odaklanan daha üst seviyeli"* bir kavramdır ve *"yazılımın gerçek iş süreçlerini
destekleyip desteklemediğini"* doğrular (K3).

**Bizim ürünün var oluş sebebi ikinci sütundur.** Birincisi için MCP gerekmez.

---

## 2. Örneği uçtan uca çözmek: "A saatinde bilet varsa bilet al"

### 2.1 İnsanın söylediği

> *"Yarın saat 10:00'da İstanbul–Ankara seferinde bilet varsa satın al ve gerçekten
> satın alındığını doğrula."*

### 2.2 Bunun içinde gizli olan altı ayrı gereksinim

| # | Gizli gereksinim | Bugünkü tasarımda | Durum |
|---|---|---|---|
| 1 | **Arama** yapılacak, sonuç sayısı bilinmiyor | Var (HTTP adımı) | ✅ |
| 2 | **Karar** verilecek: varsa al, yoksa? | Yok | ❌ |
| 3 | Aramanın çıktısı satın almanın **girdisi** olacak | Var (Arazzo `outputs`) | ✅ |
| 4 | **Önkoşul**: o saatte bilet olmalı — kim sağlayacak? | Yok | ❌ |
| 5 | **İş kuralı** doğrulanacak: koltuk 1 azaldı, ödeme tutarı eşleşti | Yok (yalnız satır kontrolü var) | ❌ |
| 6 | Bilet yoksa senaryo **ne** dönecek? | Yok (yeşil dönerdi — tehlikeli) | ❌ |

Altı gereksinimin dördü eksik. Bu belge dördünü de kapatıyor.

### 2.3 Hedef senaryo dosyası

```yaml
arazzo: 1.1.0
info: { title: Bilet satin alma is senaryosu, version: 1.0.0 }

x-checknexus-scenario:
  kind: BusinessScenario            # TechnicalCheck | BusinessScenario
  intent: "A saatinde bilet varsa satin al ve satin alindigini kanitla"

  # ---- ONKOSUL: senaryo calismadan once saglanmali ----
  preconditions:
    - id: biletVar
      strategy: Arrange             # Arrange (veriyi biz yaratiriz)  | Discover (arayip buluruz)
      arrange:
        datasetRef: ist-ank-yarin-10-00
      onUnsatisfied: Inconclusive    # Inconclusive | Fail | Skip

workflows:
  - workflowId: bilet-al
    inputs:
      type: object
      properties:
        route: { type: string }
        departAt: { type: string, format: date-time }

    steps:
      # ---- 0. ADIM: is degismezinin BASLANGIC olcumu ----
      - stepId: oncekiBosKoltuk
        x-checknexus-db:
          connectionRef: booking-db
          operation: assertCount
          schema: sales
          table: Seats
          key: { FlightId: "{$inputs.flightId}", Status: "Available" }
          cardinality: atLeast 1
          outputs:
            bosKoltukOnce: observedRowCount     # sayisal olcumu sonraki adima tasi

      # ---- 1. ADIM: arama ----
      - stepId: seferAra
        operationId: searchFlights
        parameters:
          - name: route      ; in: query ; value: "{$inputs.route}"
          - name: departAt   ; in: query ; value: "{$inputs.departAt}"
        successCriteria:
          - condition: $statusCode == 200
        outputs:
          bulunanSayi: $response.body#/totalCount
          ilkSeferId:  $response.body#/items/0/id
          fiyat:       $response.body#/items/0/price

      # ---- 2. ADIM: KARAR NOKTASI ----
      - stepId: biletVarMi
        x-checknexus-branch:
          when: "$steps.seferAra.outputs.bulunanSayi > 0"
          then: goto satinAl
          else:
            end: Inconclusive         # YESIL DEGIL: hicbir sey test edilmedi
            reason: "Belirtilen saatte sefer bulunamadi; ana yol kosmadi."

      # ---- 3. ADIM: satin alma ----
      - stepId: satinAl
        operationId: purchaseTicket
        requestBody:
          payload:
            flightId: "{$steps.seferAra.outputs.ilkSeferId}"
            seatCount: 1
        successCriteria:
          - condition: $statusCode == 201
        outputs:
          biletId:    $response.body#/ticketId
          odenenTutar: $response.body#/amount

      # ---- 4. ADIM: kalicilik kaniti ----
      - stepId: biletKaydedildi
        x-checknexus-db:
          connectionRef: booking-db
          operation: assertRow
          schema: sales
          table: Tickets
          key:    { Id: "{$steps.satinAl.outputs.biletId}" }
          expect:
            Status:   { matcher: equals, value: "Confirmed" }
            FlightId: { matcher: equals, value: "{$steps.seferAra.outputs.ilkSeferId}" }
        timeout: 5000
        onFailure:
          - type: retry
            retryLimit: 10
            retryAfter: 0.5

      # ---- 5. ADIM: IS DEGISMEZI — korunum ----
      - stepId: koltukAzaldi
        x-checknexus-db:
          connectionRef: booking-db
          operation: assertCount
          schema: sales
          table: Seats
          key: { FlightId: "{$steps.seferAra.outputs.ilkSeferId}", Status: "Available" }
          outputs:
            bosKoltukSonra: observedRowCount
        successCriteria:
          - condition: >
              $steps.oncekiBosKoltuk.outputs.bosKoltukOnce
              - $steps.koltukAzaldi.outputs.bosKoltukSonra == 1
            type: simple

      # ---- 6. ADIM: IS DEGISMEZI — tutar tutarliligi ----
      - stepId: tutarEslesti
        successCriteria:
          - condition: >
              $steps.satinAl.outputs.odenenTutar
              == $steps.seferAra.outputs.fiyat
            type: simple

      # ---- 7. ADIM: IS DEGISMEZI — cift satis yok ----
      - stepId: koltukTekSatildi
        x-checknexus-db:
          connectionRef: booking-db
          operation: assertCount
          schema: sales
          table: Tickets
          key: { FlightId: "...", SeatNo: "..." }
          cardinality: exactly 1
```

### 2.4 Bu dosyadaki her yeni şey neyi çözüyor

| Yenilik | Çözdüğü gereksinim |
|---|---|
| `x-checknexus-scenario.kind` | Teknik kontrol ile iş senaryosunu ayırır; raporlama ve sağlık ayrı ölçülür |
| `preconditions` + `strategy` | "Bilet olmalı" gereksinimini açık hale getirir; kimin sağladığı kaydedilir |
| `x-checknexus-branch` | Karar noktası **adımın içinde gizli değil**, ayrı ve görünür bir adım |
| `end: Inconclusive` | "Hiçbir şey test edilmedi" durumu yeşil sayılmaz |
| DB adımında `outputs` | Sayısal ölçüm sonraki adıma taşınır — **delta assertion mümkün olur** |
| Adımlar arası `successCriteria` | İş değişmezi (korunum, tutarlılık, tekillik) ifade edilebilir |

**Kritik nokta:** Bu dosyanın **hiçbir satırı** koşum anında model gerektirmiyor. Hepsi
deklaratif ve deterministik. Model yalnız bu dosyayı **yazarken** devrede.

---

## 3. En tehlikeli tuzak: yeşil dönen ama hiçbir şey test etmeyen senaryo

### 3.1 Problem

"Bilet varsa al" senaryosunda bilet yoksa ne olur? Naif tasarımda senaryo **yeşil** döner —
çünkü hiçbir assertion başarısız olmadı. Ama **hiçbir şey de doğrulanmadı.**

Bu, literatürde bilinen ve **yanlış pozitiften çok daha tehlikeli** sayılan bir durumdur:
*"Yanlış negatifler geçen ama geçmemesi gereken testlerdir... uygulamanın kalitesi hakkında
yanlış bir güven duygusu aşıladıkları için çok daha tehlikelidirler."* (K3)

Aynı literatür "assertion içermeyen test", "totolojik test", "kimsenin incelemediği snapshot testi"
gibi kalıpları **işe yaramaz testler** başlığında toplar ve ortak sorunu şöyle özetler:
*"sorun testin yokluğu değil, yanlış güvendir."* (K3)

### 3.2 Karar: `Inconclusive` ayrı bir sonuçtur

Statü kümesi **altıya** çıkar:

| Statü | Anlamı |
|---|---|
| `Passed` | Ana yol koştu, tüm beklentiler tuttu |
| `Failed` | Hakem "hayır" dedi — gerçek bulgu |
| `Broken` | Adım hiç koşamadı — ortam/altyapı |
| `Skipped` | Bilinçli olarak atlandı |
| `Quarantined` | Karantinada; sonucu build'i kırmaz |
| **`Inconclusive`** | **Önkoşul sağlanmadı, ana yol koşmadı — hiçbir şey doğrulanmadı** |

### 3.3 `Inconclusive` yeşil değildir, kırmızı da değildir

- CI'ı **kırmaz** (bir hata bulunmadı)
- Ama **"geçti" sayılmaz** (bir şey de doğrulanmadı)
- Raporda ayrı renkte ve ayrı sayaçta görünür
- **`inconclusive_rate` bir sağlık metriğidir**: yükseliyorsa test ortamının verisi çürümüş
  ya da önkoşul stratejisi yanlış seçilmiş demektir

Bu metrik olmadan bir suite yıllarca "yeşil" görünüp hiçbir şey test etmeyebilir.

---

## 4. Önkoşul stratejileri

"A saatinde bilet olmalı" gereksinimini iki yolla karşılayabiliriz:

| Strateji | Nasıl | Artı | Eksi | Sonuç kuralı |
|---|---|---|---|---|
| **Arrange** (yarat) | Sandbox veri kümesini seeder ile yaratır | Deterministik, tekrarlanabilir | Seed kodu bakım ister | Ana yol koşar → `Passed`/`Failed` |
| **Discover** (bul) | Canlı veriden arayıp bulur | Gerçekçi | Non-deterministik | Bulunamazsa **`Inconclusive`** |

**Varsayılan: `Arrange`.** Sektör pratiği de bunu söylüyor: sentetik veriyi varsayılan yap,
her koşuya izole veri kümesi ver, koşu sonunda yok et, veri kümesini sürümle (K3).

**`Discover` ne zaman meşru?** Üretim benzeri doğrulama (smoke) ve "gerçek veriyle çalışıyor mu"
sorusunda. O zaman da `Inconclusive` oranı izlenir.

Her koşum satırı hangi stratejiyi kullandığını kaydeder (`PreconditionStrategyCode`).

---

## 5. İş değişmezleri — iş kuralı oracle'ı

### 5.1 Problem: iş kuralının "beklenen değeri" yoktur

"Bilet satın alındı" doğru mu? Şema kontrolü bunu söyleyemez — yanıt şemaya uyuyor olabilir
ama koltuk düşmemiş olabilir. Beklenen tam değeri de yazamayız çünkü fiyat/koltuk değişkendir.

### 5.2 Akademik cevap: metamorphic testing

Metamorphic testing, oracle problemini **tek bir beklenen çıktı** yerine
**çalıştırmalar arasındaki ilişkiyi** doğrulayarak çözer. REST API'ler için altı soyut ilişki
kalıbı (MROP — Metamorphic Relation Output Patterns) tanımlanmıştır ve
*"MR oracle'ı tek bir beklenen çıktı değildir; iki gözlenen çıktı arasındaki ilişkidir."* (K2)

2026'da bu yaklaşımın LLM'li versiyonu da yayımlandı: OpenAPI dokümanından yüksek seviyeli
metamorphic testler çıkarıp bunları **Gherkin Given-When-Then** kalıbıyla ifade eden çok-ajanlı
bir akış (K2). Ayrıca AGORA gibi araçlar REST API çalıştırmalarından **olası değişmezleri
dinamik olarak çıkarıyor** (105 farklı değişmez tipi) (K2).

**Bizim için sonuç:** İş oracle'ı = değişmez (invariant) assertion'ı. Ve bunların çoğu
**ölçüm farkı** olarak ifade edilebilir — yani sayı, karşılaştırma, mantık. Model gerekmez.

### 5.3 İş değişmezi kalıp kataloğu

Bu katalog yazım ajanının şablonu olur; her iş alanı bunları doldurur:

| # | Kalıp | Genel form | Bilet örneği |
|---|---|---|---|
| **M-1** | **Korunum** | Toplam sabit kalır | boş koltuk + satılan koltuk = kapasite |
| **M-2** | **Delta** | İşlem tam N kadar değiştirir | satın alma sonrası boş koltuk **tam 1** azalır |
| **M-3** | **Tutarlılık** | İki kaynak aynı değeri söyler | ödenen tutar = arama sonucundaki fiyat |
| **M-4** | **Tekillik** | Aynı kaynak iki kez tahsis edilemez | aynı koltuk için `exactly 1` bilet |
| **M-5** | **Gidiş-dönüş** | Oluştur → oku → aynı veri | bilet oluştur, `GET /tickets/{id}` aynı alanları döner |
| **M-6** | **İdempotans** | Aynı işlem iki kez → ikinci ya reddedilir ya aynı sonuç | aynı `idempotencyKey` ile iki satın alma → tek bilet |
| **M-7** | **Monotonluk** | Değer yalnız bir yönde değişir | satış sonrası boş koltuk artmaz |
| **M-8** | **Negatif yol** | Geçersiz istek reddedilir **ve durum değişmez** | koltuk yokken satın alma → 409 **ve** hiçbir satır oluşmaz |
| **M-9** | **Yetki sınırı** | Başka kiracının kaydına erişilemez | A tenant'ı B'nin biletini göremez |
| **M-10** | **Durum geçişi** | Yalnız izinli geçişler | `Cancelled → Paid` reddedilmeli |

**M-8 özellikle kritik:** "Reddedildi" yeterli değil; **yan etki oluşmadığı** da doğrulanmalı.
Bu tam olarak DB checker'ın `AssertAbsent` yüzeyinin işidir.

### 5.4 Bunun için checker'da ne gerekiyor?

**Neredeyse hiçbir şey.** Gerekli parçaların tamamı `0.2.0-alpha.2`'de mevcut (K1):

| İhtiyaç | Karşılayan mevcut yetenek |
|---|---|
| Sayım ölçümü | `AssertCountAsync` → `ObservedRowCount` döner |
| Yokluk kanıtı | `AssertAbsentAsync` |
| Tekillik | `cardinality: exactly 1` |
| Tip-farkında karşılaştırma | `MatcherKindCodes` (`withinTolerance` dahil) |
| Çoklu kontrol tek çağrıda | `AssertBatchAsync` |
| Asenkron bekleme | `TimeoutMs` + `PollIntervalMs` |

**Eksik olan tek şey runner tarafında:** DB adımının sonucunu **senaryo değişkeni** olarak
taşıyabilmek (`outputs: bosKoltukOnce: observedRowCount`). Bu bir Test Module özelliğidir,
checker değişikliği değildir.

---

## 6. Doğal dilden senaryo üretimi — moment A'nın gerçek işi

### 6.1 Ajanın çözmesi gereken beş soru

İnsan *"A saatinde bilet varsa al"* dediğinde ajan şunları bulmalı:

| # | Soru | Kaynağı |
|---|---|---|
| 1 | "Bilet aramak" hangi operasyon? | İş sözlüğü + OpenAPI |
| 2 | "A saati" hangi parametre? | Operasyon şeması |
| 3 | Aramanın çıktısı satın almanın girdisine nasıl bağlanır? | **OpenAPI `links`** / şema analizi |
| 4 | "Bilet" hangi tabloda yaşıyor? | `db.binding.suggest` (FK grafiği + isim benzerliği) |
| 5 | Hangi iş değişmezleri kontrol edilmeli? | §5.3 kalıp kataloğu + tablo şekli |

3. madde için hazır bir teknik var: Schemathesis, OpenAPI `links`, `Location` başlıkları ve
şema analiziyle **üretici → tüketici** zincirleri kuruyor (K2). Aynı bilgi bizim
`SuggestOperationBindings` yüzeyimizin girdisidir.

### 6.2 Eksik parça: iş sözlüğü (business glossary)

Ajanın "bilet" kelimesinin ne olduğunu bilmesi gerekiyor. Bunu her seferinde keşfettirmek
hem pahalı hem hatalı.

**Çözüm:** İnsanın bir kez doldurduğu, ajanın sürekli kullandığı bir eşleme tablosu:

| Terim | Operasyonlar | Tablolar | Anahtar değişmezler |
|---|---|---|---|
| bilet | `searchFlights`, `purchaseTicket`, `getTicket` | `sales.Tickets` | M-2 delta, M-4 tekillik |
| koltuk | — | `sales.Seats` | M-1 korunum, M-7 monotonluk |
| ödeme | `capturePayment` | `billing.Payments` | M-3 tutarlılık, M-6 idempotans |

**Kazancı iki yönlü:** ajanın doğruluğu artar **ve** token düşer — çünkü keşif turları ortadan
kalkar. Bu, RESEARCH-0007'deki "bilgi katmanı" ilkesinin iş alanına uygulanmış hali.

### 6.3 İki aşamalı artefakt (Playwright deseni)

1. **İnsan-okur plan** (Markdown): *"Kullanıcı A saatinde bilet arar. Varsa satın alır.
   Doğrulanacak: koltuk 1 azaldı, tutar eşleşti, çift satış yok."*
2. **Makine-koşar senaryo** (Arazzo): §2.3'teki dosya.

İnsan onayı **birinci** artefakt üzerinde verilir — Markdown incelemek koddan ucuzdur.

---

## 7. Yolculuk (journey) ve durum geçişi doğrulaması

İş senaryolarının çoğu bir **durum makinesi** üzerinde yürür:

```
Draft ──► Reserved ──► Paid ──► Confirmed ──► Used
                │                    │
                └──► Expired         └──► Cancelled ──► Refunded
```

Senaryo iki şeyi doğrulamalı:
1. **İzinli geçiş gerçekleşti** — `Reserved → Paid` sonrası durum `Paid`
2. **Yasadışı geçiş reddedildi ve yan etki yok** — `Cancelled → Paid` denemesi 409 döner
   **ve** ödeme kaydı oluşmaz (M-8 + M-10)

Bu, senaryo dosyasında `expectedStateTransition` bloğu ve negatif yol adımlarıyla ifade edilir.

---

## 8. Zaman bağımlı senaryolar

"Yarın saat 10:00" gibi ifadeler sabit tarihe çevrilirse senaryo yarın çöpe gider.

**Karar:** Göreli zaman ifadeleri desteklenir (`now + 1d @ 10:00`), koşum anında çözülür ve
**çözülmüş değer koşum kaydına yazılır** (tekrar üretilebilirlik için). `HistoryId` hesabına
girmez — aksi halde her koşu ayrı trend kovasına düşer (RESEARCH-0008 S-01 kararıyla tutarlı).

---

## 9. Veri modeline etkiler

| Tablo | Ekleme | Gerekçe |
|---|---|---|
| `test_execution_statuses` | **`Inconclusive`** kodu | §3 |
| `scenario_executions` | `PreconditionStrategyCode`, `PreconditionSatisfied` bool | §4 |
| `scenario_executions` | `TakenBranchPath` (hangi dal koşuldu) | §2 karar noktası |
| `scenario_executions` | `ResolvedInputs` (owned json — çözülmüş göreli zamanlar) | §8 |
| `scenarios` | `ScenarioKindCode` (`TechnicalCheck` / `BusinessScenario`) | §2.4 |
| `scenario_step_bindings` | `InvariantPatternCode` (M-1..M-10) | §5.3, kapsam analizi |
| `step_results` | `StepOutputs` (owned json — sayısal ölçümler) | §5.4 delta assertion |
| `scenario_health` | **`InconclusiveRate`** | §3.3 |
| Yeni tablo | `business_glossary` (terim ↔ operasyon ↔ tablo ↔ değişmez) | §6.2 |
| Yeni lookup | `test_precondition_strategies` (`Arrange` / `Discover`) | §4 |
| Yeni lookup | `test_scenario_kinds` (`TechnicalCheck` / `BusinessScenario`) | §2.4 |
| Yeni step kind | `Branch`, `Precondition`, `Invariant` | §2.3 |

---

## 10. Bu eksik kapandığında ürün ne yapabiliyor olacak?

| Senaryo tipi | Örnek | Destekleniyor mu? |
|---|---|---|
| Teknik doğrulama | "`POST /orders` 201 dönmeli ve şemaya uymalı" | ✅ Zaten vardı |
| Kalıcılık kanıtı | "Sipariş veritabanına yazılmalı" | ✅ Zaten vardı |
| **Koşullu iş akışı** | "Bilet varsa al" | ✅ **Bu belgeyle** |
| **İş değişmezi** | "Koltuk tam 1 azalmalı" | ✅ **Bu belgeyle** |
| **Negatif iş kuralı** | "Koltuk yoksa satış reddedilmeli ve kayıt oluşmamalı" | ✅ **Bu belgeyle** |
| **Yolculuk** | "Ara → seç → rezerve et → öde → onayla" | ✅ **Bu belgeyle** |
| **Doğal dilden üretim** | "A saatinde bilet varsa al" cümlesinden senaryo | ✅ **Bu belgeyle** (moment A) |
| UI akışı | "Butona tıkla" | ❌ Kapsam dışı (bilinçli) |

---

## 11. Değişmeyen kararlar

Bu belge hiçbir temel kararı bozmuyor:

- **Koşumda model yok** — koşullu akış, önkoşul ve değişmezler deklaratiftir
- **Hakem checker'dır** — iş değişmezleri sayım/karşılaştırma; hakem yine deterministik
- **Checker yazmaz** — önkoşul verisi sandbox'ın işi
- **İnsan onaylar** — plan artefaktı üzerinde
- **Standart format** — hepsi Arazzo uzantısı, çatallama yok

---

## 12. Kaynaklar (bu belgeye özel; erişim 2026-08-12)

| Kaynak | Neyi kanıtlıyor | Sınıf |
|---|---|---|
| https://javiertroyauma.github.io/publications/TSE2017_REST_prePrint.pdf | REST API'ler için metamorphic testing; altı soyut ilişki kalıbı (MROP); oracle tek beklenen çıktı değil, çıktılar arası ilişkidir | K2 |
| https://dl.acm.org/doi/10.1145/3597926.3598114 (AGORA, ISSTA 2023) | REST API çalıştırmalarından dinamik değişmez çıkarımı; 105 değişmez tipi | K2 |
| https://arxiv.org/html/2605.28321v1 (ARMeta, 2026) | OpenAPI'den LLM çok-ajanlı metamorphic test çıkarımı; Gherkin Given-When-Then ifadesi | K2 |
| https://arxiv.org/pdf/2606.10465 (MASTOR) | RESTful API'ler için semantik test oracle üretimi | K2 |
| https://schemathesis.readthedocs.io/en/stable/explanations/stateful/ | OpenAPI `links` ile üretici→tüketici zinciri; durum makinesi | K2 |
| https://www.ontestautomation.com/on-false-negatives-and-false-positives/ | Yanlış negatiflerin yanlış pozitiflerden daha tehlikeli olması; yanlış güven | K3 |
| https://getautonoma.com/blog/useless-unit-tests-tautological-anti-pattern | Assertion'sız/totolojik testler; "sorun testin yokluğu değil yanlış güven" | K3 |
| https://testsigma.com/blog/precondition-in-test-case/ | Önkoşul/sonkoşul kavramı: başlangıç durumu ve doğrulanacak sonuç | K3 |
| https://www.headspin.io/blog/test-scenarios-comprehensive-guide | Senaryo = *ne* test edilir (iş süreci); test durumu = *nasıl* | K3 |
| https://arxiv.org/pdf/2604.13559 (WebMAC, 2026) | Web sistemleri için çok-ajanlı senaryo testi çerçevesi | K2 |
</content>
