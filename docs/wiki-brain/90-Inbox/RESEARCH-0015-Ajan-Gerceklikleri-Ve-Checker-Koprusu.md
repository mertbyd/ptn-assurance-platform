---
id: RESEARCH-0015
type: research
status: draft
title: Ajan gerceklikleri ve checker koprusu — halusinasyon, baglam kaybi, yerel model ve kanit zinciri
created: 2026-08-13
updated: 2026-08-13
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
---

# RESEARCH-0015 — Ajan gerçeklikleri ve checker köprüsü

> **DURUM: Araştırma tamamlandı (Tur 1-6).** Karar ADR-0018'e yazılacak.
> Bölümler: halüsinasyon mekanizması · bağlam kaybı · yerel model gerçekliği · kanıt zinciri ·
> örnek uygulamalar · MCP kontrol düzlemleri · bağlam yönetimi · çift yönlü kural kapsamı ·
> köprünün on kuralı.

Bu belge tek soruyu cevaplar:

> Ajan **kafası karışmadan, tahmin etmeden, halüsinasyon görmeden ve bağlamdan kopmadan**
> çalışsın istiyorsak, iki checker'ı nasıl bağlamalıyız ve ajana ne vermeliyiz?

Ürün gereksinimi (kullanıcı tanımı):

- Ajan **tahmin etmek zorunda kalmasın**; kalırsa tahmin etmesin, **checker'a sorsun**
- Hataların sebebini **köprü ile** çözebilsin
- Şirketin senaryosunu **tüm kurallarıyla** test edebilsin — ve kuralın **hem engellediği hem
  izin verdiği** yönü denesin
- Ajan **bağlamdan kopmasın**

---

## Bölüm 1 — Halüsinasyon neden olur (mekanizma)

### 1.1 Kök sebep bir eğitim/değerlendirme teşvikidir, prompt sorunu değil

OpenAI'nin *"Why Language Models Hallucinate"* (Kalai, Nachum, Vempala, Zhang; 2025) çalışması
halüsinasyonu **teşvik problemi** olarak formalize ediyor: standart eğitim ve değerlendirme
yordamları **belirsizliği kabul etmek yerine tahmin etmeyi ödüllendiriyor.** Modeller iyi birer
sınav çözücü olarak optimize ediliyor; emin değilken tahmin etmek sınav performansını artırıyor,
*"bilmiyorum"* cezalandırılıyor.

Teorik çerçeve: *"Is-It-Valid"* (IIV) ikili sınıflandırma problemi; üretken hata oranlarının
IIV yanlış-sınıflandırma oranının katlarıyla **alttan sınırlı** olduğu ispatlanıyor.

Önerdikleri çözüm **sosyo-teknik**: ek halüsinasyon değerlendirmesi eklemek değil, liderlik
tablolarına hâkim olan mevcut doğruluk-tabanlı değerlendirmelerin **puanlamasını değiştirmek** —
çünkü *"iyi bir halüsinasyon değerlendirmesinin, alçakgönüllülüğü cezalandırıp tahmini
ödüllendiren yüzlerce geleneksel doğruluk değerlendirmesine karşı etkisi az kalıyor."*

> **Bizim için sonuç:** Modelin varsayılan davranışı **tahmin etmektir**. Bu prompt ile
> kapatılabilecek bir hata değil, eğitime gömülü bir eğilimdir. Mimari, modelden tahmin etmemesini
> **istemek** yerine **tahmin etme fırsatını ortadan kaldırmalıdır.**

### 1.2 Taksonomi — bizim maruz kaldığımız tür

| Tür | Tanım | Bizdeki karşılığı |
|---|---|---|
| **Intrinsic** | Verilen bağlamla **çelişen** çıktı | Şemada olan kolonu yanlış okumak |
| **Extrinsic** | Verilen bağlamdan **doğrulanamayan** çıktı | **Olmayan endpoint, olmayan kolon, olmayan yetki uydurmak** |
| Factuality | Dış dünyaya uygunluk | — |
| Faithfulness | Prompt/bağlama sadakat | — |

**Bizim baskın riskimiz extrinsic'tir.** Karşı önlem doğrudan: **her şey verilen bağlamdan
doğrulanabilir olmalı** — yani ajanın eline sözleşme ve şema kanıtı verilmeli, serbest bilgi değil.

Ayrıca literatürde halüsinasyonun **herhangi bir hesaplanabilir LLM için kaçınılmaz** olduğuna
dair hesaplanabilirlik/diyagonalizasyon temelli bir ispat da mevcut. Yani hedef "halüsinasyonu
sıfırlamak" değil, **halüsinasyonun ulaşabileceği yüzeyi sıfırlamaktır.**

### 1.3 Ajanın kendi güveni kontrol sinyali olarak kullanılamaz

Abstention (çekimserlik) literatürünün en sert bulgusu:

- **Karar-eylem boşluğu:** *"Modelin 'emin değilim' demesi, güvenilir biçimde uygun temkinli
  davranışa dönüşmüyor; sözel güven bir kontrol sinyali olarak güvenilemez."*
- *"Halüsine edilmiş veya desteklenmemiş yanıtlar **yüksek güvenle** üretilebiliyor"*; sezgisel
  belirsizlik skorlarını eşiklemek kabul edilen cevaplar arasındaki hata oranı üzerinde
  **istatistiksel garanti vermiyor.**
- Abstention tek bir problem değil: **cevabın doğruluğu** ile **sorunun cevaplanabilirliği** ayrı
  boyutlar ve ayrı eşikler istiyor.

> **Bizim için sonuç:** "Ajan emin değilse sorsun" kuralı **ajanın kendi kararına bırakılamaz.**
> Bu, RESEARCH-0014 §7.3'teki *"LLM'ler belirsizliği tanıyor ama nadiren soruyor"* bulgusuyla
> aynı yere çıkıyor: **soru sorma kararı deterministik boşluk analizinden doğmalıdır.**

---

## Bölüm 2 — Bağlam kaybı neden olur (mekanizma)

### 2.1 Context rot: 18 modelin **hepsi** uzun bağlamda bozuluyor

Chroma'nın *"Context Rot"* çalışması (Temmuz 2025) **18 frontier model** üzerinde ölçüm yaptı:
Claude Opus 4 / Sonnet 4 / 3.7 / 3.5 / Haiku 3.5, o3, GPT-4.1 (+mini/nano), GPT-4o, GPT-4 Turbo,
GPT-3.5 Turbo, Gemini 2.5 Pro/Flash, Gemini 2.0 Flash, **Qwen3-235B / 32B / 8B.**

Bulgular:

- **İstisnasız hepsi** girdi uzunluğu arttıkça bozuluyor; F1 monoton düşüyor.
- **Bu pencere taşması değil.** 200K pencereli bir model **50K'da** belirgin biçimde bozulabiliyor;
  belgelenen limitin çok altında **%30-50** düşüşler ölçülmüş.
- **Anlamsal benzerlik etkisi:** iğne-soru benzerliği düştükçe uzunlukla bozulma **daha dik** oluyor.
- **Dikkat dağıtıcı (distractor) etkisi:** konuyla **ilgili ama yanlış** bilgi, ilgisiz bilgiden
  **daha çok** zarar veriyor. Distractor varken GPT ailesi en yüksek halüsinasyon oranını
  gösterdi; Claude ailesi daha düşük.
- **Ters-sezgisel yapı etkisi:** *"modeller, samanlık mantıklı bir fikir akışını koruduğunda
  **daha kötü** performans gösteriyor."* Karıştırılmış (tutarsız) samanlık **18 modelin
  tamamında** performansı iyileştirdi.
- Tekrarlanan-kelime sentez görevinde modeller *"girdide bulunmayan yeni kelimeler halüsine etti."*

### 2.2 Lost in the middle

Liu ve ark.: bilgi konumu U-şeklinde etki yapıyor; ilgili bilgi **ortada** olduğunda doğruluk
**%30'dan fazla** düşüyor. Altı model ailesinde tekrarlandı. Mimari kök sebep: RoPE uzun-mesafe
sönümü + softmax'in en yüksek skorlu token'lara yoğunlaşması → birincillik ve yakınlık avantajı.

Yeni modellerde **basit olgusal** soru-cevap için etki azalmış görünüyor (ör. Gemini 2.5 Flash);
ancak bu **çok-adımlı akıl yürütme** için geçerli değil.

### 2.3 Tasarıma etkisi — "bağlamdan kopmasın"ın gerçek cevabı

Bağlam kaybı **daha büyük pencere** ile çözülmez; **bağlamı küçük tutarak** çözülür.

| Ölçülmüş etki | Bizim karşı hamlemiz |
|---|---|
| Uzunluk arttıkça bozulma (18/18 model) | Ajanın bağlamına **giren token sayısını** azalt: ara sonuçlar modüle kalsın |
| Distractor ilgisiz bilgiden **daha zararlı** | **5 aday operasyon vermek, 1 doğru operasyondan kötüdür** → skorlu bağ + eşik; eşik altındaysa **soru sor**, liste dökme |
| Düşük anlamsal benzerlikte dik bozulma | Ajanın sorusuyla dönen kanıt **aynı sözlükte** olmalı (tek ajan sözlüğü) |
| Mantıklı akış bile zarar veriyor | Uzun anlatı yerine **yapılandırılmış, kısa, tipli** kanıt döndür |
| Orta konum kaybı | Kritik olguyu yanıtın **başına** koy (özet-önce) |

---

## Bölüm 3 — Yerel model (Ollama) gerçekliği

### 3.1 Ölçülmüş sonuçlar

Docker'ın pratik değerlendirmesi: **21 model, 3.570 test vakası, 210 toplu koşum**
(MacBook Pro M4 Max / 128 GB). Görev: alışveriş sepeti asistanı; tool çağırma, **tool seçimi**,
parametre doğruluğu; 5 tura kadar ajan döngüsü.

| Model | Tool seçim F1 | Gecikme |
|---|---|---|
| gpt-4 (hosted) | **0,974** | ~5 sn |
| **qwen3:14B-Q4_K_M** | **0,971** | **~142 sn** |
| qwen3:14B-Q6_K | 0,943 | — |
| claude-3-haiku | 0,933 | 3,56 sn |
| **qwen3:8B-F16** | **0,933** | **~84 sn** |
| qwen3:8B-Q4_K_M | 0,919 | — |
| gpt-3.5-turbo | 0,899 | — |
| llama3.1:8B-F16 | 0,835 | — |
| llama3.2:3B-F16 | 0,727 | — |
| watt-tool:8B | 0,484 | — |

**Üretim eşiği: F1 < 0,70 güvenilmez.**

Ek bulgular:

- **Nicemleme (quantization) anlamlı fark yaratmıyor** — Q4 ile F16 arasında belirgin fark yok.
- **Gecikme asıl bedel:** 14B ~142 sn, 8B ~84 sn. Bulut modeli 3-5 sn.
- `num_ctx` VRAM'in üstüne çıkarılırsa **sessiz CPU fallback** oluyor ve **tool çağrı doğruluğu
  da düşüyor** — operasyonel tuzak.

### 3.2 Gözlenen hata biçimleri

1. **Aceleci çağrı (eager invocation):** gerek yokken tool çağırmak ("Merhaba" mesajına tool çağrısı)
2. **Yanlış tool seçimi**
3. **Geçersiz argüman**
4. **Yanıtı görmezden gelme:** tool çıktısını işlememek

### 3.3 En kritik cümle

> *"Küçük modeller, çağrıyı **biçimlendirmekten** daha az güvenilir biçimde **doğru tool'u
> seçiyor**; bu yüzden tool sayısını düşük (3-5) ve açıklamaları keskin tutun."*

Yani **biçim sorunu çözülmüş** (grammar-constrained üretim ile "bozuk JSON mekanik olarak
imkânsız"); **seçim sorunu çözülmemiş.**

Ve grounding literatüründen tamamlayıcı bulgu:

> *"Prompt seviyesinde temellendirme, düşük sıcaklıkta ve güçlü modelle yeterlidir; ancak
> sıcaklık arttıkça ve **model yeteneği azaldıkça etkinliği düşer** — **deterministik katmanlar
> en çok, prompt uyumunun en zayıf olduğu yerde değer üretir.**"*

### 3.4 Cevap: Ollama bunu yapabilir mi?

| Soru | Cevap |
|---|---|
| Şema-uyumlu JSON üretebilir mi? | **Evet** — grammar-constrained üretimle mekanik olarak garanti |
| 7 tool arasından doğru olanı seçebilir mi? | **Qwen3 8B/14B evet** (F1 0,93-0,97); Llama 3.1 8B sınırda (0,84); 3B **hayır** |
| 40 tool arasından seçebilir mi? | **Hayır** — literatür 3-5 diyor |
| Gecikmesi kabul edilebilir mi? | **Yazarlık anı için evet** (dakikalar); etkileşimli sohbet için hayır |
| Bizim mimarimiz yerel modele uygun mu? | **Evet, ve tam da bu yüzden** — deterministik katman ne kadar kalınsa model o kadar küçülebilir |

> **Stratejik sonuç:** Deterministik katmanı kalınlaştırmak yalnız doğruluk değil,
> **maliyet ve veri ikametgâhı** kazancıdır. Modelin işi küçüldükçe yerel model gerçek bir
> seçenek hâline gelir.

---

## Bölüm 4 — Kanıt zinciri: "403 geldi, kim ne yetkisine sahip?"

### 4.1 Kullanıcının tarif ettiği akış

```
403 geldi
  → user_roles tablosuna bak            (DB Checker)
  → kullanıcının rollerini al
  → role_permission_grants eşleşmesine bak (DB Checker)
  → hangi izinlere sahip
  → OpenAPI'den operasyonun gerektirdiği scope'a bak (API Checker)
  → bizde o rol/izin gerçekten yok
  → DOĞRULANDI
```

Ajan **hiçbir adımda tahmin etmiyor**; her adımda bir checker'dan olgu alıyor.

### 4.2 Global karşılığı var: evidence-grounded agentic RCA

*AgentRCA* ve benzeri 2026 çalışmaları tam bu deseni tarif ediyor:

- *"Teşhis kanıtı **açık** hâle getirilir — tool çıktıları izlenebilirdir, aday teşhisler
  **sıralanır**, ve **hipotez tablosu ajanı nihai cevaptan önce destekleyen ve
  çelişen kanıtı izlemeye zorlar.**"*
- *"Bir **Verifier** ajanı her hipoteze karşı çekişmeli doğrulama yapar ve bir mühendise
  ulaşmadan önce **somut kanıt** talep eder."*
- *"Bir süpervizör kanıtı modaliteler arasında ilişkilendirir ve gözlemlenebilirlik sinyallerini
  **sınırlı bağlamlarda** tutar."*

**Bizdeki durum:** iki `DiagnosisManager` bu döngüyü **zaten** çalıştırıyor — hipotez üret,
bütçeli kanıt topla, güvene göre sırala, anlat. **Eksik olan tek şey süpervizör:** kanıtı
**iki alan arasında** ilişkilendiren katman. Köprünün asıl işi budur.

### 4.3 403'ün alan bilgisi — neden zincir şart

Operasyonel literatür bunu net söylüyor:

> *"Aynı 403 hata kodu tamamen farklı kök sebeplerden gelebilir — eksik bir IAM rolü, kapalı bir
> API ve bir organizasyon politikası kısıtı **hepsi 403 döner.**"*
>
> *"Rol vermeden önce hatadan **çağıran, izin ve kaynağı** çıkarın."*

Ve token yapısı: devredilmiş izinler `scp` claim'inde, uygulama izinleri `roles` claim'inde;
başarısız koşumlarda *"roles ve scp claim'leri boş veya null — hiçbir izin atanmadığı doğrulanır."*

> **Bizim için sonuç:** 403 tek başına bir teşhis **değildir**; üç bağımsız olgunun kesişimidir
> (çağıran kimliği · gereken izin · sahip olunan izin). Ajan bunu tahminle birleştirirse
> halüsinasyon üretir; köprü zinciri yürütürse **kanıt** üretir.

### 4.4 Temellendirmenin ölçülmüş gücü

- Tool temellendirmesinde **düz metin açıklama yetersiz**: doğrudan prompt en yüksek halüsinasyon
  oranını veriyor.
- Buna karşılık her tool'un **ayrılmış bir token** olarak temsil edildiği ve üretimin tool-token
  uzayıyla **kısıtlandığı** yaklaşım **%0,00 halüsinasyon oranı** raporluyor.

> **İlke:** Halüsinasyonu azaltmanın en güçlü yolu modeli ikna etmek değil, **üretim uzayını
> daraltmaktır.** Bizde bu şu demek: ajan operasyon **adı yazmaz**, skorlu adaylardan **seçer**;
> kolon **adı uydurmaz**, şemadan **gelen listeden seçer**.

---

## Bölüm 5 — Ara sonuç (Tur 1-4)

| Ölçülmüş gerçek | Mimari karşılık |
|---|---|
| Model varsayılan olarak **tahmin eder** (eğitim teşviki) | **Tahmin fırsatını kaldır**: her açık uçlu alan yerine kapalı seçim koy |
| Sözel güven **kontrol sinyali değildir** | Soru sorma kararı **deterministik boşluk analizinden** doğar, modelden değil |
| 18/18 model uzun bağlamda bozuluyor | Ara sonuçlar **modülde kalır**; ajana **sonuç** döner |
| Distractor ilgisizden **daha zararlı** | Eşik altı adayları **listeleme, sor** |
| Küçük model **seçimde** zayıf, biçimde iyi | Tool sayısı **≤7**; şema-kısıtlı çıktı |
| Deterministik katman **zayıf modelde en çok değer** üretir | Katman kalınlaştıkça **yerel model mümkün** hâle gelir |
| Aynı hata kodu **farklı kök sebepler** | Köprü **kanıt zinciri** yürütür, tek sinyalle hüküm vermez |
| Kanıt açık + destekleyen/çelişen ayrımı | Hipotez tablosu **iki yönlü kanıt** taşır |

---

## Bölüm 6 — Örnek uygulamalar: gerçek kod ne yapıyor

### 6.1 GitHub MCP Server — 100+ yeteneği 5 varsayılan gruba indirmek

Üretimde çalışan referans uygulama. Mimarisi:

- **Toolset** = alana göre hiyerarşik gruplama (`repos`, `issues`, `pull_requests`, `users`,
  `context`, `actions` …). `pkg/github/tools.go` içinde **metadata sabiti** olarak tanımlı:
  `ID`, `Description`, `Icon`, **`Default` (otomatik açık mı)**, opsiyonel `InstructionsFunc`.
- Tool'lar `AllTools()` içinde **kayıt anında** hangi toolset'e ait olduklarını beyan eder.
- **Yalnız beş toolset varsayılan açık**: `context`, `repos`, `issues`, `pull_requests`, `users`.
  Diğerleri **açıkça etkinleştirilmeli**.
- `--toolsets` / `GITHUB_TOOLSETS` ile kontrol; üç özel anahtar: `all`, `default`, **`dynamic`**.
- **`dynamic`**: MCP host çalışma anında toolset **keşfeder ve talep üzerine açar**.

Gerekçe belgede açıkça yazılı: *"Tool maruziyetini sınırlamak dil modellerini boğmayı önler —
tasarım felsefesi **'çok fazla seçenek' karar kalitesini düşürür** varsayımına dayanır. Yalnız
ilgili alanları etkinleştirerek kullanıcılar **bağlam boyutunu azaltır ve tool seçim doğruluğunu
artırır.**"*

> **Bizim için:** iki checker'da **19 public AppService** var. Bunu bire bir yansıtmak
> reddedilen desen. Doğru cevap: **az sayıda varsayılan toolset + dinamik keşif.**

### 6.2 RAPTOR — alıntısız hipotez **mekanik olarak reddedilir**

Çok-ajanlı adli inceleme çerçevesi; kanıt disiplini tam olarak bizim ihtiyacımız:

| Faz | Yaptığı |
|---|---|
| **Hipotez oluşturma** | *"Her hipotez kanıtı **kimliğiyle alıntılamak zorundadır.**"* |
| **Kanıt doğrulama** | Her kanıt parçası bir `ConsistencyVerifier` ile **orijinal kaynağa karşı** doğrulanır |
| **Hipotez geçerleme** | *"**Alıntısız veya doğrulanmamış kanıt içeren hipotezleri reddeder.**"* |

Bu, halüsinasyona karşı **mekanik bir kapı**: model istediği kadar hipotez üretsin, alıntısı
yoksa rapora giremez.

### 6.3 GSAR — tipli temellendirme

Çok-ajanlı sistemlerde ajanlar arası iletişime **anlamsal tip kısıtı** koyar. Yapılandırılmamış
metin yerine önceden tanımlı tiplere uyan bilgi alışverişi; tip ihlali = tespit edilebilir
halüsinasyon. Kazanç: *"aşağı akıştaki ajanların halüsine edilmiş öncüller üzerine inşa etmesini
engeller."*

> **Bizde:** checker'lar zaten tipli DTO döndürüyor. Eksik olan, **iki checker'ın tiplerinin tek
> tipli sözlüğe** normalize edilmesi.

### 6.4 Deterministik doğrulayıcı deseni

Veri kalitesi literatüründen, bizim yayın kapımızın birebir karşılığı:

- Doğrulama bileşeni bir **kapı** olarak çalışır; yalnız geçerli ve çalıştırılabilir kurallar
  deterministik yürütme katmanına geçer.
- Doğrulayıcının rolü **yalnız değerlendirmedir**: *"yeni kural icat etmesine, mevcutları
  değiştirmesine veya verilen şema ve bağlamın ötesinde varsayım yapmasına **izin verilmez**;
  bunun yerine deterministik, şema-tabanlı hükümler üretir."*
- Çıktı: **onaylananlar + reddedilenler + her ret için gerekçe** (eksik alan, tip uyumsuzluğu,
  çalıştırılamaz kural, gerçekçi olmayan kısıt, politika ihlali, deterministik olmayan mantık).

### 6.5 Salt-okuma SQL ajanı repoları

`db-agent`: *"tüm yazma/yönetim SQL'ini veritabanına **ulaşmadan önce** engelleyen güvenlik
katmanı"* + şema-farkında açıklanabilirlik. Diğerleri RBAC/RLS zorlaması ve SQL enjeksiyon
tespiti ekliyor.

> **Bizde karşılığı zaten var:** `ConnectionSafetyProfileResolver` ve *"serbest SQL taşımayan"*
> assertion sözleşmesi (ADR-0007 salt-okunur değişmezi).

---

## Bölüm 7 — Protokol seviyesinde bir keşif: MCP'nin üç kontrol düzlemi

MCP'nin üç primitive'i yalnız veri tipi değil, **kimin karar verdiğini** belirliyor:

| Primitive | Kontrol eden | Anlamı |
|---|---|---|
| **Tool** | **Model** | LLM ne zaman çağıracağına karar verir |
| **Prompt** | **İnsan** | Kullanıcı seçer/tetikler |
| **Resource** | **Uygulama (host)** | Host neyin bağlama gireceğine karar verir |

Bu, RULE-0005'in dört kademeli izin modelini **protokol seviyesinde** uygulanabilir kılıyor:

| Kademe | MCP karşılığı | Neden |
|---|---|---|
| 1 — Salt okuma | **Tool** (güvenli) veya **Resource** | Model serbestçe çağırabilir |
| 2 — Geri alınabilir | **Tool**, kayıtlı | Model çağırır, iz tutulur |
| 3 — Dış sisteme dokunan | **Tool**, kuyruğa alınır | Model çağırır, yürütme geciktirilir |
| **4 — Geri alınamaz** | **Prompt** (insan-kontrollü) — **asla Tool değil** | Tool model-kontrollüdür; kademe 4 tanım gereği model kontrolünde olamaz |

Ve `kurallar.md` bir **Resource**'tur: **uygulama-kontrollü**, yani neyin bağlama gireceğine
host karar verir, model değil. Bu, ADR-0014 §A'daki *"`kurallar.md` tablo değil, MCP Resource"*
kararının neden doğru olduğunun protokol seviyesindeki gerekçesidir.

> **Kural adayı:** *"Kademe 4 eylemi Tool olarak kaydedilemez."* Bu, RULE-0005'i belgeden
> **protokole** taşır ve ihlali yapısal olarak imkânsız kılar.

---

## Bölüm 8 — Bağlam yönetimi: ajanın uzun oturumda kopmaması

Anthropic'in dört tekniği ve bizdeki karşılıkları:

| Teknik | Mekanizma | Bizde |
|---|---|---|
| **Compaction** | Limit yaklaşınca özetle, özetle devam et. Claude Code *"mimari kararları, çözülmemiş hataları ve uygulama ayrıntılarını korur"*, gereksiz çıktıyı atar. Hafif hâli: **işlenen tool sonucunu temizle** | Yazarlık oturumunda: bağlanan operasyon **kararı** kalır, ham OpenAPI parçası **atılır** |
| **Structured note-taking** | Bağlam dışına kalıcı not; *"asgari yükle kalıcı hafıza"* | **Karar tablosu ve bağ tablosu diskte** — ajan her turda yeniden keşfetmez |
| **Sub-agent** | Her alt ajan **temiz bağlamla** çalışır, geriye **1.000-2.000 token'lık damıtılmış özet** döner | `ptn_ground` ve `ptn_explain` birer alt-ajan gibi davranır: içeride çok iş, dışarı küçük özet |
| **Just-in-time retrieval** | Hafif tanımlayıcılar tut, çalışma anında yükle; **progressive disclosure**. Bedeli: önceden hesaplanmıştan yavaş | `resource_link` + toolset dinamik keşfi |

Bunlar Bölüm 2'nin (context rot) doğrudan panzehiri: **bağlamı büyütmeyip küçük tutmak.**

---

## Bölüm 9 — Çift yönlü kural kapsamı: kullanıcının "6 saat" örneği

**Senaryo:** *"Öğrenci 6 saatlik dilimde tek bilet alabilir."* Ajan bir bilet aldı, 6 saat
geçirdi, tekrar denedi — **alamadı**. Yani kural **fazla engelliyor**.

Bu bir *"kural çalışmıyor"* vakası değil; **aşırı-engelleme (over-blocking / yanlış ret)**
vakasıdır ve klasik test literatüründe adı var.

Karar tablosu testinin tanımı bunu mekanik olarak yakalar:

- *"Karar tablosu testi geçerli ve geçersiz kombinasyonları içererek hem pozitif hem negatif
  testi destekler. Pozitif test hem olumlu sonuçları (onayla, işle, yönlendir) hem olumsuz
  sonuçları (reddet, hata göster, yükselt) kapsar."*
- Kapsam ölçüsü: *"en az bir test vakasıyla test edilen karar kuralı sayısı / toplam karar kuralı
  sayısı."*
- Ve doğrudan bizim vakamız: *"uygunluk kontrolleri, fiyatlandırma kuralları, erişim kontrolü
  veya iş akışı kararları içeren senaryolar için etkilidir; bu da onu **aşırı-engelleme (yanlış
  ret)** gibi sorunları tespit etmek için ideal kılar."*

**Sonuç: kural başına en az iki test zorunludur.**

```
"Öğrenci 6 saatlik dilimde tek bilet"
        ↓ DMN
| userType | sonBiletten geçen süre | sonuç |
| Student  | < 6 saat               | Deny  |
| Student  | >= 6 saat              | Allow |   ← BU SATIR DA TEST EDİLMELİ
| Regular  | *                      | Allow |
        ↓ MC/DC + sınır
T1  5s59d sonra  → Deny bekle    (engelleme doğru mu)
T2  6s00d sonra  → Allow bekle   ← AŞIRI-ENGELLEMEYİ YAKALAYAN TEST
T3  6s01d sonra  → Allow bekle
```

> **Kural adayı:** *"Karar tablosunun her satırı en az bir testle kapsanmalıdır; `Allow` satırı
> kapsanmayan sürüm yayınlanamaz."* Bu, RULE-0006'nın (`assertion_count > 0`) tamamlayıcısıdır:
> orada **assertion var mı**, burada **kuralın her iki yönü de sınandı mı** sorulur.

Aynı ilke 12:30 örneğinde de geçerli: `now < departure → Allow` satırı test edilmezse, sistem
**hiç bilet satmıyor** olsa bile testler yeşil kalır.

---

## Bölüm 10 — Sentez: köprünün on kuralı

Bu belgedeki her ölçüm tek bir mimari ilkeye bağlanıyor: **ajana karar verdirme, seçim yaptır.**

| # | Kural | Dayanak |
|---|---|---|
| 1 | **Açık uçlu alan yoktur.** Ajan operasyon adı, kolon adı, hata kodu **yazmaz**; skorlu/şemalı listeden **seçer** | Model varsayılan olarak tahmin eder (§1.1); üretim uzayını daraltmak %0,00 halüsinasyon veriyor (§4.4) |
| 2 | **Soru sorma kararı ajanda değildir.** Deterministik boşluk analizi tetikler | Sözel güven kontrol sinyali değil (§1.3) |
| 3 | **Eşik altı adaylar listelenmez, sorulur** | Distractor ilgisizden daha zararlı (§2.1) |
| 4 | **Ara sonuç ajana girmez.** İş modülde biter, ajana **sonuç** döner | 18/18 model uzun bağlamda bozuluyor (§2.1); alt-ajan 1-2K token özet (§8) |
| 5 | **Aktif tool ≤ 7**, geri kalanı toolset + dinamik keşif | Küçük model **seçimde** zayıf, 3-5 önerisi (§3.3); GitHub MCP deseni (§6.1) |
| 6 | **Her hipotez kanıtı kimliğiyle alıntılar; alıntısız hipotez rapora giremez** | RAPTOR (§6.2); AgentRCA destekleyen/çelişen kanıt tablosu (§4.2) |
| 7 | **Tek sinyalle hüküm verilmez; kanıt zinciri yürütülür** | Aynı 403 farklı kök sebeplerden gelir (§4.3) |
| 8 | **Kademe 4 eylemi Tool olarak kaydedilemez** | MCP kontrol düzlemleri: Tool = model-kontrollü (§7) |
| 9 | **Her karar tablosu satırı, `Allow` dahil, en az bir testle kapsanır** | Aşırı-engelleme tespiti (§9) |
| 10 | **Deterministik katman kalınlaştıkça model küçülebilir** | Deterministik katman zayıf modelde en çok değer üretir (§3.3); Qwen3 8B F1 0,93 (§3.1) |

---

## Bölüm 11 — Yazarlık için generic yetenek katmanı

Bölüm 4'teki 403 zinciri **teşhis** tarafının örneğiydi. Aynı generic/dinamik güç **yazarlık**
tarafında da gerekiyor: LLM Arazzo yazarken yüzlerce benzer soruyla karşılaşır.

**Doğru soru şudur:** *"Bir Arazzo yazarının cevaplaması gereken soruların tamamı nedir, ve her
biri hangi deterministik yetenekle cevaplanır?"*

Sorular alan-bağımsızdır; bilet, sipariş, abonelik fark etmez.

### 11.1 Yazarlık soru kataloğu

| # | Yazarın sorusu | Generic cevap kaynağı | Bugün |
|---|---|---|---|
| 1 | Bu iş adımı **hangi operasyona** düşüyor? | `SuggestOperationBindingsAsync` — skorlu aday | ✅ |
| 2 | **Geçerli istek gövdesi** nedir? | `BuildRequestExampleAsync` | ✅ |
| 3 | Adım N'in çıktısı adım N+1'in girdisine **nasıl bağlanır**? | **OpenAPI `links`** + şema eşleşmesi + `Location` başlığı gözlemi | ⚠️ kısmen |
| 4 | **Başarı** neye benziyor? | OpenAPI response şeması + durum kodları | ✅ |
| 5 | Bu assertion **sözleşmeden türetilebilir mi**? | `ValidateScenarioAssertionsAsync` | ✅ |
| 6 | **Sınır değerler** neler? | **JSON Schema kısıtlarından mekanik üretim** | ❌ |
| 7 | **Negatif vaka** nasıl kurulur? | Kısıtın sistematik ihlali | ❌ |
| 8 | Bu veri **hangi tabloda** yaşıyor? | `DescribeTableAsync` + FK grafiği | ✅ |
| 9 | Anahtar **PK/unique mi**? | `DescribeTableAsync` | ✅ |
| 10 | Bu operasyon **DB'de neyi değiştiriyor**? | **Etki ayak izi: önce/sonra fark** | ⚠️ motor var, akış yok |
| 11 | **Ön koşul** nasıl sağlanır? | Test verisi seed'i / sandbox | ❌ |
| 12 | **Kural hangi sınırları** dayatıyor? | DMN karar tablosu (ADR-0017) | ❌ yazılacak |

> **Değişmez:** Her soru için ya bir checker **deterministik cevap** verir, ya da ajan
> **insana sorar**. Üçüncü seçenek — **tahmin** — yoktur.

### 11.2 Soru 3 — adım zincirleme: standart mekanizma zaten var

**OpenAPI `links`** nesnesi tam olarak *"bir operasyonun çıktısı bir diğerinin girdisini nasıl
besler"* sorusunu tanımlar. Schemathesis bunun referans uygulamasıdır:

- Şemada beyan edilen `links`'i okur, **istek zincirleri** kurar ve bunları **durum makinesi**
  olarak keşfeder.
- Yakaladığı hata sınıfları: *"silindikten sonra hâlâ getirilebilen kaynak"*, *"create'in
  döndürdüğü id'yi get'in reddetmesi"*.
- **Beyan yoksa çalışma anında öğrenir:** yanıtlardaki **`Location` başlığını gözlemleyerek**
  takip operasyonlarını **otomatik keşfeder**.
- GraphQL tarafında üretici/tüketici çıkarımı yapar.

> **Bizim için iki sonuç:**
> **(a)** Zincirleme ajanın icat edeceği bir şey değil — **spec'ten okunur**.
> **(b)** Spec'te `links` yoksa **runtime gözlemiyle aday üretilir ve insana onaylatılır**
> (Schemathesis'in `Location` deseni). Ajan yine tahmin etmez; **aday alır**.

### 11.3 Soru 6-7 — sınır ve negatif vaka: tamamen mekanik

Şema kısıtlarından test verisi üretimi olgun ve deterministik:

- `type: string, minLength: 2, maxLength: 10` → üretilen uzunluklar **1, 2, 3, 9, 10, 11**
- **Negatif veri her kısıtın sistematik ihlaliyle**: zorunlu alanı **teker teker** çıkar,
  `maxLength`/`minLength` sınırını aş, yanlış tip gönder, `min`/`max` aralığı dışına çık,
  geçersiz `enum` değeri ver, bozuk JSON gönder.
- *"Spec'ten üretim, pozitif testler için verinin **her zaman geçerli**, negatif testler için
  **sistematik olarak geçersiz** olmasını sağlar ve API değişikliğiyle **otomatik senkron kalır**."*

> Bu, ADR-0017'deki MC/DC sınır üretiminin **şema tarafındaki eşidir**: kural sınırları DMN'den,
> **veri sınırları JSON Schema'dan** gelir. İkisi de LLM'e sorulmaz.

### 11.4 Soru 10 — etki ayak izi: motoru zaten biz yazdık

*"Bilet alındıysa koltuk sistemden düşmeli"* gibi bir kuralın Arazzo'ya inmesi için yazarın
şunu bilmesi gerekir: **bu operasyon hangi tabloda neyi değiştiriyor?**

İki generic yol var:

| Yol | Nasıl | Gereksinim | Bizde |
|---|---|---|---|
| **Telemetri** | OTel veritabanı span'leri: HTTP isteği altında SQL alt-span'leri otomatik oluşur; span adı `<db.operation> <db.name>.<db.sql.table>` | **SUT'un enstrümante olması** | ❌ müşteriye bağlı |
| **Önce/sonra farkı** | Operasyonu bir kez koş, DB durumunu önce ve sonra karşılaştır, değişen tablo/kolonları çıkar | **Sandbox + salt-okuma bağlantı** | ✅ **motor var** |

İkinci yolun motoru elimizde: `TableDataComparisonManager`, `DataRowCountComparisonManager`,
`SchemaComparisonManager`, `SchemaDefinitionNormalizer`. **Eksik olan motor değil, akış.**

Ve bu yol telemetriye göre üstün: **SUT'tan hiçbir şey istemiyor.** (Test saati gibi bir
uygunluk şartı yaratmıyor.)

**Akış:**
```
1. sandbox sıfırla
2. aday tablo kümesini daralt   ← DescribeTable + FK grafiği (maliyet kontrolü)
3. row-count / veri fotoğrafı al
4. operasyonu bir kez çağır
5. tekrar fotoğraf al, farkı çıkar
6. denetim alanlarını (CreationTime vb.) filtrele
7. sonucu ÖNERİ olarak sun → insan onaylar → kalıcılaşır
```

Çıktı bir **etki ayak izi**dir ve ondan sonra o operasyon için M-1/M-2/M-7 değişmezleri
**mekanik** üretilir. Ajan "koltuk hangi kolonda" diye tahmin etmez; ayak izi söyler.

> **Kritik sınır:** Ayak izi **gözlemden** çıkar, yani B7 tuzağına açıktır (uygulamadan öğrenme).
> Bu yüzden ayak izi **oracle değildir**; yalnız **insana öneri**dir ve onaylanana kadar
> assertion üretiminde kullanılmaz. Aynı kural AGORA+ için de geçerliydi (RESEARCH-0014 §7).

### 11.5 Tavan: spec kalitesi

Literatür net: *"Spec kalitesi test kalitesini belirler"*, *"spec ile uygulama arasındaki
boşluk hataların yaşadığı yerdir."* Sık görülen boşluklar: spec'te olmayan alan döndüren
uçlar, yok sayılan zorunlu parametreler, belgelenen 200 yanıtıyla uyuşmayan şemalar.

> **Sonuç:** Köprü spec boşluğunu **sessizce telafi etmemeli**, **ölçüp raporlamalı**.
> Bu zaten API Contract Checker'ın işi — köprü onu yazarlık anında da kullanır:
> *"bu operasyonun yanıt şeması eksik; assertion türetilemez"* bir **kırmızı karttır**,
> ajanın doldurması gereken bir boşluk değil.

### 11.6 Yazarlık tarafının kanıt zinciri — teşhisin aynadaki eşi

Bölüm 4'teki 403 zinciri ile aynı desen, ters yönde:

```
TEŞHİS (An 6)                        YAZARLIK (An 2-3)
403 sinyali                          "öğrenci 6 saatte bir bilet"
  → user_roles          (DB)           → operasyon adayı      (API, skorlu)
  → role_grants         (DB)           → istek örneği         (API)
  → gereken scope       (API)          → etkilenen tablo      (DB, ayak izi)
  → DOĞRULANDI: yetki yok              → anahtar PK/unique mi (DB)
                                       → assertion türetilebilir mi (API)
                                       → DOĞRULANDI: yazılabilir
```

**Her iki yönde de ajan tahmin etmiyor; her adımda bir checker'dan olgu alıyor.** Köprü ikisinde
de aynı işi yapıyor: **alanlar arası kanıtı sıralı yürütmek ve tek sözlükte sunmak.**

---

## Kaynaklar

**Halüsinasyon mekanizması**
- Why Language Models Hallucinate (Kalai, Nachum, Vempala, Zhang; OpenAI) — <https://arxiv.org/abs/2509.04664>
- A comprehensive taxonomy of hallucinations in LLMs — <https://arxiv.org/pdf/2508.01781>
- Hallucination is Inevitable: An Innate Limitation of LLMs — <https://arxiv.org/pdf/2401.11817>
- Know Your Limits: A Survey of Abstention in LLMs (TACL) — <https://direct.mit.edu/tacl/article/doi/10.1162/tacl_a_00754/131566/Know-Your-Limits-A-Survey-of-Abstention-in-Large>
- Uncertainty-Aware Abstention with Provable Alignment Guarantees — <https://arxiv.org/html/2607.04430v1>
- Two Axes of LLM Abstention — <https://arxiv.org/html/2607.08456v1>

**Bağlam kaybı**
- Context Rot: How Increasing Input Tokens Impacts LLM Performance (Chroma, 2025) — <https://www.trychroma.com/research/context-rot>
- Lost in the Middle (Liu ve ark.) — <https://www.emergentmind.com/papers/2307.03172>
- Lost in the Middle: An Emergent Property from Information Retrieval Demands — <https://arxiv.org/html/2510.10276>

**Yerel model / tool calling**
- Local LLM Tool Calling: A Practical Evaluation (Docker; 21 model, 3.570 vaka) — <https://www.docker.com/blog/local-llm-tool-calling-a-practical-evaluation/>
- docker/model-test (açık kaynak değerlendirme çerçevesi) — <https://github.com/docker/model-test>
- Berkeley Function Calling Leaderboard v4 — <https://gorilla.cs.berkeley.edu/leaderboard.html>

**Temellendirme ve kanıt zinciri**
- Agentic Root Cause Analysis through Evidence-Grounded Reasoning — <https://arxiv.org/html/2607.22385v1>
- Stalled, Biased, and Confused: Reasoning Failures in LLMs for Cloud RCA — <https://arxiv.org/html/2601.22208v1>
- A Multi-Dataset Benchmark for LLM Agents in Microservice Failure Diagnosis — <https://arxiv.org/pdf/2606.29193>
- GRAFT: Graph-Tokenized LLMs for Tool Planning (tool-token kısıtlama, %0,00) — <https://arxiv.org/pdf/2605.11706>
- Troubleshoot HTTP 403 from API Gateway (AWS) — <https://repost.aws/knowledge-center/api-gateway-troubleshoot-403-forbidden>
- Resolve Microsoft Graph authorization errors (`scp` / `roles` claim) — <https://learn.microsoft.com/en-us/graph/resolve-auth-errors>

**Örnek uygulamalar ve referans kod**
- GitHub MCP Server — toolset mimarisi ve dinamik keşif — <https://deepwiki.com/github/github-mcp-server/3-github-toolsets>
- GitHub MCP Server — ek toolset'ler — <https://deepwiki.com/github/github-mcp-server/3.8-additional-toolsets>
- Dinamik tool seçimi tartışması (issue #275) — <https://github.com/github/github-mcp-server/issues/275>
- tool-gating-mcp (bağlamı korumak için tool geçitleme) — <https://github.com/ajbmachon/tool-gating-mcp>
- RAPTOR / OSS Security Forensics — alıntısız hipotez reddi — <https://github.com/NousResearch/hermes-agent/issues/384>
- GSAR: Typed Grounding for Hallucination Detection and Recovery — <https://arxiv.org/pdf/2604.23366>
- db-agent (yazma SQL'ini DB'ye ulaşmadan engelleyen katman) — <https://github.com/db-agent/db-agent>
- schema-aware-ai-sql-agent (RBAC/RLS zorlaması) — <https://github.com/raedmajid/schema-aware-ai-sql-agent>
- docker/model-test — <https://github.com/docker/model-test>

**Bağlam mühendisliği ve protokol**
- Effective context engineering for AI agents (Anthropic) — compaction, note-taking, alt-ajan (1-2K token özet), just-in-time retrieval — <https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents>
- MCP Demystified: Tools vs Resources vs Prompts — **kontrol düzlemleri** (model / insan / uygulama) — <https://techcommunity.microsoft.com/blog/azuredevcommunityblog/mcp-demystified-tools-vs-resources-vs-prompts-explained-simply/4508057>
- MCP Architecture Deep Dive — <https://www.getknit.dev/blog/mcp-architecture-deep-dive-tools-resources-and-prompts-explained>

**Generic yazarlık katmanı (Bölüm 11)**
- Schemathesis — stateful testing, OpenAPI `links` durum makinesi — <https://schemathesis.readthedocs.io/en/stable/explanations/stateful/>
- Schemathesis — stateful testing özelleştirme, `Location` başlığından runtime link keşfi — <https://schemathesis.readthedocs.io/en/stable/guides/stateful-testing/>
- Schemathesis — şemadan veri üretimi ve sınır değerler — <https://schemathesis.readthedocs.io/en/stable/explanations/data-generation/>
- API test verisi üretimi: kısıt ihlaliyle negatif veri — <https://totalshiftleft.ai/blog/how-to-generate-test-data-api-testing>
- JSON Schema statik analiz — <https://json-schema.org/blog/posts/schema-static-analysis>
- OpenTelemetry veritabanı span sözleşmesi (`db.operation`, `db.sql.table`) — <https://opentelemetry.io/docs/specs/semconv/db/database-spans/>
- OTel trace ↔ veritabanı korelasyonu — <https://docs.datadoghq.com/opentelemetry/correlate/dbm_and_traces/>
- Spec kalitesi test kalitesini belirler — <https://totalshiftleft.ai/blog/how-ai-generates-api-tests-from-openapi>
- OpenAPI test kapsamının dört boyutu — <https://totalshiftleft.ai/blog/how-to-measure-api-test-coverage>

**Çift yönlü kural kapsamı**
- Decision Table Testing (ISTQB) — pozitif/negatif kapsam, kural kapsam oranı — <https://www.toolsqa.com/software-testing/istqb/decision-table-testing/>
- Decision Table Testing: aşırı-engelleme (yanlış ret) tespiti — <https://www.virtuosoqa.com/post/decision-table-testing>
- Negative testing — <https://en.wikipedia.org/wiki/Negative_testing>
- An Agentic Retrieval Framework for Data Quality (deterministik doğrulayıcı kapısı) — <https://arxiv.org/pdf/2606.13692>
