---
id: RULE-0007
type: rule
status: active
title: Ajan tahmin etmez; her soru checker'dan cevaplanir ya da insana sorulur
updated: 2026-08-13
severity: mandatory
scope: test-module
sources:
  - https://arxiv.org/abs/2509.04664
  - https://www.trychroma.com/research/context-rot
  - https://www.docker.com/blog/local-llm-tool-calling-a-practical-evaluation/
decision_refs:
  - ADR-0014
  - ADR-0017
  - ADR-0018
rule_refs:
  - RULE-0005
  - RULE-0006
---

# RULE-0007 — Ajan tahmin etmez

## Kural

Ajanın karşılaştığı **her soru** için üç seçenek vardır ve **yalnız ikisi** kabul edilir:

1. Bir **checker deterministik cevap** verir, veya
2. **İnsana sorulur** (kapalı uçlu, seçenekli),
3. ~~Ajan tahmin eder~~ — **yasak**.

Bunun somut uygulaması dört maddedir:

### 1. Açık uçlu alan yoktur

Ajan **operasyon adı, kolon adı, tablo adı, hata kodu, scope adı yazmaz** — deterministik
kaynaktan gelen listeden **seçer**. Bir alan serbest metinse, o alan bir tasarım hatasıdır.

### 2. Eşik altı adaylar listelenmez, sorulur

Skorlu öneri eşiğin altındaysa ajana **aday listesi dökülmez**; kapalı uçlu soru sorulur.
*"Konuyla ilgili ama yanlış"* bilgi, ilgisiz bilgiden **daha çok** zarar verir.

### 3. Aktif tool sayısı ≤ 7

Fazlası **toolset** olarak gruplanır ve **dinamik keşifle** açılır. Yanıtlarda
`responseFormat: concise | detailed` zorunludur; ağır gövde `resource_link` ile verilir.

### 4. Kademe 4 eylemi **Tool olarak kaydedilemez**

MCP kontrol düzlemleri: **Tool = model-kontrollü**, **Prompt = insan-kontrollü**,
**Resource = uygulama-kontrollü**. Geri alınamaz eylem tanım gereği model kontrolünde olamaz;
**Prompt** olarak kaydedilir. `kurallar.md` ve benzeri salt-okunur bağlam **Resource**'tur.

## Neden

Üç ölçüm ([[90-Inbox/RESEARCH-0015-Ajan-Gerceklikleri-Ve-Checker-Koprusu|RESEARCH-0015]]):

- **Halüsinasyon bir eğitim teşvikidir.** Standart eğitim/değerlendirme belirsizliği kabul etmek
  yerine **tahmin etmeyi ödüllendirir**; *"bilmiyorum"* cezalandırılır. Bu prompt ile kapatılamaz —
  **tahmin fırsatı kaldırılmalıdır.**
- **Ajanın sözel güveni kontrol sinyali değildir.** *"Emin değilim demesi güvenilir biçimde
  temkinli davranışa dönüşmüyor"*; halüsine edilmiş yanıtlar **yüksek güvenle** üretilebiliyor.
  Bu yüzden **soru sorma kararı ajana bırakılamaz** (RULE-0005 ile aynı yere çıkar).
- **Bağlam uzadıkça bozulur.** 18 frontier modelin **tamamı** girdi uzunluğuyla bozuluyor;
  200K pencereli model **50K'da** %30-50 kaybediyor. Ve **distractor** (ilgili ama yanlış bilgi)
  ilgisiz bilgiden daha zararlı.
- **Küçük model seçimde zayıf:** *"çağrıyı biçimlendirmekten daha az güvenilir biçimde doğru
  tool'u seçiyor — tool sayısını 3-5'te tutun."*

Karşı-panzehir ölçülmüş: üretimin **tool-token uzayıyla kısıtlandığı** yaklaşım **%0,00**
halüsinasyon oranı raporluyor. İlke: **modeli ikna etme, üretim uzayını daralt.**

## Doğrulama

- Tool şemalarında serbest metin alan taraması: operasyon/tablo/kolon/kod alanı **enum veya
  referans** olmalı.
- Kayıtlı aktif tool sayısı ≤ 7; fazlası toolset bayrağı arkasında.
- MCP kataloğunda **kademe 4 eylemi Tool olarak kayıtlı değil**.
- Yanıt şemalarında `responseFormat` mevcut.
- Yerel model hedefleniyorsa tool-seçim F1 ölçülür; **≥ 0,90** altında "destekleniyor" denmez.

## İstisna süreci

Tool sayısının 7'yi aşması veya bir alanın serbest metne açılması ancak ölçüm gösteren yeni bir
ADR ile mümkündür; ölçüm tool-seçim doğruluğunu ve token maliyetini raporlamalıdır.
