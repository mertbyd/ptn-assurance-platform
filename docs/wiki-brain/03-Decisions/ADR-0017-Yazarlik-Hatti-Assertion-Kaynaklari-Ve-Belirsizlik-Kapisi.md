---
id: ADR-0017
type: decision
status: accepted
title: Yazarlik hatti — assertion kaynaklari, degismez derlemesi ve belirsizlik kapisi
created: 2026-08-13
updated: 2026-08-13
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
rule_refs:
  - RULE-0005
  - RULE-0006
---

# ADR-0017 — Yazarlık hattı, assertion kaynakları ve belirsizlik kapısı

> ADR-0014'ün (yazarlık modeli) uygulama kararıdır. Dayanak:
> [[90-Inbox/RESEARCH-0014-Senaryo-Yazarlik-Hatti-Ve-Belirsizlik-Yonetimi|RESEARCH-0014]].

## Bağlam

ADR-0014 "ajan sorar, uydurmaz" ilkesini koydu ama üretim hattının mekaniğini bırakmıştı.
Üç ölçüm bu mekaniği belirliyor (RESEARCH-0014 §1-3):

- LLM'in yazdığı oracle'ın mutasyon skoru **%19,1** (klasik Evosuite %17,3); model **beklenen
  değil gerçekleşen** davranışı kodluyor. Metamorphic relation ile aynı iş **%95,3**.
- RESTestBench: net gereksinimde %13-92, **belirsizde %2-54**; belirsiz gereksinimde **%90'a
  ulaşan model yok**. Fark **26-40 puan**.
- Coverage ve mutasyon skoru **bozuk kodda öngörü gücünü kaybediyor**; kabul kriteri
  tohumlanmış hata ölçümü olmalı.

## Karar

### A. Assertion'ın beş kaynağı — hiçbiri LLM değil

| Assertion türü | Kaynak | Üretim |
|---|---|---|
| Şema / sözleşme uygunluğu | OpenAPI snapshot | mekanik |
| İş kuralı | **DMN karar tablosu** | mekanik (**MC/DC**) |
| İş değişmezi | **M-1..M-10 + MROP kataloğu** | mekanik |
| Durum geçişi | yolculuk durum makinesi | mekanik (transition coverage) |
| Parametre kombinasyonu | **pairwise (PICT/ACTS)** | mekanik |

**LLM assertion üretmez.** Payı üç yerdedir ve üçünde de çıktısı küçük ve şema kısıtlıdır:
`kurallar.md` → SBVR/EARS ifadesi *(insan onaylar)*, `senaryo.md` → adım niyeti listesi,
adım niyeti + operasyon bağı → **tek** Arazzo adımı.

### B. Hat

```
kurallar.md ─[LLM önerir · İNSAN onaylar]→ SBVR Structured English / EARS
     └─[MEKANIK]→ DMN karar tablosu ─[dmn-check: boşluk/örtüşme/subsumption]
           ├─[MC/DC]→ karar ve sınır testleri
           ├─[PICT]→ parametre kombinasyonları
           └─[MR kataloğu]→ değişmez testleri

senaryo.md ─[LLM]→ adım niyeti listesi
     └─[SuggestOperationBindingsAsync · SKORLU]→ operasyon bağı
           └─[LLM · TEK ADIM · şema kısıtlı]→ Arazzo adımı
                 └─[MEKANIK birleştirme]→ Arazzo dokümanı
                       └─[lint + türetilebilirlik + DMN kapsam]→ YAYIN
```

**Adım üretimi tek adımdır.** JSONSchemaBench karmaşık şemalarda constrained decoding'in
çöktüğünü gösteriyor (GitHub-Hard: Guidance %41, XGrammar %28, **Outlines %3**). Birleştirme
modülün işidir.

**.NET tarafı birinci parti:** `Microsoft.Extensions.AI` →
`ChatClientStructuredOutputExtensions`; generic tip argümanından JSON şeması çıkarılır.

**DMN motoru:** `net.adamec.lib.common.dmn.engine` (NuGet, OMG standart XML).
Statik analiz: `dmn-check` deseni.

### C. İş değişmezleri: Arazzo aritmetik yapamaz, oracle adımı yapar

Arazzo `Criterion.simple` yalnız karşılaştırma (`<` `<=` `>` `>=` `==` `!=`), mantık
(`!` `&&` `||`) ve yapısal (`()` `[]` `.`) operatörleri destekler. **Aritmetik yoktur.**
`jsonpath` tipi de RFC 9535 filtrelerine dayanır ve orada da yoktur.

Dolayısıyla `koltukSonra == koltukÖnce - 1` native kriter **olamaz**.

**Çözüm:** değişmez değerlendirmesi bir HTTP ucudur ve runner onu sıradan bir adım olarak
çağırır (ADR-0015 §C'nin deseni):

```
POST /invariants/check
{ "patternCode": "Delta",
  "left":  "{$steps.sonraOlcum.outputs.koltuk}",
  "right": "{$steps.oncekiOlcum.outputs.koltuk}",
  "delta": -1 }
successCriteria: $statusCode == 200 && $response.body#/data/passed == true
```

Aritmetiği **değerlendirici** yapar; Arazzo yalnız değerleri taşır ve `passed == true`
karşılaştırmasını yapar.

**Ölçüm DB Checker'dan gelir** — `RowAssertionResultDto.ObservedRowCount` zaten dönüyor ve
Arazzo adım `outputs`'u ile yakalanır. API'nin iddiası değil, yer gerçeği.

**Sahiplik:** değerlendirici alan bilgisi taşımaz; saf bir yüklem hesaplayıcıdır ve Test
Module'de `IBusinessInvariantPort` arkasında yaşar. `test_failure_categories`'deki
**`Business`** kategorisine karşılık gelir. **RULE-0005 ihlali değildir** — kural ajanın hakem
olmamasını söyler; deterministik bir değerlendiricinin hakem olması kuralın istediği şeydir.

**On kalıbın dördü bugünkü yüzeylerle karşılanır:** M-4 (`AssertCount exactly 1`),
M-8 (native status + `AssertAbsent`), M-9 (native status), M-10 (native status + `AssertRow`
+ `AssertAbsent`). Yeni uç altı kalıp içindir.

Sorgu/liste uçları için ikinci aile aynı uçtan geçer: **MROP** küme kalıpları
(equivalence, equality, subset, disjoint, complete, difference).

### D. Belirsizlik kapısı: soruyu analiz seçer, model değil

Ölçüm: *"LLM'ler belirsizliği tanıyor ama nadiren soru soruyor."* Dolayısıyla **soru sorma
kararı modele bırakılmaz**; deterministik boşluk tespitine bağlanır ve şablon **belirsizlik
tipine** göre seçilir.

| Boşluk tipi | Tespit eden | Soru kalıbı |
|---|---|---|
| Incompleteness | DMN gap analizi | *"X durumunda ne olmalı?"* |
| Vagueness | ölçü birimi / sınır eksikliği | *"'Tek bilet' neye göre? (a)…(d)"* |
| Referential | senaryo ↔ kural varlık eşleşmemesi | *"Ali öğrenci mi?"* |
| Overlap | DMN örtüşme analizi | *"İki kural aynı durumda çelişiyor."* |
| Subsumption | DMN analizi | *"Bu kural şunun içinde eriyor."* |
| Operasyon belirsizliği | `SuggestOperationBindings` skoru eşik altı | *"İki aday var: … Hangisi?"* |

Sorular **her zaman kapalı uçlu ve seçeneklidir**; serbest metin cevap istenmez.

### E. Üç katman, üç okuyucu

Karar tablosu atılmaz — ampirik çalışma karar tablolarını karar ağacı ve kural listesine karşı
**doğruluk, yanıt süresi ve cevap güveninde** önde buluyor. Ama tablo *inceleme* artefaktıdır,
*soru* artefaktı değil:

| Katman | Kime | Biçim |
|---|---|---|
| **Anlatım** | iş insanı | SBVR Structured English + **iki somut örnek** |
| **İnceleme** | onaylayan | DMN karar tablosu |
| **İcra** | runner | Arazzo adımları |

Soru formatı **Example Mapping**'in kırmızı kartıdır (🟨 Story · 🟦 Rule · 🟩 Example ·
🟥 Question) ve kart dağılımı hazırlık ölçüsüdür: *"çok kırmızı kart varsa bu hikâye
geliştirmeye hazır değil"* — RULE-0006'nın yayın kapısının birebir karşılığı.

### F. İki mod

| Mod | Davranış |
|---|---|
| **A — Soran (varsayılan)** | Belirsizlikte durur, kapalı uçlu sorar; cevap `kurallar.md`'ye geri yazılır, aynı soru bir daha sorulmaz |
| **B — Varsayan (kapalı ayar)** | Durmaz; varsayımı **işaretler** — test dosyasında ve raporda görünür |

B modunda **"%100 doğrulanmış" iddiası düşer**; işaretli varsayım doğrulanmamış varsayımdır.
Bu, ayarın açıklamasında yazılı olmalıdır.

### G. Öz-düzeltme sınırı

Ajanlar tartıştırılmaz. Ölçüm: dış sinyal olmadan öz-düzeltme **güvenilmez**; faydanın çoğu
aggregation ile açıklanıyor; degeneration-of-thought görülüyor; maliyet 3×. LLM-as-judge uzman
alanlarda insanla **%60-68** uyumlu ve position/verbosity/self-preference/family bias taşıyor.

Bizim dış sinyalimiz deterministiktir: `redocly lint`, türetilebilirlik kapısı, DMN tablo
analizi, mutasyon skoru. Düzeltme döngüsü **yalnız bu sinyallere** karşı çalışır;
`dryRun` sonucuna karşı **çalışmaz** (RULE-0005).

### H. Kabul kriteri: kendi benchmark'ımız

Kapsam sayısı kanıt değildir. RESTestBench deseni izlenir:

```
Ptn Assurance Benchmark
├── 3 örnek SUT (bilet, sipariş, abonelik) — test clock'lu
├── N gereksinim × 2 varyant (precise / vague)
├── M elle tasarlanmış, GEREKSİNİME BAĞLI mutasyon
└── CI kapısı: precise gereksinimlerde mutasyon skoru >= %90
```

Aynı benchmark ikinci silahı da ölçer: mutant hangi `failure_category`'ye atandı, teşhis doğru
adımı gösterdi mi.

**Belirsiz gereksinim için hedef konmaz** — kapı reddeder.

### I. Zaman kuralları SUT'tan bir şey ister

**Stripe Test Clocks** deseni: test modunda zamanın ileri akışını simüle eden bir API; saat
kurulduktan sonra **yalnız ileri gider**. .NET karşılığı `TimeProvider`/`FakeTimeProvider`.

Bu bizim çözemeyeceğimiz bir şeydir. İki sonucu vardır: **satış öncesi uygunluk sorusu**
(*"sisteminizde test saati var mı?"*) ve **entegrasyon rehberinde beklenen yüzey tarifi**.
Test saati yoksa zaman kuralları test edilemez ve bu peşinen söylenir.

## Alternatifler

- **LLM'e assertion yazdırmak:** ölçülmüş mutasyon skoru %19,1; model gerçekleşen davranışı
  kodluyor. Ürün olmaz.
- **Tek seferde tam Arazzo dokümanı ürettirmek:** karmaşık şemada constrained decoding çöküyor.
- **İki ajanı tartıştırmak / LLM-as-judge:** bias'ı artırıyor, 3× maliyet, uzman alanda %60-68.
- **Aritmetiği Arazzo'ya eklemek için runner fork'lamak:** ADR-0015 §C deseni fork gerektirmiyor.
- **Değişmez değerlendiricisini checker'lara koymak:** değerlendirici alan-bağımsızdır;
  checker'a koymak yanlış sahiplik ve iki checker'da çoğaltma demek.
- **Belirsiz `kurallar.md` kabul etmek:** hiçbir model %90'a çıkmıyor; kapı reddeder.
- **Kapsam/mutasyon skorunu kabul kriteri yapmak:** bozuk kodda öngörü gücü yok.

## Sonuçlar ve riskler

Yeni yüzeyler: `RuleCompiler` ve `ScenarioCompiler` (`Domain/Managers/Authoring/`),
`IBusinessInvariantPort` + `POST /invariants/check`. **Yeni proje veya katman açılmaz.**

Veri modeli **değişmez** (ADR-0016). Karar tablosu ayrı kolon istemez: `rules_fingerprint` +
deterministik derleyici = yeniden üretilebilir. Bulgunun hangi kural satırını doğruladığı
`test_result_findings.rule_ref = "BR-014#R2"` ile taşınır.

| Risk | Önlem |
|---|---|
| Arazzo'da iki runtime expression karşılaştırılabiliyor mu belirsiz | **İlk gün** Respect'e karşı sözleşme testi; güvenli varsayım: tümü oracle adımından geçer |
| Türkçe SBVR/EARS kalıbı literatürde yok | Kendi kalıp setimiz tanımlanacak; kabul kriteri benchmark'ta ölçülür |
| Değişmez seçimi ajana kayar | Terim ↔ operasyon ↔ tablo ↔ değişmez eşleme tablosu insan onaylı; ajan yalnız öneri üretir |
| Büyük OpenAPI'de operasyon bağı zayıflar | Chunking / Discovery Agent deseni **ölçülecek**; `SuggestOperationBindings` skoru eşikli |
| B modu sessizce varsayılan olur | Ayar açıklamasında doğruluk iddiasının düştüğü yazılı; rapor işareti zorunlu |
| Benchmark bakımsız kalır | CI kapısı; benchmark geçmezse sürüm çıkmaz |
