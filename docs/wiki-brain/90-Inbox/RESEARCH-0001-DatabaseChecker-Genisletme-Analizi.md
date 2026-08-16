---
id: RESEARCH-0001
type: research
status: draft
title: Database Checker genisletme analizi ve kanitli global karsilastirma
updated: 2026-08-12
decision_refs:
  - ADR-0002
  - ADR-0003
  - ADR-0005
  - ADR-0006
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0003
  - RULE-0004
---

# Database Checker genişletme analizi

> **Bu belge kanonik bilgi değildir.** `90-Inbox` altındadır: öneri ve kanıt taşır, karar taşımaz.
> Bir madde kabul edilirse ilgili `01-Current` / `02-Rules` / `03-Decisions` sayfası aynı iş içinde
> güncellenir ve madde buradan kaldırılır (bkz. [[00-Home]] "Wiki kuralı").
>
> **Kimler için:** (a) projeyi hiç bilmeyen yeni ekip üyesi — Bölüm 1–3 yeterli;
> (b) `database-comparison` üzerinde çalışacak geliştirici — Bölüm 2, 5;
> (c) MCP/Test Module tarafını kuracak kişi — Bölüm 6.

---

## 0. Yöntem ve kanıt disiplini

Bu belgedeki her iddia üç sınıftan birine dayanır. Kaynak listesi Bölüm 8'dedir.

| Kanıt sınıfı | Anlamı | Nasıl gösterilir |
|---|---|---|
| **K1 — Yerel kod** | Bu workspace'teki çalışan koddan doğrudan okundu | `dosya:satır` |
| **K2 — Birincil dış kaynak** | Spec, resmî dokümantasyon, resmî registry, vendor doc | URL + erişim tarihi |
| **K3 — İkincil kaynak** | Blog, pratisyen raporu, ölçüm iddiası | URL + "ölçüm iddiası" etiketi |

K3 kaynaklar **karar gerekçesi** değildir; yön göstergesidir. Bu ayrım [[05-Operations/Source-Registry|SOURCE-0001]]
"Kaynak politikası" maddesinin aynısıdır ve burada da geçerlidir.

Araştırma tarihi: **2026-08-12**. Bütün dış kaynaklar bu tarihte erişildi.

---

## 1. Projeyi hiç bilmeyen biri için: bu nedir?

### 1.1 Tek cümle

**PTN Assurance Platform**, bir yazılım ekibinin "sürüm çıkmadan önce gerçekten kırılan bir şey var mı?"
sorusunu insan gözüne bırakmadan cevaplayan bir *güvence (assurance)* platformudur. Bugün iki bilgi
motoru vardır — **API sözleşmesi** ve **veritabanı** — ve bunlar bilinçli olarak birbirinden bağımsız
NuGet paketleri olarak yazılmıştır.

### 1.2 Fiziksel gerçek

```text
ptn-assurance-platform/
  checkers/
    api-contract/          -> CheckNexus.ApiContracts*        (8 paket, 0.1.0-alpha.5)
    database-comparison/   -> CheckNexus.DatabaseComparison*  (8 paket, 0.1.0-alpha.5)
  vault/                   -> CheckNexus.Vault                (source 0.1.0-alpha.5, NuGet'te public değil)
  docs/wiki-brain/         -> bu wiki
```

Bu klasörde **ana ürün hostu yoktur**. Burada üretilen şey *paket*tir. Paketleri tüketen "Test Module"
composition host'u ayrı bir iştir ve henüz bu workspace'te değildir ([[01-Current/Platform-Truth|CURRENT-0001]]).

**K2 doğrulama:** `CheckNexus.DatabaseComparison` NuGet.org'da tek sürümle listelidir: `0.1.0-alpha.5`,
yayın tarihi 11 Ağustos 2026, sahip `mertbyd`, hedef `net10.0`, lisans MIT; bağımlılıkları
`.Application`, `.EntityFrameworkCore`, `.HttpApi` paketleridir.

### 1.3 Kavram sözlüğü (bunu okumadan koda girme)

| Terim | Bu projede ne demek |
|---|---|
| **Checker** | Kendi başına bilgi üreten, auth sahibi olmayan bir ABP capability modülü. Karar vermez, *bulgu* üretir. |
| **Capability module** | Kendi issuer'ı, login'i, kullanıcı tablosu olmayan; kimlik bağlamını host'tan alan modül (RULE-0004). |
| **Composition host** | Modülleri tek process'te birleştirip çalıştıran executable. Auth, DB bağlantısı, migration sırası onun sorumluluğu. |
| **Connection** | Hedef veritabanının *adres defteri* kaydı. Parola burada değil, Vault'ta; DB'de sadece `VaultSecretPath` durur. |
| **Definition** | "Şu bağlantı ile şu bağlantıyı, şu modda karşılaştır" tarifi. Kalıcıdır. |
| **Run** | Bir tarifin fiilen çalıştırılmış hali. Append-only tarih kaydıdır, elle düzeltilmez. |
| **Finding (bulgu)** | Tek bir fark: şema farkı, migration farkı veya veri farkı. |
| **Scope rule** | `Include / Exclude / Ignore / DataCompare` — hangi şema/nesne/kolonun karşılaştırmaya gireceğini daraltan runtime kuralı. |
| **Confidence** | Bulgunun güven seviyesi: aynı motor → `Exact`, çapraz motor (PG↔MSSQL) → `Canonical`. |
| **Engine component** | Motor-özel (PostgreSql / SqlServer) okuyucu. Yeni motor = yeni sınıf; çağıran kod değişmez. |

### 1.4 Pazarlıksız dört sınır (ve *neden*)

Bunlar wiki'de RULE-0001..0004 olarak yazılıdır. Yeni gelenin en sık ihlal ettiği yer burasıdır:

1. **Checker auth sahibi değildir.** İki checker aynı üründe iki issuer, iki user tablosu doğurmasın diye.
2. **Host paketlenmez.** `Ptn.DatabaseChecker.HttpApi.Host` sadece geliştirme/doğrulama içindir; production'da
   ikinci runtime owner olmaz.
3. **Şema/migration sahipliği tektir.** Aynı tabloyu iki modül migration'ı yaratamaz.
4. **Secret değeri hiçbir katmana sızmaz.** Port Domain'de, provider composition sınırında.

---

## 2. `database-comparison` paketi — kod seviyesinde gerçek

### 2.1 Katman ve çağrı zinciri

```text
Controller  ->  AppService  ->  Manager (Domain)  ->  Repository (EF Core)  ->  hedef DB katalogu
```

Paket ailesi 8 parçadır; `CheckNexus.DatabaseComparison` meta paketi Application + EntityFrameworkCore + HttpApi
taşır. Ana motor sınıfları:

| Sınıf | Sorumluluk | Kanıt (K1) |
|---|---|---|
| `SchemaComparisonExecutionManager` | Moda göre hangi I/O bloklarının çalışacağına karar verir | `SchemaComparisonExecutionManager.cs:43` |
| `SchemaDiscoveryManager` | Secret çöz → motor seç → katalog oku | `SchemaDiscoveryManager.cs:40` |
| `SchemaComparisonManager` | İki snapshot'tan **saf** şema farkı üretir (I/O yok) | `SchemaComparisonManager.cs:37` |
| `ComparisonScopeRuleEvaluator` | Include/Exclude/Ignore/DataCompare semantiğinin tek karar merkezi | `ComparisonScopeRuleEvaluator.cs:13` |
| `MigrationComparisonManager` | `__EFMigrationsHistory` defter farkı | `MigrationComparisonManager.cs:13` |
| `TableDataComparisonManager` | PK eşleme + hücre farkı + SHA-256 tablo hash'i | `TableDataComparisonManager.cs:18` |
| `ComparisonRunExecutionManager` | Pending → Running → Completed/Failed yaşam döngüsü | `ComparisonRunExecutionManager.cs:26` |
| `EngineComponentResolver<T>` | Motor kodu → bileşen seçimi (açık/kapalı prensibi) | `EngineComponentResolver.cs:13` |

### 2.2 Ne kalıcı, ne değil — bu paketin en önemli tasarım kararı

Kullanıcının açıkça vurguladığı invariant koda uyuyor ve **doğrulanmıştır**:

**Kalıcı DEĞİL (doğru):**

- Hedef veritabanının şema fotoğrafı (`SchemaSnapshotModel`) hiçbir tabloya yazılmaz. `ReadSnapshotAsync`
  ile okunur, bellekte karşılaştırılır, atılır (`SchemaComparisonExecutionManager.cs:57-59`).
- Scope kuralları `ComparisonDefinition`'a yazılmaz; job argümanı olarak taşınır
  (`ComparisonRunExecutionManager.cs:62`, yorum: *"scope runtime'ini kalici definition alanina yazmadan job isteginden alir"*).
- Parola DB'ye yazılmaz; sadece `VaultSecretPath` durur (`DatabaseConnection.cs:32`).

**Kalıcı (bilinçli):**

- `ComparisonRun` header'ı + özet sayaçlar (denormalize, liste ekranı COUNT atmasın diye).
- `ComparisonRun.Findings` → `run.ComparisonRuns.findings` **jsonb** kolonu (`ComparisonRunConfiguration.cs:50-62`).
- `ComparisonRun.Reports` → `reports` jsonb kolonu.

**Ve burada kod ile ilkenin çeliştiği tek nokta var** — Bölüm 5 / E-05'te ayrıntılı:
`DataValueDifferenceModel.SourceValue` / `TargetValue` ve `DataRowDifferenceModel.PrimaryKeyValue`
**gerçek müşteri hücre değerleridir** ve `findings` jsonb'sine yazılır
(`DataValueDifferenceDto.cs:15-20`, `ComparisonRunConfiguration.cs:59`). Yani "kimsenin DB'sini komple
yazmıyorum" ilkesi şema tarafında %100 tutuyor, **veri farkı tarafında delik**.

### 2.3 Motor akışı — adım adım

```text
ExecuteAsync(definitionId)                      [HTTP, kısa]
  -> PrepareAsync            : tarif + iki bağlantı aktif mi? -> Pending run insert
  -> EnqueueAsync            : ABP background job kuyruğu
                                   |
ComparisonRunExecutionBackgroundJob             [worker]
  -> StartExecutionAsync     : Pending -> Running   (kısa UOW)
  -> BuildExecutionContext   : bağlantı + mod snapshot'ı  (kısa UOW)
  -> ExecuteAsync            : *** UOW YOK ***  uzun dış I/O burada
        SchemaOnly / Both -> iki tarafın tam snapshot'ı -> saf diff -> migration defteri diff
        DataOnly  / Both -> DataCompare işaretli tablolar -> tüm satırlar -> PK/hash diff
  -> CompleteExecutionAsync  : Completed + findings yaz   (kısa UOW)
  -> (hata) FailExecutionAsync : Failed + **house error code** (mesaj değil)
```

İki tasarım kararı burada özellikle iyi ve korunmalı:

- **Uzun I/O açık transaction tutmaz.** Uygulama DB'sinin transaction'ı, hedef DB okunurken açık kalmaz
  (`ComparisonRunExecutionBackgroundJob.cs:47-77`).
- **Hata mesajı sızdırmaz.** `ResolveErrorCode` sadece `BusinessException.Code` taşır; beklenmeyen
  exception'ın mesajı (connection string, secret parçası olabilir) tarihe yazılmaz
  (`ComparisonRunExecutionBackgroundJob.cs:81-84`).

### 2.4 Yanlış-pozitifle savaş: motorun asıl değeri burada

Bir şema diff motorunun kalitesi "farkı bulmak" değil, **olmayan farkı bulmamak**tır. Mevcut motorda
bunun için yazılmış, isimlendirilmiş kararlar var — yeni gelenin silmeye kalkacağı, ama silmemesi gereken yerler:

| Karar | Ne çözüyor | Kanıt |
|---|---|---|
| `CompareColumnOrder` **bağıl** sırayı kıyaslar | Ham `attnum`/`column_id` boşluk taşır; ham kıyas her kolon eklemede sahte fark üretir | `SchemaComparisonManager.cs:126-172` |
| Tanımı da değişmiş kolon `Ordinal` bulgusundan atlanır | Aynı kolon iki kez raporlanmaz | `SchemaComparisonManager.cs:158-161` |
| `BuildIndexProviderDetailDefinition` index adını placeholder'lar | `pg_get_indexdef` metnindeki isim farkı sahte "değişti" üretmesin; expression index'te semantik korunur | `SchemaComparisonManager.cs:277-308` |
| Child scope kararı **simetrik** | Tek tarafta olan kolon, whitelist yüzünden sahte `OnlyInSource` seline dönmez | `ComparisonScopeRuleEvaluator.cs:71-107` |
| PK benzersiz değilse content-hash'e düşülür | Duplicate PK sözlükte ezilip false-negative üretmez | `TableDataComparisonManager.cs:112-118, 224-244` |
| `EncodeValue` NULL'u `"<NULL>"` metninden ayırır | Gerçek metin ile SQL NULL aynı hash'e düşemez (`V{len}:` öneki) | `TableDataComparisonManager.cs:300-306` |
| `Exact` vs `Canonical` confidence | Çapraz motor kıyasında "kesin" iddiası edilmez | `SchemaComparisonManager.cs:429-434` |

Bu liste, motorun olgunluk seviyesini gösteriyor: bunlar bir hafta sonunda yazılmış şeyler değil,
gerçek false-positive'lerden öğrenilmiş kararlar. **Genişletme yaparken bunlara dokunmadan ekleme yapılmalı.**

### 2.5 Bugün ne YOK (boşluk envanteri)

| # | Boşluk | Neden önemli |
|---|---|---|
| G1 | Bulguda **severity/impact** yok; sadece `confidence` var | "13 fark var" cümlesi karar verdirmez. api-contract tarafında `Breaking/NonBreaking/DocsOnly` var, DB tarafında yok |
| G2 | Bulgunun **kararlı kimliği (fingerprint)** yok | "Bu farkı zaten biliyoruz" / "bu yeni çıktı" ayrımı yapılamaz; baseline ve suppression imkânsız |
| G3 | **Beklenen durum** kavramı yok; sadece A↔B canlı kıyas var | Drift ("kimse dokunmadı demişti ama şema değişmiş") tespit edilemez |
| G4 | Veri kıyasında **tüm satırlar** app process'ine çekilir | `MaxRowsPerTable` (varsayılan 100.000) aşılırsa iş **hata verir**, kademeli düşmez |
| G5 | Scope kuralları katalog sorgusuna **push-down** edilmiyor | Önce tüm şema okunuyor, sonra filtreleniyor (`SchemaComparisonManager.cs:43-46`) |
| G6 | Hedef DB'de **read-only transaction / statement_timeout / application_name** yok | Canlı DB'de uzun sorgu ve "bu sorgu kimin?" sorusu kontrolsüz |
| G7 | `TrustServerCertificate = true` ve `SslMode.Prefer` sabit | Sertifika doğrulaması kapalı; PG tarafında sessizce plaintext'e düşebilir (`DatabaseConnectionStringFactory.cs:26,40`) |
| G8 | **Zamanlanmış/sürekli** kontrol yok | api-contract'ta `ScheduledSpecDocumentCheckManager` var, DB'de karşılığı yok |
| G9 | **Hedefli assertion** (find-by-key) API'si yok | Test Module'ün asıl ihtiyacı bu; bugün tek yol tam karşılaştırma |
| G10 | Dış format ihracı yok (SARIF/JUnit) | CI'ya, GitHub'a, test raporlayıcısına bağlanamıyor |
| G11 | Motorun doğruluğu için **differential oracle** yok | api-contract'ta oasdiff karşılaştırması var (`.agents/skills/acc-comparison-engine/scripts/oasdiff_oracle.py`), DB'de yok |
| G12 | Paket kalite kapıları eksik | `PackageValidation` baseline, SourceLink, deterministic build doğrulanmamış |

---

## 3. Bu paket hangi işte kullanılır?

### 3.1 Consumer sözleşmesi (PACKAGE-README'den, K1)

```xml
<PackageReference Include="CheckNexus.DatabaseComparison" Version="0.1.0-alpha.5" />
```

Composition host'un yapması gerekenler:

1. `DatabaseCheckerModule`'ü ABP module grafiğine ekle.
2. `DatabaseChecker` connection string'ini ver (yoksa ABP `Default`'a düşer).
3. `DatabaseCheckerDbContext` migration'larını **kendi** migration akışından uygula.
4. `Ptn.DatabaseChecker.Interface.Secrets.ISecretProvider` implementasyonu kaydet (opsiyonel `CheckNexus.Vault`).
5. Auth, tenant, bildirim, zamanlama ve production hosting'i host sağlar.

### 3.2 Gerçek kullanım senaryoları

| Senaryo | Mod | Kim tetikler |
|---|---|---|
| "Test ile Canlı şemaları aynı mı?" | `SchemaOnly` | Release öncesi, insan |
| "Migration'lar iki ortamda aynı noktada mı?" | `SchemaOnly` (migration bloğu dahil) | CI |
| "Şu referans tablosu (ör. `lookup.Country`) iki ortamda birebir mi?" | `DataOnly` + `DataCompare` scope | Nightly |
| "Günlük tam sağlık kontrolü" | `Both` | Zamanlanmış |
| **"API şu kaydı gerçekten yazdı mı?"** | *bugün yok* — bkz. E-09 | Test Module |

Son satır kritik: **paketin bugünkü şekli Test Module'ün asıl ihtiyacını karşılamıyor.** Bir tester'ın
işi "iki DB'yi kıyaslamak" değil, "yaptığım çağrının izini DB'de doğrulamak"tır. Bölüm 5/E-09 ve Bölüm 6 bunu çözüyor.

---

## 4. Global karşılaştırma — aynı işi yapanlar ne yapıyor?

Bu bölüm "biz ne yapmalıyız"ı değil, **"dünya bu problemi nasıl çözmüş"ü** anlatır. Her satır kanıtlıdır.

### 4.1 Şema diff / drift ailesi

| Proje | Davranışı (kanıt) | Bizim için dersi |
|---|---|---|
| **Atlas (ariga)** — K2 | İki mod: `atlas migrate apply` öncesi **senkron drift check** (son uygulanmış revizyonun *beklenen* şeması ↔ canlı şema) ve ayrıca sürekli, bant-dışı **schema monitoring**. Kritik ayrıntı: *"Atlas doesn't store drift data itself — it compares against the registry on-demand."* Ayrıca `exclude` ile kasıtlı sapmalar (extension, audit tablosu, 3. parti şema) hariç tutulur | Drift için **beklenen durum** referansı şart; ama bu referansı checker'ın kendi DB'sinde şema olarak tutması zorunlu değil. Ve "kasıtlı sapma" birinci sınıf kavram olmalı |
| **Bytebase** — K2 | Arka planda periyodik olarak *kaydedilmiş son şema* ile *canlı şema*yı kıyaslar, farkı **Anomalies** altında gösterir; remediation iki seçenek: **Baseline** (canlıyı doğru kabul et) veya **Revert**. Drift detection Enterprise planında | Drift bulunduğunda "ne yapayım?" sorusunun iki kanonik cevabı vardır. Bulgu üretmek yetmiyor, **kapatma yolu** da ürün yüzeyi |
| **Flyway / Liquibase** — K2 | Flyway `flyway_schema_history` tablosunda **checksum** tutar; migration dosyası uygulandıktan sonra değişirse başlangıçta checksum uyuşmazlığı ile patlar. Liquibase aynısını `DATABASECHANGELOG.MD5SUM` ile yapar; her çalıştırmada yeniden hesaplar ve saklananla kıyaslar. Flyway OSS'te `validate` var, **drift/diff ücretli katmanda**; Liquibase changelog'a karşı diff yapabiliyor | **En değerli ders bu:** bu araçlar migration'ın *içeriğini* değil **parmak izini** saklar. "Şemayı tutmak güvenilir değil ve gereksiz veri" tezinin sektörel karşılığı tam olarak budur |
| **SqlPackage / DacFx `DriftReport`** — K2 | `/Action:DriftReport` → veritabanının *kayıtlı data-tier application* referansından bu yana sapmasının XML raporu | Microsoft da "kayıtlı referans ↔ canlı" modelini kullanıyor. Referans = tam şema kopyası olmak zorunda değil, ama bir referans **gerekiyor** |
| **migra / pgdiff / pg-schema-diff (stripe)** — K2 | Hepsi "iki şemayı kıyasla → **eşitleyecek SQL üret**" yönünde. `pg-schema-diff` özellikle "minimal downtime ve lock" hedefliyor | Bizim motorumuz bilinçli olarak SQL üretmiyor — bu doğru bir sınır (checker eylem motoru değil). Ama **"bu farkı kapatmak ne kadar tehlikeli?"** sorusuna cevap vermek bizim işimiz olabilir |

### 4.2 Veri diff ailesi — "tüm satırı çekme" probleminin çözülmüş hali

| Proje | Davranışı (kanıt) | Bizim için dersi |
|---|---|---|
| **datafold/data-diff** — K2 | Tabloyu `--bisection-factor` (varsayılan 10) segmente böler, **checksum'u veritabanının içinde** hesaplar, eşleşmeyen segmenti aynı faktörle özyinelemeli böler. Doküman cümlesi: *"This keeps the amount of data that has to be transferred between the databases to a minimum"*. Segment `--bisection-threshold`'un altına inince satırları çekip bellekte kıyaslar. Bilinen zaafı: key kolonundaki büyük boşluklar performansı bozar | **Doğru veri diff algoritması budur.** Hepsini çek + bellekte hash'le değil; DB'de checksum + ikili arama |
| **Sunset uyarısı** — K2 | Datafold açık kaynak data-diff'i **17 Mayıs 2024**'te sonlandırdı; CEO gerekçesi: *"maintaining two distinct products with different codebases yet significantly overlapping functionality"*. Topluluk fork'u **reladiff** (erezsh) sürüyor | Bu aileyi bağımlılık olarak almak riskli; **algoritmayı** almak doğru. Kendi motorumuzda uygulanabilir |
| **Percona `pt-table-checksum`** — K2 | "Nibbling" tekniğiyle indeks üzerinden chunk'lar; **chunk hedefi 0,5 saniye**, üstel ağırlıklı hareketli ortalama ile chunk boyutunu ayarlar. Hash seçenekleri: `CRC32, FNV1A_64, MURMUR_HASH, MD5, SHA1` — dokümantasyon CRC32'nin çarpışmaya açık ama ucuz olduğunu açıkça söyler | **Adaptif chunk boyutu** fikri: canlı DB'yi yormamak bir *ayar* değil, algoritmanın parçası |
| **SQL Server tarafı** — K2/K3 | `CHECKSUM`/`CHECKSUM_AGG` 32-bit int döner; 100.000 rastgele değerde çarpışma olasılığı ~%30. Doğru cevap `HASHBYTES` (daha pahalı ama kriptografik) | Bizim motor zaten SHA-256 kullanıyor (`TableDataComparisonManager.cs:309`) — ama **bellekte**. DB tarafına indirirken `CHECKSUM_AGG` cazibesine kapılmamak lazım |

### 4.3 Değişikliği sınıflandırma (severity) ailesi

| Proje | Davranışı (kanıt) | Bizim için dersi |
|---|---|---|
| **Atlas migration analyzers** — K2 | Kapalı, kodlu katalog: **DS101** şema silindi, **DS102** tablo silindi, **DS103** kolon silindi; **MF101** mevcut kolona unique index, **MF103** mevcut tabloya non-nullable kolon, **MF104** nullable→non-nullable; **BC101** tablo yeniden adlandırma, **BC102** kolon yeniden adlandırma; **NM1xx** isimlendirme; **PG101/102** `CONCURRENTLY` eksik | **Kod adı + kategori + sabit anlam.** Bizim `DifferenceKindCodes`'un üstüne bindirilebilecek hazır bir taksonomi |
| **Squawk** — K2 | Postgres migration linter'ı; `ban-drop-column`, `ban-drop-database` gibi kurallar; `--exclude`/`--include` ve `squawk-ignore` yorumu ile kural bazlı susturma | **Susturma birinci sınıf özellik.** Kural varsa istisnası da olmalı, yoksa ekip aracı kapatır |
| **Bytebase SQL Review** — K2 | Anti-pattern, kilitleyen DDL, geriye dönük uyumsuz değişiklik ve isimlendirme ihlalleri; **severity, scope ve engine başına** yapılandırılır | Severity motorun sabiti değil, **politikanın** parçası. Aynı fark bir ortamda `Error`, diğerinde `Warning` olabilir |
| **oasdiff** (bizim api-contract'ın oracle'ı) — K2 | 250+ kontrol, üç seviye: **ERR** kesin kırıcı, **WARN** potansiyel kırıcı, **INFO**. WARN yalnızca *tanım gerçekten bilgi taşımıyorsa* kullanılır — "bazı kurulumda kırar bazısında kırmaz" durumları ERR sayılır. Severity dosya ile kural bazında değiştirilebilir; `--fail-on WARN` çıkış kodunu belirler | **WARN'ın disiplinli tanımı.** "Emin değilim"i WARN'a çöp kutusu yapmamak. DB tarafında aynı disiplin kurulmalı |

### 4.4 Bulgu taşıma ve kimlik

| Standart | Davranışı (kanıt) | Bizim için dersi |
|---|---|---|
| **SARIF 2.1.0 (OASIS)** — K2 | Statik analiz sonuçları için OASIS standardı; GitHub code scanning SARIF 2.1.0'ın bir alt kümesini kabul eder | Bulguyu dışa taşımanın **hazır ve yaygın** formatı var; kendi formatımızı icat etmeye gerek yok |
| **`partialFingerprints`** — K2 | GitHub *"uses the partialFingerprints property ... to detect when two results are logically identical"*; fingerprint kararsızsa her analizde eski alert kapanıp yenisi açılır ve **aynı sorun için birden çok alert** oluşur | G2'nin doğrudan cevabı. Fingerprint yoksa "yeni fark / bilinen fark" ayrımı ve baseline **mümkün değil**, sadece zor değil |

### 4.5 Test/oracle tarafı (köprü için)

| Kaynak | Davranışı (kanıt) | Bizim için dersi |
|---|---|---|
| **Arazzo Specification (OpenAPI Initiative)** — K2 | OpenAPI tek endpoint'in *şeklini* verir, iş akışını vermez. Arazzo, çok adımlı akışı input/output/success-criteria/step-dependency ile makine-okur tanımlar; kullanım alanlarından biri açıkça **senaryo testi otomasyonu** | Test Module'ün `TestPlan` modeli sıfırdan icat edilmemeli; Arazzo hazır bir gramer |
| **Schemathesis** — K2/K3 | OpenAPI/GraphQL'den property-based test üretir; spec'te `links` varsa **stateful** dizi üretir. Akademik karşılaştırmada diğer araçlardan 1.4x–4.5x daha fazla kusur bulduğu raporlanmış (K3) | Sözleşme testinden senaryo testine geçiş için hazır fikir tabanı |
| **Citrus Framework** — K2 | Mesaj tabanlı entegrasyon testi; **JDBC istemcisi olarak DB'nin beklenen durumda olduğunu doğrular**; istekler arasında DB sorgusu çalıştırabilir | "HTTP çağır → DB'de doğrula" akışı yeni bir fikir değil, olgun bir pattern. Bizim farkımız: bunu *paket* olarak ve *çok kiracılı* sunmak |
| **Testcontainers for .NET** — K2 | `Testcontainers.PostgreSql`, `Testcontainers.MsSql`; hazırlık kontrolleri dahili — `StartAsync()` DB gerçekten bağlantı kabul edene kadar dönmez | E-12'nin altyapısı; motor doğruluğunu gerçek PG ve gerçek MSSQL üstünde kanıtlamanın yolu |

---

## 5. Önerilen geliştirmeler

Her madde şu şablonda: **Sorun (koddan) → Global kanıt → Öneri → Paket sınırına etkisi → Bitti ölçütü.**
Sıra, *önem* sırasıdır; uygulama sırası Bölüm 7'dedir.

---

### E-01 — Şema saklamadan drift: *fingerprint baseline*

**Sorun.** Motor yalnızca "A ↔ B" canlı kıyas yapabiliyor. "Geçen hafta ile bugün" kıyaslanamıyor (G3).
Klasik çözüm — şema snapshot'ını tabloya yazmak — kullanıcı tarafından açıkça reddedildi: *güvenilir değil
ve gereksiz veri*. Bu itiraz **teknik olarak da doğrudur**: saklanan snapshot ilk provider sürümü
değişiminde eskir, PII olmasa bile müşterinin iç yapısını taşır ve şişer.

**Global kanıt.** Flyway `checksum` kolonu ve Liquibase `MD5SUM` kolonu tam olarak bu problemi
*içeriği saklamadan* çözer: saklanan şey parmak izidir, gövde değil (K2). Atlas da drift verisini
kendi tutmaz, talep anında karşılaştırır (K2).

**Öneri.** Motor zaten her nesne için **kanonik tanım metni** üretiyor (`BuildColumnDefinition`,
`BuildIndexDefinition`, `BuildConstraintDefinition` — `SchemaComparisonManager.cs:241-329`). Bu metinleri
hiyerarşik hash'le:

```text
column_fp   = SHA256(canonical_column_definition)
table_fp    = SHA256(sorted(column_fp | index_fp | constraint_fp | trigger_fp))
schema_fp   = SHA256(sorted(table_fp | object_fp))
snapshot_fp = SHA256(sorted(schema_fp))     -- Merkle ağacı
```

Kalıcı yazılan **tek şey**: `snapshot_fp` + `schema_fp` seti + okuma anı + engine kodu. Yani bir
run başına birkaç yüz bayt; ne kolon adı, ne tip, ne veri.

Bu tek karar üç yeteneği aynı anda açar:
1. **Drift:** `snapshot_fp` değiştiyse bir şey değişmiş — %100 kesin, sıfır false-positive.
2. **Ucuz ön kontrol:** İki tarafın `schema_fp`'i eşitse o şemanın alt karşılaştırması **hiç çalıştırılmaz**.
3. **Merkle inişi:** Hangi tablonun değiştiği, tam diff yapmadan bulunur — data-diff'in bisection'ının
   şema tarafındaki karşılığı.

**Paket sınırına etkisi.** Yeni tablo değil, mevcut `ComparisonRun`'a birkaç kolon (`SnapshotFingerprint`
kaynak/hedef) + opsiyonel küçük bir `run.SchemaFingerprints` tablosu. RULE-0002 ihlali yok (kendi şeması).

**Bitti ölçütü.** Aynı DB iki kez okunduğunda fingerprint birebir aynı; tek kolon `nullable` değiştiğinde
`table_fp` ve `snapshot_fp` değişiyor; ilgisiz tablo eklenince o tablonun dışındaki `table_fp`'ler sabit kalıyor.

---

### E-02 — Bulgu kimliği ve baseline / suppression

**Sorun.** Bulgunun kararlı kimliği yok (G2). Her run sıfırdan liste üretir; "bunu zaten biliyoruz"
denemez, "bu yeni çıktı" denemez.

**Global kanıt.** GitHub code scanning, iki sonucun *mantıksal olarak aynı* olduğunu `partialFingerprints`
ile anlar; fingerprint kararsız olduğunda **aynı sorun için birden çok alert** oluştuğunu dokümante eder (K2).
Squawk `squawk-ignore` yorumu ve kural bazlı exclude sunar (K2). Atlas kasıtlı sapmalar için `exclude` verir (K2).

**Öneri.**
```text
finding_fp = SHA256(engine_pair | schema | object_type | object_name | child_name | kind_code | normalized_definition_delta)
```
`ComparisonRun` içinde her bulguya `Fingerprint` alanı; ayrıca `definition.SuppressedFindings`
(fingerprint + gerekçe + kim + ne zaman + opsiyonel son kullanma tarihi) — **fingerprint tutmak veri tutmak değildir.**

Rapor üç kova döner: `New` / `Known` / `Resolved`.

**Bitti ölçütü.** Aynı fark iki ardışık run'da aynı fingerprint'i alıyor; tabloya ilgisiz bir kolon
eklenince mevcut bulguların fingerprint'i **değişmiyor**; susturulmuş bulgu raporda `Known` olarak,
sayaçta ayrı görünüyor.

---

### E-03 — Severity / impact sınıflandırıcı (api-contract paritesi)

**Sorun.** DB bulgusunda `confidence` var, **severity yok** (G1). "34 fark" cümlesi kimseye karar verdirmiyor.

**Global kanıt.** Atlas'ın kapalı analyzer kataloğu (DS/MF/BC/NM/PG kodları, K2); Squawk'ın
`ban-drop-column` gibi adlandırılmış kuralları (K2); Bytebase'in severity/scope/engine başına
yapılandırılabilir SQL review politikası (K2); oasdiff'in ERR/WARN/INFO disiplini ve
"tanım gerçekten belirsizse WARN" kuralı (K2).

**Ve en güçlü iç kanıt:** api-contract tarafında `SpecDifferenceSeverityClassifier` zaten var ve
**tek karar noktası** olarak yazılmış (`SpecDifferenceSeverityClassifier.cs:10-37`). DB tarafı bu paritesizliği taşıyor.

**Öneri.** `DatabaseDifferenceSeverityClassifier` — api-contract'takiyle **aynı şekle** sahip, tek karar noktası:

| Fark | Kaynak→Hedef yönünde (hedef "uygulanacak" taraf) | Karşılık |
|---|---|---|
| Tablo/kolon hedefte yok | `Breaking` | Atlas DS102/DS103 |
| Kolon `nullable` → `not null` | `Breaking` | Atlas MF104 |
| Yeni `not null` kolon (default yok) | `Breaking` | Atlas MF103 |
| Mevcut kolona `unique` index | `Breaking` (veri bağımlı) | Atlas MF101 |
| Tip daraldı (`varchar(200)`→`varchar(50)`, `bigint`→`int`) | `Breaking` | — |
| Tip genişledi | `NonBreaking` | — |
| Yeni nullable kolon / yeni index | `NonBreaking` | — |
| Sadece `COMMENT`/açıklama | `DocsOnly` | oasdiff `DescriptionChanged` paraleli |
| Cross-engine `Canonical` confidence ile üretilmiş tip farkı | `Warning` | oasdiff WARN disiplini |

**Kritik uyarı — yön asimetrisi.** api-contract skill'i bunu zaten yazmış: *"Yön parametresi alan tek bir
'generic' metod yazma: iki taraf farklı kurallara tabidir"*. DB tarafında da aynısı geçerli: "kolon
hedefte yok" ile "kolon kaynakta yok" **aynı şiddet değildir** ve hangisinin "doğru" taraf olduğu
`ComparisonDefinition`'ın anlamına bağlıdır. Bu yüzden severity, definition'a bir **rol** alanı
(`Reference` / `Audited`) eklemeden doğru hesaplanamaz.

**Bitti ölçütü.** Her `DifferenceKind × direction` kombinasyonunun pozitif **ve negatif** birim testi var
(negatif test = kuralın yanlışlıkla tetiklenmediğini kanıtlayan test; api-contract skill'i bunu zaten şart koşuyor).

---

### E-04 — Veri karşılaştırmasında chunked checksum + bisection

**Sorun (K1, ölçülebilir).** `ReadTableDataAsync` seçili tabloların **bütün satırlarını** `to_jsonb(row)::text`
olarak app process'ine çekiyor (`DatabaseDataComparisonRepositoryBase.cs:109-120`,
`PostgreSqlDatabaseDataComparisonRepository.cs:30-31`). Sonra bellekte SHA-256'lanıyor. Koruma tek: tablo
`MaxRowsPerTable` (varsayılan **100.000**) üstündeyse `RowLimitExceeded` **fırlatılıyor**
(`DatabaseDataComparisonManager.cs:83-93`). Yani 100.001 satırlık tabloda ürün "kısmi cevap" değil, **hata** veriyor.

Bu, bu paketin en büyük teknik borcudur ve kullanıcının kendi ilkesiyle de çelişir: 100.000 satırlık
bir tablonun tamamını ağdan çekip belleğe almak, "kimsenin DB'sini komple yazmıyorum" ilkesinin ruhuna aykırıdır
— yazmıyoruz ama **okuyup taşıyoruz**.

**Global kanıt.** data-diff: segmentlere böl, **checksum'u DB içinde** hesapla, eşleşmeyeni özyinelemeli
böl, eşik altına inince satırı çek — *"keeps the amount of data that has to be transferred ... to a minimum"* (K2).
pt-table-checksum: indeks üzerinden nibbling, **0,5 sn hedefli adaptif chunk**, üstel ağırlıklı ortalama (K2).

**Öneri — üç kademeli, mevcut motoru bozmadan:**

```text
Kademe 0  COUNT(*)  ....................  zaten var (DataRowCountComparisonManager)
Kademe 1  Tablo hash'i DB İÇİNDE  ......  PG: md5(string_agg(md5(t::text), '' ORDER BY pk))
                                          MSSQL: HASHBYTES üzerinden agrega
          -> eşitse tablo temiz, satır hiç okunmaz
Kademe 2  PK aralığına göre N segment  .  segment hash'i eşitse in, değilse böl (bisection)
Kademe 3  Eşik altındaki segmentin satırları çekilir -> MEVCUT TableDataComparisonManager çalışır
```

Kademe 3, bugünkü kodun **aynısıdır**. Yani bu değişiklik motoru değiştirmiyor, motorun **önüne**
üç kademe filtre koyuyor. `MaxRowsPerTable` artık "hata fırlatma eşiği" değil, "bisection'a geç" eşiği olur.

**Dikkat — kanıtlı tuzaklar:**
- data-diff'in kendi dokümanı, key kolonundaki büyük boşlukların performansı bozduğunu söylüyor (K2) →
  segment sınırı `min/max` bölmesiyle değil, `NTILE`/percentile ile seçilmeli.
- SQL Server'da `CHECKSUM_AGG` **32-bit**, 100k satırda ~%30 çarpışma (K3 hesap, K2 doküman uyarısı) → `HASHBYTES`.
- Sıralama belirleyici olmalı; aksi halde aynı veri farklı hash üretir. Motorun mevcut kanonik metin
  üretimi (`BuildCanonicalRow`, `EncodeValue`) bu disipline zaten sahip — DB tarafına **aynı** kanonikleştirme taşınmalı,
  yoksa kademe 1 ile kademe 3 birbirini yalanlar.

**Bitti ölçütü.** 1M satırlık, tek satırı farklı iki tabloda: (a) doğru satır bulunuyor, (b) ağdan çekilen
satır sayısı < 10.000, (c) sonuç bugünkü tam-okuma yolunun sonucuyla **birebir** aynı (aynı testte iki yol karşılaştırılır).

---

### E-05 — Bulgularda ham müşteri verisi: redaction politikası

**Sorun (K1, ciddi).** `DataValueDifferenceModel.SourceValue`/`TargetValue` ve
`DataRowDifferenceModel.PrimaryKeyValue` **gerçek hücre değerleridir** ve `run.ComparisonRuns.findings`
jsonb kolonuna kalıcı yazılır (`ComparisonRunConfiguration.cs:50-62`), `GetDetailAsync` ile API'den döner
(`ComparisonRunDetailDto.cs:14`). Bir `Person` tablosunda veri karşılaştırması yapıldığında TC kimlik,
e-posta, telefon platformun kendi DB'sine düşer ve orada süresiz kalır.

Bu, wiki'nin kendi araştırma arşivindeki §12.3 "Zero-retention önerisi" maddesinin **ihlalidir**:
*"Karar verilene kadar secret, credential, raw müşteri satırı ve sınırsız payload kalıcılaştırılmaz."*

**Global kanıt.** EDPB'nin pseudonymisation rehberi, kişisel veri işleyen **test ortamları için
pseudonymisation'ı asgari güvenlik önlemi** kabul eder; hiyerarşi: mümkünse anonim, değilse
pseudonymised, doğrudan tanımlayıcı veri yalnızca kesinlikle gerekliyse (K2/K3 — hukuki yorum kaynakları).
Anthropic'in code-execution-with-MCP yazısı da aynı mimari refleksi teknik tarafta savunuyor:
*"intermediate results stay in the execution environment by default"* + PII'nin otomatik tokenizasyonu (K2).

**Öneri — kolon başına politika, varsayılan kapalı:**

| `ValueRetentionMode` | Saklanan | Kullanım |
|---|---|---|
| `None` (**varsayılan**) | Sadece "farklı" bayrağı | Production hedefler |
| `Hashed` | `SHA256(salt‖value)` — eşitlik karşılaştırılabilir, değer okunamaz | PK ve join alanları |
| `Masked` | `Ali***@***.com`, `+90***4567` | Hata ayıklama |
| `Full` | Ham değer | **Yalnızca** açık onay + TTL + izin ile |

Buna eşlik etmesi gerekenler: `run.ComparisonRuns` üzerinde **TTL/purge** işi, `Full` modun
`DatabaseChecker.Runs.ViewRawValues` gibi ayrı bir izne bağlanması ve run kaydında hangi modun
kullanıldığının yazılı olması (denetlenebilirlik).

**Bitti ölçütü.** Varsayılan ayarla çalışan bir `DataOnly` run'ından sonra `findings` jsonb'sinde
kaynak veriden gelen tek bir okunabilir string bulunmuyor (otomatik test bunu tarayarak kanıtlıyor).

---

### E-06 — Scope push-down

**Sorun.** `ReadSnapshotAsync(connection, schemaNames)` scope kurallarını **almıyor**; tüm şema okunuyor,
filtreleme sonradan bellekte yapılıyor (`SchemaComparisonExecutionManager.cs:57-59` → `SchemaComparisonManager.cs:43-46`).
Kullanıcı "tek tabloyu kıyasla" dese bile 4.000 tablolu bir katalog baştan sona okunuyor.

**Global kanıt.** MCP Toolbox for Databases'in temel tezi aynıdır: *"Instead of granting an agent
unrestricted schema access, you use a declarative configuration file to define specific, safe actions"* (K2).
Atlas drift check'inde `exclude` **okuma** aşamasında uygulanır (K2).

**Öneri.** `IDatabaseSchemaDiscoveryRepository.ReadSnapshotAsync`'e opsiyonel bir
`SchemaReadPlan` (şema listesi + tablo whitelist/blacklist) parametresi ekle; `ComparisonScopeRuleEvaluator`
bu planı üretsin (`BuildDataCompareTableIdentifiers` için zaten benzer bir yeteneği var —
`ComparisonScopeRuleEvaluator.cs:47`). Bellekteki filtreleme **kaldırılmaz**; savunma katmanı olarak kalır.

**Bitti ölçütü.** Tek tabloluk `Include` kuralıyla çalışan bir run'da hedefe giden katalog sorgusu
o tablonun dışındaki satırları getirmiyor (sorgu logu ile kanıtlanır).

---

### E-07 — Hedef DB'ye dokunma hijyeni

**Sorun (K1).**
- Connection string sabit 10 sn timeout; ama **`statement_timeout` / `lock_timeout` yok**, `SET TRANSACTION READ ONLY` yok,
  `ApplicationName` yok (`DatabaseConnectionStringFactory.cs:16-41`).
- PostgreSQL: `SslMode.Prefer` → sunucu TLS desteklemiyorsa **sessizce plaintext**'e düşer.
- SQL Server: `TrustServerCertificate = true` → sertifika doğrulaması **kapalı**, MITM'e açık.
  Koddaki yorum bunun kurumsal self-signed sertifikalar için bilinçli olduğunu söylüyor; sorun kararın
  kendisi değil, **bağlantı başına yapılandırılamaz oluşu**.

**Global kanıt.** PostgreSQL dokümanı `statement_timeout`'un kilit beklemesini kesmediğini, bunun
`lock_timeout`'un işi olduğunu açıkça yazar; `transaction_timeout` ile birlikte **en kısası kazanır** (K2).
Ayrıca `postgresql.conf`'ta global `statement_timeout` önerilmez; **rol/bağlantı bazında** verilmelidir (K2).

**Öneri.** Bağlantı başına `ConnectionSafetyProfile`:
```text
StatementTimeoutMs, LockTimeoutMs, ReadOnlyTransaction (varsayılan true),
ApplicationName = "CheckNexus.DatabaseComparison/{version} run={runId}",
TlsMode: Require | Prefer | Disable,  TrustServerCertificate: bool (varsayılan false)
```
Okuma yolu daima `READ ONLY` transaction içinde açılır. `ApplicationName`, DBA'nın
`pg_stat_activity` / `sys.dm_exec_sessions` üzerinde "bu sorgu kim?" sorusunu cevaplamasını sağlar —
canlı DB'ye bağlanan bir aracın vermesi gereken asgari nezakettir.

**Bitti ölçütü.** Hedefte `SELECT pg_sleep(60)` benzeri bir yük simüle edildiğinde run,
`StatementTimeoutMs` sonunda temiz `Failed` oluyor; hedefte yazma denemesi transaction seviyesinde reddediliyor.

---

### E-08 — En az yetki (least privilege) sözleşmesinin dokümante edilmesi

**Sorun.** Paket, hedef DB'de hangi yetkilerin **yeterli** olduğunu söylemiyor. Pratikte müşteri
`db_owner`/superuser verir; bu da E-07'deki riski çarpar.

**Global kanıt.** PostgreSQL 14+ `pg_read_all_data` rolü: tablo/view/sequence okuma + tüm şemalarda
`USAGE`, superuser olmadan (K2). SQL Server: `VIEW DEFINITION` metadata görünürlüğü verir ama
**veriye erişim vermez**; katalog view görünürlüğü kullanıcının sahip olduğu/izinli olduğu nesnelerle sınırlıdır (K2).
MCP Toolbox rehberi: *"credentials used by Toolbox only have read-only permissions ... the database will
reject any data modification attempts"* (K2).

**Öneri.** `PACKAGE-README.md`'ye motor başına **kopyala-yapıştır GRANT bloğu** ve iki profil:
`SchemaOnly` profili (PG: `pg_read_all_data` gerekmez, katalog okuma yeter / MSSQL: `VIEW DEFINITION`)
ve `DataCompare` profili (PG: `pg_read_all_data` veya hedef tablolara `SELECT` / MSSQL: `db_datareader`).
Ek olarak bağlantı testi, verilen kimliğin **yazma yetkisi olup olmadığını** raporlasın — fazla yetki bir bulgudur.

**Bitti ölçütü.** README'deki minimum yetkilerle kurulmuş bir kullanıcı ile `SchemaOnly` run'ı tam çalışıyor;
aynı kullanıcı `INSERT` denemesinde reddediliyor.

---

### E-09 — Hedefli assertion API'si (**köprünün taşıyıcı kirişi**)

**Sorun.** Test Module'ün ihtiyacı "iki DB'yi kıyasla" değil: *"POST /orders çağırdım, `201` ve `id=X` döndü;
`sales.Orders` tablosunda `Id=X` satırı 5 saniye içinde `Status='Pending'` ile oluştu mu?"*
Bugün bunu yapmanın yolu yok (G9). Tam karşılaştırma bu iş için hem çok pahalı hem de yanlış araç.

**Global kanıt.** Bu tam olarak wiki'nin kendi araştırma arşivi §14.6'da yazan sonuçtur:
*"Hedefli find-by-key, bounded polling, transaction isolation ve deterministic cleanup büyük full-table
diff'ten daha güvenli test primitive'leridir."* Dış kanıt: Citrus Framework tam da bunu yapar — JDBC
istemcisi olarak istekler arasında DB'nin beklenen durumda olduğunu doğrular (K2).

**Öneri — yeni bir Application.Contracts yüzeyi (`IDatabaseAssertionAppService`):**

```csharp
// Tek kayıt, anahtarla, sınırlı bekleme ile
Task<RowAssertionResultDto> AssertRowAsync(AssertRowRequestDto input);
// { ConnectionId, Schema, Table, Key: {col:value}, Expect: {col:matcher},
//   TimeoutMs, PollIntervalMs, ValueRetentionMode }

// Sayım invariant'ı: "bu filtreye uyan tam 1 satır olmalı"
Task<CountAssertionResultDto> AssertCountAsync(AssertCountRequestDto input);

// Yokluk: "silindi mi?"
Task<RowAssertionResultDto> AssertAbsentAsync(AssertRowRequestDto input);
```

Kurallar (pazarlıksız):
- **Yalnız okuma.** Yazma/temizlik bu API'nin işi değil.
- **Yalnız anahtarla.** Serbest `WHERE` yok, serbest SQL yok — enjeksiyon ve kaçak sorgu yüzeyi kapalı kalır.
  Tablo ve kolon adları katalogdan doğrulanır (motor bu yeteneği `ReadTableStructuresAsync` ile zaten taşıyor).
- **Sınırlı bekleme.** `TimeoutMs` zorunlu ve üst sınırlı; sonsuz polling yok.
- **Sonuç redaction'lı.** E-05 politikası burada da geçerli; varsayılan `None`.
- **Bulgu değil, kanıt.** Sonuç `Pass/Fail` + ne kadar sürdü + kaç deneme + hangi matcher patladı.

**Neden bu madde en önemlisi:** Bölüm 6'daki MCP tester'ının **token bütçesinin %80'ini** bu API belirler.
Tam karşılaştırma sonucu 50–500 KB JSON'dur; bir assertion sonucu 200 bayttır.

**Bitti ölçütü.** Test Module olmadan, ince host üzerinden: bir satır insert edilir, `AssertRowAsync`
1 sn içinde `Pass` döner; satır silinir, `AssertAbsentAsync` `Pass` döner; hiç oluşmayan satırda
`TimeoutMs` dolduğunda deterministik `Fail` döner.

---

### E-10 — Zamanlanmış drift izleme (api-contract paritesi)

**Sorun.** DB tarafında zamanlanmış kontrol yok (G8). api-contract'ta `ScheduledSpecDocumentCheckManager`
ve `ConfigureDocumentMonitoringAsync` var (`ISpecSourceAppService.cs:26-29`) — iki checker arasında
gereksiz bir asimetri.

**Global kanıt.** Bytebase: arka planda periyodik karşılaştırma + `Scan Interval` + manuel "Sync Database" (K2).
Atlas: `migrate apply` öncesi senkron kontrol **ve** ayrıca sürekli bant-dışı monitoring (K2).

**Öneri.** `ComparisonDefinition`'a `MonitoringEnabled` + `CheckIntervalMinutes`; ABP background worker
`SchemaOnly` modda çalışır ve **yalnızca E-01 fingerprint'i değiştiyse** tam karşılaştırmayı tetikler.
Zamanlayıcının kendisi paketin içinde değil — RULE-0004 gereği tetikleme composition host'un işidir;
paket yalnızca "çalıştırılabilir iş" ve "bir sonraki çalışma zamanı" bilgisini sunar.

**Bitti ölçütü.** Hedefte elle bir kolon eklendiğinde, bir sonraki periyotta drift bulgusu üretiliyor;
hiçbir şey değişmediğinde tam karşılaştırma **hiç çalışmıyor** (fingerprint kısa devresi).

---

### E-11 — Dışa aktarım: SARIF ve JUnit

**Sorun.** Bulgular yalnızca kendi DTO'muzda ve Html/Markdown raporunda (G10). CI'a, GitHub'a,
test raporlayıcısına bağlanamıyor.

**Global kanıt.** SARIF 2.1.0 OASIS standardıdır ve GitHub code scanning bu formatı kabul eder;
`partialFingerprints` alert kimliğini korur (K2). oasdiff `--fail-on WARN` ile çıkış kodunu severity'ye
bağlar (K2) — CI entegrasyonunun asgari sözleşmesi budur.

**Öneri.** `ReportFormatCodes`'a `Sarif` ve `JUnit` ekle (lookup zaten var, mimari hazır).
E-02 fingerprint'i → SARIF `partialFingerprints`; E-03 severity → SARIF `level`
(`error`/`warning`/`note`); `ruleId` → `DifferenceKindCodes`. `physicalLocation` yerine
`logicalLocations` (şema.tablo.kolon) kullanılır — DB nesnesinin dosya yolu yoktur.

**Bitti ölçütü.** Üretilen SARIF, OASIS 2.1.0 şemasına karşı valide oluyor; ardışık iki run'da
aynı fark aynı fingerprint ile geliyor.

---

### E-12 — Differential oracle + Testcontainers matrisi

**Sorun.** Motorun doğruluğu bugün kendi birim testlerine dayanıyor (G11). Kendi kendini doğrulayan bir motor,
kendi kör noktalarını göremez.

**Global kanıt — ve bu sefer kanıt evin içinden:** api-contract ekibi bunu **zaten çözmüş**.
`.agents/skills/acc-comparison-engine/scripts/oasdiff_oracle.py` motoru oasdiff'e karşı çalıştırıyor;
`accepted-deviations.json` her sapmayı `deliberate` / `known-gap` olarak, gerekçesiyle ve RULE referansıyla
kaydediyor. Örnek satır: *"oasdiff has no rename detection and reports the old name as removed; we report
schema-renamed instead (RULE-0007)"*. Bu, bir motorun doğruluğunu kanıtlamanın **en dürüst** yoludur:
farkı gizlemek değil, gerekçelendirmek.

**Öneri.** DB tarafında aynı deseni kur:
- **Oracle adayları:** `migra` veya `atlas schema diff` (PostgreSQL), `SqlPackage /Action:DeployReport`
  (SQL Server). Hiçbiri "doğru cevap" değildir — **ikinci bir görüş**tir.
- **Fikstür:** Testcontainers ile gerçek PG ve gerçek MSSQL; `StartAsync()` DB hazır olana kadar döner (K2).
- **Sapma defteri:** `accepted-deviations.json` ile birebir aynı format ve aynı `deliberate` / `known-gap` disiplini.

**Bitti ölçütü.** CI'da her iki motor için oracle koşusu var; sapma defterinde gerekçesiz tek satır yok.

---

### E-13 — Paket kalite kapıları

**Sorun.** `0.1.0-alpha.5` yayımlandı ama sürüm disiplininin **otomatik** kapıları görünmüyor (G12).
Ayrıca sürüm grafiği riski var: paket ABP 10.3.0'a göre yazılmış (PACKAGE-README, K1), NuGet'te
`Volo.Abp` 10.5.0/10.6.0 mevcut (K2). Consumer host farklı bir ABP sürümü getirirse çakışma
Test Module entegrasyonunda patlar — bu risk zaten [[05-Operations/Roadmap|GUIDE-0002]] ilk maddesi.

**Global kanıt.** .NET Package Validation üç doğrulayıcı sunar; `EnablePackageValidation` +
`PackageValidationBaselineVersion` ile **ikili kırıcı değişiklikler** otomatik yakalanır (K2).
Deterministic build için `ContinuousIntegrationBuild=true`; SourceLink için `Microsoft.SourceLink.GitHub` (K2).

**Öneri.** `common.props` içine:
```xml
<EnablePackageValidation>true</EnablePackageValidation>
<PackageValidationBaselineVersion>0.1.0-alpha.5</PackageValidationBaselineVersion>
<ContinuousIntegrationBuild Condition="'$(CI)'=='true'">true</ContinuousIntegrationBuild>
<PublishRepositoryUrl>true</PublishRepositoryUrl>
<EmbedUntrackedSources>true</EmbedUntrackedSources>
<IncludeSymbols>true</IncludeSymbols>
<SymbolPackageFormat>snupkg</SymbolPackageFormat>
```
Ek olarak ABP sürüm aralığı kararı ([[90-Inbox/Inbox|INBOX-0001]] açık sorusu) bir ADR ile kapatılmalı:
sabit sürüm mü, aralık mı, Central Package Management mi.

**Bitti ölçütü.** `0.2.0-alpha.1` build'i, `0.1.0-alpha.5`'e göre ikili kırıcı bir değişiklik varsa
**derlemede** patlıyor; `.snupkg` üretiliyor; `.nupkg` içinde host/test çıktısı yok (RULE-0001 otomatik doğrulaması).

---

### E-14 — Gözlemlenebilirlik

**Sorun.** Run'ın nerede yavaşladığı (secret çözme mi, katalog okuma mı, diff mi) ölçülemiyor.

**Global kanıt.** OpenTelemetry veritabanı istemci span'ları **stable**; `db.system.name` stabil ve
`postgresql`, `microsoft.sql_server` değerlerini içeriyor. Geçiş için `OTEL_SEMCONV_STABILITY_OPT_IN`
mekanizması var (K2).

**Öneri.** `ActivitySource` ile span'lar: `checknexus.db.discovery.read`, `checknexus.db.compare.schema`,
`checknexus.db.compare.data`, `checknexus.db.assert.row`. Öznitelikler `db.system.name`, `db.namespace`,
`checknexus.run.id`, `checknexus.finding.count`. **Yasak:** span özniteliğine hücre değeri, host adı,
kullanıcı adı, secret path yazmak (RULE-0003'ün doğal uzantısı).

**Bitti ölçütü.** Tek run'ın trace'inde dört aşama ayrı ayrı görünüyor; hiçbir öznitelikte müşteri verisi yok.

---

### 5.X Bilinçli olarak ÖNERMEDİKLERİM

Bir analiz, ne yapılmayacağını da söylemelidir:

| Öneri | Neden hayır |
|---|---|
| Şema snapshot'ını tabloya yazmak | Kullanıcı reddetti ve **haklı**: eskir, şişer, müşteri iç yapısını taşır. E-01 fingerprint aynı yeteneği veriyor |
| Farkı kapatacak SQL üretmek (`migra`/`pg-schema-diff` gibi) | Checker **bilgi** motorudur, eylem motoru değil ([[04-Architecture/System-Context\|ARCH-0001]]). SQL üreten bir checker, üretim DB'sini değiştirebilen bir checker'a bir adım uzaktır |
| MCP'de serbest SQL çalıştıran bir tool | Bölüm 6.5'te kanıtlı gerekçe: bu, bilinen ve gerçekleşmiş bir veri sızıntısı desenidir |
| Checker'a bildirim/e-posta göndermek | RULE-0004 + ADR-0002. Notifications ayrı capability |
| Bağımlılık olarak `data-diff` almak | Upstream 17 Mayıs 2024'te arşivlendi (K2). **Algoritma** alınır, paket alınmaz |
| Elasticsearch'ü zorunlu kılmak | Kodda arama için mevcut (`ElasticsearchRepository.cs`) ama consumer'a ikinci bir altyapı dayatmak paket sınırını şişirir; opsiyonel kalmalı |

---

## 6. Köprü ve MCP: "tester'ın tam işini yapan, en az token yakan" sistem

Kullanıcının tek cümlesi buydu. Bu bölüm o cümlenin mühendislik karşılığıdır.

### 6.1 Köprü neden ayrı bir şey?

İki checker aynı soruyu **farklı dillerde** cevaplıyor:

```text
api-contract   :  "POST /orders  ->  201  ->  OrderResponse { id, status }"
db-comparison  :  "sales.Orders  ->  (Id uuid PK, Status varchar(20) not null)"
```

Arada **hiçbir otomatik bağ yok**. Wiki arşivi §14.6 bunu net söylüyor: *"OpenAPI alanı ile DB kolonunu
HTTP methodundan otomatik ve kesin eşlemek mümkün değildir. Explicit, versioned binding gerekir."*
Bu doğru ve önemli bir tespit — köprü **tahmin** değil, **beyan**dır:

```text
ApiDbBinding (versiyonlu, insan tarafından beyan edilmiş)
  operation : POST /orders                  -> api-contract'taki operation kimliği
  produces  : sales.Orders                  -> db-checker'daki tablo kimliği
  key       : response.$.id  ->  Orders.Id  -> korelasyon kuralı
  expect    : response.$.status -> Orders.Status
```

Bu tanım hazırsa bir tester'ın işi mekanikleşir: çağır → çıkar → doğrula → kanıtla.

### 6.2 MCP'nin bugünkü gerçeği (2026-08-12 itibarıyla)

**Bu, wiki'nin düzeltilmesi gereken bir noktasıdır.** [[05-Operations/Source-Registry|SOURCE-0001]]
MCP kaynağı olarak `specification/2025-06-18`'i gösteriyor. Bugün **iki revizyon geride**:

| Revizyon | Durum (K2, `modelcontextprotocol.io/specification/versioning`) |
|---|---|
| 2025-06-18 | Final |
| 2025-11-25 | Final — tasks (deneysel), icons, elicitation genişletmeleri, sampling'de tool desteği |
| **2026-07-28** | **Current** |

2026-07-28 ile gelen ve bizi doğrudan ilgilendiren değişiklikler (K2):

1. **Stateless çekirdek.** `initialize` el sıkışması ve `Mcp-Session-Id` kalktı. Sunucu, sıradan bir
   round-robin load balancer arkasında çalışabiliyor. Bizim için: MCP adapteri ABP host'unun içinde
   **ölçeklenebilir bir controller gibi** durabilir; sticky session altyapısı gerekmez.
2. **Cacheable list sonuçları.** `tools/list` cevabı `ttlMs` + `cacheScope` taşıyor ve sunucuların
   **deterministik sırada** tool döndürmesi isteniyor — gerekçe spec'te açıkça yazılı:
   *"Deterministic ordering enables clients to reliably cache the tool list and improves LLM prompt cache hit rates."*
   **Bu doğrudan token maliyeti maddesidir.**
3. **Tasks artık resmî bir extension** (`io.modelcontextprotocol/tasks`): `tasks/get`, `tasks/update`, poll tabanlı.
4. **MRTR (Multi Round-Trip Requests).** `resultType: "input_required"` + `inputResponses` ile,
   sunucudan istemciye açık stream tutmadan ek girdi istenebiliyor.
5. **Stateful Tools rehberi** (non-normative ama tam bizim desenimiz): *"servers ... should do so by
   returning an explicit handle from a creation tool and accepting that handle as an argument on subsequent calls"* —
   ve handle tasarımında yetkilendirme, opaklık, ömür, expiry hatası maddeleri.
6. **Deprecation:** Roots, Sampling ve Logging deprecated (en az 12 ay geçiş); eski HTTP+SSE transport resmen deprecated.

.NET tarafı hazır: **ModelContextProtocol 2.1.0 stable**, 5 Ağustos 2026, `net8.0/net9.0/net10.0/netstandard2.0`;
`ModelContextProtocol.AspNetCore` 2.1.0 (K2, nuget.org). Yani ABP 10 / .NET 10 host'umuzla uyumlu.

### 6.3 Token maliyeti: ölçülmüş gerçekler

| Bulgu | Kaynak | Sınıf |
|---|---|---|
| Tool kataloğu büyüdükçe tool seçim doğruluğu düşüyor; pratik eşik **~10–20 aktif tool** civarında raporlanıyor; ~%90 üstü başarı ~30 aday tool'a kadar, ~100'den sonra keskin düşüş | çeşitli benchmark/derleme | K3 |
| GitHub'ın resmî MCP sunucusu tek başına ~42.000 token'lık tool tanımı yüklüyor; 4–5 sunucu 60.000+ token | pratisyen ölçümü | K3 |
| Playwright MCP tipik bir görevde ~**114.000 token**; CLI alternatifi ~**27.000** | pratisyen ölçümü | K3 |
| Anthropic'in code-execution yaklaşımı bir Google Drive→Salesforce iş akışında **150.000 → 2.000 token (%98,7)** | Anthropic engineering | K2 |

K3'ler kesin sayı değil; ama **yönü** tartışmasız ve K2 ile aynı yöne bakıyor. Sonuç net:
**bir tester MCP'sinin maliyeti, tool sayısı ve tool çıktısının boyutu tarafından belirlenir; model
tarafından değil.**

### 6.4 Önerilen tool kataloğu — 9 tool, hepsi Application.Contracts üstünde

RULE'lara ve ARCH-0001'e sadık: *"MCP repository, EF DbContext veya Vault'a doğrudan erişmez; izinli
application contractlarını çağırır."*

| # | Tool | `readOnlyHint` | Döndürdüğü |
|---|---|---|---|
| 1 | `plan.list` | ✔ | Çalıştırılabilir TestPlan başlıkları (id + ad + son durum) |
| 2 | `plan.get` | ✔ | Tek planın adımları — **resource_link**, gövde değil |
| 3 | `api.operation.find` | ✔ | Operation özeti: method, path, required alanlar, response şeması **özeti** |
| 4 | `api.call` | ✘ | Durum kodu + seçili alanlar (JSONPath ile daraltılmış), **tam gövde değil** |
| 5 | `db.assert.row` | ✔ | E-09: `Pass/Fail` + süre + patlayan matcher |
| 6 | `db.assert.count` | ✔ | E-09 |
| 7 | `db.compare.start` | ✔ (hedefte) | **Task handle** — sonucu değil |
| 8 | `run.get` | ✔ | Handle ile durum + özet sayaçlar + severity dağılımı |
| 9 | `run.findings.page` | ✔ | Sayfalı, filtreli bulgu (severity/kind/tablo), varsayılan `New` kovası |

**Tasarım kuralları — her biri kanıtlı:**

- **9 tool, 20 değil.** Tool sayısı eşiği (K3) + spec'in deterministik sıralama/caching tavsiyesi (K2).
- **Serbest SQL tool'u YOK.** Bkz. 6.5.
- **Her tool'da `outputSchema`.** Spec: output schema varsa sunucu **MUST** uyumlu `structuredContent`
  döndürür, istemci **SHOULD** doğrular (K2). Model, JSON'u tahmin etmek yerine şemayı okur → daha az deneme, daha az token.
- **Ağır veri `resource_link` olarak.** Tam bulgu listesi tool sonucunda değil, kaynak bağlantısı olarak
  döner; model gerçekten gerekirse çeker.
- **Handle tabanlı akış.** `db.compare.start` → handle; `run.get` → özet; `run.findings.page` → sayfa.
  Spec'in "Stateful Tools" rehberiyle birebir (K2). 500 KB'lık bulgu JSON'u modelin bağlamına **hiç girmez**.
- **Sonuç filtresi sunucuda.** Anthropic'in "context-efficient tool results" maddesi: 10.000 satırı
  execution ortamında süzüp modele sadece ilgili kaydı göstermek (K2). Bizde bunun karşılığı
  `run.findings.page`'in severity+kind+tablo filtresidir.

### 6.5 Güvenlik: bu sistemin en kolay yanlış yapılacak yeri

DB'ye dokunan bir MCP sunucusu yazıyorsanız, aşağıdakiler teorik risk değil, **gerçekleşmiş olaylardır**:

| Olay / desen | Ne oldu (kanıt) | Bizim önlemimiz |
|---|---|---|
| **Supabase MCP sızıntısı** (Temmuz 2025) | Saldırgan bir destek talebine talimat gömüyor; ajan `service_role` ile — yani **RLS'i baypas ederek** — çalıştığı için tüm SQL veritabanını okuyup dışarı taşıyabiliyor. Simon Willison'ın "lethal trifecta" tanımı: özel veriye erişim + güvenilmez girdi + dışarı veri taşıma yeteneği (K2/K3) | Bizde 4, 5, 6 numaralı tool'lar dışında yazma yok; `db.*` tool'ları **yalnız anahtar** alır; hedef kimlik bilgisi en az yetkili (E-08); E-07 read-only transaction |
| **Tool poisoning / rug pull** (Invariant Labs, Nisan 2025) | Zararlı bir sunucunun **tool açıklamasına** gömülü talimat, aynı bağlamdaki meşru WhatsApp MCP'sinden mesaj geçmişini çektirip sızdırıyor. Ayrıca istemciler tool açıklaması **değiştiğinde** kullanıcıyı uyarmıyor (K2) | Spec'in kendi uyarısı: *"clients MUST consider tool annotations to be untrusted unless they come from trusted servers"*. Bizim sunucumuz tek ve iç; tool açıklamaları sürümlenir ve değişikliği release notunda görünür |
| **Token passthrough** | MCP sunucusunun, kendisine düzenlenmediğini doğrulamadan aldığı token'ı aşağı akışa iletmesi bir **anti-pattern**; doğrusu RFC 8693 token exchange (K3, spec güvenlik rehberine dayanıyor) | MCP adapteri consumer host'un kimlik bağlamını kullanır (RULE-0004); hedef DB kimliği **Vault'tan** çözülür, çağrandan gelmez |
| **Confused deputy** | Statik client ID + dinamik kayıt kombinasyonunda mevcut onay çerezinin yeniden kullanılması (K3) | 2026-07-28'in CIMD + RFC 9207 issuer doğrulaması yönü izlenir |

Ve tool sonuçları için: hedef veritabanından okunan **veri de güvenilmez girdidir**. Bir müşteri tablosundaki
`Notes` kolonunda "önceki talimatları unut" yazıyor olabilir. Bu yüzden E-05'in `None`/`Hashed` varsayılanı
sadece gizlilik değil, **prompt injection** önlemidir: modele hiç ham hücre girmezse, hücre üzerinden
enjeksiyon da olmaz.

### 6.6 Uzun işler: MCP Tasks ile run yaşam döngüsünün birebir eşleşmesi

Bu, bu analizin en şanslı bulgusudur — **ekstra iş yok, mevcut model zaten doğru**:

```text
CheckNexus                          MCP Tasks extension
------------------------------      ----------------------------
ExecuteAsync -> Pending run    ==>   CreateTaskResult (taskId, status: working)
Running                        ==>   working
Completed                      ==>   completed
Failed (house error code)      ==>   failed + statusMessage
GetDetailAsync                 ==>   tasks/result
GetStatus (header projeksiyon) ==>   tasks/get   (owned jsonb ÇEKMEDEN — perf zaten var)
```

`ComparisonRunRepository.FindHeaderAsync` ağır owned jsonb'yi projekte etmiyor
(`ComparisonRunAppService.cs:121-122`) — yani `tasks/get` polling'i **zaten ucuz**. Spec'in
`pollInterval` alanı ile birleşince, model gereksiz sıklıkta yoklamaz.

Ek olarak spec, task ID'lerinin yetki bağlamına bağlanmasını **MUST** olarak şart koşuyor; bağlanamıyorsa
kriptografik entropili ID ve kısa TTL. Bizde run ID zaten `Guid` + tenant filtresi arkasında.

### 6.7 Paketleme kararı: MCP nereye konur?

**Öneri:** ayrı bir paket — `CheckNexus.Assurance.Mcp` — ve şu bağımlılıklarla:

```text
CheckNexus.Assurance.Mcp
  -> CheckNexus.ApiContracts.Application.Contracts       (sadece contracts!)
  -> CheckNexus.DatabaseComparison.Application.Contracts (sadece contracts!)
  -> ModelContextProtocol.AspNetCore 2.1.0
```

Gerekçe: MCP adapteri `.Application` veya `.EntityFrameworkCore`'a **bağlanmamalı**. Contracts'a bağlanırsa
repository'ye, DbContext'e veya Vault'a erişme imkânı **derleme zamanında** yok olur — ARCH-0001'in
"MCP izinli application contractlarını çağırır" kuralı yorum olmaktan çıkıp **derleyici tarafından
zorlanan bir kısıt** haline gelir. Bu, kuralı dokümantasyonda tutmaktan çok daha güçlüdür.

---

## 7. Yol haritası — hangi sırayla ve neden

Sıralamanın mantığı: **önce ilkeyi ihlal edeni durdur, sonra köprüyü kur, sonra ölçekle.**

### Dalga 1 — "Önce zarar verme" (E-05, E-07, E-08)

Bunlar özellik değil, **düzeltme**. Ham müşteri verisinin kalıcılaşması ve sertifika doğrulamasının
kapalı olması, paket bir müşteri ortamına girdiği anda geri alınması pahalı sorunlardır. Yeni özellik
eklemeden önce bunlar kapanmalı. Yeni sürüm: `0.2.0-alpha.1`.

### Dalga 2 — "Köprüyü taşıyan kiriş" (E-09, E-03, E-02)

`AssertRow`/`AssertCount` olmadan Test Module ve MCP tester'ı yazılamaz — yazılırsa tam karşılaştırma
üzerine kurulur ve token maliyeti baştan yanlış olur. Severity ve fingerprint bunun hemen ardından gelir,
çünkü MCP'nin `run.findings.page` tool'u filtre için ikisine de muhtaçtır.

### Dalga 3 — "Ölçek ve olgunluk" (E-01, E-04, E-06, E-10, E-11, E-12, E-13, E-14)

Fingerprint (E-01) ve chunked diff (E-04) motoru büyük veritabanlarında kullanılabilir kılar; kalanlar
CI/operasyon olgunluğudur. E-13 bu dalganın **başında** yapılmalı ki E-01/E-04'ün getirdiği API
değişiklikleri baseline validator tarafından yakalansın.

### Paralel: MCP

Dalga 2 biter bitmez `CheckNexus.Assurance.Mcp` başlayabilir; Dalga 3 ile paralel yürür. MCP tarafının
ilk hedefi **tek dikey dilim** olmalı: *bir operation çağır → response'tan id çıkar → tek DB assertion'ı → sonucu döndür*.
Arşiv §12.1'in önerdiği ilk dilimle aynı; genel amaçlı motor yazılmadan önce bir gerçek senaryonun uçtan uca çalışması.

---

## 8. Kaynak defteri

Hepsine 2026-08-12'de erişildi.

### Birincil spesifikasyon ve resmî dokümantasyon (K2)

| Kaynak | Neyi kanıtlıyor |
|---|---|
| https://modelcontextprotocol.io/specification/versioning | Current revizyon 2026-07-28; revizyon durumları |
| https://modelcontextprotocol.io/specification/2026-07-28/server/tools | outputSchema, annotations güvenilmezliği, resource_link, Stateful Tools handle rehberi, ttlMs/cacheScope, deterministik sıralama |
| https://modelcontextprotocol.io/specification/2025-11-25/changelog | 2025-11-25 değişiklikleri |
| https://modelcontextprotocol.io/specification/2025-11-25/basic/utilities/tasks | Task yaşam döngüsü, TTL, pollInterval, güvenlik (yetki bağlama) |
| https://blog.modelcontextprotocol.io/posts/2026-07-28/ | Stateless çekirdek, MRTR, extensions, deprecation'lar |
| https://www.anthropic.com/engineering/code-execution-with-mcp | 150.000 → 2.000 token; progressive disclosure; ara sonuçların ortamda kalması |
| https://www.nuget.org/packages/ModelContextProtocol | 2.1.0 stable, 5 Ağu 2026, TFM'ler |
| https://www.nuget.org/packages/CheckNexus.DatabaseComparison/ | Kendi paketimizin yayın gerçeği |
| https://atlasgo.io/versioned/drift-detection | Drift modeli; "Atlas doesn't store drift data itself" |
| https://atlasgo.io/lint/analyzers | DS/MF/BC/NM/PG analyzer kodları |
| https://docs.bytebase.com/change-database/drift-detection | Periyodik drift, Anomalies, Baseline/Revert |
| https://www.oasdiff.com/docs/breaking-changes | ERR/WARN/INFO disiplini, kural bazlı severity |
| https://github.com/datafold/data-diff/blob/master/docs/technical-explanation.md | Bisection algoritması, veri transferini minimize etme, key boşluğu zaafı |
| https://www.datafold.com/blog/sunsetting-open-source-data-diff/ | 17 Mayıs 2024 arşivleme gerekçesi |
| https://github.com/erezsh/reladiff | Bakımlı fork |
| https://docs.percona.com/percona-toolkit/pt-table-checksum.html | Nibbling, 0,5 sn chunk hedefi, hash fonksiyonu seçenekleri |
| https://learn.microsoft.com/en-us/sql/t-sql/functions/checksum-transact-sql | CHECKSUM'ın 32-bit sınırı |
| https://learn.microsoft.com/en-us/sql/tools/sqlpackage/sqlpackage-deploy-drift-report | DriftReport aksiyonu |
| https://www.postgresql.org/docs/current/predefined-roles.html | `pg_read_all_data`, `pg_monitor` |
| https://www.postgresql.org/docs/current/runtime-config-client.html | statement_timeout / lock_timeout / idle_in_transaction_session_timeout |
| https://learn.microsoft.com/en-us/sql/relational-databases/security/permissions-database-engine | VIEW DEFINITION metadata görünürlüğü |
| https://docs.github.com/en/code-security/code-scanning/integrating-with-code-scanning/sarif-support-for-code-scanning | SARIF 2.1.0 + partialFingerprints |
| https://github.com/oasis-tcs/sarif-spec/blob/main/sarif-2.1/schema/sarif-schema-2.1.0.json | SARIF 2.1.0 şeması |
| https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/overview | Package Validation doğrulayıcıları |
| https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/baseline-version-validator | Baseline sürüm doğrulaması |
| https://opentelemetry.io/docs/specs/semconv/db/database-spans/ | DB span'larının stable durumu, `db.system.name` |
| https://spec.openapis.org/arazzo/latest.html | Arazzo workflow spec |
| https://squawkhq.com/docs/rules | Squawk kural kataloğu, ignore mekanizması |
| https://dotnet.testcontainers.org/modules/ | Testcontainers .NET modülleri |
| https://citrusframework.org/ | Mesaj + DB doğrulama entegrasyon testi |
| https://mcp-toolbox.dev/documentation/configuration/ | Deklaratif tool tanımı, read-only kimlik önerisi |
| https://developer.hashicorp.com/vault/docs/secrets/kv/kv-v2 | KV v2 (mevcut Vault adapteri) |

### İkincil / ölçüm iddiası (K3 — karar gerekçesi değildir)

| Kaynak | Ne için |
|---|---|
| https://simonwillison.net/2025/Jul/6/supabase-mcp-lethal-trifecta/ | "Lethal trifecta" çerçevesi |
| https://generalanalysis.com/blog/supabase-mcp-blog | Supabase MCP sızıntısının teknik anlatımı |
| https://invariantlabs.ai/blog/mcp-security-notification-tool-poisoning-attacks | Tool poisoning / rug pull |
| https://arxiv.org/html/2505.03275v1 | RAG-MCP: tool kalabalığının seçim doğruluğuna etkisi |
| Playwright MCP token ölçümleri (pratisyen blogları) | 114K vs 27K token karşılaştırması |
| https://schemathesis.io/ | Property-based API testi |

### Yerel kaynaklar (K1)

`checkers/database-comparison/**`, `checkers/api-contract/**` (özellikle
`.agents/skills/acc-comparison-engine/` — oracle deseni), `docs/wiki-brain/**`.

---

## 9. Bu belge kabul edilirse wiki'de ne değişir?

Karar verilmeden hiçbiri yapılmamalı. Kabul edilen madde başına:

| Madde | Etkilenen kanonik sayfa | Gerekli işlem |
|---|---|---|
| E-01, E-02, E-03 | [[01-Current/Checker-Packages-Truth\|CURRENT-0002]] | Bulgu modeli değişikliği yazılır |
| E-01 (fingerprint tablosu) | [[04-Architecture/Database-Ownership\|ARCH-0003]], RULE-0002 | Yeni tablo sahipliği eklenir |
| E-05 | RULE-0003 veya **yeni RULE** | "Müşteri verisi kalıcılık sınırı" kuralı — bugün yok |
| E-07, E-08 | [[01-Current/Vault-Truth\|CURRENT-0003]] + PACKAGE-README | Bağlantı güvenlik profili sözleşmesi |
| E-09 | [[04-Architecture/System-Context\|ARCH-0001]] | Checker'ın "assertion" rolü açıkça yazılır |
| E-13 (ABP sürüm aralığı) | [[90-Inbox/Inbox\|INBOX-0001]] açık sorusu → **yeni ADR** | Sürüm grafiği kararı |
| MCP paketi | **Yeni ADR** + ARCH-0002 | `CheckNexus.Assurance.Mcp` kimliği ve contracts-only bağımlılık kuralı |
| MCP revizyonu | [[05-Operations/Source-Registry\|SOURCE-0001]] | `2025-06-18` → `2026-07-28` düzeltmesi **(bu, karar beklemeyen bir hata düzeltmesidir)** |
| Tüm dalgalar | [[05-Operations/Roadmap\|GUIDE-0002]] | "Sıradaki paket işleri" listesi somutlaştırılır |
