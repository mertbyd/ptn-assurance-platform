---
kind: rules
id: RULES-BILET-001
title: Bilet satis is kurallari
version: 7
updated: 2026-08-14
owners:
  - urun.sahibi@ornek-musteri.tr
domain: bilet-satis
authoring_mode: A
profile_pack: ptn-profile-pack/bilet-satis@r14
rule_ids: [BR-011, BR-012, BR-013, BR-014, BR-015]
journey_ids: [JR-002]
decision_refs: [ADR-0014, ADR-0017, ADR-0019, ADR-0020]
rule_refs: [RULE-0006, RULE-0007, RULE-0008]
---

# `kurallar.md` — Bilet satış iş kuralları

> [!IMPORTANT] Bu dosya bir **örnektir**, kanonik bilgi değildir.
> Amacı, [[03-Decisions/ADR-0014-Senaryo-Yazarlik-Modeli-Ve-Turetilebilirlik-Kapisi|ADR-0014]] §A'nın
> *"niyet girdisi"* dediği dosyanın **gerçekte nasıl görüneceğini** göstermektir.
> Eşi: [[99-Templates/ornek-giris/senaryo|senaryo.md]].

## 0. Bu dosya nedir, ne değildir

| | |
|---|---|
| **Rolü** | An 1 girdisi — ajanın **niyet** kaynağı ([[04-Architecture/Alti-An\|ARCH-0004]]) |
| **Nerede yaşar** | Git. **Veritabanı tablosu değildir** (ADR-0014 §A) |
| **Ajana nasıl ulaşır** | MCP **`Resource`** → `ptn_knowledge` tool'u. Ajan 40 sayfayı bağlamına almaz; **tek kural** çeker |
| **Koşuda ne kalır** | Yalnız `test_scenarios.rules_fingerprint` — bu dosyanın kanonik hash'i |
| **Kim yazar** | **İnsan.** Ajan yalnız SBVR/EARS ifadesi *önerir*; onay insanındır (ADR-0017 §A) |
| **Ne değildir** | Test dosyası değil, Arazzo değil, DMN XML değil. Bunların **kaynağıdır** |

Üç katman, üç okuyucu (ADR-0017 §E). Bu dosya ilk iki katmanı taşır:

| Katman | Kime | Bu dosyada |
|---|---|---|
| **Anlatım** | iş insanı | SBVR Structured English + **iki somut örnek** |
| **İnceleme** | onaylayan | **DMN karar tablosu** |
| İcra | runner | ✗ — Arazzo'da, bu dosyada değil |

> [!WARNING] Belirsiz `kurallar.md` kabul edilmez
> Ölçüm net: net gereksinimde %13–92, belirsizde %2–54; **belirsiz gereksinimde %90'a ulaşan
> model yok** (RESEARCH-0014 §2). Fark **26–40 puan** ve bunu hiçbir model yükseltmesi vermez.
> Bu yüzden §7'deki açık kırmızı kart, ilgili kuraldan senaryo türetilmesini **engeller**.

---

## 1. İş sözlüğü

Bu bölüm değişmez seçimini **deterministik** yapar. Kalıp seçimi ajana bırakılmaz
(RESEARCH-0014 §12.6).

| Terim | Tanım | Operasyonlar | Tablolar | Kimlik | Değişmezler | Kavram (ADR-0019 §B) |
|---|---|---|---|---|---|---|
| **bilet** | Bir yolcunun belirli bir seferdeki koltuk hakkı | `searchTrips`, `purchaseTicket`, `getTicket`, `cancelTicket` | `sales.Tickets` (ana), `sales.TicketHistory` (arşiv) | `Tickets.Id` | M-2, M-4, M-6 | `Resource` |
| **koltuk** | Bir araçtaki fiziksel yer | — | `sales.Seats`, sayaç: `sales.Trips.AvailableSeats` | `Seats.(TripId, SeatNo)` | M-1, M-7 | `Resource` |
| **sefer** | Bir aracın belirli tarih/saatteki yolculuğu | `searchTrips`, `getTrip` | `sales.Trips` | `Trips.Id` | M-1 | `TimeAnchor` |
| **ödeme** | Bilet karşılığı tahsilat | `capturePayment`, `refundPayment` | `billing.Payments` | `Payments.Id` | M-3, M-6 | `Resource` |
| **yolcu** | Bilet alan kişi | `getPassenger` | `identity.Passengers` | `Passengers.Id` | M-9 | **`Subject`** |
| **öğrencilik** | Yolcunun doğrulanmış öğrenci belgesi | — | `identity.PassengerDocuments` | `(PassengerId, DocumentType)` | — | `RoleAssignment` |
| **dönem** | Öğrenci kotasının sıfırlandığı takvim aralığı | — | `sales.Terms` | `Terms.Id` | — | `TimeAnchor` |

> **Kavram sütunu neden var:** somut tablo/kolon eşlemesini **profil manifesti** yapar, ajan
> değil (ADR-0019 §B). Bağlanmamış kavram tahmin değil `NOT_BOUND` üretir ve §7'deki kırmızı
> karta düşer — *"kanıt toplanamadı"*, *"yetki yok"* değil (ADR-0019 §C).

### 1.1 Kolon anlamı — sessiz varsayıma kapalı

| Kolon | Anlamı | Neden yazılı |
|---|---|---|
| `Trips.AvailableSeats` | `Seats.Status = 'Available'` satır sayısının **denormalize sayacı** | İki kaynak var; hangisinin oracle olduğu yazılmazsa M-2 yanlış tabloya bakar |
| `Seats.Status` | `Available` \| `Reserved` \| `Sold` | *"`Reserved` boş sayılır mı"* sorusu §6'da cevaplandı: **hayır** |
| `Tickets.IsStudentFare` | İndirimli tarife bayrağı | BR-014 sayacının kaynağı |
| `Tickets.TermId` | Biletin sayıldığı dönem | Kota penceresinin **veri karşılığı** |

---

## 2. Yolculuk — `JR-002` bilet yaşam döngüsü

```
Reserved ──► Paid ──► Confirmed ──► Used
    │                      │
    └──► Expired           └──► Cancelled ──► Refunded
```

### 2.1 İzinli geçişler

| # | Kaynak | Hedef | Tetikleyen | Ön koşul |
|---|---|---|---|---|
| T1 | `Reserved` | `Paid` | `capturePayment` | rezervasyon süresi (15 dk) dolmamış |
| T2 | `Paid` | `Confirmed` | otomatik | ödeme sağlayıcı onayı geldi |
| T3 | `Confirmed` | `Used` | `checkIn` | sefer kalkış saati geldi |
| T4 | `Confirmed` | `Cancelled` | `cancelTicket` | kalkıştan **≥ 2 saat** önce (BR-015) |
| T5 | `Cancelled` | `Refunded` | otomatik | iade politikası uygun |
| T6 | `Reserved` | `Expired` | otomatik | 15 dk içinde ödeme yok |

### 2.2 Yasak geçişler — **test edilmesi zorunlu**

| # | Geçiş | Beklenen | Hata kodu | Yan etki |
|---|---|---|---|---|
| X1 | `Cancelled → Paid` | red | `InvalidStateTransition` | `billing.Payments` satırı **oluşmaz** |
| X2 | `Used → Cancelled` | red | `TicketAlreadyUsed` | `sales.Tickets.Status` **değişmez** |
| X3 | `Expired → Paid` | red | `ReservationExpired` | koltuk **serbest kalır** |

> Yasak geçiş testi iki parçalıdır: *"reddedildi"* yeterli değil, **yan etki oluşmadığı** da
> kanıtlanmalıdır — M-10 + M-8, `AssertAbsent` (RESEARCH-0014 §12.4).

---

## 3. İş kuralları

Her kural dört blok taşır: **Anlatım** (SBVR) · **Örnekler** (Specification by Example) ·
**Karar tablosu** (DMN) · **Doğrulama yükümlülüğü**.

Karar tablosu satır kimlikleri `BR-0xx#Rn` biçimindedir ve bulguya
`test_result_findings.rule_ref` olarak düşer. Ters okunduğunda **kural kapsam raporunu** verir
([[02-Rules/RULE-0006-Turetilemeyen-Assertion-Yayinlanamaz|RULE-0006]]).

---

### BR-011 — Satış penceresi kesimi

```yaml
id: BR-011
scope: sefer
appliesTo: [ purchaseTicket ]
errorCode: SalesWindowClosed
hitPolicy: U          # Unique — dmn-check bosluk/ortusme dogrular
requiresTestClock: true
```

#### Anlatım (SBVR Structured English)

> **Bir sefer için bilet satın alınabilmesi zorunlu olarak** satış anının, o seferin
> **satış kapanış anından önce** olmasını gerektirir.
> **Satış kapanış anı**, seferin kalkış anından **15 dakika** öncesidir.

#### Örnekler

```
✓ Sefer 10:00 kalkislidir, saat 09:30'dur   → bilet alinabilir
✗ Sefer 10:00 kalkislidir, saat 09:50'dir   → alinamaz (SalesWindowClosed)
```

#### Karar tablosu

| Satır | `now` vs `departAt − 15dk` | `Trips.Status` | Sonuç | Hata kodu |
|---|---|---|---|---|
| `#R1` | `<` (pencere açık) | `Scheduled` | **Allow** | — |
| `#R2` | `>=` (pencere kapalı) | `Scheduled` | **Deny** | `SalesWindowClosed` |
| `#R3` | `*` | `Cancelled` | **Deny** | `TripCancelled` |
| `#R4` | `*` | `Departed` | **Deny** | `TripDeparted` |

#### Sınır değerler — MC/DC (ε = 1 saniye)

| Nokta | `now` | Beklenen |
|---|---|---|
| eşik − ε | `departAt − 15dk − 1sn` | **Allow** (`#R1`) |
| eşik | `departAt − 15dk` | **Deny** (`#R2`) ← asıl sınır |
| eşik + ε | `departAt − 15dk + 1sn` | **Deny** (`#R2`) |

#### Doğrulama yükümlülüğü

> [!WARNING] `#R1` kapsanmazsa aşırı-engelleme görünmez
> [[02-Rules/RULE-0008-Cift-Yonlu-Kural-Kapsami|RULE-0008]]'in birebir vakası: yalnız `#R2`
> test edilirse, sistem **hiç bilet satmıyor** olsa bile testler yeşil kalır.
> `#R1` **Allow** satırı kapsanmadan bu kuraldan üretilen sürüm **yayınlanamaz**.

- Reddedildiğinde: HTTP `409` + `SalesWindowClosed` **ve** `sales.Tickets` satır sayısı
  değişmez **ve** `billing.Payments` satırı oluşmaz (M-8).
- Değişmez bağı: M-8, M-10.

> [!IMPORTANT] SUT şartı — test saati
> Bu kural zaman bağımlıdır. SUT'ta **test saati** (Stripe Test Clocks / `TimeProvider` deseni)
> yoksa `#R2`/`#R3`/`#R4` koşulamaz ve sonuç `Passed` değil **`Inconclusive`** işaretlenir
> (RULE-0008 istisna süreci, ADR-0017 §I). Bu ürün tarafında çözülemez; §8'de beyan edilmiştir.

---

### BR-012 — Koltuk tekilliği

```yaml
id: BR-012
scope: sefer
appliesTo: [ purchaseTicket ]
errorCode: SeatAlreadyTaken
hitPolicy: U
```

#### Anlatım

> **Bir seferdeki bir koltuk için en fazla bir aktif bilet bulunması zorunludur.**
> Aktif bilet, durumu `Reserved`, `Paid`, `Confirmed` veya `Used` olan bilettir.

#### Örnekler

```
✓ 12A koltugu Available, Ali 12A istiyor        → alabilir
✗ 12A koltugu Sold, Veli 12A istiyor            → alamaz (SeatAlreadyTaken)
```

#### Karar tablosu

| Satır | `Seats.Status` | Aktif bilet sayısı | Sonuç | Hata kodu |
|---|---|---|---|---|
| `#R1` | `Available` | `0` | **Allow** | — |
| `#R2` | `Reserved` | `>= 1` | **Deny** | `SeatAlreadyTaken` |
| `#R3` | `Sold` | `>= 1` | **Deny** | `SeatAlreadyTaken` |
| `#R4` | `Available` | `>= 1` | **Deny** | `SeatStateCorrupt` (tutarsızlık alarmı) |

#### Doğrulama yükümlülüğü

- **Anahtar tekilliği ön şarttır:** `sales.Tickets` üzerinde `(TripId, SeatNo)` **unique**
  olmalıdır. Değilse assertion `KeyNotUnique` döner ve bunu **yayında** yakalamak gerekir
  (RULE-0006 doğrulama maddesi).
- Değişmez bağı: **M-4 tekillik** — `AssertCount` + `cardinality: exactly 1`.
  Bu kalıp bugünkü DB Checker yüzeyiyle **yeni uç gerektirmeden** karşılanır
  (RESEARCH-0014 §12.4).
- `#R4` bir **veri tutarsızlığı** satırıdır: kural ihlali değil, alarm. Bulgu
  `failure_category = Persistence` taşır.

---

### BR-013 — Grup rezervasyonu

```yaml
id: BR-013
scope: rezervasyon
appliesTo: [ purchaseTicket ]
errorCode: GroupSizeExceeded
hitPolicy: U
status: BLOCKED          # §7 K-07 cevaplanmadan senaryo turetilemez
```

#### Anlatım (taslak — onaylanmadı)

> **Tek bir rezervasyonda en fazla 6 koltuk alınabilir.**

> [!CAUTION] Bu kuraldan **senaryo türetilemez**
> §7'deki **K-07** kırmızı kartı açıktır: *"6 sınırı tek işlemde mi, aynı yolcunun aynı
> seferdeki toplamında mı?"* Cevap gelmeden karar tablosu yazılamaz; yazılırsa ölçülmüş
> 26–40 puanlık belirsizlik kaybı doğrudan üretime geçer (RESEARCH-0014 §2).
>
> Mod **A** (soran) etkin olduğu için hat burada **durur** (ADR-0017 §F).
> Mod **B** açık olsaydı varsayım işaretlenir, **"%100 doğrulanmış" iddiası düşerdi.**

---

### BR-014 — Öğrenci indirimli bilet kotası

```yaml
id: BR-014
scope: donem
appliesTo: [ purchaseTicket ]
errorCode: StudentQuotaExceeded
hitPolicy: U
```

#### Anlatım

> **Bir öğrenci için dönem başına en fazla 2 adet indirimli bilet alınması zorunludur.**
> Kota dolduğunda satın alma reddedilir **ve hiçbir yan etki oluşmaz.**

#### Örnekler

```
✓ Ali ogrenci, bu donem 1 indirimli bileti var   → ikincisini alabilir
✗ Ali ogrenci, bu donem 2 indirimli bileti var   → alamaz (StudentQuotaExceeded)
```

#### Karar tablosu

| Satır | Öğrenci belgesi | Dönem içi indirimli bilet | Sonuç | Hata kodu |
|---|---|---|---|---|
| `#R1` | yok | `*` | **Allow** (normal tarife) | — |
| `#R2` | var | `0` | **Allow** (indirimli) | — |
| `#R3` | var | `1` | **Allow** (indirimli) | — |
| `#R4` | var | `2` | **Deny** | `StudentQuotaExceeded` |
| `#R5` | var | `> 2` | **Deny** (tutarsızlık alarmı) | `QuotaStateCorrupt` |

#### Sayaç kaynağı

`sales.Tickets` içinde `IsStudentFare = true` **ve** `TermId = <aktif dönem>` **ve**
`Status IN ('Reserved','Paid','Confirmed','Used')` satır sayısı.

> İptal edilmiş bilet kotayı **iade eder** — §6 **K-02** ile cevaplandı.

#### Sınır değerler — MC/DC

| Nokta | Sayaç | Beklenen |
|---|---|---|
| eşik − ε | `1` | **Allow** (`#R3`) |
| eşik | `2` | **Deny** (`#R4`) ← asıl sınır |
| eşik + ε | `3` | **Deny** (`#R5`, alarm) |

#### Doğrulama yükümlülüğü

- Reddedildiğinde (`#R4`): HTTP `409` + `StudentQuotaExceeded` **ve** `sales.Tickets` satır
  sayısı değişmez **ve** `billing.Payments` satırı **oluşmaz** — M-8'in iki parçası da
  (`AssertAbsent`).
- `#R1`, `#R2`, `#R3` **Allow** satırları kapsanmadan yayın **reddedilir** (RULE-0008).
- Değişmez bağı: M-2 (sayaç deltası), M-8 (negatif yol).

---

### BR-015 — İptal ve iade penceresi

```yaml
id: BR-015
scope: bilet
appliesTo: [ cancelTicket, refundPayment ]
errorCode: CancellationWindowClosed
hitPolicy: U
requiresTestClock: true
```

#### Anlatım

> **Bir biletin iptal edilebilmesi zorunlu olarak**, iptal anının seferin kalkış anından
> **en az 2 saat önce** olmasını gerektirir.
> İptal edilen biletin ödemesi **tam tutarıyla** iade edilir.

#### Örnekler

```
✓ Sefer 10:00, saat 07:30, bilet Confirmed  → iptal edilir, tam iade
✗ Sefer 10:00, saat 08:30, bilet Confirmed  → iptal edilemez (CancellationWindowClosed)
```

#### Karar tablosu

| Satır | `departAt − now` | `Tickets.Status` | Sonuç | İade | Hata kodu |
|---|---|---|---|---|---|
| `#R1` | `>= 2sa` | `Confirmed` | **Allow** | tam tutar | — |
| `#R2` | `>= 2sa` | `Paid` | **Allow** | tam tutar | — |
| `#R3` | `< 2sa` | `Confirmed` | **Deny** | — | `CancellationWindowClosed` |
| `#R4` | `*` | `Used` | **Deny** | — | `TicketAlreadyUsed` |
| `#R5` | `*` | `Cancelled` | **Deny** | — | `AlreadyCancelled` |

#### Sınır değerler — MC/DC (ε = 1 saniye)

`departAt − 2sa − 1sn` → **Allow** · `departAt − 2sa` → **Allow** (sınır dahil) ·
`departAt − 2sa + 1sn` → **Deny**

#### Doğrulama yükümlülüğü

- İptal sonrası: `sales.Seats.Status` `Available`'a döner (M-1 korunum),
  `billing.Payments` iade satırı **oluşur** (M-3 tutarlılık: iade tutarı = ödenen tutar).
- `#R5` idempotans satırıdır: ikinci iptal **yeni yan etki üretmez** (M-6).
- SUT şartı: **test saati** — BR-011 ile aynı (§8).

---

## 4. İş değişmezi eşlemesi

Katalog [[05-Operations/Ekip-Kilavuzu|GUIDE-0004]] §9'dadır. Bu alanda kullanılanlar:

| # | Kalıp | Bu alandaki karşılığı | Derleme | Yeni uç? |
|---|---|---|---|---|
| **M-1** | Korunum | `Available + Reserved + Sold = Trips.Capacity` | `invariants/check` `Conservation` | evet |
| **M-2** | Delta | satış → `AvailableSeats` **tam 1** azalır | önce-ölç → işlem → sonra-ölç → `Delta` | evet |
| **M-3** | Tutarlılık | ödenen tutar = aramadaki fiyat | iki ölçüm → `Equality` | evet |
| **M-4** | Tekillik | koltuk başına `exactly 1` aktif bilet | `AssertCount` | **hayır — bugün var** |
| **M-6** | İdempotans | aynı `idempotencyKey` ile iki satış → tek bilet | `IdempotentOutcome` + `AssertCount` | evet |
| **M-7** | Monotonluk | satış sonrası boş koltuk **artmaz** | `Monotonic` | evet |
| **M-8** | Negatif yol | red **ve** satır oluşmaz | native status + `AssertAbsent` | **hayır — bugün var** |
| **M-9** | Yetki sınırı | A kiracısı B'nin biletini göremez | ikinci token + native `403` | **hayır — bugün var** |
| **M-10** | Durum geçişi | §2.2 yasak geçişler | native status + `AssertRow` + `AssertAbsent` | **hayır — bugün var** |

> [!NOTE] Aritmetik Arazzo'da yazılamaz
> `koltukSonra == koltukÖnce - 1` **native kriter olamaz**; Arazzo `Criterion.simple`
> aritmetik operatör tanımlamaz (ADR-0017 §C). Delta/korunum/monotonluk ilişkileri
> `POST /invariants/check` ucundan geçer; Arazzo yalnız değerleri taşır ve
> `passed == true` karşılaştırmasını yapar.

---

## 5. Hata kodu sözlüğü

Kapalı küme. Ajan **kod yazmaz, seçer** ([[02-Rules/RULE-0007-Ajan-Tahmin-Etmez-Ve-Tool-Butcesi|RULE-0007]] §1).

| Kod | HTTP | Kural | Kategori |
|---|---|---|---|
| `SalesWindowClosed` | 409 | BR-011 `#R2` | `Business` |
| `TripCancelled` | 409 | BR-011 `#R3` | `Business` |
| `TripDeparted` | 409 | BR-011 `#R4` | `Business` |
| `SeatAlreadyTaken` | 409 | BR-012 `#R2`,`#R3` | `Business` |
| `SeatStateCorrupt` | 500 | BR-012 `#R4` | `Persistence` |
| `StudentQuotaExceeded` | 409 | BR-014 `#R4` | `Business` |
| `QuotaStateCorrupt` | 500 | BR-014 `#R5` | `Persistence` |
| `CancellationWindowClosed` | 409 | BR-015 `#R3` | `Business` |
| `TicketAlreadyUsed` | 409 | BR-015 `#R4`, JR-002 `X2` | `Business` |
| `AlreadyCancelled` | 409 | BR-015 `#R5` | `Business` |
| `InvalidStateTransition` | 409 | JR-002 `X1` | `Business` |
| `ReservationExpired` | 409 | JR-002 `X3` | `Business` |

---

## 6. Belirsizlik kaydı — sorulmuş ve cevaplanmış

Mod **A**: belirsizlikte hat durur, **kapalı uçlu** sorulur, cevap **bu dosyaya geri yazılır**
ve **aynı soru bir daha sorulmaz** (ADR-0017 §F). Bu bölüm o kurumsal hafızadır.

| # | Tip | Soru | Cevap | Yazıldığı yer | Tarih |
|---|---|---|---|---|---|
| **K-01** | vagueness | *"Tek bilet"* hangi pencerede? (a) aynı sefer (b) aynı gün (c) aynı anda aktif (d) ömür boyu | **(c)** → dönem penceresine bağlandı | BR-014 sayaç kaynağı | 2026-08-11 |
| **K-02** | incompleteness | Bilet iptal edilirse kota iade edilir mi? (a) evet (b) hayır | **(a) evet** | BR-014 sayaç kaynağı `Status` filtresi | 2026-08-11 |
| **K-03** | vagueness | *"Saati geçmiş"* kesim noktası kalkış saati mi, satış kapanışı mı? (a) kalkış (b) kalkış − 15dk | **(b)** | BR-011 anlatım + `#R1`/`#R2` | 2026-08-12 |
| **K-04** | referential | Senaryodaki Ali öğrenci mi? (a) evet (b) hayır (c) senaryoya göre değişir | **(c)** → aktör bloğunda beyan edilir | `senaryo.md` §1 | 2026-08-12 |
| **K-05** | vagueness | Boş koltuk sayarken `Reserved` dahil mi? (a) yalnız `Available` (b) `Available + Reserved` | **(a)** | §1.1 `Seats.Status` | 2026-08-13 |
| **K-06** | overlap | BR-012 `#R1` ile `#R4` aynı `Seats.Status` değerinde çelişiyor | `#R4` **tutarsızlık alarmı** olarak ayrıldı, hit policy `U` korundu | BR-012 karar tablosu | 2026-08-13 |

> Sorular **kapalı uçlu ve seçeneklidir**; serbest metin cevap istenmez. Soru sorma kararını
> **analiz** verir, model değil — DMN boşluk/örtüşme analizi ve varlık eşleşmesi
> (ADR-0017 §D).

---

## 7. Açık kırmızı kartlar — **yayını engelleyen**

Example Mapping'in 🟥 kartı. Kart yoğunluğu bir **hazırlık ölçüsüdür**: *"çok kırmızı kart
varsa bu hikâye geliştirmeye hazır değil"* (RESEARCH-0014 §7.2).

| # | Tip | Soru | Bloke ettiği | Durum |
|---|---|---|---|---|
| **K-07** | vagueness | Grup sınırı **6**, tek işlemde mi yoksa aynı yolcunun aynı seferdeki toplamında mı? (a) tek işlem (b) yolcu+sefer toplamı (c) yolcu+dönem toplamı | **BR-013** | 🟥 **AÇIK** |
| **K-08** | incompleteness | `Refunded` durumundaki bilet yeniden satın alınabilir mi? Kural JR-002'de `Refunded` çıkışını tanımlamıyor | JR-002 `T5` sonrası | 🟥 **AÇIK** |

**Sonuç:** BR-013 ve `Refunded` sonrası akış için **senaryo türetilemez**. Diğer dört kural
(BR-011, BR-012, BR-014, BR-015) ve JR-002'nin `X1`–`X3` yasak geçişleri **nettir ve
yayınlanabilir**.

---

## 8. SUT'tan beklenenler

Bu maddeler **ürün tarafında çözülemez**; entegrasyon şartıdır (ADR-0017 §I).

| # | Beklenti | Olmazsa |
|---|---|---|
| S-1 | **Test saati** — zamanı ileri alan test-modu API'si (yalnız ileri) | BR-011 ve BR-015 `Inconclusive`; **`Passed` sayılmaz** |
| S-2 | **Idempotency key** — `purchaseTicket` başlığı | M-6 idempotans testi üretilemez |
| S-3 | **Sandbox veri kümesi** — `Arrange` stratejisi için tohumlanabilir sefer/koltuk | Önkoşul `Discover`'a düşer, `inconclusive_rate` yükselir |
| S-4 | **İkinci kiracı token'ı** — yetki sınırı testi için | M-9 üretilemez |

---

## 9. Kapsam beyanı

RULE-0008: karar tablosunun **her satırı** en az bir testle kapsanmalıdır — `Deny` satırları
kadar **`Allow` satırları da**. Kapsam ölçüsü: kapsanan karar kuralı / toplam = **%100**.

| Kural | Satır | Kapsanması zorunlu | Not |
|---|---|---|---|
| BR-011 | 4 | ✅ 4/4 | `#R1` **Allow** kritik — aşırı-engelleme kör noktası |
| BR-012 | 4 | ✅ 4/4 | `#R4` alarm satırı dahil |
| BR-013 | — | ⛔ türetilemez | §7 K-07 açık |
| BR-014 | 5 | ✅ 5/5 | üç `Allow`, iki `Deny` |
| BR-015 | 5 | ✅ 5/5 | S-1 yoksa `#R3` `Inconclusive` |
| JR-002 | 3 yasak | ✅ 3/3 | her biri red **+ yan etki yok** |

---

## 10. Mühür ve sürüm

Bu dosya, senaryo sürümünü üreten **dört malzemeden biridir**
([[03-Decisions/ADR-0020-Senaryo-Malzeme-Muhru-Ve-Baglama-Butunlugu|ADR-0020]] §A):

```
senaryo.md ──┐
kurallar.md ─┼─→ [ajan + derleyici] ─→ senaryo surumu (compiled Arazzo)
API snapshot ┤
DB semasi ───┘   + profil paketi
```

| Malzeme | Senaryo satırındaki kolon |
|---|---|
| `senaryo.md` → Arazzo | `source_hash` |
| **`kurallar.md`** | **`rules_fingerprint`** ← bu dosya |
| API snapshot | `spec_snapshot_id` + `spec_fingerprint` |
| DB şeması | `db_connection_id` + `db_schema_fingerprint` |
| Profil paketi | `profile_fingerprint` |

**Bu dosya değişirse:** koşum anında `rules_fingerprint` tutmaz ve sonuç `Failed` değil
**`Inconclusive`** olur — *"kural değişti, senaryo bayat"* (ADR-0020 §C). Kayma bir hata değil,
bir **bilgi eksikliğidir**; `Failed` saymak yanlış alarm üretir.

Mührü **checker üretir, köprü taşır, ajan yazamaz** (ADR-0020 risk tablosu).

> [!NOTE] Açık madde
> Markdown için **kanonikleştirme kuralı** (satır sonu, boşluk, frontmatter sırası) wiki'de
> henüz karara bağlanmamıştır. Bu örnek onu tanımlamaz; `rules_fingerprint`'in nasıl
> hesaplandığı modülün kararıdır.

---

## 11. Değişiklik kaydı

| Sürüm | Tarih | Değişiklik | Etkilenen |
|---|---|---|---|
| 7 | 2026-08-14 | BR-015 iade penceresi eklendi | yeni senaryo gerekir |
| 6 | 2026-08-13 | K-05, K-06 cevaplandı; BR-012 `#R4` ayrıldı | mevcut senaryolar `Inconclusive` → yeniden yazım |
| 5 | 2026-08-12 | K-03, K-04 cevaplandı; BR-011 kesim noktası netleşti | BR-011 senaryoları güncellendi |
| 4 | 2026-08-11 | K-01, K-02 cevaplandı; BR-014 sayaç kaynağı netleşti | BR-014 senaryoları güncellendi |

> Her sürüm `rules_fingerprint`'i kaydırır. Kaydırma, ilgili senaryoları
> `trigger_kind = ContractChange` ile yeniden koşuma sokar ve bayat olanları listeler.
