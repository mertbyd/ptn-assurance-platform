---
id: RULE-0005
type: rule
status: active
title: Ajan hakem degildir; hukum ve teshis deterministik motorlarindir
updated: 2026-08-13
severity: mandatory
scope: test-module
sources:
  - https://arxiv.org/pdf/2607.05139
  - https://arxiv.org/html/2602.07900
decision_refs:
  - ADR-0007
  - ADR-0014
  - ADR-0015
rule_refs:
  - RULE-0006
---

# RULE-0005 — Ajan hakem değildir

## Kural

**Geçti/kaldı kararını yalnız checker verir.** Yapay zekâ testi yazmayı hızlandırır; hüküm
vermez, hata bulmaz, sebep uydurmaz.

Bu üç sınır ihlal edilemez:

1. **Koşum anında (An 5) ve yargı anında (An 6) ajan yoktur.** Modele hiçbir çağrı yapılmaz.
2. **Ajanın kataloğunda hüküm yazan tool bulunmaz.** `Published` durumuna yazan, koşu sonucunu
   değiştiren veya bulgu oluşturan tool ajana verilmez.
3. **Onarım sözleşmeye karşıdır, gözleme karşı değildir.** `dryRun` kırmızıysa ajana sonuç
   verilmez; **çelişki bildirimi** döner ve kararı insan verir.

## Neden

İki ölçüm ([[90-Inbox/RESEARCH-0013-Runner-Oracle-Ayrimi-Ve-Ajan-Yazarlik-Kaniti|RESEARCH-0013]] §3):

- Mevcut koddan/gözlenen davranıştan üretilen testler **uygulamanın davranışını doğrulamaya**
  optimize olur, niyeti değil. Kod önce yazılmışsa test onun yanlış varsayımlarını miras alır.
- Modeller bozuk sisteme karşı iyileştirme yaparken assertion'ları hataya uyacak şekilde
  değiştiriyor; sonuç hiç etkileşmemekten kötü çıkıyor.

Bu ayrım bozulursa ölçülmüş olumsuz sonuçlar bizim sonucumuz olur.

## Kademeli izin modeli

Sınıflandırma işlem kategorisine göre değil **geri alınabilirlik ve etki yarıçapına** göredir:

| Kademe | Örnek | Gözetim |
|---|---|---|
| 1 — Salt okuma | `knowledge.lookup`, `run.get` | Kesintisiz |
| 2 — Geri alınabilir | `scenario.dryRun`, `run.trigger` | Serbest, **kayıtlı** |
| 3 — Dış sisteme dokunan | Sandbox seed'i | Kuyruğa alınır |
| 4 — Geri alınamaz | Yayınlama, yama uygulama, karantina kaldırma | **Zorunlu insan onayı** |

**Kademe 4 hiçbir otonomi seviyesinde otomatikleşmez.** Otonomi seviyesi kiracı ayarıdır:
`Observe` (yalnız 1) · `Assist` (1–2, varsayılan) · `Act` (1–3).

Onay içerik hash'ine bağlanır; yama değişirse hash tutmaz ve uygulama **reddedilir**.
Onay ekranı dört şeyi göstermek zorundadır: ne yapılacak, **neden**, ne değişecek,
nasıl geri alınır.

## Ek sınırlar

**Tur sınırı serttir:** yazım 8, teşhis 4, bakım 5, sohbet 10. Aşımda **başarısızlık**, sessiz
devam değil. Ölçüm: adım başına %85 güvenilir bir ajan 10 adımda ~%20'ye düşer.

**Model bir portun arkasındadır** (`IAgentModelPort`). v1'de tek sağlayıcı; yerel model adapter
olarak sonradan eklenir. Karar ölçümle verilir: `test_scenarios.agent_model_ref` ile model
başına kabul oranı ve maliyet karşılaştırılır (ADR-0014 §F).

**Denetim izi orantılıdır.** Sonuçlu eylemler zaten domain modelinde kalıcıdır
(`trigger_ref`, `approved_by`, `approval_bound_to_hash`); `trace_id` model çağrısından koşuya
kadar taşınır; **ham girdi/çıktı saklanmaz**. Hash zincirli ekle-only `agent_action_log`
**v1'de kurulmaz**; EU AI Act Madde 12 seviyesi kanıt gerektiren müşteri çıkarsa ayrı karar
olarak açılır.

**Telemetri sözlüğü OpenTelemetry GenAI konvansiyonlarıdır** (`gen_ai.conversation.id`,
`gen_ai.usage.input_tokens`, `gen_ai.usage.output_tokens`, `gen_ai.request.model`). Kendi
sözlüğümüzü icat etmiyoruz.

## Doğrulama

- MCP tool kataloğu gözden geçirilir: kademe 4 tool'u kayıtlı **değildir**.
- `dryRun` akışının ajana dönüş yolu yoktur; testle doğrulanır.
- Koşum ve yargı yollarında model istemcisi bağımlılığı bulunmaz.
- `test_scenarios.authored_by_agent` ve `agent_model_ref` doldurulur; model başına kabul
  oranı ölçülür.

## İstisna süreci

Bu sınırın gevşetilmesi ancak ölçüm gösteren yeni bir ADR ile mümkündür. "Model artık daha
iyi" gerekçesi tek başına yeterli değildir; kabul oranı ve yanlış hüküm oranı raporlanmalıdır.
