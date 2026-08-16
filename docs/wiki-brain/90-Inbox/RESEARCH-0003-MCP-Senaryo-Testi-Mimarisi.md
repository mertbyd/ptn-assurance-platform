---
id: RESEARCH-0003
type: research
status: draft
title: MCP ile senaryo testi mimarisi ve az-token tester tasarimi
updated: 2026-08-12
decision_refs:
  - ADR-0002
  - ADR-0005
rule_refs:
  - RULE-0001
  - RULE-0003
  - RULE-0004
---

# MCP ile senaryo testi — mimari, kanıtlanmış örnek projeler ve token ekonomisi

> Kanonik değildir. [[90-Inbox/RESEARCH-0001-DatabaseChecker-Genisletme-Analizi|RESEARCH-0001]] (paket/platform)
> ve [[90-Inbox/RESEARCH-0002-DbChecker-Motor-Yetenek-Haritasi|RESEARCH-0002]] (motor genişliği)
> belgelerinin üçüncüsüdür: **iki geniş motorun üstüne kurulacak tester'ın mimarisi.**
> Kanıt sınıfları RESEARCH-0001 §0 ile aynı (K1 / K2 / K3).

---

## 1. Tezin tamamı tek paragrafta

Senaryo testini "MCP'ye bağlı bir ajanın her koşuda uygulamayı gezip karar vermesi" olarak kurgularsak,
her test koşusu **on binlerce token** yakar, sonuç non-deterministik olur ve test raporu güvenilmez hale
gelir. Doğru mimari şudur: **model yazım (authoring) döngüsündedir, koşum (execution) döngüsünde değildir.**
Ajan bir kez, MCP üzerinden, insan gözetiminde **kalıcı ve deterministik bir senaryo artefaktı** üretir;
o artefakt bundan sonra **sıfır token** ile, sıradan bir .NET runner tarafından çalıştırılır. Model yalnız
üç anda geri gelir: (1) yeni senaryo yazarken, (2) bir koşu başarısız olduğunda **sınırlı kanıtla** teşhis
için, (3) **checker'lar bir sözleşme değişikliği raporladığında** etkilenen senaryoyu onarmak için.

Üçüncü madde bu platformun asıl silahıdır ve bizde olup piyasada olmayan şeydir: *ajana neyin değiştiğini
tahmin ettirmiyoruz; iki deterministik motor ona söylüyor.*

---

## 2. Kanıt: model neden koşum döngüsünde olmamalı?

| Kanıt | Kaynak | Sınıf |
|---|---|---|
| Playwright'ın üç ajanı (**planner → generator → healer**) modeli koşuma değil **üretime** koyar: planner Markdown test planı, generator TypeScript test dosyası üretir; sonrasında test sıradan Playwright testi olarak koşar. Ajanların kendisi "bir talimat ve MCP tool tanımı derlemesidir, bağımsız bir binary değil" | playwright.dev/docs/test-agents | K2 |
| Aynı ekosistemde ölçülen fark: tam MCP ajan koşusu **~114K token/test**, CLI/skill tabanlı akış **~27K** — 4 kat | pratisyen ölçümü | K3 |
| **RESTifAI** (ICSE 2026 Demonstration Track): temel iddiası **"test üretimini test koşumundan ayırmak"**; LLM somut script değil, **yeniden kullanılabilir, parametrik test spesifikasyonu** üretir; spesifikasyon farklı API sürümlerine/deployment'lara değiştirilmeden uygulanır | arXiv 2512.08706 | K2 |
| Anthropic: MCP tool'larını tek tek çağırmak yerine **kod yazdırmak** bir iş akışında **150.000 → 2.000 token (%98,7)**; ara sonuçlar varsayılan olarak yürütme ortamında kalır | anthropic.com/engineering/code-execution-with-mcp | K2 |
| Agent Skills: **progressive disclosure**; her skill bağlamda yalnızca birkaç düzine token yer kaplar, gövdesi ancak tetiklenince yüklenir; 50 skill ≈ 5.000 token sabit bağlam | platform.claude.com / atlan | K2/K3 |
| MCP 2026-07-28: `tools/list` sonucu `ttlMs` + `cacheScope` taşır ve sunucuların **deterministik sırayla** tool döndürmesi istenir — spec gerekçeyi açıkça yazar: *"improves LLM prompt cache hit rates when tools are included in model context"* | modelcontextprotocol.io | K2 |
| **Jentic "MCP Tool Trap"**: tool sayısı arttıkça token şişmesi, seçim doğruluğunda düşüş, muhakemede düşüş ve bakım/güvenlik yükü; sorun *"birkaç tool ekledikten sonra şaşırtıcı derecede erken"* başlıyor. Önerileri: bilgiyi bağlamın dışında bir **knowledge layer**'da tut, MCP'yi taşıyıcı say — *"MCP is the USB-C port, not the hard drive"* | jentic.com/blog/the-mcp-tool-trap | K2 |

Beş bağımsız kaynak aynı yere çıkıyor: **bağlama ne kadar az şey koyarsan o kadar iyi çalışır ve o kadar
ucuz olur.** Bunun test alanındaki karşılığı, koşum anında modelin hiç devrede olmamasıdır.

---

## 3. Örnek proje analizleri — ne öğreniyoruz?

### 3.1 Playwright Test Agents (Microsoft) — **kopyalanacak iskelet**

| Bileşen | Ne yapar | Ürettiği artefakt |
|---|---|---|
| **planner** | Uygulamayı gezer, seed test ile ortamı kurar | `specs/*.md` — insan-okur test planı |
| **generator** | Planı çalıştırılabilir teste çevirir, **seçicileri ve assertion'ları canlıda doğrular** | `tests/*.spec.ts` |
| **healer** | Başarısız testi tekrar koşar, kırık locator/bekleme sorunlarını onarır | yama |

Üç ders:
1. **İki aşamalı artefakt.** Önce *insan-okur plan*, sonra *makine-koşar test*. İnsan onayı ortadadır.
   Plan Markdown olduğu için gözden geçirmesi ucuzdur; kod incelemesi kadar yük getirmez.
2. **Generator canlıda doğrular.** Üretilen test "umarım çalışır" değildir; üretim anında gerçek sisteme karşı sınanır.
3. **Ajanlar binary değil, talimat + tool demeti.** Bizim tarafta da "tester ajanı" bir uygulama değil,
   bir prompt + izinli MCP tool alt kümesi olacak.

Ayrıca mimari bir gözlem: Playwright MCP **ekran görüntüsü değil, erişilebilirlik ağacı** üzerinde çalışır —
yani modele piksel değil, **yapılandırılmış ve aranabilir metin** verir. Bizim karşılığımız: modele ham
JSON gövdesi/ham satır değil, **şema özeti ve assertion sonucu** vermek.

### 3.2 Jentic / OAK + Arazzo — **bilgi katmanı ayrımı**

Jentic'in tezi: agentic bilgi (**ne çağrılır, hangi sırayla, hangi başarı ölçütüyle**) OpenAPI + Arazzo gibi
**deklaratif** formatlarda dursun; MCP yalnızca bu bilgiye erişimin ergonomik yolu olsun. Bu, bizim
"checker'lar bilgi motoru, MCP taşıyıcı" ayrımımızın dışarıdan doğrulanmış hali.

**Bizim için doğrudan sonuç:** senaryo planını kendi icat ettiğimiz bir DSL'de tutmamalıyız. Kamuya açık
bir standartta tutarsak model onu **zaten bilir**; formatı anlatmak için bağlam harcamayız.

### 3.3 Testkube MCP — **tool kataloğu nasıl olmamalı (ve nasıl olmalı)**

Testkube MCP sunucusu **30 tool** yayınlıyor: workflow (7), workflow template (4), agent (1),
execution (8+), artifact (2), query (2), schema (2), utility (3–4).

İki ders, biri olumlu biri olumsuz:

- **Olumsuz:** 30 tool, §2'deki "tool trap" eşiğinin çok üstünde. Tek bir görevde bunların hepsi bağlamda
  duruyorsa doğruluk düşer.
- **Olumlu ve kopyalanacak:** `query_workflows` / `query_executions` **JSONPath filtresi** alıyor ve
  `list_artifacts` / `read_artifact` ayrı tool'lar. Yani **"listele" ile "gövdeyi getir" ayrılmış**.
  Ajan önce ucuz meta veriyi alır, gerçekten gerekiyorsa gövdeyi çeker. Bizim `run.findings.page`
  tasarımımızın aynısı.

### 3.4 Arazzo 1.1.0 (OpenAPI Initiative) — **senaryo formatı sorunu zaten çözülmüş**

Arazzo 1.1.0 (17 Mayıs 2026) incelendiğinde, bizim "TestPlan" için sıfırdan tasarlayacağımız her şeyin
zaten spec'te olduğu görülüyor:

| İhtiyacımız | Arazzo karşılığı |
|---|---|
| Adım dizisi | `workflows[].steps[]` |
| Çağrılacak operasyon | `operationId` / `operationPath` / `workflowId` (birbirini dışlar) |
| Girdi parametreleri | `inputs` (JSON Schema) + `parameters` |
| **Adımdan değer çıkarma** | `outputs` + runtime expression: `$response.body#/id` |
| **Adımlar arası korelasyon** | `$steps.<stepId>.outputs.<field>` |
| **Başarı ölçütü** | `successCriteria` — dört tip: `simple` (`$statusCode == 200`), `regex`, **`jsonpath` (RFC 9535)**, `xpath`. Çoklu ölçütte hepsi geçmeli (AND) |
| **Sınırlı bekleme / yeniden deneme** | `onFailure: retry` + `retryLimit` + `retryAfter` |
| Dallanma | `onSuccess/onFailure: goto/end` + koşullu `criteria` |
| Adım zaman aşımı | `timeout` (ms) |
| **Asenkron kanıt** | `action: receive`, **`correlationId`**, `dependsOn` (join noktaları), AsyncAPI `sourceDescriptions` |
| Yeniden kullanım | `components.inputs/parameters/successActions/failureActions` + `$components.*` referansları |

**Bu, arşiv §14.7'de "asenkron bildirim testi: listen-before-act, correlation ID, deadline, cardinality
gerekir" diye yazılan gereksinimin standartlaşmış hali.** Kendi modelimizi yazmamıza gerek yok.

**Eksik olan tek şey: veritabanı adımı.** Arazzo HTTP ve mesaj adımlarını tanımlar, DB assertion'ını tanımlamaz.
Bunu spec'in uzantı mekanizmasıyla ekleriz (§5.2).

### 3.5 Akademik cephe — üretimi koşumdan ayırma tezi

- **RESTifAI** (ICSE 2026): LLM'in ürettiği şey somut script değil **yeniden kullanılabilir spesifikasyon**;
  aynı spesifikasyon farklı API sürümlerinde değiştirilmeden koşar.
- **AutoRestTest** (arXiv 2501.08600): dört ajanı (API / bağımlılık / parametre / değer) çok-ajanlı
  pekiştirmeli öğrenme + semantik bağımlılık grafiği ile birleştiriyor; 12 gerçek serviste kod kapsamı,
  operasyon kapsamı ve hata tespitinde önde gelen kara-kutu araçlarını (RESTGPT destekli olanlar dahil) geçiyor.
- **Ders:** ajanın asıl değeri "çağrı yapmak" değil, **bağımlılık ve değer keşfi**dir — yani yazım aşamasıdır.

### 3.6 Specmatic — sözleşmeyi çalıştırılabilir kılmak

Specmatic OpenAPI/AsyncAPI/gRPC/GraphQL spesifikasyonlarını **kod yazmadan** çalıştırılabilir sözleşmeye
çeviriyor; aynı spec'ten hem contract test hem stub server üretiyor; geriye dönük uyumluluğu
**yeni spec'ten mock ayağa kaldırıp eski spec'ten üretilen testleri ona koşturarak** ölçüyor.

**Ders:** bizim api-contract checker'ımızın ürettiği fark, tek başına bir "uyumluluk kapısı"na
dönüştürülebilir. Ve senaryo testleri için **stub** üretimi, dış bağımlılıkları izole etmenin standart yolu.

---

## 4. Token ekonomisi: dört an, dört bütçe

Bir tester'ın maliyeti tek bir sayı değildir. Dört ayrı ana ayrılır ve **her biri farklı optimize edilir.**

| An | Sıklık | Token bütçesi | Optimizasyon |
|---|---|---|---|
| **A — Yazım** | Senaryo başına **bir kez** | Yüksek (10–50K kabul edilebilir) | Amorti edilir; kalite burada satın alınır |
| **B — Koşum** | Her CI, her gece, her deploy | **SIFIR** | Model hiç devrede değil |
| **C — Teşhis** | Yalnız kırmızı koşuda | Düşük (1–5K) | Sınırlı kanıt: yalnız patlayan adım + minimal bağlam |
| **D — Bakım** | Yalnız sözleşme/şema değiştiğinde | Düşük–orta | **Checker bulgusu girdi olarak verilir**; ajan dünyayı yeniden keşfetmez |

### 4.1 B anının sıfır olması neden mümkün?

Çünkü artefakt deterministiktir: Arazzo planı + JSON Schema + DB assertion'ı. Koşum, .NET runner'ın işidir.
Bu, Playwright'ın generator'dan sonra sıradan `playwright test` koşmasıyla birebir aynı yapıdır (K2).

### 4.2 D anı — bu platformun asıl farkı

Piyasadaki "healer" ajanları **başarısızlıktan geriye doğru tahmin eder**: test kırıldı, UI'ya bakayım,
benzer bir eleman bulayım. Bizde ise iki deterministik motor **neyin değiştiğini zaten söylüyor**:

```text
api-contract checker  ->  "POST /orders yanit semasinda 'status' alani opsiyonel oldu — Breaking"
database checker      ->  "sales.Orders.Status  varchar(20) -> varchar(50)  — NonBreaking"
                                  |
                                  v
                        Etkilenen senaryo secimi (fingerprint eslesmesi)
                                  |
                                  v
              Ajan YALNIZ o adimin successCriteria'sini gunceller
```

Ajanın bağlamına giren şey: bir fark kaydı (birkaç yüz token) + etkilenen adım (birkaç yüz token).
Uygulamayı gezmek, şemayı okumak, tüm planı yeniden üretmek **yok**.

Bunu mümkün kılan üç şey ve hepsi önceki belgelerde zaten öneri olarak duruyor:
**bulgu fingerprint'i** (RESEARCH-0001/E-02), **şiddet sınıflandırması** (E-03) ve
**şema fingerprint'i** (E-01). Yani senaryo testi hedefi, o önerilerin gerekçesini güçlendiriyor.

### 4.3 Kaçınılacak desen: SUT'u MCP tool'una çevirmek

OpenAPI'yi otomatik MCP tool'una çeviren jeneratörler var. Test için **yanlış** bir yoldur:
200 endpoint'lik bir API 200 tool demektir; §2'deki tool trap'in tam ortasıdır ve modele test edilen
sistemin ham gövdelerini bağlama boşaltır. Doğrusu: **bir tane** `api.call` tool'u + operasyon bilgisini
talep üzerine veren bir bilgi katmanı (Jentic/OAK deseni).

---

## 5. Önerilen mimari

### 5.1 Katmanlar

```text
                 ┌──────────────────────────────────────────────┐
   INSAN  ─────► │  Tester Agent (prompt + izinli MCP alt kumesi)│
                 └───────────────┬──────────────────────────────┘
                                 │  MCP (2026-07-28, stateless)
                 ┌───────────────▼──────────────────────────────┐
                 │  CheckNexus.Assurance.Mcp                    │
                 │  (yalniz *.Application.Contracts'a baglanir) │
                 └───┬───────────────┬───────────────┬──────────┘
                     │               │               │
        ┌────────────▼───┐  ┌────────▼────────┐  ┌───▼─────────────────┐
        │ api-contract   │  │ database-checker│  │ Test Module         │
        │ (bilgi motoru) │  │ (bilgi motoru)  │  │ (eylem + kanit)     │
        └────────────────┘  └─────────────────┘  └───┬─────────────────┘
                                                     │
                                          ┌──────────▼──────────┐
                                          │ Scenario Runner     │
                                          │ *** MODEL YOK ***   │
                                          └─────────────────────┘
```

**Değişmez:** Runner hiçbir koşulda modele başvurmaz. Bir adım "karar veremiyorsa" test **başarısızdır**,
modele sorulmaz. Non-determinizm test raporuna girmez.

### 5.2 Senaryo artefaktı: Arazzo 1.1.0 + DB uzantısı

Kendi DSL'imizi yazmıyoruz. Arazzo dokümanı saklıyoruz; DB adımını uzantı olarak ekliyoruz:

```yaml
arazzo: 1.1.0
info: { title: Order creation, version: 1.0.0 }
sourceDescriptions:
  - name: ordersApi
    url: ./openapi.json
    type: openapi
workflows:
  - workflowId: create-order-persists
    inputs:
      type: object
      properties: { sku: { type: string } }
    steps:
      - stepId: createOrder
        operationId: createOrder
        requestBody:
          contentType: application/json
          payload: { sku: "{$inputs.sku}", quantity: 1 }
        successCriteria:
          - condition: $statusCode == 201
          - context: $response.body
            condition: '$[?length(@.id) > 0]'
            type: jsonpath
        outputs:
          orderId: $response.body#/id

      # --- CheckNexus uzantisi: veritabani kaniti ---
      - stepId: orderPersisted
        x-checknexus-db:
          connectionRef: sales-db
          operation: assertRow            # assertRow | assertCount | assertAbsent
          schema: sales
          table: Orders
          key:    { Id: "{$steps.createOrder.outputs.orderId}" }
          expect: { Status: "Pending" }
          valueRetention: None            # RESEARCH-0001/E-05
        timeout: 5000
        onFailure:
          - type: retry
            retryLimit: 10
            retryAfter: 0.5
```

Neden bu tasarım doğru:

1. **Standart.** API tarafını herhangi bir Arazzo aracı okuyabilir; kilitlenme yok.
2. **Token ucuz.** Model Arazzo'yu zaten biliyor; "bizim DSL şöyle çalışır" diye 2.000 token
   harcamıyoruz. Yalnız `x-checknexus-db` bloğunu anlatmak yetiyor.
3. **Sınırlı bekleme bedava geliyor.** Asenkron kanıt için `retry` + `retryLimit` + `retryAfter` +
   `timeout` zaten spec'te; kendi polling semantiğimizi icat etmiyoruz.
4. **Asenkron/event adımları bedava geliyor.** `action: receive` + `correlationId` + `dependsOn`
   AsyncAPI kaynaklarıyla birlikte çalışıyor.
5. **Assertion dili bedava geliyor.** `simple` + `regex` + **RFC 9535 JSONPath** + `xpath`.

### 5.3 Oracle yığını — model asla hakem değildir

**Kanıt (K2/K3).** LLM tabanlı oracle'lar kırılgan: küçük prompt değişimi veya model güncellemesi kararı
çevirebiliyor, gerekçe kara kutu, bilgi eskiyor; pasif LLM hakemlerin örtük oracle'lara göre iyileşme
sağladığı ama düşük precision ve yüksek yanlış-pozitif oranıyla sınırlı kaldığı raporlanıyor. Önerilen
disiplin: **üreteci değerlendiriciden ayır ve değerlendiriciyi güvenilen bir oracle değil, kontrollü bir
ölçüm aleti olarak ele al.**

Bizim oracle katmanlarımız (arşiv §12.2'nin somutlaştırılmışı) — hepsi deterministik:

| Katman | Ne doğrular | Kim |
|---|---|---|
| Transport | HTTP durum, header, content-type | Runner |
| Contract | JSON Schema / OpenAPI uyumu | api-contract contracts |
| Domain | `successCriteria` (jsonpath/simple) | Runner |
| **Persistence** | **hedefli DB assertion'ı** | **db-checker** |
| Async | correlation + deadline + kardinalite | Runner (Arazzo `receive`) |
| Security | izin, tenant, negatif yollar | Runner + host |

Modelin rolü yalnızca **hangi assertion'ın yazılacağını önermek**tir. Yazıldıktan sonra o assertion
insan onayından geçer ve deterministik olarak koşar.

### 5.4 "Healer" ama gözü kapalı değil

**Kanıt (K3, ama önemli).** Denetimsiz self-healing gerçek hataları saklayabiliyor: gerçek
"Submit order" butonu silinip yerine "Submit feedback" kalırsa naif bir healer metin benzerliğinden onu
seçer, test yeşile döner. En pahalı tarafı: **sessiz onarım raporda görünmez** — geliştirici yeşil görür,
PR birleşir, hata üretime çıkar.

Bu yüzden bizde healer'ın üç kuralı olmalı:

1. **Gerekçesiz onarım yok.** Her yama, bir checker bulgusuna (fingerprint) bağlanmak zorundadır.
   *"POST /orders yanıt şeması değişti (fp: a1b2…), bu yüzden 3. adımın successCriteria'sı güncellendi."*
2. **Onarım bir bulgudur.** Yama otomatik uygulanmaz; `PendingApproval` durumunda bir kayıt olur.
3. **Sessiz geçiş yasak.** Onarılmış bir senaryonun ilk yeşil koşusu raporda `Healed` etiketi taşır.

---

## 6. MCP yüzeyi — an bazında tool kataloğu

RESEARCH-0001 §6.4'teki 9 tool'u, senaryo testi ihtiyacına göre **ana göre ayrılmış** hale getiriyorum.
Toplam yine 12'yi geçmiyor; ve kritik olan: **her an için ayrı bir ajan profili** var, hepsi aynı anda
bağlamda durmuyor.

### 6.1 Yazım ajanı (moment A)

| Tool | `readOnly` | Döndürdüğü | Token disiplini |
|---|---|---|---|
| `contract.operation.find` | ✔ | Operasyon **özeti**: method, path, zorunlu parametreler, yanıt şeması özeti | Tam OpenAPI gövdesi **asla** dönmez; `resource_link` verilir |
| `db.table.describe` | ✔ | Tablo **şekli**: kolonlar (ad+tip+nullable), PK, unique key'ler, FK komşuları | Tam snapshot dönmez; tek tablo + 1 seviye komşu |
| `db.binding.suggest` | ✔ | Operasyon ↔ tablo eşleme **önerisi** (isim benzerliği + FK grafiği + PK tipi) | Öneri; karar insanın |
| `scenario.validate` | ✔ | Arazzo dokümanını şema + referans bütünlüğü açısından doğrular | Hata listesi, gövde değil |
| `scenario.dryRun` | ✘ | Senaryoyu **tek sefer** koşar, adım adım sonuç döner | Playwright generator'ın "canlıda doğrula" adımının karşılığı |
| `scenario.save` | ✘ | Arazzo dokümanını sürümleyerek kaydeder | Handle döner |

### 6.2 Teşhis ajanı (moment C)

| Tool | `readOnly` | Döndürdüğü |
|---|---|---|
| `run.get` | ✔ | Handle ile durum + adım özeti (yeşil/kırmızı listesi) |
| `run.step.evidence` | ✔ | **Yalnız patlayan adımın** kanıtı: beklenen vs gerçek, redaction'lı |
| `db.assert.explain` | ✔ | DB adımı neden patladı: satır yok mu, kolon farklı mı, zaman aşımı mı |

### 6.3 Bakım ajanı (moment D)

| Tool | `readOnly` | Döndürdüğü |
|---|---|---|
| `change.since` | ✔ | İki checker'ın son koşusundan **New** bulgular (severity filtreli) |
| `scenario.impacted` | ✔ | Bir bulgu fingerprint'inden etkilenen senaryo/adım listesi |
| `scenario.patch.propose` | ✘ | Gerekçeli yama önerisi (`PendingApproval`) |

### 6.4 MCP protokol disiplini (2026-07-28)

- **Deterministik tool sırası + `ttlMs`** → prompt cache isabet oranı (K2).
- **Her tool'da `outputSchema`** → model JSON'u tahmin etmez.
- **Handle deseni** → spec'in "Stateful Tools" rehberi: oluşturma tool'u opak handle döndürür, sonraki
  çağrılar handle alır; yetki her çağrıda doğrulanır, ömür sınırlıdır.
- **Uzun koşular Tasks extension'ı** (`io.modelcontextprotocol/tasks`) ile: `tasks/get` + `tasks/update`,
  poll tabanlı. Bizim `Pending → Running → Completed/Failed` modelimiz birebir oturuyor (RESEARCH-0001 §6.6).
- **Ağır çıktı `resource_link`** → bulgu/kanıt gövdesi modelin bağlamına girmez.

---

## 7. Database Checker'ın senaryo testindeki rolü

Bu bölüm, kullanıcının isteği doğrultusunda RESEARCH-0001/0002'yi senaryo testi yönünde ilerletir.

### 7.1 İki farklı ihtiyaç, iki farklı yüzey

DB checker senaryo testine **iki** şey verir ve ikisi karıştırılmamalıdır:

| Yüzey | Ne zaman kullanılır | Çıktı boyutu | Öncelik |
|---|---|---|---|
| **Bilgi yüzeyi** (`db.table.describe`, `db.binding.suggest`) | Yalnız **yazım** anında | Orta (tek tablo) | M2 |
| **Assertion yüzeyi** (`AssertRow/Count/Absent`) | **Her koşumda**, runner tarafından | **~200 bayt** | **M1 — en yüksek öncelik** |

RESEARCH-0001/E-09'un neden en kritik madde olduğu burada netleşiyor: assertion yüzeyi olmadan senaryo
adımı ya tam karşılaştırma çağırır (50–500 KB ve saniyeler) ya da runner kendi SQL'ini yazar
(paket sınırı ihlali + enjeksiyon yüzeyi).

### 7.2 Assertion yüzeyinin senaryo testi için ek gereksinimleri

RESEARCH-0001/E-09'a **senaryo testi bakışıyla** eklenenler:

| # | Gereksinim | Gerekçe |
|---|---|---|
| S-01 | **Kardinalite assertion'ı**: `expectedCount: exactly 1 / atLeast 1 / none` | Arşiv §14.7 "cardinality"; "tam 1 satır oluştu" ile "en az 1 satır var" farklı testlerdir |
| S-02 | **Sınırlı polling sunucu tarafında** (`TimeoutMs` + `PollIntervalMs`, üst sınırlı) | Asenkron yazımda runner'ın döngü kurmasını engeller; tek çağrı = tek sonuç |
| S-03 | **`ObservedAtMs`** (kaç ms sonra gerçekleşti) | Performans regresyonu senaryo testinin yan ürünü olur; ayrıca flaky testi teşhis eder |
| S-04 | **Okuma tutarlılığı seçeneği**: `READ COMMITTED` / snapshot | Replica gecikmesi olan kurulumda "yok" sonucu yanlış olabilir |
| S-05 | **`ConnectionRef` mantıksal ad** | Senaryo dokümanı ortamdan bağımsız kalır; Arazzo'nun `sourceDescriptions` mantığının DB karşılığı |
| S-06 | **Matcher sözlüğü**: `equals, notEquals, isNull, isNotNull, greaterThan, matchesRegex, oneOf, withinTolerance` | Arazzo `successCriteria` ile aynı ifade gücü; tip-farkında kıyas (RESEARCH-0002 §6.1) burada da geçerli |
| S-07 | **Redaction varsayılan `None`** | RESEARCH-0001/E-05; ayrıca **prompt injection** savunması: modele ham hücre girmezse hücre üzerinden enjeksiyon olmaz |

### 7.3 Sınır kararı: checker **yazmaz**

Senaryo testi er ya da geç şunu isteyecek: *"testten önce şu satırı ekle, sonra temizle."*

**Öneri: bu yetenek DB checker'a eklenmemeli.** Gerekçeler:

1. RULE-0004 / ADR-0002: checker bilgi motorudur.
2. Güvenlik: hedef DB kimliğinin **salt-okunur** olması, RESEARCH-0001 §6.5'teki "lethal trifecta"nın
   ayağını kesen ana önlemdir. Yazma yetkisi verilirse o savunma çöker.
3. Test verisi izolasyonu zaten runner'ın işidir ve olgun desenleri var: **transaction rollback**
   (test başına ~2–4 ms, truncation'a göre %50–75 daha ucuz; bir vakada 447 test 245,87 sn → 2,84 sn),
   Respawn benzeri temizleyiciler, Testcontainers ile tek kullanımlık DB (K2/K3).

Yani: `ITestDataSandbox` portu **Test Module'de** yaşar, ayrı ve açıkça yetkilendirilmiş bir bağlantıyla
çalışır; DB checker o bağlantıyı hiç görmez.

### 7.4 Motor genişliğinin senaryo testine katkısı (RESEARCH-0002 bağlantısı)

| RESEARCH-0002 maddesi | Senaryo testine katkısı |
|---|---|
| M-01 kanonik tip haritası | `withinTolerance`, `equals` gibi matcher'ların tip-doğru çalışması |
| M-02 kısıt güvenilirliği | "FK var" diyen bir senaryo, `NOT VALID` FK'da yanlış güven verir |
| M-05 kolon derinliği (generated/computed) | Hesaplanan kolonu assertion'da beklemek → yanlış test |
| M-07 fingerprint + şiddet | **Moment D'nin tamamı buna dayanıyor** |
| M-08 lint | Senaryo yazımında "bu tablonun PK'sı yok, anahtarla assertion yazamazsın" uyarısı |
| M-12 bağımlılık grafiği | `db.binding.suggest`'in FK komşuluğu üzerinden öneri üretmesi |

---

## 8. Riskler ve karşı önlemler

| Risk | Kanıt | Önlem |
|---|---|---|
| **Prompt injection** — test edilen sistemin yanıtı veya DB satırı modele talimat taşır | Supabase MCP olayı: destek talebine gömülü talimat ile tüm DB sızdırıldı; "lethal trifecta" | Koşum anında model yok (B anı sıfır token); teşhiste ham gövde yerine redaction'lı özet; `valueRetention: None` varsayılan |
| **Tool trap** — tool sayısı ve çıktı boyutu doğruluğu düşürür | Jentic; tool sayısı benchmark'ları | An bazında ajan profili; toplam ≤ 12 tool; `resource_link` |
| **Sessiz self-heal** — test yanlış nedenle yeşile döner | §5.4 kanıtı | Gerekçesiz yama yok; `Healed` etiketi; insan onayı |
| **LLM-as-judge** — hakem kararı kırılgan | §5.3 kanıtı | Oracle deterministik; model yalnız öneri üretir |
| **Flaky testler** — asenkron adımlarda | — | `S-03 ObservedAtMs` ile p95 takibi; `retryLimit` açıkça yazılır, sessiz sonsuz bekleme yok |
| **Maliyet kaçağı** — ajan döngüye girer | Playwright ~114K/test ölçümü | Ajan başına `maxTurns` + token bütçesi; A anı dışında model çağrısı yok |
| **Standart kayması** — Arazzo veya MCP değişir | MCP 2025-06-18 → 2026-07-28 iki revizyon | Sürüm sabitlenir; `Source-Registry`'ye erişim tarihiyle yazılır; adapter katmanı ince tutulur |

---

## 9. Uygulama sırası

### Faz 1 — Tek dikey dilim (model olmadan çalışan iskelet)

Arşiv §12.1'in önerdiği ilk dilimin aynısı, ama artık formatı belli:

1. `AssertRowAsync` / `AssertCountAsync` / `AssertAbsentAsync` (RESEARCH-0001/E-09 + §7.2 S-01..S-07).
2. Arazzo 1.1.0 okuyucu + `x-checknexus-db` uzantısı.
3. Scenario Runner: HTTP adımı + DB adımı + `successCriteria` + `retry` + `timeout`. **Model yok.**
4. Sonuç formatı: CTRF JSON (test sonuçları için diller/araçlar arası JSON şema standardı) veya JUnit XML.

**Kabul ölçütü:** Elle yazılmış bir Arazzo dokümanı, uçtan uca yeşil koşuyor ve tek satır model çağrısı yok.

### Faz 2 — Yazım ajanı (moment A)

5. `CheckNexus.Assurance.Mcp` — yalnız §6.1 tool'ları, contracts-only bağımlılık.
6. `scenario.dryRun` ile "canlıda doğrula" (Playwright generator deseni).
7. İnsan onay akışı: plan → onay → kaydet.

**Kabul ölçütü:** Ajan bir OpenAPI operasyonundan başlayıp DB assertion'lı bir senaryo üretiyor;
üretilen doküman Faz 1 runner'ında değişmeden koşuyor.

### Faz 3 — Teşhis (moment C)

8. `run.step.evidence` + `db.assert.explain`; kanıt redaction'lı ve sınırlı.

### Faz 4 — Bakım (moment D) — **asıl fark burada**

9. Checker bulgularından `change.since` + `scenario.impacted` (fingerprint eşleşmesi — E-02 şart).
10. `scenario.patch.propose` + `Healed` etiketi + onay kaydı.

**Kabul ölçütü:** Bir API alanı opsiyonel yapıldığında; sistem etkilenen 3 senaryoyu bulur, yamayı gerekçesiyle
önerir ve **ajanın bağlamına giren toplam veri 2.000 token'ın altında kalır.**

---

## 10. Kaynaklar (bu belgeye özel)

| Kaynak | Neyi kanıtlıyor | Sınıf |
|---|---|---|
| https://spec.openapis.org/arazzo/latest.html | Arazzo 1.1.0 tam yapısı: successCriteria tipleri, runtime expression, retry, dependsOn, correlationId, components | K2 |
| https://playwright.dev/docs/test-agents | planner/generator/healer; artefaktlar; ajan = talimat + MCP tool demeti | K2 |
| https://jentic.com/blog/the-mcp-tool-trap | Tool sayısıyla bozulan doğruluk/muhakeme; knowledge layer; "MCP is the USB-C port, not the hard drive" | K2 |
| https://docs.jentic.com/ + OAK.md | OpenAPI+Arazzo = deklaratif bilgi katmanı, MCP = erişim biçimi | K2 |
| https://docs.testkube.io/articles/mcp-overview | 30 tool'luk katalog; `query_*` JSONPath filtresi; `list_artifacts`/`read_artifact` ayrımı | K2 |
| https://arxiv.org/pdf/2512.08706 (RESTifAI, ICSE 2026) | "Test üretimini test koşumundan ayır"; yeniden kullanılabilir spesifikasyon | K2 |
| https://arxiv.org/pdf/2501.08600 (AutoRestTest) | Çok ajanlı MARL + semantik bağımlılık grafiği; kara-kutu araçlarını geçmesi | K2 |
| https://docs.specmatic.io/contract_driven_development | Spec'ten çalıştırılabilir sözleşme; stub; geriye dönük uyumluluk yöntemi | K2 |
| https://www.anthropic.com/engineering/code-execution-with-mcp | 150K → 2K token; ara sonuçların ortamda kalması | K2 |
| https://platform.claude.com/docs/en/agents-and-tools/agent-skills/overview | Progressive disclosure üç aşaması | K2 |
| https://modelcontextprotocol.io/specification/2026-07-28/server/tools | `ttlMs`/`cacheScope`, deterministik sıra, outputSchema, Stateful Tools handle rehberi | K2 |
| https://dl.acm.org/doi/10.1145/3715107 (Test Oracle Automation in the Era of LLMs) | LLM oracle'ların sınırları | K2 |
| https://arxiv.org/html/2607.06195 (LogicHunter) | Pasif LLM hakemlerin düşük precision / yüksek FP oranı | K2 |
| https://ctrf.io / github.com/ctrf-io | Test sonucu için diller arası JSON şeması | K2 |
| https://lostechies.com/jimmybogard/2013/06/18/strategies-for-isolating-the-database-in-tests/ | Transaction rollback izolasyon deseni | K3 |
| Self-healing eleştirisi (testomat.io, qaskills.sh, getautonoma.com) | Sessiz onarımın gerçek hatayı gizlemesi | K3 |
| Playwright MCP token ölçümleri | ~114K vs ~27K | K3 |
