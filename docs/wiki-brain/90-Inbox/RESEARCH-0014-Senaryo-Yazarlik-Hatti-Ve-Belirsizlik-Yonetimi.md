---
id: RESEARCH-0014
type: research
status: active
title: Senaryo yazarlik hatti — assertion kaynagi, belirsizlik yonetimi ve iki mod
created: 2026-08-13
updated: 2026-08-13
decision_refs:
  - ADR-0007
  - ADR-0014
  - ADR-0015
  - ADR-0016
rule_refs:
  - RULE-0005
  - RULE-0006
---

# RESEARCH-0014 — Senaryo yazarlık hattı, belirsizlik yönetimi ve iki mod

Bu belge **ADR-0017'nin dayanak kaydıdır** ve tek soruyu cevaplar:

> `kurallar.md` + `senaryo.md` verildiğinde, çalıştırılabilir ve **doğru** bir Arazzo dokümanı
> nasıl üretilir; hangi kısmı deterministik olur, hangi kısmı yapay zekâya kalır, ve yapay
> zekâ ne zaman soru sorar?

[[90-Inbox/RESEARCH-0013-Runner-Oracle-Ayrimi-Ve-Ajan-Yazarlik-Kaniti|RESEARCH-0013]]'ün
devamıdır; oradaki B7 (uygulamadan öğrenme kör noktası) ve B8 (zayıf assertion) bulgularını
somut bir hatta çevirir.

> `90-Inbox` kanonik değildir. Karar ADR'dedir; çelişkide ADR kazanır.

---

## 1. En sert bulgu: assertion LLM'den gelmemeli

| Assertion kaynağı | Mutasyon skoru | Ölçüm |
|---|---|---|
| **LLM'in yazdığı oracle** | **%19,1** | 24 Java repo, TOGLL + Evosuite, GitBug-Java (199 hata) |
| Klasik Evosuite oracle | %17,3 | aynı çalışma |
| **Metamorphic relation** | **%95,3** | REST API, 317 tohumlanmış hatanın 302'si |

*(Farklı çalışmalar, doğrudan kıyaslanamaz — yön tartışmasız.)*

Aynı çalışma LLM'in **hangi davranışı** kodladığını da ölçüyor:

| Senaryo | LLM'in tanıma doğruluğu |
|---|---|
| Doğru kod + doğru assertion | %41–46 |
| **Bozuk kod + doğru assertion** | **%32–37** (8–9 puan düşüş) |
| Bozuk kod + **bozuk** assertion | %80–84 |

Yani model, **uygulamaya uyan yanlış assertion'ı doğru sanıyor**. Yazarların sonucu:
*"LLM tabanlı test üretim yaklaşımları da beklenen davranış yerine **gerçekleşen** davranışı
yakalayan oracle üretmeye yatkındır."*

Ek bulgular: assertion üretim doğruluğu **%58–60**; değişken/test adları otomatik üretilmişse
performans **%16 düşüyor**. Yazarların tavsiyesi: LLM oracle'ları **regresyon ve test artırma**
için, **mantık hatası tespiti için değil**.

**Karar sonucu:** `assertion` üretimi hattın hiçbir yerinde LLM'e bırakılmaz.

---

## 2. RESTestBench — problemimizin birebir ölçümü

3 REST servis, **106 insan-doğrulamalı NL gereksinim**, **228 elle tasarlanmış mutasyon**,
10 model. Mutasyonlar **gereksinime bağlıdır**: bir mutant ancak o özelliğin ihlalini
tetiklerse "anlamlı biçimde öldürülmüş" sayılır (property-based mutation testing).

| Bulgu | Sayı |
|---|---|
| **Net (precise) gereksinim** | %13 – **%92** |
| **Belirsiz (vague) gereksinim** | %2 – %54 — tipik **26–40 puan** daha düşük |
| Belirsiz gereksinimde %90'a ulaşan model | **YOK** |
| En iyi precise (Sonnet 4.5) | %92 · Llama 3.1 8B vague'de **%2** |
| Maliyet | GPT-5 Nano **%70 @ $0,41** · Sonnet 4.5 **%65 @ $10,13** (25×) |

İki cümle tasarımı doğrudan belirliyor:

> *"Mutasyona uğramış implementasyona karşı üretilen testler, geçerli implementasyona karşı
> üretilenlerden tutarlı biçimde düşük skorladı."*
>
> *"Yüksek detaylı gereksinimlerde gerçek SUT davranışını dahil etmek gereksiz ve bazen
> ters etkilidir."*

**Üç çıkarım:**

1. **Gereksinim netliği model seçiminden önemli.** 26–40 puanlık farkı hiçbir model yükseltmesi
   vermez.
2. **Model seçimi kalite değil maliyet kararı.** Ucuz model pahalıyı geçti.
3. **Üretici SUT davranışını görmemeli.** Üçüncü bağımsız doğrulama (B7).

---

## 3. Proxy metrik tuzağı — kabul kriterini değiştiriyor

Replikasyon çalışması: Defects4J v3.0, **854 hata**, 17 proje, 11 model, **101.123 test /
8.268 suite**.

| İlişki | Korelasyon |
|---|---|
| Coverage ↔ gerçek hata tespiti (**bozuk kodda**) | **tutarlı zayıf** — *"öngörü gücünü kaybediyor"* |
| Coverage ↔ gerçek hata tespiti (hatasız kodda) | r ≈ 0,37–0,48 |
| Mutation ↔ gerçek hata tespiti | r ≈ 0,48–0,51 |

Yazarların tavsiyesi: **proxy metriğe güvenme, tohumlanmış hata senaryosunda doğrudan ölç.**

**Sonuç:** kabul kriterimiz kapsam olamaz; §8'deki seeded-fault benchmark olmalı.

---

## 4. Öz-düzeltme ve çok-ajan: ölçüm karşı çıkıyor

Doğal refleks "iki ajan birbirini kontrol etsin" olurdu. Ölçüm bunu desteklemiyor:

- *"Dış sinyal olmadan intrinsic self-correction temelde güvenilmez."*
- *"Gözlenen faydanın çoğu tek başına **aggregation** ile açıklanabiliyor."*
- **Degeneration-of-thought:** hata açıkça gösterilse bile ajan aynı kusurlu akıl yürütmeyi
  tekrarlıyor.
- Maliyet: görev başına **300–400 API çağrısı**, tek-ajanın **3 katı**.
- **LLM-as-judge:** uzman alanlarda insan uzmanla uyum **%60–68**; position, verbosity,
  self-preference ve family bias belgelenmiş.
- Tavsiye: *debate tarzı çoklu-model kurulumları bias'ı **artırıyor***; bunun yerine
  referans-rehberli değerlendirme ve **farklı sağlayıcıdan** judge.

**Sonuç:** ajanlar tartıştırılmaz. Araştırmanın istediği "dış sinyal" bizde deterministik olarak
zaten var: `redocly lint`, türetilebilirlik kapısı, DMN tablo analizi, mutasyon skoru.

---

## 5. Assertion'ın dört kaynağı — hiçbiri LLM değil

| Assertion türü | Kaynak | Üretim | Örnek |
|---|---|---|---|
| Şema/sözleşme uygunluğu | OpenAPI snapshot | mekanik | yanıt gövdesi şemaya uyar |
| **İş kuralı** | **DMN karar tablosu** | mekanik (**MC/DC**) | `Student + activeTickets≥1 → Deny` |
| **İş değişmezi** | **Metamorphic relation kataloğu** | mekanik | koltuk **tam 1** azaldı |
| Durum geçişi | state machine (yolculuk) | mekanik (transition coverage) | `Cancelled → Paid` reddedilmeli **ve yan etki olmamalı** |
| Parametre kombinasyonu | **pairwise (PICT/ACTS)** | mekanik | *"hataların çoğu en fazla iki faktörün etkileşiminden"* |

**MC/DC neden:** N girdili, her biri m değerli bir tablo tam kapsam için m^N test ister;
MC/DC bunu doğrusala indirir ve her koşulun sonucu bağımsız etkilediğini gösterir.

**LLM'in payı üç yerde ve üçünde de küçük, şema-kısıtlı çıktı:**

1. `kurallar.md` → EARS/SBVR ifadesi *(insan onaylar)*
2. `senaryo.md` → adım niyeti listesi
3. adım niyeti + operasyon bağı → **tek** Arazzo adımı

---

## 6. Hat

```
kurallar.md
   └─[LLM önerir · İNSAN onaylar]→ SBVR Structured English / EARS
        └─[MEKANIK]→ DMN karar tablosu ──[dmn-check: boşluk / örtüşme / subsumption]
              ├─[MEKANIK MC/DC]→ karar ve sınır testleri
              ├─[MEKANIK PICT]→ parametre kombinasyonları
              └─[MEKANIK MR kataloğu]→ değişmez testleri

senaryo.md
   └─[LLM]→ adım niyeti listesi
        └─[SuggestOperationBindingsAsync · SKORLU]→ operasyon bağı
              │   eşik altı → soru
              └─[LLM · TEK ADIM · Microsoft.Extensions.AI şema kısıtı]→ Arazzo adımı
                    └─[MEKANIK birleştirme]→ Arazzo dokümanı
                          └─[lint + türetilebilirlik + DMN kapsam]→ YAYIN
```

**Adım üretiminin "tek adım" olması zorunlu:** JSONSchemaBench (10K gerçek şema, 6 framework)
karmaşık şemalarda constrained decoding'in çöktüğünü gösteriyor — GitHub-Hard kümesinde
Guidance %41, Llamacpp %39, XGrammar %28, **Outlines %3**. Küçük ve düz tut; birleştirmeyi
modül yapsın.

**.NET tarafı hazır:** `Microsoft.Extensions.AI` → `ChatClientStructuredOutputExtensions`,
generic tip argümanından JSON şeması çıkarıyor. Ayrı kütüphane gerekmiyor.

---

## 7. Belirsizlik yönetimi — asıl bulgu bu turda

### 7.1 Karar tablosu anlaşılmaz değil — ama soru formatı değil

Ampirik çalışma karar tablosu / karar ağacı / propositional rule / oblique rule karşılaştırmasında
**karar tablolarını üç ölçütte de** (doğruluk, yanıt süresi, cevap güveni) anlamlı biçimde önde
buluyor; test sonrası oylamada da kullanıcılar **kolaylık açısından tabloyu tercih ediyor**.

**Dolayısıyla tablo atılmaz.** Ama tablo bir *inceleme* artefaktıdır, *soru* artefaktı değildir.
Üç katman, üç okuyucu:

| Katman | Kime | Biçim |
|---|---|---|
| **Anlatım** | iş insanı | SBVR Structured English + **somut örnekler** |
| **İnceleme** | onaylayan | DMN karar tablosu |
| **İcra** | runner | Arazzo adımları |

**SBVR Structured English** OMG standardıdır ve modal operatörlerle iş diline yazılır:
*"It is obligatory that each rental has at most three additional drivers."* Türkçesi:
**"Bir öğrencinin aynı anda en fazla bir aktif bileti olabilir."**

**Specification by Example** (Adzic) örneklerin rolünü tanımlar: örnekler testin **tek
çalıştırılabilir kısmıdır** ve girdi–çıktı ilişkisini açıkça göstermelidir. Yani kuralı
anlatmanın yolu tablo değil, **iki somut örnektir**:

```
✓ Ali öğrenci, hiç aktif bileti yok      → bilet alabilir
✗ Ali öğrenci, 1 aktif bileti var        → alamaz (StudentTicketLimitExceeded)
```

### 7.2 Soru formatı: Example Mapping'in kırmızı kartı

**Example Mapping** (Matt Wynne) tam olarak bu konuşmanın endüstri standardı formatıdır.
Dört renkli kart:

| Kart | İçerik | Bizde karşılığı |
|---|---|---|
| 🟨 Sarı — **Story** | kullanıcı hikâyesi | `senaryo.md` |
| 🟦 Mavi — **Rule** | kabul kriteri / iş kuralı | `kurallar.md` → DMN satırı |
| 🟩 Yeşil — **Example** | kuralı örnekleyen somut vaka | **MC/DC + MR ile üretilen testler** |
| 🟥 Kırmızı — **Question** | oturumda cevaplanamayan soru | **ajanın sorusu** |

Ve kartların dağılımı bir **hazırlık ölçüsüdür**: *"Masada çok kırmızı kart varsa bu hikâye
geliştirmeye hazır olmayabilir."* Bu bizim yayın kapımızın birebir karşılığıdır.
Oturum süresi hikâye başına ~25 dakika; katılımcılar "üç amigo" (geliştirici, tester, ürün).

### 7.3 Soruyu LLM seçmez — boşluk analizi seçer

Kritik bulgu: *"Belirsizliği çözmek iki yetenek ister: sorgunun belirsiz olduğunu **tanımak**
ve bu tanımaya göre **davranmak**."* Ölçüm, LLM'lerin belirsizliği **tanıdığını ama nadiren
soru sorduğunu** gösteriyor.

**Sonuç: soru sorma kararı LLM'e bırakılamaz.** Deterministik boşluk tespitine bağlanır.
Ve soru şablonu **belirsizlik tipine** göre seçilir — literatürdeki AT-CoT deseni (önce
belirsizlik tipini belirle, sonra ona uygun açıklama üret) bunun LLM tarafındaki karşılığıdır;
bizde tip **analizden** gelir, modelden değil.

Belirsizlik taksonomisi (Massey ve ark.): **lexical, syntactic, semantic, vagueness,
referential, incompleteness.** Bizim tespit edicilerimize eşlemesi:

| Boşluk | Tespit eden | Soru kalıbı |
|---|---|---|
| **Incompleteness** | DMN gap analizi | *"X durumunda ne olmalı? Kuralda yazmıyor."* |
| **Vagueness** | ölçü birimi/sınır eksikliği | *"'Tek bilet' neye göre? (a) aynı sefer (b) aynı gün (c) aynı anda aktif (d) ömür boyu"* |
| **Referential** | senaryo ↔ kural varlık eşleşmemesi | *"Kural öğrenciler için. Senaryodaki Ali öğrenci mi?"* |
| **Overlap** | DMN örtüşme analizi | *"İki kural aynı durumda farklı sonuç veriyor."* |
| **Subsumption** | DMN analizi | *"Bu kural şunun içinde eriyor; gereksiz mi?"* |
| **Operasyon belirsizliği** | `SuggestOperationBindings` skoru eşik altı | *"'Bilet al' için iki aday var: ... Hangisi?"* |

Soru her zaman **kapalı uçlu ve seçenekli** verilir; serbest metin cevap istenmez.

### 7.4 Ölçülmüş örnek — kullanıcının kendi vakası

```
kurallar.md
  R1  Öğrenci olan kişi tek bilet alabilir.
  R2  Alınan koltuk bir daha alınmamalı.
  R3  Saati geçmiş bilet alınamaz.
senaryo.md
  Ali login oldu. Ali 12:30 biletini almak istiyor.
```

Deterministik analiz **dört** kırmızı kart üretir:

| # | Tip | Soru |
|---|---|---|
| 1 | vagueness | "Tek bilet" hangi pencerede? (aynı sefer / aynı gün / aynı anda aktif / ömür boyu) |
| 2 | referential | Ali öğrenci mi? (senaryoda yazmıyor, R1 öğrenciyle ilgili) |
| 3 | incompleteness | Bilet iptal edilirse koltuk yeniden alınabilir mi? (R2 iptali kapsamıyor) |
| 4 | vagueness | "Saati geçmiş" kesim noktası kalkış saati mi, satış kapanışı mı? |

Bu dört soru **ajanın zayıflığı değil, `kurallar.md`'deki gerçek boşluklardır** — ve
RESTestBench'in ölçtüğü 26–40 puanlık precise/vague farkı tam olarak bunlardır.

---

## 8. İki mod

| Mod | Davranış | Sonuç |
|---|---|---|
| **A — Soran (varsayılan)** | Belirsizlikte durur, kapalı uçlu sorar; cevap `kurallar.md`'ye geri yazılır ve **aynı soru bir daha sorulmaz** | Doğruluk iddiası korunur |
| **B — Varsayan (kapalı ayar)** | Durmaz; varsayımı yapar ama **işaretler** — test dosyasında ve raporda görünür | **"%100 doğrulanmış" iddiası düşer**; işaretli varsayım doğrulanmamış varsayımdır |

B modunun çıktısı her zaman görünür işaret taşır:

```
⚠ VARSAYIM: "tek bilet" = aynı anda tek aktif bilet
   Kaynak: kurallar.md R1 — vagueness
   Yanlışsa geçersiz olan testler: T2, T3
```

---

## 9. Kabul kriteri — kendi benchmark'ımız

§3 gereği kapsam sayısı kanıt değildir. RESTestBench'in deseni izlenir:

```
Ptn Assurance Benchmark
├── 3 örnek SUT (bilet, sipariş, abonelik) — test clock'lu
├── N gereksinim × 2 varyant (precise / vague)
├── M elle tasarlanmış, GEREKSİNİME BAĞLI mutasyon
│      (mutant ancak o kuralın ihlalini tetiklerse anlamlı öldürülmüş sayılır)
└── CI kapısı: precise gereksinimlerde mutasyon skoru >= %90
```

Aynı benchmark ikinci silahı da ölçer: mutant hangi `failure_category`'ye atandı, teşhis doğru
adımı gösterdi mi.

---

## 10. Zaman kuralları — SUT'tan bir şey istiyor

**Stripe Test Clocks** endüstri cevabıdır: test modunda zamanın ileri akışını simüle eden bir
API; saat ilerletilince abonelik/fatura nesneleri gerçekten zaman geçmiş gibi durum değiştirir
ve webhook tetikler. Kritik kısıt: **saat kurulduktan sonra yalnız ileri gidebilir.**

.NET karşılığı `TimeProvider` / `FakeTimeProvider`: zaman, kontrol edilmesi zor bir çevresel
değişken yerine **bilinçli modellenen bir bağımlılık** olur.

**Bu bizim çözemeyeceğimiz bir şeydir.** İki sonucu var:

1. **Satış öncesi uygunluk sorusu:** *"Sisteminizde test saati var mı?"* Yoksa zaman kuralları
   test edilemez ve bu peşinen söylenmelidir.
2. **Entegrasyon rehberine madde:** beklenen "test clock" yüzeyi tarif edilmelidir.

---

## 11. Kapanan açık maddeler

| Madde | Cevap |
|---|---|
| .NET DMN motoru | `net.adamec.lib.common.dmn.engine` (NuGet); OMG standart XML, Camunda Modeler ile tasarlanabilir. Statik analiz için `dmn-check` |
| .NET şema-kısıtlı üretim | **Birinci parti:** `Microsoft.Extensions.AI` `ChatClientStructuredOutputExtensions` |
| Türkçe EARS/SBVR kalıpları | **Literatürde yok** (Almanca MASTER var, Türkçe yok) — kendi kalıp setimizi tanımlayacağız |

---

## 12. MR kataloğunun Arazzo'ya derlenmesi

### 12.1 Sert kısıt: Arazzo aritmetik yapamaz

Arazzo spec'i `Criterion` nesnesinin `simple` tipini şöyle sınırlıyor — **doğrulandı**:

| Operatör sınıfı | Destekleniyor |
|---|---|
| Karşılaştırma | `<` `<=` `>` `>=` `==` `!=` |
| Mantık | `!` `&&` `\|\|` |
| Yapısal | `()` `[]` `.` |
| **Aritmetik** | **YOK** — toplama, çıkarma, çarpma, bölme tanımlı değil |

Spec'in verdiği tek örnek biçimi: `$statusCode == 200`, `$statusCode == 200 && $response.body.data != null`.

`jsonpath` tipi de RFC 9535 filtre ifadelerine dayanır ve orada da aritmetik yoktur.

**Sonuç: `koltukSonra == koltukÖnce - 1` Arazzo'da yazılamaz.** İş değişmezlerinin en değerli
ailesi (korunum, delta, monotonluk) native kriter olamaz.

> **Doğrulanmamış nokta:** İki runtime expression'ın **birbiriyle** karşılaştırılabilip
> karşılaştırılamayacağı (embedding ile `{$steps.a.outputs.x}`) spec'te net değil ve
> **runner'a bağlı**. Bu, Respect'e karşı **ilk gün sözleşme testiyle** doğrulanmalıdır.
> Güvenli tasarım: tüm MR ilişkileri oracle adımından geçer; test olumlu çıkarsa eşitlik
> ilişkileri sonradan native kritere indirilebilir.

### 12.2 Çözüm: değişmez oracle'ı bir Arazzo adımıdır

ADR-0015 §C'nin deseni burada da çalışır. Değişmez değerlendirmesi bir **HTTP ucu** olur,
runner onu sıradan bir adım olarak çağırır:

```
POST /invariants/check
{
  "patternCode": "Delta",                          // M-1..M-10
  "left":  "{$steps.sonraOlcum.outputs.koltuk}",
  "right": "{$steps.oncekiOlcum.outputs.koltuk}",
  "delta": -1
}
→ { "passed": true, "observedLeft": 41, "observedRight": 42, "expectedLeft": 41 }

successCriteria: $statusCode == 200 && $response.body#/data/passed == true
```

Aritmetiği **değerlendirici yapar**, Arazzo değil. Arazzo yalnız `outputs` ile değerleri taşır
ve `passed == true` karşılaştırmasını yapar — ikisi de yeteneği dahilinde.

**Ölçüm nereden gelir:** `RowAssertionResultDto.ObservedRowCount` zaten dönüyor; Arazzo adım
`outputs`'u ile yakalanır. Yani ölçüm **DB Checker'dan** gelir (yer gerçeği), API'nin
iddiasından değil.

### 12.3 Sahiplik: üçüncü oracle

Değerlendirici **alan bilgisi taşımaz** — saf bir yüklem hesaplayıcıdır (`left RELATION right ± delta`).
Karşılaştırdığı değerler diğer iki checker'dan gelir. Bu yüzden Test Module içinde yaşar
(`IBusinessInvariantPort`) ve `test_failure_categories`'deki **`Business`** kategorisine karşılık
gelir.

**RULE-0005 ihlali değildir:** kural ajanın hakem olmamasını söyler; deterministik bir
değerlendiricinin hakem olması zaten kuralın istediği şeydir.

### 12.4 M-1..M-10 derleme tablosu

RESEARCH-0009 §5.3'teki katalog korunur. Derlenmesi:

| # | Kalıp | Derleme | Yeni uç gerekiyor mu |
|---|---|---|---|
| **M-1** | Korunum (toplam sabit) | ölç → `invariants/check` `Conservation` | ✅ evet |
| **M-2** | Delta (tam N) | önce-ölç → işlem → sonra-ölç → `Delta` | ✅ evet |
| **M-3** | Tutarlılık (iki kaynak aynı) | iki ölçüm → `Equality` | ✅ evet |
| **M-4** | Tekillik | **DB Checker `AssertCount` + `cardinality = exactly 1`** | ❌ **bugün var** |
| **M-5** | Gidiş-dönüş | oluştur → oku → `Equality` (alan alan) | ✅ evet |
| **M-6** | İdempotans | aynı istek 2× → `IdempotentOutcome` + `AssertCount exactly 1` | ✅ evet |
| **M-7** | Monotonluk | önce-ölç → işlem → sonra-ölç → `Monotonic` | ✅ evet |
| **M-8** | Negatif yol | native `$statusCode == 409` **+ DB `AssertAbsent`** | ❌ **bugün var** |
| **M-9** | Yetki sınırı | ikinci token ile aynı istek, native `$statusCode == 403` | ❌ **bugün var** |
| **M-10** | Durum geçişi | native status + DB `AssertRow` durum kolonu + `AssertAbsent` | ❌ **bugün var** |

**On kalıbın dördü bugünkü yüzeylerle karşılanıyor.** Yeni uç altı kalıp için gerekiyor ve
tek, küçük, alan-bağımsız bir yüklem değerlendiricisidir.

M-8'in iki parçalı olması kritik: *"reddedildi"* yeterli değil, **yan etki oluşmadığı** da
doğrulanmalıdır — bu tam olarak `AssertAbsent`'in işidir.

### 12.5 Sorgu/liste uçları için ikinci aile: MROP

Segura ve ark. RESTful Web API'ler için **altı çıktı kalıbı (MROP)** tanımlıyor; hepsi küme
işlemleri üzerinden: **equivalence, equality, subset, disjoint, complete, difference.** MROP
*"uygulama alanından bağımsız olarak Web API'lerde tipik biçimde görülen soyut bir çıktı
ilişkisi"* olarak tanımlanıyor ve girdiler arasındaki ilişkiye kısıt koymuyor.

Bu aile M-1..M-10'un **numerik/durum** ailesinden farklıdır ve arama/liste uçlarına uygulanır
(*"filtre daralttığında sonuç kümesi alt kümedir"*). Aynı `invariants/check` ucu
`patternCode: Subset` gibi küme kalıplarını da alır.

Bu yaklaşımın ölçülmüş gücü: Spotify'da 20, YouTube'da 40 MR tanımlanmış, 469K metamorphic
test koşulmuş, **317 tohumlanmış hatanın 302'si (%95,3)** yakalanmış, 11 gerçek sorun
bildirilmiş ve 10'u geliştiricilerce doğrulanmış.

### 12.6 Değişmez seçimi kim yapar

Kalıp seçimi ajana bırakılmaz. RESEARCH-0009 §6.2'deki eşleme tablosu bunu deterministik yapar:

| Terim | Operasyonlar | Tablolar | Anahtar değişmezler |
|---|---|---|---|
| bilet | `purchaseTicket`, `getTicket` | `sales.Tickets` | M-2, M-4 |
| koltuk | — | `sales.Seats` | M-1, M-7 |
| ödeme | `capturePayment` | `billing.Payments` | M-3, M-6 |

Bu tablo `kurallar.md`'nin sözlük bölümünden derlenir; ajan yalnız **öneri** üretir, insan
onaylar. Tablo dolduğunda değişmez üretimi tamamen mekaniktir.

---

## 13. Hâlâ açık

1. **Arazzo'da iki runtime expression'ın doğrudan karşılaştırılması** — Respect'e karşı ilk
   gün sözleşme testiyle doğrulanacak (§12.1).
2. **Büyük OpenAPI'de grounding.** Chunking araştırması LLM-tabanlı ve format-özel chunking'in
   naif yöntemi geçtiğini, "Discovery Agent" deseninin (önce özet, talep üzerine ayrıntı)
   token/precision/F1'i iyileştirdiğini gösteriyor. `SuggestOperationBindings` tek başına
   yeter mi — **ölçülmeli**.
3. **Türkçe SBVR/EARS kalıp seti** tanımlanacak.

---

## 14. Dürüst tavan

| Aşama | Beklenen | Dayanak |
|---|---|---|
| SBVR/EARS + DMN çıkarımı | ~%92 **+ insan kapısı** | NL formalizasyon ölçümü |
| Karar ve sınır testi üretimi | **%100** | mekanik (MC/DC) |
| Değişmez testi üretimi | **%100** | mekanik (MR kataloğu) |
| Operasyon bağı (skorlu, eşikli) | ~%82 | graf tabanlı retrieval |
| Adım üretimi (parçalı, kısıtlı, 3 deneme) | ~%88 | APITestGenie |
| **Net gereksinimde uçtan uca mutasyon skoru** | **hedef ≥ %90** | RESTestBench precise aralığı |
| **Belirsiz gereksinimde** | **hedef yok — kapı reddeder** | hiçbir model %90'a çıkmıyor |

Son satır bir ürün kararıdır: **belirsiz `kurallar.md` kabul edilmez.** Belirsizliği yayın
kapısında yakalamak bir kısıt değil, ürünün asıl değeridir.

---

## 15. Kaynaklar

**Ölçümler — LLM test üretimi**
- Do LLMs generate test oracles that capture the actual or the expected program behaviour? — <https://arxiv.org/pdf/2410.21136>
- RESTestBench (NL gereksinimden REST testi) — <https://arxiv.org/html/2604.25862>
- Coverage/mutation korelasyon replikasyonu — <https://arxiv.org/html/2607.22880v1>
- APITestGenie — <https://arxiv.org/html/2604.02039>
- Meta TestGen-LLM (Assured Offline LLM-Based SE) — <https://arxiv.org/abs/2402.09171>
- LogiAgent (LLM çok-ajanlı REST mantık testi) — <https://arxiv.org/html/2503.15079v1>

**Deterministik test üretimi**
- Metamorphic Testing of RESTful Web APIs (%95,3) — <https://javiertroyauma.github.io/publications/TSE2017_REST_prePrint.pdf>
- Metamorphic relation patterns for query-based systems — <https://personales.us.es/sergiosegura/files/papers/segura19-met.pdf>
- MR Generation: State of the Art (TOSEM) — MROP/MRP tanımları, altı küme kalıbı — <https://arxiv.org/html/2406.05397v2>
- MR-Scout (mevcut testlerden MR sentezi) — <https://arxiv.org/pdf/2304.07548>
- Arazzo Criterion Object — `simple` operatör kümesi (aritmetik **yok**) — <https://spec.openapis.org/arazzo/latest.html>
- DMN + MC/DC — <https://blog.kie.org/2020/07/making-executable-dmn-modeling-more-business-friendly.html>
- PICT / pairwise — <https://www.pairwise.org/>
- ACTS (NIST) — <https://arxiv.org/pdf/1803.09006>
- Hypothesis stateful testing (rule/invariant/precondition) — <https://hypothesis.readthedocs.io/en/latest/stateful.html>
- GraphWalker / MBT endüstriyel — <https://link.springer.com/article/10.1007/s42979-025-03823-7>

**Anlatım ve belirsizlik**
- Example Mapping (Matt Wynne) — <https://cucumber.io/docs/bdd/example-mapping/>
- Introducing Example Mapping — <https://medium.com/@mattwynne/introducing-example-mapping-42ccd15f8adf>
- Specification by Example (Adzic) — <https://gojko.net/books/specification-by-example/>
- SBVR 1.5 (OMG) — <https://www.omg.org/spec/SBVR/1.5/PDF>
- SBVR Structured English örnekleri — <http://www.kdmanalytics.com/sbvr/sbvr_intro_2.html>
- EARS — <https://en.wikipedia.org/wiki/Easy_Approach_to_Requirements_Syntax>
- Karar tablosu anlaşılabilirlik karşılaştırması — <https://www.sciencedirect.com/science/article/abs/pii/S0167923610002368>
- Belirsizlik taksonomisi — <https://arxiv.org/pdf/2607.04436>
- Knowing but Not Showing (LLM belirsizliği tanıyor ama sormuyor) — <https://arxiv.org/html/2605.25284v1>
- ClarifyGPT — <https://dl.acm.org/doi/full/10.1145/3660810>
- Ambiguity Type-CoT — <https://dl.acm.org/doi/10.1145/3726302.3729922>
- Requirements ambiguity, endüstriyel çalışma — <https://conf.researchr.org/details/icsme-2025/icsme-2025-industry-track/8/Requirements-Ambiguity-Detection-and-Explanation-with-LLMs-An-Industrial-Study>

**Üretim güvenilirliği**
- JSONSchemaBench — <https://arxiv.org/abs/2501.10868>
- Microsoft.Extensions.AI structured output — <https://devblogs.microsoft.com/semantic-kernel/using-json-schema-for-structured-output-in-net-for-openai-models/>
- LLM-as-judge bias — <https://www.adaline.ai/blog/llm-as-a-judge-reliability-bias>
- Multi-agent reflexion sınırları — <https://arxiv.org/html/2512.20845>
- Generation-Verification Gap — <https://arxiv.org/html/2506.18203v1>
- OpenAPI chunking for RAG — <https://arxiv.org/abs/2411.19804>
- Graph tabanlı tool retrieval — <https://github.com/SonAIengine/graph-tool-call>

**Araçlar ve altyapı**
- Common.DMN.Engine (.NET) — <https://github.com/adamecr/Common.DMN.Engine>
- dmn-check (statik analiz) — <https://github.com/red6/dmn-check>
- Stripe Test Clocks — <https://docs.stripe.com/billing/testing/test-clocks>
- .NET TimeProvider — <https://benjamin-abt.com/blog/2026/07/27/dotnet-timeprovider/>
- Arazzo örnekleri — <https://github.com/frankkilcommins/arazzo-examples>
- Fault injection in OpenAPI specs — <https://arxiv.org/pdf/2607.12101>
