---
id: RESEARCH-0005
type: research
status: draft
title: API Contract Checker oracle yuzeyi, dinamik teshis motoru ve MCP butce/dogruluk kapisi
updated: 2026-08-12
decision_refs:
  - ADR-0002
  - ADR-0005
  - ADR-0007
  - ADR-0008
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0003
  - RULE-0004
---

# API Contract Checker — oracle yüzeyi, dinamik teşhis motoru ve MCP bütçe/doğruluk kapısı

> Kanonik değildir. [[90-Inbox/RESEARCH-0003-MCP-Senaryo-Testi-Mimarisi|RESEARCH-0003]] (tester mimarisi)
> ve [[90-Inbox/RESEARCH-0004-Hata-Teshis-Motoru|RESEARCH-0004]] (DB teşhis motoru) belgelerinin
> **API tarafındaki karşılığıdır.** [[03-Decisions/ADR-0007-Checker-Oracle-Surface|ADR-0007]] Database
> Checker için iki yüzey açtı; bu belge aynı soruyu API Contract Checker için sorar ve cevaplar.
>
> **Kapsam dışı:** spec karşılaştırma motorunun genişletilmesi, kaynak/format taraması, motor kıyaslaması.
> Bu belge yalnız **Test Module'ün koşum, teşhis ve bakım anlarında api-contract'tan isteyeceği yüzeyleri**
> ve **MCP'nin token maliyeti ile işin doğruluğunu ölçen yapıyı** tasarlar.
>
> Kanıt sınıfları RESEARCH-0001 §0 ile aynı: **K1** bu depodaki çalışan kod, **K2** resmî spesifikasyon /
> dokümantasyon / hakemli yayın, **K3** pratisyen ölçümü.

---

## 0. Tez — tek paragrafta

Test Module bir API adımı koştuğunda üç soru sorar ve üçü de farklı yüzey ister:
*"aldığım yanıt sözleşmeye uyuyor mu"* (**oracle**), *"uymadıysa neden"* (**teşhis**),
*"sözleşme değiştiğinde hangi senaryolar bozuldu"* (**bakım**). Bugün api-contract checker bu üçünden
**hiçbirini** karşılamıyor: elindeki tek yüzey iki snapshot'ı kıyaslayan karşılaştırma motoru, ki o da
Test Module'ün koşum döngüsüne göre çok büyük ve çok yavaş. RESEARCH-0004'ün DB tarafında kanıtladığı
şey burada birebir geçerlidir — **teşhis bir arama tablosu değil, bir aramadır** — ve API tarafında bu
aramanın üç canlı bilgi kaynağı zaten elimizdedir: HTTP'nin kendi **zorunlu yapılandırılmış alanları**,
saklanan **spec snapshot'ı** ve karşılaştırma motorunun ürettiği **bulgular**. Üçüncü soru — MCP'nin
maliyeti ve ürettiği işin doğruluğu — literatürde artık tahmin değil **ölçüm** konusudur: token maliyeti
`tools/list` çıktısı sayılarak, doğruluk ise mutasyon öldürme oranıyla ölçülür. Bu belge her üçü için de
ölçülebilir kapı önerir.

---

## 1. Bugünkü kod gerçeği (K1) — neyimiz var, neyimiz yok

Tasarımdan önce depoda gerçekten ne olduğunu saymak zorundayız; RESEARCH-0004'ün DB tarafında yaptığı
gibi. Aşağıdaki tablo `checkers/api-contract/src` üzerinde doğrudan okumayla çıkarıldı.

| Var olan | Nerede | Teşhis/oracle için değeri |
|---|---|---|
| **Ham spec metni kalıcı ve değişmez** | `Entities/Snapshots/SpecContent.cs` — `RawHash`, `CanonicalHash`, `Content`, `MediaType`, tenant içi dedup | **Kritik.** Tam JSON Schema kaybolmamış; şema çözümü için migration gerekmez |
| Yapısal snapshot modeli | `Models/Snapshots/SpecOperationModel`, `SpecParameterModel`, `SpecRequestBodyModel`, `SpecResponseModel`, `SpecSchemaModel`, `SpecSchemaPropertyModel`, `SpecHeaderModel`, `SpecSecuritySchemeModel`, `SpecSecurityRequirementModel` | Teşhisin **canlı kataloğu**. DB tarafındaki discovery repository'lerinin karşılığı |
| Parser sınırı | `Interface/Snapshots/ISpecDocumentReader` + `Microsoft.OpenApi 2.11.0` (+ `YamlReader`) | Domain'i parser'dan ayıran tek okuma sınırı; teşhis buraya yaslanır, ikinci parser yazılmaz |
| Bulgu modeli | `Models/Runs/Finding` (`KindCode`, `SeverityCode`, `DirectionCode`, `Address`, `OldValue`, `NewValue`) + `FindingAddress` (8 bileşenli adres) | Bakım anının (`moment D`) girdi tarafı |
| Kapalı kod katalogları | `DifferenceKindCodes` (16), `DifferenceSeverityCodes` (3), `DifferenceDirectionCodes` | MCP'ye borçlu olduğumuz "kararlı kod kümesi" (ADR-0008) |
| Dış HTTP sahibi | `EntityFrameworkCore/Adapters/Sources/SpecFetcherClient.cs` (`ISpecFetcherClient`), `SpecSourceReachabilityTester` | Probe'ların yerleşeceği **mevcut** adapter deseni; yeni katman gerekmez |
| Dayanıklılık altyapısı | `Microsoft.Extensions.Http.Resilience 10.8.0` paket referansı | Probe bütçesi (timeout/retry) sıfırdan yazılmaz |
| Farklılaştırma oracle'ı deseni | `.agents/skills/acc-comparison-engine/scripts/oasdiff_oracle.py` + `accepted-deviations.json` | Doğruluk kapısının ev içi precedent'i (§5.6) |

| Olmayan | Kanıt | Sonucu |
|---|---|---|
| **Yanıt uygunluk doğrulaması** | `src` altında yanıt gövdesini şemaya karşı doğrulayan tek tip yok | Senaryo adımının oracle'ı yok |
| **Bulgu fingerprint'i** | `grep -ril "fingerprint" src` → **0 sonuç** | `moment D` (bakım) kurulamaz; DB tarafındaki DBC-09'un karşılığı burada da borç |
| **Teşhis yüzeyi** | `Diagnosis` adında klasör/tip yok | "Neden patladı" sorusu cevapsız |
| **Değer saklama politikası** | `ValueRetentionMode` karşılığı yok; `Finding.OldValue/NewValue` ham metin | Müşteri verisi ve prompt-injection yüzeyi |
| **Şema derinliği** | `SpecSchemaPropertyModel` yalnız `Name/Type/Nullable/Required/EnumValues/ReferenceId` taşıyor; `format`, `maxLength`, `pattern`, `minimum`, `additionalProperties`, iç içe `properties` **yok**. `SpecResponseModel` yalnız `SchemaReferenceId` taşıyor, şemanın kendisini taşımıyor | Normalize model diff için yeterli, **doğrulama için yeterli değil** → §3.6 |
| `servers[]`, `info.version` | `ParsedSpecModel` yüzeyinde yok | Probe hedefi ve snapshot tazeliği hesaplanamaz |
| MCP bütçe ölçümü | test projelerinde token/bayt tavanı iddiası yok | ADR-0008'in "sınırlı çıktı" borcu **belgeli ama denetimsiz** |

> **Not:** Bu son satır önemli. ADR-0008 checker'ın MCP'ye borcunu üç maddede yazıyor
> (kararlı kodlar, sınırlı çıktı, sayfalama). Birincisi kodda var, ikincisi ve üçüncüsü
> **yalnız belgede** var. Belgelenmiş ama ölçülmeyen bütçe, bütçe değildir.

---

## 2. Küresel cephe — aynı işi kim, nasıl yapmış

### 2.1 Spec'i koşum anında oracle sayanlar

| Proje / şirket | Ne yapıyor | Bizim için ders | Sınıf |
|---|---|---|---|
| **Schemathesis** | OpenAPI/GraphQL şemasını **test oracle'ı** kabul eder; sabit bir check kataloğu koşar: `not_a_server_error`, `status_code_conformance`, `content_type_conformance`, `response_headers_conformance`, `response_schema_conformance`. Ayrıca state machine ile `create → read → delete` dizileri, başarısızlıkta **curl reprodüksiyonu** ve JUnit/Allure raporu | **Check kataloğu kapalı ve adlandırılmış olmalı.** Bizim `ConformanceRuleCodes`'umuz bu beşin üstüne kurulur; icat edilecek bir şey yok | K2 |
| **Stoplight Prism** (validation proxy) | İstemci ile gerçek sunucu arasına girer, **hem isteği hem yanıtı** OpenAPI'ye karşı doğrular; mevcut entegrasyon testini aynı anda spec-uygunluk testine çevirir | Uygunluk kontrolü **ayrı bir test türü değil**, mevcut adımın yanına takılan ikinci bir oracle'dır | K2 |
| **Kong OAS Validation plugin** | Aynı doğrulamayı gateway'de, JSON Schema Draft 2019-09 doğrulayıcısıyla yapar | Aynı yeteneğin üretimde de karşılığı var; sözleşme "sadece CI işi" değil | K2 |
| **Atlassian `swagger-request-validator`** | In-process doğrulayıcı; `LevelResolver` ile **mesaj anahtarı başına seviye** (`IGNORE`/`INFO`/`WARN`/`ERROR`), hazır `withAdditionalPropertiesIgnored()` kısayolu | **En önemli ders:** uygunluk ikili (geçti/kaldı) değil, **politika**dır. Bir `additionalProperties` ihlali senaryoyu kırmamalı, ama görünmez de olmamalı | K2 |

**Sonuç:** Dördü de aynı şeyi söylüyor — spec zaten makine-okunur bir sözleşmedir; koşum anında ona karşı
doğrulama yapmak yeni bir motor değil, **var olan belgenin ikinci kullanımıdır.**

### 2.2 Sözleşmeyi kapıya çevirenler (CI cephesi)

| Proje | Ne yapıyor | Ders | Sınıf |
|---|---|---|---|
| **oasdiff** | OpenAPI değişikliklerini **509 ayrı kural** olarak sınıflandırıyor: 213 breaking, 30 warning, 266 info. Kural kimliği grameri sabit: `request-`/`response-`/`endpoint-`/`api-` öneki + alan + değişim tipi | **Doğrulanmış hizalanma:** bizim `DifferenceKindCodes` sabitimizdeki `new-required-request-property` oasdiff kataloğundaki kuralla **birebir aynı dize**. Yani 16 kodumuz, 509 kuralın aynı gramerdeki alt kümesi. Genişletirken gramer icat etmeyeceğiz | K2 |
| **Specmatic** | Geriye dönük uyumluluğu şöyle ölçüyor: **yeni spec'ten mock ayağa kaldır, eski spec'ten test üret, testleri mock'a koştur**; CLI 0/1 döndürür, CI kapısı olur. Merkezî contract repo tek doğruluk kaynağıdır | Bu yöntemin **tersi** bizim mutasyon kapımızdır (§5.5/G3): spec'i bilerek boz, senaryonun **kırmızıya dönmesini şart koş** | K2 |
| **PactFlow bi-directional** | Consumer contract (pact) ⊂ provider contract (OAS) ilişkisini statik doğruluyor; `can-i-deploy` çağrıldığında çapraz doğrulama sonucu **o an** üretiliyor | Senaryo ↔ sözleşme ilişkisi de bir alt küme ilişkisidir: **senaryonun assertion'ı sözleşmeden türetilemiyorsa geçersizdir** (§5.5/G2) | K2 |

### 2.3 Gerçek trafiği oracle yapanlar

| Proje | Ne yapıyor | Ders | Sınıf |
|---|---|---|---|
| **Optic** | Yerel MITM proxy ile trafiği yakalar, spec'e karşı doğrular veya spec'i günceller; yakalanan trafiği "working copy" gibi ele alır, `oas status` ile "spec ne diyor / sistem ne yapıyor" farkını gösterir | Uygunluk ihlali **bir fark türüdür**; bizim `DifferenceKindCodes` mantığımızın çalışma zamanı karşılığı | K2 |
| **Diffy** (Twitter; Airbnb, Baidu, ByteDance kullanımı) | Aynı isteği primary / secondary / candidate'e gönderir; **primary↔secondary farkını primary↔candidate farkıyla kıyaslayarak gürültüyü iptal eder.** Alan bazında gürültü kuralları, "Safe to ship / Regressions detected" verdict'i | **Flaky alanı tahmin etme, ölç.** Zaman damgası/GUID gibi doğası gereği değişken alanları listeleyerek değil, **iki koşunun farkını ölçerek** bul (§5.5/G4) | K2/K3 |
| **Trace-based testing** (Tracetest + OTel) | Assertion'ı yalnız HTTP yanıtına değil, üretilen **span'lara** yazar: "yanlış veritabanı", "atlanan cache", "iki kez çağrılan downstream", "hiç yayınlanmayan event" — hepsi **doğru görünen bir yanıtla** birlikte olabilir | Yanıt yeşilken sistemin yanlış olabileceği kanıtı. Bizim karşılığımız: HTTP oracle'ı **tek başına yeterli değildir**, DB oracle'ı (ADR-0007) onun tamamlayıcısıdır | K2/K3 |

### 2.4 Akademik cephe — üretim, doğruluk ve bağımlılık

| Çalışma | Bulgu | Ders | Sınıf |
|---|---|---|---|
| **RESTler** (Microsoft Research) | İlk stateful REST fuzzer'ı; **spec'ten istek tipleri arasındaki bağımlılıkları çıkarır**, bir isteği ancak ön koşul kaynağı üretilebiliyorsa fuzz'lar, servis yanıtlarından dinamik öğrenir | Adım sırası bir **çıkarım** konusudur, sabit liste değil. `H-ST-03` (yanlış adım sırası) hipotezinin dayanağı | K2 |
| **RestTestGen** — Operation Dependency Graph | Operasyonları düğüm, "n1'in yanıt alanı n2'nin istek alanında geçiyor" ilişkisini kenar kabul eden yönlü graf | Senaryo adımları arası korelasyonun formel modeli; `scenario.impacted` ve adım sırası hipotezleri bunun üstünde çalışır | K2 |
| **EvoMaster** | Evrimsel algoritma + dinamik program analizi; bağımsız çalışmalarda (2022, 2024) en etkili sonuç | Kapsam artırma bizim işimiz değil; ama "oracle'ı üretimden ayır" tezini destekler | K2 |
| **RESTestBench** (2026) | LLM'in ürettiği REST API testlerini **Property-Based Mutation Testing** ile ölçtü: 3 servis, 106 doğrulanmış gereksinim, 228 elle tasarlanmış mutasyon. **Kesin gereksinimle mutasyon skoru %13–92; belirsiz gereksinimle %2–54.** Hiçbir model belirsiz gereksinimde %90'ı geçemedi. **Kritik uyarı: hatalı (mutasyona uğramış) implementasyona karşı refinement yapan testler, tek adımda üretilenlerin bile altına düştü** — model oracle'ı hataya uydurdu | **Bu belgenin en önemli tek kanıtı.** "Ajan dryRun'la yeşile boyayana kadar denesin" akışı, SUT hatalıysa **hatayı sözleşme sanır**. Bu yüzden G2 (sözleşmeden türetilebilirlik) kapısı zorunludur; ayrıca doğruluk **mutasyonla ölçülür**, yeşil koşuyla değil | K2 |
| **APITestGenie** | Gerçek 10 API'de geçerli test script'i oranı **%57**, üç denemeyle **%80**; sonraki sürümde **%89**. Yazarlar entegrasyondan önce elle doğrulama öneriyor | Üretim ≠ doğruluk. Makine kapısı olmadan her 5 senaryodan biri sessizce bozuk | K2 |

### 2.5 MCP cephesi — maliyet artık ölçülüyor

| Kaynak | Ölçüm | Sınıf |
|---|---|---|
| **REST→MCP ampirik çalışması** (116 resmî MCP sunucusu; 42'si OpenAPI'li; **298 tool ↔ 857 REST operasyonu**; 80 spec / 2.190 operasyonla üretim denemesi) | Sunucular mevcut operasyonların **medyan %19'unu** açıyor; tool'ların **%92'si çıplak API sarmalayıcısı**; otomatik üretim %76 başarılı, spec onarımıyla %94,2. Öneri: seçici açma, Collection/Item birleştirme, sunucu başına 50–100 tool tavanı | K2 |
| **MCP token benchmark'ı** (9 sunucu; yöntem: `initialize` + `tools/list` → her tool tanımını JSON'a çevir → `o200k_base` ile say) | Notion 24 tool = **17.161 token**; Firecrawl 26 tool = 16.565; GitHub 26 tool = 3.546; Slack 8 tool = **679**. En iyi/en kötü arası **25×**. Gerçekçi 5 sunucu = **26.224 token = 200K pencerenin %13,1'i**, ilk prompttan önce. En kötü sunucuda maliyetin **%97'si `inputSchema`**, açıklamalar değil. Yeniden tasarımla 17.161 → 773 (**%95,5**) | K3 |
| Bağımsız ölçüm | 72 tool = 33.709 token; **tool başına ~468 token** taban | K3 |
| **Thin vs Thick MCP** (30 konfigürasyon; HubSpot/NetSuite/QuickBooks; Haiku 4.5 + GPT-5-mini) | Göreve-göre daraltılmış "thin" tasarım token tüketimini **~%75** düşürdü, **ilk-cevap doğruluğunu düşürmeden** | K3 |
| **Anthropic Tool Search Tool** (`defer_loading`) | **%85** token azalması **ve doğrulukta artış**: Opus 4 %49 → %74, Opus 4.5 %79,5 → **%88,1** | K2 |
| **Anthropic code execution with MCP** | 150.000 → 2.000 token (%98,7); ara sonuçlar yürütme ortamında kalır | K2 |
| **Anthropic — agent tool'ları yazmak** | Tool yanıtları için sayfalama/filtreleme/kırpma ve makul varsayılanlar; Claude Code'da tool yanıtı varsayılan **25.000 token** ile sınırlı; tool açıklamaları **eval sonuçlarıyla** iyileştirilmeli; ölçülecek metrikler: doğruluk, süre, token tüketimi, tool hataları | K2 |
| **MCP 2026-07-28** | `tools/list` sonucu `ttlMs` + `cacheScope` taşır (SEP-2549, HTTP Cache-Control modeli); `inputSchema`/`outputSchema` tam **JSON Schema 2020-12**'ye yükseltildi (SEP-2106); uzun işler `io.modelcontextprotocol/tasks` uzantısında (`tasks/get`, `tasks/update`, kararlı task id + TTL + önerilen poll aralığı); çekirdek **stateless** | K2 |
| **`cocaxcode/api-testing-mcp`** — gerçek, yayımlanmış API testi MCP sunucusu | 42 tool (istek, assertion, flow, koleksiyon, environment, grup, spec, mock, yardımcılar). **Kopyalanacak iki desen:** `request` tool'unda `verbosity` — "normal" gürültülü header'ları eler ve gövdeyi **2.048 bayta** kırparak ~**%65**, "minimal" ~**%95** tasarruf sağlar; sıkıştırılmış yanıt bir **`call_id`** taşır ve tam gövde `inspect_last_response()` ile **isteği tekrar çalıştırmadan** geri alınır. **Kopyalanmayacak taraf:** 42 tool, tool trap eşiğinin çok üstünde | K2/K3 |

**Beş bağımsız ölçüm aynı yere çıkıyor:** tool sayısını ve şema hacmini düşürmek yalnız ucuzlatmıyor,
**doğruluğu da artırıyor.** ADR-0008'in "≤12 tool" kararı bu yüzden bir tasarım tercihi değil, ölçülmüş bir
performans kararıdır.

---

## 3. Yüzey 1 — Yanıt uygunluk oracle'ı (`ResponseConformance`)

### 3.1 Soru

Test Module'ün API adımındaki sorusu *"iki spec aynı mı"* değildir. Sorusu şudur:

> *"Az önce aldığım yanıt, senaryonun yazıldığı sözleşmeye uyuyor mu?"*

Bunu tam karşılaştırmayla cevaplamak ADR-0007'nin DB tarafında reddettiği hatanın aynısıdır: çıktı büyük,
süre uzun ve soru yanlış. Doğru cevap **hedefli, tek çağrılık, ~200 baytlık** bir uygunluk sonucudur.

### 3.2 Sözleşme

```text
AssertResponseAsync(request) -> result

request:  SnapshotId | SnapshotRef        (hangi sozlesme)
          OperationRef { OperationId | (Method, PathTemplate) }
          ObservedStatusCode
          ObservedContentType
          ObservedHeaders   (kapali beyaz liste: spec'in bildirdigi header adlari + RFC 9110 tanili header'lar)
          ObservedBody      (host sinirinda kalir; disari cikmaz)
          ProfileCode       (Strict | Runtime | Lenient)
          ValueRetention    (varsayilan None)

result:   OutcomeCode                     (kapali kume)
          Violations[ { RuleCode, Pointer, Level, ExpectedKeyword } ]   (azami N)
          ObservedAtMs
          SnapshotCanonicalHash
```

**Kapalı kod kümeleri (Domain.Shared sahipliği):**

| Küme | Değerler | Kaynak |
|---|---|---|
| `ConformanceOutcomeCodes` | `Passed`, `StatusCodeUndocumented`, `MediaTypeUndocumented`, `ResponseSchemaViolation`, `RequiredHeaderMissing`, `UndocumentedProperty`, `ServerError`, `OperationNotResolved`, `SnapshotNotFound`, `PolicySuppressed` | Schemathesis check ailesi + iki bizim |
| `ConformanceRuleCodes` | `not-a-server-error`, `status-code-conformance`, `content-type-conformance`, `response-headers-conformance`, `response-schema-conformance`, `additional-properties`, `security-requirement` | Schemathesis adlandırması korunur; model bu adları **zaten biliyor**, anlatmak için bağlam harcanmaz |
| `ConformanceLevelCodes` | `Ignore`, `Info`, `Warn`, `Fail` | `LevelResolver` deseni |
| `ConformanceProfileCodes` | `Strict`, `Runtime` (varsayılan), `Lenient` | DBC-17 karşılaştırma profillerinin API karşılığı |

**Profil neden zorunlu:** `additionalProperties` ihlali bir sözleşme sinyalidir ama bir senaryo adımını
kırmamalıdır (sunucu alan ekleyebilir; bu `NonBreaking`'dir). Profil olmadan iki kötü seçenekten birine
mahkûm oluruz: ya gürültüyle testleri kırmızıya boğarız, ya kuralı tamamen kapatırız. `LevelResolver`
tam olarak bu problemi çözmek için var (K2).

### 3.3 Pazarlıksız invariant — operasyon tek çözülmezse assertion **koşmaz**

ADR-0007'nin DB tarafındaki kritik invariant'ı şuydu: *anahtar kolonları PK/unique değilse assertion
çalışmaz ve `KeyNotUnique` döner.* API tarafındaki birebir karşılığı:

> **Gözlenen istek, snapshot içinde tek bir operasyona çözülemiyorsa assertion koşmaz ve
> `OperationNotResolved` döner.**

Çözülememe sebepleri gerçektir ve sessizce yanlış cevap üretirler: `operationId` yokluğu
(`SpecOperationModel.OperationId` nullable — K1), path şablonu belirsizliği (`/orders/{id}` ile
`/orders/active` aynı somut yola eşleşebilir), aynı path+method'un birden çok server prefix'i altında
bulunması. "O operasyon" garantisi olmadan uygunluk kararı verilmez.

### 3.4 Değer saklama ve enjeksiyon sınırı

Uygunluk sonucu **değer taşımaz**. İhlal şu üçlüyle adreslenir: `RuleCode` + **JSON Pointer** (RFC 6901,
JSON Schema'nın kendi hata adresleme biçimi) + ihlal edilen **şema anahtar sözcüğü** (`maxLength`,
`type`, `required`…). Beklenen/gerçek **değerler** varsayılan olarak (`ValueRetention: None`) rapora
girmez.

Bunun iki ayrı gerekçesi var ve ikisi de tek başına yeterli:

1. **Gizlilik.** Yanıt gövdesi müşteri verisidir; DB tarafında `ValueRetentionMode` ile çözülmüş problemin
   aynısı (RESEARCH-0001/E-05).
2. **Prompt injection.** Yanıt gövdesi, ajanın bağlamına giren **güvenilmez içeriktir**. "Lethal trifecta"
   analizinin açık uyarısı: enjeksiyon yüzeyi kullanıcı mesajıyla sınırlı değildir, **modele geri beslenen
   her tool çıktısıdır** (K2/K3). Modele ham gövde yerine "`$.items[3].price` alanı `type: number`
   bekliyordu" cümlesi giderse, gövdedeki hiçbir metin talimat taşıyamaz.

**Sınır notu (K1):** ADR-0002 gereği checker in-process bir pakettir; gözlenen gövde composition host
sınırının **dışına çıkmaz**. Dışarı çıkan tek şey ~200 baytlık sonuçtur. Bu, "gövdeyi bir servise
göndermek" endişesini mimari olarak ortadan kaldırır.

### 3.5 Bu yüzey neyi engelliyor

RESTestBench'in bulgusu şuydu: model, hatalı implementasyona karşı iyileştirme yaparken oracle'ı **hataya
uyduruyor** (K2). Uygunluk yüzeyi bunun panzehridir: senaryo yeşil görünse bile, yanıt sözleşmeyi ihlal
ediyorsa adım `ResponseSchemaViolation` üretir. Yani **domain oracle'ı ile sözleşme oracle'ı ayrı ayrı
konuşur** — Prism'in "entegrasyon doğruluğu ve spec uygunluğu aynı anda" tezinin bizdeki karşılığı (K2).

### 3.6 Borç — şema derinliği ve `ISpecSchemaResolver`

§1'de saptandı: normalize model diff için tasarlanmış, **doğrulama için yeterli değil**. Ama ham metin
`SpecContent.Content` içinde duruyor (K1), yani bu bir veri kaybı değil, bir **çözümleme eksiği**.

**Öneri:** yeni bir alan/migration değil, yeni bir okuma sınırı:

```csharp
// Domain/Interface/Snapshots/ISpecSchemaResolver.cs
public interface ISpecSchemaResolver
{
    // Operasyon + durum + medya tipi icin dogrulanabilir sema dugumunu cozer.
    Task<ResolvedSchema> ResolveResponseSchemaAsync(
        SpecContent content, SpecOperationModel operation,
        string statusCode, string mediaType, CancellationToken ct = default);
}
```

- `$ref` çözümü, `allOf` düzleştirmesi ve dialect uyarlaması burada yapılır; `Managers/Snapshots`
  altındaki normalizasyon davranışı **tekrarlanmaz**, yeniden kullanılır.
- Önbellek anahtarı `SpecContent.CanonicalHash` — zaten var (K1) ve içerik değişmezdir.
- **Dialect uyarısı:** OAS 3.1 JSON Schema 2020-12 ile hizalıdır; OAS 3.0 bir *dialect* kullanır ve
  `nullable`, `exclusiveMinimum` gibi noktalarda ayrışır. Doğrulayıcı seçimi ve dialect uyarlaması
  **ADR gerektirir**; kütüphane adayları (`JsonSchema.Net` + `JsonSchema.Net.OpenApi`,
  `Corvus.JsonSchema`, `NJsonSchema`) kurulu API'ye karşı doğrulanmadan sabitlenmemelidir.
  `Microsoft.OpenApi 2.11.0` bir okuyucu/modeldir, doğrulayıcı değildir (K1 + K2).

---

## 4. Yüzey 2 — Dinamik teşhis motoru (API tarafı)

### 4.1 "Dinamik" ne demek — burada da statik eşleme yasak

RESEARCH-0004'ün şartı aynen geçerlidir: `if (statusCode == 415) return "medya tipi hatası";` **yasaktır**.
Yasağın API tarafındaki gerekçesi daha da güçlüdür: aynı `400` on farklı sebepten dönebilir ve
`500` hiçbir şey söylemez. Durum kodu bir cevap değil, **soruşturmanın kapısıdır**.

Kural şudur — kimlik **olgudur**, kod değil:

```csharp
// Yanlis (yasak):  identity.StatusCode == 415
// Dogru:           identity.StatusClass == HttpStatusClassCodes.ClientError
//                  && context.Operation.RequestBodies.Count > 0
//                  && !context.Operation.RequestBodies
//                        .Any(body => body.MediaType == identity.SentContentType)
```

Böylece aynı kural, `415` döndüren API'de de, `400` + `problem+json` döndüren API'de de, `406` döndüren
API'de de tek sınıfla çalışır.

### 4.2 Üç canlı bilgi kaynağı — DB'dekinin birebir karşılığı

RESEARCH-0004'ün gücü, teşhisi kodda taşımayıp üç canlı kaynaktan türetmesiydi. API tarafında o üçlünün
karşılığı vardır ve **üçü de zaten elimizdedir**:

| # | DB tarafındaki kaynak | API tarafındaki karşılığı | Nereden |
|---|---|---|---|
| 1 | Motorun hata sözlüğü (`errcodes`, `sys.messages`) | **HTTP'nin kendi zorunlu yapılandırılmış alanları** (§4.3) | RFC 9110 / 9457 / 6750 — protokolün kendisi |
| 2 | Canlı katalog (discovery repository) | **Spec snapshot'ı** (`SpecOperationModel` ailesi) | `ISpecDocumentReader` + `SpecContent` (K1) |
| 3 | Karşılaştırma bulguları + fingerprint | **`Finding` + `FindingAddress`** (+ eklenecek fingerprint) | `Models/Runs/Finding` (K1) |

### 4.3 Birinci kaynak — HTTP zaten yapılandırılmış hata alanı veriyor

Bu, bu belgenin en önemli mimari bulgusudur. PostgreSQL'in nesne adlarını ayrı alanlarda vermesinin
gerekçesi *"uygulamalar bunları lokalize edilebilir mesaj metninden çıkarmak zorunda kalmasın"*dı (K2).
**HTTP aynı şeyi yapar ve çoğu ekip bunu kullanmaz:**

| Hata ailesi | Protokolün zorunlu/standart yapılandırılmış alanı | Kaynak | Sınıf |
|---|---|---|---|
| Kimlik doğrulama (401) | Sunucu **MUST** `WWW-Authenticate` üretir; en az bir challenge, scheme + parametreler | RFC 9110 §15.5.2 | K2 |
| Bearer token hataları | `error="invalid_token" \| "insufficient_scope" \| "invalid_request"`, `error_description`, `scope` | RFC 6750 §3.1 | K2 |
| Metot (405) | Sunucu **MUST** `Allow` üretir; kaynağın desteklediği metot listesi | RFC 9110 §15.5.6 | K2 |
| Throttling / kullanılamama (429, 503) | `Retry-After` | RFC 9110 | K2 |
| Ön koşul (412, 428) | `ETag` / `If-Match` semantiği | RFC 9110 §13 | K2 |
| Yönlendirme (3xx) | `Location` | RFC 9110 | K2 |
| **Her aile** | **RFC 9457 Problem Details**: `type`, `title`, `status`, `detail`, `instance` + **extension member'lar** (ör. `errors[]`) | RFC 9457 | K2 |
| ABP tabanlı SUT | `RemoteServiceErrorInfo`: `code` (`<namespace>:<code>`), `details`, `validationErrors[]` | ABP | K2 |
| Korelasyon | OTel: `error.type` ve `http.response.status_code` | OTel semconv | K2 |

Ve **sınıf davranışı** de standartla tanımlıdır: RFC 9110 §15 durum kodlarını `1xx…5xx` sınıflarına ayırır;
`4xx` isteği, `5xx` sunucuyu işaret eder. Bu, SQLSTATE'in *"ilk iki karakter sınıfı belirtir"* kuralının
tam karşılığıdır ve `AppliesTo` yordamlarının sabitleyebileceği **tek** şeydir.

**Kimlik güveni (`IdentityConfidence`) burada da zorunludur.** PostgreSQL dokümanı yapılandırılmış alan
kapsamının tam olarak yalnız sınıf 23'te bulunduğunu söylüyordu; API tarafındaki karşılığı şudur:
**her API RFC 9457 döndürmez.** Motor bunu güven olarak taşımalıdır:

| Gözlem | `IdentityConfidence` |
|---|---|
| RFC 9457 gövdesi + tanınan `type` URI | `High` |
| Standart tanılı header var (`WWW-Authenticate`, `Allow`, `Retry-After`) | `High` |
| ABP `RemoteServiceErrorInfo` + `code` | `High` |
| Yalnız durum kodu ve `content-type` | `Low` |
| Gövde var ama yapılandırılmamış (HTML hata sayfası, düz metin) | `Low` — **metin ayrıştırılmaz** |

> **Kural (RESEARCH-0004'ün `RawMessage` kuralının aynısı):** yapılandırılmamış hata gövdesi hiçbir
> kararın girdisi değildir. Lokalize olabilir, sürümle değişir, müşteri verisi ve **enjeksiyon** taşıyabilir.
> Yalnız kanıt olarak, redaction'dan geçirilerek ve **kırpılmış** olarak raporda görünür.

**Doğrulama kuralı (kritik):** çıkarılan hiçbir ad doğrulanmadan kullanılmaz. `WWW-Authenticate`'ten
çıkarılan scheme, snapshot'ın `SpecSecuritySchemeModel` listesinde aranır; `Allow`'dan çıkarılan metotlar
`SpecOperationModel.Method` kümesiyle karşılaştırılır; problem `type` URI'si tanınmıyorsa **ad atılır ve
güven düşer**. SQL Server tarafındaki "çıkarılan ad `sys.objects`'te aranır" kuralıyla birebir aynı disiplin.

### 4.4 Boru hattı — yedi adım, aynı iskelet

```text
[1] YAKALA      HttpFailureSignal      (yanit + gonderilen istegin metadatasi, gövde disinda)
       |
[2] KIMLIKLE    FailureIdentity        (status sinifi + yapilandirilmis alanlar; PARSE DEGIL EXTRACT)
       |
[3] YERELLESTIR ResolvedContext        (snapshot: operasyon, parametreler, medya tipleri,
       |                                yanit sozlesmeleri, security requirement + RelatedFindings)
[4] HIPOTEZ URET Hypothesis[]          (IDiagnosisRule — olgu yordamlari, switch YOK)
       |
[5] KANIT TOPLA  Evidence[]            (IDiagnosisProbe — YALNIZ GUVENLI METOT, butceli)
       |
[6] SIRALA       RankedHypothesis[]    (Confirmed / Likely / Possible / RuledOut)
       |
[7] ANLAT        DiagnosisReport       (RFC 9457 + checknexus uzantilari, <= 4 KB)
```

Bileşen sözleşmeleri DB tarafındakiyle **aynı şekle** sahiptir; bu bilinçlidir, çünkü composition host iki
checker'ın raporunu tek bir teşhis akışında birleştirecektir (§4.8):

```csharp
// Domain/Interface/Diagnosis/IDiagnosisRule.cs
public interface IDiagnosisRule
{
    string HypothesisKindCode { get; }
    int Priority { get; }
    bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context);
    List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context);
    HypothesisAssessment Assess(FailureIdentity identity, ResolvedFailureContext context,
                                List<ProbeEvidence> evidence);
}
```

### 4.5 Probe'lar — **yalnız güvenli metot**

DB tarafında pazarlıksız kural `READ ONLY` transaction'dı. API tarafındaki karşılığı protokolün kendi
tanımından gelir: RFC 9110 **safe** metotları (`GET`, `HEAD`, `OPTIONS`, `TRACE`) salt-okuma olarak tanımlar.

> **Sözleşme kuralı:** `IDiagnosisProbe` yalnız safe metot çağırabilir. `POST/PUT/PATCH/DELETE`
> **yeteneği arayüzde hiç yoktur.** Teşhis, test edilen sistemin durumunu değiştiremez.

| Probe | Ne yapar | Ağ? | Karşılığı (DB) |
|---|---|---|---|
| `SpecFact` | Snapshot'tan olgu okur (operasyon var mı, hangi metotlar, hangi medya tipleri, hangi security scheme) | Hayır | `CatalogFact` |
| `SchemaViolationLocation` | Uygunluk ihlalinin en derin başarısız anahtar sözcüğünü ve JSON Pointer'ını hesaplar | Hayır | — |
| `ContractDriftFact` | Adres/fingerprint ile ilgili `Finding` arar | Hayır | `RelatedFindings` |
| `OptionsAllow` | `OPTIONS <path>` → `Allow` başlığını spec metotlarıyla kıyaslar | Evet (safe) | — |
| `HeadResource` | Önceki adımın ürettiği kaynak URL'ine `HEAD` → kaynak gerçekten oluştu mu | Evet (safe) | **`RowExists`** |
| `ServerReachability` | Yapılandırılmış `servers[]` kökü veya health yoluna `GET` → servis mi kapalı, endpoint mi bozuk | Evet (safe) | — |
| `AuthMetadata` | Yapılandırılmış issuer/protected-resource metadata belgesini `GET` → beklenen scope/audience | Evet (safe) | `ObjectPrivileges` |
| `ResponseHeaderFact` | `Retry-After`, `RateLimit`, `ETag`, `Location` başlıklarını olguya çevirir | Hayır | — |
| `SnapshotFreshness` | Yanıtın sürüm başlığı / `info.version` ile snapshot'ın yaşını kıyaslar | Hayır | — |

**Pazarlıksız kurallar (DB tarafıyla aynı beş madde):**

1. **Yalnız safe metot.** Yazma yeteneği arayüzde yok.
2. **Serbest URL yok.** Probe hedefi **yalnız** snapshot'ın `servers[]` girdisinden veya yapılandırılmış
   `ConnectionRef`ten üretilir. Ajanın verdiği, yanıt gövdesinden çıkan veya `Location` başlığından gelen
   **keyfi URL çağrılmaz** — bu SSRF ve enjeksiyon yüzeyidir. (`Location` yalnız aynı origin'de ve
   spec'te tanımlı bir path şablonuna eşleşiyorsa kullanılır.)
3. **Bütçe.** `MaxProbeCount` + `MaxProbeDurationMs` + probe başına timeout.
   **Teşhis ikinci bir kesinti olamaz.** `Microsoft.Extensions.Http.Resilience` zaten referansta (K1).
4. **Redaction.** Probe sonucundaki değerler saklama politikasına tabidir; varsayılan `None`.
5. **İdempotent.** Probe durumu değiştirmez, sıcaklık ölçer.

### 4.6 Hipotez kataloğu v1

Her hipotez **ayrı bir sınıftır** ve uygulanabilirliğini kendisi söyler; motor hipotezleri bilmez,
DI konteynerinden toplar.

**A. Sözleşme sapması** *(bu platformun asıl silahı — iki motor neyin değiştiğini zaten biliyor)*

| Kod | Hipotez | Kanıt |
|---|---|---|
| `H-CD-01` | Yanıt şeması senaryo yazıldığından beri değişti | `ContractDriftFact` (`response-*`, `Breaking`) |
| `H-CD-02` | İsteğe yeni zorunlu alan eklendi | `new-required-request-property` bulgusu + problem `errors[]` |
| `H-CD-03` | Enum değeri kaldırıldı | `request-parameter-enum-value-removed` + gönderilen değer |
| `H-CD-04` | Endpoint kaldırıldı veya taşındı | `endpoint-removed` + `OptionsAllow` |
| `H-CD-05` | Başarı durum kodu değişti (201 → 200) | `response-success-status-removed` |
| `H-CD-06` | Medya tipi kaldırıldı | `response-media-type-removed` |
| `H-CD-07` | Alan opsiyonel/nullable oldu → o alana yazılmış assertion artık kırılgan | `response-property-became-optional/nullable` |

**B. İstek şekli / doğrulama**

`H-RQ-01` zorunlu parametre/alan eksik · `H-RQ-02` tip/format uyuşmazlığı (`format: uuid/date-time`) ·
`H-RQ-03` gönderilen `Content-Type` operasyonun bildirdiği medya tipleri arasında değil ·
`H-RQ-04` parametre yanlış konumda (query ↔ path ↔ header) · `H-RQ-05` değer kısıt ihlali
(`maxLength`/`pattern`/`enum`) · `H-RQ-06` gövde kabul etmeyen operasyona gövde gönderildi.

**C. Kaynak / durum / sıra**

`H-ST-01` referans verilen kaynak hiç oluşmadı (**`HeadResource` → false**) · `H-ST-02` oluştu ama **geç**
(bütçe içinde tekrar; `ObservedAtMs`) · `H-ST-03` adım sırası yanlış (bağımlılık grafiği — RestTestGen ODG) ·
`H-ST-04` kaynak başka tenant/scope'ta · `H-ST-05` önceki temizlik adımı sildi · `H-ST-06` idempotency /
tekrar gönderim → `409`.

**D. Yetki**

`H-AU-01` kimlik hiç gönderilmedi (`WWW-Authenticate` challenge var) · `H-AU-02` token süresi dolmuş
(`error="invalid_token"`) · `H-AU-03` **yetersiz scope** (`error="insufficient_scope"` + `scope`
parametresi ↔ operasyonun `SecurityRequirements`'ı) · `H-AU-04` yanlış güvenlik şeması (spec `apiKey`
diyor, runner `Bearer` gönderdi) · `H-AU-05` audience/issuer uyuşmazlığı (`AuthMetadata`) ·
`H-AU-06` tenant bağlamı eksik (ABP `__tenant`).

**E. Yönlendirme / dağıtım / ortam**

`H-EN-01` path spec'te var, bu ortama **deploy edilmemiş** · `H-EN-02` metot burada desteklenmiyor
(`Allow` farkı) · `H-EN-03` base URL / reverse proxy prefix uyuşmazlığı · `H-EN-04` **snapshot ile
deploy edilen sürüm farklı** (`SnapshotFreshness`) · `H-EN-05` taşıma seviyesi hatası (DNS/TLS/bağlantı)
— HTTP durumu **yok**.

**F. İçerik pazarlığı**

`H-NG-01` `Accept` karşılanamıyor (406) · `H-NG-02` charset/encoding · `H-NG-03` gövde boş ama şema içerik
bekliyor (204 ↔ 200 karışıklığı).

**G. Throttling / erişilebilirlik / zamanlama**

`H-TH-01` hız sınırı (429 + `Retry-After`/`RateLimit`) · `H-TH-02` gateway timeout (504) ile istemci
timeout'unun ayrımı · `H-TH-03` upstream bağımlılık hatası (5xx + problem `type`) ·
`H-TH-04` ön koşul başarısız (412/428 + `ETag`).

**H. Assertion başarısızlığı** *(Test Module'ün en sık hatası)*

`H-AS-01` değer beklenenden farklı (JSON Pointer) · `H-AS-02` zorunlu alan yok ·
`H-AS-03` **assertion artık sözleşmede olmayan bir alana yazılmış** (→ `H-CD-*` ile eşleşir, yama önerilir) ·
`H-AS-04` **kararsız alan** literal olarak assert edilmiş (zaman damgası/id → flaky; Diffy gürültü dersi) ·
`H-AS-05` sırasız dizide sıra varsayımı · `H-AS-06` **assertion geçti ama yanıt şemayı ihlal ediyor**
(domain yeşil, sözleşme kırmızı — Prism/Schemathesis'in yakaladığı vaka).

### 4.7 Sıralama ve rapor

Güven merdiveni DB tarafıyla aynıdır: `Confirmed` → `Likely` → `Possible` → `RuledOut`; **`RuledOut`
gizlenmez** ve **tek kök neden dayatılmaz**.

Rapor RFC 9457'dir, `type` = `urn:checknexus:problem:api-contract-diagnosis`, ve **≤ 4 KB**'dır.
Bu tavanı uygulayan kırpma algoritması DB tarafında **zaten yazılmış ve çalışıyor**
(`DiagnosisReport.TrimToBudget()`: önce `nextChecks`, sonra kanıt, sonra düşük sıralı hipotez,
en son `detail` — K1). API tarafında aynı davranış tekrar tasarlanmaz, aynı şekil kullanılır.

### 4.8 İki checker'ın birleşimi — `SuggestedCheck`

Teşhisin en değerli anı, cevabın **öteki checker'da** olduğu andır:

```text
API teshisi:  H-ST-01 Confirmed — POST /orders 201 dondu ama HEAD /orders/{id} 404.
                                   "Kaynak gorunmuyor."
                          |
                          v
              SuggestedCheck { capability: "database", operation: "assert.row",
                               arguments: { table: "sales.Orders", key: { Id: "<orderId>" } } }
                          |
                          v
DB teshisi:   H-FK-01 Confirmed — FK_Orders_Customers'in isaret ettigi musteri satiri yok.
```

**Sınır:** `nextChecks` düz metin değil, **tipli öneri** olur; ama api-contract paketi db-checker'a
bağımlılık **almaz**. `SuggestedCheck` yalnız `CapabilityCode` + `OperationCode` + parametre çantasıdır ve
Domain.Shared'da yaşar; çözümü composition host yapar. Bu, ADR-0002 (paket sınırı) ve ADR-0008 (MCP
composition host'ta) kararlarını bozmadan iki motoru birleştirir.

### 4.9 Uçtan uca örnek

```text
Senaryo adimi 3:  POST /orders  ->  422, content-type: application/problem+json

[1] YAKALA        HttpFailureSignal: status=422, contentType=problem+json,
                  gonderilen contentType=application/json, operationRef=createOrder
[2] KIMLIKLE      RFC 9457 govdesi: type=".../validation-error", errors=[{pointer:"/customerId",
                  code:"required"}]  ->  IdentityConfidence = High
                  StatusClass = ClientError
[3] YERELLESTIR   Snapshot: createOrder requestBody sema = OrderCreateRequest
                  RelatedFindings: "new-required-request-property @ OrderCreateRequest.customerId
                                    (Breaking, fp: 7c1e..)"     *** 
[4] HIPOTEZ       H-CD-02 (yeni zorunlu alan) · H-RQ-01 (zorunlu alan eksik) ·
                  H-RQ-02 (tip) · H-AU-06 (tenant) · H-EN-04 (snapshot eski)
[5] KANIT         SpecFact(OrderCreateRequest.customerId.Required)      -> true (yeni snapshot'ta)
                  ContractDriftFact(address = OrderCreateRequest.customerId) -> fp 7c1e, Breaking
                  SnapshotFreshness()                                    -> snapshot deploy ile ayni
                  SpecFact(securityRequirements)                         -> karsilanmis
[6] SIRALA        H-CD-02  Confirmed  (bulgu + spec olgusu)
                  H-RQ-01  Confirmed  (H-CD-02'nin sonucu)
                  H-EN-04  RuledOut
                  H-AU-06  RuledOut
                  H-RQ-02  Possible   (ayirt edici kanit yok)
[7] ANLAT         "createOrder istek govdesine 'customerId' alani zorunlu olarak eklendi
                   (Breaking, fp 7c1e). Senaryonun 3. adimi bu alani gondermiyor.
                   Etkilenen diger senaryolar: scenario.impacted(7c1e)."
```

Dikkat: motor **tek bir ağ çağrısı yapmadan**, yalnız yapılandırılmış hata alanları + snapshot + bulgu ile
kesin cevaba ulaştı. Probe bütçesi harcanmadı. Bu, teşhisin ucuz olmasının asıl sebebidir.

---

## 5. Yüzey 3 — MCP token bütçesi ve doğruluk kapısı

Kullanıcının asıl talebi burada: *"MCP'nin token maliyetini ve işin doğruluğunu kontrol ettirecek yapı."*
Bu ikisi ayrı kapılardır ve ayrı ölçülür.

### 5.1 Dört an artık ölçülen bir şeydir

RESEARCH-0003 §4 dört anı tanımladı (A yazım / B koşum / C teşhis / D bakım) ama bütçeler **hedef**
olarak yazıldı. Ölçüm olmadan hedef, temenni olarak kalır.

| An | Sıklık | Bütçe | **Nasıl ölçülür** |
|---|---|---|---|
| **A — Yazım** | Senaryo başına bir kez | ≤ 50K token | Oturum toplamı; `gen_ai.usage.*` |
| **B — Koşum** | Her CI / her gece | **0** | **İhlal testi:** runner sürecinde model çağrısı sayacı > 0 ise **build kırılır** |
| **C — Teşhis** | Yalnız kırmızı koşuda | ≤ 5K | Rapor bayt tavanı (4 KB) + tool çağrı sayısı |
| **D — Bakım** | Sözleşme değiştiğinde | ≤ 2K | Bulgu + etkilenen adım baytı |

`B = 0` iddiası bir mimari karardır ve **testle korunmalıdır**; belge ile değil.

### 5.2 Statik katalog bütçesi — CI kapısı

**Ölçüm yöntemi** (benchmark literatürünün yöntemiyle aynı, K3):
MCP yüzeyini ayağa kaldır → `tools/list` çağır → her tool tanımını (ad + açıklama + `inputSchema` +
`outputSchema`) JSON'a serileştir → tokenizer ile say.

**Kırmızı çizgiler (öneri):**

| Ölçüt | Eşik | Gerekçe |
|---|---|---|
| Toplam tool sayısı | **≤ 12** | ADR-0008; Tool Search ölçümü tool azaltmanın doğruluğu da artırdığını gösteriyor (Opus 4.5: %79,5 → %88,1) |
| Toplam katalog | **≤ 4.000 token** | Sektör tabanı tool başına ~468 token; 12 tool × 468 ≈ 5.600. 4.000 bilinçli disiplindir |
| Tek tool | **≤ 400 token** | En kötü sunucuda maliyetin %97'si `inputSchema`; tavan şemayı düz tutmaya zorlar |
| `inputSchema` derinliği | **≤ 2 seviye** | İç içe istek nesnesi yerine handle/ref; aynı ölçümün doğrudan sonucu |
| `outputSchema` | **her tool'da zorunlu** | MCP 2026-07-28 tam JSON Schema 2020-12 destekliyor; model çıktı biçimini tahmin etmez |

**Uygulama:** eşikler bir **baseline dosyasına** yazılır (`mcp-token-baseline.json`) ve test her koşuda
ölçülen değeri baseline ile kıyaslar. Bu desen evde zaten var: `accepted-deviations.json` (K1) ve
paket tarafındaki `PackageValidationBaselineVersion` disiplini. Artış **gerekçesiz geçemez**;
baseline değişikliği kod incelemesinde görünür.

> **Tokenizer dürüstlüğü:** .NET test sürecinde model tokenizer'ı yoksa PR kapısı bayt tabanlı
> proxy ile koşar ve bunu **açıkça** böyle raporlar; gerçek token sayımı gecelik bir işte resmî
> token sayma ucuyla doğrulanır. Yaklaşık ölçüm, ölçüm olmamasından iyidir; ama yaklaşık olduğu
> yazılmadan kullanılamaz.

### 5.3 Dinamik çıktı bütçesi

| Çıktı | Tavan | Mekanizma |
|---|---|---|
| Uygunluk sonucu | **512 bayt** | Sabit alan sayısı; ihlal listesi `MaxViolations` ile kırpılır |
| Teşhis raporu | **4 KB** | `TrimToBudget()` — DB tarafında yazılmış ve çalışan algoritma (K1) |
| Bulgu sayfası | **32 KB** | ADR-0008; sayfalama + severity/kind filtresi |
| Operasyon özeti | **2 KB** | Tam OpenAPI gövdesi **asla** dönmez; `resource_link` |
| Ham yanıt gövdesi | **hiç** | Yalnız `resultRef` handle ile, ayrı ve açıkça istenen çağrıda |

**Kopyalanacak desen (K2/K3):** `api-testing-mcp`'nin `verbosity` + `call_id` çifti. Varsayılan yanıt
sıkıştırılmıştır; tam gövde **isteği yeniden çalıştırmadan**, handle ile geri alınabilir. Ölçülen tasarruf
"normal"de ~%65, "minimal"de ~%95. Bizde karşılığı: `resultRef` + `verbosity` (`minimal` varsayılan) +
`MaxBodyBytes`.

MCP protokol tarafı: `tools/list` yanıtında `ttlMs` + `cacheScope` (prompt cache isabeti), deterministik
tool sırası, uzun koşular için Tasks uzantısı (`tasks/get` + `tasks/update`, TTL + önerilen poll aralığı).

### 5.4 Telemetri — maliyet raporlanabilir olmalı

- **Model tarafı:** OTel GenAI semconv — `gen_ai.operation.name`, `gen_ai.request.model`,
  `gen_ai.usage.input_tokens`, `gen_ai.usage.output_tokens` (1.37+ stabil; 1.41 reasoning token ekliyor).
  Sahibi composition host'tur.
- **Checker tarafı:** kendi span'larımız — `checknexus.api.conformance.assert`,
  `checknexus.api.diagnosis.run`, `checknexus.api.diagnosis.probe`; öznitelikler
  `checknexus.run.id`, `checknexus.moment` (`A|B|C|D`), `checknexus.response_bytes`, `error.type`.
  **Yasak:** gövde içeriği, URL query değerleri, token, secret path, müşteri kimliği.
- **Ürün çıktısı:** senaryo başına maliyet kartı — *"bu senaryo yazımda 34K token yaktı, bakımda 1,2K,
  koşumda 0."* Dört an modeli ancak bu kart varsa yönetilebilir.

### 5.5 Doğruluk kapısı — G1…G5

Kanıt nettir: LLM üretimi API testleri ilk denemede %57 geçerli, üç denemede %80–89 (K2); ve mutasyon
testiyle ölçüldüğünde belirsiz gereksinimde mutasyon skoru %2–54'te kalıyor (K2). **Yani "yeşil koştu"
bir doğruluk kanıtı değildir.** Beş kapı:

| Kapı | Ne doğrular | Model devrede mi | Kırmızıysa |
|---|---|---|---|
| **G1 — Şema** | Arazzo 1.1.0 şeması + `x-checknexus-*` uzantı şeması; her `operationId` snapshot'ta çözülüyor; her runtime expression bildirilmiş bir çıktıya bağlanıyor | Hayır | Senaryo kaydedilmez |
| **G2 — Sözleşmeden türetilebilirlik** | **Her `successCriteria` sözleşmeden türetilebilir olmalı.** Yanıt şemasında bulunmayan bir alana yazılmış assertion `AssertionNotInContract` ile reddedilir | Hayır | Senaryo kaydedilmez |
| **G3 — Mutasyon** | Snapshot'a **kendi `DifferenceKindCodes` kataloğumuzdan** mutasyon uygulanır (alanı opsiyonel yap, enum değeri kaldır, tipi değiştir, başarı durumunu kaldır), mutant spec'ten stub üretilir, senaryo ona koşturulur ve **kırmızıya dönmesi şart koşulur**. Skor = öldürülen mutasyon oranı | Hayır | Senaryo "zayıf" etiketlenir; eşik altındaysa onaya gitmez |
| **G4 — Kararlılık** | Aynı senaryo sabit stub'a karşı N kez koşar; adım bazında değişen alanlar `Volatile` işaretlenir ve literal assertion'ları uyarı üretir | Hayır | Adım flaky olarak raporlanır |
| **G5 — İnsan onayı** | Plan Markdown olarak incelenir; yamalar `PendingApproval`; onarılmış senaryonun ilk yeşili `Healed` etiketi taşır | Hayır | — |

**G2 neden en kritik kapı:** RESTestBench'in bulgusu, modelin hatalı implementasyona bakarak oracle'ı
hataya uydurmasıydı. G2 bunu **yapısal olarak** imkânsız kılar: sözleşmede olmayan bir beklenti
yazılamaz. Bu kapıyı kurabilmemizin tek sebebi sözleşme snapshot'ının bizde olmasıdır — piyasadaki
"dryRun'la yeşile boya" akışlarının sahip olmadığı şey budur.

**G3 neden bizde ucuz:** mutasyon kataloğu sıfırdan tasarlanmaz; `DifferenceKindCodes` (16 kod, K1)
zaten "bir sözleşme kaç farklı şekilde bozulabilir" sorusunun cevabıdır ve oasdiff'in 509 kurallık
kataloğuyla aynı gramerdedir. Yöntem Specmatic'in geriye dönük uyumluluk testinin **tersidir**:
Specmatic yeni spec'ten mock kurup eski testleri koşturur; biz **bozulmuş** spec'ten stub kurup
senaryonun kırılmasını bekleriz.

### 5.6 MCP yüzeyinin kendi regresyon kapısı

Yukarıdaki kapılar **senaryonun** doğruluğunu ölçer. **Tool'un** doğruluğunu ayrıca ölçmek gerekir;
Anthropic'in tool yazma rehberi bunu açıkça söylüyor: gerçek görevlerle eval kur, doğruluk/süre/token/hata
metriklerini topla, tool açıklamalarını eval sonuçlarına göre iyileştir (K2). τ-bench ve MCP-Bench'in
ortak yöntemi de aynıdır: transkript değil, **son durum** doğrulanır.

Bizdeki karşılığı, evde zaten olan desenin genişletilmesidir (`oasdiff_oracle.py` + `accepted-deviations.json`, K1):

- Her tool için N **golden vaka**: sabit girdi → sabit çıktı JSON'u, bayt bayt kıyaslanır.
- Aynı testte **boyut iddiası**: çıktı `MaxUtf8Bytes` tavanının altında mı.
- Bilinçli sapmalar `accepted-deviations` biçiminde, gerekçeli ve tarihli.

### 5.7 An bazında tool kataloğu — api-contract payı

ADR-0008 toplamı ≤ 12 tool'da tutuyor ve katalog **ürün başına** küratörleniyor. api-contract'ın payı:

| An | Tool | `readOnly` | Döndürdüğü | Bütçe |
|---|---|---|---|---|
| A — Yazım | `contract.operation.find` | ✔ | Operasyon özeti: method, path, zorunlu parametreler, yanıt şeması **özeti** | ≤ 2 KB; tam gövde `resource_link` |
| A — Yazım | `contract.schema.describe` | ✔ | Tek şemanın alanları (ad + tip + zorunlu + enum), 1 seviye | ≤ 2 KB |
| C — Teşhis | `contract.diagnose` | ✔ | RFC 9457 raporu | ≤ 4 KB |
| D — Bakım | `contract.change.since` | ✔ | Son koşudan `New` bulgular, severity filtreli | sayfalı, ≤ 32 KB |
| D — Bakım | `scenario.impacted` | ✔ | Fingerprint'ten etkilenen senaryo/adım listesi | ≤ 4 KB |

Koşum anında (`B`) **api-contract'ın MCP tool'u yoktur** — uygunluk oracle'ı runner tarafından doğrudan
HTTP/servis çağrısıyla kullanılır. Bu, `B = 0` iddiasının mimari karşılığıdır.

### 5.8 Kaçınılacak desen: OpenAPI → MCP otomatik tool üretimi

RESEARCH-0003 bunu zaten reddetmişti; artık ampirik kanıtı da var:

- Gerçek MCP sunucuları operasyonların **medyan %19'unu** açıyor; %92'si çıplak sarmalayıcı;
  çalışma seçici açmayı ve Collection/Item birleştirmeyi öneriyor (K2).
- Ham operasyonları tool'a çevirmek, ajanı **niyeti yeniden kurmaya** zorlar; semantik boşluk
  hallüsinasyonu artırır (K2).
- 200 endpoint = 200 tool = tek başına tool bütçesinin tamamı ve §5.2'nin her eşiğinin ihlali.

**Doğrusu:** bir `contract.operation.find` + talep üzerine bilgi veren katman; test edilen sistemin
kendisi tool'a **hiç** çevrilmez.

---

## 6. Riskler ve karşı önlemler

| Risk | Kanıt | Önlem |
|---|---|---|
| **Yanlış oracle** — model, hatalı SUT davranışını sözleşme sanar | RESTestBench: mutasyona uğramış implementasyona karşı refinement, tek adımlı üretimin altına düşüyor (K2) | **G2** sözleşmeden türetilebilirlik + **G3** mutasyon skoru |
| **Sessiz yanlış assertion** — operasyon yanlış eşleşir | `OperationId` nullable (K1); path şablonu belirsizliği | `OperationNotResolved`; eşleşme tek değilse assertion **koşmaz** |
| **Prompt injection** — yanıt gövdesi modele talimat taşır | Lethal trifecta: tool çıktısı da enjeksiyon yüzeyidir (K2/K3) | Koşumda model yok; sonuçta değer yok, yalnız pointer + kural kodu; ham gövde yalnız handle ile |
| **SSRF / probe kötüye kullanımı** | Probe hedefi gövdeden gelirse keyfi çağrı | Hedef yalnız `servers[]` veya yapılandırılmış `ConnectionRef`; **yalnız safe metot** |
| **Teşhis ikinci kesinti olur** | — | `MaxProbeCount`, `MaxProbeDurationMs`, probe timeout; olgu-probe'ları ağa çıkmaz |
| **Token kaçağı** | Tool tanımı maliyetinin %97'si `inputSchema` (K3) | §5.2 baseline kapısı; şema derinliği tavanı; `verbosity` + handle |
| **Tool trap** | Tool azaltmak doğruluğu artırıyor (K2) | ≤ 12 tool; an bazında profil; `B` anında tool yok |
| **Flaky yeşil** | Diffy'nin gürültü iptali problemi (K2/K3) | **G4** kararlılık kapısı; `Volatile` alan işaretleme; `ObservedAtMs` |
| **Sessiz self-heal** | RESEARCH-0003 §5.4 | Gerekçesiz yama yok; her yama bir bulgu fingerprint'ine bağlı; `Healed` etiketi |
| **Şema doğrulayıcı seçimi yanlış** | OAS 3.0 dialect ≠ JSON Schema 2020-12 (K2) | Kütüphane kararı **ADR ile**; kurulu API'ye karşı doğrulanmadan sabitlenmez |
| **Standart kayması** | MCP iki revizyon (2025-06-18 → 2026-07-28); Overlay 1.0 → 1.1 (K2) | Sürüm sabitlenir, `Source-Registry`'ye erişim tarihiyle yazılır, adapter ince tutulur |

---

## 7. Uygulama sırası

### Faz 1 — Oracle iskeleti (model yok)

1. `ISpecSchemaResolver` + doğrulayıcı kütüphane ADR'si (§3.6).
2. `ConformanceOutcomeCodes` / `ConformanceRuleCodes` / `ConformanceLevelCodes` / `ConformanceProfileCodes`
   (Domain.Shared) + değer saklama politikası.
3. `ResponseConformanceManager` + `IResponseConformanceAppService` + FluentValidation + controller.
4. `OperationNotResolved` invariant'ı ve test kapsamı.

**Kabul ölçütü:** elle hazırlanmış bir yanıt, snapshot'a karşı doğrulanıyor; sonuç **512 baytın altında**
ve içinde **hiçbir hücre değeri yok**.

### Faz 2 — Bulgu kalitesi (bakım anının ön şartı)

5. Bulgu fingerprint'i + `New`/`Known`/`Resolved` kovaları (DB tarafındaki DBC-09 ile **aynı formül şekli**).
6. `Finding` üzerinde değer saklama politikası.

**Kabul ölçütü:** aynı fark iki koşuda aynı fingerprint'i üretiyor; `scenario.impacted` bağlanabilir.

### Faz 3 — Teşhis motoru

7. `HttpFailureSignal` + `IFailureIdentityExtractor` (ProblemDetails / Challenge / Allow / Transport /
   Assertion) + katalog doğrulama kuralı.
8. `IDiagnosisRule` + `IDiagnosisProbe` + bütçe yöneticisi + güven merdiveni + RFC 9457 rapor (≤ 4 KB).
9. Hipotez kataloğu v1: A–H aileleri.

**Kabul ölçütü:** §4.9 örneği **sıfır ağ çağrısıyla** `Confirmed` üretiyor; `RuledOut` hipotezler raporda.

### Faz 4 — Bütçe ve doğruluk kapıları

10. Statik katalog bütçesi testi + `mcp-token-baseline.json`.
11. Çıktı bütçesi testleri (`TrimToBudget` şekli) + `verbosity`/`resultRef`.
12. G1/G2 kapıları; ardından G3 mutasyon kapısı ve G4 kararlılık kapısı.
13. Tool golden eval seti + boyut iddiaları.

**Kabul ölçütü:** bir API alanı zorunlu yapıldığında sistem etkilenen senaryoları buluyor, yamayı
gerekçesiyle öneriyor ve **ajanın bağlamına giren toplam veri 2.000 token'ın altında kalıyor**;
mutasyon kapısı bu senaryo için ≥ %80 öldürme oranı raporluyor.

---

## 8. Bilinçli olarak yapmayacaklarımız

| Öneri | Neden hayır |
|---|---|
| Karar yolunda LLM kullanmak | Oracle deterministik olmalı; LLM oracle'ları kırılgan (RESEARCH-0003 §5.3) ve hataya uyum sağlıyor (RESTestBench, K2). Model yalnız **öneri** üretir |
| Durum kodu → metin eşleme tablosu | Talebin kendisi bunu dışlıyor; ayrıca aynı `400` on farklı sebepten döner |
| Yapılandırılmamış hata gövdesini ayrıştırmak | Lokalize, sürümlü, enjeksiyon taşıyabilir. Yalnız kanıt olarak, kırpılmış ve redaction'lı |
| Checker'ın SUT'a yazması (test verisi seed/cleanup) | RULE-0004 + ADR-0002; safe-metot sınırı güvenlik modelinin taşıyıcısı. `ITestDataSandbox` Test Module'de |
| SUT'u MCP tool'una çevirmek | §5.8 kanıtı |
| Checker paketine MCP tipi koymak | ADR-0008 |
| Teşhis raporunu kalıcı tabloya yazmak | DB tarafındaki kararla aynı: hesaplanır ve döner; şema genişlemesi RULE-0002'ye takılır |
| Serbest JSON Schema'yı çağrandan almak | Serbest SQL yasağının karşılığı: şema **yalnız** saklanan snapshot'tan çözülür |
| Yanıt gövdesini raporda taşımak | Gizlilik + enjeksiyon; pointer + kural kodu yeterlidir |

---

## 9. PLAN-0002'ye giren maddeler

Bu belgenin gerekçesi burada, **yapılacak iş** [[90-Inbox/PLAN-0002-ApiContract-Ozellik-Listesi|PLAN-0002]]
belgesindedir (`ACC-01…ACC-18`).

---

## 10. Kaynaklar

| Kaynak | Neyi kanıtlıyor | Sınıf |
|---|---|---|
| https://schemathesis.io/ · https://github.com/schemathesis/schemathesis | Spec'i test oracle'ı sayan check kataloğu; stateful state machine; curl reprodüksiyonu; JUnit/Allure | K2 |
| https://stoplight.io/open-source/prism | Validation proxy: istek **ve** yanıt uygunluğu; entegrasyon testinin yanına takılan ikinci oracle | K2 |
| https://developer.konghq.com/plugins/oas-validation/ | Aynı doğrulamanın gateway karşılığı; Draft 2019-09 | K2 |
| https://javadoc.io/static/com.atlassian.oai/swagger-request-validator-core/2.27.2/index-all.html | `LevelResolver`: kural bazında IGNORE/INFO/WARN/ERROR; uygunluk = politika | K2 |
| https://www.oasdiff.com/docs/breaking-changes | 509 kural; 213 breaking / 30 warning / 266 info; kural kimliği grameri; `new-required-request-property` | K2 |
| https://docs.specmatic.io/contract_driven_development/backward_compatibility | Yeni spec'ten mock + eski spec'ten test = uyumluluk kapısı; CI exit code | K2 |
| https://docs.pactflow.io/docs/bi-directional-contract-testing/compatibility-checks/ | Consumer contract ⊂ provider contract; `can-i-deploy` anında çapraz doğrulama | K2 |
| https://www.useoptic.com/docs/verify-openapi | Yakalanan gerçek trafiğin spec'e karşı doğrulanması; `oas status` | K2 |
| https://github.com/twitter-archive/diffy · https://github.com/opendiffy/diffy | Primary/secondary/candidate üçlüsü ile **gürültü iptali**; alan bazında noise kuralları | K2/K3 |
| https://tracetest.io/ · https://opentelemetry.io/blog/2023/testing-otel-demo/ | Span üzerinde assertion; "doğru görünen yanıtın arkasındaki yanlış hop" | K2/K3 |
| https://github.com/microsoft/restler-fuzzer | Spec'ten istek bağımlılığı çıkarımı; stateful dizi | K2 |
| https://profs.scienze.univr.it/~ceccato/papers/2020/icst2020api.pdf (RestTestGen) | Operation Dependency Graph tanımı | K2 |
| https://github.com/webfuzzing/EvoMaster | Bağımsız çalışmalarda en etkili sonuç; üretim/koşum ayrımı | K2 |
| https://arxiv.org/html/2604.25862v1 (RESTestBench) | PBMT ile ölçüm; precise %13–92, vague %2–54; **hatalı SUT'a refinement daha kötü**; maliyet farkı | K2 |
| https://arxiv.org/abs/2409.03838 (APITestGenie) | Geçerli script %57 → %80 (3 deneme) → %89; elle doğrulama önerisi | K2 |
| https://arxiv.org/html/2507.16044v4 (REST→MCP ampirik çalışma) | 116 sunucu / 298 tool ↔ 857 operasyon; medyan %19 açılım; %92 çıplak sarmalayıcı; üretim %76 → %94,2 | K2 |
| https://github.com/zhang-liz/mcp-token-benchmark | `tools/list` → serialize → tokenizer yöntemi; 25× yayılım; 17.161 vs 679; %97 `inputSchema`; %95,5 azaltma | K3 |
| https://www.anthropic.com/engineering/advanced-tool-use | Tool Search / `defer_loading`: %85 token azalması **ve** doğruluk 79,5 → 88,1 | K2 |
| https://www.anthropic.com/engineering/writing-tools-for-agents | Token-verimli tool yanıtı; 25.000 token tavanı; eval ile tool açıklaması iyileştirme; ölçülecek metrikler | K2 |
| https://www.anthropic.com/engineering/code-execution-with-mcp | 150K → 2K (%98,7); ara sonuçlar bağlama girmez | K2 |
| https://blog.modelcontextprotocol.io/posts/2026-07-28/ | Stateless çekirdek; `ttlMs`/`cacheScope` (SEP-2549); JSON Schema 2020-12 input/outputSchema (SEP-2106) | K2 |
| https://modelcontextprotocol.io/extensions/tasks/overview · https://github.com/modelcontextprotocol/ext-tasks | Tasks uzantısı: task handle, `tasks/get`, `tasks/update`, TTL + önerilen poll aralığı | K2 |
| https://github.com/cocaxcode/api-testing-mcp | Gerçek API testi MCP sunucusu: `verbosity` (~%65 / ~%95), 2.048 baytlık gövde kırpma, `call_id` handle; **42 tool = tool trap** | K2/K3 |
| https://blog.christianposta.com/semantics-matter-exposing-openapi-as-mcp-tools/ | Ham operasyonu tool'a çevirmenin semantik boşluğu; yetenek odaklı tool tasarımı | K2 |
| https://www.speakeasy.com/mcp/tool-design/generate-mcp-tools-from-openapi/ | Otomatik üretim + küratörleme; scope ayrımı (read/write/destructive); zayıf dokümantasyon → hallüsinasyon | K2 |
| https://httpwg.org/specs/rfc9110.html | Durum kodu **sınıfları**; 401 → `WWW-Authenticate` MUST; 405 → `Allow` MUST; safe/idempotent metot tanımı | K2 |
| https://datatracker.ietf.org/doc/html/rfc6750 | Bearer hata kodları: `invalid_token`, `insufficient_scope` + `scope` parametresi | K2 |
| https://www.rfc-editor.org/rfc/rfc9457.html | Problem Details: `type/title/status/detail/instance` + extension member'lar | K2 |
| https://www.rfc-editor.org/rfc/rfc6901.html | JSON Pointer — ihlal adresinin standart biçimi | K2 |
| https://opentelemetry.io/blog/2026/genai-observability/ | GenAI semconv: `gen_ai.usage.input_tokens`/`output_tokens`; 1.37+ stabil, 1.41 reasoning token | K2 |
| https://spec.openapis.org/overlay/v1.1.0.html | Overlay 1.1.0 — spec'i **değiştirmeden** yamalama; `update`/`remove`/`copy` | K2 |
| https://spec.openapis.org/arazzo/latest.html | Senaryo artefaktının standardı (RESEARCH-0003 §5.2 ile ortak) | K2 |
| https://simonwillison.net/2025/Jun/16/the-lethal-trifecta/ | Tool çıktısının enjeksiyon yüzeyi olması; üç yetenekten birini kesme disiplini | K2/K3 |
| https://www.nuget.org/packages/JsonSchema.Net.OpenApi · https://github.com/corvus-dotnet/Corvus.JsonSchema · https://github.com/RicoSuter/NJsonSchema | .NET doğrulayıcı adayları; OAS 3.1 vocabulary; dialect farkı — **kurulu API'ye karşı doğrulanacak** | K2 |
