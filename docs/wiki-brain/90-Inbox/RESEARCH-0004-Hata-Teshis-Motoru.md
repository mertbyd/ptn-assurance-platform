---
id: RESEARCH-0004
type: research
status: draft
title: Dinamik veritabani hata teshis motoru — tasarim ve kanit
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

# Dinamik hata teşhis motoru

> Kanonik değildir. Test Module'ün **"bu hata neden oldu?"** sorusunu deterministik olarak cevaplayan
> motorun tasarımı. [[90-Inbox/PLAN-0001-DbChecker-Ozellik-Listesi|PLAN-0001]]'e yeni maddeler ekler (§11).
> Kanıt sınıfları RESEARCH-0001 §0 ile aynı.

---

## 1. Problem

Test Module bir senaryo koştuğunda karşılaşacağı hatalar tek tip değil:

| Hata ailesi | Örnek |
|---|---|
| Bütünlük kısıtı | FK ihlali, unique ihlali, NOT NULL, CHECK |
| Veri uyumsuzluğu | tip dönüşümü, string truncation, sayısal taşma, tarih/timezone |
| Şema uyumsuzluğu | kolon yok, tablo yok, tip değişmiş, generated kolona yazma |
| **SQL/DB konfigürasyonu** | `search_path`, collation, isolation level, `ANSI_NULLS`, timeout, `standard_conforming_strings` |
| Eşzamanlılık | deadlock, serialization failure, lock timeout |
| Yetki | insufficient privilege, RLS politikası, metadata görünürlüğü |
| **Assertion başarısızlığı** | "satır 5 sn içinde oluşmadı", "kolon değeri beklenenden farklı" |

Kullanıcının şartı net: **"bir şeyleri eşleyerek değil, dinamik bir yapı."** Yani
`if (errorNumber == 547) return "Foreign key hatası";` **yasak**.

---

## 2. "Dinamik" ne demek — ve statik eşleme neden yetmez?

### 2.1 Statik eşlemenin duvarı

Piyasadaki en olgun örnek **EntityFramework.Exceptions** (Giorgi): tüm DB istisnalarını
`DbUpdateException` içinden çıkarıp `UniqueConstraintException`, `ReferenceConstraintException`,
`CannotInsertNullException`, `MaxLengthExceededException`, `NumericOverflowException`,
`DeadlockException` gibi tipli istisnalara çeviriyor; SQL Server, PostgreSQL, SQLite, Oracle, MySQL
destekliyor ve `ConstraintName` + `ConstraintProperties` veriyor (K2).

Bu, doğru ve faydalı bir katman — **ama teşhis değil, sınıflandırma**. Sana "bu bir FK ihlaliydi"
der; **"neden"** demez. Nitekim projenin kendi issue'sunda sınırın adı konmuş: *"No easy way to
determine which constraint failed."*

**Sınıflandırma ≠ teşhis.** Bizim istediğimiz ikincisi.

### 2.2 Dinamikliğin üç kaynağı

Bir teşhis motoru, bilgiyi kodda taşımadığı ölçüde dinamiktir. Bizim üç canlı bilgi kaynağımız var
ve **üçü de zaten elimizde**:

| Kaynak | Ne verir | Nereden |
|---|---|---|
| **1. Motorun hata sözlüğü** | Kodun anlamı, mesaj şablonu | PG `errcodes` / MSSQL `sys.messages` — **veritabanının kendisinden okunur** |
| **2. Canlı katalog** | Kısıt/kolon/index/tip/collation gerçeği | DB Checker'ın mevcut discovery repository'leri |
| **3. Karşılaştırma bulguları** | "Bu nesne referans ortamdan farklı mı?" | ComparisonRun findings + fingerprint (DBC-09) |

Bu üçü birleştiğinde, kodda tek bir `switch` olmadan şu cümle kurulabilir:

> *"`FK_Orders_Customers` kısıtı ihlal edildi. Bu kısıt `sales.Orders.CustomerId → sales.Customers.Id`
> üzerinde ve `ON DELETE NO ACTION`. Değeri `7f3a…` olan parent satır hedef veritabanında **yok**.
> Ayrıca bu kısıt referans ortamda `NOT VALID` durumda (bulgu `a1b2c3`), yani orada mevcut veri
> hiç doğrulanmamış — bu yüzden aynı veri orada hata vermiyor."*

Bu cümlenin hiçbir parçası kodda sabit değildir.

---

## 3. Mimari: teşhis bir arama, bir arama tablosu değil

Akademik cephede bu problemin adı **abductive diagnosis / fault localization**'dır. Yaklaşım tutarlı:
hipotezleri sabit etiketlerden okumak yerine **hipotez üret → kanıt topla → tutarsızları ele → sırala**.
2026 tarihli AgentRCA çalışması bunu açıkça böyle formüle ediyor: *"iteratively gathers statistical
evidence, compares fault hypothesis, and systematically rules out explanations that are inconsistent
with the observed behavior"*. JustDiag! ise **gerekçelendirmenin kendisini** nesne yapıyor: kanıt,
bulgu, iddia, hipotez ve değerlendirme ayrı ayrı incelenebilir kayıtlar (K2).

Bizim boru hattımız yedi adımdır ve **her adım ayrı bir bileşen ailesidir**:

```text
[1] YAKALA      FailureSignal        (provider-notr sinyal)
       ↓
[2] KİMLİKLE    FailureIdentity      (kod + nesne referanslari, PARSE DEGIL EXTRACT)
       ↓
[3] YERELLEŞTİR ResolvedContext      (canli katalogdan gercek: kisit, kolon, tip, collation)
       ↓
[4] HİPOTEZ ÜRET  Hypothesis[]       (uygulanabilirlik yordamlari — switch YOK)
       ↓
[5] KANIT TOPLA   Evidence[]         (sinirli, salt-okunur, katalogdan uretilmis probe'lar)
       ↓
[6] SIRALA        RankedHypothesis[] (Confirmed / Likely / Possible / RuledOut)
       ↓
[7] ANLAT         DiagnosisReport    (RFC 9457 + ABP RemoteServiceErrorInfo + lokalizasyon)
```

---

## 4. Adım adım tasarım

### [1] Yakala — `FailureSignal`

Sinyal dört kaynaktan gelebilir; hepsi tek değer nesnesine indirgenir:

| Kaynak | Nasıl yakalanır | Kanıt |
|---|---|---|
| `DbException` (PostgresException / SqlException) | EF Core **interceptor**: `ISaveChangesInterceptor.SaveChangesFailedAsync` ve `IDbCommandInterceptor.CommandFailedAsync` | K2 — ancak **bilinen sınır**: batch'in ilk ifadesi başarılıysa ve sonraki patlarsa `CommandFailedAsync` **çağrılmaz**; bu yüzden `SaveChangesFailed` da dinlenmeli |
| ABP katmanındaki her istisna | `IExceptionSubscriber` / `ExceptionSubscriber.HandleAsync(ExceptionNotificationContext)` | K2 |
| SUT'un HTTP hatası | Yanıt gövdesi RFC 9457 ise `type/title/status/detail/instance` + extension member'lar | K2 |
| Kendi assertion'ımız | `AssertRowAsync` sonucu `Fail` (DBC-05) | K1 |

```csharp
// Domain/Models/Diagnosis/FailureSignal.cs  (deger nesnesi, kalici degil)
public sealed class FailureSignal
{
    public string SourceKindCode { get; init; }      // DbException | Assertion | HttpProblem
    public string? EngineCode { get; init; }         // PostgreSql | SqlServer
    public string? RawCode { get; init; }            // "23503" | "547"
    public string? RawMessage { get; init; }         // lokalize olabilir — KARAR VERICI DEGIL
    public IReadOnlyDictionary<string, string?> ProviderFields { get; init; }
    public Guid? ConnectionId { get; init; }
    public Guid? ScenarioRunId { get; init; }
    public DateTime OccurredAt { get; init; }
}
```

> **Kural:** `RawMessage` hiçbir kararın girdisi değildir. Lokalize edilebilir, sürümle değişir,
> müşteri verisi içerebilir. Yalnızca son raporda **kanıt** olarak, redaction'dan geçirilerek taşınır.

### [2] Kimlikle — `IFailureIdentityExtractor` (motor başına bir sınıf)

Bu, tasarımın en teknik ve en "dinamik olmalı" noktası. İki motor iki farklı gerçeğe sahip:

#### PostgreSQL — yapılandırılmış alanlar, parse yok

Npgsql `PostgresException` şu alanları **doğrudan** verir: `SqlState`, `ConstraintName`, `TableName`,
`ColumnName`, `SchemaName`, `DataTypeName`, `Detail`, `Hint` (K2). PostgreSQL dokümanı amacını da yazar:
*"Such names are supplied in separate fields of the error report message so that applications need not
try to extract them from the possibly-localized human-readable text of the message."*

**Ama dokümante edilmiş bir sınır var ve tasarımın ona saygı duyması gerekir:**
> *"As of PostgreSQL 9.3, complete coverage for this feature exists only for errors in SQLSTATE
> class 23 (integrity constraint violation), but this is likely to be expanded in future."*

Yani: **class 23 → yüksek güvenli yapılandırılmış kimlik**; diğer sınıflar → alanlar olabilir de
olmayabilir de, kimlik güveni düşer. Motor bunu `IdentityConfidence` olarak taşımalı.

İlgili sınıflar (K2, `errcodes-appendix`):
`23502 not_null_violation` · `23503 foreign_key_violation` · `23505 unique_violation` ·
`23514 check_violation` · `23001 restrict_violation` · `23P01 exclusion_violation` ·
`22001 string_data_right_truncation` · `22003 numeric_value_out_of_range` · `22007 invalid_datetime_format` ·
`22P02 invalid_text_representation` · `40001 serialization_failure` · `40P01 deadlock_detected` ·
`55P03 lock_not_available` · `42501 insufficient_privilege` · `42703 undefined_column` ·
`42P01 undefined_table` · `42804 datatype_mismatch` · `42P21 collation_mismatch` · `428C9 generated_always`

Bu liste **kodda sabitlenmez**; `errcodes` sınıf/koşul adları bir seed lookup'ına (`FailureCode`) girer
ve gerekirse hedef sürümden doğrulanır. Sabit olan tek şey **sınıf davranışı**dır (ilk iki karakter),
ki bunu standart tanımlar: *"the first two characters of an error code denote a class of errors."*

#### SQL Server — şablondan regex üretimi (asıl dinamik hile)

SQL Server, nesne adlarını yapılandırılmış alanda **vermez**; mesaj metnine gömer. Naif çözüm
her hata numarası için elle regex yazmaktır — tam da yasaklanan şey.

**Dinamik çözüm:** `sys.messages` zaten mesaj **şablonlarını** yer tutucularla (`%ls`, `%.*ls`, `%d`)
ve **dil bazında** tutar (K2). Dolayısıyla:

```text
1. Baglanti basina bir kez:  SELECT message_id, language_id, severity, text FROM sys.messages
                             WHERE language_id = <oturum dili>          -> onbellege alinir
2. Sablon -> regex:          literal parcalar escape edilir,
                             %d      -> (?<p{n}>-?\d+)
                             %ls/%.*ls/%s -> (?<p{n}>.*?)
3. Gercek mesaj eslestirilir -> yakalanan gruplar = parametreler
4. ETIKETLEME: her parametrenin anlami, sablondaki ONUNDEKI literal kelimeden turetilir
               ("constraint", "table", "column", "database")
5. DOGRULAMA: cikarilan ad canli katalogda aranir (sys.objects / sys.columns).
              Bulunursa IdentityConfidence = High; bulunmazsa Low ve ad ATILIR.
```

Beşinci adım kritik: **çıkarılan hiçbir ad, katalogda doğrulanmadan kullanılmaz.** Böylece regex
yanlış eşleşse bile motor uydurma bir cevap vermez — güveni düşürür ve o hipotezi elemez.

İlgili numaralar (K2): `547` FK/CHECK çakışması · `2627` PK/UNIQUE kısıt ihlali ·
`2601` unique index'te tekrar eden anahtar · `515` NOT NULL kolona NULL · `8152`/`2628` string truncation ·
`245` dönüşüm hatası · `1205` deadlock kurbanı · `1222` lock request timeout · `8115` aritmetik taşma.
Bunlar da lookup verisidir, kod değil.

```csharp
// Domain/Interface/Diagnosis/IFailureIdentityExtractor.cs
public interface IFailureIdentityExtractor : IEngineComponent   // mevcut resolver deseni
{
    Task<FailureIdentity> ExtractAsync(FailureSignal signal, CancellationToken ct);
}
```

`FailureIdentity` çıktısı: `EngineCode`, `Code`, `CodeClass`, `ConditionName`,
`ObjectRefs[] { Kind (Constraint|Table|Column|Index|Type|Schema), Name, Verified }`, `IdentityConfidence`.

### [3] Yerelleştir — `ResolvedContext` (yeni SQL **yazılmaz**)

Bu adımda tek satır yeni katalog sorgusu yazmıyoruz: DB Checker'ın discovery repository'leri
zaten kısıt, kolon, index, tip, sequence ve (DBC-12/13/15 sonrası) `IsValidated`/`IsTrusted`/collation/
generated bilgisini okuyor. DBC-11'in `ISchemaObjectProvider` kaydı burada **ikinci kez** işe yarar.

Örnek: `ConstraintName = FK_Orders_Customers` →
```text
type=ForeignKey, table=sales.Orders, columns=[CustomerId],
referencedTable=sales.Customers, referencedColumns=[Id],
onDelete=NoAction, onUpdate=NoAction, isValidated=true, isEnabled=true
```

Buna ek olarak **karşılaştırma bulgusu** aranır: aynı nesne için `DifferenceKind`/severity var mı?
(DBC-09 fingerprint eşleşmesi). Varsa `ResolvedContext.RelatedFindings` dolar.

### [4] Hipotez üret — `IDiagnosisRule` (switch yok, bileşen var)

Her hipotez **ayrı bir sınıftır** ve uygulanabilirliğini kendisi söyler. Motorun kendisi hipotezleri
bilmez; DI konteynerinden toplar — `EngineComponentResolver<T>` ile aynı açık/kapalı desen
(`EngineComponentResolver.cs:12`: *"Yeni motor eklemek = yeni sinif yazmak; bu sinifa ve cagiranlara dokunulmaz"*).

```csharp
public interface IDiagnosisRule : ITransientDependency
{
    string HypothesisKindCode { get; }
    int Priority { get; }                                   // esit kanitta siralama
    bool AppliesTo(FailureIdentity id, ResolvedContext ctx); // YORDAM, esleme tablosu degil
    IReadOnlyList<ProbeRequest> RequiredProbes(FailureIdentity id, ResolvedContext ctx);
    HypothesisAssessment Assess(FailureIdentity id, ResolvedContext ctx, EvidenceSet ev);
}
```

`AppliesTo` **kod eşitliğine değil, olgulara** bakar. Örnekler:

```csharp
// Yanlis (yasak):   id.Code == "23503"
// Dogru:            id.CodeClass == FailureCodeClass.IntegrityConstraint
//                   && ctx.Constraint?.TypeCode == SchemaConstraintTypeCodes.ForeignKey
```

Böylece aynı kural PostgreSQL `23503` ve SQL Server `547` için **tek sınıfla** çalışır; üçüncü motor
eklendiğinde kural dosyasına dokunulmaz.

#### Başlangıç hipotez ailesi

**A. Bütünlük kısıtı (FK)**

| Kod | Hipotez | Kanıt (probe) |
|---|---|---|
| `H-FK-01` | Parent satır gerçekten yok | `SELECT 1 FROM parent WHERE key = @v` |
| `H-FK-02` | Parent var ama **başka tenant/partition**'da | Aynı sorgu tenant filtresi kapalı |
| `H-FK-03` | Parent var ama **commit edilmemiş** (görünürlük) | İzolasyon seviyesi + aktif transaction sayımı |
| `H-FK-04` | Tip/collation uyuşmazlığı (`char(10)` dolgusu, farklı collation) | FK kolon çiftinin kanonik tipi + collation (DBC-01/13) |
| `H-FK-05` | Kısıt bu ortamda var, **referans ortamda yok** | `RelatedFindings` (`OnlyInTarget`) |
| `H-FK-06` | Kısıt `NOT VALID` / `is_not_trusted` — eski veri hiç doğrulanmamış | `IsValidated`/`IsTrusted` (DBC-12) |
| `H-FK-07` | Adım sırası yanlış (child parent'tan önce) | Senaryo planındaki adım sırası |
| `H-FK-08` | Parent, `ON DELETE` davranışı yüzünden silinmiş | `onDelete` + silme izi |

**B. Unique / PK**

`H-UQ-01` gerçek tekrar · `H-UQ-02` case/collation duyarlılığı (`ci` vs `cs`) · `H-UQ-03` trailing space /
`char` dolgusu · `H-UQ-04` sequence/identity senkron dışı (`setval` gerideyse) · `H-UQ-05` partial/filtered
index koşulu farkı · `H-UQ-06` test verisi temizlenmemiş (önceki koşudan kalıntı).

**C. NOT NULL / CHECK**

`H-NN-01` uygulama alanı göndermedi · `H-NN-02` kolonun default'u referans ortamda var, burada yok ·
`H-NN-03` kolon referans ortamda nullable (`RelatedFindings`) · `H-NN-04` generated/computed kolona
yazma denemesi (`428C9 generated_always`, DBC-15).

**D. Veri uyumsuzluğu**

`H-DT-01` string truncation — hedef `MaxLength` kaynaktan küçük · `H-DT-02` sayısal taşma —
precision/scale farkı · `H-DT-03` tarih/timezone — `timestamptz` ↔ `datetime` dönüşümü ·
`H-DT-04` metin gösterimi (`22P02`) — kültür/format · `H-DT-05` kanonik tip ailesi farklı (DBC-01).

**E. Konfigürasyon** *(kullanıcının özel olarak istediği aile — §7)*

`H-CF-01` `search_path` farkı → yanlış şemadaki nesne · `H-CF-02` veritabanı/kolon collation farkı ·
`H-CF-03` `ANSI_NULLS`/`QUOTED_IDENTIFIER`/`ARITHABORT` oturum ayarı · `H-CF-04` isolation level ·
`H-CF-05` `statement_timeout`/`lock_timeout` · `H-CF-06` timezone/`DATEFIRST` · `H-CF-07` sunucu
sürümü farkı (özellik yok).

**F. Eşzamanlılık**

`H-CC-01` deadlock (`40P01` / `1205`) — kilit sırası · `H-CC-02` serialization failure (`40001`) —
yeniden deneme gerekli · `H-CC-03` lock timeout (`55P03` / `1222`).

**G. Yetki / görünürlük**

`H-PR-01` `insufficient_privilege` · `H-PR-02` RLS politikası satırı gizliyor (DBC-16) ·
`H-PR-03` metadata görünürlüğü (SQL Server `VIEW DEFINITION` yok → nesne "yok" görünüyor).

**H. Assertion başarısızlığı** *(Test Module'ün en sık hatası)*

`H-AS-01` satır hiç oluşmadı · `H-AS-02` oluştu ama **geç** (timeout'tan sonra — `ObservedAtMs`, DBC-06) ·
`H-AS-03` oluştu ama başka değerle · `H-AS-04` başka tenant'ta · `H-AS-05` replica gecikmesi ·
`H-AS-06` beklenen kolon artık yok/adı değişti (`RelatedFindings` + rename önerisi, DBC-24) ·
`H-AS-07` senaryo kendi verisini temizlememiş.

### [5] Kanıt topla — `IDiagnosisProbe` (sınırlı, salt-okunur, katalogdan üretilmiş)

```csharp
public interface IDiagnosisProbe : IEngineComponent
{
    string ProbeKindCode { get; }                          // RowExists | RowCount | SettingValue | LockGraph ...
    Task<Evidence> RunAsync(ProbeRequest request, DatabaseConnectionInfo info, CancellationToken ct);
}
```

**Pazarlıksız kurallar:**

1. **Salt okuma.** Probe `READ ONLY` transaction içinde çalışır (DBC-03).
2. **Serbest SQL yok.** Probe SQL'i, katalogda **doğrulanmış** nesne adlarından ve parametreli
   değerlerden üretilir; kullanıcı metni SQL'e girmez.
3. **Bütçe.** Teşhis toplamı için `MaxProbeCount` + `MaxProbeDurationMs` + probe başına
   `statement_timeout`. **Teşhis ikinci bir kesinti olamaz.**
4. **Redaction.** Probe sonucundaki değerler DBC-02 politikasına tabidir; varsayılan `None`.
5. **İdempotent.** Probe hiçbir durumu değiştirmez, sıcaklık ölçer.

Probe aileleri: `RowExists`, `RowCountByFilter`, `SequenceCurrentValue`, `SettingValue`
(`pg_settings` / `sys.configurations` / `SESSIONPROPERTY`), `ActiveLocks` (`pg_locks` / `sys.dm_tran_locks`),
`RecentDeadlock` (SQL Server `system_health` XE `xml_deadlock_report` — 2008'den beri varsayılan açık, K2),
`ObjectPrivileges`, `RlsPolicyEffect`, `CatalogFact` (zaten okunan katalogdan türetilmiş, sorgusuz).

### [6] Sırala — güven merdiveni

| Seviye | Anlamı |
|---|---|
| `Confirmed` | Bir probe hipotezi **kanıtladı** (ör. parent satır gerçekten yok) |
| `Likely` | Dolaylı kanıt destekliyor, çelişki yok |
| `Possible` | Uygulanabilir ama ayırt edici kanıt yok |
| `RuledOut` | Bir probe hipotezi **çürüttü** |

Rapor `Confirmed` → `Likely` → `Possible` sırasıyla döner, `RuledOut` olanlar **gizlenmez**;
"neyi eledik" bilgisi teşhisin güvenilirliğinin bir parçasıdır (JustDiag!'in "reviewable objects" ilkesi, K2).

**Kural:** birden fazla `Confirmed` varsa hiçbiri tek "kök neden" ilan edilmez; hepsi listelenir.
Tek-kök-neden dayatması teşhis motorlarının klasik hatasıdır.

### [7] Anlat — `DiagnosisReport`

**Taşıma formatı: RFC 9457 Problem Details** (RFC 7807'yi geçersiz kılar; `type`, `title`, `status`,
`detail`, `instance` + genişletme üyeleri; istemciler tanımadıkları uzantıları yok saymalıdır — K2).

```jsonc
{
  "type": "https://checknexus.dev/problems/db-integrity-violation",
  "title": "Foreign key constraint violated",
  "status": 409,
  "detail": "FK_Orders_Customers ihlal edildi.",
  "instance": "/scenario-runs/9f2c.../steps/3",
  "checknexus:identity": { "engine": "PostgreSql", "code": "23503",
                            "condition": "foreign_key_violation", "confidence": "High" },
  "checknexus:location": { "schema": "sales", "table": "Orders",
                            "constraint": "FK_Orders_Customers", "columns": ["CustomerId"] },
  "checknexus:hypotheses": [
    { "kind": "H-FK-01", "confidence": "Confirmed",
      "statement": "Referans satir hedef veritabaninda yok.",
      "evidence": [ { "probe": "RowExists", "result": false } ] },
    { "kind": "H-FK-06", "confidence": "Likely",
      "statement": "Kisit referans ortamda NOT VALID; orada mevcut veri hic dogrulanmamis.",
      "evidence": [ { "finding": "a1b2c3d4", "severity": "Warning" } ] },
    { "kind": "H-FK-04", "confidence": "RuledOut",
      "statement": "Tip/collation uyusmazligi.",
      "evidence": [ { "probe": "CatalogFact", "result": "uuid == uuid" } ] }
  ],
  "checknexus:nextChecks": [ "sales.Customers icinde Id=7f3a... satirini olusturan adimi kontrol et" ]
}
```

**ABP entegrasyonu — yeniden icat etmeden:** ABP'nin `BusinessException` yapısı `IHasErrorCode`,
`IHasErrorDetails`, `IHasLogLevel` taşır; hata kodu `<namespace>:<code>` biçimindedir ve
`AbpExceptionLocalizationOptions.MapCodeNamespace` ile bir lokalizasyon kaynağına bağlanır;
parametreler `Data` sözlüğünden gelir; yanıt `RemoteServiceErrorInfo` ile şekillenir; detayların
istemciye gidip gitmeyeceğini `AbpExceptionHandlingOptions.SendExceptionsDetailsToClients` belirler (K2).

Dolayısıyla:
- Hipotez metinleri **lokalizasyon kaynağında** yaşar (`DatabaseCheckerResource`), kodda değil.
- Teşhis raporu ABP'nin hata yanıtının **uzantısı** olarak taşınır; paralel bir hata sistemi kurulmaz.
- Ham DB mesajı yalnız `SendExceptionsDetailsToClients` açıkken ve redaction'dan geçtikten sonra görünür.

**Gözlemlenebilirlik:** OTel'in "Recording errors" rehberi `error.type`'ın
*"SHOULD match the db.response.status_code returned by the database ... or the canonical name of exception"*
demesi bizim `FailureIdentity.Code`'umuzla birebir örtüşür (K2). Yani `error.type = "23503"` /
`"547"`, `db.response.status_code` aynı değer, span status `ERROR`. DBC-29 ile aynı `ActivitySource`.

---

## 5. Uçtan uca örnek

```text
Senaryo adimi 3:  POST /orders  ->  500
                  |
[1] YAKALA        SUT RFC 9457 dondurmedi; ama adim 4 (db.assert.row) da Fail.
                  Test Module'un kendi baglantisi uzerinden ayni insert denenmiyor —
                  bunun yerine ASSERTION FAIL sinyali teshise girer.
[2] KIMLIKLE      Assertion sinyali: sales.Orders / Id=7f3a.. bulunamadi. Kod yok, kaynak=Assertion.
[3] YERELLESTIR   sales.Orders katalogdan cozulur: PK Id, FK CustomerId -> sales.Customers.Id,
                  NOT NULL Status(varchar 20), CHECK CK_Orders_Status.
                  RelatedFindings: "sales.Orders.Status varchar(20)->varchar(50) (NonBreaking)"
[4] HIPOTEZ       H-AS-01 (hic olusmadi) · H-AS-02 (gec olustu) · H-AS-04 (baska tenant) ·
                  H-FK-01 (parent yok -> insert patlamis olabilir) · H-NN-01 · H-DT-01
[5] KANIT         RowExists(sales.Orders, Id=7f3a..)          -> false
                  RowExists(sales.Customers, Id=<istekteki>)  -> FALSE   *** 
                  RowCountByFilter(sales.Orders, tenant kapali) -> 0
                  SettingValue(search_path)                   -> beklenen
[6] SIRALA        H-FK-01  Confirmed   (parent yok)
                  H-AS-01  Confirmed   (satir yok — H-FK-01'in sonucu)
                  H-AS-02  RuledOut    (tenant kapali sorguda da yok)
                  H-AS-04  RuledOut
                  H-DT-01  Possible    (kanit yok)
[7] ANLAT         "Siparis olusmadi cunku FK_Orders_Customers'in isaret ettigi musteri satiri
                   (Id=...) veritabaninda yok. Senaryonun 1. adimi musteriyi olusturmali;
                   o adimin ciktisini kontrol edin."
```

Dikkat: motor `500` gövdesini **okumadan**, yalnız katalog + probe ile doğru cevaba ulaştı.
SUT'un hata mesajına bağımlı değiliz — bu, mesajı gizleyen (doğru davranan) API'lerde kritiktir.

---

## 6. DDD / ABP katman yerleşimi

| Katman | Ne konur |
|---|---|
| **Domain.Shared** | `FailureCodeClassCodes`, `HypothesisKindCodes`, `DiagnosisConfidenceCodes`, `ProbeKindCodes`, `FailureSourceKindCodes`, `DiagnosisExceptionCodes`, lokalizasyon anahtarları |
| **Domain / Models** | `FailureSignal`, `FailureIdentity`, `ObjectRef`, `ResolvedContext`, `ProbeRequest`, `Evidence`, `EvidenceSet`, `HypothesisAssessment`, `DiagnosisReport` — **hepsi değer nesnesi, entity değil** |
| **Domain / Interface** | `IFailureIdentityExtractor` (`IEngineComponent`), `IDiagnosisRule`, `IDiagnosisProbe` (`IEngineComponent`), `IDiagnosisContextResolver` |
| **Domain / Managers** | `DiagnosisManager` (7 adımlı orkestrasyon), `HypothesisRankingManager` (saf), `ProbeBudgetManager`, `DiagnosisContextResolver` (mevcut discovery provider'larını kullanır) |
| **EntityFrameworkCore** | `PostgreSqlFailureIdentityExtractor`, `SqlServerFailureIdentityExtractor` (+ `sys.messages` şablon önbelleği), probe implementasyonları. **Tüm SQL yalnız burada** |
| **Application.Contracts** | `IDiagnosisAppService`, `DiagnoseRequestDto` + FluentValidation, `DiagnosisReportDto`, izin `DatabaseChecker.Diagnosis` |
| **Application** | `DiagnosisAppService` (atomik use-case), Mapperly mapper'ları |
| **HttpApi** | `DiagnosisController` → `POST /api/database-checker/diagnosis` |

**DDD kuralları — mevcut ev kurallarıyla uyumlu:**
- Entity davranış orkestrasyonu taşımaz; **manager** invariant sahibidir (kural 3).
- Mapping **Mapperly**; manuel mapping yok (kural 4).
- Her public DTO'ya FluentValidation (kural 5).
- AppService atomik use-case; uzun dış I/O açık UOW içinde tutulmaz (kural 6) — teşhis probe'ları
  uygulama DB transaction'ı açıkken çalışmaz.
- Motor bileşen adlandırması conventional DI ile uyumlu olmalı (kural 11) — `PostgreSql…Extractor : IFailureIdentityExtractor`.
- Her sınıfta `// islevi:` / `// sistemdeki gorevi:` yorum çifti (kural 13).

**Kalıcılık kararı:** `DiagnosisReport` **varsayılan olarak saklanmaz** — hesaplanır ve döner.
Saklanması istenirse `ScenarioRun` tarafında, DBC-02 redaction'ı ve TTL ile. DB Checker'ın kendi
şemasına yeni bir "teşhis" tablosu **eklenmez** (RULE-0002 gereksiz genişleme).

---

## 7. Konfigürasyon boyutu — yeni yetenek

Kullanıcının özellikle saydığı *"sql'de olan configuration'lardan"* gelen hatalar, bugün hiçbir
katmanda görünmüyor. İki parçalı çözüm:

**(a) Konfigürasyonu karşılaştırılabilir nesne yap.**
- PostgreSQL: `pg_settings` (ayrıca `source`, `boot_val`, `reset_val` sütunlarıyla **nereden geldiği**),
  `pg_file_settings`, `pg_db_role_setting`. Dokümantasyon `pg_settings`'in `SHOW ALL`'dan daha esnek
  olduğunu ve filtre/join yapılabildiğini söylüyor (K2). Konfigürasyon sapması gerçek bir problem sınıfı:
  `ALTER SYSTEM`, kaçırılmış reload, OS ortam değişkeni, `ALTER DATABASE`/`ALTER ROLE` override'ları (K2/K3).
- SQL Server: `sys.configurations`, `sys.database_scoped_configurations`, `DATABASEPROPERTYEX`,
  `SESSIONPROPERTY()` (`ANSI_NULLS`, `QUOTED_IDENTIFIER`, `ARITHABORT`, `ANSI_PADDING`).

**Beyaz liste zorunlu:** tüm ayarlar değil, **davranışı değiştiren** kapalı bir küme karşılaştırılır
(collation, timezone, `search_path`, `standard_conforming_strings`, isolation, ANSI ayarları,
`statement_timeout`/`lock_timeout`, `max_identifier_length`, sunucu sürümü). Gerekçe: tüm ayarları
kıyaslamak gürültü üretir ve ayar değerleri güvenlik açısından hassastır.

**(b) Teşhis girdisi yap.** `H-CF-*` hipotezleri bu okumayı `SettingValue` probe'u ile kullanır.
"Aynı sorgu test'te çalışıyor canlıda patlıyor" vakalarının büyük kısmı buradan çıkar.

Bu, PLAN-0001'e **DBC-33** olarak girer.

---

## 8. Genişletme sözleşmesi — "bir şey eklemek ne demek?"

| Eklemek istediğin | Yazacağın | Dokunacağın mevcut dosya |
|---|---|---|
| Yeni hipotez | 1 sınıf (`IDiagnosisRule`) | **hiçbiri** |
| Yeni kanıt türü | 1 sınıf (`IDiagnosisProbe`) | **hiçbiri** |
| Yeni motor (MySQL…) | 1 extractor + N probe implementasyonu | **hiçbiri** |
| Yeni hata kodu anlamı | lookup seed satırı + lokalizasyon anahtarı | **hiçbiri** (kod değişmez) |
| Yeni sinyal kaynağı | 1 adapter (`FailureSignal` üretir) | **hiçbiri** |

Bu tablo, "dinamik yapı" talebinin ölçülebilir tanımıdır: **hiçbir genişletme, mevcut bir dosyanın
değiştirilmesini gerektirmez.** Karşılanmıyorsa tasarım bozulmuştur.

---

## 9. Güvenlik ve emniyet sınırları

| Risk | Önlem |
|---|---|
| Teşhis ikinci bir kesinti olur | Probe bütçesi (`MaxProbeCount`, `MaxProbeDurationMs`), `statement_timeout`, `READ ONLY` transaction (DBC-03) |
| Müşteri verisi rapora sızar | DBC-02 redaction; varsayılan `None`; `Full` ayrı izin + TTL |
| **Prompt injection** — hata mesajı veya satır değeri modele talimat taşır | Rapor modele **ham metin değil yapılandırılmış** gider; değerler redaction'lı; MCP tarafında `resource_link` |
| Ham DB mesajı istemciye sızar | ABP `SendExceptionsDetailsToClients` kapalı varsayılan; `RawMessage` yalnız yetkili yolda |
| Yanlış teşhis güven kaybettirir | Çıkarılan hiçbir ad katalogda doğrulanmadan kullanılmaz; `RuledOut` da raporlanır; tek-kök-neden dayatılmaz |
| SQL enjeksiyonu | Probe SQL'i yalnız doğrulanmış katalog adlarından + parametrelerden üretilir |
| Teşhis yazma yapar | `IDiagnosisProbe` sözleşmesi salt-okunur; yazma yeteneği **hiç yok** |

---

## 10. Bilinçli olarak yapmayacaklarımız

| Öneri | Neden hayır |
|---|---|
| Karar yolunda LLM kullanmak | Teşhis deterministik olmalı; LLM oracle'lar kırılgan (RESEARCH-0003 §5.3). Model yalnız **anlatımı** zenginleştirebilir, güveni ve kanıtı **hesaplayamaz** |
| Hata numarası → metin eşleme tablosu | Talebin kendisi bunu dışlıyor; ayrıca lokalizasyon ve sürüm değişimlerinde kırılır |
| SUT'un log dosyalarını okumak | Checker'ın erişim sınırı dışında; korelasyon `ApplicationName` (DBC-03) + assertion ile yapılır |
| Teşhis sırasında düzeltme uygulamak | Checker eylem motoru değil (ARCH-0001); rapor "ne kontrol edilmeli" der, yapmaz |
| Her ayarı karşılaştırmak | Gürültü + hassas veri; kapalı beyaz liste kullanılır |
| Teşhis için ayrı bir hata sistemi | ABP'nin `BusinessException` + `RemoteServiceErrorInfo` + lokalizasyon altyapısı kullanılır |

---

## 11. PLAN-0001'e eklenen maddeler

| # | Ne | Bağımlılık | Boyut |
|---|---|---|---|
| **DBC-33** | **Konfigürasyon karşılaştırması.** `pg_settings`/`pg_file_settings`/`pg_db_role_setting` ve `sys.configurations`/`sys.database_scoped_configurations`/`SESSIONPROPERTY`; kapalı beyaz liste; yeni nesne türü `Setting` | DBC-11 | S |
| **DBC-34** | **Sinyal yakalama katmanı.** EF interceptor (`SaveChangesFailed` + `CommandFailed`, batch sınırı bilinerek), ABP `IExceptionSubscriber`, RFC 9457 ayrıştırıcı, assertion sinyali → `FailureSignal` | DBC-05 | S |
| **DBC-35** | **Kimlik çıkarıcılar.** PG yapılandırılmış alanlar (+ class-23 kapsam uyarısı); MSSQL `sys.messages` şablon→regex + katalog doğrulaması | DBC-34 | **M** |
| **DBC-36** | **Teşhis çekirdeği.** `DiagnosisManager` 7 adım, `IDiagnosisRule` / `IDiagnosisProbe` kayıtları, bütçe yöneticisi, güven merdiveni | DBC-35, DBC-11 | **L** |
| **DBC-37** | **Hipotez kataloğu v1.** A–H aileleri (FK, unique, not-null/check, veri tipi, konfigürasyon, eşzamanlılık, yetki, assertion) | DBC-36 | **L** |
| **DBC-38** | **Rapor yüzeyi.** RFC 9457 + ABP `RemoteServiceErrorInfo` entegrasyonu, lokalizasyon kaynağı, OTel `error.type`/`db.response.status_code` | DBC-36, DBC-29 | S |

**Sıra:** DBC-33 → 34 → 35 → 36 → 37 → 38. DBC-36 öncesinde **DBC-11** (nesne türü kaydı) ve
**DBC-12** (kısıt güvenilirliği) bitmiş olmalı: teşhisin en değerli hipotezleri (`H-FK-06`, `H-NN-03`)
o veriye dayanıyor. **DBC-09** (bulgu fingerprint'i) olmadan `RelatedFindings` bağlanamaz.

---

## 12. Kaynaklar

| Kaynak | Neyi kanıtlıyor | Sınıf |
|---|---|---|
| https://www.postgresql.org/docs/current/errcodes-appendix.html | SQLSTATE sınıf/koşul listesi; **"complete coverage ... only for errors in SQLSTATE class 23"**; ilk iki karakter = sınıf | K2 |
| https://www.npgsql.org/doc/api/Npgsql.PostgresException.html | `SqlState`, `ConstraintName`, `TableName`, `ColumnName`, `SchemaName`, `Detail`, `Hint` alanları | K2 |
| https://learn.microsoft.com/en-us/sql/t-sql/functions/formatmessage-transact-sql | `sys.messages` şablonları, yer tutucular, dil bazlı mesaj | K2 |
| https://abp.io/docs/latest/framework/fundamentals/exception-handling | `BusinessException`, `IHasErrorCode/Details/LogLevel`, `MapCodeNamespace`, `IExceptionSubscriber`, `IExceptionToErrorInfoConverter`, `SendExceptionsDetailsToClients`, `RemoteServiceErrorInfo` | K2 |
| https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors | `ISaveChangesInterceptor.SaveChangesFailed`, `IDbCommandInterceptor.CommandFailed` | K2 |
| https://github.com/dotnet/efcore/issues/35229 | `CommandFailed`'in batch'te tüm istisnaları yakalamaması | K2 |
| https://www.rfc-editor.org/rfc/rfc9457.html | Problem Details: `type/title/status/detail/instance` + extension members; RFC 7807'yi geçersiz kılar | K2 |
| https://opentelemetry.io/docs/specs/semconv/general/recording-errors/ | `error.type`'ın `db.response.status_code` ile eşleşmesi; span status ERROR | K2 |
| https://github.com/Giorgi/EntityFramework.Exceptions | Tipli DB istisnaları (sınıflandırma) ve sınırı (#35 "which constraint failed") | K2 |
| https://www.postgresql.org/docs/current/view-pg-settings.html | `pg_settings`, `SHOW ALL`'dan esnek olması | K2 |
| https://www.postgresql.org/docs/current/config-setting.html | Ayar önceliği: `postgresql.conf`, `auto.conf`, `ALTER SYSTEM`, `ALTER DATABASE/ROLE`, oturum `SET` | K2 |
| https://learn.microsoft.com/en-us/sql/relational-databases/sql-server-deadlocks-guide | 1205, `xml_deadlock_report`, `system_health` varsayılan açık | K2 |
| https://arxiv.org/html/2607.22385v1 (AgentRCA) | RCA'nın sabit etiket eşlemesi değil, hipotez-eleme çıkarımı olarak formüle edilmesi | K2 |
| https://arxiv.org/html/2606.19407v1 (JustDiag!) | Kanıt/bulgu/iddia/hipotezin incelenebilir nesneler olması | K2 |
| https://theory.stanford.edu/~aiken/publications/papers/pldi12b.pdf | Abductive inference ile otomatik hata teşhisi | K2 |
