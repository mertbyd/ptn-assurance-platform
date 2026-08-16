---
id: PLAN-0003
type: plan
status: draft
title: Test Module ozellik listesi — alti an, kosum, kayit, yazarlik ve bakim
updated: 2026-08-16
decision_refs:
  - ADR-0007
  - ADR-0008
  - ADR-0014
  - ADR-0015
  - ADR-0016
rule_refs:
  - RULE-0002
  - RULE-0005
  - RULE-0006
---

# Test Module'e yapılacak işler

Bu liste consumer tarafının **tek, tekrarsız ve sıralı** iş özetidir. PLAN-0001 (`DBC-xx`) ve
PLAN-0002 (`ACC-xx`) checker listeleridir; bu listenin numaralandırması **TM-xx**'tir.

Ürünün akışı için önce [[04-Architecture/Alti-An|ARCH-0004]] okunur.

> [!IMPORTANT] Kararda ADR kazanır
> Geçerli model **4 ana tablo + 5 lookup**tur (ADR-0016); şema kaynağı
> `04-Architecture/Test-Platform-Schema.dbml`. Koşum motoru **dışarıdadır** (ADR-0015).
> Ajan **hakem değildir** (RULE-0005). Maddelerin **kapsamı** geçerlidir; tablo adları
> ADR-0016'ya göre okunur.

**Boyut:** S ≈ 1–3 gün · M ≈ 1–2 hafta · L ≈ 2–4 hafta (tek geliştirici, test dahil).

---

## ADR-0014/0015/0016'nın bu listeye etkisi

TM numaraları **korunmuştur** ki araştırma belgelerindeki çapraz referanslar kırılmasın.

| # | Eski hali | Yeni hali |
|---|---|---|
| TM-05 | Arazzo parser + runtime expression çözümü (**L**) | `redocly lint` çağrısı + `x-checknexus-db` derleyicisi (**S**) |
| **TM-07** | **Kendi adım koşum motorumuz (L)** | **İPTAL** — dış runner icra eder (ADR-0015) |
| TM-08 | Koşum sırasında oracle çağrısı (M) | DB adımı Arazzo'ya derlenir + HAR'dan toplu conformance (M) |
| TM-09 | Adım checkpoint'i + devam (M) | Job + idempotent claim + stale recovery (**S**) |
| TM-12 | `run_steps` tablosu (M) | HAR artefaktı; **tablo yok** (**S**) |
| TM-17 | Senaryo doğrulama (M) | **Büyür** — RULE-0006'nın uygulaması, asıl kapı (M) |
| TM-46, TM-51 | `business_glossary` tablosu | **Ertelendi** — Git + MCP `Resource` |
| TM-52 | `business_rules` tablosu (L) | **Ertelendi** — `kurallar.md` + `rules_fingerprint` |
| TM-54 | `operation_links` tablosu (L) | **Ertelendi** — `SuggestOperationBindingsAsync` yeterli |
| TM-55 | `effect_footprints` tablosu (L) | **Ertelendi** — ölçülmüş ihtiyaç yok |
| TM-56 | `knowledge_contents` tablosu | **İPTAL** — MCP `Resource` (ADR-0014 §A) |
| TM-23 | `finding_links` tablosu (L) | **Ertelendi** — `compiled_document`'tan türetilen indeks, ölçüldüğünde |
| TM-27 | `scenario_health` tablosu (M) | **Materialized view** olarak başlar (**S**) |
| TM-31 | MCP Tasks eşlemesi | Aynen kalır |
| — | — | **TM-60 yeni:** runner adapter'ı ve süreç sınırı |

**Silinen toplam: iki L kalemi (TM-07, TM-56 ve altı tablo).**

---

## Blok 0 — Temel

| # | Ne | Neden | Boyut |
|---|---|---|---|
| **TM-01** | **Şema ve migration sahipliği.** `test_lookup` / `test_catalog` / `test_run`; `TestModuleDbProperties` configuration'dan; ayrı migration assembly | Ad çakışması aynı tablonun iki modülce oluşturulmasına yol açar (RULE-0002) | S |
| **TM-02** | **5 lookup + kod sabitleri.** `test_run_statuses`, `test_outcome_statuses` (+`breaks_build`), `test_failure_categories`, `test_trigger_kinds`, `test_scenario_states` + `DataSeedContributor` | `Failed` (hakem hayır dedi) ile `Broken` (adım hiç koşamadı) ayrımı teşhis girdisini belirler — JUnit `<failure>`/`<error>` | S |
| **TM-03** | **`test_scenarios` tablosu — Kapandı (KBP-92).** Her satır bir sürüm: `scenario_key` + `version_no`, `source_document`/`source_hash`, `compiled_document`/`compiled_hash`, onay alanları | Kimlik hash'ten türetilirse ad değişiminde çift kayıt doğar; iki belge reprodüksiyon için zorunlu (ADR-0016) | **M** |
| **TM-04** | **Ortam bağlaması ABP `Setting` olarak.** Mantıksal ad → `baseUrl` / `specSnapshotId` / `dbConnectionId` / `secretRef`; koşumda çözülür, koşu satırına snapshot'lanır | Tablo ölçülmüş ihtiyaç değil; ABP ayar sistemi zaten kiracı kapsamlı (ADR-0016 §G) | S |
| **TM-05** | **Arazzo doğrulama + `x-checknexus-db` derleyicisi.** `redocly lint` çağrısı; uzantıyı DB Checker HTTP adımına derleme; XPath criteria yasağı | Parser yazmıyoruz; derleyici ise bizim işimiz (ADR-0015 §C) | S |

---

## Blok 1 — An 5: Koşum

| # | Ne | Neden | Boyut |
|---|---|---|---|
| **TM-06** | **Koşu kayıt modeli.** `test_runs` (durum, tetikleyici, trace, iki fingerprint, `runner_ref`, `is_dry_run`, HAR pointer) | Tanım sonradan değişse bile tarih doğru kalmalı; fingerprint ikilisi ortam kaymasını tespit eder | M |
| ~~TM-07~~ | ~~Adım koşum motoru~~ | **İPTAL** — dış runner (ADR-0015) | — |
| **TM-60** | **Runner adapter'ı.** `IWorkflowRunnerPort` + `redocly/cli` süreç sınırı; sabit sürüm, env ile girdi, `--har-output`/`--json-output`, sert timeout | .NET Arazzo runner yok; Respect MIT ve HAR üretiyor | **M** |
| **TM-08** | **Oracle dağıtıcısı.** HAR'ın **her** entry'si → `AssertResponseAsync`; kırmızılar → `DiagnoseAsync`; DB adımı zaten Arazzo içinde | Yalnız kırmızı adımlara bakmak şema kaymasını kaçırır (ADR-0015 §D) | M |
| **TM-09** | **Dayanıklı koşum.** ABP `BackgroundJobs` + idempotent claim (`StartAsync → bool`) + stale `Running` süpürücüsü + kooperatif iptal | Uzun koşu HTTP isteği içinde yaşayamaz; adım seviyesinde devam **yok** | S |
| **TM-10** | **Test verisi sandbox'ı.** `ITestDataSandbox` portu: ayrı ve açıkça yetkilendirilmiş bağlantı, reset stratejisi | Checker hedefe **yazmaz** (ADR-0007); rollback SUT kendi bağlantısını açtığında çalışmaz | M |
| **TM-11** | **Eşzamanlılık.** Aynı ortamda çakışan koşuların sıraya alınması | Paralel iki koşu birbirinin verisini bozar | S |

---

## Blok 2 — An 6: Yargı, kayıt ve rapor

| # | Ne | Neden | Boyut |
|---|---|---|---|
| **TM-06b** | **Hüküm ve bulgu modeli.** `test_run_results` (+`diagnosis_report` jsonb) ve `test_result_findings` (+`source_checker_code`, `rule_ref`) | Üç hakem var; hangi bulgunun kimden geldiği bilinmezse çelişki çözülemez | M |
| **TM-12** | **HAR artefaktı.** ABP BLOB Storing'e yazma, satırda `har_blob_name`; TTL | Ham gövde ilişkisel tabloya konmaz; HAR 1.2 standart ve eksiksiz | S |
| **TM-13** | **Artefakt deposu.** `Ctrf`/`JUnit`/`Sarif`/`Report` çıktıları blob'da, `resource_link` ile verilir | Ağır çıktı modelin ve tablonun dışında kalmalı | S |
| **TM-14** | **CTRF + JUnit dışa aktarımı.** `summary` sayaçları, `tests[]`, `environment` | CTRF diller/araçlar arası JSON standardıdır; `Broken → other` eşlemesi kayıpsız | M |
| **TM-15** | **Saklama.** 90 gün; parçalı silme (partition **yok**); blob TTL; hepsi ABP setting'i | Partition ABP'nin tek kolonlu `Guid` anahtar sözleşmesini kırar (ADR-0016 §H) | S |
| **TM-16** | **Telemetri.** OTel `test.case.result.status`, `test.suite.run.status`; `trace_id` köprüsü | Ayrıntı trace'te, hüküm veritabanında | S |
| **TM-21** | **Teşhis bağlama.** Kırmızı adımda `IDiagnosisAppService.DiagnoseAsync`; RFC 9457 raporu ≤ 4 KB, `diagnosis_report` jsonb'sine yazılır | **Asıl değer burada** — global araçlarda hipotez üreten teşhis motoru yok | M |
| **TM-22** | **Rapor read model'i.** Tek sorguda (findings `Include`) `TestReportDetailDto`; liste ucu findings ve `diagnosis_report` projekte etmez | Ayrı `reports` tablosu yok; ağır kolon liste sorgusuna girmez | S |

---

## Blok 3 — An 2-3-4: Yazarlık ve kapılar

| # | Ne | Neden | Boyut |
|---|---|---|---|
| **TM-17** | **Türetilebilirlik kapısı.** `ValidateScenarioAssertionsAsync` + `assertion_count > 0` + `DescribeTableAsync` ile anahtarın PK/unique kontrolü | **RULE-0006.** Ajan geri bildiriminin %70-77'si `print`; ilişkisel assertion %3-8 | **M** |
| **TM-18** | **Kuru koşum.** `scenario.dryRun`: `is_dry_run = true`, sağlık hesabına girmez; **kırmızıysa ajana çelişki bildirimi döner** | RULE-0005 — düzeltme sözleşmeye karşıdır, gözleme karşı değil | M |
| **TM-19** | **Onay akışı — Kapandı (KBP-92).** `Draft → PendingApproval → Published`; `approval_bound_to_hash` | Onay içeriğe bağlı; belge değişirse uygulama reddedilir | S |
| **TM-20** | **Ajan profilleri ve tool bütçesi.** An bazında izinli tool alt kümesi, `maxTurns`, token tavanı; **kademe 4 tool'u katalogda yok** | Tüm tool'lar aynı anda bağlamda durursa seçim doğruluğu düşer (RULE-0005) | S |
| **TM-31** | **MCP Tasks eşlemesi.** `taskId` + `ttlMs` + `pollIntervalMs`; `working/input_required/completed/failed/cancelled`; onay `input_required` ile | Durum sözlüğümüz protokole kayıpsız oturur | M |

---

## Blok 4 — Bakım ve etki

| # | Ne | Neden | Boyut |
|---|---|---|---|
| **TM-22b** | **Adım adres indeksi.** `compiled_document`'tan türetilir; `operation_fingerprint` + `db_target_fingerprint` | *"Bu bulgu hangi senaryoyu bozar"* join olmalı, JSON taraması değil. **Ölçülene kadar açılmaz** | M |
| **TM-23** | **Etki analizi.** Bulgu parmak izi ↔ senaryo sürümü + adım; `impact_state` | Pact Broker matrix'inin karşılığı. **TM-22b'ye bağlı, ertelendi** | L |
| **TM-24** | **Yama önerisi = Overlay dokümanı.** `finding_fingerprint` **NOT NULL**; uygulama kademe 4 | Gerekçesiz yama yok; Overlay incelenebilir (RULE-0005) | L |
| **TM-25** | **`Healed` etiketi.** Onarılmış senaryonun ilk yeşil koşusu raporda işaretlenir | Sessiz onarım gerçek hatayı gizler | S |
| **TM-26** | **Sözleşme değişikliği tetikleyicisi.** `New` + `Breaking` bulgu → etkilenen senaryolar → `trigger_kind = ContractChange` | **KAPANDI (KBP-110)** — `ContractChangeTriggerHandler` + `ContractChangeImpactManager`; eşleme snapshot seviyesinde, kaba ve bilinçli | M |

---

## Blok 5 — Sağlık ve operasyon

| # | Ne | Neden | Boyut |
|---|---|---|---|
| **TM-27** | **Senaryo sağlığı — materialized view.** `history_id` + `is_dry_run` + `attempt` üzerinden pass/fail/flaky oranı, p95 | **KAPANDI (KBP-110)** — `test_run.scenario_health` view, `percentile_cont(0.95)` SQL'de, `CONCURRENTLY` yenileme gerçek PostgreSQL'de doğrulandı | S |
| **TM-28** | **Karantina politikası.** `breaks_build = false` olan outcome; süre zorunlu | **KAPANDI (KBP-110)** — süpürücü `ExpiredQuarantineSweepWorker` ile süresi dolan karantina elle müdahale olmadan temizleniyor | S |
| **TM-29** | **Zamanlama ve tetikleyiciler.** `schedule_cron`, webhook, manuel | **KAPANDI (KBP-110)** — cron (`Cronos`) + `DueScenarioRunWorker` + idempotent, sırlı webhook ucu; dördü de tek `AutomatedRunTriggerManager`'dan doğuyor | S |
| **TM-30** | **SARIF dışa aktarımı.** Bulgu + severity → `results[]` + `partialFingerprints` | Checker bulgusunun CI/kod tarama yüzeyine taşınmasının standart yolu | S |
| **TM-61** | **Kapsam raporu.** `compiled_document` + `spec_snapshot_id` → operasyon kapsamı; `rule_ref` → kural kapsamı | **PAY KAPANDI (KBP-110). PAYDA SAĞLAYICI HAZIR (KBP-630), BAĞLANTI AÇIK (KBP-111 Dilim 8):** API Contract Checker `alpha.7`, sayfalı snapshot operasyon envanterini public sunuyor ve Test Module aynı sürüme pinli. Coverage henüz bu AppService'i çağırmadığı için `DenominatorState = Unknown` dönmeye devam ediyor | S |

---

## Blok 6 — Köprü katmanı ve token ekonomisi

Gerekçe: [[90-Inbox/RESEARCH-0007-Test-Module-Kopru-Katmani-Ve-Token-Ekonomisi|RESEARCH-0007]].
Bu blok checker'ları değiştirmez; **ajan yüzeyini** değiştirir. TM-32..TM-40 **aynen geçerlidir**:
dar varsayılan yanıt + `outputSchema`, iş-şekilli `scenario.draft`, tek ajan sözlüğü, token
bütçeli sayfalama, TSV tablo verisi, öğreten hata, handle deseni, karar döndüren özet,
token telemetrisi ve CI bütçe kapısı.

---

## Blok 7 — İş senaryosu yetenekleri

Gerekçe: [[90-Inbox/RESEARCH-0009-Is-Senaryosu-Testi-Kosullu-Akis-Ve-Is-Oracle-i|RESEARCH-0009]].
TM-41..TM-50 **kapsam olarak geçerlidir**, iki düzeltmeyle:

- **TM-41 koşullu akış** için ayrı `x-checknexus-branch` uzantısına gerek yok — Arazzo'nun
  kendi `onSuccess`/`onFailure` + `criteria` + `goto` mekanizması bunu zaten veriyor.
  Seçilen dal `test_run_results.taken_branch_path`'e yazılır.
- **TM-46 iş sözlüğü** tablo değil; `kurallar.md` içinde ve MCP `Resource` olarak sunulur.

---

## Blok 8 — İş bilgisi katmanı — **ertelendi**

Gerekçe: [[90-Inbox/RESEARCH-0010-Is-Bilgisinin-Ajana-Aktarimi|RESEARCH-0010]].

TM-51..TM-59 **v1'de tablo olarak açılmaz.** İş bilgisi Git'te durur ve MCP `Resource`
primitive'i ile sunulur; koşuda yalnız `rules_fingerprint` kaydedilir (ADR-0014 §A).
Ajanın erişimi sorgulanabilir olmalı diye ölçülmüş bir ihtiyaç doğarsa ayrı ADR ile açılır.

**TM-56 (`knowledge_contents` tablosu) iptal edilmiştir** — Git zaten sürümleme, içerik
adresleme ve erişim kontrolü veriyor.

---

## Sıra ve gerekçesi

1. **Blok 0** — veri modeli ve şema. Yanlış kurulursa her blok migration borcu yaratır.
2. **Blok 1** — **model olmadan** çalışan dikey dilim. Kabul ölçütü: elle yazılmış Arazzo
   dokümanı runner ile uçtan uca koşar ve **tek satır model çağrısı yoktur**.
3. **Blok 2** — yargı, kayıt ve teşhis. Ürünün asıl değeri burada görünür.
4. **Blok 3** — yazarlık ajanı. Ancak koşum ve yargı kanıtlandıktan sonra anlamlı.
5. **Blok 4** — bakım ve etki.
6. **Blok 5** — kararlılık ve operasyon.

---

## Kapsam dışı (bilinçli hayır)

| Ne | Neden |
|---|---|
| **Kendi Arazzo runner'ımız** | .NET karşılığı yok, Respect MIT; jenerik HTTP motoru rekabet avantajı değil (ADR-0015) |
| Runner'ı fork'layıp plugin eklemek | DB oracle zaten HTTP; adım olarak çağrılır (ADR-0015 §C) |
| XPath criteria | Respect desteklemiyor; yayın kapısında engellenir |
| Temporal veya ayrı orkestrasyon kümesi | İkinci durum sahibi; ABP background job yeterli |
| ML tabanlı test seçimi | Seçim problemimiz parmak izi join'i; ML açıklanabilirliği düşürür |
| Ham log deposu (ClickHouse/OpenSearch) | Ayrıntı trace'te, hüküm veritabanında |
| SUT'un OpenAPI'sinden otomatik MCP tool üretimi | Endpoint sayısı kadar tool; bütçeyi tek başına tüketir (ADR-0008) |
| Kendi senaryo DSL'imiz | Arazzo + Overlay standarttır |
| Checker'a yazma yetkisi | ADR-0007 salt-okunur invariant'ı |
| **Model tabanlı oracle / LLM hakem** | RULE-0005 — kırılgan ve açıklanamaz; oracle deterministik kalır |
| Yük/performans testi | `duration_ms` regresyon sinyali verir, k6 yerine geçmez |
