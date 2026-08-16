---
id: RESEARCH-0016
type: research
status: draft
title: Generic ve dinamik kopru — yetenek sablonu, kanit yolu ve semantik baglama
created: 2026-08-14
updated: 2026-08-14
decision_refs:
  - ADR-0007
  - ADR-0008
  - ADR-0014
  - ADR-0015
  - ADR-0016
  - ADR-0017
  - ADR-0018
rule_refs:
  - RULE-0005
  - RULE-0006
  - RULE-0007
  - RULE-0008
---

# RESEARCH-0016 — Generic ve dinamik köprü

> **DURUM: araştırma sürüyor.** Her tur bittiğinde bu belgeye bölüm eklenir; karar ADR-0019'a
> yazılır. Öncülü: [[90-Inbox/RESEARCH-0015-Ajan-Gerceklikleri-Ve-Checker-Koprusu|RESEARCH-0015]]
> ve [[03-Decisions/ADR-0018-Checker-Koprusu-Tek-Sozluk-Tool-Butcesi-Ve-Kanit-Zinciri|ADR-0018]].

## Sorunun tanımı

RESEARCH-0015 köprünün **ne yapması gerektiğini** kanıtladı: ajan tahmin etmesin, checker'a
sorsun; kanıt zinciri yürüsün; tek sözlük olsun. ADR-0018 bunu karara bağladı.

**Bu belge farklı bir soruyu cevaplıyor:** köprü **tek tek vakalar için yazılmadan**, yani
`if (403) { user_roles'a bak }` gibi elle kodlanmış akışlar olmadan, **generic ve dinamik**
nasıl kurulur?

Somut test cümlesi (kullanıcının koyduğu):

> *"401 geldi, 403 geldi → a, yetki hatası → dbchecker'la bakalım → user_roles'tan rolü aldı →
> permission_grant ↔ role ↔ user eşleşmesine baktı → hangi rollere sahibiz → Swagger'dan
> gereken scope'a baktı → bizde o rol gerçekten yok → **doğrulandı**."*
>
> *"Tek örnekten diğer istekleri de yorumlayabilsin."*

Yani: **bir vakayı çözen kod değil, vaka sınıfını çözen motor** isteniyor. Ve aynı motorun
yazarlık yönünde de çalışması: *"bu operasyon DB'de neyi değiştiriyor?"*

### Bu belgenin cevaplayacağı beş soru

| # | Soru | Tur |
|---|---|---|
| 1 | "Neden 403" sorusunu **generic** cevaplayan ürünleşmiş motorlar ne yapıyor? | 1 |
| 2 | Soyut kavram (*"kullanıcının rolü"*) somut şemaya **kim** bağlıyor — ajan mı, manifest mi? | 2 |
| 3 | *"Bu operasyon DB'de neyi değiştiriyor"* generic olarak nasıl bulunur? | 3 |
| 4 | Yetenek sayısı büyürken tool bütçesi nasıl korunur (dinamik yüzey)? | 4 |
| 5 | Kanıt zinciri **kod** mu olmalı, **veri** mi (deklaratif yol + paket dağıtımı)? | 5 |

---

## Tur 1 — "Neden 403?" sorusu zaten ürünleşmiş: açıklama ağacı deseni

Kullanıcının tarif ettiği 403 zinciri **bizim icat edeceğimiz bir şey değil.** Üç bağımsız
üretim sistemi tam olarak bu işi yapıyor ve **üçü de aynı şekle yakınsıyor.**

### 1.1 Üç referans uygulama

| Sistem | Yüzey | Ne döndürür |
|---|---|---|
| **GCP Policy Troubleshooter** | `troubleshootIamPolicy` | `overallAccessState` + politika→binding→koşul **ağacı**, her düğümde kendi durum enum'u |
| **AWS IAM Policy Simulator** | `SimulatePrincipalPolicy` | `EvalDecision` ∈ `allowed \| explicitDeny \| implicitDeny` + **`MatchedStatements[]`** |
| **Zanzibar ailesi** (OpenFGA / SpiceDB) | `Expand`, `Check(withTracing)` | **Userset ağacı** — yaprakta kullanıcılar, ara düğümlerde `union` / `intersection` / `difference`; SpiceDB'de `debug_trace` gezilen tüm ilişkileri taşır |

### 1.2 Ortak şekil — beş özellik

**(a) Soru önce normalize edilir.** GCP `accessTuple` alır: *principal · resource · permission ·
condition context*. Serbest metin bir hata mesajı değil, **tipli bir dörtlü**. Simülasyon
`EvalActionName` + `EvalResourceName` ile aynı şeyi yapar.

> Bizde karşılığı: köprü *"403 aldım"* cümlesini değil, `{subject, operation, requiredPermission,
> context}` dörtlüsünü işler. Bu dörtlüyü **API Checker'ın `FailureIdentityDto`'su zaten üretiyor**
> — `ChallengeScheme`, `ChallengeError`, `ChallengeScopes` alanları tam olarak `WWW-Authenticate`
> başlığından çıkarılmış gereken izindir.

**(b) Açıklama anlatı değil, ağaçtır.** GCP'nin şekli:

```
overallAccessState
└── allowPolicyExplanation.allowAccessState
    └── explainedPolicies[]
        └── bindingExplanations[]
            ├── role                       (hangi rol)
            ├── rolePermission             ROLE_PERMISSION_INCLUDED | NOT_INCLUDED
            ├── combinedMembership         MEMBERSHIP_MATCHED | NOT_MATCHED
            ├── memberships{}              (üye üye)
            ├── conditionExplanation       (koşul true/false)
            └── relevance                  HEURISTIC_RELEVANCE_HIGH | NORMAL
```

**Her düğüm kendi durumunu taşır.** "Rol var mıydı" ile "rol o izni içeriyor muydu" **ayrı
alanlardır**. Bu, bizim `HypothesisAssessmentDto` + `ProbeEvidenceDto` ikilisiyle birebir aynı
mantık — ama onlarda **bağlantı yapısı** (hangi kanıt hangi düğümün altında) da var.

**(c) Sonuç ikili değil, üç durumludur.** AWS: `allowed` / `explicitDeny` / `implicitDeny`.
Fark kritik: *"açıkça reddedildi"* ile *"hiçbir kural izin vermedi"* **farklı teşhislerdir** ve
farklı düzeltme gerektirir.

**(d) — ve en önemlisi — "bilmiyorum" birinci sınıf bir sonuçtur.** GCP'nin durum sözlüğünde
bilgi eksikliğine ayrılmış durumlar var: troubleshooter **politikayı okuma yetkisine sahip
değilse** sonucu "erişim yok" değil, **bilinmiyor** olarak işaretler.

> **Bizim için doğrudan sonuç:** Salt-okunur DB bağlantımız `user_roles` tablosunu göremiyorsa,
> doğru cevap *"kullanıcının rolü yok"* **değildir** — *"kanıt toplanamadı"*dır. Bu ayrım
> yapılmazsa köprü **yanlış teşhis üretir** ve bu, halüsinasyondan daha tehlikelidir çünkü
> deterministik motordan gelmiş gibi görünür.
>
> Kayıt karşılığı zaten modelde: `test_outcome_statuses.Inconclusive` (ADR-0016 §F). Kanıt
> düğümü seviyesinde de aynı üçlü gerekir: `Observed` / `NotObserved` / **`Unavailable`**.

**(e) Alaka (relevance) motorun kendi hesabıdır, modelin yorumu değil.** GCP her binding'e
`HEURISTIC_RELEVANCE_HIGH|NORMAL` yazıyor: *"bu binding sonucu ne kadar etkiledi."* AWS
`MatchedStatements` ile *"kararı hangi ifade belirledi"* diyor.

> Bizde karşılığı `HypothesisAssessmentDto.Priority` + `ConfidenceCode`. **Google'ın üretim
> yüzeyi bizim tasarımımızı doğruluyor:** sıralamayı motor yapar, ajan değil (RULE-0005).

### 1.3 Açıklama, çözümün **yan ürünüdür**

En öğretici nokta: SpiceDB `withTracing` bayrağı **ayrı bir açıklama motoru çalıştırmıyor** —
zaten yapılan çözümleme (resolution) sırasında gezilen ilişkileri kaydediyor. OpenFGA `Expand`
de değerlendirme ağacının kendisidir.

> **Tasarım kuralı:** köprü *"neden"* sorusuna cevap üretmek için **ikinci bir akıl yürütme**
> kurmamalı. Zincir zaten yürütülüyor; **yürüyüşün kendisi kaydedilir ve rapor olur.**
> Bu ADR-0018 §D'nin (alıntısız hipotez rapora giremez) mekanik garantisidir: kanıt
> uydurulamaz çünkü kanıt = atılan adımın kaydı.

### 1.4 Bizim eksiğimiz: **okuma yüzeyi yok**

Kod seviyesinde doğrulandı. Database Checker bugün üç şey verebiliyor:

| Yüzey | Ne yapar | 403 zinciri için yeterli mi |
|---|---|---|
| `AssertRowAsync` / `AssertCountAsync` / `AssertAbsentAsync` | **Beklentiyi doğrular** (`Expectations`, `Cardinality`) | ❌ — *"kullanıcının rolleri neler"* bir beklenti değil, bir **sorudur** |
| `DescribeTableAsync` | Tablo yapısı + **bir seviye FK komşusu** | ✅ yapı için, ❌ veri için |
| `GetSnapshotAsync` | Şema fotoğrafı | ✅ yapı için |

`RowAssertionResultDto.RowSummary` yalnız `IncludeRowOnFailure` ile ve **başarısızlıkta** dolar.
Yani bugün *"user_roles'ta bu kullanıcının satırlarını göster"* demenin tek yolu **kasten
başarısız olacak bir assertion yazmaktır.** Bu bir tasarım kokusu, ürün yüzeyi değil.

> **Bulgu 1:** Kanıt zincirinin çalışması için Database Checker'a **salt-okunur, bütçeli,
> anahtarla sınırlı bir projeksiyon (probe) yüzeyi** gerekiyor:
> *"şu tabloda şu anahtara uyan satırların şu kolonlarını, en fazla N satır, redaksiyonlu ver."*
> Bu, ADR-0007'nin salt-okunur değişmezini **bozmaz** — assertion sözleşmesindeki gibi serbest
> SQL taşımaz, adres + anahtar + kolon listesi taşır.
>
> Bu, PLAN-0001'e (`DBC-xx`) düşen bir checker işidir ve köprünün **ön koşuludur.**

### 1.5 Tur 1 çıktısı — köprünün teşhis tarafı için şablon

```
PtnAccessTuple { subject, operation, requiredPermission, context }      ← soru normalize
        │
PtnExplanationNode (ağaç)
   ├── nodeKindCode      SubjectResolved | RoleHeld | GrantMatched | ScopeRequired | ...
   ├── stateCode         Observed | NotObserved | Unavailable              ← üç durum
   ├── relevanceCode     High | Normal                                     ← motor hesaplar
   ├── evidence[]        PtnFindingRef { sourceChecker, probeKind, fact }   ← alıntı zorunlu
   └── children[]
        │
PtnVerdict  Confirmed | Likely | Possible | RuledOut | Inconclusive
```

**Bu ağaç alan-bağımsızdır.** İçindeki `nodeKindCode` değerleri bilet/sipariş/abonelik bilmez;
onları **somut tablolara bağlayan şey Tur 2'nin konusu olan manifesttir.**

---

## Tur 2 — Kavramı şemaya kim bağlar: manifest, ajan değil

Tur 1'in ağacındaki `RoleHeld` düğümü *"kullanıcının rolü var mı"* diye sorar. Ama **hangi
tabloda?** `user_roles` mü, `AbpUserRoles` mi, `membership` mi, `party_role_assignment` mi?

Bu soruyu ajana sordurmak, RULE-0007'nin yasakladığı şeyin ta kendisidir. Cevabı **veri**
olmalı, tahmin değil. Bu, çözülmüş bir problem sınıfıdır ve iki olgun kaynağı var.

### 2.1 Ölçüm: semantik katman modelden daha belirleyici

**Birincil ölçüm** (arXiv 2604.25149, 2026): 100 doğal dil sorusu, ClickHouse üzerinde Contoso
Retail veri kümesi, üç frontier model (Claude Opus 4.7, Claude Sonnet 4.6, GPT-5.4), eşli
(paired) protokol.

| Koşul | Doğruluk |
|---|---|
| Semantik belge **yok** (ham şema) | **%45,5 – %50,5** |
| Semantik belge **var** (elle yazılmış **4 KB** markdown) | **%67,7 – %68,7** |

**Kazanç +17 ile +23 puan; her karşılaştırma p < 0,01.** Ve kritik cümle:
**model seçimi, semantik katmanın varlığından çok daha az fark yaratıyor.**

> Bu, RESEARCH-0015 §3.4'teki *"deterministik katman kalınlaştıkça model küçülebilir"*
> tezinin **doğrudan ölçülmüş hâlidir.** 4 KB'lık bir belge, model ailesini değiştirmekten
> daha etkili.

**İkincil ölçümler** (satıcı kaynaklı, teyit amaçlı): dbt 2026 karşılaştırmasında aynı modeller
semantik katmanla %84→%100 (GPT-5.3 Codex) ve %90→%98,2 (Claude Sonnet 4.6); AtScale TPC-DS'te
ham modellerde <%20 iken semantik katmanla %92,5.

### 2.2 Asıl bulgu doğrulukta değil, **başarısızlığın şeklinde**

dbt'nin 2026 ölçümündeki kapsam tablosu bizim için doğruluk yüzdesinden daha önemli:

| Soru | Text-to-SQL | Semantik katman |
|---|---|---|
| Modelin **kapsamındaki** sorular | %62,5 | **%100** |
| Kapsam **dışı** sorular | %70,0 | **%0** |

Ve gerekçesi:

> *"Üretimde en çok önemsenen ayrım budur. Text-to-SQL'de başarısızlık **makul ama yanlış bir
> cevap** gibi görünür. Semantik katmanda başarısızlık **bir hata mesajı** gibi görünür."*

Semantik katman kapsam dışı soruda **%0 doğruluk** verir — çünkü **cevap vermeyi reddeder.**
Text-to-SQL o sorularda %70 doğruluk verir, yani **%30 sessiz yanlış.**

> **Bizim için sonuç — bu ürünün tüm tezi:** Köprünün hedefi *"her soruya cevap veren bir
> katman"* değil, **"cevaplayamadığını söyleyen bir katman"**dır. Kapsam dışı soruda %0
> vermek, %70 verip %30 uydurmaktan **iyidir** (RULE-0007, RULE-0005).
>
> Ölçüm ayrıca kapsamın maliyetini de söylüyor: ilk turda soruların **%27'si kapsam dışıydı**;
> **üç küçük model** eklenince kapsam %100'e çıktı. Yani **kapsam tavandır ve kapatılabilir bir
> tavandır.**

### 2.3 Bağlamanın standart formu vardır: R2RML

*"Şu ontoloji kavramı = şu tablonun şu kolonu"* eşlemesi bir **W3C standardıdır**: **R2RML**
(RDB to RDF Mapping Language). Eşlemeler **verinin kendisidir** (Turtle/RDF grafiği), kod
değil; Ontop gibi motorlar bu eşlemeyle ilişkisel veritabanını **sanal bilgi grafiği** olarak
sorgular.

Ve eşlemeler **elle sıfırdan yazılmak zorunda değil**: VKG literatüründe *mapping pattern*
kataloğu var — şema yapısından (PK, FK, junction tablo, kimlik ilişkisi) **desen türetilir**:

| Desen | Şemadaki karşılığı | Bizim kavramımız |
|---|---|---|
| **SE** — tekil varlık | PK'lı tablo | `Subject`, `Resource` |
| **SR** — ilişki | FK | `Ownership` |
| **SRa** — nitelikli ilişki | Ek kolonlu junction tablo | `RoleAssignment` (+ `valid_from`) |
| **SRR** — reifikasyon | İlişkinin ayrı tabloya açılması | `PermissionGrant` |
| **SH** — hiyerarşi | Kendine FK | `RoleHierarchy` |

> **Sonuç:** Bağlama manifesti **mekanik olarak aday üretebilir** (FK grafiği + kardinalite +
> ad benzerliği → desen eşleşmesi), sonra **insan onaylar.** Bu tam olarak ADR-0018 §F'deki
> *"aday üretilir, insan onaylar"* deseni ve RULE-0007'nin 1. maddesiyle (açık uçlu alan yok)
> uyumlu.

### 2.4 Text-to-SQL literatürünün uyarısı: bağlama **tek kritik adımdır**

Text-to-SQL araştırması on yıldır aynı şeyi söylüyor: **schema linking** (soruyu doğru
tablo/kolona bağlamak) hattın **darboğazıdır**. DIN-SQL bunu dört modülün **ilki ve en
kritiği** yapar; RESDSQL şema bağlamayı SQL iskeletinden **ayırır**; C3'ün tek işi modelin
**fazladan veya yanlış kolon seçme eğilimini** düzeltmektir ve bunu %11 azaltır.

> **Bizim için:** biz text-to-SQL yapmıyoruz — **daha kolay** bir problemimiz var. Bizim
> soru uzayımız kapalı: *"bu kavram hangi tabloda"* sorusunun cevabı **sonlu bir kavram
> kümesi** üzerinden verilir. Yine de aynı ders geçerli: **bağlama ayrı, açık ve ölçülebilir
> bir artefakt olmalı** — çıkarımın içine gömülmemeli.

### 2.5 Tur 2 çıktısı — `PtnProfilePack` (bağlama manifesti)

Köprünün generic olmasını sağlayan şey budur: **kod değişmez, manifest değişir.**

```yaml
# ptn-profile.yaml — kiracı/SUT başına bir tane, Git'te durur, MCP Resource olarak sunulur
profileKey: acme-ticketing
boundTo:
  dbSchemaFingerprint: sha256:...      # şema kayarsa manifest geçersizleşir
  specSnapshotId: ...
concepts:
  Subject:
    table: identity.users
    identityColumn: id
    naturalKeyColumns: [email]
    confidence: Approved               # Proposed | Approved | Rejected
    approvedBy: mertbyd
  RoleAssignment:                      # SRa deseni: junction + nitelik
    table: identity.user_roles
    subjectColumn: user_id
    roleColumn: role_id
    pattern: SRa
  PermissionGrant:                     # SRR deseni
    table: identity.role_permission_grants
    roleColumn: role_id
    permissionColumn: permission_name
  # Bağlanmamış kavram = NOT_BOUND → köprü sorar, tahmin etmez
coverage:
  required: [Subject, RoleAssignment, PermissionGrant]
  bound: 3/3
```

**Dört değişmez:**

1. **Manifest veridir, kod değil.** Yeni müşteri = yeni manifest, yeni derleme değil.
2. **Bağlanmamış kavram bir hata değil, bir sorudur.** `NOT_BOUND` → kapalı uçlu soru
   (ADR-0017 §D). Asla varsayım.
3. **Kapsam ölçülür ve raporlanır.** *"3/5 kavram bağlı"* teşhis raporunun başında durur;
   kapsam dışı bir zincir **`Inconclusive`** döner, `Failed` değil.
4. **Manifest şema parmak iziyle mühürlüdür.** Şema değişirse ilgili bağlamalar
   `Proposed`'a düşer ve yeniden onay ister — sessiz kayma imkânsız (ADR-0018 §E'nin
   veri tarafındaki eşi).

---

## Tur 3 — "Bu operasyon DB'de neyi değiştiriyor?" — üç yol, tek çözümleyici

ADR-0018 §F bu soruyu iki seçenekle kapatmıştı: **OTel telemetrisi** (SUT enstrümantasyonu
şart) veya **önce/sonra farkı** (motor bizde). Tarama **üçüncü ve daha güçlü bir yol** buldu:
**log tabanlı değişiklik yakalama.**

### 3.1 Üç yolun karşılaştırması

| # | Yol | SUT'tan istenen | Veritabanından istenen | Ne verir |
|---|---|---|---|---|
| **A** | **Önce/sonra farkı** | **hiçbir şey** | salt-okuma | Değişen tablo/satır **adayları**; denetim kolonları gürültü |
| **B** | **Log tabanlı yakalama** | **hiçbir şey** | yapılandırma + ayrıcalık | **Kesin yazma kümesi**: tablo, işlem türü, sıra, (motora göre) önce/sonra değerler |
| **C** | **SUT enstrümantasyonu** | **kod / agent** | — | Çağrı bağlamıyla ilişkilendirilmiş SQL |

**C yolunun referansı EvoMaster'dır** ve tam olarak neden istemediğimizi gösterir: beyaz kutu
kipi, çalışma anında **yürütülen SQL komutlarını toplayan bir enstrümantasyona** dayanır ve
bu enstrümantasyon SUT'un içine girer. Karşılığında çok şey verir (SQL'i fitness fonksiyonuna
katar, veritabanına doğrudan `INSERT` ile ön koşul kurar) ama **her müşteride kurulamaz.**

### 3.2 B yolu motor başına farklı ve **bedeli farklı**

| Motor | Mekanizma | Ne şart | Ne verir |
|---|---|---|---|
| **PostgreSQL** | Logical decoding (`pgoutput` / `wal2json` / `test_decoding`) | `wal_level = logical` (**sunucu yeniden başlatma**), `max_replication_slots`/`max_wal_senders` ≥ 1, replication yetkili rol, **slot** | İşlem sınırlı, sıralı, tablo ve kolon seviyesinde değişiklik akışı |
| **SQL Server — Change Tracking** | Senkron, sistem tablosuna yazar | DB **ve tablo** seviyesinde etkinleştirme; şema değişikliği veya trigger **yok** | **Yalnız birincil anahtar + işlem türü (I/U/D)** — *"ne değişti"* değil, ***"hangi satır değişti"*** |
| **SQL Server — CDC** | Asenkron, Agent job'ları | DB + tablo etkinleştirme, SQL Agent | Önce **ve** sonra değerleri, geçmişle birlikte; **I/O ve CPU maliyeti belirgin** |

**Change Tracking bizim için sürpriz derecede iyi bir eşleşme:** *"hangi satır değişti"*
sorusunu **çok ucuza** cevaplıyor, değerleri hiç saklamıyor. Değerleri zaten **Tur 1'de
gerekli olduğunu tespit ettiğimiz salt-okunur projeksiyon yüzeyinden** okuyabiliriz.
**Adres CT'den, değer probe'dan.**

.NET tarafı hazır: **Npgsql birinci parti replication API'si sunuyor** —
`LogicalReplicationConnection` + `PgOutputReplicationSlot` + `StartReplication`; mesaj akışı
`IAsyncEnumerable` olarak geliyor ve `SetReplicationStatus` ile sunucuya hangi WAL'in
serbest bırakılabileceği bildiriliyor.

> **Operasyonel tuzak — kayda geçer:** replication slot **tüketilmezse** sunucu WAL'i geri
> dönüştüremez. Bir koşum yarıda kalır ve slot açık kalırsa **müşterinin diski dolar.** Bu,
> "yalnız okuyoruz, zararsız" sanılan bir yeteneğin **üretimi durdurabileceği** yerdir.
> Slot yaşam döngüsü (oluştur → tüket → **her koşumda garantili düşür**) ADR seviyesinde
> sabitlenmeli; geçici (temporary) slot tercih edilmeli.

### 3.3 Karar: yol seçimi **yetenek yoklamasıyla** yapılır, varsayımla değil

Bu, kullanıcının istediği *"generic ve dinamik"* davranışın tam merkezidir. Köprü hangi yolun
mümkün olduğunu **sormaz, ölçer**:

```
WriteSetStrategyResolver
 ├─ CT var mı?            sys.change_tracking_databases / _tables      → CT stratejisi
 ├─ wal_level = logical?  SHOW wal_level + slot yetkisi                → Logical decoding
 ├─ ikisi de yok?         önce/sonra farkı                             → Diff stratejisi
 └─ sandbox paylaşımlı?   → hiçbiri: ayak izi Inconclusive
```

Ve sonuç bir **yetenek seviyesi** olarak raporlanır:

| Seviye | Strateji | Ayak izinin gücü |
|---|---|---|
| `Exact` | Logical decoding / CDC | Tablo + kolon + işlem türü + sıra |
| `RowAddressed` | Change Tracking | Tablo + değişen satırın anahtarı |
| `Inferred` | Önce/sonra farkı | Tablo + satır sayısı deltası (**aday**) |
| `Unavailable` | Yetenek yok veya sandbox paylaşımlı | **Yok** → soru insana |

**Dört seviye de aynı sözleşmeyi döndürür**, yalnız `strengthCode` farklıdır. Ajan
`strengthCode` görür ve `Inferred` ise ADR-0018 §F'nin kuralı devreye girer: **öneri olarak
sunulur, onaysız assertion üretimine giremez.**

### 3.4 Neden bu, ADR-0018'i **değiştirmiyor** ama genişletiyor

ADR-0018 §F telemetriyi *"SUT'un enstrümante olmasını şart koşar"* diye reddetti; bu doğru
kalıyor (C yolu). Ama **B yolu SUT'tan hiçbir şey istemiyor** — yalnız veritabanı
yapılandırması. Yani ADR-0018'in gerekçesi B yolunu dışlamıyor; B yolu o gerekçenin
**daha iyi karşılanmış hâli**.

> Ayrım şudur: **C müşterinin yazılımını değiştirmeyi ister; B müşterinin veritabanı
> ayarını değiştirmeyi ister.** İkincisi bir DBA işidir, bir geliştirme işi değil ve
> satış öncesi uygunluk sorusuna çevrilebilir (ADR-0017 §I'daki test saati sorusuyla
> aynı kategori).

### 3.5 Concurrency: ayak izi **tekil sandbox ister**

Her üç yol da aynı yerde kırılır: koşum sırasında başka trafik varsa değişiklikler
karışır. Log tabanlı yolda işlem kimliği/LSN ile daraltma yapılabilir, fark yolunda
**hiç yapılamaz.**

Bu zaten kararlı: TM-11 (aynı ortamda çakışan koşumların sıraya alınması). Yeni olan şu:
**ayak izi keşfi, sıraya alma garantisi yoksa çalıştırılamaz** ve sonucu `Unavailable`'dır.

---

## Tur 4 — Dinamik yüzey: yetenek büyürken tool bütçesi nasıl korunur

RULE-0007 aktif tool sayısını ≤ 7'de sabitledi. Ama köprü büyüyor: kanıt yolları, profil
bağlama, ayak izi stratejileri… Soru şu: **yetenek artarken yüzey nasıl sabit kalır?**

2025-2026'da bu problem üç ayrı çözümle **ürünleşti** ve üçü de aynı ilkeye dayanıyor:
**şemayı ihtiyaç anına kadar bağlama girdirme (progressive disclosure).**

| Çözüm | Mekanizma | Raporlanan kazanç |
|---|---|---|
| Tool Search Tool (Anthropic, 2025-11) | Tool şemaları talep üzerine yüklenir | **~%85** token azalması, tüm kütüphaneye erişim korunur |
| Code execution with MCP (Anthropic, 2025-11) | Sunucular **dosya sistemi gibi keşfedilebilir tipli API** olarak sunulur; model kod yazar | 150.000 → 2.000 token (**%98,7**) |
| Code Mode (Cloudflare, 2026-02) | API yüzeyi tipli SDK'ya sarılır | Girdi token'ında **%99,9** azalma |

Üretim doğrulaması: GitHub MCP sunucusunda **112 tool'a** ölçeklenirken %98 azalma korunmuş.

> **Bizim için sonuç — ADR-0018 §B'yi bozmuyor, güçlendiriyor:**
> ≤7 **aktif** tool kuralı doğru. Ama "geri kalanı toolset" derken kastedilen mekanizma artık
> daha net: **tool şeması bağlama girmez, talep üzerine yüklenir.** `ptn_ground`'un üç çağrıyı
> tek çağrıda birleştirmesi (ADR-0018 §B) aynı ilkenin küçük ölçekli hâli.
>
> **Sınır:** "code mode" (modele kod yazdırıp yürütmek) bizim için **kademe 3-4 riski**dir ve
> RULE-0005'in izin modeline girer. v1'de **benimsenmez**; benimsenirse yürütme sandbox'ı ve
> izin kademesi ayrı ADR ister. Alınan ders yalnız **progressive disclosure**'dır.

## Tur 5 — Kanıt yolu kod mu, veri mi: analyzer + paket modeli

Son soru: 403 zinciri, ayak izi akışı, ön koşul zinciri… bunlar **C# metotları** mı olacak,
yoksa **veri** mi?

### 5.1 k8sgpt: deterministik analyzer + yalnız anlatım için model

k8sgpt tam bizim ayrımımızı üretimde uyguluyor: **analyzer**'lar Kubernetes nesnelerinde bilinen
arıza desenlerini arayan **kodlanmış deterministik kontrollerdir**; model yalnız bulguyu
**insan diline çevirir**. Yerleşik analyzer kümesi var ve **kendi analyzer'ını yazabilirsin.**

İki ayrıntı doğrudan bize uyuyor:

- **Model çıktısı önbelleklenir ve önbellek anahtarı arıza tanımının hash'idir** — yani anlatım
  deterministik girdiye bağlıdır. Bizde karşılığı: aynı `{sourceChecker, fingerprint}` ikilisi
  için anlatım yeniden üretilmez.
- **Analyzer kümesi genişletilebilir** ama **çekirdek karar** her zaman analyzer'da kalır.

> Bu, RULE-0005'in *"ajan hakem değildir"* kuralının çalışan bir üretim örneğidir.

### 5.2 Paket (pack) dağıtım modeli olgun ve standarttır

OPA **bundle**'ı deseni: Rego politikaları + **yapılandırılmış veri** + manifest tek
**sürümlü artefakt** hâlinde paketlenir, `revision` alanıyla sürümlenir, HTTP/S3'ten çekilir;
OCI imajı olarak etiketlenip paylaşılabilir. Semgrep aynı şeyi kural kayıt defteriyle yapıyor.

> **Bizim için:** kanıt yolları ve profil bağlamaları **sürümlü paket** olmalı:
> `ptn-profile-pack` = `{profil manifesti + kanıt yolu tanımları + revision + fingerprint}`.
> Git'te durur, MCP `Resource` olarak sunulur (ADR-0014 §A deseni), koşuda yalnız
> `profile_fingerprint` kaydedilir. **Yeni tablo açılmaz** (ADR-0016 korunur).

### 5.3 Yol yürütmesi: graf motoru gerekmiyor

Kanıt yolu bir graf gezintisidir (`Subject → RoleAssignment → PermissionGrant`). Standart
karşılığı SQL:2023 **SQL/PGQ**'dur ve PostgreSQL'de **19 sürümüyle** geliyor; bugünün
alternatifi **Apache AGE** (openCypher eklentisi, PG 11-18).

> **Karar: ikisi de kullanılmaz.** Yol uzunluğumuz 2-4 atlama ve her atlama **anahtarla
> sınırlı bir okuma**dır. Bunu **ardışık probe çağrılarıyla** yürütmek hem yeterli, hem
> müşterinin veritabanına **eklenti kurmayı gerektirmez** — ki bu bizim en pahalı
> uygunluk şartımız olurdu. Graf motoru, yol uzunluğu ölçülerek büyüdüğünde tekrar açılır.

### 5.4 Tur 5 çıktısı — kanıt yolu **veridir**

```yaml
# evidence-paths.yaml — profil paketinin parçası
- pathKey: access-denied-403
  trigger: { statusCode: [401, 403] }          # ne zaman yürür
  steps:
    - nodeKind: ScopeRequired                   # API Checker · FailureIdentity.ChallengeScopes
      source: api.failureIdentity
    - nodeKind: SubjectResolved                 # profil: Subject kavramı
      source: db.projection
      concept: Subject
    - nodeKind: RoleHeld                        # profil: RoleAssignment
      source: db.projection
      concept: RoleAssignment
      joinFrom: SubjectResolved
    - nodeKind: GrantMatched
      source: db.projection
      concept: PermissionGrant
      joinFrom: RoleHeld
  verdict:
    confirmedWhen: "ScopeRequired.observed && !GrantMatched.contains(ScopeRequired)"
    inconclusiveWhen: "any(step.state == Unavailable)"
```

**Motor tek tanedir ve alanı bilmez.** Yeni bir teşhis sınıfı eklemek = **yeni bir yaml
girdisi**, yeni bir `if` değil. Kullanıcının istediği *"tek örnekten diğerlerini yorumla"*
davranışının mühendislik karşılığı budur.

---

## Sentez — generic köprünün beş bileşeni

| # | Bileşen | Ne yapar | Dayanak |
|---|---|---|---|
| 1 | **Tek sözlük** | İki checker'ın kodunu tek ajan sözlüğüne normalize eder | ADR-0018 §A |
| 2 | **Profil bağlama manifesti** | Kavram → somut tablo/kolon; bağlanmamış kavram = **soru** | Tur 2 (R2RML, semantik katman ölçümü) |
| 3 | **Kanıt yolu tanımları (veri)** | Hangi zincir ne zaman yürür; sonuç **açıklama ağacı** | Tur 1 + Tur 5 |
| 4 | **Yetenek çözümleyici** | Ayak izi / probe / test saati yeteneğini **yoklar**, seviye döndürür | Tur 3 |
| 5 | **Dar tool yüzeyi** | ≤7 aktif tool, şema talep üzerine, `resource_link` | Tur 4 + RULE-0007 |

**Ve hepsinin üzerinde tek değişmez:** her bileşen *"bunu cevaplayamıyorum"* diyebilmelidir.
Ölçüm bunun bedelini de faydasını da gösterdi: kapsam dışı soruda **%0 doğruluk + açık hata**,
**%70 doğruluk + %30 sessiz yanlıştan iyidir.**

---

## Kaynaklar

**Tur 1 — yetkilendirme teşhisi**
- GCP Policy Troubleshooter — `overallAccessState`, `bindingExplanations[]`, `rolePermission`, `combinedMembership`, `relevance` — <https://docs.cloud.google.com/policy-intelligence/docs/troubleshoot-access>
- IAP 403 sayfasından Policy Troubleshooter'a bağlanma — <https://cloud.google.com/chrome-enterprise-premium/docs/troubleshooter>
- AWS `SimulatePrincipalPolicy` — `EvalDecision`, `MatchedStatements` — <https://docs.aws.amazon.com/IAM/latest/APIReference/API_SimulatePrincipalPolicy.html>
- AWS `EvaluationResult` yapısı — <https://docs.aws.amazon.com/it_it/IAM/latest/APIReference/API_EvaluationResult.html>
- OpenFGA ilişki sorguları — `Expand` userset ağacı, `Check`, `ListObjects`, contextual tuples — <https://openfga.dev/docs/interacting/relationship-queries>
- SpiceDB doğrulama/test/hata ayıklama — `withTracing`, `debug_trace` — <https://authzed.com/docs/spicedb/modeling/validation-testing-debugging>

**Tur 2 — semantik bağlama**
- Semantic Layers for Reliable LLM-Powered Data Analytics (100 soru, 3 model, +17/+23 puan, p<0,01) — <https://arxiv.org/abs/2604.25149>
- dbt — Semantic Layer vs Text-to-SQL 2026 benchmark; kapsam içi %100 / kapsam dışı %0 ve *"başarısızlık bir hata mesajı gibi görünür"* — <https://docs.getdbt.com/blog/semantic-layer-vs-text-to-sql-2026>
- AtScale — semantik katman ve GenAI doğruluğu (satıcı ölçümü) — <https://www.atscale.com/blog/semantic-layers-make-genai-more-accurate/>
- dbt semantic model özellikleri — entities / dimensions / measures YAML — <https://docs.getdbt.com/docs/build/semantic-models>
- R2RML (W3C) uyum ve Ontop sanal bilgi grafiği — <https://github.com/ontop/ontop/wiki/W3C-R2RML-Compliance>
- Mapping Patterns for Virtual Knowledge Graphs — SE / SR / SRa / SRR / SH desen kataloğu — <https://arxiv.org/pdf/2012.01917>
- Rethinking Schema Linking (bağlama darboğazı) — <https://arxiv.org/pdf/2510.14296>
- BIRD-SQL benchmark — dış bilgi kanıtı (external knowledge evidence) — <https://bird-bench.github.io/>

**Tur 3 — etki ayak izi / yazma kümesi**
- EvoMaster beyaz kutu heuristikleri — çalışma anında yürütülen SQL toplama — <https://dl.acm.org/doi/10.1145/3652157>
- PostgreSQL logical decoding + wal2json; `wal_level = logical` ve yeniden başlatma şartı — <https://techcommunity.microsoft.com/blog/adforpostgresql/change-data-capture-in-postgres-how-to-use-logical-decoding-and-wal2json/1396421>
- Logical decoding eklenti karşılaştırması (`pgoutput` / `wal2json` / `test_decoding`) — <https://www.stacksync.com/blog/postgresql-logical-decoding-plugins-developers-guide>
- Npgsql replication API — `LogicalReplicationConnection`, `PgOutputReplicationSlot`, `SetReplicationStatus` — <https://www.npgsql.org/doc/replication.html>
- SQL Server Change Tracking ve CDC karşılaştırması (yalnız PK + I/U/D; şema değişikliği/trigger yok) — <https://learn.microsoft.com/en-us/previous-versions/sql/sql-server-2008-r2/cc280519(v=sql.105)>
- Track Data Changes — SQL Server — <https://learn.microsoft.com/en-us/sql/relational-databases/track-changes/track-data-changes-sql-server>

**Tur 4 — dinamik yüzey**
- MCP bağlam şişmesi: Tool Search, Code Mode, progressive disclosure ölçümleri — <https://mcp.directory/blog/mcp-context-bloat-fix-2026-tool-search-code-mode-progressive-disclosure>
- Üretim doğrulaması: 112 GitHub tool'unda %98 token azalması — <https://github.com/orgs/modelcontextprotocol/discussions/629>

**Tur 5 — analyzer ve paket modeli**
- k8sgpt — deterministik analyzer + yalnız anlatım için model; hash tabanlı önbellek — <https://deepwiki.com/k8sgpt-ai/k8sgpt>
- k8sgpt analyzer deseni — <https://devopslearning.medium.com/what-makes-k8sgpt-work-its-all-about-the-analyzers-part-4-76c219ec19dd>
- OPA bundle — sürümlü politika + veri artefaktı, `revision`, OCI ile paylaşım — <https://www.openpolicyagent.org/docs/management-bundles>
- SQL/PGQ PostgreSQL 19 ile geliyor; bugünün alternatifi Apache AGE — <https://pgweekly.github.io/en/2026/07/sql-property-graph-queries-pgq.html>
- Apache AGE genel bakış — <https://age.apache.org/overview/>
