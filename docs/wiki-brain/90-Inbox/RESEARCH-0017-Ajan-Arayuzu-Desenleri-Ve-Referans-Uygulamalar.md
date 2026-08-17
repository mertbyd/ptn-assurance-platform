---
id: RESEARCH-0017
type: research
status: draft
title: Ajan arayuzu desenleri, protokoller ve referans uygulamalar — UI yigini icin dis tarama
updated: 2026-08-17
decision_refs:
  - ADR-0023
  - ADR-0025
rule_refs:
  - RULE-0005
  - RULE-0007
---

# Ajan arayüzü desenleri ve referans uygulamalar

> Kanonik değildir. Tek soruyu cevaplar: **bizim ajan + test platformu UI'ımızı yazarken
> dünyada hangi desenler, protokoller ve hazır repo'lar gerçekten işe yarar, hangileri
> bizim kısıtlarımızla çelişir?**
>
> Kanıt sınıfları: **K1** çalışan kod · **K2** yayımlanmış spesifikasyon/standart ·
> **K3** sektör analizi. Erişim: 2026-08-17.

---

## 0. Özet hüküm

| Soru | Cevap |
|---|---|
| Ajan olay akışı için protokol benimseyelim mi? | **Hayır, ama sözlüğünü ödünç alalım.** AG-UI'nin olay adlandırması olgun; tam protokol bizim tool-sonucu-gizleme kararımızla çelişiyor |
| Hazır sohbet UI kütüphanesi kullanalım mı? | **Kısmen.** `assistant-ui` mesaj/stream primitives için uygun; kesinti (interrupt) modelimiz özel kalır |
| API istemcisi elle mi yazılsın? | **Hayır.** `openapi-typescript` + `openapi-react-query` bugünün standardı ve eski UI kuralımızla (RULE-0001) örtüşüyor |
| Arazzo belgesi için editör yazalım mı? | **Hayır.** Görselleştirme için hazır araç var; **yazma** zaten ajanda ve kapalı formda |
| Onay ekranı nasıl tasarlanmalı? | **Plan-and-execute + güven sinyali**; onay kolaylaştıkça denetim düşüyor (ölçülmüş) |

---

## 1. AG-UI — ajan/ön yüz olay protokolü (K2)

AG-UI (Agent–User Interaction Protocol), ajan davranışını **düz metin yerine tipli olay
akışı** olarak açan açık bir protokoldür. Taşıma SSE veya WebSocket'tir. Olaylar altı
kategoriye ayrılır:

| Kategori | Olaylar |
|---|---|
| Yaşam döngüsü | `RunStarted` `RunFinished` `RunError` `StepStarted` `StepFinished` |
| Metin | `TextMessageStart` `TextMessageContent` `TextMessageEnd` `TextMessageChunk` |
| Tool | `ToolCallStart` `ToolCallArgs` `ToolCallEnd` `ToolCallResult` `ToolCallChunk` |
| Durum | `StateSnapshot` `StateDelta` `MessagesSnapshot` |
| Etkinlik | `ActivitySnapshot` `ActivityDelta` |
| Özel | `Raw` `Custom` |

Tasarım felsefesi kasıtlı minimalizm: her olay bir `type` ve küçük bir payload taşır;
**protokol veri sözleşmesini belirler, görsel muameleyi değil.**

### Bizimle karşılaştırma

Bizim akışımız yedi olaydır (`text_delta`, `tool_call`, `input_required`,
`approval_required`, `completed`, `cancelled`, `error`) ve AG-UI'nin sadeleştirilmiş bir
alt kümesidir.

| AG-UI | Bizde | Karar |
|---|---|---|
| `TextMessageContent` | `text_delta` | ✅ Aynı |
| `ToolCallStart` | `tool_call` | ✅ Aynı |
| **`ToolCallResult`** | **yok** | ⛔ **Bilinçli.** Ajan checker'ın ham kodunu ve kanıtını UI'ya akıtmaz (ADR-0018). Tool sonucu modele gider, tarayıcıya gitmez |
| `RunStarted` | yok | ⚠️ **Eksik.** UI "başladı"yı istekten türetmek zorunda; ucuz bir ekleme |
| `StateSnapshot` / `StateDelta` | yok | ⚠️ Belge durumu ayrı REST çağrısıyla çekiliyor; olay olsaydı tek kaynak olurdu |
| Interrupt/approval | `input_required` + `approval_required` | ✅ **Bizde daha ayrıntılı** — bilgi eksiği ile onay ayrı olaylar |

**Hüküm:** protokolü benimsemek gereksiz bağımlılık getirir (bizim ajanımız Fastify +
elle SSE, ADR-0023 §B). Ama **iki olay eklemek** UI'yi belirgin biçimde basitleştirir:
`run_started` ve `state_delta`. Öneri ADR-0025 §D'de.

---

## 2. Kesinti ve insan onayı desenleri (K3)

Üretimdeki uygulamalarda tekrar eden akış aynı: ajan onay isteyen bir tool çağırır →
iş akışı **duraklar** → ön yüz onay modalini tool adı ve argümanlarıyla render eder →
operatör onaylar/reddeder → çalışma **kaldığı yerden** devam eder.

LangChain tarafında bu `useStream` benzeri tipli bir akış durumu ve "interrupt algılandığında
onay kartı çiz, onaydan sonra `resume` et" kalıbıyla ifade ediliyor.

### Bizim farkımız — ve neden doğru tarafta

| Sektör | Bizde |
|---|---|
| Onay sonrası **aynı çalışma devam eder** (`resume`) | Onay sonrası oturum `ready`'ye döner; devam **yeni bir mesajla** olur |
| Reddetme genelde "tool'u atla" | Reddetme oturumu **`cancelled`** yapar (geri dönüşsüz) |
| Onay tool argümanlarını gösterir | Onay **tek kapalı yapı** gösterir (`StepProposal`) |

Redde davranışımız sektörden serttir. Bu **kasıtlı olabilir** ama UX bedeli var: kullanıcı
"öneri kötüydü, düzelt" diyemiyor, oturumu kaybediyor. UI bunu açıkça yazmalı
(ARCH-0005 §5); yumuşatmak isteniyorsa bu bir **ajan kodu kararıdır**, UI hilesi değil.

---

## 3. Sohbet UI kütüphaneleri (K1/K3)

| Seçenek | Ne verir | Bize uyumu |
|---|---|---|
| **`assistant-ui`** (TS/React, açık kaynak) | Mesaj listesi, streaming primitives, gen-UI, HITL | **Orta-yüksek** — mesaj/stream katmanı doğrudan kullanılabilir; kesinti modelimiz özel |
| **CopilotKit + AG-UI** | Hazır ajan bileşenleri, protokol köprüsü | **Düşük** — protokolü benimsemeyi zorunlu kılar |
| Kendi bileşenlerimiz | Tam kontrol | **Yüksek maliyet**, ama kesinti ve rozet mantığı zaten özel |

**Öneri:** sohbet kabuğu (mesaj listesi, otomatik kaydırma, streaming balonu, kopyala)
için hazır primitives; **kapalı soru kartı, onay kartı, bütçe göstergesi ve tool rozetleri
kendi bileşenlerimiz**. Bunlar ürünün ayırt edici yüzeyidir ve hiçbir kütüphanede karşılığı
yoktur.

---

## 4. Ajan UX desenleri ve güven kalibrasyonu (K3)

Üç desen kullanıcı testlerinde tutarlı biçimde ayakta kalıyor:

| Desen | İçerik | Bizdeki karşılığı |
|---|---|---|
| **Plan-and-execute** | 30 saniyeden uzun işlerde, başlamadan önce **niyet dizisini göster** ve onay al | Ajan turu başlamadan "hangi tool'lar çağrılacak" özeti — bugün **yok** |
| **Güven sinyali** | Sistemin ne kadar emin olduğunu görünür yap | `PtnVerdictCodes` (`Confirmed`…`Inconclusive`) + `CoverageReportDto` zaten var, **ekranda kullanılmalı** |
| **Kademeli devir** | Kullanıcı otonomiyi zamanla artırır (`Suggest` / `Co-pilot` / `Autopilot`) | RESEARCH-0012 §5A.3'ün `Observe`/`Assist`/`Act` önerisi — **ayar kodda yok** |

Ve en önemli uyarı:

> Aşırı güven için tasarlamak, az güven kadar tehlikelidir; arayüz ajanı yanılmaz gibi
> sunarsa operatörler kendi yargılarını uygulamayı bırakır ve onaylamamaları gerekeni
> onaylar.

**Bizim için doğrudan sonuç:** onay kartında ajanın metni **birincil** olmamalı. Birincil
olan, checker'dan gelen kanıt ve kapı sonucudur. Ajan metni "gerekçe" alanına, kanıtın
**yanına** konur — üstüne değil.

---

## 5. API istemcisi ve tip üretimi (K1/K3)

Eski API Contract Checker UI'ında zaten kural vardı: **tipler elle yazılmaz**,
`openapi-typescript` ile Swagger JSON'dan üretilir (CURRENT-0006, ARCH-0007 §1). 2026
ekosisteminde bu kural aynı kaldı; olgunlaşan şey sorgu katmanı:

| Araç | Ne yapar |
|---|---|
| `openapi-typescript` | Swagger/OpenAPI → `schema.d.ts` (yalnız tip, runtime yok) |
| `openapi-fetch` | ~2 KB, tip güvenli `fetch` sarmalayıcısı; URL ve parametrelerde yazım hatası imkânsız |
| `openapi-react-query` | ~1 KB, `openapi-fetch` üzerine TanStack Query sarmalayıcısı |
| `openapi-qraft` · `Hey API` · `openapi-react-query-codegen` | Hook/`queryOptions` üreteçleri (daha çok kod üretir) |

**Öneri:** `openapi-typescript` + `openapi-fetch` + `openapi-react-query`. Gerekçe: üretilen
kod yerine **üretilen tip**; runtime yüzeyi 3 KB; eski UI kuralıyla birebir sürekli.

> [!WARNING] Tek `schema.d.ts` bizde çalışmaz
> Eski karar (ARCH-0007 §2) *"tek `gen:api` scripti, tek `schema.d.ts`"* diyordu. Bugün
> **üç köken** var (ARCH-0006 §0) ve ajan yüzeyi OpenAPI **yayınlamıyor** — Fastify'da
> şema Zod'dur. Yani: Test Module + checker için üretilmiş tipler, ajan için **elle yazılmış
> ve Zod ile hizalanmış** tipler. Bu ADR-0025 §C'de karara bağlanır.

---

## 6. Arazzo görselleştirme (K1)

Arazzo belgesi için hazır araçlar var: form tabanlı editörler, YAML/JSON ile senkron
diyagram, akış ve sekans diyagramı üreten görselleştiriciler, VS Code eklentisi.

**Bizim ihtiyacımız yazma değil okuma:**

- Belgeyi **ajan** yazıyor, hem de kapalı formda (`AddAuthoringStepDto`).
- Kullanıcı belgeyi **anlamak** ve **onaylamak** için görüyor.
- Serbest YAML düzenleme yayın kapısıyla çelişir (`SourceHash` mührü değişir).

**Öneri:** ekran 16 (Arazzo önizleme) bir **okuyucu + diff**'tir, editör değil. Adım listesi
ve basit bir akış diyagramı yeterlidir; hazır bir editörü gömmek yayın mührünü kırma yolu
açar.

---

## 7. Karşılaştırılabilir ürün yüzeyleri

| Ürün deseni | Alınacak ders |
|---|---|
| **Sentry Seer** (K3) | Tek ajan mimarisi, iki iş akışı (geniş sohbet + dar autofix); **kuruluş otonomi seviyesini kontrol ediyor**. Bizde dört an tek runner üzerinde — aynı desen |
| API kontrat izleme panoları | Snapshot zaman çizelgesi + fark kartı; bizim ekran 19–20'nin şekli |
| Test raporlama araçları (CTRF/JUnit/SARIF tüketicileri) | İhracat formatını **UI'da göstermek yerine indirtmek** yeterli; ekran 7 hafif kalabilir |

---

## 8. Bu belgeden çıkan somut öneriler

| # | Öneri | Nereye |
|---|---|---|
| Ö-1 | `run_started` ve `state_delta` olaylarını ajan SSE'sine ekle | ADR-0025 §D · ajan kodu |
| Ö-2 | `openapi-typescript` + `openapi-fetch` + `openapi-react-query`; ajan için elle tip | ADR-0025 §C |
| Ö-3 | Onay kartında **kanıt birincil, ajan metni ikincil** | CURRENT-0007 G-03 |
| Ö-4 | Tur başlamadan "hangi tool'lar çağrılacak" özeti (plan-and-execute) | Ürün kararı — S-2 ile birlikte |
| Ö-5 | Arazzo yüzeyi **okuyucu + diff**, editör değil | GUIDE-0006 |
| Ö-6 | Reddetmenin oturumu öldürmesi ürün kararı olarak yeniden değerlendirilsin | Ajan kodu — RESEARCH-0017 §2 |

---

## 9. Kaynaklar (erişim 2026-08-17)

| Kaynak | Neyi kanıtlıyor | Sınıf |
|---|---|---|
| https://docs.ag-ui.com/introduction | AG-UI'nin amacı, taşıma seçenekleri, olay tabanlı tasarım | K2 |
| https://docs.ag-ui.com/concepts/events | Olay tipleri ve kategorileri (yaşam döngüsü, metin, tool, durum, etkinlik, özel) | K2 |
| https://github.com/ag-ui-protocol/ag-ui | Referans uygulama ve SDK'lar | K1 |
| https://www.copilotkit.ai/blog/master-the-17-ag-ui-event-types-for-building-agents-the-right-way | 17 olay tipinin ürün açısından okunuşu | K3 |
| https://learn.microsoft.com/en-us/agent-framework/integrations/ag-ui/ | SSE ile gerçek zamanlı akış, insan onayı, durum senkronizasyonu | K3 |
| https://devblogs.microsoft.com/agent-framework/ag-ui-multi-agent-workflow-demo/ | React ön yüzün SSE akışını tüketip iş akışı durumunu render etmesi; onay modali deseni | K3 |
| https://docs.langchain.com/oss/python/langchain/frontend/human-in-the-loop | Interrupt algılama → onay kartı → `resume` kalıbı | K3 |
| https://www.assistant-ui.com/ | React sohbet UI primitives; streaming, gen-UI, HITL | K1 |
| https://openapi-ts.dev/openapi-react-query/ | `openapi-fetch` üzerine ~1 KB TanStack Query sarmalayıcısı | K1 |
| https://github.com/OpenAPI-Qraft/openapi-qraft | Proxy tabanlı tip güvenli hook üretimi (alternatif) | K1 |
| https://heyapi.dev/docs/openapi/typescript/plugins/tanstack-query | `queryOptions` fabrikası üreten alternatif | K1 |
| https://openapi.tools/tools/jentic-arazzo-editor | Form tabanlı Arazzo editörü, canlı diyagram, YAML senkronu | K1 |
| https://openapi.tools/tools/jentic-arazzo-ui | Arazzo belgesini sekans/akış diyagramına çeviren açık kaynak görselleştirici | K1 |
| https://spec.openapis.org/arazzo/latest.html | Arazzo spesifikasyonu (bizim hedefimiz **1.0.1**, ADR-0014 §C düzeltmesi) | K2 |
| https://zylos.ai/research/2026-05-28-agentic-ux-frontend-design-patterns-ai-agents/ | Plan-and-execute, güven sinyali, kademeli devir; güven kalibrasyonu uyarısı | K3 |
| https://mantlr.com/blog/designing-for-ai-agents-ux-patterns-2026 | Otonomi kaydırıcısı (`Suggest`/`Co-pilot`/`Autopilot`), onay örüntülerinin kalıcılaştırılması | K3 |
| https://thenewstack.io/sentrys-seer-agent-debug/ | Tek ajan mimarisi + iki iş akışı; otonomi seviyesinin kuruluş kontrolünde olması | K3 |
