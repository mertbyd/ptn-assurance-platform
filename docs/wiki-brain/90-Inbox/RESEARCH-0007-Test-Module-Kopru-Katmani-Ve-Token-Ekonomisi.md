---
id: RESEARCH-0007
type: research
status: draft
title: Test Module kopru katmani — iki checker'i tek ajan yuzeyine indiren tasarim ve token ekonomisi
updated: 2026-08-12
decision_refs:
  - ADR-0002
  - ADR-0007
  - ADR-0008
rule_refs:
  - RULE-0001
  - RULE-0004
---

# Test Module köprü katmanı — iki checker'ı tek ajan yüzeyine indirmek

> Kanonik değildir. [[90-Inbox/RESEARCH-0003-MCP-Senaryo-Testi-Mimarisi|RESEARCH-0003]] (mimari tez)
> ve [[90-Inbox/RESEARCH-0006-TestModule-Global-Tarama-Ve-Veri-Modeli|RESEARCH-0006]] (veri modeli)
> belgelerinin üçüncüsüdür. Sorusu şudur: **Test Module, iki checker ile ajan arasında hangi köprü
> özelliklerini taşırsa checker'lar daha işlevli ve daha az token yakan hale gelir?**
>
> Kanıt sınıfları: **K1** çalışan kod · **K2** birincil spesifikasyon/resmî ürün dokümantasyonu ·
> **K3** ikincil ölçüm iddiası.

---

## 0. Tez

Ajanı doğrudan iki checker'a bağlarsak, ajan **iki ayrı sözlük** öğrenmek, **N ayrı çağrı**
yapmak ve **ham sonuçları bağlamında taşımak** zorunda kalır. Köprü katmanının işi bunların
üçünü de ortadan kaldırmaktır: tek sözlük, iş-şekilli tek çağrı, karar döndüren yanıt.

Ölçülmüş referanslar bu üç eksenin her birinde büyük kazanç gösteriyor:
tool yüzeyini daraltmak **%60–90** bağlam düşüşü (GitHub MCP, K2), tool tanımlarını
ertelemek **%85** düşüş + seçim doğruluğunda 49→74 puan (Anthropic, K2), ara sonuçları
bağlam yerine çalıştırma ortamında tutmak **%98,7** düşüş (Anthropic, K2), yanıt formatını
değiştirmek aynı token'da **~5 kat** kayıt (Datadog, K2).

---

## 1. Bugünkü sorun — köprü olmasa ne olur

Ajan "orders API'si için senaryo yaz" görevini doğrudan checker'larla yapsa:

| Adım | Çağrı | Bağlama giren |
|---|---|---|
| 1 | `contract.operation.find` | operasyon özeti |
| 2 | `db.table.describe` | tablo şekli |
| 3 | `db.binding.suggest` | eşleme önerileri |
| 4 | `scenario.validate` | doğrulama hataları |
| 5 | `scenario.dryRun` | adım sonuçları |

**Beş ayrı çıkarım turu.** Her tur modelin tüm bağlamını yeniden okumasıdır. Üstelik ajan
`AssertionOutcomeCodes` (DB) ve `ConformanceOutcomeCodes` (API) diye **iki ayrı kod sözlüğü**
öğrenmek zorundadır; ikisi farklı gramerdedir.

Köprü katmanı bunu **tek çağrıya** indirir ve tek sözlük gösterir.

---

## 2. Grup A — Yanıtın şekli (ne döndürüyoruz)

### A-01 — Token bütçeli sayfalama (kayıt sayısı değil)

**Ne:** Sayfa boyutu "20 kayıt" değil, "8 KB token bütçesi" olarak belirlenir; bütçe dolunca
kesilir ve `nextCursor` döner.

**Neden:** Checker'lar bugün kayıt sayısıyla sayfalıyor (`FindingQueryInput.MaxResultCount`,
varsayılan 20 — K1). Ama tek bir bulgu 200 bayt da olabilir 8 KB da. 20 kayıt istediğinde
ajan ne kadar token yiyeceğini **bilemiyor**.

**Kanıt:** Datadog aynı sorunu yaşayıp kayıt sayısından token bütçesine geçmiş: *"the server
cuts off its response after a certain number of tokens and returns a cursor for more"* (K2).

**Not:** Checker tarafında `ComparisonRunConsts.DefaultFindingResponseBytes = 32 KB` ve
`FindingResponseEnvelopeReserveBytes = 512` sabitleri **zaten var** (K1) — yani bayt bütçesi
kavramı checker'da mevcut; köprü bunu token bütçesine çevirip ajan yüzeyine taşır.

### A-02 — Tablo verisi JSON değil TSV

**Ne:** Bulgu listesi, adım sonuçları, tablo tanımı gibi **düzenli tablo** şeklindeki veriler
JSON yerine TSV/CSV olarak döner.

**Neden:** JSON her satırda alan adlarını tekrarlar. TSV başlığı bir kez yazar.

**Kanıt:** Datadog ölçümü: CSV/TSV tablo verisinde **~%50 daha az token**, YAML iç içe yapıda
JSON'a göre **~%20 daha az**; bir tool'da *"aynı token sayısında ~5 kat daha fazla kayıt"* (K2).

**Sınır:** Yalnız düz tablo için. İç içe yapı (teşhis raporu, Overlay yaması) JSON kalır.

### A-03 — Varsayılan dar yanıt (alan kırpma)

**Ne:** Yanıtta yalnız kararı etkileyen alanlar döner. Geri kalanı ajan **isterse** çeker.

**Neden:** Datadog'un tespiti net: *"Returning fewer fields from tool responses is the single
highest-leverage optimization."* Çoğu MCP sunucusu API yanıtını olduğu gibi geçiriyor (K2).

**Bizde somut karşılığı:** `FindingDto` 15 alan taşıyor (`SourceValue`, `TargetValue`,
`SourceRowCount`, `TargetRowCount`, `RowCountDifference`, `ChangeSummary`... — K1).
Bakım anında ajanın ihtiyacı **4 alan**: `Fingerprint`, `SeverityCode`, `Address`, `ChangeStateCode`.

### A-04 — Ham veri değil **karar** döndür

**Ne:** `impact.summary` çağrısı 200 bulgu döndürmez; şunu döndürür:
`{ etkilenenSenaryo: 3, enYuksekSeverity: "Breaking", yeniBulgu: 7, oneriHazir: true }`

**Neden:** Datadog'un "sorgu yazdır, ham veri çektirme" dersi: seçici alan + sunucu tarafı
toplama ile örnekleme yapmaya göre **%40 daha ucuz** koşu (K2).

**Bizde:** Toplama zaten veritabanında yapılabilir (`scenario_finding_links` üzerinde
`COUNT` + `MAX(severity)`); ajanın 200 satırı görmesine gerek yok.

### A-05 — Ağır çıktı `resource_link` ile bağlam dışında

**Ne:** Rapor, tam bulgu listesi, kanıt gövdesi bağlam yerine **link** olarak döner.

**Kanıt:** MCP 2026-07-28 tool sonucu `type: "resource_link"` içerik tipini tanımlıyor:
`uri`, `name`, `description`, `mimeType` (K2). Ajan gerçekten gerekiyorsa çeker.

### A-06 — `outputSchema` + `structuredContent`

**Ne:** Her tool yanıt şemasını yayınlar; sonuç `structuredContent` alanında tipli döner.

**Neden:** Model JSON şeklini tahmin etmez, istemci doğrulayabilir. Spec: *"Servers MUST
provide structured results that conform to this schema"* (K2).

---

## 3. Grup B — Yüzeyin şekli (kaç tool, nasıl bulunur)

### B-01 — İş-şekilli tool'lar, endpoint-şekilli değil

**Ne:** `contract.operation.find` + `db.table.describe` + `db.binding.suggest` ayrı ayrı değil;
tek `scenario.draft` çağrısı üçünü içeride yapar ve **taslak senaryo** döner.

**Kanıt:** Datadog: *"Rather than one tool per API endpoint, we design tools that can serve
multiple use cases."* (K2)

**Kazanç:** 3 çıkarım turu → 1. Ara sonuçlar (tam tablo tanımı, operasyon listesi) ajanın
bağlamına **hiç girmez**.

### B-02 — An bazında toolset profilleri

**Ne:** Yazım ajanı 6, teşhis ajanı 3, bakım ajanı 3 tool görür. Hepsi aynı anda açık değildir.

**Kanıt:** GitHub MCP: yalnız gereken toolset'leri açmak, varsayılan tüm setlere göre
**%60–90 bağlam düşüşü** sağlıyor; ayrıca dinamik toolset ile ajan çalışma anında set
açabiliyor (K2).

### B-03 — `defer_loading` / Tool Search uyumu

**Ne:** Çekirdek 3–4 tool hep yüklü; gerisi ertelenmiş, ajan arayarak bulur.

**Kanıt:** Anthropic ölçümü: **77K → 8,7K token (%85 düşüş)**; tool seçim doğruluğu
Opus 4'te 49→74, Opus 4.5'te 79,5→88,1 (K2).

**Akademik doğrulama:** RAG-MCP doğruluğu %13'ten %43'e çıkarıp token maliyetini yarıya
indiriyor; MCP-Zero bağlam kullanımını iki büyüklük mertebesi azaltıyor (K2/K3).

### B-04 — Prosedür bilgisi tool değil **Skill**

**Ne:** "Bu evde senaryo nasıl yazılır", "hangi assertion hangi durumda", "onay akışı"
gibi **prosedür** bilgisi tool açıklamasına gömülmez; Skill olarak durur.

**Kanıt:** Skill'ler progressive disclosure ile yüklenir — tetiklenene kadar ~100 token yer
kaplar; MCP tool tanımları ise oturum başında **tam olarak** yüklenir (K2).

### B-05 — Tool tanımlarında örnek çağrılar

**Ne:** Her tool tanımına 1–5 gerçekçi örnek çağrı konur.

**Kanıt:** Anthropic ölçümü: karmaşık parametre kullanımında doğruluk **%72 → %90** (K2).

---

## 4. Grup C — Tekrarı önleme

### C-01 — Deterministik tool sırası + `ttlMs` / `cacheScope`

**Ne:** `tools/list` her seferinde **aynı sırada** döner ve `ttlMs` + `cacheScope` taşır.

**Kanıt:** Spec bunu açıkça gerekçelendiriyor: *"Deterministic ordering enables clients to
reliably cache the tool list and improves LLM prompt cache hit rates when tools are included
in model context."* (K2)

**Neden bizim için önemli:** Prompt cache isabeti, aynı oturumda tekrar tekrar aynı tool
tanımlarının **yeniden ücretlendirilmemesi** demektir.

### C-02 — İçerik hash'i ile bilgi önbelleği

**Ne:** Operasyon özeti ve tablo tanımı gibi pahalı hesaplamalar, kaynak dokümanın
**`CanonicalHash`** değeri anahtarıyla önbelleklenir.

**Bizde hazır:** `SpecContent.CanonicalHash` zaten var ve "anlamsal eşitlik anahtarı" olarak
tanımlanmış (K1). Spec değişmediyse özet yeniden hesaplanmaz **ve metni bayt bayt aynı kalır**
— bu ikincisi prompt cache isabeti için birincisinden değerlidir.

### C-03 — Handle deseni

**Ne:** Taslak senaryo, koşu ve teşhis oturumu opak birer **handle** döner; sonraki çağrılar
handle alır. Gövde asla iki kez bağlama girmez.

**Kanıt:** Spec'in "Stateful Tools" rehberi: handle opak olmalı, ömrü sınırlı olmalı,
yetki **her çağrıda** doğrulanmalı, süresi dolan handle için ajanın kurtarabileceği bir hata
dönmelidir (K2).

### C-04 — Delta yanıt

**Ne:** "Son koşudan beri ne değişti" sorusu tam liste değil fark döndürür.

**Bizde hazır:** İki checker'da da `SinceRunId` ve `ChangeStateCode` (`New`/`Known`/`Resolved`)
`0.2.0-alpha.2` ile public (K1).

### C-05 — Ön-hesaplanmış ajan-hazır projeksiyon

**Ne:** Ajanın soracağı soruların cevabı koşu biterken **bir kez** hesaplanıp saklanır.

**Bizde:** `scenario_step_bindings` ve `scenario_finding_links` tam olarak bu
(RESEARCH-0006 §5.4/§5.6). Buna koşu başına `impact_summary` eklenir.

---

## 5. Grup D — Ajanı eğiten yüzey

### D-01 — Tek sözlük (vocabulary unification)

**Ne:** Köprü, iki checker'ın kod kümelerini **tek** ajan sözlüğüne normalize eder.

**Neden:** Bugün `AssertionOutcomeCodes` (DB) ve `ConformanceOutcomeCodes` (API) ayrı
gramerlerde (K1: DB `PascalCase`, API `kebab-case`). Ajan iki sözlük öğrenirse hem token
harcar hem yanlış eşleştirir.

**Sınır:** Normalizasyon **köprüde** yapılır; checker'ların kendi kararlı kodları değişmez
(ADR-0008: checker'ın MCP'ye borcu kararlı kod kümesidir).

### D-02 — Öğreten hata mesajları

**Ne:** Hata, ajanın bir sonraki adımını **söyler**.

**Kanıt:** Datadog: *"An error message like 'invalid query' usually isn't helpful; something
like 'unknown field stauts – did you mean status?' gives the agent a clear next step."* (K2)

**Bizdeki karşılığı:** `KeyNotUnique` ham haliyle "anahtar tekil değil" der. Köprü şunu
demeli: *"`sales.Orders` tablosunda `CustomerRef` tekil değil; tekil alternatifler: `Id` (PK),
`OrderNumber` (unique)."*

### D-03 — Yanıt içinde bağlamsal ipucu

**Ne:** Sonuç doğru ama şüpheliyse, yanıt bir ipucu taşır.

**Kanıt:** Datadog: *"You searched for payment service, did you mean payments service instead?"* (K2)

**Bizde:** *"`sales.Order` bulunamadı; benzer: `sales.Orders`."*

### D-04 — Protokol hatası ile araç hatası ayrımı

**Ne:** Ajanın düzeltebileceği hatalar `isError: true` ile **araç sonucu** olarak döner;
düzeltemeyecekleri JSON-RPC protokol hatası olur.

**Kanıt:** Spec: araç yürütme hataları *"contain actionable feedback that language models can
use to self-correct and retry"*; istemciler bunları modele **vermelidir** (K2).

---

## 6. Grup E — İleri seviye: kod çalıştırma yüzeyi

### E-01 — Toplu iş için tool çağrısı değil kod

**Ne:** "Tüm senaryoları tara, `sales` şemasına dokunan ve son 30 günde 2 kereden fazla
kırılmış olanları listele" gibi işler N tool çağrısı yerine **tek sandbox script**i ile yapılır.

**Kanıt:** İki ayrı Anthropic ölçümü:
- MCP tool'larını kod olarak sunmak: **150.000 → 2.000 token (%98,7)**; ara sonuçlar
  varsayılan olarak yürütme ortamında kalır, PII bağlama hiç girmez.
- Programmatic tool calling: karmaşık araştırma görevinde **43.588 → 27.297 token (%37)**,
  doğrulukta artış, 19+ çıkarım turu ortadan kalkıyor (K2).

**Maliyeti:** Güvenli sandbox, kaynak sınırı, izleme. Operasyon yükü gerçektir.

**Karar önerisi:** Faz 1'de **hayır**. Önce A/B/C/D grupları uygulanır, ölçülür. Kod
çalıştırma ancak "tek çağrıya sığmayan toplu analiz" gerçek bir ihtiyaç haline gelirse açılır.

### E-02 — Bağlam mühendisliği disiplinleri

Köprünün doğrudan desteklemesi gerekenler:
- **Just-in-time retrieval:** ağır içerik yerine hafif kimlik (handle, `resource_link`).
- **Structured note-taking:** ajanın ara notu bağlamda değil, `heal_proposals.rationale`
  gibi kalıcı alanlarda.
- **Compaction:** uzun teşhis oturumunda önceki adımların özeti köprüde tutulur.

(Anthropic'in "effective context engineering" çerçevesi — K2)

---

## 7. Ölçme: iddia edilen kazanç doğrulanabilir olmalı

Köprü her çağrı için şunları kaydeder ve `gen_ai.usage.*` semconv alanlarıyla raporlar:

| Ölçüt | Hedef |
|---|---|
| Tool kataloğu statik maliyeti | ≤ 3.000 token |
| Yazım anı (A) toplam | ≤ 15.000 token / senaryo |
| Koşum anı (B) | **0** |
| Teşhis anı (C) | ≤ 5.000 token / kırmızı koşu |
| Bakım anı (D) | ≤ 2.000 token / bulgu |
| Tek tool yanıtı | ≤ 8.000 token (bütçe dolunca cursor) |

Bu tablo CI kapısına bağlanır: eşik aşılırsa build kırılır. Aksi halde "token ucuz" iddiası
ölçülmeyen bir temenni olur.

---

## 8. Öncelik

| Dalga | Maddeler | Gerekçe |
|---|---|---|
| **K1 — hemen** | A-03 dar yanıt, A-05 resource_link, A-06 outputSchema, B-01 iş-şekilli tool, D-01 tek sözlük | En yüksek kazanç/maliyet oranı; hiçbiri altyapı gerektirmiyor |
| **K2 — kısa vade** | A-01 token bütçesi, A-02 TSV, B-02 profiller, C-01 ttlMs, C-03 handle, D-02/D-03 öğreten hata | Ölçülebilir kazanç, orta iş |
| **K3 — orta vade** | A-04 karar döndürme, C-02 hash cache, C-04 delta, C-05 projeksiyon, B-05 örnekler | Veri modeli hazır olduktan sonra |
| **K4 — ileride** | B-03 defer_loading, B-04 Skill katmanı, E-01 kod çalıştırma | Ölçüm gösterirse |

---

## 9. Kaynaklar (bu belgeye özel; erişim 2026-08-12)

| Kaynak | Neyi kanıtlıyor | Sınıf |
|---|---|---|
| https://modelcontextprotocol.io/specification/2026-07-28/server/tools | `outputSchema`/`structuredContent`, `resource_link`, `ttlMs`/`cacheScope`, deterministik sıra, Stateful Tools handle rehberi, araç hatası ile protokol hatası ayrımı | K2 |
| https://www.datadoghq.com/blog/engineering/mcp-server-agent-tools/ | Token bütçeli sayfalama; CSV/TSV ~%50, YAML ~%20 tasarruf, ~5× kayıt; alan kırpma "en yüksek kaldıraç"; sorgu > ham veri (~%40 ucuz); endpoint başına değil iş başına tool; öğreten hata mesajı; bağlamsal ipucu | K2 |
| https://www.anthropic.com/engineering/advanced-tool-use | Tool Search: 77K→8,7K (%85), doğruluk 49→74 / 79,5→88,1; Programmatic tool calling: 43.588→27.297 (%37); tool örnekleri: %72→%90 | K2 |
| https://www.anthropic.com/engineering/code-execution-with-mcp | Kod olarak sunulan tool'lar: 150K→2K (%98,7); ara sonuçların ortamda kalması; PII'nin bağlama girmemesi; progressive disclosure | K2 |
| https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents | Compaction, structured note-taking, just-in-time retrieval, alt-ajan izolasyonu | K2 |
| https://deepwiki.com/github/github-mcp-server/3-github-toolsets | Toolset seçimi ile %60–90 bağlam düşüşü; dinamik toolset keşfi | K2 |
| https://arxiv.org/html/2506.01056v1 (MCP-Zero) | Aktif tool keşfi; bağlam kullanımında iki büyüklük mertebesi azalma | K2 |
| RAG-MCP (retrieval ile tool seçimi) | Doğruluk %13 → %43, token maliyeti yarıya | K3 |
| https://atlan.com/know/ai-agent/ai-agent-skills/agent-skills-vs-mcp/ | Skill progressive disclosure (~100 token) ile MCP tool tam yükleme farkı | K3 |
| https://www.zenml.io/llmops-database/scaling-an-mcp-server-for-error-monitoring-to-60-million-monthly-requests | MCP sunucusunu üretim servisi gibi ele alma; gözlemlenebilirlik ve bağlam kirliliği dersleri | K3 |
</content>
