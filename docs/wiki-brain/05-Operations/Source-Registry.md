---
id: SOURCE-0001
type: source-registry
status: active
title: Source and provenance registry
updated: 2026-08-13
decision_refs:
  - ADR-0001
  - ADR-0002
  - ADR-0004
  - ADR-0005
  - ADR-0009
  - ADR-0012
rule_refs: []
---

# Kaynak ve provenance kaydı

## Eksiksiz araştırma kataloğu

Önceki wiki ve araştırma dosyalarından toplanmış bütün dış URL’ler, domain bazlı katalog ve tarihsel sentez [[Research-Archive|ARCHIVE-0001]] içinde eksiksiz korunur. Archive güncel karar kaynağı değildir; kaynak kaybını ve tekrar araştırmayı önleyen kanıt deposudur.

## Birincil yerel kaynaklar

| Kaynak | Yol | Kullanım |
|---|---|---|
| Merkezi paket workspace | `C:\Users\mertb\RiderProjects\ptn-assurance-platform` | Güncel checker/Vault source ve package metadata |
| API Contract Checker upstream | `C:\Users\mertb\RiderProjects\ptn-api-contract-checker` | Motor ayrıntısı, karşılaştırma semantiği, eski Wiki Brain |
| Database Checker upstream | `C:\Users\mertb\Documents\Codex\2026-07-06\bi\ptn-database-comparison-api` | DB motor invariantları, migration ve T12 geçmişi |
| Authenticator upstream | `C:\Users\mertb\RiderProjects\pintern-authenticator-latest-api` | Tek issuer/identity owner bilgisi |
| Foundation upstream | `C:\Users\mertb\RiderProjects\nexum-abp-foundation` | Authenticator'ın public ortak base paket ailesi |
| Notifications upstream | `C:\Users\mertb\RiderProjects\pintern-notifications` | Bildirim capability durumu |

## Seçilmiş resmî dış kaynaklar

| Kaynak | Kullanım | Son erişim |
|---|---|---|
| https://www.nuget.org/packages/CheckNexus.ApiContracts/0.1.0-alpha.5 | API checker public paket kaydı | 2026-08-11 |
| https://www.nuget.org/packages/CheckNexus.DatabaseComparison/0.1.0-alpha.5 | DB checker public paket kaydı | 2026-08-11 |
| https://api.nuget.org/v3-flatcontainer/ | Exact PackageId/version yayın preflight ve yayın-sonrası doğrulama | 2026-08-12 |
| https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-push | `dotnet nuget push`, symbol source/key ve duplicate davranışı | 2026-08-12 |
| https://learn.microsoft.com/en-us/nuget/nuget-org/scoped-api-keys | Aile glob'una daraltılmış push-only API key sözleşmesi | 2026-08-12 |
| https://learn.microsoft.com/en-us/nuget/api/symbol-package-publish-resource | `.snupkg` symbol publish endpoint ve tekrar deneme davranışı | 2026-08-12 |
| https://www.nuget.org/packages/CheckNexus.ApiContracts/0.2.0-alpha.1 | API checker 0.2 public release doğrulaması | 2026-08-12 |
| https://www.nuget.org/packages/CheckNexus.DatabaseComparison/0.2.0-alpha.1 | DB checker 0.2 public release doğrulaması | 2026-08-12 |
| https://www.nuget.org/packages/Pintern.SaaS.Notifications.Domain.Shared/0.1.0-alpha.1 | Notifications public aile doğrulaması | 2026-08-12 |
| https://api.nuget.org/v3/index.json | NuGet V3 registry doğrulaması | 2026-08-11 |
| https://www.nuget.org/packages/Nexum.Abp.Foundation.Domain.Shared/1.0.0 | Foundation public aile doğrulaması | 2026-08-13 |
| https://www.nuget.org/packages/Authenticator.Application/1.0.1 | Authenticator public `1.x` envanteri | 2026-08-13 |
| https://learn.microsoft.com/dotnet/core/project-sdk/msbuild-props#package-validation-properties | `EnablePackageValidation` ve baseline özellikleri | 2026-08-12 |
| https://learn.microsoft.com/nuget/create-packages/symbol-packages-snupkg | `IncludeSymbols` + `SymbolPackageFormat=snupkg` üretim sözleşmesi | 2026-08-12 |
| https://github.com/dotnet/sourcelink/blob/main/README.md | Repository metadata ve Azure Repos SourceLink provider seçimi | 2026-08-12 |
| https://github.com/RicoSuter/NJsonSchema | KBP-621 runtime JSON Schema doğrulayıcı kaynağı | 2026-08-12 |
| https://abp.io/docs/latest/framework/architecture/modularity/basics | ABP module graph | 2026-08-11 |
| https://abp.io/docs/latest/framework/data/entity-framework-core/migrations | ABP/EF migration sınırı | 2026-08-11 |
| https://learn.microsoft.com/ef/core/managing-schemas/migrations/projects | Ayrı migration project/assembly modeli | 2026-08-11 |
| https://developer.hashicorp.com/vault/docs/secrets/kv/kv-v2 | Vault KV v2 davranışı | 2026-08-11 |
| https://developer.hashicorp.com/vault/docs/concepts/policies | Least-privilege policy modeli | 2026-08-11 |
| https://openid.net/specs/openid-connect-core-1_0.html | Tek issuer/OIDC sözleşmesi | 2026-08-11 |
| https://modelcontextprotocol.io/specification/versioning | MCP revizyon durumu; **current revizyon `2026-07-28`** | 2026-08-12 |
| https://modelcontextprotocol.io/specification/2026-07-28/server/tools | Tool sözleşmesi: `outputSchema`, `ttlMs`/`cacheScope`, deterministik sıra, handle deseni | 2026-08-12 |
| https://www.postgresql.org/docs/current/errcodes-appendix.html | SQLSTATE sınıfları; nesne adı alanlarının tam kapsamı yalnız sınıf 23'te | 2026-08-12 |
| https://www.npgsql.org/doc/api/Npgsql.PostgresException.html | Yapılandırılmış hata alanları (`SqlState`, `ConstraintName`, `TableName`, `ColumnName`) | 2026-08-12 |
| https://www.rfc-editor.org/rfc/rfc9457.html | Problem Details; teşhis raporunun taşıma formatı | 2026-08-12 |
| https://spec.openapis.org/arazzo/latest.html | Senaryo adımı, `successCriteria`, `retry`, `correlationId` sözleşmesi | 2026-08-12 |
| https://www.nuget.org/packages/ModelContextProtocol.AspNetCore | Mevcut ASP.NET Core host'una `MapMcp()` ile MCP ekleme | 2026-08-12 |
| https://httpwg.org/specs/rfc9110.html | HTTP durum kodu **sınıfları**; 401 → `WWW-Authenticate` MUST; 405 → `Allow` MUST; safe/idempotent metot tanımı — API teşhisinin yapılandırılmış alan temeli | 2026-08-12 |
| https://datatracker.ietf.org/doc/html/rfc6750 | Bearer hata kodları (`invalid_token`, `insufficient_scope`) ve `scope` parametresi | 2026-08-12 |
| https://www.rfc-editor.org/rfc/rfc6901.html | JSON Pointer — uygunluk ihlali adresinin standart biçimi | 2026-08-12 |
| https://www.oasdiff.com/docs/breaking-changes | 509 kurallık fark kataloğu (213 breaking / 30 warning / 266 info) ve kural kimliği grameri; `DifferenceKindCodes` bu gramerin alt kümesi | 2026-08-12 |
| https://schemathesis.io/ | Spec'i test oracle'ı sayan kapalı check kataloğu (`response_schema_conformance` vb.) | 2026-08-12 |
| https://stoplight.io/open-source/prism | Validation proxy: istek ve yanıt uygunluğunun aynı anda doğrulanması | 2026-08-12 |
| https://docs.specmatic.io/contract_driven_development/backward_compatibility | Yeni spec'ten mock + eski spec'ten test = uyumluluk kapısı; mutasyon kapısının tersi | 2026-08-12 |
| https://blog.modelcontextprotocol.io/posts/2026-07-28/ | Stateless çekirdek; `ttlMs`/`cacheScope` (SEP-2549); JSON Schema 2020-12 input/outputSchema (SEP-2106) | 2026-08-12 |
| https://modelcontextprotocol.io/extensions/tasks/overview | Tasks uzantısı: task handle, `tasks/get`, `tasks/update`, TTL + önerilen poll aralığı | 2026-08-12 |
| https://www.anthropic.com/engineering/advanced-tool-use | Tool Search / `defer_loading`: %85 token azalması ve tool seçim doğruluğunda artış | 2026-08-12 |
| https://www.anthropic.com/engineering/writing-tools-for-agents | Token-verimli tool yanıtı; 25.000 token yanıt tavanı; eval ile tool iyileştirme | 2026-08-12 |
| https://arxiv.org/html/2604.25862v1 | RESTestBench: LLM üretimi testlerin mutasyon skoru; hatalı SUT'a refinement'in oracle'ı bozması | 2026-08-12 |
| https://arxiv.org/html/2507.16044v4 | REST→MCP ampirik çalışması: 116 sunucu, medyan %19 operasyon açılımı, seçici tool tasarımı | 2026-08-12 |
| https://opentelemetry.io/blog/2026/genai-observability/ | GenAI semconv token kullanım öznitelikleri (`gen_ai.usage.*`) | 2026-08-12 |
| https://modelcontextprotocol.io/extensions/tasks/overview | Tasks durumları `working / input_required / completed / failed / cancelled`; `ttlMs`, `pollIntervalMs`, kooperatif iptal; onay adımının protokoldeki karşılığı | 2026-08-12 |
| https://spec.openapis.org/overlay/v1.0.0.html | Overlay: sıralı `actions[]`, JSONPath `target`, `update` merge / `remove: true` — heal yamasının taşıma formatı | 2026-08-12 |
| https://ctrf.io/docs/full-schema | CTRF tam şeması: `summary`, `tests[]` (`retries`, `flaky`, `steps[]`, `attachments[]`, `insights`), `environment`, `baseline`; statü enum'u beş değerli | 2026-08-12 |
| https://opentelemetry.io/docs/specs/semconv/registry/attributes/test/ | `test.case.name`, `test.case.result.status` (`pass`/`fail`), `test.suite.name`, `test.suite.run.status` (altı değer) | 2026-08-12 |
| https://docs.oasis-open.org/sarif/sarif/v2.1.0/sarif-v2.1.0.html | SARIF 2.1.0 OASIS standardı; bulgu taşıma, fingerprint ve baseline/suppression kavramları | 2026-08-12 |
| https://docs.pact.io/getting_started/versioning_in_the_pact_broker | Sözleşme içeriğinin **hash ile dedup'u**, pacticipant/version ayrımı, matrix ve `can-i-deploy`; "etkilenen consumer" akışı | 2026-08-12 |
| https://docs.datadoghq.com/tests/flaky_management/ | Flaky durum makinesi: `Active / Quarantined / Disabled / Fixed` ve izlenen etki metrikleri | 2026-08-12 |
| https://docs.qameta.io/allure-testops/briefly/test-results/ | Kimlik üçlüsü (AllureID / `testCaseId = md5(fullName + sort(params))` / `historyId`), beş statü, launch kapanışında case upsert | 2026-08-12 |
| https://kiwitcms.readthedocs.io/en/latest/_modules/tcms/testruns/models.html | Açık kaynak ilişkisel model: `TestRun`/`TestExecution` alanları, **`case_text_version`**, statü lookup'ı (`weight`) | 2026-08-12 |
| https://reportportal.io/docs/developers-guides/ReportingDevelopersGuide/ | `Launch → TestItem → Log → Attachment`; `hasStats=false` nested step; Postgres + nesne deposu + log indeksi ayrımı | 2026-08-12 |
| https://docs.testkube.io/articles/test-workflows-high-level-architecture | Tanım kaynağı ile koşum kaynağının ayrılması; efemer koşum; sonuç/artefakt taşınması | 2026-08-12 |
| https://playwright.dev/docs/test-agents | planner/generator/healer; `seed.spec.ts` tohum testi; ajan = talimat + MCP tool demeti | 2026-08-12 |
| https://schemathesis.readthedocs.io/en/stable/explanations/stateful/ | OpenAPI link'lerinden üretici/tüketici zinciri ve durum makinesi | 2026-08-12 |
| https://learn.microsoft.com/azure/devops/pipelines/test/test-impact-analysis | Deterministik etki analizi (çağrı grafiği + kapsam); ML'siz seçim gerekçesi | 2026-08-12 |
| https://genai.owasp.org/resource/owasp-genai-llm-top-10-2026/ | LLM01 Prompt Injection birinci; **LLM03 Excessive Agency** altıncılıktan üçüncülüğe; gerçek olay verisiyle ağırlıklandırma | 2026-08-12 |
| https://abp.io/docs/latest/framework/infrastructure/blob-storing | ABP BLOB Storing container sistemi ve sağlayıcıları (Database/FileSystem/S3-uyumlu/MinIO) | 2026-08-12 |
| https://abp.io/docs/latest/framework/infrastructure/background-jobs | Arka plan iş yöneticisi; kümede tek örnek çalıştırma için distributed lock gereksinimi | 2026-08-12 |
| https://www.datadoghq.com/blog/engineering/mcp-server-agent-tools/ | Token bütçeli sayfalama; CSV/TSV ~%50 ve YAML ~%20 tasarruf, aynı bütçede ~5× kayıt; alan kırpmanın "tek en yüksek kaldıraç" olması; sorgu > ham veri (~%40 ucuz); endpoint başına değil iş başına tool; öğreten hata mesajı | 2026-08-12 |
| https://www.anthropic.com/engineering/code-execution-with-mcp | Tool'ları kod API'si olarak sunmak: 150.000 → 2.000 token (%98,7); ara sonuçların yürütme ortamında kalması; PII'nin bağlama girmemesi | 2026-08-12 |
| https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents | Compaction, structured note-taking, just-in-time retrieval, alt-ajan izolasyonu | 2026-08-12 |
| https://deepwiki.com/github/github-mcp-server/3-github-toolsets | Toolset seçimiyle %60–90 bağlam düşüşü; dinamik toolset keşfi | 2026-08-12 |
| https://arxiv.org/html/2506.01056v1 | MCP-Zero: aktif tool keşfi; bağlam kullanımında iki büyüklük mertebesi azalma | 2026-08-12 |
| https://dl.acm.org/doi/10.1145/2635868.2635920 | Luo ve ark. (FSE 2014): 51 repo / 201 commit; flakiness kök sebepleri — `Async Wait`, `Concurrency`, `Test Order Dependency` ilk üç | 2026-08-12 |
| https://arxiv.org/html/2602.03556 | SAP HANA 2026: endüstriyel DBMS'te `Async Wait` baskınlığının sürdüğü | 2026-08-12 |
| https://www.sciencedirect.com/science/article/pii/S0164121223002327 | Flakiness üzerine çok-sesli derleme: sebep, tespit, etki, yanıt | 2026-08-12 |
| https://www.engr.ship.edu/~chuo/papers/huo14.pdf | Kırılgan assertion tespiti; oracle kırılganlığı ("too restrictive range") | 2026-08-12 |
| https://flakyguard.com/blog/cost-of-flaky-tests | 1.000+ ekip verisi: flaky triyaj medyanı 28 dk; ekip başına yıllık maliyet | 2026-08-12 |
| https://totalshiftleft.ai/blog/test-data-management-best-practices-api-testing | Koşu başına izole veri kümesi, fixture sürümleme, spec'ten veri üretimi | 2026-08-12 |

### Runner sınırı ve ajan yazarlığı (RESEARCH-0013 → ADR-0014/0015/0016)

| Kaynak | Kullanım | Son erişim |
|---|---|---|
| https://redocly.com/respect-cli | **Seçilen runner**: Arazzo iş akışı koşturma, adım başına şema/durum/content-type kontrolü | 2026-08-13 |
| https://redocly.com/docs/cli/commands/respect | `--har-output`, `--json-output`, `--execution-timeout`, `--max-fetch-timeout`, `--no-secrets-masking` | 2026-08-13 |
| https://redocly.com/docs/respect/guides/severity | `STATUS_CODE_CHECK` / `SCHEMA_CHECK` / `SUCCESS_CRITERIA_CHECK` / `CONTENT_TYPE_CHECK` seviyeleri (`off\|warn\|error`) | 2026-08-13 |
| https://github.com/Redocly/redocly-cli | **MIT lisansı**, `redocly/cli` Docker imajı, Node 22.12+ | 2026-08-13 |
| https://github.com/workflows-guru/awesome-arazzo | Araç kataloğu — **.NET/C# runner yok** | 2026-08-13 |
| https://github.com/jentic/arazzo-engine | Python runner (Apache-2.0); runner projesi ölürse geçiş hedefi | 2026-08-13 |
| https://github.com/API-Flows/openapi-workflow-parser | Java parser; Arazzo parser'ının bile .NET karşılığı olmadığının kanıtı | 2026-08-13 |
| https://docs.specmatic.io/supported_protocols/arazzo | AsyncAPI adımını destekleyen **tek** araç — ticari (Enterprise); olay tabanlı büyüme yönü | 2026-08-13 |
| http://www.softwareishard.com/blog/har-12-spec/ | **HAR 1.2** (donmuş): `entries[]`, tam request/response, `timings` — kanıt formatı | 2026-08-13 |
| https://www.w3.org/TR/trace-context/ | `traceparent`; **trace-id 32 hex, span-id 16 hex** — `test_runs.trace_id` biçimi | 2026-08-13 |
| https://github.com/windyroad/JUnit-Schema/blob/master/JUnit.xsd | `<failure>` (assertion) ile `<error>` (beklenmeyen sorun) ayrımı — `Failed`/`Broken` dayanağı | 2026-08-13 |
| https://allurereport.org/docs/how-it-works-test-result-file/ | `historyId`/`testCaseId` ile `name` ayrımı — kalıcı kimlik tuzağı | 2026-08-13 |
| https://docs.tracetest.io/ | "Tetikle → bekle → ayrı motorla yargıla" precedent'i | 2026-08-13 |
| https://schemathesis.readthedocs.io/en/stable/reference/checks/ | Genişletilebilir `checks` koleksiyonu; runner ≠ hakem | 2026-08-13 |
| https://github.com/apiaryio/dredd/blob/master/docs/hooks/index.rst | Dil-bağımsız hook sunucusu; icra/doğrulama ayrımı | 2026-08-13 |
| https://github.com/citrusframework/citrus/blob/main/src/manual/actions-database.adoc | HTTP adımından sonra ayrı SQL query + validate action'ı | 2026-08-13 |
| https://arxiv.org/pdf/2607.05139 | **Coding before testing**: uygulamadan üretilen test uygulamanın davranışını doğrular, niyeti değil — RULE-0005 dayanağı | 2026-08-13 |
| https://arxiv.org/html/2602.07900 | **Agent-generated tests**: geri bildirimin %70-77'si `print`; ilişkisel assertion %3-8 — RULE-0006 dayanağı | 2026-08-13 |
| https://arxiv.org/html/2511.21382v1 | LLM birim test üretimi: başarılar, zorluklar, iteratif düzeltmenin %24→%70+ etkisi | 2026-08-13 |
| https://getdx.com/blog/dora-metrics/ | **DORA 2025**: AI benimsemesi arttıkça teslim kararsızlığı artıyor; çözüm "daha fazla test kapsamı" | 2026-08-13 |
| https://dora.dev/insights/dora-metrics-history/ | CFR ve failed deployment recovery time tanımları | 2026-08-13 |
| https://testdino.com/blog/flaky-test-benchmark | Google: pass→fail geçişlerinin **%84'ü flaky**, kodlama zamanının %2'si; Microsoft: araştırma başına 30 dk | 2026-08-13 |
| https://www.readability.com/integration-testing-roi-what-it-actually-costs-vs-what-it-prevents | Entegrasyon hatasının orantısız maliyeti; sözleşme testi %30 daha az üretim olayı | 2026-08-13 |
| https://testomat.io/blog/software-bug-cost/ | Erken $200 / üretim $4.500; IBM SSI ~100 kat | 2026-08-13 |
| https://totalshiftleft.ai/blog/owasp-api-security-top-10-explained | OWASP API Top 10 2023; BOLA testi Arazzo workflow'u olarak ifade edilebilir | 2026-08-13 |
| https://karatelabs.io/api-coverage | Sözleşmeye karşı operasyon kapsamı — büyüme yönü | 2026-08-13 |

### Yazarlık hattı ve belirsizlik yönetimi (RESEARCH-0014 → ADR-0017)

| Kaynak | Kullanım | Son erişim |
|---|---|---|
| https://arxiv.org/pdf/2410.21136 | **LLM oracle'ları beklenen değil GERÇEKLEŞEN davranışı kodluyor**; mutasyon skoru %19,1 (Evosuite %17,3); bozuk kodda tanıma %41-46 → %32-37 | 2026-08-13 |
| https://arxiv.org/html/2604.25862 | **RESTestBench**: net gereksinim %13-92, belirsiz %2-54 (26-40 puan fark); belirsizde %90'a çıkan model **yok**; ucuz model pahalıyı geçiyor | 2026-08-13 |
| https://arxiv.org/html/2607.22880v1 | Coverage/mutation **bozuk kodda öngörü gücünü kaybediyor**; Defects4J v3.0, 854 hata, 101K test — kabul kriteri seeded-fault olmalı | 2026-08-13 |
| https://arxiv.org/html/2604.02039 | APITestGenie: %69,3 / %88,6; somutluk p=0,039, karmaşıklık p=0,038, **doküman zenginliği p=0,57 (etkisiz)** | 2026-08-13 |
| https://arxiv.org/html/2503.15079v1 | LogiAgent: LLM'i hakem yapınca tavan **%66,19 precision**; yanlış pozitifler halüsinasyondan | 2026-08-13 |
| https://javiertroyauma.github.io/publications/TSE2017_REST_prePrint.pdf | Metamorphic testing REST API: **%95,3 mutasyon skoru** (302/317 tohumlanmış hata) | 2026-08-13 |
| https://personales.us.es/sergiosegura/files/papers/segura19-met.pdf | Metamorphic relation kalıpları — MR kataloğunun temeli | 2026-08-13 |
| https://blog.kie.org/2020/07/making-executable-dmn-modeling-more-business-friendly.html | DMN + **MC/DC**: m^N test yerine doğrusal; tablo analizi boşluk/örtüşme/subsumption tespit ediyor | 2026-08-13 |
| https://www.pairwise.org/ · https://arxiv.org/pdf/1803.09006 | PICT / ACTS pairwise: *"hataların çoğu en fazla iki faktörün etkileşiminden"* | 2026-08-13 |
| https://cucumber.io/docs/bdd/example-mapping/ | **Example Mapping**: Story/Rule/Example/**Question** kartları; *"çok kırmızı kart = hikâye hazır değil"* — soru formatının standardı | 2026-08-13 |
| https://medium.com/@mattwynne/introducing-example-mapping-42ccd15f8adf | Example Mapping'in kaynağı (Matt Wynne); ~25 dk/hikâye, üç amigo | 2026-08-13 |
| https://gojko.net/books/specification-by-example/ | Specification by Example: örnekler testin **tek çalıştırılabilir kısmıdır**, girdi–çıktı ilişkisini açıkça göstermeli | 2026-08-13 |
| https://www.omg.org/spec/SBVR/1.5/PDF | **SBVR 1.5**: iş-okur kural notasyonu, modal operatörler (*"It is obligatory that…"*) | 2026-08-13 |
| http://www.kdmanalytics.com/sbvr/sbvr_intro_2.html | SBVR Structured English somut örnekleri | 2026-08-13 |
| https://en.wikipedia.org/wiki/Easy_Approach_to_Requirements_Syntax | **EARS** (Rolls-Royce, IEEE RE'09): 5 kalıp, sınırlı anahtar kelime; LLM'in ayrıştırması için daha basit | 2026-08-13 |
| https://www.sciencedirect.com/science/article/abs/pii/S0167923610002368 | **Karar tabloları anlaşılabilirlikte önde** (doğruluk, süre, güven) ve kullanıcı tercihi — tablo atılmaz, soru formatı ayrılır | 2026-08-13 |
| https://arxiv.org/pdf/2607.04436 | Belirsizlik taksonomisi: lexical / syntactic / semantic / vagueness / referential / incompleteness | 2026-08-13 |
| https://arxiv.org/html/2605.25284v1 | **LLM'ler belirsizliği tanıyor ama nadiren soruyor** → soru kararı LLM'e bırakılamaz | 2026-08-13 |
| https://dl.acm.org/doi/full/10.1145/3660810 | ClarifyGPT: tutarlılık kontrolüyle belirsizlik tespiti + hedefli soru üretimi | 2026-08-13 |
| https://dl.acm.org/doi/10.1145/3726302.3729922 | **AT-CoT**: önce belirsizlik tipini belirle, sonra ona uygun soruyu üret | 2026-08-13 |
| https://arxiv.org/abs/2501.10868 | **JSONSchemaBench**: karmaşık şemada constrained decoding çöküyor (Outlines %3, XGrammar %28, Guidance %41) → tek-adım üretim | 2026-08-13 |
| https://devblogs.microsoft.com/semantic-kernel/using-json-schema-for-structured-output-in-net-for-openai-models/ | **.NET birinci parti** şema-kısıtlı üretim: `ChatClientStructuredOutputExtensions` | 2026-08-13 |
| https://www.adaline.ai/blog/llm-as-a-judge-reliability-bias | LLM-as-judge: uzman alanda insanla **%60-68** uyum; position/verbosity/self-preference/family bias | 2026-08-13 |
| https://arxiv.org/html/2512.20845 | Çok-ajan reflexion: dış sinyal olmadan öz-düzeltme **güvenilmez**; degeneration-of-thought; 3× maliyet | 2026-08-13 |
| https://arxiv.org/html/2506.18203v1 | **Generation-Verification Gap**: doğrulamak üretmekten kolay | 2026-08-13 |
| https://arxiv.org/abs/2411.19804 | OpenAPI chunking: LLM-tabanlı ve format-özel chunking naifi geçiyor; **Discovery Agent** deseni | 2026-08-13 |
| https://github.com/SonAIengine/graph-tool-call | Graf tabanlı tool retrieval: **248 tool'da %82 doğruluk, %79 daha az token**; düz embedding ilişkileri kaybediyor | 2026-08-13 |
| https://github.com/adamecr/Common.DMN.Engine | **.NET DMN motoru** (NuGet); OMG standart XML, Camunda Modeler uyumlu | 2026-08-13 |
| https://github.com/red6/dmn-check | DMN dosyalarında statik analiz (boşluk/örtüşme tespiti) | 2026-08-13 |
| https://docs.stripe.com/billing/testing/test-clocks | **Stripe Test Clocks**: zaman kurallarının test edilebilirliği için endüstri deseni; saat yalnız ileri gider | 2026-08-13 |
| https://hypothesis.readthedocs.io/en/latest/stateful.html | Rule/invariant/precondition tabanlı durum makinesi testi; Schemathesis bunu kullanıyor | 2026-08-13 |
| https://link.springer.com/article/10.1007/s42979-025-03823-7 | GraphWalker tabanlı MBT, endüstriyel vaka (Alstom) | 2026-08-13 |
| https://arxiv.org/abs/2402.09171 | **Meta TestGen-LLM**: filtreler halüsinasyonu eliyor; %75 derlendi, %57 güvenilir geçti, **%25 kapsamı artırdı**, %73 kabul | 2026-08-13 |

> **Düzeltme (2026-08-12):** Bu tablo daha önce MCP kaynağı olarak `specification/2025-06-18`
> gösteriyordu. O revizyon iki sürüm geride kalmıştır; güncel revizyon `2026-07-28`tir
> (stateless çekirdek, extensions çerçevesi, Tasks extension'ı).

### TypeScript yazarlık ajanı runtime kararı (ADR-0023)

| Kaynak | Kullanım | Son erişim |
|---|---|---|
| https://nodejs.org/en/about/previous-releases | Node 24'ün LTS, Node 26'nın Current olduğunun üretim runtime kararı | 2026-08-16 |
| https://fastify.dev/docs/latest/Reference/LTS/ | Fastify 5 LTS ve desteklenen Node LTS hatları | 2026-08-16 |
| https://github.com/modelcontextprotocol/typescript-sdk/blob/main/docs/client.md | Streamable HTTP client, bearer auth provider, Resource/tool discovery ve kapanış akışı | 2026-08-16 |
| https://github.com/modelcontextprotocol/typescript-sdk/blob/main/docs/get-started/packages.md | MCP TypeScript SDK v2 paket sınırı: `@modelcontextprotocol/client` | 2026-08-16 |
| https://developers.openai.com/api/docs/guides/function-calling | Responses function calling JSON Schema, `call_id` ve tool-output döngüsü | 2026-08-16 |
| https://developers.openai.com/api/docs/guides/streaming-responses | Responses API streaming event akışı | 2026-08-16 |

## Kaynak politikası

Güncel olgu önce çalışan kod, migration, `.nupkg` ve resmî registry’den doğrulanır. Blog/forum keşif için kullanılabilir; karar kanıtının yerine geçmez. Yeni dış kaynak erişim tarihi ve desteklediği iddia ile bu sayfaya veya Research Archive kataloğuna eklenir.
