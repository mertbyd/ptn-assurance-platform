---
id: PLAN-0001
type: plan
status: draft
title: Database Checker eklenecek ozellikler — tek liste
updated: 2026-08-12
decision_refs:
  - ADR-0002
  - ADR-0005
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0003
  - RULE-0004
---

# Database Checker'a eklenecek özellikler

Bu liste [[90-Inbox/RESEARCH-0001-DatabaseChecker-Genisletme-Analizi|RESEARCH-0001]] (E-01..E-14),
[[90-Inbox/RESEARCH-0002-DbChecker-Motor-Yetenek-Haritasi|RESEARCH-0002]] (M-00..M-16) ve
[[90-Inbox/RESEARCH-0003-MCP-Senaryo-Testi-Mimarisi|RESEARCH-0003]] (S-01..S-07) belgelerinin
**tek, tekrarsız ve sıralı** özetidir. Araştırma gerekçesi orada, **yapılacak iş burada**.

Bundan sonra tek numaralandırma geçerlidir: **DBC-xx**. Eski kodlar "kaynak" sütununda izlenebilir.

**Boyut:** S ≈ 1–3 gün · M ≈ 1–2 hafta · L ≈ 2–4 hafta (tek geliştirici, test dahil).

## Kaynak gerçeği durum notu — 2026-08-12

Bu Inbox planı kanonik Current/Rules/ADR sayfalarının yerine geçmez. Kaynak doğrulaması:

| Madde | Durum | Kaynak kanıtı |
|---|---|---|
| DBC-01 | Tamamlandı | Provider type mapper'ları ve `CanonicalDataType` karşılaştırması |
| DBC-02 | Tamamlandı | `ValueRetentionPolicyResolver`, `FindingValueRedactor` |
| DBC-03/04 | Tamamlandı | TLS/timeout/read-only profil kodu, privilege probe ve `PACKAGE-README` grant sözleşmesi |
| DBC-05 | Tamamlandı | `IDatabaseAssertionAppService` row/count/absent/batch yüzeyi |
| DBC-06 | Kısmi | Polling, matcher, outcome, `ObservedAtMs`/`AttemptCount` ve batch tavanı mevcut; mantıksal `ConnectionRef` consumer sorumluluğunda kaldı |
| DBC-07 | Kısmi | `DescribeTableAsync` ve bir seviye FK komşuluğu mevcut; öneri yüzeyinin kalan kısmı açık |
| DBC-08 | Tamamlandı | `DifferenceSeverityClassifier` ve dört değerli severity kodları |
| DBC-09 | Kısmi | Kararlı fingerprint, typed address, `SinceRunId` ve bounded fingerprint filtreleri tamamlandı; kalıcı suppression kaydı açık |

Tablodaki kısmi maddeler tamamlandı sayılmaz; aşağıdaki özgün kapsam kalan işi korur.

---

## Blok 0 — Borç kapatma (özellik değil; bunlar bitmeden yeni özellik yok)

| # | Ne | Neden | Dokunulan yer | Kaynak | Boyut |
|---|---|---|---|---|---|
| **DBC-01** | **Kanonik tip haritası.** `CanonicalDataType`'ı gerçekten doldur: tip ailesi + genişlik + hassasiyet + işaret; eşleşmeyen tipe `Unsupported` | Bugün alan hiç doldurulmuyor, motor `RawDataType`'a düşüyor ama kıyasa `Canonical` damgası basıyor → **PG↔MSSQL modunda her kolon `Modified`** | `PostgreSqlDatabaseSchemaDiscoveryRepository.cs:837`, `SqlServerDatabaseSchemaDiscoveryRepository.cs:692`, `SchemaComparisonManager.cs:490` | M-01 | M |
| **DBC-02** | **Bulgu redaction politikası.** `ValueRetentionMode`: `None`(varsayılan) / `Hashed` / `Masked` / `Full`; `Full` ayrı izin + TTL ister | `DataValueDifference.SourceValue/TargetValue` ve `PrimaryKeyValue` ham müşteri verisi olarak `findings` jsonb'sine yazılıyor | `ComparisonRunConfiguration.cs:50-62`, `DataValueDifferenceDto.cs`, `TableDataComparisonManager` | E-05 | M |
| **DBC-03** | **Bağlantı güvenlik profili.** `TlsMode`, `TrustServerCertificate` (varsayılan **false**), `StatementTimeoutMs`, `LockTimeoutMs`, `ReadOnlyTransaction`(varsayılan **true**), `ApplicationName` | `TrustServerCertificate=true` sabit (sertifika doğrulaması kapalı), `SslMode.Prefer` sessizce plaintext'e düşebilir, hedefte statement/lock timeout yok, sorgu sahipsiz | `DatabaseConnectionStringFactory.cs:16-41`, `DatabaseConnectionInfo`, `DatabaseConnection` entity | E-07 | S |
| **DBC-04** | **En az yetki sözleşmesi.** Motor başına GRANT bloğu + `SchemaOnly`/`DataCompare` profilleri; bağlantı testi fazla yetkiyi **bulgu** olarak raporlar | Müşteri bugün `db_owner`/superuser veriyor; DBC-03'ün riskini çarpıyor | `PACKAGE-README.md`, `IDatabaseConnectionTester` | E-08 | S |

---

## Blok 1 — Test Module + MCP köprüsü (en yüksek iş değeri)

| # | Ne | Neden | Dokunulan yer | Kaynak | Boyut |
|---|---|---|---|---|---|
| **DBC-05** | **Assertion API.** Yeni `IDatabaseAssertionAppService`: `AssertRowAsync`, `AssertCountAsync`, `AssertAbsentAsync`. **Yalnız anahtarla**, serbest SQL yok, tablo/kolon katalogdan doğrulanır, salt okuma | Senaryo testinin her adımı bunu çağıracak. Alternatifi tam karşılaştırma (50–500 KB, saniyeler) veya runner'ın kendi SQL'i (paket sınırı ihlali + enjeksiyon) | Yeni `Services/Assertions/*`, `Managers/Assertions/*`, mevcut `ReadTableStructuresAsync` yeniden kullanılır | E-09 | **L** |
| **DBC-06** | **Assertion ayrıntıları.** Kardinalite (`exactly N`/`atLeast N`/`none`); sunucu tarafında sınırlı polling (`TimeoutMs`+`PollIntervalMs`, üst sınırlı); `ObservedAtMs`; okuma tutarlılığı seçeneği; mantıksal `ConnectionRef`; matcher sözlüğü (`equals, notEquals, isNull, isNotNull, greaterThan, matchesRegex, oneOf, withinTolerance`); redaction varsayılan `None` | Senaryo adımının tek çağrıda deterministik sonuç vermesi; flaky teşhisi; replica gecikmesinin yanlış "yok" üretmemesi | DBC-05 ile aynı dosyalar | S-01..S-07 | M |
| **DBC-07** | **Bilgi yüzeyi (yalnız yazım anı).** `DescribeTableAsync` (kolonlar + PK + unique + 1 seviye FK komşusu), `SuggestBindingsAsync` (operasyon ↔ tablo eşleme önerisi) | Ajan senaryoyu yazarken tam snapshot okumasın; token bütçesi burada belirlenir | `ISchemaDiscoveryAppService` genişletmesi | RESEARCH-0003 §6.1 | M |

> **Sınır kararı:** Checker **yazmaz.** Test verisi seed/cleanup Test Module'ün `ITestDataSandbox` portunda,
> ayrı ve açıkça yetkilendirilmiş bağlantıyla yaşar. Salt-okunur kimlik, güvenlik modelinin ana taşıdır.

---

## Blok 2 — Bulgu kalitesi (MCP "bakım anı"nın ön şartı)

| # | Ne | Neden | Kaynak | Boyut |
|---|---|---|---|---|
| **DBC-08** | **Şiddet sınıflandırıcı.** `DatabaseDifferenceSeverityClassifier` — tek karar noktası, api-contract'takiyle aynı şekil. `Breaking / NonBreaking / Warning / DocsOnly`. `ComparisonDefinition`'a **rol** alanı (`Reference` / `Audited`) — yön asimetrisi olmadan şiddet hesaplanamaz | "34 fark var" karar verdirmez. Atlas DS/MF/BC kodları + oasdiff ERR/WARN disiplini referans | E-03, M-07 | M |
| **DBC-09** | **Bulgu fingerprint'i.** `SHA256(engine_pair|schema|object_type|object_name|child_name|kind|normalized_delta)` + `New`/`Known`/`Resolved` kovaları + `SuppressedFindings` (fingerprint + gerekçe + kim + TTL) | Fingerprint yoksa baseline, susturma ve "bu yeni mi?" ayrımı **mümkün değil**. GitHub code scanning `partialFingerprints` deseni | E-02 | M |
| **DBC-10** | **Şema fingerprint'i (Merkle).** `column_fp → table_fp → schema_fp → snapshot_fp`; kalıcı yazılan yalnız hash'ler (run başına birkaç yüz bayt) | Şema saklamadan drift; `schema_fp` eşitse o dalın karşılaştırması **hiç çalışmaz**; hangi tablonun değiştiği tam diff yapmadan bulunur. Flyway `checksum` / Liquibase `MD5SUM` deseni | E-01 | M |

---

## Blok 3 — Motor genişliği

| # | Ne | Neden | Kaynak | Boyut |
|---|---|---|---|---|
| **DBC-11** | **Nesne türü kaydı + yetenek matrisi + `Unsupported` bulgu türü.** `ISchemaObjectProvider` (motor×tür başına okuma) + `ISchemaObjectComparer` (tür başına kıyas); `SchemaComparisonManager` generic sürücüye dönüşür; `GET /capabilities` yayınlanır | Yeni nesne türü = **2 sınıf, 0 düzenleme**. Ve "fark yok" cümlesi ancak neye bakılmadığı söylenince dürüst | M-00 | **L** |
| **DBC-12** | **Kısıt güvenilirliği.** `IsValidated` (PG `pg_constraint.convalidated`), `IsTrusted` (`sys.foreign_keys/check_constraints.is_not_trusted`), `IsEnabled`/`is_disabled`, `IsDeferrable`/`InitiallyDeferred` | Bugün "kısıt iki ortamda da var" cümlesi yanıltıcı: `NOT VALID`/`is_not_trusted` kısıt mevcut veriyi doğrulamaz ve SQL Server onu sorgu planında kullanmaz. **En tehlikeli sessiz fark** | M-02 | M |
| **DBC-13** | **Collation.** Kolon collation'ı + veritabanı collation'ı + **collation sürüm sapması** (`pg_database.datcollversion` / `pg_collation.collversion` ↔ OS sürümü); sapmada etkilenen metin index listesi | glibc 2.28 olayı: index'ler sessizce bozuldu, unique kısıtlar güvenilmez oldu. Redgate/Liquibase/Bytebase diff'i bunu bulmaz — çünkü diff değil, sağlık bulgusu | M-03 | M |
| **DBC-14** | **Yeni nesne türleri (A1).** `MaterializedView`, `Partition`(+`PartitionScheme`/`PartitionFunction`), `Comment`/`ExtendedProperty`, `Domain`, `Synonym` | Redgate 30+ nesne türü sayıyor; partition farkı sessiz veri kaybı riski; comment `DocsOnly` şiddetinin karşılığı | M-04 | M |
| **DBC-15** | **Kolon derinliği.** `Collation`(DBC-13 ile), `IsGenerated`+`GenerationExpression`, `IsPersisted`, `IdentitySeed`/`IdentityIncrement`, `Comment`, `IsSparse`/`Compression`/`Storage`, `MaskingFunction` | `SchemaColumnModel` bugün 10 alan taşıyor; hesaplanan kolon sıradan kolon gibi görünüyor, ifade farkı görünmüyor | M-05 | S |
| **DBC-16** | **Güvenlik nesneleri (A2).** `Policy` (RLS), `Role`/`Grant`, `Owner` | Atlas'ın açık farkı: "schema drift, **permission mismatches**, policy violations". Şema aynı görünürken erişim farklı olabilir = veri sızıntısı | M-09 | M |
| **DBC-17** | **Karşılaştırma profilleri.** `Strict` / `Deployment`(varsayılan) / `CrossEngine` / `Security` / `Custom`; normalizasyon kararları profil parametresi olur; kural bazında yükselt/düşür/kapat | 40 nesne türü okuyan motor profil olmadan gürültüden kullanılamaz. Bytebase severity yapılandırması + oasdiff severity dosyası deseni | M-06 | M |

---

## Blok 4 — Veri karşılaştırma motoru

| # | Ne | Neden | Kaynak | Boyut |
|---|---|---|---|---|
| **DBC-18** | **Chunked checksum + bisection.** Kademe 0 `COUNT(*)` → Kademe 1 **DB içinde** tablo hash'i → Kademe 2 PK aralığına göre segment hash'i + özyinelemeli bölme → Kademe 3 eşik altındaki segmentin satırları çekilir (mevcut `TableDataComparisonManager` **aynen** çalışır). `MaxRowsPerTable` artık "hata fırlat" değil "bisection'a geç" eşiği | Bugün seçili tablonun **tüm satırları** `to_jsonb(row)::text` ile app process'ine çekiliyor; 100k üstünde `RowLimitExceeded` **fırlatıyor**. data-diff bisection + pt-table-checksum adaptif chunk deseni | E-04, M-11 | **L** |
| **DBC-19** | **Tip-farkında değer karşılaştırma.** Kolon tipi başına `ValueComparisonPolicy`: sayısal tolerans/ölçek normalizasyonu, zaman damgası hassasiyeti kırpma, timezone → UTC, metin trim/case/collation-farkında, JSON **semantik** eşitlik, binary hash. Kanonik satır metni bu politikadan **sonra** üretilir | Bugün her hücre metne çevrilip kıyaslanıyor → `12.34` vs `12.340`, kayan nokta gösterimi, kesirli saniye hassasiyeti, `char` dolgusu **sahte fark** üretiyor | M-10 | M |
| **DBC-20** | **Filtre / pencere / örnekleme.** `WHERE` zaman penceresi, kolon alt kümesi ve dışlama (`updated_at`, `row_version`), PK aralığı, `Sample` modu (`TABLESAMPLE SYSTEM`/`BERNOULLI`). **Örnekleme sonucu asla "eşit" raporlanmaz** → `SampledMatch(confidence=n%)` | Nightly kıyası dakikalara indirir; doğası gereği farklı kolonlar gürültü üretmez | M-10 | S |
| **DBC-21** | **Ucuz ön kontrol.** `pg_class.reltuples` / `sys.dm_db_partition_stats` ile tahmini sayım; kesin `COUNT(*)` yalnız gerekince | Bugün her zaman kesin sayım yapılıyor | M-10 | S |

---

## Blok 5 — Analiz (motoru "akıllı" yapan katman)

| # | Ne | Neden | Kaynak | Boyut |
|---|---|---|---|---|
| **DBC-22** | **Şema lint kataloğu.** `LNT-101` PK'sız tablo · `LNT-102` FK'da index yok · `LNT-103` doğrulanmamış kısıt · `LNT-104` redundant index · `LNT-105` unique içinde nullable · `LNT-106` karışık collation · `LNT-107` devre dışı trigger/kısıt · `LNT-108` isimlendirme ihlali · `LNT-109` yetim satır | Aynı katalog okuyucuları "tek DB sağlıklı mı?" sorusunu bedavaya cevaplar. Ürün "iki ortamı kıyaslayan araç"tan **güvence platformu**na döner; tek bağlantısı olan müşteri de kullanır. SchemaCrawler lint ailesi referans | M-08 | M |
| **DBC-23** | **Bağımlılık grafiği + etki analizi.** FK / view→tablo / trigger→tablo / tip→kolon DAG'ı; bulgular topolojik sırada; "bu farkı kapatmak şu 3 nesneyi etkiler" | Bugün bulgular alfabetik sıralı. Redgate "dependencies in the right order" sunuyor. **SQL üretmiyoruz** (bilinçli sınır) ama etki analizi checker'ın işi | M-12 | M |
| **DBC-24** | **Rename önerisi.** Aynı tabloda bire bir eşleşen drop+add çifti (aynı tip/nullable/default, yakın ordinal) → `PossibleRename(confidence=Heuristic)`; **ham bulgular raporda kalır** | Rename ile DROP+ADD şemadan tam ayırt edilemez; Atlas kullanıcıya sorar, otomatik karar vermez. Biz de öneri veririz, karar vermeyiz | M-12 | S |

---

## Blok 6 — Entegrasyon ve operasyon

| # | Ne | Neden | Kaynak | Boyut |
|---|---|---|---|---|
| **DBC-25** | **Dışa aktarım.** `ReportFormatCodes`'a `Sarif` + `JUnit`/`CTRF`. Fingerprint → SARIF `partialFingerprints`, severity → `level`, `DifferenceKindCodes` → `ruleId`, `logicalLocations` (şema.tablo.kolon) | CI'a, GitHub code scanning'e, test raporlayıcısına bağlanma. Lookup altyapısı zaten hazır | E-11 | S |
| **DBC-26** | **Zamanlanmış drift izleme.** `ComparisonDefinition`'a `MonitoringEnabled` + `CheckIntervalMinutes`; worker `SchemaOnly` koşar, **yalnız DBC-10 fingerprint'i değiştiyse** tam karşılaştırma tetikler. Tetikleme sahibi composition host | api-contract'ta `ScheduledSpecDocumentCheckManager` var, DB'de yok — gereksiz asimetri. Bytebase/Atlas sürekli izleme deseni | E-10 | M |
| **DBC-27** | **Operasyonel olgunluk.** İptal (token'lar repository'de **zaten var**, job'a bağlanacak), ilerleme raporu, nesne türü/şema bazında checkpoint, paralellik + adaptif backpressure, **bulgu sayfalama** (bugün tek jsonb) | 40 dk'lık run 39. dk'da patlamamalı; canlı DB yorulmamalı; 50.000 bulgu tek JSON'a sığmaz | M-14 | M |
| **DBC-28** | **Drift atıfı (opsiyonel sağlayıcı).** PG event trigger/pgaudit log'u veya SQL Server default trace okunur → `AttributedTo`/`AttributedAt`/`AttributionConfidence`. Sağlayıcı yoksa alan boş kalır; **checker hedef DB'ye kurulum yapmaz** | "Kim, ne zaman" sorusunu kutudan cevaplayan açık kaynak diff aracı yok | M-13 | M |
| **DBC-29** | **Gözlemlenebilirlik.** `ActivitySource` span'ları: `checknexus.db.discovery.read` / `compare.schema` / `compare.data` / `assert.row`; öznitelikler OTel `db.system.name`, `db.namespace`, `checknexus.run.id`. **Yasak:** hücre değeri, host, kullanıcı adı, secret path | Run'ın nerede yavaşladığı ölçülemiyor | E-14 | S |
| **DBC-30** | **Paket kalite kapıları.** `EnablePackageValidation` + `PackageValidationBaselineVersion`, `ContinuousIntegrationBuild`, SourceLink, `.snupkg`; ABP sürüm aralığı kararı (ADR) | `0.2.0` ikili kırıcı değişikliği **derlemede** patlasın; ABP 10.3 ↔ 10.5/10.6 sürüm grafiği riski açık | E-13 | S |
| **DBC-31** | **Differential oracle + Testcontainers matrisi.** `migra`/`atlas schema diff` (PG), `SqlPackage /Action:DeployReport` (MSSQL) oracle olarak; gerçek PG + gerçek MSSQL container'ı; `accepted-deviations.json` formatı api-contract'takiyle **birebir aynı** (`deliberate` / `known-gap`) | Kendi kendini doğrulayan motor kör noktasını göremez. Desen evin içinde zaten var: `.agents/skills/acc-comparison-engine/scripts/oasdiff_oracle.py` | E-12, M-16 | M |
| **DBC-32** | **Yeni motorlar.** MySQL/MariaDB → Oracle → SQLite | Tek motorun çok DB'yi tutarlı bulgu modeliyle kapsaması piyasada başlı başına fark (Redgate her DB için ayrı ürün satıyor). **Şart:** DBC-01 ve DBC-11 bitmiş olmalı | M-15 | **L** |

---

## Sıra ve gerekçesi

```text
Dalga 1  (borç + köprü)     DBC-01 → 02 → 03 → 04 → 05 → 06 → 07
Dalga 2  (bulgu kalitesi)   DBC-08 → 09 → 10
Dalga 3  (motor genişliği)  DBC-11 → 12 → 13 → 14 → 15 → 17 → 16
Dalga 4  (veri motoru)      DBC-18 → 19 → 20 → 21
Dalga 5  (analiz)           DBC-22 → 23 → 24
Dalga 6  (operasyon)        DBC-25 → 26 → 27 → 28 → 29
Sürekli                     DBC-30, DBC-31  (her dalgada koşar)
En son                      DBC-32
```

**Neden bu sıra:**

1. **DBC-01/02/03** özellik değil borçtur; müşteri ortamına girdikten sonra geri alınması pahalıdır.
2. **DBC-05** Test Module ve MCP'nin ön şartıdır; o olmadan senaryo testi mimarisi kurulamaz.
3. **DBC-08/09/10** MCP "bakım anı"nın ön şartıdır — az-token tester'ın asıl mekanizması onlara dayanır.
4. **DBC-11** bundan sonraki her genişlemenin altyapısıdır; ondan önce nesne türü eklemek borç üretir.
5. **DBC-32** en sonda: DBC-01 ve DBC-11 bitmeden eklenen her yeni motor, cross-engine hatasını tekrar üretir.

## Kapsam dışı (bilinçli hayır)

| Öneri | Neden hayır |
|---|---|
| Şema snapshot'ını tabloya yazmak | Eskir, şişer, müşteri iç yapısını taşır. DBC-10 fingerprint aynı yeteneği verir |
| Farkı kapatacak SQL üretmek | Checker bilgi motorudur, eylem motoru değil (ARCH-0001) |
| Checker'a yazma yetkisi / test verisi seed-cleanup | Salt-okunur kimlik güvenlik modelinin ana taşı; `ITestDataSandbox` Test Module'de |
| MCP'de serbest SQL tool'u | Bilinen ve gerçekleşmiş veri sızıntısı deseni |
| `data-diff` paketini bağımlılık almak | Upstream 17 Mayıs 2024'te arşivlendi; **algoritma** alınır, paket alınmaz |
| Checker'a bildirim/e-posta | RULE-0004 + ADR-0002; Notifications ayrı capability |
