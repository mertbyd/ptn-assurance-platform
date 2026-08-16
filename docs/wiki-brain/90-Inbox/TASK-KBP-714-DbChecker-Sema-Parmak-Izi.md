# AJAN GÖREVİ — KBP-714 · Database Checker şema parmak izi yüzeyi

Tek görev. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

**Bu görev KBP-91'den tamamen bağımsızdır.** Ayrı depo (`checkers/database-comparison`),
ayrı solution, ayrı sürüm hattı. KBP-91 yalnız `ptn-test-module` altında çalışır; iki iş
tek bir dosyayı bile paylaşmaz. Paralel yürütülebilir.

---

## 0. Kimlik

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform\checkers\database-comparison
Modül   : Database Checker   (CheckNexus.DatabaseComparison ailesi, 8 paket)
Branch  : KBP-714   (KBP-713 üzerinden)
Motor   : PostgreSQL + SQL Server (mevcut sağlayıcı ailesi)
Sürüm   : 0.2.0-alpha.7   ·   PackageValidationBaselineVersion = 0.2.0-alpha.6
Commit  : #KBP-714 <type>: <past-tense English description>
```

Derlenebilir dilimler, **en fazla 4 commit**, testler son dilimde. Yayın **yapılmaz** —
paketleme dry-run'da kalır.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek — **bu depoda**) |
|---|---|---|
| Saf hesaplayıcı | `house-profile.md` → *Base classes* | `src/Ptn.DatabaseChecker.Domain/Managers/Comparison/FindingFingerprintCalculator.cs` |
| Manager | `house-profile.md` → *Base classes* | `src/Ptn.DatabaseChecker.Domain/Managers/SchemaDiscovery/SchemaDiscoveryManager.cs` |
| AppService üyesi | `house-profile.md` → *Contracts live in Application.Contracts* | `src/Ptn.DatabaseChecker.Application.Contracts/Services/SchemaDiscovery/ISchemaDiscoveryAppService.cs` |
| DTO | `mapping.md` → *DTOs* | `src/Ptn.DatabaseChecker.Application.Contracts/Dtos/SchemaDiscovery/SchemaSnapshotDto.cs` |
| Validator | `mapping.md` → *Validation* | `src/Ptn.DatabaseChecker.Application.Contracts/FluentValidation/Projections/ProjectionRequestDtoValidator.cs` |
| Mapper | `house-profile.md` → *Mapper files contain declarations only* | mevcut SchemaDiscovery mapper'ı |
| Sabit | `house-profile.md` → *Stable strings* | `src/Ptn.DatabaseChecker.Domain.Shared/Constants/Comparison/Projections/ProjectionConsts.cs` |

**Kanonik kararlar:** `ADR-0007` (salt-okunur değişmez), `ADR-0020` (malzeme mührü — bu
görevin **talep sahibi**), `PLAN-0001 DBC-10` (özgün kapsam), `RULE-0002`.

---

## 2. Neden bu iş — kanıtlanmış boşluk

`ADR-0020` senaryo sürümünü dört malzemeye mühürlüyor ve risk tablosunda şunu iddia ediyor:

> *"Mühür hesabı pahalı olur → Şema mührü zaten `ISchemaKnowledgePort.GetSchemaFingerprintAsync`'te;
> snapshot mührü API Checker'da hazır."*

**İddianın yarısı yanlış.** Kod seviyesinde doğrulandı (2026-08-14):

| Malzeme | Kimlik | İçerik mührü | Gerçek |
|---|---|---|---|
| API sözleşmesi | `spec_snapshot_id` | `spec_fingerprint` | ✅ `SpecContentDto.CanonicalHash` / `RawHash` **var** |
| DB şeması | `db_connection_id` | `db_schema_fingerprint` | ❌ **Database Checker'da hiçbir karşılığı yok** |

`SchemaFingerprint`, `GetSchemaFingerprint`, `SchemaHash`, `snapshot_fp` aramaları
`checkers/database-comparison/src` altında **sıfır** sonuç veriyor. `ISchemaKnowledgePort`
Test Module'ün PLAN-0004 manifestosunda tarif edilmiş bir porttu; port soyutlamaları
KBP-89'da kaldırıldı ve arkasında zaten checker yüzeyi yoktu.

**Sonucu:** senaryo aggregate'i (Faz 1) yazıldığında `ADR-0020 §B`'nin **dördüncü yayın
kapısı** (malzeme bütünlüğü) ve `§C`'nin koşum anı kayma tespiti **uygulanamaz**. Kapı ya
hiç yazılmaz ya da uydurma bir değerle doldurulur — ikisi de sessiz yanlış üretir.

Bu görev o boşluğu kapatır ve `PLAN-0001 DBC-10`'un birinci yarısını gerçekler.

---

## 3. İkinci kazanç — ucuz drift

`DBC-10`'un özgün gerekçesi ikinci bir değer taşıyor: **şema saklamadan drift tespiti.**

```
column_fp  →  table_fp  →  schema_fp  →  snapshot_fp
```

`snapshot_fp` iki koşuda aynıysa şema değişmemiştir; o dalın tam karşılaştırması **hiç
çalıştırılmaz**. Farklıysa hangi `table_fp`'nin değiştiği tam diff yapılmadan bulunur.
Kalıcı yazılan tek şey hash'lerdir (koşu başına birkaç yüz bayt) — şema fotoğrafı
saklanmaz (`PLAN-0001` kapsam dışı listesi: *"Şema snapshot'ını tabloya yazmak: eskir,
şişer, müşteri iç yapısını taşır"*).

---

## 4. Ne yapılacak

### 4.1 Saf hesaplayıcı

`Managers/SchemaDiscovery/SchemaFingerprintCalculator.cs` — `ITransientDependency`,
**tek sorumluluk: hash hesaplamak.** Ev precedent'i `FindingFingerprintCalculator`:
SHA-256, normalize edilmiş girdi, saf fonksiyon, I/O yok.

Girdi `SchemaSnapshotModel`'dir (mevcut derin okuyucunun çıktısı). Katman sırası:

| Seviye | Girdi bileşenleri |
|---|---|
| `column_fp` | kolon adı · kanonik tip · nullable · default · generated ifadesi · collation · identity |
| `table_fp` | şema adı · tablo adı · **sıralı** `column_fp` listesi · PK · unique · FK · check kısıtları |
| `schema_fp` | şema adı · **sıralı** `table_fp` listesi · şemadaki tablo-dışı nesne tanımları |
| `snapshot_fp` | motor kodu · veritabanı collation'ı · **sıralı** `schema_fp` listesi |

### 4.2 Kanoniklik — bu görevin en kritik maddesi

`ADR-0020` mührün **kanonik** olmasını şart koşuyor: *"sıralı, denetim/istatistik alanı
hariç; ilgisiz değişiklik mührü kaydırmaz."*

**Hash'e giremeyecek alanlar** (girerlerse her çağrı farklı mühür üretir ve `§C`'nin kayma
tespiti her koşuda `Inconclusive` verir):

- `SchemaSnapshotDto.CollectedAt` — **fotoğrafın çekilme anı**; en tehlikelisi budur
- satır sayısı, tablo boyutu, istatistik, `reltuples` benzeri her tahmin
- okuma sırası — her liste **kararlı bir anahtarla sıralanır**, geldiği sırayla değil
- ordinal pozisyon (kolon eklenip çıkarılınca kayar; ad esas alınır)

Girecek olan: yapının kendisi. Kolon **sırası** değil, kolon **kümesi** ve tanımları.

### 4.3 Yüzey

`ISchemaDiscoveryAppService`'e **tek üye** eklenir:

```csharp
Task<SchemaFingerprintDto> GetSchemaFingerprintAsync(
    Guid connectionId,
    List<string> schemaNames,
    CancellationToken cancellationToken)
    => Task.FromException<SchemaFingerprintDto>(new NotSupportedException());
```

**Varsayılan gövde zorunludur.** Bu deponun kendi precedent'i `DescribeTableAsync`'tir:
arayüze varsayılan uygulamayla eklenen üye `CP0006` kırığı üretmez ve
`CompatibilitySuppressions.xml` şişmez. Yeni bir AppService **açılmaz** — soru şema
keşfine aittir, sahibi `ISchemaDiscoveryAppService`'tir.

Dönen DTO:

| Alan | İçerik |
|---|---|
| `SnapshotFingerprint` | Tüm hedefin tek mührü — `ADR-0020`'nin `db_schema_fingerprint`'i |
| `AlgorithmCode` + `AlgorithmVersion` | Formül değişirse eski mühürler yanlış "kaydı" demez |
| `Schemas[]` | Şema adı + `schema_fp` |
| `Tables[]` | `schema.table` + `table_fp` — hangi tablonun kaydığı |
| `ComputedAt` | Bilgi alanı; **hash'in içinde değildir** |

Controller'a karşılık gelen `GET` ucu eklenir; route ve Swagger grubu `Domain.Shared`
sabitlerinden gelir.

### 4.4 Sürüm hattı

`common.props`: `Version` → `0.2.0-alpha.7`, `PackageValidationBaselineVersion` →
`0.2.0-alpha.6`. `PACKAGE-README` yeni ucu ve **kanoniklik sözleşmesini** yazar: hangi
alanların hash'e girdiği, hangilerinin girmediği ve `AlgorithmVersion`'ın ne zaman
artacağı. Tüketici mühre güveniyorsa bu sözleşme public API kadar bağlayıcıdır.

---

## 5. Yasaklar

1. Şema fotoğrafını tabloya **yazma** — yalnız hesapla ve dön (`PLAN-0001` kapsam dışı).
2. Hedef veritabanına **yazma** (`ADR-0007` salt-okunur değişmezi).
3. `CollectedAt`, satır sayısı, istatistik veya okuma sırasını hash'e **katma**.
4. MD5 veya FIPS dışı algoritma — **SHA-256** (`ADR-0016 §H` ile aynı gerekçe).
5. Yeni AppService, yeni proje, yeni katman açma — üye mevcut arayüze eklenir.
6. Arayüze **varsayılan gövdesiz** üye ekleme (`CP0006` kırığı).
7. Motor farkını `if`/`switch` ile çözme — mevcut sağlayıcı/resolver deseni kullanılır.
8. Serbest SQL; şema/tablo adları katalogdan doğrulanır.
9. Yayımlanmış `alpha.6` binary'sini değiştirme; push yapma.
10. Ara dilimlerde build/test; geçmiş commit arkeolojisi.

---

## 6. Testler (son dilim)

| Test | Doğruladığı |
|---|---|
| `SchemaFingerprintCalculatorTests` | Aynı şema iki kez → **aynı** mühür (kararlılık) |
| `SchemaFingerprintCalculatorTests` | Yalnız `CollectedAt` farklı → mühür **değişmiyor** |
| `SchemaFingerprintCalculatorTests` | Okuma sırası karıştırılınca → mühür **değişmiyor** |
| `SchemaFingerprintCalculatorTests` | Tek kolonun tipi değişince → `snapshot_fp` **ve** yalnız o `table_fp` değişiyor |
| `SchemaFingerprintCalculatorTests` | Kolon eklenince ilgisiz tablonun `table_fp`'si **aynı** kalıyor |
| `SchemaFingerprintCalculatorTests` | Çıktı 64 karakter, kararlı büyük/küçük harf biçimi |
| `SchemaDiscoveryAppServiceTests` | Uç, katalogda olmayan şema adında hata döndürüyor |
| Paket uyumluluk kapısı | `alpha.6` baseline'ına karşı **yeni `CP0006` suppression'ı yok** |

---

## 7. Kabul kriterleri

- `GetSchemaFingerprintAsync` public sözleşmede ve `alpha.6` baseline'ını **kırmıyor**.
- Aynı hedef, aynı yapı → bit düzeyinde aynı `SnapshotFingerprint`.
- Zaman, istatistik ve okuma sırası mühre **girmiyor** (üç ayrı testle kanıtlı).
- Değişen tek tablo `Tables[]` içinde tek başına işaretleniyor.
- Şema fotoğrafı hiçbir tabloya yazılmıyor; hedefe yazma yok.
- `AlgorithmCode`/`AlgorithmVersion` dönüyor ve `PACKAGE-README`'de tanımlı.
- Release build 0 hata; tüm testler yeşil; 8 `.nupkg` + 8 `.snupkg` dry-run temiz.
- Migration üretilmedi.

---

## 8. Bitiş

1. §5'in 10 maddesini kendi kodunda tek tek kontrol et.
2. Son dilimi commit et.
3. Tek sefer: Release build → tüm testler → paketleme dry-run.
4. `/abp-backend-dev` mimari incelemesi + `/backend-verify` gate'i.
5. Raporda: dosya listesi, hash'e giren/girmeyen alanların **tam listesi**, her varsayım.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout; kilit hatasında
`dotnet build-server shutdown` → **bir kez** dene; aynı komutu döngüde tekrarlama;
tek engelde 10 dakikadan fazla harcama. **Push yok.**

---

## 9. Bu görevin açtığı kapı

| Kayıt | Madde | Durum |
|---|---|---|
| `ADR-0020 §A` | `db_schema_fingerprint` kolonu doldurulabilir | Bu görevle mümkün olur |
| `ADR-0020 §B/4` | Yayın kapısı: malzeme bütünlüğü | Faz 1'de uygulanabilir hâle gelir |
| `ADR-0020 §C` | Koşum anı kayma → `Inconclusive` | Faz 2'de uygulanabilir hâle gelir |
| `PLAN-0001 DBC-10` | Merkle şema fingerprint | Birinci yarısı kapanır |
| `ADR-0016` risk satırı | *"Ortam kayması yanlış alarma dönüşür"* önlemi | Gerçek dayanağını kazanır |

**Kapsam dışı, bilinçli:** `DBC-10`'un ikinci yarısı — karşılaştırma motorunun `schema_fp`
eşitliğinde dalı atlaması — ayrı bir performans işidir ve ölçüm ister. Bu görev mührü
**üretir**, onu optimizasyon kararı olarak **kullanmaz**.

---

## 10. `ADR-0020` düzeltme notu

Bu görev bittiğinde `ADR-0020`'nin risk tablosundaki *"Şema mührü zaten
`ISchemaKnowledgePort.GetSchemaFingerprintAsync`'te"* satırı düzeltilir: mühür
`ISchemaDiscoveryAppService.GetSchemaFingerprintAsync`'ten gelir ve `0.2.0-alpha.7` ile
publictir. Satır bugünkü hâliyle **var olmayan bir yüzeye** atıf yapıyor.
