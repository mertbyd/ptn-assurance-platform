---
id: RESEARCH-0010
type: research
status: draft
title: Is bilgisinin ajana aktarimi — dort bilgi katmani, kural katalogu ve iliski grafigi
updated: 2026-08-12
decision_refs:
  - ADR-0007
  - ADR-0008
rule_refs:
  - RULE-0004
---

# İş bilgisinin ajana aktarımı

> Kanonik değildir. RESEARCH-0009 "iş senaryosu nasıl ifade edilir" sorusunu çözdü.
> Bu belge şunu çözer: **ajan iş kurallarını ve API uçlarının birbiriyle ilişkisini
> nereden öğrenecek?**
>
> Somut sorular: *"Bilet aldık ama araçtaki yolcu sayısı düşmedi"* nasıl test edilir?
> *"Öğrenci bileti hakkı 2, birini kullandık"* kuralı ajana nasıl aktarılır?
>
> Kanıt sınıfları: **K1** çalışan kod · **K2** birincil/akademik · **K3** sektör ölçümü.

---

## 0. Kanıtla başlayan tez

**APITestGenie** (AST 2026, K2) tam bu problemi ölçtü: **iş gereksinimleri + OpenAPI**
dokümanını RAG ile birleştirip test üretti. 10 gerçek API, biri otomotiv sektöründen
~1.000 canlı endpoint. Sonuç: **%89 oranında elle düzeltme gerektirmeyen, sözdizimsel ve
anlamsal olarak geçerli test**, en fazla üç denemede. Üstelik üretilen testler
**endpoint'ler arası entegrasyon hataları dahil** daha önce bilinmeyen kusurlar buldu.

Aynı çalışmanın belirlediği başarı faktörleri şunlar:

1. API karmaşıklığı
2. **İş gereksinimlerinin ayrıntı düzeyi**
3. API dokümantasyonunun ayrıntı düzeyi

**Yani sorunun cevabı ölçülmüş durumda:** evet, ajana doküman vereceğiz — ve
**o dokümanın ayrıntı düzeyi başarı oranını doğrudan belirliyor.**

Karşı kanıt da net: RAGcceptance M2RE çalışması, yönlendirilmemiş bir LLM'in
*"paydaşların belirtmediği koşulları uydurabileceğini"* ve belirsiz bir gereksinimde
*"boşlukları kendi varsayımlarıyla doldurup makul görünen ama gerçek niyeti yansıtmayan
kriterler üreteceğini"* raporluyor (K2). RAFT çalışması ise örtük bilgiyi
**yapılandırılmış artefaktlara** dönüştürmenin gerekliliğini gösteriyor ve başarının
*"bu dış bilgi kaynaklarının kalitesine, eksiksizliğine ve temsil gücüne"* bağlı
olduğunu söylüyor (K2).

**Sonuç:** serbest metin yetmez. **Yapılandırılmış** bilgi gerekir.

---

## 1. Dört bilgi katmanı

İş bilgisi tek bir yerden gelmez. Dört kaynağı var; maliyetleri ve otoriteleri farklı.
Doğru tasarım dördünü de kullanır ve **her öğrenileni kalıcı hale getirir**.

| Katman | Kaynak | Maliyet | Otorite | Örnek |
|---|---|---|---|---|
| **K-1 Türetilebilir** | OpenAPI şeması, `links`, FK grafiği, kısıtlar, tekil indeksler | **Sıfır** | Kesin | `Tickets.TripId → Trips.Id` |
| **K-2 Gözlemlenebilir** | İşlemi bir kez koşup **etkisini** ölçmek | Düşük | Olasılıksal — insan onaylar | "`purchaseTicket` çalışınca `Trips.AvailableSeats` 1 azalıyor" |
| **K-3 Beyan edilen** | İnsanın yazdığı sözlük + kural kataloğu | Yüksek | **Otoriter** | "Öğrenci bileti hakkı dönem başına 2" |
| **K-4 Etkileşimli** | Ajan emin değilse sorar | Nokta atışı | Otoriter | "`Seats.Status` alanında `Reserved` da boş sayılır mı?" |

**Altın kural:** K-2 ve K-4 ile öğrenilen her şey **K-3'e yazılır**. Aynı şey iki kez
keşfedilmez, iki kez sorulmaz.

---

## 2. K-1 — Bedava bilgi: neyi zaten biliyoruz

İki checker `0.2.0-alpha.2` ile şunları **hâlihazırda** verebiliyor (K1):

| Bilgi | Nereden | Ne söyler |
|---|---|---|
| Operasyon listesi, parametreler, yanıt şeması | API checker snapshot | "`searchFlights` `departAt` alır, `items[].id` döner" |
| **OpenAPI `links`** | Spec | "`searchFlights` çıktısı `purchaseTicket` girdisine bağlanır" |
| Yabancı anahtar grafiği | DB checker şema keşfi | `Tickets.TripId → Trips.Id → Vehicles.Id` |
| Birincil/tekil anahtarlar | DB checker | "`Tickets` üzerinde `(TripId, SeatNo)` tekil" |
| Kısıtlar ve **doğrulanmışlık durumu** | DB checker | "`FK_Ticket_Trip` var ama `NOT VALID`" |
| Kolon tipi, nullable, generated | DB checker | "`AvailableSeats` int, generated değil" |
| Tablo/operasyon eşleme önerisi | `SuggestOperationBindings`, `db.binding.suggest` | "`purchaseTicket` muhtemelen `sales.Tickets`" |

### 2.1 API uçlarının ilişkisi nasıl çözülür — beş kaynak

Sorunun bu kısmının cevabı katmanlı:

| # | Yöntem | Kesinlik | Not |
|---|---|---|---|
| 1 | **OpenAPI `links`** | Kesin | Spec'te varsa doğrudan okunur; en ucuz yol |
| 2 | **Şema analizi (üretici→tüketici)** | Yüksek | `searchFlights` yanıtı `id` döner, `purchaseTicket` isteği `flightId` ister → ad+tip eşleşmesi. Schemathesis bunu böyle yapıyor (K2) |
| 3 | **`Location` başlığı** | Yüksek | `201` yanıtındaki `Location`, oluşan kaynağın adresini verir |
| 4 | **FK grafiği** | Kesin (DB tarafı) | Tablolar arası ilişki |
| 5 | **Gözlenen etki ayak izi** | Olasılıksal | §3 |

Bunların birleşimi bir **operasyon ilişki grafiği** olarak saklanır. Bilgi grafiği
literatürü şunu uyarıyor: çıkarım artık LLM ile ucuz, asıl zorluk **varlık çözümleme ve
ontoloji hizalaması** (K3). Bu yüzden grafik **insan onayıyla** kesinleşir.

### 2.2 K-1'in sınırı

K-1 sana **yapıyı** verir, **kuralı** vermez.

`Tickets.TripId → Trips.Id` ilişkisini görür ama *"her bilet bir koltuk tüketir"*
kuralını **bilemez**. Senin ilk örneğin (*"bilet aldık ama yolcu sayısı düşmedi"*)
tam bu boşluktadır.

---

## 3. K-2 — Gözlemlenebilir bilgi: **etki ayak izi**

### 3.1 Fikir

Bir işlemin ne yaptığını öğrenmenin en kesin yolu **onu çalıştırıp bakmaktır**.

```
1. Sandbox'ta DB durumunu olcumle           (once)
2. purchaseTicket operasyonunu bir kez cagir
3. DB durumunu tekrar olcumle               (sonra)
4. Farki cikar  ->  ETKI AYAK IZI
```

Örnek çıktı:

```
purchaseTicket etki ayak izi:
  sales.Tickets           +1 satir
  sales.Trips             1 satir guncellendi:  AvailableSeats  42 -> 41
  billing.Payments        +1 satir
  notify.Outbox           +1 satir
```

**Ajanın önüne bu tablo konur ve şu sorulur:**
> "Bu etkiler değişmez (invariant) olarak yazılsın mı?"

İnsan onaylar, kural kataloğuna yazılır, bir daha keşfedilmez.

### 3.2 Akademik temeli

Bu, **Daikon**'un dinamik değişmez çıkarımının bizim alanımıza uygulanmış hali.
**AGORA+** aynısını REST API'ler için yapıyor: operasyon seviyesinde
**olası önkoşullar** (girdi parametrelerinin sağladığı özellikler) ve
**olası sonkoşullar** (yanıtların tutarlı biçimde sağladığı özellikler) çıkarıyor;
Daikon'a girdi hazırlayan `Beet` adlı bir ön-uç kullanıyor (K2).
AGORA ailesi REST API'lerde **105 farklı değişmez tipini** tespit edebiliyor (K2).

**Bizim avantajımız:** AGORA yalnız **HTTP yanıtına** bakabiliyor.
Bizim Database Checker'ımız **veritabanının kendisine** bakabiliyor — yani
yanıtta görünmeyen yan etkileri de görüyoruz. `notify.Outbox` satırı hiçbir API
yanıtında görünmez ama bizim ayak izimizde görünür.

### 3.3 Bu bizde neden ucuz?

Gerekli parçaların tamamı mevcut (K1):

| İhtiyaç | Mevcut yetenek |
|---|---|
| Öncesi/sonrası şema+veri karşılaştırması | Database Checker karşılaştırma motoru |
| Satır sayısı ölçümü | `AssertCountAsync` → `ObservedRowCount` |
| Değişen kolonların tespiti | Veri farkı bulguları (`RowDifferences` / `ValueDifferences`) |
| Salt-okunur emniyet | Bağlantı emniyet profili |
| Test verisi izolasyonu | `ITestDataSandbox` |

**Yeni yazılacak tek şey:** bu adımları sıraya dizen bir "etki ayak izi çıkarma" akışı.
Yeni motor değil, mevcut motorların yeni bir dizilişi.

### 3.4 Sınırları — dürüstçe

- **Olasılıksaldır.** Bir kez gözlemlenen etki her zaman geçerli olmayabilir
  (koşula bağlı yan etkiler). Bu yüzden çıktı **öneri**dir, kural değil.
- **Negatif kuralları bulamaz.** "Kota dolduğunda reddedilmeli" kuralını gözlemleyerek
  öğrenmek için önce kotayı doldurman gerekir — bu bir keşif değil, zaten bilinen kuralın testidir.
- **Gürültü üretir.** `UpdatedAt`, `LastModifiedBy` gibi denetim alanları her işlemde değişir;
  bunlar filtrelenmelidir.

Bu yüzden K-2 **tek başına yeterli değildir** ve K-3'ü ortadan kaldırmaz.

---

## 4. K-3 — Beyan edilen bilgi: "md mi vereceğiz?"

**Evet — ama serbest metin değil, yapılandırılmış markdown.**

Serbest metin RAG'e verilebilir ama garantisi yoktur; kanıt bölümünde (§0) LLM'in
belirtilmemiş koşulları uydurduğu raporlanıyor. Yapılandırılmış markdown ise
**hem insan okur hem makine ayrıştırır**.

Üç dosya tipi öneriyoruz.

### 4.1 Dosya 1 — İş sözlüğü (`domain/glossary.md`)

Terimi teknik gerçekliğe bağlar. **Ubiquitous language** pratiğinin dosya hali:
iş, test ve kod aynı terimleri kullanır (K3).

```markdown
---
kind: glossary
version: 3
---

## bilet
- **Tanım:** Bir yolcunun belirli bir seferdeki koltuk hakkı.
- **Operasyonlar:** `searchFlights`, `purchaseTicket`, `getTicket`, `cancelTicket`
- **Tablolar:** `sales.Tickets` (ana), `sales.TicketHistory` (arşiv)
- **Kimlik:** `Tickets.Id`
- **Durumlar:** `Reserved → Paid → Confirmed → Used | Cancelled`
- **Beklenen değişmezler:** M-2 (delta), M-4 (tekillik), M-6 (idempotans)

## koltuk
- **Tanım:** Bir araçtaki fiziksel yer.
- **Tablolar:** `sales.Seats`, sayaç: `sales.Trips.AvailableSeats`
- **Kural bağı:** her onaylı bilet **tam 1** koltuk tüketir
- **Beklenen değişmezler:** M-1 (korunum), M-7 (monotonluk)
```

**Bu dosya senin birinci örneğini çözer.** *"Bilet aldık ama yolcu sayısı düşmedi"*
testini ajanın yazabilmesi için bilmesi gereken tek şey, sözlükteki şu satırdır:
**"her onaylı bilet tam 1 koltuk tüketir"**.

### 4.2 Dosya 2 — İş kuralı kataloğu (`domain/rules/*.md`)

Her kural bir dosya. **Karar tablosu** içerir.

```markdown
---
kind: business-rule
id: BR-014
title: Ogrenci bileti kotasi
scope: donem
appliesTo: [ purchaseTicket ]
errorCode: StudentQuotaExceeded
---

## Kural

Öğrenci indirimli bilet, bir öğrenci için **dönem başına en fazla 2 adet** alınabilir.
Kota dolduğunda satın alma reddedilir ve **hiçbir yan etki oluşmaz**.

## Karar tablosu

| Öğrenci belgesi | Dönem içi alınan | Sonuç | Hata kodu |
|---|---|---|---|
| yok | * | normal fiyat | — |
| var | 0 | **izin ver** (indirimli) | — |
| var | 1 | **izin ver** (indirimli) | — |
| var | 2 | **reddet** | `StudentQuotaExceeded` |
| var | >2 | tutarsız durum — alarm | `QuotaStateCorrupt` |

## Sayaç kaynağı

`sales.Tickets` içinde `IsStudentFare = true` ve `TermId = <aktif dönem>` satır sayısı.

## Sınır değerler

`0 → izin`, `1 → izin`, **`2 → red`** (asıl sınır), `3 → tutarsızlık`

## Reddedildiğinde doğrulanacaklar

- HTTP 409 + `StudentQuotaExceeded`
- `sales.Tickets` satır sayısı **değişmedi** (M-8)
- `billing.Payments` satırı **oluşmadı** (M-8)
```

**Bu dosya senin ikinci örneğini çözer** ve daha fazlasını: karar tablosundan
**sınır değer testleri otomatik türetilir**. Ajanın "kaç kere alınabilir" diye tahmin
etmesine gerek kalmaz.

**Neden karar tablosu?** Karar tablosu, iş kurallarını makine-okunur ifade etmenin
endüstri standardıdır (OMG **DMN**: karar tabloları + FEEL ifade dili, K2). DMN'in
kendi literatürü şunu da söylüyor: *"endüstriyel araçlarda doğrulama yeteneklerinin
kapsamı endişe verici derecede düşük"* — yani karar tablosundan test üretmek
**bizim doldurabileceğimiz gerçek bir boşluk**.

### 4.3 Dosya 3 — Yolculuk / durum makinesi (`domain/journeys/*.md`)

```markdown
---
kind: journey
id: JR-002
title: Bilet yasam dongusu
---

## Durumlar ve gecisler

| Kaynak | Hedef | Tetikleyen | Ön koşul |
|---|---|---|---|
| Reserved | Paid | `capturePayment` | rezervasyon süresi dolmamış |
| Paid | Confirmed | otomatik | ödeme onayı geldi |
| Confirmed | Used | `checkIn` | sefer saati geldi |
| Confirmed | Cancelled | `cancelTicket` | seferden 2 saat önce |
| Cancelled | Refunded | otomatik | iade politikası |

## Yasak gecisler (test edilmeli)

- `Cancelled → Paid` → **red**, `InvalidStateTransition`, yan etki yok
- `Used → Cancelled` → **red**, `TicketAlreadyUsed`, yan etki yok
```

### 4.4 Bu dosyalar nerede yaşar?

**İkisi de:**

| Yer | Rol |
|---|---|
| **Repo / Git** (`domain/*.md`) | **Kaynak.** İnsan burada düzenler, PR ile inceler, sürümlenir |
| **Veritabanı** (`business_glossary`, `business_rules`) | **Türev.** Ajan buradan sorgular; kapsam ölçülür; senaryo bağlantısı kurulur |

Yükleme, senaryolarla aynı desenle çalışır: içerik hash'lenir, değişmemişse yeniden
yazılmaz (`SpecContent` deseni, K1).

**Neden veritabanına da:** ajanın 40 sayfalık markdown'ı bağlamına alması gerekmemeli.
"BR-014 kuralını getir" diye sorgulayabilmeli. Bu, RESEARCH-0007'deki
"karar döndür, ham veri değil" ilkesinin iş bilgisine uygulanması.

---

## 5. K-4 — Etkileşimli sorgu: ajan emin değilse sorar

Bazı bilgiler ne türetilebilir ne gözlemlenebilir ne de önceden yazılmıştır.

**Ajanın davranışı:** tahmin etmez, **sorar**. MCP protokolünde bunun karşılığı
`input_required` durumudur (RESEARCH-0006 §10).

Örnek:
> *"`Seats.Status` kolonunda `Reserved` değeri var. Boş koltuk sayarken `Reserved`
> koltuklar dahil edilmeli mi?
> (a) hayır, yalnız `Available`  (b) evet, `Available + Reserved`"*

**Kritik kural:** verilen cevap **kural kataloğuna yazılır**. Aynı soru ikinci kez sorulmaz.
Bu, hem token tasarrufu hem kurumsal hafızadır.

---

## 6. Senin iki örneğin — uçtan uca

### 6.1 "Bilet aldık ama araçtaki yolcu sayısı düşmedi"

| Adım | Hangi katman | Ne olur |
|---|---|---|
| 1 | K-1 | FK grafiği: `Tickets → Trips → Vehicles`; `Trips.AvailableSeats` int kolonu |
| 2 | K-2 | Etki ayak izi: `purchaseTicket` sonrası `Trips.AvailableSeats` 42→41 gözlendi |
| 3 | K-3 | Sözlükte doğrulanır: *"her onaylı bilet tam 1 koltuk tüketir"* |
| 4 | Üretim | Ajan **M-2 (delta)** kalıbını uygular |

Üretilen senaryo adımı:

```yaml
- stepId: koltukSayaciDustu
  x-checknexus-db:
    operation: assertRow
    schema: sales ; table: Trips
    key: { Id: "{$steps.seferAra.outputs.ilkSeferId}" }
    outputs: { kalanKoltuk: AvailableSeats }
  successCriteria:
    - condition: "$steps.oncekiDurum.outputs.kalanKoltuk
                  - $steps.koltukSayaciDustu.outputs.kalanKoltuk == 1"
```

**Ve asıl kazanç:** bu senaryo bir kez yazıldığında, ileride biri "sayaç güncelleme"
kodunu bozarsa test **kırmızı** döner. Bugün bu hata canlıya çıkar ve müşteri bulur.

### 6.2 "Öğrenci bileti hakkı 2, birini kullandık"

| Adım | Hangi katman | Ne olur |
|---|---|---|
| 1 | K-3 | `BR-014` kural dosyası okunur: kota 2, sayaç kaynağı, hata kodu |
| 2 | K-1 | `purchaseTicket` şeması: `fareType` parametresi var |
| 3 | Üretim | Karar tablosundan **dört senaryo** türetilir |

Türetilen senaryolar:

| # | Önkoşul | Eylem | Beklenen |
|---|---|---|---|
| 1 | öğrenci, 0 bilet | indirimli al | ✅ 201, sayaç 1 olur |
| 2 | öğrenci, 1 bilet | indirimli al | ✅ 201, sayaç 2 olur |
| 3 | **öğrenci, 2 bilet** | indirimli al | ❌ **409 `StudentQuotaExceeded`** + `Tickets` sayısı değişmez + `Payments` oluşmaz |
| 4 | öğrenci değil | indirimli al | ❌ red veya normal fiyat (kurala göre) |

**3 numaralı senaryo asıl değerli olandır** ve karar tablosundaki sınır satırından
otomatik doğar. İnsanın "sınır testini de yazayım" diye hatırlaması gerekmez.

Önkoşullar `Arrange` stratejisiyle sandbox'ta kurulur (RESEARCH-0009 §4):
2 numaralı senaryo için "1 öğrenci bileti olan öğrenci" veri kümesi yaratılır.

---

## 7. Kural kapsamı — piyasada olmayan metrik

Bilgi yapılandırılmış olduğu için şu soru **cevaplanabilir** hale gelir:

> *"Kaç iş kuralımız var ve kaçı test ediliyor?"*

```
Is kurali kapsami
  BR-011  Iptal politikasi        ✅ 3 senaryo
  BR-012  Yas indirimi            ✅ 2 senaryo
  BR-013  Grup rezervasyonu       ⚠️  1 senaryo (yalniz mutlu yol)
  BR-014  Ogrenci kotasi          ✅ 4 senaryo (sinir dahil)
  BR-015  Iade suresi             ❌ 0 senaryo
```

Bu, satır kapsamından (**line coverage**) çok daha anlamlı bir metriktir: satır kapsamı
"kod çalıştı mı" der, kural kapsamı **"iş kuralı doğrulandı mı"** der.

DMN literatürünün *"doğrulama kapsamı endüstriyel araçlarda endişe verici derecede düşük"*
tespiti (K2) bu metriğin neden nadir olduğunu açıklıyor — çünkü kuralların makine-okunur
olmasını gerektiriyor. Bizde olacak.

`scenario_step_bindings.InvariantPatternCode` ve yeni `RuleRef` alanı bu raporu üretir.

---

## 8. Bilgi olgunluk seviyeleri

Bir ekip bu sistemi kademeli benimseyebilmeli. Sıfırdan mükemmel katalog beklemek gerçekçi değil.

| Seviye | Ekipte olan | Ajan ne yapabilir | Beklenen kalite |
|---|---|---|---|
| **L0** | Sadece OpenAPI | Teknik doğrulama, mutlu yol | Düşük — iş kuralı bilinmiyor |
| **L1** | + FK grafiği (otomatik) | Kalıcılık kanıtı, temel delta önerileri | Orta |
| **L2** | + etki ayak izi (K-2) | Yan etki değişmezleri, gerçek entegrasyon testi | İyi |
| **L3** | + iş sözlüğü (K-3) | Doğru terim, doğru tablo, doğru değişmez kalıbı | İyi–yüksek |
| **L4** | + kural kataloğu | **Sınır ve negatif senaryolar**, kural kapsamı raporu | Yüksek |
| **L5** | + yolculuk tanımları | Durum geçişi ve yasak geçiş testleri | Tam |

**Ürün L0'da da çalışmalı**, ama panelde ekibe *"L2'desiniz; kural kataloğu eklerseniz
şu 7 senaryo türü açılır"* demeli. Bu, hem dürüst hem de benimsemeyi yönlendiren bir tasarımdır.

---

## 9. Ajanın bilgi tüketim akışı (moment A)

```
Insan: "A saatinde bilet varsa al"
   │
   ├─ 1) Sozlukten "bilet" cozulur           -> operasyonlar + tablolar + degismezler   (K-3)
   ├─ 2) Ilgili kural dosyalari cekilir      -> BR-014 gibi                              (K-3)
   ├─ 3) Operasyon ozeti alinir              -> parametreler, yanit semasi               (K-1)
   ├─ 4) Iliski grafigi okunur               -> search.id -> purchase.flightId           (K-1)
   ├─ 5) Etki ayak izi varsa okunur          -> hangi tablolar degisiyor                 (K-2)
   ├─ 6) Belirsizlik varsa SORULUR           -> input_required                           (K-4)
   │
   ├─ 7) Markdown PLAN uretilir              -> insan onaylar
   └─ 8) Arazzo senaryosu uretilir           -> dogrulanir, kuru kosulur, kaydedilir
```

**Token disiplini:** hiçbir adımda tam doküman bağlama girmez. Sözlükten **tek terim**,
kataloğdan **tek kural**, spec'ten **tek operasyon özeti** çekilir
(RESEARCH-0007 "dar yanıt" ilkesi).

---

## 10. Veri modeline eklemeler

| Tablo | İçerik |
|---|---|
| `business_glossary` | Terim, tanım, operasyon listesi, tablo listesi, kimlik kolonu, durum kümesi, beklenen değişmez kalıpları |
| `business_rules` | `RuleRef` (BR-014), başlık, kapsam, uygulandığı operasyonlar, hata kodu, karar tablosu (owned json), sınır değerler |
| `business_journeys` | Durum kümesi, izinli geçişler, yasak geçişler, tetikleyen operasyonlar |
| `operation_links` | Kaynak operasyon → hedef operasyon, bağlanan alan, keşif yöntemi (`SpecLink`/`SchemaMatch`/`LocationHeader`/`Observed`/`Declared`), güven derecesi, onay durumu |
| `effect_footprints` | Operasyon, gözlem koşusu, etkilenen tablo/kolon, değişim yönü, onay durumu |
| `knowledge_contents` | Tüm bilgi dosyalarının içerik-adresli saklanması (`SpecContent` deseni) |
| `scenario_step_bindings` | **+ `RuleRef`** — hangi adım hangi iş kuralını test ediyor |
| `scenario_health` | **+ kural kapsam metrikleri** |

---

## 11. Değişmeyen kararlar

- **Koşumda model yok** — bilgi katmanları yalnız **yazım** anında okunur
- **Hakem checker'dır** — kural katalogu neyin doğrulanacağını söyler, doğrulamayı checker yapar
- **İnsan onaylar** — K-2 önerileri ve K-4 cevapları katalogda kalıcılaşmadan önce
- **Standart** — karar tablosu DMN, senaryo Arazzo, yama Overlay
- **Checker değişikliği yok** — K-1 ve K-2 mevcut yeteneklerle karşılanıyor

---

## 12. Kaynaklar (bu belgeye özel; erişim 2026-08-12)

| Kaynak | Neyi kanıtlıyor | Sınıf |
|---|---|---|
| https://dl.acm.org/doi/full/10.1145/3793654.3793743 (APITestGenie, AST 2026) | İş gereksinimi + OpenAPI + RAG → 10 API / ~1.000 endpoint'te **%89** elle düzeltmesiz geçerli test; endpoint'ler arası entegrasyon hataları bulundu; başarı **gereksinim ayrıntı düzeyine** bağlı | K2 |
| https://arxiv.org/pdf/2508.06888 (RAGcceptance M2RE) | Domain artefaktlarından RAG ile kabul kriteri üretimi; yönlendirilmemiş LLM'in belirtilmemiş koşulları uydurması | K2 |
| https://arxiv.org/html/2601.09762 (RAFT) | Örtük bilginin **yapılandırılmış artefaktlara** çıkarılması; başarının kaynak kalitesine bağlılığı | K2 |
| https://personales.us.es/sergiosegura/files/papers/alonso25-tosem.pdf (AGORA+/Beet, TOSEM) | Operasyon seviyesinde **olası önkoşul ve sonkoşul** çıkarımı; Daikon ön-ucu | K2 |
| Daikon (dinamik değişmez tespiti) | Çalıştırma izlerinden önkoşul/sonkoşul/değişmez öğrenme | K2 |
| https://www.omg.org/spec/DMN/ | Karar tabloları + FEEL; makine-okunur iş kuralı standardı | K2 |
| DMN karar tablosu analizi literatürü | *"Endüstriyel araçlarda doğrulama kapsamı endişe verici derecede düşük"* | K2 |
| https://schemathesis.readthedocs.io/en/stable/explanations/stateful/ | OpenAPI `links` + şema analizi + `Location` ile üretici→tüketici zinciri | K2 |
| https://cucumber.io/docs/gherkin/reference/ | `Rule` anahtar kelimesi: bir iş kuralına ait senaryoları gruplama; ubiquitous language | K3 |
| Specification by Example / living documentation pratiği | İş, test ve kodun **aynı terimleri** kullanması; örneklerin hem şartname hem test olması | K3 |
| https://atlan.com/know/ai-agent/knowledge-graph/knowledge-graph-construction-for-ai/ | Çıkarım LLM ile ucuz; asıl zorluk **varlık çözümleme ve ontoloji hizalama** | K3 |
</content>
