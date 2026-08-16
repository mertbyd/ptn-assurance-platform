---
id: GUIDE-0004
type: guide
status: active
title: Ekip kilavuzu — urun, yetenekler, is senaryosu testi ve merkezi karar sicili
updated: 2026-08-16
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

# Ekip kılavuzu

Bu belge projeyi **hiç bilmeyen** birinin okuyup anlaması için yazıldı. Terimleri kullanmadan
önce tanımlıyor. Stajyer, test ekibi, frontend ekibi ve backend ekibi aynı belgeyi okuyabilir.

## Nasıl okunur

| Rolün | Oku |
|---|---|
| Yeni başlayan / stajyer | §0 → §1 Sözlük → §3 İki tür test → §4 Örnek → §14 SSS |
| Test ekibi | §0 → §1 → §3 → §4 → §8 İş bilgisi → §9 Değişmez kataloğu → §10 Olgunluk |
| Frontend ekibi | §0 → §2 → §5/§6'nın "dışarıya ne verir" satırları → §7 → §13.1 |
| Backend ekibi | Tamamı; özellikle **§11 Merkezi karar sicili** |
| Yönetici / karar verici | §0 → §3 → §10 Olgunluk → §11 karar sicili → §12 sorular |

---

# 0. Bu ürün nedir?

## 0.1 Yanlış anlaşılmaması gereken şey

**Bu proje teknik test aracı değildir.**

"HTTP 201 döndü mü", "yanıt şemaya uyuyor mu" gibi teknik doğrulamalar bu ürünün
**tabanıdır, amacı değildir**. O kadarını iki checker'ımızla, hiçbir yapay zekâ
olmadan da yapabiliriz.

## 0.2 Ürünün gerçek amacı

**İş senaryolarını test etmek.** İnsanın iş diliyle söylediği şeyi çalıştırılabilir
teste çevirmek:

> *"Yarın saat 10:00'da İstanbul–Ankara seferinde bilet varsa satın al ve gerçekten
> satın alındığını kanıtla."*
>
> *"Öğrenci indirimli bilet hakkı dönem başına 2'dir. İkisini kullanmış bir öğrenci
> üçüncüyü alamamalı — ve reddedildiğinde hiçbir yan etki oluşmamalı."*
>
> *"Bilet satıldığında araçtaki boş koltuk sayısı tam 1 azalmalı."*

Bu üç cümlenin hiçbiri "şema doğrulaması" değildir. Üçü de **iş kuralıdır** ve
üçü de bugün canlıya çıkıp müşteri tarafından bulunan hata türleridir.

## 0.3 Farkımız

| Piyasa | Biz |
|---|---|
| Test kırılınca **tahmin eder**: "sanırım locator değişti" | İki motor **söyler**: "`POST /orders` yanıt şeması değişti, fingerprint `a1b2`" |
| Yapay zekâ her koşuda çalışır (~114.000 token/test) | Yapay zekâ **yalnız yazarken** çalışır, koşumda **0 token** |
| Hakem model olur (kararı prompt'a göre değişir) | Hakem **deterministik motor**, model asla |
| "Yeşil" = geçti sayılır | "Hiçbir şey doğrulanmadı" ayrı bir sonuçtur (`Inconclusive`) |
| Sadece API yanıtını görür | **Veritabanının kendisini** görür — yanıtta görünmeyen yan etkileri de |

## 0.4 Tek cümleyle

> **İnsanın iş diliyle anlattığı senaryoyu, yapay zekâ bir kez çalıştırılabilir
> artefakta çevirir; o artefakt sonsuza kadar sıfır token ile, deterministik hakemlerle koşar;
> uygulama değiştiğinde hangi senaryonun bozulduğunu tahmin etmeden, indeksten okuyarak bulur.**

---

# 1. Sözlük — terimleri sıfırdan

Bu bölümü atlamayın. Aşağıdaki kelimeler belgenin geri kalanında sürekli geçiyor.

## 1.1 API dünyası

**API** — Bir yazılımın başka yazılımlara açtığı kapı. "Sipariş oluştur", "bilet ara" gibi
işlemleri HTTP üzerinden sunar.

**Endpoint / operasyon** — API'nin tek bir kapısı. `POST /tickets` gibi.

**OpenAPI dokümanı (spec)** — API'nin makine-okunur kullanım kılavuzu. Kısaca **sözleşme**.

**Sözleşme (contract)** — API'nin verdiği söz. Değişirse onu kullanan herkes etkilenir.

**JSON şeması** — Bir JSON verisinin nasıl görünmesi gerektiğini tanımlayan kural seti.

> ⚠️ **"Şema" kelimesi iki anlamda kullanılıyor:** JSON şeması ≠ veritabanı şeması (§1.2).
> Belgede hangisi kastediliyorsa açıkça yazıyoruz.

## 1.2 Veritabanı dünyası

**Veritabanı şeması** — Tabloları gruplayan isim alanı. `sales.Tickets`'ta `sales` şemadır.

**Birincil anahtar (PK)** — Bir satırı tekil bulmaya yarayan kolon. İki satırın aynı PK'sı olamaz.

**Tekil indeks (unique)** — PK olmayan ama tekrar edemeyen kolon.

**Yabancı anahtar (FK)** — Bir satırın başka tablodaki satıra işaret etmesi.
`Tickets.TripId → Trips.Id` gibi. **İş ilişkilerini keşfetmenin ana kaynağıdır.**

**Migration** — Veritabanı yapısını değiştiren, sırayla çalışan betikler.

**Collation** — Metin karşılaştırma kuralı. İki ortamda farklıysa aynı sorgu farklı sonuç verir.

## 1.3 Test dünyası

**Assertion (beklenti)** — "Şunun şöyle olmasını bekliyorum" ifadesi.

**Oracle (hakem)** — Bir beklentinin doğru mu yanlış mı olduğuna karar veren şey.
Bizde hakem **her zaman** checker'dır, asla yapay zekâ değildir.

**Matcher** — Karşılaştırma biçimi: `equals`, `withinTolerance`, `oneOf`, `matchesRegex`.
Yanlış matcher seçimi kırılganlığın en büyük sebeplerinden biridir.

**Cardinality** — "Kaç satır olmalı": `tam 1`, `en az 1`, `hiç yok`.

**Flaky test** — Kod değişmeden bazen geçen bazen kalan test.

**Karantina** — Kararsız testi kapatmak yerine "koşsun ama build'i kırmasın" durumuna almak.

**Triyaj** — Kırmızı bir testin **neden** kırmızı olduğunu bulma işi. Sektörde medyan 28 dakika.

### **Test durumu ≠ İş senaryosu** — bu ayrımı bilin

| | Test durumu (test case) | **İş senaryosu (scenario)** |
|---|---|---|
| Odak | *Nasıl* test edilir | ***Ne* test edilir** |
| Kapsam | Tek fonksiyon | Uçtan uca iş süreci |
| Örnek | "`POST /tickets` çağır, 201 bekle" | "Kullanıcı A saatinde bilet bulup alabilmeli" |

**Bu ürünün var oluş sebebi sağ sütundur.**

### **Önkoşul (precondition)**
Senaryonun çalışabilmesi için sağlanması gereken başlangıç durumu.
*"A saatinde bilet olmalı"* bir önkoşuldur. İki strateji: **`Arrange`** (veriyi biz yaratırız)
veya **`Discover`** (canlıdan arayıp buluruz).

### **`Inconclusive` (sonuçsuz)**
Önkoşul sağlanmadığı için ana yolun hiç koşmadığı durum. **Yeşil değildir** —
çünkü hiçbir şey doğrulanmamıştır. Kırmızı da değildir — çünkü hata bulunmamıştır.

### **İş değişmezi (business invariant)**
İşlem sonrası her zaman doğru kalması gereken iş kuralı.
*"Bilet satıldığında boş koltuk tam 1 azalır"* bir değişmezdir. Katalog için bkz. §9.

### **Karar tablosu (decision table)**
Bir iş kuralının tüm girdi-çıktı kombinasyonlarını satır satır yazan tablo.
Sınır değer testleri buradan **otomatik** türetilir.

### **Etki ayak izi (effect footprint)**
Bir operasyonun veritabanında **hangi tabloları nasıl değiştirdiğinin** gözlenmiş kaydı.
"Bilet al" çalışınca `Tickets` +1, `Trips.AvailableSeats` −1, `Payments` +1, `Outbox` +1.

## 1.4 Checker dünyası

**Checker** — Kontrolcü. İki tanemiz var. **Soru cevaplayan** modüllerdir; iş yapmazlar.

**Capability module** — Tek başına uygulama değil, başka uygulamanın içine takılan parça.

**Snapshot** — Bir dokümanın belirli andaki hali.

**Run (koşu)** — Karşılaştırmayı bir kez çalıştırma kaydı.

### **Finding (bulgu)** — en çok geçen terim

**Bir karşılaştırma sonucunda bulunan tek bir farkın kaydıdır.**

Günlük dille: iki resmi yan yana koyup "şurası farklı" dediğinde, işaret ettiğin
**her bir** fark bir finding'dir.

| Alan | Anlamı | Örnek |
|---|---|---|
| **Kind** | Ne tür fark | `OnlyInSource`, `OnlyInTarget`, `Modified` |
| **Severity** | Ne kadar tehlikeli | `Breaking`, `NonBreaking`, `Warning`, `DocsOnly` |
| **Address** | Nerede | API: `POST /tickets` → yanıt → `status`<br>DB: `sales.Tickets.Status` |
| **Fingerprint** | Kalıcı kimliği | `A1B2C3…` (64 karakter) |
| **ChangeState** | Yeni mi eski mi | `New`, `Known`, `Resolved` |

**İnsan diliyle bir finding:**
> `POST /tickets` operasyonunun 201 yanıtındaki `status` alanı artık zorunlu değil.
> Şiddet: **Breaking**. Durum: **New**.

**Severity anlamları:**

| Severity | Anlamı |
|---|---|
| `Breaking` | Bu değişiklik API'yi kullanan kodları kırar |
| `NonBreaking` | Değişti ama kimseyi kırmaz |
| `Warning` | Kırmaz ama dikkat gerektirir |
| `DocsOnly` | Sadece açıklama değişmiş |

### **Fingerprint (parmak izi)**
Bir finding'in **kimliği**. Adres + fark türü + farkın içeriği SHA-256 ile hash'lenir.
İşe yarar: tekrar tespiti, susturma, "bu yeni mi?" hesabı.

> ⚠️ Fingerprint bir **adres değildir**; içinde farkın kendisi de var. "Bu senaryo hangi
> adrese dokunuyor" sorusu fingerprint ile **cevaplanamaz** (bkz. §11 karar C-5).

### **ChangeState**
`New` (önceki koşuda yoktu) · `Known` (vardı) · `Resolved` (vardı, artık yok).
`New` bulgular bakım anını tetikler.

### **Diagnosis (teşhis)**
Bir başarısızlığın **nedeni** hakkında sıralı, güven dereceli tahminler.
**Hipotez:** tek bir tahmin. **Probe:** hipotezi sınayan küçük, salt-okunur sorgu.

### **Value retention**
Bulgularda gerçek verinin tutulup tutulmayacağı politikası. Varsayılan `None`.

## 1.5 Yapay zekâ dünyası

**Token** — Modelin metni ölçtüğü birim (~4 karakter). **Fatura token üzerinden.**

**Bağlam (context)** — Modelin o an önünde duran metnin tamamı. Sınırlıdır.

**Ajan (agent)** — Araç kullanarak iş yapan model kurulumu.

**MCP** — Ajanın dış sistemlere bağlandığı standart protokol. USB portu gibi.

**Tool (araç)** — Ajanın çağırabildiği tek fonksiyon. Tanımı bağlamda yer kaplar.

**Prompt cache** — Modelin daha önce gördüğü **birebir aynı** metni indirimli okuması.

**RAG** — Modelin cevap üretirken dış kaynaktan ilgili parçayı çekip bağlamına koyması.

## 1.6 Formatlar

**Arazzo** — OpenAPI'yi yapan kurumun çok adımlı senaryo standardı. Senaryo formatımız.

**Overlay** — Aynı kurumun "dokümana yama uygula" standardı. Onarım önerisi formatımız.

**DMN** — OMG'nin karar tablosu / iş kuralı standardı. Kural kataloğumuzun temeli.

**Gherkin** — İş diliyle senaryo yazma dili (`Given/When/Then`, `Rule`). Plan artefaktımızın ilhamı.

**CTRF / JUnit / SARIF / OpenTelemetry** — Dışa aktarım formatları.

**RFC 9457** — HTTP hatalarının standart taşınma biçimi. Teşhis raporu formatımız.

## 1.7 Altyapı

**ABP** — .NET uygulama çatımız. Modülerlik, çok kiracılılık, yetki, arka plan işleri.

**Multi-tenancy** — Aynı uygulamanın çok müşteriye hizmet vermesi; `TenantId` filtresi.

**Lookup tablosu** — Sabit değer listelerini tutan tablo.

**Vault** — Parolaların saklandığı kasa. Veritabanında yalnız kasadaki **adres** durur.

**Content-addressed** — İçeriği kendi hash'iyle adreslemek; aynı içerik iki kez yazılmaz.

**Handle** — Nesneye işaret eden opak kimlik; nesnenin tamamı yerine kimliği taşınır.

---

# 2. Ürün resmi

```
                    ┌──────────────────────────┐
     Insan  ──────► │  Test Module             │  ◄── senaryolari saklar, kosar,
     (is dili)      │  (yapilacak)             │      kanit uretir, etkiyi analiz eder
                    └──────┬───────────────────┘
                           │  "bu dogru mu?" diye sorar
              ┌────────────┴────────────┐
              ▼                         ▼
  ┌───────────────────────┐  ┌──────────────────────┐
  │ API Contract Checker  │  │ Database Checker     │
  │ (hazir, 0.2.0-alpha.7)│  │ (hazir, 0.2.0-alpha.8)│
  │ "yanit sozlesmeye     │  │ "beklenen satir      │
  │  uyuyor mu?"          │  │  olustu mu?"         │
  └───────────────────────┘  └──────────────────────┘
```

**Neden üç parça?**
1. Checker'lar tek başına da değerli ("iki ortam aynı mı" sorusu testten bağımsızdır)
2. Ayrı sürümlenirler
3. Tek doğruluk kaynağı — şema doğrulaması iki yerde yazılırsa zamanla kayar

---

# 3. İki tür test — ve ürünün asıl hedefi

## 3.1 Teknik doğrulama (taban)

| Soru | Kim cevaplar |
|---|---|
| HTTP 201 döndü mü? | Test Module |
| Yanıt gövdesi sözleşmeye uyuyor mu? | API Contract Checker |
| `sales.Tickets` tablosunda satır oluştu mu? | Database Checker |

Bu katman **gereklidir ama yeterli değildir.** MCP olmadan da yapılabilir.

## 3.2 İş senaryosu (asıl hedef)

| Soru | Nasıl çözülür |
|---|---|
| Bilet varsa al, yoksa? | **Koşullu akış** — karar adımı |
| "A saatinde bilet olmalı" — kim sağlayacak? | **Önkoşul** — `Arrange` veya `Discover` |
| Bilet yoksa senaryo ne dönecek? | **`Inconclusive`** — yeşil değil |
| Koltuk sayısı gerçekten azaldı mı? | **İş değişmezi** — M-2 delta |
| Ödenen tutar aramadaki fiyatla aynı mı? | **İş değişmezi** — M-3 tutarlılık |
| Aynı koltuk iki kez satıldı mı? | **İş değişmezi** — M-4 tekillik |
| Kota dolunca reddediliyor **ve yan etki oluşmuyor** mu? | **İş değişmezi** — M-8 negatif yol |
| İptal edilmiş bilet tekrar ödenebiliyor mu? | **Durum geçişi** — M-10 |

**Bu satırların hiçbiri şema doğrulamasıyla yakalanamaz.** Hepsi canlıya çıkıp
müşteri tarafından bulunan hata türleridir.

## 3.3 Kritik: bu, "koşumda model yok" kararını bozmuyor

Koşullu akış bir `if`. Önkoşul bir veri hazırlığı. İş değişmezi bir çıkarma işlemi.
Hepsi **deklaratif ve deterministik**.

**Model yalnız bu senaryoyu yazarken devrede.** Zaten ürünün amacı budur.

---

# 4. Uçtan uca örnek: "A saatinde bilet varsa al"

## 4.1 İnsanın söylediği

> *"Yarın saat 10:00'da İstanbul–Ankara seferinde bilet varsa satın al ve gerçekten
> satın alındığını doğrula."*

## 4.2 Bu cümlenin içindeki altı gizli gereksinim

| # | Gizli gereksinim | Nasıl karşılanır |
|---|---|---|
| 1 | Arama yapılacak, sonuç sayısı bilinmiyor | HTTP adımı |
| 2 | **Karar** verilecek | `x-checknexus-branch` |
| 3 | Aramanın çıktısı satın almanın girdisi olacak | Arazzo `outputs` |
| 4 | **Önkoşul**: o saatte bilet olmalı | `preconditions` + `Arrange` |
| 5 | **İş kuralı**: koltuk 1 azaldı, tutar eşleşti | M-2, M-3 değişmezleri |
| 6 | Bilet yoksa ne dönecek? | **`Inconclusive`** |

## 4.3 Üretilen senaryo

```yaml
x-checknexus-scenario:
  kind: BusinessScenario
  intent: "A saatinde bilet varsa satin al ve satin alindigini kanitla"
  preconditions:
    - id: biletVar
      strategy: Arrange
      arrange: { datasetRef: ist-ank-yarin-10-00 }
      onUnsatisfied: Inconclusive

steps:
  # 0) BASLANGIC OLCUMU — is degismezi icin
  - stepId: oncekiDurum
    x-checknexus-db:
      operation: assertRow
      schema: sales ; table: Trips
      key: { Id: "{$inputs.tripId}" }
      outputs: { kalanKoltukOnce: AvailableSeats }

  # 1) ARAMA
  - stepId: seferAra
    operationId: searchFlights
    parameters: [ { name: departAt, value: "{$inputs.departAt}" } ]
    outputs:
      bulunanSayi: $response.body#/totalCount
      ilkSeferId:  $response.body#/items/0/id
      fiyat:       $response.body#/items/0/price

  # 2) KARAR NOKTASI
  - stepId: biletVarMi
    x-checknexus-branch:
      when: "$steps.seferAra.outputs.bulunanSayi > 0"
      then: goto satinAl
      else:
        end: Inconclusive
        reason: "O saatte sefer yok; ana yol kosmadi."

  # 3) SATIN ALMA
  - stepId: satinAl
    operationId: purchaseTicket
    requestBody: { payload: { flightId: "{$steps.seferAra.outputs.ilkSeferId}" } }
    successCriteria: [ { condition: $statusCode == 201 } ]
    outputs: { biletId: $response.body#/ticketId, odenen: $response.body#/amount }

  # 4) KALICILIK KANITI
  - stepId: biletKaydedildi
    x-checknexus-db:
      operation: assertRow
      schema: sales ; table: Tickets
      key: { Id: "{$steps.satinAl.outputs.biletId}" }
      expect: { Status: { matcher: equals, value: "Confirmed" } }
    timeout: 5000
    onFailure: [ { type: retry, retryLimit: 10, retryAfter: 0.5 } ]

  # 5) IS DEGISMEZI M-2: koltuk tam 1 azaldi
  - stepId: koltukAzaldi
    x-checknexus-db:
      operation: assertRow
      schema: sales ; table: Trips
      key: { Id: "{$steps.seferAra.outputs.ilkSeferId}" }
      outputs: { kalanKoltukSonra: AvailableSeats }
    successCriteria:
      - condition: "$steps.oncekiDurum.outputs.kalanKoltukOnce
                    - $steps.koltukAzaldi.outputs.kalanKoltukSonra == 1"

  # 6) IS DEGISMEZI M-3: tutar tutarliligi
  - stepId: tutarEslesti
    successCriteria:
      - condition: "$steps.satinAl.outputs.odenen == $steps.seferAra.outputs.fiyat"

  # 7) IS DEGISMEZI M-4: cift satis yok
  - stepId: koltukTekSatildi
    x-checknexus-db:
      operation: assertCount
      schema: sales ; table: Tickets
      key: { FlightId: "...", SeatNo: "..." }
      cardinality: exactly 1
```

**Bu dosyanın hiçbir satırı koşum anında model gerektirmiyor.**

## 4.4 İkinci örnek: kota kuralı

> *"Öğrenci indirimli bilet hakkı dönem başına 2. İkisini kullanan üçüncüyü alamamalı."*

Karar tablosundan **dört senaryo otomatik türetilir**:

| # | Önkoşul | Eylem | Beklenen |
|---|---|---|---|
| 1 | öğrenci, 0 bilet | indirimli al | ✅ 201, sayaç 1 |
| 2 | öğrenci, 1 bilet | indirimli al | ✅ 201, sayaç 2 |
| 3 | **öğrenci, 2 bilet** | indirimli al | ❌ **409** + `Tickets` değişmez + `Payments` oluşmaz |
| 4 | öğrenci değil | indirimli al | ❌ red / normal fiyat |

**3 numara asıl değerli olandır** ve karar tablosunun sınır satırından otomatik doğar.

---

# 5. API Contract Checker — yetenekleri

Paket: `CheckNexus.ApiContracts` (8 paket) · Yayımlı: `0.2.0-alpha.7`

| # | Yetenek | Ne yapar | Dışarıya ne verir |
|---|---|---|---|
| Y-1 | Sözleşme kaynağı yönetimi | Dokümanın nereden alınacağını kaydeder, erişilebilirliğini test eder | Kaynak listesi |
| Y-2 | Snapshot + içerik-adresli saklama | `RawHash` (ham) + `CanonicalHash` (anlamsal) ile saklar; aynı içerik iki kez yazılmaz | Snapshot listesi, hash'ler |
| Y-3 | Karşılaştırma motoru | İki snapshot'ı kıyaslar, farkları **finding** olarak üretir | Koşu + bulgu listesi |
| Y-4 | Şiddet sınıflandırma | `Breaking` / `NonBreaking` / `Warning` / `DocsOnly` | Severity kodu |
| Y-5 | Parmak izi + değişim durumu | Kalıcı SHA-256 kimlik; `New`/`Known`/`Resolved` | Fingerprint, ChangeState |
| Y-6 | Sayfalı + filtreli bulgu okuma | Şiddet, tür, adres, durum; **`SinceRunId`**, **`Fingerprints`** | Sayfalı bulgu |
| **Y-7** | **Yanıt/istek uygunluk kontrolü (oracle)** | "Bu yanıt sözleşmeye uyuyor mu?" — kapalı sonuç kodu + JSON Pointer ile ihlal listesi | Uygunluk sonucu |
| Y-8 | Örnek istek gövdesi üretme | Sözleşmeye uygun örnek gövde | Örnek payload |
| Y-9 | Operasyon eşleme önerisi | Gözlenen isteğin hangi operasyona karşılık geldiği | Eşleme önerileri |
| **Y-10** | **Senaryo beklentisi doğrulama (yayın kapısı)** | "Bu beklentiler sözleşmeden türetilebilir mi?" | Türetilebilirlik raporu |
| Y-11 | Dinamik teşhis motoru | HTTP'nin yapılandırılmış hata alanları + güvenli sondalar → RFC 9457 rapor | Sıralı hipotezler |
| Y-12 | Değer saklama / maskeleme | Varsayılan: gerçek değer tutulmaz | — |
| Y-13 | Kararlı kod kümeleri + lookup'lar | Dışarıya verilen sözleşme; değişirse tüketici kırılır | Lookup listeleri |
| Y-14 | Çok kiracılılık, yetki, Vault | Kendi kullanıcı yönetimi **yok**; host'un bağlamını kullanır | — |

**Kritik kural:** Gözlenen istek **tek** operasyona çözülemiyorsa kontrol **çalışmaz**
(`OperationNotResolved`). Belirsizken tahmin etmez.

---

# 6. Database Checker — yetenekleri

Paket: `CheckNexus.DatabaseComparison` (8 paket) · Yayımlı: `0.2.0-alpha.8`

| # | Yetenek | Ne yapar |
|---|---|---|
| Y-1 | Bağlantı defteri + emniyet profili | Motor, host, port, **Vault parola adresi**; TLS, `READ ONLY`, timeout |
| Y-2 | Şema keşfi | Tablolar, kolonlar (tip/nullable/generated/açıklama), PK, unique, **FK**, kısıtlar (**ve doğrulanmışlık durumu**), collation |
| Y-3 | Karşılaştırma tarifi + kapsam kuralları | Hangi iki bağlantı, hangi modda, hangi tablolar |
| Y-4 | Karşılaştırma koşusu + bulgular | Şema / veri / migration farkları + özet sayaçlar |
| Y-5 | Motorlar arası kanonik tip haritası | `nvarchar(50)` ↔ `varchar(50)`; dört değerli güven kodu |
| Y-6 | Şiddet, parmak izi, sayfalama, filtreler | API tarafıyla aynı; `SinceRunId`, `Fingerprints` |
| **Y-7** | **Hedefli assertion yüzeyi (oracle)** | `AssertRow` / `AssertCount` / `AssertAbsent` / **`AssertBatch`** |
| Y-8 | Dinamik teşhis motoru | Sinyal → kimlik → katalog → **10 hipotez kuralı** → **3 sonda** → sıralama → RFC 9457 |
| Y-9 | Değer saklama politikası | Varsayılan `None` |
| Y-10 | Rapor üretimi | HTML / Markdown |
| Y-11 | Kararlı kodlar + lookup'lar | — |
| Y-12 | Telemetri | Koşu süresi, sorgu sayısı, sonda bütçesi |

## Y-7'nin ayrıntısı — iş senaryolarının motoru

| Çağrı | Soru |
|---|---|
| `AssertRow` | Bu anahtarla satır var mı, kolon beklentileri tutuyor mu? |
| `AssertCount` | Kaç satır var? (`tam 1` / `en az 1` / `hiç`) — **`ObservedRowCount` döner** |
| `AssertAbsent` | Bu satır **yok** mu? — **M-8 negatif yolun temeli** |
| `AssertBatch` | Birkaçını **tek çağrıda** |

Ek: sunucu tarafında sınırlı bekleme (`TimeoutMs` + `PollIntervalMs`), tip-farkında
matcher'lar, **`ObservedAtMs`** (kaç ms sonra gerçekleşti), `AttemptCount`, kararlı sonuç kodları.

**Kritik kural:** Anahtar PK/tekil değilse assertion **çalışmaz**, `KeyNotUnique` döner.
"O satır" garantisi olmadan sessiz yanlış cevap verilmez. **Serbest SQL kabul edilmez.**

## 10 teşhis hipotezi

`RowNeverCreated` · `RowCreatedLate` · `RowInAnotherScope` · `ForeignKeyParentMissing` ·
`UniqueDuplicateExists` · `ConstraintNotValidated` · `GeneratedColumnWrite` ·
`ExpectedColumnMissing` · `ServerSettingMismatch` · `RowValueDiffers`

---

# 7. Test Module — yetenekleri

| # | Yetenek | Açıklama |
|---|---|---|
| T-1 | Senaryo yazımı ve sürümleme | İçerik-adresli saklama, sürüm, onay akışı |
| T-2 | Ortam bağlama | "test ortamında `booking-db` şu adrestir"; senaryo adres bilmez |
| T-3 | **Koşum motoru** | Adımları çalıştırır, oracle'lara sorar. **Model yok.** |
| T-4 | Test verisi sandbox'ı | Ayrı yetkili bağlantı; önkoşul verisini kurar ve temizler |
| T-5 | Kanıt | Adım adım, maskelenmiş |
| T-6 | Dışa aktarım | CTRF, JUnit, SARIF, OpenTelemetry |
| T-7 | **Etki analizi** | "Bu sözleşme farkı hangi senaryonun hangi adımını bozar" |
| T-8 | **Onarım önerisi** | Overlay formatında, gerekçeli; insan onaylar |
| T-9 | Sağlık ve karantina | Kararsızlık takibi, süreli karantina |
| T-10 | **MCP köprüsü** | Ajan yüzeyi; iki checker'ı tek sözlüğe indirir |
| **T-11** | **Koşullu akış ve önkoşul** | Karar adımı, `Arrange`/`Discover`, `Inconclusive` |
| **T-12** | **İş değişmezi motoru** | Adım çıktısı olarak sayısal ölçüm; delta/korunum/tekillik kontrolü |
| **T-13** | **İş bilgisi katmanı** | Sözlük, kural kataloğu, yolculuklar, ilişki grafiği, etki ayak izi |
| **T-14** | **Kural kapsam raporu** | "12 iş kuralın var, 9'u test ediliyor" |

---

# 8. İş bilgisi ajana nasıl aktarılır?

Bu bölüm ürünün kalbidir. Ajan *"öğrenci bileti hakkı 2"* kuralını nereden bilecek?

## 8.1 Dört bilgi katmanı

| Katman | Kaynak | Maliyet | Otorite | Örnek |
|---|---|---|---|---|
| **K-1 Türetilebilir** | OpenAPI, `links`, FK grafiği, kısıtlar | **Sıfır** | Kesin | `Tickets.TripId → Trips.Id` |
| **K-2 Gözlemlenebilir** | İşlemi koş, **etkisini** ölç | Düşük | Olasılıksal | "`AvailableSeats` 1 azalıyor" |
| **K-3 Beyan edilen** | İnsanın yazdığı sözlük + kural kataloğu | Yüksek | **Otoriter** | "Kota dönem başına 2" |
| **K-4 Etkileşimli** | Ajan emin değilse sorar | Nokta atışı | Otoriter | "`Reserved` boş sayılır mı?" |

**Altın kural:** K-2 ve K-4 ile öğrenilen her şey **K-3'e yazılır**. Aynı şey iki kez
keşfedilmez, aynı soru iki kez sorulmaz.

## 8.2 API uçlarının ilişkisi beş kaynaktan çözülür

| # | Yöntem | Kesinlik |
|---|---|---|
| 1 | **OpenAPI `links`** | Kesin |
| 2 | **Şema analizi**: `search` yanıtı `id`, `purchase` isteği `flightId` → ad+tip eşleşmesi | Yüksek |
| 3 | **`Location` başlığı** | Yüksek |
| 4 | **FK grafiği** | Kesin (DB tarafı) |
| 5 | **Gözlenen etki ayak izi** | Olasılıksal |

Beşi birleşip bir **operasyon ilişki grafiği** oluşturur; her kenarda keşif yöntemi,
güven derecesi ve onay durumu durur.

## 8.3 Etki ayak izi — en güçlü kozumuz

```
1. Sandbox'ta DB durumunu olc        (once)
2. purchaseTicket'i bir kez cagir
3. DB durumunu tekrar olc            (sonra)
4. Farki cikar  ──►  ETKI AYAK IZI
```

```
purchaseTicket etki ayak izi:
  sales.Tickets       +1 satir
  sales.Trips          AvailableSeats  42 → 41
  billing.Payments    +1 satir
  notify.Outbox       +1 satir
```

Ajan bunu insana gösterip sorar: *"Bu etkiler değişmez olarak yazılsın mı?"*

**Avantajımız:** akademik muadilleri (AGORA/Daikon) yalnız **HTTP yanıtına** bakabiliyor.
Biz **veritabanının kendisine** bakıyoruz — `notify.Outbox` satırı hiçbir API yanıtında
görünmez, bizim ayak izimizde görünür.

**Ve yeni motor gerekmiyor:** karşılaştırma motoru, `AssertCount`, veri farkı bulguları,
emniyet profili, sandbox — hepsi mevcut. Yeni olan sadece bu adımların dizilişi.

## 8.4 K-3: "Ajana md mi vereceğiz?" — evet, ama yapılandırılmış

### Dosya 1 — İş sözlüğü (`domain/glossary.md`)

```markdown
## bilet
- Tanım: Bir yolcunun belirli bir seferdeki koltuk hakkı.
- Operasyonlar: searchFlights, purchaseTicket, getTicket, cancelTicket
- Tablolar: sales.Tickets (ana), sales.TicketHistory (arşiv)
- Kimlik: Tickets.Id
- Durumlar: Reserved → Paid → Confirmed → Used | Cancelled
- Beklenen değişmezler: M-2, M-4, M-6

## koltuk
- Tablolar: sales.Seats, sayaç: sales.Trips.AvailableSeats
- Kural bağı: her onaylı bilet **tam 1** koltuk tüketir
- Beklenen değişmezler: M-1, M-7
```

### Dosya 2 — İş kuralı kataloğu (`domain/rules/BR-014.md`)

```markdown
---
id: BR-014
title: Ogrenci bileti kotasi
scope: donem
appliesTo: [ purchaseTicket ]
errorCode: StudentQuotaExceeded
---

## Karar tablosu
| Öğrenci belgesi | Dönem içi alınan | Sonuç | Hata kodu |
|---|---|---|---|
| yok | *  | normal fiyat | — |
| var | 0  | izin ver     | — |
| var | 1  | izin ver     | — |
| var | 2  | **REDDET**   | StudentQuotaExceeded |
| var | >2 | tutarsız     | QuotaStateCorrupt |

## Sayaç kaynağı
sales.Tickets: IsStudentFare = true AND TermId = <aktif dönem>

## Reddedildiginde dogrulanacaklar
- HTTP 409 + StudentQuotaExceeded
- sales.Tickets satır sayısı değişmedi
- billing.Payments satırı oluşmadı
```

### Dosya 3 — Yolculuk (`domain/journeys/JR-002.md`)

```markdown
## Gecisler
| Kaynak | Hedef | Tetikleyen | Ön koşul |
| Reserved | Paid | capturePayment | süre dolmamış |
| Confirmed | Cancelled | cancelTicket | seferden 2 saat önce |

## Yasak gecisler (test edilmeli)
- Cancelled → Paid  → red, InvalidStateTransition, yan etki yok
- Used → Cancelled  → red, TicketAlreadyUsed, yan etki yok
```

### Bu dosyalar nerede yaşar?

| Yer | Rolü |
|---|---|
| **Repo/Git** (`domain/*.md`) | **Kaynak** — insan düzenler, PR'da incelenir, sürümlenir |
| **Veritabanı** | **Türev** — ajan sorgular, kapsam ölçülür, senaryoya bağlanır |

Ajan 40 sayfa markdown'ı bağlamına almaz; *"BR-014'ü getir"* der.

## 8.5 Ajanın bilgi tüketim akışı

```
Insan: "A saatinde bilet varsa al"
  ├─ 1) Sozlukten "bilet" cozulur     -> operasyon + tablo + degismez  (K-3)
  ├─ 2) Ilgili kurallar cekilir       -> BR-014                         (K-3)
  ├─ 3) Operasyon ozeti alinir        -> parametre + yanit semasi       (K-1)
  ├─ 4) Iliski grafigi okunur         -> search.id -> purchase.flightId (K-1)
  ├─ 5) Etki ayak izi okunur          -> hangi tablolar degisiyor       (K-2)
  ├─ 6) Belirsizlik varsa SORULUR     -> input_required                 (K-4)
  ├─ 7) Markdown PLAN uretilir        -> insan onaylar
  └─ 8) Arazzo senaryosu uretilir     -> dogrula, kuru kos, kaydet
```

---

# 9. İş değişmezi kalıp kataloğu (M-1..M-10)

Yazım ajanının şablonu. Her iş alanı bunları kendi terimleriyle doldurur.

| # | Kalıp | Genel form | Bilet örneği |
|---|---|---|---|
| **M-1** | **Korunum** | Toplam sabit kalır | boş + satılan = kapasite |
| **M-2** | **Delta** | İşlem tam N kadar değiştirir | satış → boş koltuk **tam 1** azalır |
| **M-3** | **Tutarlılık** | İki kaynak aynı değeri söyler | ödenen = aramadaki fiyat |
| **M-4** | **Tekillik** | Aynı kaynak iki kez tahsis edilemez | aynı koltuğa `exactly 1` bilet |
| **M-5** | **Gidiş-dönüş** | Oluştur → oku → aynı veri | `GET /tickets/{id}` aynı alanlar |
| **M-6** | **İdempotans** | Aynı işlem iki kez → ikinci red veya aynı sonuç | aynı anahtarla iki satış → tek bilet |
| **M-7** | **Monotonluk** | Değer tek yönde değişir | satış sonrası boş koltuk **artmaz** |
| **M-8** | **Negatif yol** | Geçersiz istek reddedilir **ve durum değişmez** | kota dolu → 409 **ve satır oluşmaz** |
| **M-9** | **Yetki sınırı** | Başka kiracının kaydına erişilemez | A tenant'ı B'nin biletini göremez |
| **M-10** | **Durum geçişi** | Yalnız izinli geçişler | `Cancelled → Paid` reddedilmeli |

**M-8'i özellikle vurguluyoruz:** çoğu ekip "reddedildi mi" diye bakıp geçer.
Asıl soru **"yan etki oluştu mu"**. `AssertAbsent` tam bunun için var.

---

# 10. Bilgi olgunluk seviyeleri

Ekip sıfırdan mükemmel katalog yazmak zorunda değil. Ürün her seviyede çalışır ama
seviyeyi ve bir üstte ne açılacağını **söyler**.

| Seviye | Ekipte olan | Ajan ne yapabilir | Kalite |
|---|---|---|---|
| **L0** | Sadece OpenAPI | Teknik doğrulama, mutlu yol | Düşük |
| **L1** | + FK grafiği (otomatik) | Kalıcılık kanıtı, temel delta önerileri | Orta |
| **L2** | + etki ayak izi | **Yan etki değişmezleri** — gerçek entegrasyon testi | İyi |
| **L3** | + iş sözlüğü | Doğru terim, doğru tablo, doğru kalıp | İyi–yüksek |
| **L4** | + kural kataloğu | **Sınır ve negatif senaryolar**, kural kapsamı | Yüksek |
| **L5** | + yolculuk tanımları | Durum geçişi ve yasak geçiş testleri | Tam |

---

# 11. MERKEZİ KARAR SİCİLİ

Verilen her karar, gerekçesi ve dayandığı küresel kaynak. Kaynak künyeleri §16'da.

## 11.A Mimari

| # | Karar | Neden | Küresel kaynak |
|---|---|---|---|
| A-1 | **Yapay zekâ koşum döngüsünde değil** | Ajanlı koşum ~114K token/test; model olasılıksal; koşum saatler sürer | Playwright Test Agents (planner/generator/healer üretimde, koşum düz test) [S-01]; token ölçümleri [S-02] |
| A-2 | **Hakem checker, asla model** | LLM hakem kırılgan: prompt değişince karar değişir, gerekçe kara kutu | Test Oracle Automation in the Era of LLMs [S-03]; LogicHunter düşük precision [S-04] |
| A-3 | **Checker'lar hedefe yazmaz** | "Ölümcül üçlü"nün en kritik ayağını kesmek | Supabase MCP olayı; lethal trifecta [S-05] |
| A-4 | **Reset, transaction geri alma değil** | SUT kendi bağlantısını açtığında rollback çalışmaz | Testcontainers/.NET izolasyon pratiği [S-06] |
| A-5 | **MCP ana uygulamada, checker'da değil** | Tool bütçesi ~12; katalog küratörlüğü host kararı | ADR-0008; GitHub MCP toolset ölçümü [S-07] |
| A-6 | **Tool ≤ 12, an bazında profil** | Tool sayısı arttıkça seçim doğruluğu düşer | Jentic MCP tool trap [S-08]; GitHub MCP %60–90 [S-07] |
| A-7 | **SUT otomatik tool'a çevrilmez** | 200 endpoint = 200 tool | REST→MCP ampirik çalışması [S-09] |

## 11.B Format ve protokol

| # | Karar | Neden | Küresel kaynak |
|---|---|---|---|
| B-1 | **Senaryo formatı Arazzo** | İhtiyacımız olan her şey spec'te; model formatı zaten biliyor | Arazzo 1.1.0 [S-10] |
| B-2 | **DB adımı `x-` uzantısı** | Standardı çatallamadan genişletme | Arazzo uzantı mekanizması [S-10] |
| B-3 | **Yama formatı Overlay** | İncelenebilir, kuru çalıştırılabilir, anlatma maliyeti sıfır | Overlay 1.0 [S-11] |
| B-4 | **Uzun koşu MCP Tasks** | Bağlantı tutulamaz; `input_required` = onay adımı | MCP Tasks uzantısı [S-12] |
| B-5 | **Rapor formatları çıktıdır** | Hepsi eksik; OTel test öznitelikleri 4 alan, `Broken` yok | CTRF [S-13]; OTel semconv [S-14]; SARIF [S-15] |

## 11.C Kimlik ve sürümleme

| # | Karar | Neden | Küresel kaynak |
|---|---|---|---|
| C-1 | **Kalıcı `scenario_key`, hash'ten türetilmez** | Ad değişince kimlik değişir, geçmiş kopar | Allure `testCaseId` uyarısı [S-16] |
| C-2 | **İçerik hash'iyle saklama** | Aynı içerik iki kez yazılmaz; anlamsal eşitlik | Pact Broker içerik dedup [S-17]; kendi `SpecContent`'imiz |
| C-3 | **Koşum, sürüm kimliğini taşır** | Tanım değişince eski rapor yalan söylememeli | Kiwi TCMS `case_text_version` [S-18] |
| C-4 | **`HistoryId`: SHA-256, değişkenler dışlanır** | MD5 FIPS ortamında çalışmaz; timestamp trend kovasını böler | Allure `historyId` [S-16] |
| C-5 | **Parmak izi ≠ adres** | Fingerprint farkın kendisini içerir; adım için hesaplanamaz | Kendi kod incelememiz (K1) |

## 11.D Veri modeli

| # | Karar | Neden | Küresel kaynak |
|---|---|---|---|
| D-1 | **Üç şema: `test_lookup`/`test_catalog`/`test_run`** | DB checker genel adları aldı; hacim ve saklama farklı | ARCH-0003 |
| D-2 | **Durumlar lookup tablosunda** | Migration'sız genişleme, FK kısıtı | Kiwi TCMS statü tablosu [S-18] |
| D-3 | **Sabit metinler Domain.Shared'da** | EF ve doğrulama aynı sabiti okur | Repo sözleşmesi (K1) |
| D-4 | **JSON yalnız sorgulanmayan payload'a** | Sorgulanan bilgi indeksli tabloda olmalı | Checker kodundaki yazılı kural (K1) |
| D-5 | **Özet sayaçlar denormalize** | Liste ekranı `COUNT` atmasın | `ComparisonRun` deseni (K1) |
| D-6 | **Altı statü (`Inconclusive` dahil)** | `Failed`≠`Broken`≠"hiçbir şey test edilmedi" | Allure 5 statü [S-16]; yanlış negatif literatürü [S-19] |
| D-7 | **v1'de partition yok, eşikli geçiş** | Partition PK'sı ABP anahtar sözleşmesini kırar | pg_partman/retention pratiği [S-20] |
| D-8 | **Büyük veri nesne deposunda** | Yedek, WAL ve geri yükleme maliyeti | ReportPortal üç katman [S-21]; ABP BLOB Storing [S-22] |
| D-9 | **Modüller arası FK yok** | Paket sınırı ve migration sırası | RULE-0001/0002 |
| D-10 | **Tek DB + `TenantId`** | Kiracı başına şema katalog şişirir | ABP multi-tenancy [S-23] |

## 11.E Koşum motoru

| # | Karar | Neden | Küresel kaynak |
|---|---|---|---|
| E-1 | **ABP arka plan işleri, Temporal değil** | İkinci durum sahibi ve ayrı küme maliyeti | Temporal durable execution [S-24]; ABP jobs/outbox [S-25] |
| E-2 | **Retry/timeout senaryodan okunur** | Bekleme senaryoya özeldir | Arazzo `retry`/`timeout` [S-10] |
| E-3 | **İptal kooperatif** | Zorla öldürme yarım işlem bırakır | MCP `tasks/cancel` [S-12] |
| E-4 | **Çoklu DB kontrolü tek çağrıda** | 2.500 → 500 gidiş-dönüş | `AssertBatchAsync` (K1) |
| E-5 | **Aynı ortamda tek koşu** | Paralel koşular birbirinin verisini bozar | Flakiness kök sebep: eşzamanlılık [S-26] |

## 11.F Güvenlik ve gizlilik

| # | Karar | Neden | Küresel kaynak |
|---|---|---|---|
| F-1 | **Ham veri saklanmaz (varsayılan)** | Gizlilik + prompt injection yüzeyi | OWASP LLM Top 10 2026, LLM01 [S-27]; GDPR md.5 [S-28] |
| F-2 | **Parola Vault adresinde** | Sır veritabanında durmaz | RULE-0003; `DatabaseConnection.VaultSecretPath` (K1) |
| F-3 | **Modelin yazma yolu yok** | Excessive Agency 6.→3. sıraya fırladı | OWASP LLM Top 10 2026, LLM03 [S-27] |

## 11.G Kalite kapıları

| # | Karar | Neden | Küresel kaynak |
|---|---|---|---|
| G-1 | **Yayın kapısı: beklenti türetilebilir mi** | Uydurulmuş alan adı yayında yakalanmalı | `ValidateScenarioAssertions` (K1) |
| G-2 | **Yayın kapısı: anahtar tekil mi** | "O satır" garantisi yoksa sessiz yanlış cevap | ADR-0007 `KeyNotUnique` |
| G-3 | **`Healed` etiketi zorunlu** | Sessiz onarım gerçek hatayı gizler | Self-healing eleştirisi [S-29] |
| G-4 | **Karantinaya son kullanma tarihi** | Karantina çöp kutusu olmasın | Datadog flaky yaşam döngüsü [S-30] |
| G-5 | **Parmak izi mutabakat testi ilk gün** | Sessiz eşleşmeme alarm üretmez | Kendi tasarım analizimiz |

## 11.H Köprü ve token ekonomisi

| # | Karar | Neden | Küresel kaynak |
|---|---|---|---|
| H-1 | **Token bütçeli sayfalama** | Kayıt boyutu öngörülemez | Datadog MCP dersleri [S-31] |
| H-2 | **Tablo verisi TSV** | ~%50 token, aynı bütçede ~5× kayıt | Datadog [S-31] |
| H-3 | **Dar varsayılan yanıt** | "Tek en yüksek kaldıraçlı optimizasyon" | Datadog [S-31] |
| H-4 | **Karar döndür, ham veri değil** | Sunucu tarafı toplama ~%40 ucuz | Datadog [S-31] |
| H-5 | **Ağır çıktı `resource_link`** | Gövde bağlama girmesin | MCP spec [S-32] |
| H-6 | **İş-şekilli tool** | 3 çıkarım turu → 1 | Datadog [S-31] |
| H-7 | **`defer_loading` / Tool Search** | 77K→8,7K (%85); doğruluk %49→%74 | Anthropic advanced tool use [S-33]; RAG-MCP [S-34]; MCP-Zero [S-35] |
| H-8 | **Prosedür bilgisi Skill** | Skill ~100 token, tool tam yüklenir | Agent Skills vs MCP [S-36] |
| H-9 | **Deterministik tool sırası + `ttlMs`** | Prompt cache isabeti | MCP spec [S-32] |
| H-10 | **Hash tabanlı bilgi önbelleği** | Metnin bayt bayt aynı kalması | Kendi `CanonicalHash`'imiz (K1) |
| H-11 | **Handle deseni** | Gövde iki kez bağlama girmez | MCP Stateful Tools [S-32] |
| H-12 | **Tek ajan sözlüğü** | İki kod sözlüğü token ve hata demek | Kendi kod incelememiz (K1) |
| H-13 | **Öğreten hata mesajı** | Deneme-yanılma turlarını keser | Datadog [S-31] |
| H-14 | **Kod çalıştırma şimdilik yok** | %98,7 kazanç ama sandbox yükü | Anthropic code execution with MCP [S-37] |

## 11.I Tester sorunlarından gelen kararlar

| # | Karar | Neden | Küresel kaynak |
|---|---|---|---|
| I-1 | **Asenkron gecikme p95 ile izlenir** | Flakiness'in 1 numaralı sebebi | Luo ve ark. FSE 2014 [S-26]; SAP HANA 2026 [S-38] |
| I-2 | **Sıra bağımlılığı ayrı durum** | 3. yaygın sebep; farklı tedavi ister | Luo ve ark. [S-26] |
| I-3 | **Matcher kaydedilir** | Oracle kırılganlığı %17; kayan nokta %48,4 | Eck ve ark.; kırılgan assertion tespiti [S-39] |
| I-4 | **Teşhis sonucu koşum satırında** | Triyaj medyanı 28 dk; otomasyon %75–80 düşürüyor | FlakyGuard 1.000+ ekip [S-40] |
| I-5 | **Veri kümesi kimliği kaydedilir** | "Hangi veriyle geçti" cevaplanabilmeli | TDM pratiği [S-41] |
| I-6 | **Onarım kabul oranı ölçülür** | İddia ölçülmezse pazarlamadır | Bakım maliyeti verisi [S-42] |
| I-7 | **Ortam parmak izi alınır** | Staging kayması yanlış alarm üretir | API test zorlukları 2026 [S-43] |
| I-8 | **Altyapı hataları ayrı kodla `Broken`** | Flaky oranı kirlenmesin | Kendi statü tasarımımız |
| I-9 | **Tam + hedefli koşum** | Deterministik seçim; ML gerekmez | Azure TIA [S-44] |
| I-10 | **Dört güven metriği** | Strateji ancak ölçülene kurulur | Capgemini WQR: %57 strateji eksikliği [S-45] |
| I-11 | **UI testi kapsam dışı** | "Her şeyi UI'dan otomatize etmek" 1 numaralı kırılganlık sebebi | Bakım maliyeti analizleri [S-42] |

## 11.J İş senaryosu kararları

| # | Karar | Neden | Küresel kaynak |
|---|---|---|---|
| J-1 | **Koşullu akış birinci sınıf** | "Bilet varsa al" düz adım listesiyle ifade edilemez | Arazzo `onSuccess`/`goto` [S-10]; koşul-gizleme test kokusu [S-46] |
| J-2 | **`Inconclusive` ayrı statü** | Yeşil dönen ama hiçbir şey test etmeyen senaryo | Yanlış negatif literatürü [S-19]; işe yaramaz test kalıpları [S-47] |
| J-3 | **Önkoşul birinci sınıf, strateji kaydedilir** | "A saatinde bilet olmalı" gereksinimi açık olmalı | Precondition/postcondition pratiği [S-48] |
| J-4 | **Adım çıktısı olarak sayısal ölçüm** | Delta assertion'ın tek ön şartı | `ObservedRowCount` (K1) |
| J-5 | **İş değişmezi kataloğu M-1..M-10** | Oracle problemi ilişkiyle çözülür | Metamorphic testing of RESTful APIs [S-49]; ARMeta 2026 [S-50] |
| J-6 | **İki aşamalı artefakt (plan → senaryo)** | Markdown incelemek koddan ucuz | Playwright planner/generator [S-01] |
| J-7 | **Durum geçişi + yasak geçiş testi** | "Reddedildi" tek başına yetmez | M-8 + M-10; Schemathesis durum makinesi [S-51] |
| J-8 | **Göreli zaman ifadeleri** | Sabit tarihli senaryo ertesi gün çöp | Kendi tasarımımız + C-4 ile tutarlı |
| J-9 | **`ScenarioKind` ayrımı** | İki türün kararlılık profili farklı | Senaryo ≠ test durumu ayrımı [S-52] |
| J-10 | **Koşumda hâlâ model yok** | Koşul `if`, değişmez çıkarma işlemi | A-1 ile tutarlı |

## 11.K İş bilgisi katmanı kararları

| # | Karar | Neden | Küresel kaynak |
|---|---|---|---|
| K-1 | **Doküman verilecek — ve ayrıntısı başarıyı belirliyor** | 10 API/~1.000 endpoint'te **%89** elle düzeltmesiz test | APITestGenie AST 2026 [S-53] |
| K-2 | **Serbest metin değil yapılandırılmış** | Yönlendirilmemiş LLM belirtilmemiş koşul uydurur | RAGcceptance M2RE [S-54]; RAFT [S-55] |
| K-3 | **Dört bilgi katmanı (türet/gözlemle/beyan et/sor)** | Maliyet ve otorite farklı; hepsi gerekli | Daikon/AGORA+ [S-56] |
| K-4 | **Etki ayak izi** | Yan etkiyi öğrenmenin tek kesin yolu | AGORA+ önkoşul/sonkoşul çıkarımı [S-56] |
| K-5 | **İş kuralı = karar tablosu** | Sınır testleri otomatik türer | DMN [S-57] |
| K-6 | **Operasyon ilişki grafiği beş kaynaktan** | Tek kaynak yetersiz; güven derecesi gerekli | OpenAPI links + Schemathesis [S-51]; bilgi grafiği pratiği [S-58] |
| K-7 | **Öğrenilen bilgi kalıcılaşır** | Aynı şey iki kez keşfedilmez, iki kez sorulmaz | Kendi tasarımımız |
| K-8 | **Kural kapsam metriği** | "Kod çalıştı mı" değil "kural doğrulandı mı" | DMN doğrulama kapsamı eksikliği [S-57] |
| K-9 | **Olgunluk seviyeleri L0–L5** | Ürün L0'da da çalışmalı, benimsemeyi yönlendirmeli | Kendi tasarımımız |

---

# 12. Sorduğumuz sorular ve cevapları

| # | Soru | Cevap |
|---|---|---|
| S-01 | Asenkron bekleme 1 numaraysa süreyi yazmak yeter mi? | Hayır; p95 izlenir, bütçeye yaklaşınca uyarılır |
| S-02 | Sıra bağımlılığı mümkün mü? | Evet; sıra + izolasyon kaydı, karışık sıra denetimi, `OrderDependent` |
| S-03 | Oracle kırılganlığı nasıl engellenir? | Matcher kaydı + yayın kapısı uyarısı + açık tolerans |
| S-04 | Triyaj 28 dk'dan nasıl iner? | Teşhis sonucu koşum satırında hazır |
| S-05 | Test verisi izolasyonu modelde nasıl? | `DatasetRef` + `DatasetVersion`, içerik-adresli fixture |
| S-06 | Bakım kazancı nasıl kanıtlanır? | Öneri sayısı, kabul oranı, onaya geçiş süresi |
| S-07 | Ortam kayması yanlış alarm üretir mi? | Evet; koşuda ortam parmak izi alınır |
| S-08 | Token süresi dolması `Failed` mi `Broken` mı? | `Broken`, ayrı sonuç koduyla |
| S-09 | Her koşuda her senaryo koşulacak mı? | Hayır; tam koşu + hedefli koşu |
| S-10 | "Testlere güven" nasıl ölçülür? | flaky, yanlış alarm, karantina oranı, teşhis süresi |
| S-11 | AI ile test patlaması vurur mu? | İnsan onayı + yayın kapısı + senaryo bütçesi |
| S-12 | UI testi yapacak mıyız? | Bu turda hayır |
| S-13 | Şema adları? | `test_lookup`, `test_catalog`, `test_run` |
| S-14 | Bölümleme? | v1'de hayır; eşik 50M satır / 10 dk silme |
| S-15 | Dosyalar nerede? | S3-uyumlu nesne deposu |
| S-16 | `HistoryId` formülü? | `SHA-256(scenarioKey ¦ environmentKey ¦ kanonik girdiler)` |
| **S-17** | **Bilet yoksa senaryo ne döner?** | **`Inconclusive`** — yeşil değil |
| **S-18** | **"Koltuk azaldı mı" nasıl doğrulanır?** | Adım çıktısı olarak sayı taşınır, delta kurulur (M-2) |
| **S-19** | **Kota kuralı ajana nasıl aktarılır?** | Karar tablosu içeren kural dosyası; sınır testleri otomatik türer |
| **S-20** | **API uçlarının ilişkisi nasıl çözülür?** | Beş kaynak: `links`, şema eşleşmesi, `Location`, FK grafiği, etki ayak izi |
| **S-21** | **Ajana md mi vereceğiz?** | Evet — **yapılandırılmış** md; ayrıntı düzeyi başarıyı belirliyor |

---

# 13. Ekip bazlı notlar

## 13.1 Frontend

**Kavramlar:** finding, severity, changeState, run status, execution status (**altı değerli**),
oracle layer, evidence link, health state, **`Inconclusive`**, kural kapsamı.

**Ekran kararları:**
- Bulgu listeleri **sayfalı**; sonsuz kaydırma yerine sayfa + filtre
- Ağır içerik **link** olarak gelir, satır içinde gösterme
- **`Broken` ile `Failed` farklı renkte** — biri ortam, diğeri gerçek hata
- **`Inconclusive` üçüncü bir renk** — yeşil değil, kırmızı değil; "hiçbir şey doğrulanmadı"
- `Quarantined` build'i kırmaz; arayüzde açıkça belirt
- Onay kuyruğu bir **iş akışı** ekranıdır: yama farkı + gerekçe + onay/ret
- **Kural kapsam paneli**: hangi iş kuralı kaç senaryoyla test ediliyor

## 13.2 Test ekibi

**İlk fazda** Arazzo senaryosunu elle yazacaksın.

**Kurallar:**
- Anahtar olarak PK/tekil indeks kullan; değilse assertion çalışmaz
- Zaman damgası ve ondalıkta `equals` kullanma → `withinTolerance`
- Sırasız listede sıra bağımlı karşılaştırma yapma
- Bekleme süresini senaryoda **açıkça** yaz
- Senaryo ortam adresi bilmez; mantıksal ad kullanır
- **Mutlu yol yetmez:** her iş kuralı için sınır ve negatif senaryo yaz (M-8)
- **Önkoşulu açıkça belirt**; `Discover` kullanıyorsan `Inconclusive` ihtimalini kabul et

## 13.3 Backend

**Katman:** `Controller → AppService → Manager → Repository`
**Entity:** veri kabuğu; kural Manager'da
**Sabit metin:** Domain.Shared
**Şema:** yalnız kendi şemamızın migration sahibiyiz; modüller arası FK yok

## 13.4 Yeni başlayan

1. §0'ı oku — ürünün teknik test aracı **olmadığını** anla
2. §1 Sözlük (özellikle **finding**, **oracle**, **iş değişmezi**, **`Inconclusive`**)
3. §4 örneği satır satır takip et
4. §9 değişmez kataloğunu ezberle
5. §14 SSS

---

# 14. Sık sorulan sorular

**"Bu sadece teknik test aracı mı?"**
Hayır. Teknik doğrulama tabandır. Amaç iş senaryolarını test etmek: koşullu akış,
iş kuralları, kotalar, durum geçişleri, yan etkiler.

**"Yapay zekâ testi mi yazacak, biz mi?"**
İkisi de. Ajan taslağı üretir, insan onaylar. Onaysız senaryo yayına girmez.

**"Test koşarken yapay zekâ para yakacak mı?"**
Hayır. Koşumda model **hiç** devrede değil.

**"Ajan iş kurallarımızı nereden bilecek?"**
Dört kaynaktan: türetir (OpenAPI/FK), gözlemler (etki ayak izi), okur (sözlük + kural
kataloğu), sorar. Ölçülmüş sonuç: gereksinim ayrıntısı arttıkça başarı oranı artıyor.

**"Kural dosyalarını yazmak zorunda mıyız?"**
Hayır ama seviyeniz L0-L1'de kalır. Kural kataloğu yazarsanız L4'e çıkar ve **sınır ile
negatif senaryolar otomatik türer**.

**"Bir test kırıldığında ne göreceğim?"**
Hangi adımın hangi oracle katmanında patladığını, checker'ın sıralı hipotezini ve güvenini.

**"Checker'lar veritabanımı bozabilir mi?"**
Hayır. Salt-okunur, `READ ONLY` transaction, serbest SQL yok.

**"Müşteri verisi raporlara sızar mı?"**
Varsayılan olarak hayır; kanıt "beklenen vs gerçek" şeklini taşır, değeri taşımaz.

**"Senaryolar Git'te mi?"**
Senaryolar veritabanında (içerik hash'li, sürümlü). **İş bilgisi dosyaları Git'te**
(`domain/*.md`), veritabanına türev olarak yüklenir.

---

# 15. Yol haritası

| Faz | İçerik | Durum |
|---|---|---|
| Checker'lar | İki checker: oracle, teşhis, bulgu kalitesi | ✅ API `0.2.0-alpha.7`, Database `0.2.0-alpha.8` yayımlandı |
| Araştırma | 13 araştırma belgesi, 3 plan, 1 backlog | ✅ Kapandı |
| Karar | ADR-0011 (**silindi**) → **ADR-0014 yazarlık · ADR-0015 koşum · ADR-0016 kayıt** | ✅ Karara bağlandı (2026-08-13) |
| T1 | Şemalar (**4 ana + 5 lookup**), senaryo saklama, Arazzo lint + `x-checknexus-db` derleyicisi, **runner adapter'ı** | ⏳ |
| T2 | HAR artefaktı, saklama politikası, CTRF/JUnit | ⏳ |
| T3 | Yazım ajanı, MCP köprüsü, onay akışı | ⏳ |
| T4–T5 | Teşhis bağlama, etki analizi, Overlay yama | ⏳ |
| T6 | Sağlık, karantina, SARIF | ⏳ |
| **T7** | **İş senaryosu: koşullu akış, önkoşul, değişmezler** | ⏳ |
| **T8** | **İş bilgisi: sözlük, kural kataloğu, etki ayak izi** | ⏳ |

**T1 kabul ölçütü:** Elle yazılmış senaryo uçtan uca yeşil koşuyor ve **tek satır model
çağrısı yok.**

---

# 16. Küresel kaynak künyesi

| Kod | Kaynak | Neyi kanıtlıyor |
|---|---|---|
| S-01 | playwright.dev/docs/test-agents | planner/generator/healer üretimde; koşum düz test; `seed.spec.ts` |
| S-02 | Playwright MCP token ölçümleri | ~114K vs ~27K token/test |
| S-03 | dl.acm.org/doi/10.1145/3715107 | LLM oracle'ların sınırları |
| S-04 | arxiv.org/html/2607.06195 (LogicHunter) | Pasif LLM hakem düşük precision |
| S-05 | Supabase MCP olayı / lethal trifecta | Veri sızdırma zinciri |
| S-06 | milanjovanovic.tech Testcontainers | Rollback'in kırıldığı durumlar |
| S-07 | deepwiki.com/github/github-mcp-server | Toolset seçimiyle %60–90 bağlam düşüşü |
| S-08 | jentic.com/blog/the-mcp-tool-trap | Tool sayısıyla bozulan doğruluk |
| S-09 | arxiv.org/html/2507.16044v4 | REST→MCP: 116 sunucu, seçici tool tasarımı |
| S-10 | spec.openapis.org/arazzo/latest.html | Arazzo 1.1.0: adım, successCriteria, retry, branching |
| S-11 | spec.openapis.org/overlay/v1.0.0.html | Overlay: JSONPath target + update/remove |
| S-12 | modelcontextprotocol.io/extensions/tasks | Tasks: 5 durum, `input_required`, kooperatif iptal |
| S-13 | ctrf.io/docs/full-schema | CTRF tam şeması |
| S-14 | opentelemetry.io/docs/specs/semconv/registry/attributes/test | `test.*` öznitelikleri (4 alan) |
| S-15 | docs.oasis-open.org/sarif/sarif/v2.1.0 | SARIF 2.1.0 |
| S-16 | docs.qameta.io/allure-testops/briefly/test-results | AllureID/testCaseId/historyId; 5 statü |
| S-17 | docs.pact.io/getting_started/versioning_in_the_pact_broker | İçerik hash'iyle dedup; matrix |
| S-18 | kiwitcms.readthedocs.io .../testruns/models.html | `case_text_version`; statü lookup'ı |
| S-19 | ontestautomation.com/on-false-negatives-and-false-positives | Yanlış negatif daha tehlikeli |
| S-20 | crunchydata.com/blog/five-great-features-of-postgres-partition-manager | Zaman bazlı partition + retention |
| S-21 | reportportal.io/docs/developers-guides/ReportingDevelopersGuide | Postgres + nesne deposu + log indeksi |
| S-22 | abp.io/docs/latest/framework/infrastructure/blob-storing | BLOB container ve sağlayıcılar |
| S-23 | abp.io/docs/latest/framework/architecture/multi-tenancy | Çok kiracılılık altyapısı |
| S-24 | learn.temporal.io .../durable-execution | Deterministik replay, activity retry |
| S-25 | abp.io/docs/latest/framework/infrastructure/background-jobs | Arka plan işleri + distributed lock |
| S-26 | dl.acm.org/doi/10.1145/2635868.2635920 (Luo, FSE 2014) | Flakiness kök sebepleri: async wait, concurrency, order |
| S-27 | genai.owasp.org/resource/owasp-genai-llm-top-10-2026 | LLM01 injection; LLM03 excessive agency 6.→3. |
| S-28 | GDPR md.5 saklama sınırlaması / veri minimizasyonu | Kanıt saklama politikası |
| S-29 | Self-healing eleştirisi (testomat/qaskills/autonoma) | Sessiz onarım gerçek hatayı gizler |
| S-30 | docs.datadoghq.com/tests/flaky_management | Active/Quarantined/Disabled/Fixed |
| S-31 | datadoghq.com/blog/engineering/mcp-server-agent-tools | Token bütçeli sayfalama; TSV %50; alan kırpma; öğreten hata |
| S-32 | modelcontextprotocol.io/specification/2026-07-28/server/tools | outputSchema, resource_link, ttlMs, handle rehberi |
| S-33 | anthropic.com/engineering/advanced-tool-use | Tool Search %85; doğruluk 49→74; örnekler %72→%90 |
| S-34 | RAG-MCP | Tool seçim doğruluğu %13→%43 |
| S-35 | arxiv.org/html/2506.01056v1 (MCP-Zero) | Bağlamda iki mertebe azalma |
| S-36 | atlan.com .../agent-skills-vs-mcp | Skill ~100 token vs tool tam yükleme |
| S-37 | anthropic.com/engineering/code-execution-with-mcp | 150K→2K (%98,7) |
| S-38 | arxiv.org/html/2602.03556 (SAP HANA 2026) | Async wait baskınlığı sürüyor |
| S-39 | engr.ship.edu/~chuo/papers/huo14.pdf | Kırılgan assertion tespiti |
| S-40 | flakyguard.com/blog/cost-of-flaky-tests | Triyaj medyanı 28 dk; ekip maliyeti |
| S-41 | totalshiftleft.ai .../test-data-management-best-practices-api-testing | İzole veri kümesi, fixture sürümleme |
| S-42 | rainforestqa.com/blog/test-automation-maintenance | Bakım QA eforunun %30–50'si; locator başına 15 dk |
| S-43 | cloudqa.io/why-traditional-e2e-api-testing-is-failing-in-2026 | Ortam kayması, token/rate limit |
| S-44 | learn.microsoft.com .../test-impact-analysis | Deterministik etki analizi |
| S-45 | Capgemini World Quality Report 2024-25 | %57 strateji eksikliği |
| S-46 | arxiv.org/pdf/2606.13804 | Koşul gizleyen adım = test kokusu |
| S-47 | getautonoma.com/blog/useless-unit-tests-tautological-anti-pattern | İşe yaramaz test kalıpları; yanlış güven |
| S-48 | testsigma.com/blog/precondition-in-test-case | Önkoşul/sonkoşul kavramı |
| S-49 | javiertroyauma.github.io .../TSE2017_REST_prePrint.pdf | Metamorphic testing of RESTful APIs; MROP |
| S-50 | arxiv.org/html/2605.28321v1 (ARMeta 2026) | OpenAPI'den LLM ile metamorphic test; Gherkin |
| S-51 | schemathesis.readthedocs.io .../stateful | OpenAPI links; üretici→tüketici; durum makinesi |
| S-52 | headspin.io/blog/test-scenarios-comprehensive-guide | Senaryo = *ne*, test durumu = *nasıl* |
| S-53 | dl.acm.org/doi/full/10.1145/3793654.3793743 (APITestGenie, AST 2026) | Gereksinim+OpenAPI+RAG → **%89** elle düzeltmesiz test |
| S-54 | arxiv.org/pdf/2508.06888 (RAGcceptance M2RE) | Yönlendirilmemiş LLM koşul uydurur |
| S-55 | arxiv.org/html/2601.09762 (RAFT) | Örtük bilgi → yapılandırılmış artefakt |
| S-56 | personales.us.es/sergiosegura/.../alonso25-tosem.pdf (AGORA+) | Operasyon seviyesinde önkoşul/sonkoşul çıkarımı |
| S-57 | omg.org/spec/DMN | Karar tabloları + FEEL; doğrulama kapsamı eksikliği |
| S-58 | atlan.com .../knowledge-graph-construction-for-ai | Çıkarım ucuz, varlık çözümleme zor |
</content>
