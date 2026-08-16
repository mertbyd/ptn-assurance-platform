---
id: RESEARCH-0002
type: research
status: draft
title: Database Checker motorunu piyasa lideri yapacak yetenek haritasi
updated: 2026-08-12
decision_refs:
  - ADR-0002
  - ADR-0005
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0004
---

# Motor yetenek haritası — "piyasanın en iyisi" ne demek?

> Kanonik değildir. [[90-Inbox/RESEARCH-0001-DatabaseChecker-Genisletme-Analizi|RESEARCH-0001]]'in devamıdır:
> orası *paket ve platform* katmanını, burası **motorun kendisini** ele alır.
> Kanıt sınıfları RESEARCH-0001 §0 ile aynıdır (K1 yerel kod / K2 birincil kaynak / K3 ikincil).

---

## 0. Önce bir tespit: bugün "cross-engine" modu çalışmıyor

Yetenek eklemeden önce kapatılması gereken bir şey var ve üç satırla kanıtlanıyor.

**K1 zinciri:**

1. `SchemaColumnModel.CanonicalDataType` alanı tanımlı ve amacı yazılı:
   *"Ayni-motor karsilastirmasi RawDataType uzerinden KESIN, capraz-motor karsilastirmasi CanonicalDataType uzerinden YAKLASIK yapilir"* ([SchemaColumnModel.cs:6](../../../checkers/database-comparison/src/Ptn.DatabaseChecker.Domain/Models/Comparison/SchemaColumnModel.cs)).
2. Ama **hiçbir yerde doldurulmuyor**. İki okuyucuda da aynı TODO duruyor:
   `// CanonicalDataType: tip-esleme adiminda doldurulacak.`
   (`PostgreSqlDatabaseSchemaDiscoveryRepository.cs:837`, `SqlServerDatabaseSchemaDiscoveryRepository.cs:692`).
3. Motor bu yüzden daima ham tipe düşüyor:
   `GetComparableDataType` → `string.IsNullOrWhiteSpace(CanonicalDataType) ? RawDataType : ...` (`SchemaComparisonManager.cs:490-493`).
4. Buna rağmen çapraz motor kıyası `ComparisonConfidenceCodes.Canonical` damgası alıyor (`SchemaComparisonManager.cs:429-434`).

**Sonuç:** PostgreSQL ↔ SQL Server karşılaştırmasında `character varying(100)` ile `nvarchar(100)`
farklı metinlerdir → **her kolon `Modified` çıkar**. Motor, sahip olmadığı bir yeteneği ilan ediyor ve
o modda %100 yanlış-pozitif üretiyor.

Bu, "piyasanın en iyisi" hedefinin **sıfır numaralı işi**dir. Aşağıdaki M-01 bunu çözüyor.

---

## 1. Asıl mesele: genişlik "daha çok if" ile gelmez

Bugünkü `SchemaComparisonManager` nesne türlerini **elle** sayıyor: tablo, kolon, index, constraint,
trigger, şema nesnesi (`SchemaComparisonManager.cs:49-51, 82-86`). Yeni bir nesne türü eklemek
(partition, policy, materialized view, collation…) bu sınıfı ve alt metotlarını **her seferinde**
değiştirmek demek. 40 nesne türüne çıkıldığında bu sınıf yönetilemez olur ve §2.4'te sayılan
yanlış-pozitif önlemleri bozulur.

Oysa aynı repoda doğru desen zaten var — `EngineComponentResolver<T>`'ın yorumu bunu tarif ediyor:
*"Yeni motor eklemek = IEngineComponent implemente eden yeni sinif yazmak; bu sinifa ve cagiranlara
dokunulmaz (acik/kapali)"* (`EngineComponentResolver.cs:12`).

**M-00 — Nesne türü kaydı (object-type registry).** Aynı prensibi nesne türlerine genişlet:

```csharp
// Motor-ozel okuma: (ObjectTypeCode x EngineCode) basina bir sinif
public interface ISchemaObjectProvider : IEngineComponent
{
    string ObjectTypeCode { get; }                 // "Partition", "Policy", "MaterializedView"...
    CapabilityLevel Capability { get; }            // Full | Partial | Unsupported
    Task<IReadOnlyList<SchemaObjectSnapshot>> ReadAsync(DatabaseConnectionInfo info, SchemaReadPlan plan, CancellationToken ct);
}

// Motor-bagimsiz kiyas: ObjectTypeCode basina bir sinif
public interface ISchemaObjectComparer
{
    string ObjectTypeCode { get; }
    string BuildIdentity(SchemaObjectSnapshot o);       // eslestirme anahtari
    string BuildDefinition(SchemaObjectSnapshot o);     // kanonik tanim metni
    string? BuildChangeSummary(SchemaObjectSnapshot a, SchemaObjectSnapshot b);
    IReadOnlyList<string> NormalizationProfileKeys { get; }
}
```

`SchemaComparisonManager` bunun üzerine **generic bir sürücüye** dönüşür: kayıtlı her nesne türü için
iki tarafı oku → kayıtlı karşılaştırıcı ile kıyasla. Yeni nesne türü = **2 yeni sınıf, 0 düzenleme**.
Mevcut `SchemaCollectionComparer` ve `SchemaDefinitionNormalizer` olduğu gibi kullanılır.

**M-00b — Yetenek matrisi bir API çıktısıdır.** Bu, kimsenin düzgün yapmadığı ve bizi ayıracak şeydir:

```json
GET /api/database-checker/capabilities
{ "engine": "PostgreSql", "version": "16.4",
  "objects": { "Table":"Full", "Partition":"Full", "Policy":"Full",
               "MaterializedView":"Full", "Collation":"Full",
               "Publication":"Partial", "ForeignTable":"Unsupported" } }
```

Gerekçe: "**fark bulunamadı**" cümlesi, ancak neye bakılmadığı da söylendiğinde dürüsttür. Bugün
motor okumadığı bir nesne türünde sessiz kalıyor ve kullanıcı bunu "aynı" sanıyor.

**M-00c — `Unsupported` birinci sınıf bulgu türüdür.** Motor okuyamadığı/karşılaştıramadığı bir nesneyle
karşılaştığında `DifferenceKindCodes.Unsupported` üretmeli; sessiz kalmamalı. Aynı disiplin oasdiff'in
WARN kuralında var: *"A warning is used only when the definition genuinely lacks the information to decide"* (K2).
Bizim karşılığımız: motor **bilmediğini bildirmeli**.

---

## 2. Eksen A — Nesne kapsamı (ne okuyoruz?)

### 2.1 Bugünkü kapsam (K1, kesin)

`SchemaObjectTypeCodes` 14 kod taşıyor: `Table, View, Trigger, Procedure, Function, Column, Index,
PrimaryKey, ForeignKey, Unique, Check, Sequence, Type, Extension`.

### 2.2 Piyasa referansı (K2)

| Araç | Kapsam |
|---|---|
| **Liquibase** varsayılan `diffTypes` | `columns, foreignkeys, indexes, primarykeys, tables, uniqueconstraints, views` — tam liste ayrıca `catalogs, checkconstraints, data, databasepackage(+body), functions, sequences, storedprocedures, triggers` |
| **Redgate SQL Compare** (SQL Server) | Assembly, Asymmetric Key, Certificate, Contract, DDL Trigger, Default, **Extended Property**, Event Notification, Full Text Catalog/Stoplist, Function, Message Type, **Partition Function**, **Partition Scheme**, Queue, **Role**, Route, Rule, Schema, Search Property List, **Security Policy**, Sequence, Service, Service Binding, Stored Procedure, Symmetric Key, Synonym, Table (index/constraint/**filegroup** dahil), **User**, User Defined Type, View, XML Schema Collection, External File Format/Data Source/Table — ayrıca **Temporal Tables** |
| **Atlas** | Tablo/kolon/view'dan **kullanıcı, rol, izin, default privileges ve RLS politikalarına** kadar; "schema drift, **permission mismatches**, policy violations" |

Yani piyasa çıtası: Liquibase bizim biraz altımızda, **Redgate ve Atlas belirgin şekilde üstümüzde** —
ve ikisinin de bizde olmayan ortak alanı **güvenlik/izin nesneleri**.

### 2.3 Eklenmesi önerilen nesne türleri

**Dalga A1 — yüksek etki, düşük maliyet (katalog okuyucuları zaten yarı yolda):**

| Kod | PostgreSQL kaynağı | SQL Server kaynağı | Neden |
|---|---|---|---|
| `MaterializedView` | `pg_class.relkind='m'` | Indexed view (`sys.views` + unique clustered index) | Bugün view sayılıyor ama davranışı bambaşka (yenilenme, index) |
| `Partition` / `PartitionScheme` | `pg_class.relispartition`, `pg_inherits`, partition bound | `sys.partition_functions`, `sys.partition_schemes` | Partition farkı = sessiz veri kaybı riski; Redgate iki nesne türü olarak taşıyor |
| `Collation` | `pg_collation`, `pg_database.datcollate` | `sys.databases.collation_name`, kolon `collation_name` | §4.1 — tek başına bir "öldürücü özellik" |
| `Comment` / `ExtendedProperty` | `pg_description` | `sys.extended_properties` (`MS_Description`) | `DocsOnly` şiddetinin DB tarafındaki karşılığı; Redgate ayrı nesne türü sayıyor |
| `Domain` | `pg_type.typtype='d'` | User-defined type (kural/varsayılan) | Tip kısıtları şemanın parçası |
| `Synonym` | — | `sys.synonyms` | SQL Server'da yaygın; yokluğu sessiz kırılma |

**Dalga A2 — güvenlik yüzeyi (Atlas'ın açık farkı):**

| Kod | Kaynak | Neden |
|---|---|---|
| `Policy` (RLS) | `pg_policy` / `sys.security_policies` | Politika farkı = **veri sızıntısı**. Şema aynı görünürken erişim farklı olabilir |
| `Role` / `Grant` | `pg_roles` + `information_schema.role_table_grants` / `sys.database_principals` + `sys.database_permissions` | "İzin drift'i": ortamlar arası yetki farkı, şema farkından daha tehlikeli olabilir |
| `Owner` | `pg_class.relowner` / `sys.objects` principal | Sahiplik değişimi yetki değişimidir |

**Dalga A3 — uzmanlık nesneleri (talep geldikçe):**
`Publication`/`Subscription` (PG logical replication), `EventTrigger`/`DDL Trigger`,
`Statistics` (`CREATE STATISTICS` / `sys.stats`), `FullTextIndex`, `ForeignTable`/`FDW`,
`Filegroup`, `XmlSchemaCollection`, `Assembly`.

> **Not:** A3 nesnelerinin çoğu için doğru cevap "oku ama varsayılan profilde karşılaştırma" olabilir.
> Kapsam genişliği ile gürültü arasındaki denge §5'teki profil sistemiyle kurulur.

---

## 3. Eksen B — Öznitelik derinliği (okuduğumuzu ne kadar iyi okuyoruz?)

Nesne türü eklemekten **daha yüksek getirili** olan eksen budur: aynı tabloyu daha derin okumak.

### 3.1 Kolon seviyesinde eksikler (K1: `SchemaColumnModel` 10 alan taşıyor)

| Eklenecek alan | PostgreSQL | SQL Server | Neden kritik |
|---|---|---|---|
| `Collation` | `pg_attribute` → `attcollation` | `sys.columns.collation_name` | İki ortamda aynı `varchar` farklı collation ise `WHERE name='ALI'` farklı sonuç döner. **Şema "aynı" görünür, uygulama farklı davranır** |
| `IsGenerated` + `GenerationExpression` | `attgenerated`, `pg_get_expr` | `sys.computed_columns.definition`, `is_persisted` | Hesaplanan kolon bugün sıradan kolon gibi görünüyor; ifade farkı görünmez |
| `IdentitySeed` / `IdentityIncrement` | `pg_sequence` (identity sequence) | `sys.identity_columns` | `IsIdentity` bool yetmez; seed/increment farkı gerçek bir fark |
| `IsSparse` / `Compression` / `Storage` | `attstorage`, `attcompression` | `is_sparse`, `sys.partitions.data_compression` | Depolama farkı performans farkıdır |
| `Comment` | `pg_description` | `MS_Description` | `DocsOnly` şiddeti |
| `MaskingFunction` | — | `sys.masked_columns` | Maskeleme farkı = gizlilik farkı |

### 3.2 Constraint seviyesinde eksikler — **en yüksek getirili tek madde**

Bugün constraint karşılaştırması tür/kolon/hedef/aksiyon/tanım taşıyor
(`SchemaComparisonManager.cs:311-329`) ama **kısıtın geçerli/güvenilir olup olmadığını taşımıyor**.

| Eklenecek alan | Kaynak | Kanıt (K2) |
|---|---|---|
| `IsValidated` (PG `NOT VALID`) | `pg_constraint.convalidated` | PG, CHECK ve FK kısıtlarının `NOT VALID` işaretlenmesine izin verir: kısıt **yeni satırları zorlar, mevcut veriyi doğrulamaz**; tamamlamak için `ALTER TABLE ... VALIDATE CONSTRAINT` gerekir |
| `IsTrusted` (SQL Server) | `sys.foreign_keys.is_not_trusted`, `sys.check_constraints.is_not_trusted` | `WITH NOCHECK` mevcut kısıt için varsayılandır; kısıt **etkin ama doğrulanmamıştır**. SQL Server güvenmediği kısıtı **sorgu planında kullanmaz**; düzeltmesi `WITH CHECK CHECK CONSTRAINT` |
| `IsDeferrable` / `InitiallyDeferred` | `pg_constraint.condeferrable/condeferred` | PG'de yalnız UNIQUE/PK/FK/EXCLUDE ertelenebilir; erteleme farkı transaction davranışını değiştirir |
| `IsEnabled` / `IsDisabled` | — | `sys.foreign_keys.is_disabled` | Devre dışı kısıt = olmayan kısıt |

Bunun anlamı şudur: **"kısıt iki ortamda da var" cümlesi bugün yanıltıcı.** Canlıda `NOT VALID` /
`is_not_trusted` olan bir FK, test ortamındaki doğrulanmış FK ile aynı şey değildir — ne veri
bütünlüğü açısından, ne sorgu planı açısından. Motor bunu görmüyorsa en tehlikeli sessiz farkı kaçırıyor.

### 3.3 Index seviyesinde eksikler

`Fillfactor` / index storage parametreleri, operator class (PG), `IsDisabled` (SQL Server),
`IsPadded`, index tipi (`btree/gin/gist/brin` — PG `pg_am`), `INCLUDE` zaten var, partial/filtered zaten var.
Bunlar Dalga B2'dir; A1/B1'den sonra.

### 3.4 Tablo seviyesinde eksikler

`IsUnlogged` (PG), `IsTemporal` / system-versioned (SQL Server — Redgate açıkça destekliyor),
`IsMemoryOptimized`, tablespace/filegroup, `RowSecurityEnabled` (PG `relrowsecurity`).

---

## 4. Eksen C — Analiz (fark bulmaktan öte)

Buraya kadar olan her şey "daha çok fark bul"du. Piyasa liderliği **farkı yorumlamak**la gelir.

### 4.1 Öldürücü özellik #1 — Collation sürüm sapması

**Kanıt (K2/K3).** glibc 2.28 (1 Ağustos 2018) collation verisini ISO 14651 / Unicode 9.0.0'a taşıdı.
Sonuç PostgreSQL tarihindeki en ciddi veri bütünlüğü olaylarından biri: **index'ler sessizce bozuldu,
sorgu sonuçları uyarısız değişti, unique kısıtlar güvenilmez hale geldi.** PostgreSQL bunu şu uyarıyla
söyler: `WARNING: database "x" has a collation version mismatch ... created using collation version 2.17,
but the operating system provides version 2.28`. Çözüm: etkilenen index'leri `REINDEX CONCURRENTLY` +
`ALTER DATABASE REFRESH COLLATION VERSION`. Ve bu yalnız 2.28'e özgü değil — **her glibc yükseltmesinde** olabilir.

**Öneri.** `CollationDriftCheck`: `pg_database.datcollversion` / `pg_collation.collversion` ile
işletim sisteminin sağladığı sürümü kıyasla; uyuşmazsa `Breaking` şiddetinde bulgu üret ve
etkilenen (metin kolonu içeren) index listesini çıkar. SQL Server tarafında karşılığı:
veritabanı/kolon collation farkı.

Neden öldürücü: bu, iki DB'yi kıyaslayan bir aracın **tek bir DB'ye bakarak** bulabildiği,
gerçekten üretimi düşüren ve çoğu ekibin varlığından haberdar olmadığı bir sorundur. Redgate, Liquibase
ve Bytebase'in şema diff'i bunu bulmaz — çünkü bu bir *diff* değil, bir *sağlık* bulgusudur.

### 4.2 Öldürücü özellik #2 — Şema lint (tek DB, karşılaştırmasız)

Bugün motor yalnız "A ↔ B" sorusunu cevaplıyor. Ama aynı katalog okuyucuları **"bu DB kendi başına
sağlıklı mı?"** sorusunu da bedavaya cevaplayabilir.

**Kanıt (K2).** SchemaCrawler'ın `lint` komutu tam olarak bunu yapar: PK'sı olmayan tablolar,
index'i olmayan tablolar, unique constraint içinde nullable kolon, gereksiz (redundant) index'ler,
ilişkisiz tablolar, silme/ekleme sorunu doğuran döngüsel ilişkiler; kurallar organizasyona göre
genişletilebilir. Atlas ise bunu migration tarafında DS/MF/BC/NM/PG kod kataloğuyla yapıyor;
Squawk aynısını Postgres migration'ları için.

**Öneri — `DatabaseLintManager`, kod tabanlı kural kataloğu:**

| Kod | Kural | Kaynak fikir |
|---|---|---|
| `LNT-101` | PK'sı olmayan tablo | SchemaCrawler |
| `LNT-102` | FK kolonunda index yok (join performansı) | SchemaCrawler / yaygın pratik |
| `LNT-103` | **Doğrulanmamış kısıt** (`NOT VALID` / `is_not_trusted`) | §3.2 |
| `LNT-104` | Redundant index (başka index'in öneki) | SchemaCrawler |
| `LNT-105` | Unique constraint içinde nullable kolon | SchemaCrawler |
| `LNT-106` | Aynı tabloda karışık collation | §4.1 |
| `LNT-107` | Devre dışı trigger / kısıt | — |
| `LNT-108` | İsimlendirme sözleşmesi ihlali (yapılandırılabilir) | Atlas NM101-NM106 |
| `LNT-109` | Yetimlenmiş satır (FK olmadan mantıksal ilişki) — **veri kontrolü, opsiyonel** | Soda/GE tarzı |

Maliyeti düşük (okuyucular hazır), değeri yüksek: ürün "iki ortamı kıyaslayan araç"tan
**"veritabanı güvence platformu"**na dönüşür. Ve tek bağlantısı olan müşteri de kullanabilir —
satış yüzeyi iki katına çıkar.

### 4.3 Öldürücü özellik #3 — Drift atıfı (kim, ne zaman?)

Drift bulmak yarısı; **"bunu kim yaptı"** diğer yarısı. Bugün hiçbir açık kaynak diff aracı bunu
kutudan vermiyor.

**Kanıt (K2/K3).** PostgreSQL **event trigger**'ları DDL ifadelerinde tetiklenir ve şema değişikliği
denetimi, isimlendirme zorlaması, kazara `DROP` engelleme ve değişiklik günlüğü için kullanılır;
pgaudit komut türü, nesne kimliği ve komutu çalıştıran rolü kaydeder. SQL Server tarafında **default trace**
(SQL Server 2005'ten beri) tablo oluşturma/silme gibi şema değişikliklerini denetler — ama bilinen sınırı var:
*bir tabloya kolon eklendiğinde ne zaman olduğu görülür, hangi kolonun eklendiği veya hangi komutun
kullanıldığı görülmez*; bu ayrıntı için **DDL trigger** gerekir.

**Öneri.** `IDriftAttributionProvider` (opsiyonel, en-iyi-çaba):
PG'de event trigger tablosu/pgaudit log'u varsa oku; SQL Server'da default trace'i sorgula.
Bulguya `AttributedTo` + `AttributedAt` + `AttributionConfidence` ekle. **Kurulum şartı değil:**
sağlayıcı yoksa alan boş kalır, motor çalışmaya devam eder. Ayrıca opsiyonel bir "kurulum betiği"
(event trigger + log tablosu) paketle birlikte dokümante edilir — ama **checker onu kendi kurmaz**
(RULE-0002: hedef DB'nin şemasına yazmayız).

### 4.4 Bağımlılık grafiği ve sıralama

Bulgular bugün alfabetik sıralanıyor (`SchemaComparisonManager.cs:535-542`). Piyasa çıtası daha yüksek:
Redgate *"scripting dependencies in your database in the right order"* sunuyor (K2).

**Öneri.** FK, view→tablo, trigger→tablo, tip→kolon bağımlılıklarından bir DAG kur; bulguları
topolojik sırada döndür ve *"bu farkın kapatılması şu 3 nesneyi etkiler"* bilgisini ekle.
SQL üretmiyoruz (bilinçli sınır) ama **etki analizi** üretmek checker'ın işidir.

### 4.5 Rename tespiti

**Kanıt (K2/K3).** Rename ile DROP+ADD'i şemadan **tamamen** ayırt etmek imkânsızdır; sonuç şema
her iki durumda aynıdır — ama istenmeyen bir DROP'un etkisi felakettir. Bazı araçlar bu yüzden hiç
denemez ("heuristics could produce false positives"). Atlas ise **planlama aşamasında olası RENAME'i
tespit edip kullanıcıya niyetini sorar**, otomatik karar vermez.

Bizim api-contract motorumuz şema rename tespitini zaten yapıyor ve `accepted-deviations.json`'da
gerekçesi yazılı: *"oasdiff has no rename detection and reports the old name as removed; we report
schema-renamed instead"*.

**Öneri.** DB tarafında rename tespitini **öneri** olarak üret, karar olarak değil:
aynı tabloda bire bir eşleşen `dropped column` + `added column` çifti; aynı tip, aynı nullable,
aynı default, benzer ordinal → `Modified(kind=PossibleRename, confidence=Heuristic)` +
her iki ham bulgu da raporda kalır. **Ham bulguyu asla gizleme** — Atlas'ın "kullanıcıya sor"
yaklaşımının API karşılığı budur.

### 4.6 Şiddet ve politika

RESEARCH-0001/E-03 bunu detaylandırdı. Buraya eklenmesi gereken tek şey: şiddet **motorun sabiti değil,
profilin parçası** olmalı. Bytebase severity'yi ortam/kapsam/motor başına yapılandırıyor; oasdiff
severity dosyasıyla kural bazında yükseltme/düşürme/kapatma veriyor (K2). Aynısı bizde de olmalı.

---

## 5. Eksen D — Karşılaştırma profilleri (gürültü kontrolü)

Genişlik gürültü demektir. 40 nesne türü okuyan bir motor, profil sistemi olmadan kullanılamaz hale gelir.

**Öneri — `ComparisonProfile` (ayar değil, birinci sınıf kavram):**

| Profil | Ne yapar |
|---|---|
| `Strict` | Her şeyi kıyasla; fillfactor, comment, ordinal dahil. Release öncesi denetim |
| `Deployment` (varsayılan) | Davranışı etkileyen farklar; comment/fillfactor/statistics yok sayılır |
| `CrossEngine` | Yalnız kanonik tip üzerinden; motor-özel nesneler (extension, filegroup) `Unsupported` işaretlenir |
| `Security` | Yalnız A2 kümesi: policy, role, grant, owner, masking |
| `Custom` | Kural bazlı yükseltme/düşürme/kapatma (oasdiff severity dosyası deseni) |

Profil, **normalizasyon** kararlarını da taşır: hangi alan normalize edilir, hangi fark yok sayılır.
Bugün bu kararlar `SchemaDefinitionNormalizer` içinde sabit; profil parametresi haline gelmeli.

---

## 6. Eksen E — Veri karşılaştırma motoru

RESEARCH-0001/E-04 chunked checksum + bisection'ı anlattı. Motor genişliği için üstüne gerekenler:

### 6.1 Tip-farkında karşılaştırma — bugünkü en büyük sessiz hata kaynağı

Bugün her hücre `to_jsonb(row)::text` ile **metne** çevrilip kıyaslanıyor
(`PostgreSqlDatabaseDataComparisonRepository.cs:30-31`). Metin kıyası şu farkları **uydurur**:

| Tuzak | Kanıt (K2/K3) |
|---|---|
| Kayan nokta gösterimi | Karakter/NUMBER ile kayan nokta arasındaki dönüşümler **kesin değildir** — biri ondalık, diğeri ikili hassasiyet kullanır |
| Ondalık ölçek | `12.34` ile `12.340` aynı değerdir, farklı metindir |
| Zaman damgası hassasiyeti | Oracle 9 basamak, MySQL 6 basamak kesirli saniye; motorlar arası kayıp olur |
| Zaman dilimi | `timestamptz` kıyasları önce **UTC'ye normalize edilir**; hedef tip `datetime` ise saat dilimi bilgisi kaybolur |
| Metin sonundaki boşluk / collation | `char` dolgusu ve collation'a bağlı eşitlik |

**Öneri.** `ValueComparisonPolicy` (kolon tipi başına): sayısal tolerans (`epsilon` veya ölçek
normalizasyonu), zaman damgası hassasiyeti kırpma, timezone normalizasyonu (hepsi UTC), metin
normalizasyonu (trim/case/collation-farkında), JSON **semantik** eşitlik (anahtar sırası önemsiz),
binary hash. Kanonik satır metni bu politikadan **sonra** üretilir — mevcut `BuildCanonicalRow`
disiplini korunur, girdisi düzelir.

### 6.2 Alt küme ve filtre

- **Zaman penceresi:** `WHERE updated_at >= now() - interval '1 day'` → nightly kıyası dakikalara indirir.
- **Kolon alt kümesi / dışlama:** `updated_at`, `row_version` gibi doğası gereği farklı kolonları dışla.
- **Anahtar aralığı:** belirli PK aralığı.
- **Örnekleme (`Sample` modu):** PostgreSQL `TABLESAMPLE SYSTEM` (blok seviyesi, hızlı, sapmalı) ve
  `BERNOULLI` (satır seviyesi, yavaş, düzgün dağılım) — SQL:2003'te tanımlı iki yöntem (K2).
  **Kural:** örnekleme sonucu asla "eşit" diye raporlanmaz; `SampledMatch(confidence=n%)` olarak raporlanır.

### 6.3 Ucuz ön kontrol

`pg_class.reltuples` / `sys.dm_db_partition_stats` üzerinden **tahmini** satır sayısı ile hızlı bir
"ilk bakış" ver; kesin `COUNT(*)` yalnız gerektiğinde. (`CountExpression` bugün her zaman kesin sayım
yapıyor — `PostgreSqlDatabaseDataComparisonRepository.cs:27`.)

### 6.4 Referans bütünlüğü kontrolü

FK olmadan mantıksal ilişkiler için "yetim satır" kontrolü — Great Expectations / Soda Core'un
veri kalitesi kontrol ailesinin DB tarafındaki karşılığı. Lint kataloğuna `LNT-109` olarak girer.

---

## 7. Eksen F — Operasyonel olgunluk

| Yetenek | Neden | Not |
|---|---|---|
| **İptal (cancellation)** | Uzun run'ın durdurulabilmesi | Repository imzalarında `CancellationToken` **zaten var**; job seviyesine kadar bağlanmalı |
| **İlerleme (progress)** | "5.000 tablodan 1.200'ü" | MCP `progressToken` ve Tasks ile birebir eşleşir (RESEARCH-0001 §6.6) |
| **Yeniden başlatılabilirlik** | 40 dakikalık run 39. dakikada patlamamalı | Nesne türü/şema bazında checkpoint |
| **Paralellik + backpressure** | Canlı DB'yi yormadan hız | pt-table-checksum'ın **0,5 sn hedefli adaptif chunk** yaklaşımı (K2) doğrudan uygulanabilir |
| **Artımlı kıyas** | Fingerprint (E-01) eşitse o dalı hiç açma | Merkle ağacının doğal getirisi |
| **Bulgu akışı (streaming)** | 50.000 farklı run'da tek JSON'a sığmama | Bugün `findings` tek jsonb; sayfalama gerekir |

---

## 8. Eksen G — Motor kapsamı (hangi veritabanları?)

Bugün: PostgreSQL + SQL Server.

**Kanıt (K2).** Atlas tek üründe PostgreSQL, MySQL, MariaDB, SQL Server, SQLite, ClickHouse, Redshift,
Oracle, Snowflake, CockroachDB, TiDB, Databricks, Spanner, Aurora DSQL, Azure Fabric destekliyor.
Redgate ise **ayrı ürünler** satıyor: SQL Compare (SQL Server), Schema Compare for Oracle,
Schema Compare for MySQL, pgCompare (PostgreSQL).

Bu ikinci gözlem stratejik: **tek motorun çok veritabanını tutarlı bir bulgu modeliyle kapsaması,
piyasada başlı başına bir farktır.** Bizim `EngineComponentResolver` mimarimiz bunun için tasarlanmış.

**Öneri sıra:** MySQL/MariaDB (pazar büyüklüğü + `information_schema` kolaylığı) → Oracle (kurumsal talep)
→ SQLite (test/gömülü senaryolar, Testcontainers'sız hızlı test fikstürü).

**Uyarı:** Yeni motor eklemeden önce **M-00 registry ve M-01 kanonik tip haritası** bitmiş olmalı.
Aksi halde her yeni motor, §0'daki hatayı bir kez daha üretir.

---

## 9. Öncelik: neyi önce yaparsak "en iyi" oluruz?

Sıralama, *kapsam genişliği* ile *güvenilirlik* arasındaki gerilime göre yapıldı. Güvenilmez geniş bir
motor, dar ve doğru bir motordan **daha kötüdür**.

### Dalga M1 — "Motor doğru mu?" (bunlar bitmeden genişleme yok)

| # | İş | Gerekçe |
|---|---|---|
| **M-01** | **Kanonik tip haritası** — `CanonicalDataType`'ı gerçekten doldur; tip ailesi + genişlik + hassasiyet + işaret; eşleşmeyen tipe `Unsupported` | §0: cross-engine modu bugün çalışmıyor |
| **M-02** | **Kısıt güvenilirliği** — `IsValidated` (PG `convalidated`) / `IsTrusted` (`is_not_trusted`) / `IsEnabled` | §3.2: en tehlikeli sessiz fark |
| **M-03** | **Collation** — kolon + veritabanı collation'ı ve **collation sürüm sapması** | §4.1: gerçek üretim felaketi |
| **M-00** | **Nesne türü kaydı + yetenek matrisi + `Unsupported` bulgu türü** | §1: bundan sonraki her genişleme bunun üstüne biner |

### Dalga M2 — "Motor geniş mi?"

| # | İş |
|---|---|
| M-04 | Nesne türleri Dalga A1: `MaterializedView`, `Partition`, `Comment`, `Domain`, `Synonym` |
| M-05 | Kolon derinliği: generated/computed, identity seed/increment, comment, storage |
| M-06 | Karşılaştırma profilleri (`Strict` / `Deployment` / `CrossEngine` / `Security` / `Custom`) |
| M-07 | Şiddet sınıflandırıcı (RESEARCH-0001/E-03) + bulgu fingerprint (E-02) |

### Dalga M3 — "Motor akıllı mı?"

| # | İş |
|---|---|
| M-08 | **Şema lint kataloğu** (`LNT-1xx`) — tek DB'ye değer üretir, satış yüzeyini genişletir |
| M-09 | Güvenlik nesneleri Dalga A2: `Policy`, `Role`/`Grant`, `Owner` |
| M-10 | Tip-farkında veri karşılaştırma politikası + filtre/pencere/örnekleme |
| M-11 | Chunked checksum + bisection (RESEARCH-0001/E-04) |
| M-12 | Bağımlılık grafiği + etki analizi; rename **önerisi** |

### Dalga M4 — "Motor kurumsal mı?"

| # | İş |
|---|---|
| M-13 | Drift atıfı (event trigger / default trace) |
| M-14 | Operasyonel olgunluk: iptal, ilerleme, checkpoint, paralellik, bulgu sayfalama |
| M-15 | Yeni motorlar: MySQL/MariaDB → Oracle → SQLite |
| M-16 | Differential oracle + Testcontainers matrisi (RESEARCH-0001/E-12) — **her dalgada koşar, sonda değil** |

---

## 10. "Piyasanın en iyisi" iddiasının tek cümlelik testi

Bu motor şu beş cümleyi aynı anda söyleyebildiğinde piyasada eşi olmaz:

1. *"İki ortam arasında **şu** farklar var"* — bugün büyük ölçüde var.
2. *"Bu farkların **hangileri kırıcı**, hangileri değil"* — M-07.
3. *"Neye **bakmadığımı** da söylüyorum"* — M-00 yetenek matrisi + `Unsupported`.
4. *"Tek bir DB'ye bakıp bile **şunlar sağlıksız** diyebilirim"* — M-08 lint + M-03 collation.
5. *"Bunu **kim, ne zaman** yaptı"* — M-13.

3. madde, 1. maddeden daha değerlidir. Fark listesi üreten çok araç var; **kapsamının sınırını dürüstçe
ilan eden** araç neredeyse yok.

---

## 11. Kaynaklar (bu belgeye özel; ortak kaynaklar RESEARCH-0001 §8'de)

| Kaynak | Neyi kanıtlıyor |
|---|---|
| https://www.postgresql.org/docs/current/catalog-pg-constraint.html | `convalidated`, `condeferrable`, `condeferred` |
| https://www.postgresql.org/docs/current/sql-set-constraints.html | Ertelenebilir kısıt semantiği |
| https://wiki.postgresql.org/wiki/Collations | Collation ve sürüm bağımlılığı |
| https://www.crunchydata.com/blog/glibc-collations-and-data-corruption | glibc 2.28 index bozulması, etkilenen index tespiti |
| https://www.cybertec-postgresql.com/en/icu-collations-against-postgresql-data-corruption/ | ICU ile azaltma; her glibc yükseltmesinde risk |
| https://blog.sqlauthority.com/2015/01/09/sql-server-what-is-is_not_trusted-in-sys-foreign_keys/ | `is_not_trusted` anlamı |
| https://www.brentozar.com/blitz/foreign-key-trusted/ | Güvenilmeyen kısıtın plan etkisi ve `WITH CHECK CHECK CONSTRAINT` düzeltmesi |
| https://documentation.red-gate.com/sc/setting-up-the-comparison/which-objects-can-be-compared | Redgate SQL Compare nesne listesi (piyasa çıtası) |
| https://docs.liquibase.com/community/reference-guide-5-1/database-inspection-change-tracking-and-utility-commands/diff | Liquibase `diffTypes` listesi ve varsayılanları |
| https://atlasgo.io/monitoring | Atlas: şema + **izin** + politika izleme |
| https://atlasgo.io/guides/security-as-code | Roller, izinler, RLS'in kod olarak yönetimi |
| https://atlasgo.io/blog/2024/05/01/atlas-v-0-22 | Rename tespitinin **kullanıcıya sorularak** yapılması |
| https://www.schemacrawler.com/lint.html | Lint kural ailesi (PK yok, index yok, redundant index, nullable-in-unique, döngüsel ilişki) |
| https://wiki.postgresql.org/wiki/Table_partitioning | `relispartition`, `pg_inherits`, partition bound |
| https://www.postgresql.org/docs/current/tablesample-method.html | `TABLESAMPLE` SYSTEM/BERNOULLI |
| https://neon.com/guides/schema-change-log | PG event trigger ile şema değişiklik günlüğü |
| https://www.mssqltips.com/sqlservertip/4057/capture-sql-server-schema-changes-using-the-default-trace/ | Default trace ile şema değişikliği yakalama ve **sınırı** |
| https://docs.oracle.com/en/database/oracle/oracle-database/18/sqlrf/Data-Type-Comparison-Rules.html | Kayan nokta / karakter dönüşümünün kesin olmaması, `timestamptz` UTC normalizasyonu |
