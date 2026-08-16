---
kind: scenario
id: SC-0007
scenario_key: bilet-al-ogrenci-ist-ank
title: Ogrenci indirimli bilet satin alma
version: 3
updated: 2026-08-14
owners:
  - test.ekibi@ornek-musteri.tr
scenario_kind: BusinessScenario
rules_ref: RULES-BILET-001@v7
covers_rules: [ BR-011, BR-012, BR-014 ]
environment_key: test
authoring_mode: A
decision_refs: [ADR-0014, ADR-0015, ADR-0017, ADR-0020]
rule_refs: [RULE-0005, RULE-0006, RULE-0008]
---

# `senaryo.md` — Öğrenci indirimli bilet satın alma

> [!IMPORTANT] Bu dosya bir **örnektir**, kanonik bilgi değildir.
> [[04-Architecture/Alti-An|ARCH-0004]] An 1'in ikinci girdisidir. Eşi:
> [[99-Templates/ornek-giris/kurallar|kurallar.md]].

## 0. Bu dosya nedir, ne değildir

| | |
|---|---|
| **Rolü** | An 1 girdisi — **insanın anlattığı iş akışı** |
| **Dili** | İş dili. Endpoint adı, tablo adı, JSON pointer **yazılmaz** |
| **Ne verir** | Ajana **adım niyeti listesi** çıkarabileceği bir anlatı (ADR-0017 §A) |
| **Ne değildir** | Arazzo değil, test kodu değil, adım adım talimat değil |
| **Koşuda ne kalır** | Bundan derlenen belgenin `source_hash`'i |

> [!NOTE] Senaryo ≠ test durumu
> Test durumu *nasıl* test edileceğini yazar (*"`POST /tickets` çağır, 201 bekle"*).
> **İş senaryosu *ne* test edileceğini** yazar. Bu ürünün var oluş sebebi ikincisidir
> ([[05-Operations/Ekip-Kilavuzu|GUIDE-0004]] §1.3).
> Bu yüzden aşağıda hiçbir yerde operasyon adı geçmez — **operasyon bağını ajan sorar**,
> `SuggestOperationBindingsAsync` skorlu cevap verir, eşik altındaysa **soru sorulur**.

---

## 1. Aktör ve bağlam

| Alan | Değer | Neden yazılı |
|---|---|---|
| **Aktör** | Ali — **doğrulanmış öğrenci belgesi olan** yolcu | `kurallar.md` K-04: *"Ali öğrenci mi?"* referential belirsizliği **burada** kapanır |
| **Kimlik durumu** | Oturum açmış, geçerli token | Yetkisizlik teşhisiyle karışmaması için |
| **Kiracı** | `tenant-a` | M-9 yetki sınırı varyantı için ikinci kiracı gerekir (§7) |
| **Dönem** | Aktif dönem, Ali'nin **1** indirimli bileti var | BR-014 `#R3` satırına oturur — ana yol |
| **Tarife** | İndirimli (öğrenci) | Normal tarife ayrı varyanttır (§7 V4) |

> Aktör bloğu olmadan ajan *"Ali öğrenci mi"* diye sorar ve hat durur. Bu blok, mod A'da
> **sorulmasını engellemek** için vardır.

---

## 2. Amaç — tek cümle

> *"Yarın saat 10:00'da İstanbul–Ankara seferinde bilet varsa, Ali indirimli biletini satın
> alabilmeli; koltuk gerçekten düşmeli, ödenen tutar arama sonucuyla aynı olmalı ve aynı
> koltuk bir daha satılamamalı."*

---

## 3. Önkoşullar

| # | Önkoşul | Strateji | Karşılanmazsa |
|---|---|---|---|
| Ö-1 | Yarın 10:00 İstanbul–Ankara seferi **var ve satışta** | **`Arrange`** — sandbox veri kümesi `ist-ank-yarin-10-00` | **`Inconclusive`** |
| Ö-2 | O seferde **en az 1 boş koltuk** var | **`Arrange`** — aynı veri kümesi | **`Inconclusive`** |
| Ö-3 | Ali'nin bu dönem **tam 1** indirimli bileti var | **`Arrange`** — `ogrenci-1-bilet` | **`Inconclusive`** |
| Ö-4 | SUT **test saati** destekliyor | — (ortam yeteneği) | BR-011 varyantları **`Inconclusive`** |

**Varsayılan strateji `Arrange`'dır.** `Discover` (canlıdan bulma) yalnız üretim-benzeri
duman testinde meşrudur ve `inconclusive_rate` izlenir (RESEARCH-0009 §4).

> [!WARNING] Önkoşul sağlanmazsa sonuç **yeşil değildir**
> Bilet bulunamazsa hiçbir assertion başarısız olmaz — ama **hiçbir şey de doğrulanmaz**.
> Bu durum `Passed` sayılamaz; `Inconclusive`'dir. Yanlış negatif, yanlış pozitiften
> **daha tehlikelidir**: yanlış güven aşılar.

---

## 4. Akış — iş adımları

Her adım **niyet** cümlesidir. Operasyon bağını ajan An 2'de `ptn_ground` ile çözer.

| # | Adım niyeti | Karar var mı |
|---|---|---|
| **A-0** | Sefere ait **boş koltuk sayısı ölçülür** (başlangıç ölçümü) | — |
| **A-1** | Yarın 10:00 İstanbul–Ankara seferi **aranır** | — |
| **A-2** | **Sefer bulundu mu?** | ✅ **evet →** A-3 · **hayır →** dur, `Inconclusive` |
| **A-3** | Bulunan sefer için **indirimli bilet satın alınır** | — |
| **A-4** | Biletin **gerçekten kaydedildiği** doğrulanır | — |
| **A-5** | Boş koltuk sayısı **yeniden ölçülür** | — |
| **A-6** | Aynı koltuğa **ikinci bilet olmadığı** doğrulanır | — |

### 4.1 Karar noktası A-2

```
Sefer sayisi > 0   → satin almaya devam et
Sefer sayisi = 0   → DUR:  sonuc = Inconclusive
                     gerekce = "Belirtilen saatte sefer bulunamadi; ana yol kosmadi."
```

Karar **adımın içinde gizli değildir**; ayrı ve görünür bir adımdır. Hangi daldan gelindiği
koşum satırına `taken_branch_path` olarak yazılır.

---

## 5. Doğrulanacaklar

Bu bölüm senaryonun **oracle'ıdır**. Her satır bir iş kuralına veya bir değişmez kalıbına
bağlıdır; **bağsız satır yazılamaz**.

| # | Doğrulanacak | Kalıp | `rule_ref` | Hakem |
|---|---|---|---|---|
| D-1 | Satın alma **kabul edildi** | — | `BR-014#R3` | API Contract Checker |
| D-2 | Yanıt gövdesi **sözleşmeye uyuyor** | şema | — | API Contract Checker |
| D-3 | Bilet satırı **oluştu** ve durumu `Confirmed` | kalıcılık | — | Database Checker |
| D-4 | Boş koltuk sayısı **tam 1 azaldı** | **M-2 delta** | `BR-014#R3` | İş değişmezi değerlendiricisi |
| D-5 | Ödenen tutar **arama sonucundaki fiyata eşit** | **M-3 tutarlılık** | — | İş değişmezi değerlendiricisi |
| D-6 | O koltuğa ait aktif bilet sayısı **tam 1** | **M-4 tekillik** | `BR-012#R1` | Database Checker |
| D-7 | Ali'nin dönem sayacı **1 → 2** oldu | **M-2 delta** | `BR-014#R3` | Database Checker |

> [!IMPORTANT] Assertion'sız adım yayınlanamaz
> [[02-Rules/RULE-0006-Turetilemeyen-Assertion-Yayinlanamaz|RULE-0006]]: `assertion_count = 0`
> olan adım **reddedilir**, ve her assertion sözleşmeden **türetilebilir** olmalıdır.
> Ölçüm sert: serbest bırakılan ajan geri bildiriminin %70–77'si `print`, assertion değil;
> ilişkisel/aralık kontrolü **yalnız %3–8**.

### 5.1 Ölçüm nereden gelir

D-4 ve D-7'nin sayısal ölçümü **veritabanından** gelir (`ObservedRowCount`), API'nin
iddiasından değil — **yer gerçeği** (ADR-0017 §C).

D-4'ün aritmetiği `koltukSonra == koltukÖnce − 1`'dir ve **Arazzo bunu yazamaz**;
değerlendirme ayrı bir adımdan geçer. Bu, senaryo yazarının bilmesi gereken bir şey değildir —
derleyicinin işidir.

---

## 6. Bu senaryonun kapsamadığı

Dürüst kapsam beyanı. Kapsam dışı olduğu **yazılmayan** şey, kapsanmış sanılır.

- **BR-011 satış penceresi** — ayrı varyantlarda (§7 V5, V6); burada pencere açık kabul edilir.
- **BR-013 grup rezervasyonu** — `kurallar.md` §7 **K-07** açık; türetilemez.
- **İptal/iade (BR-015)** — ayrı senaryo (`SC-0008`).
- **Ödeme sağlayıcı davranışı** — SUT dışı; `Paid → Confirmed` geçişi tetiklenmiş kabul edilir.
- **Yük ve süre** — `duration_ms` regresyon sinyalidir, performans testi değildir.
- **UI akışı** — kapsam API + veritabanıdır.

---

## 7. Varyantlar — kural kapsamı için zorunlu

[[02-Rules/RULE-0008-Cift-Yonlu-Kural-Kapsami|RULE-0008]]: karar tablosunun **her satırı**
kapsanmalıdır — **`Deny` satırları kadar `Allow` satırları da.** Bu liste el yazımı değildir;
karar tablolarından **MC/DC ile mekanik** türetilir.

| Varyant | Önkoşul | Beklenen | `rule_ref` |
|---|---|---|---|
| **V1** | Ali öğrenci, **0** indirimli bileti var | ✅ kabul, sayaç `0 → 1` | `BR-014#R2` |
| **V2** *(ana yol)* | Ali öğrenci, **1** indirimli bileti var | ✅ kabul, sayaç `1 → 2` | `BR-014#R3` |
| **V3** | Ali öğrenci, **2** indirimli bileti var | ❌ `409 StudentQuotaExceeded` **+ bilet satırı oluşmaz + ödeme satırı oluşmaz** | `BR-014#R4` |
| **V4** | Öğrenci belgesi yok | ✅ kabul, **normal tarife** | `BR-014#R1` |
| **V5** | Satış penceresi **açık** (kalkış − 16 dk) | ✅ kabul | `BR-011#R1` |
| **V6** | Satış penceresi **kapalı** (kalkış − 15 dk, sınır) | ❌ `409 SalesWindowClosed` **+ yan etki yok** | `BR-011#R2` |
| **V7** | İstenen koltuk `Sold` | ❌ `409 SeatAlreadyTaken` **+ yan etki yok** | `BR-012#R3` |

> [!CAUTION] V1, V4 ve V5 atlanamaz
> Bunlar **`Allow` satırlarıdır** ve atlanırsa **aşırı-engelleme görünmez olur**:
> sistem hiç bilet satmıyor olsa bile V3/V6/V7 yeşil kalır ve *"kural çalışıyor"* sanılır.
> `Allow` satırı kapsanmayan sürüm **yayınlanamaz**.

**V3, V6, V7'nin iki parçası vardır.** *"Reddedildi"* yeterli değildir; **yan etki oluşmadığı**
da kanıtlanmalıdır (M-8). Bu tam olarak `AssertAbsent`'in işidir ve çoğu ekibin atladığı yerdir.

---

## 8. Zaman ve veri

| Konu | Karar |
|---|---|
| **Göreli zaman** | `yarın 10:00` sabit tarihe **çevrilmez**; `now + 1d @ 10:00` olarak yazılır, koşum anında çözülür ve **çözülmüş değer koşum kaydına yazılır** |
| **Trend kovası** | Çözülmüş zaman `history_id` hesabına **girmez** — aksi hâlde her koşu ayrı trend kovasına düşer |
| **Test saati** | V5/V6 sınır varyantları SUT'un test saatini ileri alması ile koşar; yoksa **`Inconclusive`**, asla `Passed` |
| **Veri izolasyonu** | Her koşuya izole veri kümesi; koşu sonunda yok edilir; veri kümesi **sürümlenir** |

---

## 9. Malzeme mührü

Yayın anında bu senaryo sürümü **dört malzemeyi** mühürler
([[03-Decisions/ADR-0020-Senaryo-Malzeme-Muhru-Ve-Baglama-Butunlugu|ADR-0020]]):

| Malzeme | Kimlik | İçerik mührü |
|---|---|---|
| Bu dosya → Arazzo | — | `source_hash` |
| `kurallar.md` | `RULES-BILET-001@v7` | `rules_fingerprint` |
| API sözleşmesi | `spec_snapshot_id` | `spec_fingerprint` |
| DB şeması | `db_connection_id` | `db_schema_fingerprint` |
| Profil paketi | `ptn-profile-pack/bilet-satis@r14` | `profile_fingerprint` |

**Boş bırakılan malzeme yayını reddeder.** Koşum anında mühür tutmuyorsa sonuç `Failed` değil
**`Inconclusive`**'dir ve **kayan malzeme raporda adıyla** görünür.

---

## 10. Yayın kapıları

Bu dosya `Published` olmaz; **bundan derlenen senaryo sürümü** olur.
`Draft → PendingApproval → Published`.

| # | Kapı | Kim geçirir |
|---|---|---|
| 1 | Şema geçerliliği (`redocly lint`) | Makine |
| 2 | **Türetilebilirlik** — her assertion `{jsonPointer, outcomeCode}` çözülür | Makine |
| 3 | **Zayıflama** — `assertion_count > 0`; assertion azaltan değişiklik işaretlenir | Makine |
| 4 | **Malzeme bütünlüğü** — §9'daki dört mühür dolu | Makine |
| 5 | **`sourceDescriptions` tutarlılığı** — belge `spec_snapshot_id`'ye çözülür | Makine |
| 6 | **DMN kapsamı** — §7'deki her satır kapsanmış | Makine |
| 7 | **`dryRun` + onay** | **İnsan** |

> [!CAUTION] Yayınlama kademe 4'tür
> **Hiçbir otonomi seviyesinde otomatikleşmez.** `Published` durumuna yazan tool ajanın
> kataloğunda **yoktur** ([[02-Rules/RULE-0005-Ajan-Hakem-Degildir|RULE-0005]]).
> `dryRun` kırmızıysa ajana **sonuç verilmez**, **çelişki bildirimi** döner — kararı insan verir.
> Gözlenen davranışa karşı düzeltme **yasaktır**; ajan aksi hâlde assertion'ı hataya uyacak
> şekilde gevşetir.

---

## 11. Koşum ve rapor — ajanın olmadığı yer

```
An 5  KOSUM        dis Arazzo runner icra eder        ← ajan YOK
An 6  YARGI+TESHIS checker hakem ve teshis            ← ajan YOK
```

Bir varyant kırmızı döndüğünde rapor şunu söyler:

```
V3  ogrenci kotasi siniri
  Sonuc          Failed
  Kategori       Business
  Hata kodu      —                       (beklenen: StudentQuotaExceeded)
  Kirilan adim   A-3 indirimli bilet satin alinir
  rule_ref       BR-014#R4
  Bulgu 1  [ApiContract]  beklenen 409, gozlenen 201
  Bulgu 2  [DatabaseComparison]  sales.Tickets +1 satir  → M-8 ihlali: YAN ETKI OLUSTU
  Bulgu 3  [DatabaseComparison]  billing.Payments +1 satir → M-8 ihlali
  Teshis   kota kontrolu satin alma yolunda calismiyor  (guven: yuksek)
```

**Hüküm checker'ındır.** Ajan burada yoktur; alıntısız hipotez rapora giremez
(ADR-0018 §D) ve her hipotez en az bir kanıtı **kimliğiyle** alıntılar.

---

## Ek A — Bu dosyadan ne türer (bilgilendirme)

> Bu ek **dosyanın parçası değildir**; hattın nereye gittiğini göstermek için eklenmiştir.

```
senaryo.md ─[LLM]→ adim niyeti listesi (§4)
     └─[SuggestOperationBindings · SKORLU]→ operasyon bagi
           │   esik alti → KAPALI UCLU SORU (tahmin yok)
           └─[LLM · TEK ADIM · sema kisitli]→ Arazzo adimi
                 └─[MEKANIK birlestirme]→ Arazzo 1.0.1 dokumani
                       └─[§10 kapilari]→ YAYIN

kurallar.md ─[LLM onerir · INSAN onaylar]→ SBVR/EARS
     └─[MEKANIK]→ DMN karar tablosu
           ├─[MC/DC]→ §7 varyantlari
           ├─[PICT]→ parametre kombinasyonlari
           └─[MR katalogu]→ D-4..D-7 degismezleri
```

**Adım üretimi tek adımdır** — karmaşık şemada constrained decoding çöküyor; birleştirme
modülün işidir. **Assertion hiçbir noktada LLM'den gelmez.**

Hedef sürüm **Arazzo `1.0.1`**'dir, 1.1 değil (ADR-0014 §C düzeltmesi / AUDIT-0002):
`respect`'in 1.1 belgesi koştuğu doğrulanamadı; 1.1 yalnız ertelenmiş async adım için gerekli.
