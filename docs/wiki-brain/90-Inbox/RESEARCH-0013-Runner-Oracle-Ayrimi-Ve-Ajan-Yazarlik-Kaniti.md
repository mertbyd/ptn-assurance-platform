---
id: RESEARCH-0013
type: research
status: active
title: Runner-oracle ayrimi, ajan yazarlik kaniti ve gecisin olculmus getirisi
created: 2026-08-13
updated: 2026-08-13
decision_refs:
  - ADR-0007
  - ADR-0008
rule_refs:
  - RULE-0005
  - RULE-0006
---

# RESEARCH-0013 — Runner-oracle ayrımı, ajan yazarlık kanıtı ve geçişin ölçülmüş getirisi

Bu belge **ADR-0014, ADR-0015 ve ADR-0016'nın tek dayanak kaydıdır.** Üç sorunun global
taramasını içerir:

1. Kendi koşum motorumuzu yazmalı mıyız, yoksa hazır bir Arazzo runner mı çalıştırmalıyız?
2. Yapay zekânın yazdığı test gerçekten işe yarıyor mu, hangi koşulda yaramıyor?
3. Bu sisteme geçmenin ölçülebilir getirisi nedir?

> Bu belge `90-Inbox` altındadır ve **kanonik değildir**. Karar ADR'dedir; çelişkide ADR kazanır.

---

## 1. Arazzo runner ekosistemi — tarama sonucu

Arazzo, OpenAPI Initiative'in çok adımlı API iş akışı standardıdır. Sürüm **1.1.0**.
Step Object; `operationId`/`operationPath`/`workflowId`/`channelPath`, `parameters`,
`requestBody`, `successCriteria`, `onSuccess`, `onFailure`, `outputs`, `dependsOn`, `timeout`
alanlarını taşır. Criterion Object dört tip destekler: `simple`, `regex`, `jsonpath` (RFC 9535),
`xpath`. Failure Action Object `end`/`retry`/`goto` ile `retryAfter` ve `retryLimit` taşır.

**Kritik: `x-` uzantısı Step Object dahil 14 nesnede açıkça izinlidir.**

### Mevcut runner'lar

| Runner | Dil | Lisans | Olgunluk | Adım başına OpenAPI kontrolü | Çıktı | Plugin |
|---|---|---|---|---|---|---|
| **Respect** (Redocly CLI) | Node | **MIT** | 1.5k ★, 2258 commit, npm + Docker | **Var** (status/content-type/schema/successCriteria) | **HAR + JSON** | Yok |
| arazzo-runner (Jentic) | Python | Apache-2.0 | 61 ★, 147 commit | Yok | CLI/lib | Yok |
| itarazzo | Java | — | Düşük | Yok | — | Yok |
| arazzo-runner (AdrianMachado) | TypeScript | — | 11 ★, **npm'de yayımlanmamış** | Yok | `{status, reason}` | Yok |
| Specmatic-Arazzo | JVM/Docker | **Ticari (Enterprise)** | — | Var | HTML + konsol | — |

### Bulgular

**B1. .NET/C# Arazzo runner yoktur.** Ne `awesome-arazzo` listesinde, ne `openapi.tools`
koleksiyonunda, ne de Microsoft.OpenApi tarafında. Parser bile yalnız Java
(`API-Flows/openapi-workflow-parser`) ve Python olarak mevcuttur.

**B2. Respect tek olgun ve serbest seçenektir.** MIT, `redocly/cli` Docker imajı, Node 22.12+.
`--har-output` ve `--json-output` üretir. `REDOCLY_CLI_RESPECT_SEVERITY` ile
`STATUS_CODE_CHECK` / `SCHEMA_CHECK` / `SUCCESS_CRITERIA_CHECK` / `CONTENT_TYPE_CHECK`
seviyeleri `off|warn|error` olarak ayarlanır. XPath criteria **desteklenmiyor**.

**B3. Olay tabanlı adımı yalnız Specmatic destekliyor** ve ticari. Arazzo 1.1 `channelPath`,
`action: send|receive`, `correlationId` ve `$message.payload#/...` ile AsyncAPI adımlarını
zaten tanımlamış durumda; eksik olan runner tarafıdır.

**B4. HAR 1.2 donmuş bir JSON formatıdır** ve her `entry` için tam request/response
(header, body, content-type) ile `timings` taşır. Standart, bedava ve eksiksiz bir kanıt formatı.

---

## 2. "İcra eden" ile "hakem" ayrımı — global desen

Bu ayrımı yapan olgun projeler:

| Proje | İcra eden | Hakem |
|---|---|---|
| **Tracetest** | trigger (HTTP isteği) | OpenTelemetry span'lerine assertion — DB span'i dahil |
| **Schemathesis** | engine istekleri gönderir | `checks` koleksiyonu (`response_schema_conformance`, `status_code_conformance`, `content_type_conformance`) — **genişletilebilir** |
| **Dredd** | transaction çalıştırıcı | dil-bağımsız **hook sunucusu** (Node/Ruby/Python/PHP) |
| **Citrus** | HTTP client action | ayrı **SQL query + validate** action'ı |
| **Microcks** | test runner | `OPEN_API_SCHEMA` runner'ı uygunluk hakemi |

**Hiçbiri runner'ın hem koşturup hem hüküm vermesini istemiyor.** Tracetest tam olarak bizim
tarif ettiğimiz şeydir: tetikle → sonucu bekle → ayrı motorla yargıla.

**Farkımız:** bu araçların hakemi genel amaçlıdır ve *"muhtemelen şu, çünkü şu kanıt"* demez.
Bizim iki checker'ımızda hipotez üretip güven seviyesine göre sıralayan bir teşhis motoru
(`DiagnosisManager`) vardır.

### B5 — DB assertion bir Arazzo adımıdır, plugin gerektirmez

En değerli bulgu. Database Checker'ın assertion yüzeyi zaten gerçek HTTP endpoint'leridir
(`POST /assertions/row|count|absent|batch`, kendi Swagger grubuyla). Arazzo'nun tek işi
HTTP operasyonu çağırmaktır.

Sonuç: DB doğrulaması Arazzo dokümanında **sıradan bir adım** olur. Jenerik runner ne fork
ne plugin ister; sadece POST atar. Kazançlar:

- Zamanlama doğru — adım, mutasyon adımından hemen sonra sırayla koşar.
- Eventual consistency çözülür — `timeoutMs`/`pollIntervalMs` DB Checker'ın polling çekirdeğinde.
- Bulgu ayrıntısı HAR'a düşer — `FailedExpectations[]` response gövdesindedir.
- Ham SQL ve secret yoktur — `RowAssertionRequestDto` bilinçli olarak serbest SQL taşımaz.

### B6 — Zamanlama ayrımı (tasarımı belirleyen kısıt)

| Kontrol | Girdi | Ne zaman çalışabilir |
|---|---|---|
| Response uygunluğu | (istek, yanıt, spec) — **saf fonksiyon** | Koşum sonrası HAR'dan replay ile **birebir aynı sonuç** |
| DB assertion | O andaki **veritabanı durumu** | Yalnız koşum sırasında, doğru adımda |

DB assertion'ı koşum bittikten sonra HAR'dan çalıştırmak sessiz yanlış sonuç üretir: sonraki
adımlar durumu değiştirmiş olabilir, "geçti" diyen test hiçbir şey doğrulamamış olur.

---

## 3. Ajan yazarlığı — ölçülmüş sınırlar

### B7 — Uygulamadan test üretmek kör nokta yaratır

*"On the risk of coding before testing: An empirical study on LLM-based test generation workflow"*
(Konstantinou, Tambon, Papadakis): mevcut koddan üretilen testler **uygulamanın davranışını
doğrulamaya** optimize olur, niyeti değil. Spesifikasyondan üretilen testler anlamlı ölçüde daha
fazla hata yakalar. Kod önce yazılmışsa test onun yanlış varsayımlarını miras alır.

### B8 — Ajan kendi başına zayıf assertion yazar

*"Rethinking the Value of Agent-Generated Tests for LLM-Based Software Engineering Agents"*:

- Ajan geri bildiriminin **%70-77'si `print` ifadesi**, assertion değil
- Assertion'ların **%33-41'i** yalnız "alan var mı" kontrolü
- **%35-43'ü** tam değer eşitliği
- İlişkisel/aralık kontrolü **yalnız %3-8**
- Testleri teşvik etmek çözüm oranını değiştirmedi ("only zero net change in #Success")

### B9 — İteratif düzeltme işe yarar, ama neye karşı?

Ampirik çalışmalar iteratif düzeltmenin geçerli test oranını **%24'ten %70+'a** çıkardığını
gösteriyor. **Kritik ayrım:** düzeltme *sözleşmeye* karşı yapılırsa kazanç; *gözlenen davranışa*
karşı yapılırsa B7'ye geri düşülür.

### Tasarıma etkisi

| Ölçülmüş tuzak | Yapısal karşılık |
|---|---|
| B7 — uygulamadan öğrenme | Ajanın yazım anındaki tek girdileri: `kurallar.md` (niyet), OpenAPI snapshot (sözleşme), DB şeması (yapı). Çalışan sistemin davranışını **görmez** |
| B8 — zayıf assertion | Ajan serbest kod yazamaz; yalnız tipli sözleşmeye assertion emit eder ve `ValidateScenarioAssertionsAsync` bunu **makine ile doğrular** |
| B9 — yanlış yöne düzeltme | `dryRun` başarısızlığı ajana düzeltme yetkisi vermez; çelişki bildirimi döner (RULE-0005) |

### B10 — Yazarlık yüzeyleri zaten mevcut

Ajanın "sorması" gereken her şey iki checker'da public AppService olarak vardır:

| Ajanın sorusu | Yüzey |
|---|---|
| Bu iş adımı hangi operasyona düşüyor? | `IResponseConformanceAppService.SuggestOperationBindingsAsync` |
| Geçerli istek gövdesi nedir? | `BuildRequestExampleAsync` |
| Bu assertion sözleşmeden türetilebilir mi? | `ValidateScenarioAssertionsAsync` |
| Hangi tablo/kolon var, anahtar PK/unique mi? | `ISchemaDiscoveryAppService.DescribeTableAsync` |
| Hedef şemanın fotoğrafı | `GetSnapshotAsync` |

`DescribeTableAsync`'in kod yorumu bunu zaten söylüyor: *"MALIYETLI; yalniz senaryo YAZIM aninda"*.

---

## 4. MCP tarafı

MCP üç birinci sınıf bağlam tipi tanımlar: **Tool** (çalıştırılabilir eylem), **Resource**
(salt-okunur veri), **Prompt** (yeniden kullanılabilir şablon).

- `2025-06-18` revizyonu **elicitation** (sunucunun kullanıcıdan ek girdi istemesi),
  yapılandırılmış tool çıktısı ve tool sonucunda resource link ekledi.
- `2025-11-25` revizyonu **Tasks** primitive'ini ekledi (deneysel): uzun süren işler için
  dayanıklı durum makinesi. Durumlar: `working`, `input_required`, `completed`, `failed`,
  `cancelled`.

`input_required` durumu yama onayı akışının protokoldeki doğrudan karşılığıdır.

**`kurallar.md` için sonuç:** MCP `Resource` tipi salt-okunur bağlam için tasarlanmıştır.
Kural dokümanı ayrı bir veritabanı tablosu gerektirmez; sürüm kontrolündeki dosya olarak
sunulur, koşu satırında yalnız `rules_fingerprint` tutulur.

---

## 5. Geçişin ölçülmüş getirisi

| Ölçüm | Kaynak |
|---|---|
| Sözleşme testi benimseyen kuruluşlarda **%30 daha az üretim olayı**, **%20 daha hızlı sürüm** | 2023 State of Testing |
| Erken yakalanan hata **~$200**, üretime kaçan **~$4.500**; IBM SSI: **~100 kat** | NIST/IBM defect-cost serisi |
| Vaka: sürüm başına üretim hatası **45 → 3 (%93)**, ~$280k yatırım, yıllık $9M+ tasarruf | Sözleşme testi vaka çalışması |
| Google: CI'daki **pass→fail geçişlerinin %84'ü** gerçek hata değil, flaky | Google 2016 |
| Google: flaky testler kodlama zamanının **%2'sinden fazlasını** yiyor; 50 kişilik ekipte **yılda 1 kişi-yıl** | Google |
| Microsoft: her flaky araştırması ortalama **30 dakika**; flakiness %18 azaltımı → **%2,5 üretkenlik** | Microsoft |

### B11 — 2025'in asıl bulgusu

DORA 2025: **AI benimsemesi arttıkça yazılım teslim kararsızlığı artıyor** — bireysel etkinlik
ve kod kalitesi yükselse bile. Sebep hacim: AI, kodu inceleme ve dağıtım altyapısının
soğurabileceğinden hızlı üretiyor.

DORA'nın tavsiyesi: *"AI kullanımı artarken Change Failure Rate yükseliyorsa, daha fazla AI
aracı değil, daha fazla test kapsamı gerekir."*

Bu platform tam olarak o boşluğu doldurur ve AI'ı doğru yerde kullanır: **test yazmayı
hızlandırmakta, hüküm vermekte değil.**

### Özgül getiri

**%84'ün içinden çıkmak.** Pass→fail geçişlerinin %84'ü flaky ve bu ayrımı bugün insan elle
yapıyor (Microsoft: araştırma başına 30 dk). `spec_fingerprint` + `db_schema_fingerprint`
bu ayrımı iki kolon karşılaştırmasına indirir: parmak izi değiştiyse ortam kayması,
değişmediyse gerçek bulgu.

---

## 6. Büyüme yönleri (veri modeli değişmeden)

| Yön | Durum |
|---|---|
| **Olay tabanlı test** | Arazzo 1.1 zaten tanımlıyor; Respect desteklemiyor, Specmatic ticari. `IWorkflowRunnerPort` arkasından ikinci adapter. **Adım adımdır — model değişmez** |
| **Kapsam ölçümü** | `compiled_document` + `spec_snapshot_id` üzerinden read model. *"140 operasyonun kaçına dokunuluyor"* ve *"BR-015 hiç test edilmiyor"* aynı sorgudan |
| **Güvenlik (OWASP API Top 10 2023)** | BOLA hâlâ 1 numara; tespiti *"A olarak doğrula, token'ı B ile değiştir, tekrarla"* — birebir bir Arazzo workflow'u. Yeni motor değil, yeni şablon |
| **Sözleşme değişikliği tetikli koşum** | Tetikleyici kodu ve lookup değeri zaten modelde; tek eksik `compiled_document`'tan türetilen adım adres indeksi. **Ölçülene kadar eklenmez** |
| **Sağlık / flaky** | `history_id` + `is_dry_run` + `attempt` girdileri hazır. Sağlık **materialized view** olarak başlar |
| **Kendi kendini onarma** | Overlay ile öneri üretilir; uygulaması RULE-0005 ile insan onayına bağlı. Erken açmak B7'ye geri dönmektir |

---

## 7. Bu sistemin yapmadıkları

- **Yük/performans testi değil.** `duration_ms` regresyon sinyali verir, k6 yerine geçmez.
- **UI testi değil.** Kapsam API + veritabanı.
- **API veya DB izi bırakmayan iş mantığını doğrulayamaz.** Etki gözlenebilir değilse test edilemez.
- **Runner uyumu bizim elimizde değil.** Respect XPath criteria desteklemiyor; yayın kapısında yasaklanır.
- **Node bağımlılığı gelir.** `redocly/cli` ayrı konteyner, sabit sürüm.

---

## 8. Kaynaklar

**Standartlar**
- Arazzo Specification v1.1.0 — <https://spec.openapis.org/arazzo/latest.html>
- RFC 9457 Problem Details — <https://www.rfc-editor.org/rfc/rfc9457.html>
- SARIF 2.1.0 (OASIS) — <https://docs.oasis-open.org/sarif/sarif/v2.1.0/sarif-v2.1.0.html>
- W3C Trace Context — <https://www.w3.org/TR/trace-context/>
- HAR 1.2 — <http://www.softwareishard.com/blog/har-12-spec/>
- OpenTelemetry test attributes — <https://opentelemetry.io/docs/specs/semconv/registry/attributes/test/>
- CTRF JSON Schema — <https://ctrf.io/docs/full-schema>
- JUnit XSD (windyroad) — <https://github.com/windyroad/JUnit-Schema/blob/master/JUnit.xsd>
- Allure test result file — <https://allurereport.org/docs/how-it-works-test-result-file/>
- MCP Tools — <https://modelcontextprotocol.io/specification/2025-06-18/server/tools>
- MCP Tasks — <https://modelcontextprotocol.io/specification/2025-11-25/basic/utilities/tasks>
- OWASP Logging Cheat Sheet — <https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html>

**Runner ve araçlar**
- Respect CLI — <https://redocly.com/respect-cli>
- respect komut referansı — <https://redocly.com/docs/cli/commands/respect>
- Respect severity — <https://redocly.com/docs/respect/guides/severity>
- Redocly CLI (MIT) — <https://github.com/Redocly/redocly-cli>
- awesome-arazzo — <https://github.com/workflows-guru/awesome-arazzo>
- Jentic arazzo-engine (Apache-2.0) — <https://github.com/jentic/arazzo-engine>
- itarazzo — <https://github.com/leidenheit/itarazzo-library>
- openapi-workflow-parser (Java) — <https://github.com/API-Flows/openapi-workflow-parser>
- Specmatic Arazzo (ticari) — <https://docs.specmatic.io/supported_protocols/arazzo>

**Runner/oracle ayrımı precedent'leri**
- Tracetest — <https://docs.tracetest.io/>
- Schemathesis checks — <https://schemathesis.readthedocs.io/en/stable/reference/checks/>
- Dredd hooks — <https://github.com/apiaryio/dredd/blob/master/docs/hooks/index.rst>
- Citrus database actions — <https://github.com/citrusframework/citrus/blob/main/src/manual/actions-database.adoc>
- Microcks contract testing — <https://microcks.io/blog/continuous-testing-all-your-apis/>

**Ölçümler**
- On the risk of coding before testing — <https://arxiv.org/pdf/2607.05139>
- Rethinking the Value of Agent-Generated Tests — <https://arxiv.org/html/2602.07900>
- LLM Unit Test Generation: Achievements & Challenges — <https://arxiv.org/html/2511.21382v1>
- Flaky test benchmark (Google/Microsoft verileri) — <https://testdino.com/blog/flaky-test-benchmark>
- Flaky test geliştirici anketi — <https://arxiv.org/pdf/2203.00483>
- DORA metrics history — <https://dora.dev/insights/dora-metrics-history/>
- State of DevOps 2025, AI ve teslim kararsızlığı — <https://getdx.com/blog/dora-metrics/>
- Entegrasyon testi ROI — <https://www.readability.com/integration-testing-roi-what-it-actually-costs-vs-what-it-prevents>
- Hata maliyeti 2026 analizi — <https://testomat.io/blog/software-bug-cost/>
- OWASP API Security Top 10 2023 — <https://totalshiftleft.ai/blog/owasp-api-security-top-10-explained>
- API kapsam ölçümü — <https://karatelabs.io/api-coverage>
