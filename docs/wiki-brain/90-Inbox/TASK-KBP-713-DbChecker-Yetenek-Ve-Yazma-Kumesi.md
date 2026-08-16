# AJAN GÖREVİ — KBP-713 · Database Checker: yetenek yoklama ve yazma kümesi yüzeyi

Tek görev. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

---

## 0. Kimlik

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform\checkers\database-comparison
Paket   : CheckNexus.DatabaseComparison  (nuget.org)
Branch  : KBP-713   (KBP-712 üzerinden)
Commit  : #KBP-713 <type>: <past-tense English description>
Motor   : PostgreSQL (diğerleri Unavailable)
```

Derlenebilir dilimler, **en fazla 4 commit**, testler son dilimde. Public sözleşme
değişiyor → §7 sürüm/baseline adımı atlanamaz.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (aynı depoda) |
|---|---|---|
| Manager | `house-profile.md` → *Base classes* | `src/*.Domain/Managers/Comparison/DatabaseDataComparisonManager.cs` |
| Repository (motor ayrımlı) | `data-access.md` | `Repository/Comparison/PostgreSqlDatabaseDataComparisonRepository.cs` |
| Servis + arayüz | `house-profile.md` → *Contracts live in Application.Contracts* | `Services/Comparison/ISchemaComparisonAppService.cs` |
| DTO / Validator | `mapping.md` | `Dtos/Comparison/**` |

**Kanonik karar:** **`ADR-0019 §E`** (dört seviyeli ayak izi + slot güvenliği), `ADR-0007`.

---

## 2. Neden bu iş — sınır ihlalini kapatıyor

Test Module bugün `Application/Services/Bridge/WriteSetCapabilityService.cs` içinde
**kendi `NpgsqlConnection`'ını açıyor**, `SHOW wal_level` çalıştırıyor ve replication slot
yönetiyor.

Bu üç şeyi birden deliyor:

1. **ADR-0007** — hedef veritabanına erişim **Database Checker'ındır**; Test Module'ün ikinci
   bir bağlantı yolu açması bu sahipliği böler.
2. **Bağlantı ve secret sahipliği** — DB Checker'ın kayıtlı bağlantı defteri,
   `ConnectionSafetyProfileResolver`'ı ve Vault secret yolu **atlanıyor**.
3. **Ev kuralı** — SQL ve bağlantı açma repository'nin işi; Application katmanında olmaz.

**Bu görev o yeteneği doğru sahibine taşır.** Test Module tarafındaki ham Npgsql kodu bu uç
hazır olunca silinecek.

---

## 3. Sözleşme

```
POST /capabilities/write-set/probe      → yetenek yoklama
POST /capabilities/write-set/capture    → yazma kümesi yakalama
POST /capabilities/write-set/release    → kaynak bırakma
```

### 3.1 Yoklama — `probe`

**Girdi:** `ConnectionId`, `RequiresExclusiveSandbox` (bool).

**Çıktı — `CapabilityLevel`:**

| Alan | Anlamı |
|---|---|
| `StrengthCode` | `Exact` · `RowAddressed` · `Inferred` · `Unavailable` |
| `HasLogicalDecoding` | `wal_level = logical` **ve** replication yetkisi var mı |
| `HasExclusiveSandbox` | çağıranın bildirdiği tekillik |
| `Reasons` | kapalı kod listesi: `SharedEnvironment` · `WalLevelNotLogical` · `NoReplicationGrant` · `EngineNotSupported` · `NoCapability` |

**Yoklama sırası — bu sıra bağlayıcıdır:**

```
1. Sandbox tekil degil        -> Unavailable  (SharedEnvironment)
2. Motor PostgreSQL degil     -> Unavailable  (EngineNotSupported)
3. wal_level = logical + yetki-> Exact
4. karsilastirma motoru var   -> Inferred     (once/sonra farki)
5. hicbiri                    -> Unavailable  (NoCapability)
```

**Hiçbir seviyede exception fırlatılmaz.** Dört seviye de **aynı sözleşmeyi** döndürür;
yalnız `StrengthCode` ve `Reasons` farklıdır.

### 3.2 Yakalama — `capture`

**Girdi:** `ConnectionId`, `CaptureRef` (çağıranın verdiği kimlik), `CandidateTables[]`
(FK grafiğiyle daraltılmış aday kümesi), `Correlation`.

**Çıktı — `WriteSetResult`:**

| Alan | Anlamı |
|---|---|
| `StrengthCode` | yoklamadan gelen seviye |
| `Tables[]` | değişen tablolar |
| `Columns[]` | (yalnız `Exact`) değişen kolonlar |
| `RowDeltas[]` | tablo + önce/sonra satır sayısı + delta |
| `IsAdvisoryOnly` | **her zaman `true`** |
| `Reasons[]` | seviye gerekçeleri |

**`IsAdvisoryOnly` sabittir ve `true`'dur.** Ayak izi **oracle değildir** (ADR-0018 §F):
gözlemden çıkar, yani uygulamadan öğrenme tuzağına açıktır. Onaysız assertion üretiminde
kullanılamaz. Bu alanı `false` yapan bir yol **yazma**.

### 3.3 Slot güvenliği — **operasyonel zorunluluk**

PostgreSQL logical decoding yolunda:

- Slot **geçici (temporary)** açılır.
- Yakalama bittiğinde **`finally` içinde garantili** düşürülür.
- Düşürülemezse sonuç `Reasons`'a `SlotReleaseFailed` yazılır ve **çağırana bildirilir**.
- `release` ucu, çökme sonrası temizlik için ayrıca vardır.

**Gerekçe:** tüketilmeyen slot sunucunun WAL'i geri dönüştürmesini engeller ve **müşterinin
diski dolar**. "Yalnız okuyoruz, zararsız" sanılan bir yeteneğin üretimi durdurabileceği yer
burasıdır.

### 3.4 Denetim gürültüsü filtrelenir

Fark yönteminde (`Inferred`) `CreationTime`, `LastModificationTime`, `ConcurrencyStamp` gibi
denetim kolonları **sonuçtan çıkarılır**; aksi hâlde her operasyon "her tabloyu değiştirdi"
görünür.

---

## 4. Dosya manifestosu (≤28)

**`Domain.Shared/Constants/Capabilities/`**
1. `WriteSetConsts.cs` — slot ad öneki, yakalama zaman aşımı, aday tablo tavanı
2. `Lookups/FootprintStrengthCodes.cs` — dört kod + `All`
3. `Lookups/CapabilityReasonCodes.cs` — beş sebep + `SlotReleaseFailed` + `All`
4. `ExceptionCodes/` — mevcut sınıfa yakalama hataları **ekle**

**`Domain/Models/Capabilities/`**
5. `CapabilityProbeRequest.cs`
6. `CapabilityLevel.cs`
7. `WriteSetCaptureRequest.cs`
8. `WriteSetResult.cs`
9. `WriteSetTableDelta.cs`

**`Domain/Interface/Capabilities/`**
10. `IWriteSetRepository.cs` — `ProbeAsync`, `CaptureAsync`, `ReleaseAsync`

**`Domain/Managers/Capabilities/`**
11. `WriteSetCapabilityManager.cs` — §3.1 yoklama sırası, strateji seçimi, **`finally`'de
    release**, denetim kolonu filtresi, `IsAdvisoryOnly` sabiti

**`EntityFrameworkCore/Repository/Capabilities/`**
12. `WriteSetRepositoryBase.cs`
13. `PostgreSqlWriteSetRepository.cs` — `SHOW wal_level`, replication yetkisi sorgusu,
    **geçici** slot yaşam döngüsü, logical decoding okuma
14. `DiffWriteSetRepository.cs` — önce/sonra farkı; mevcut `DataRowCountComparisonManager` ve
    `TableDataComparisonManager` **üzerine oturur**, yeni karşılaştırma motoru **yazılmaz**
15. Resolver kaydı — mevcut `EngineComponentResolver` deseni

**`Application.Contracts/`**
16–20. `Dtos/Capabilities/`: `CapabilityProbeRequestDto`, `CapabilityLevelDto`,
   `WriteSetCaptureRequestDto`, `WriteSetResultDto`, `WriteSetTableDeltaDto`
21. `Services/Capabilities/IWriteSetCapabilityAppService.cs`
22–23. `FluentValidation/Capabilities/` iki validator
24. `Permissions/` güncelle — `Capabilities.Probe`, `Capabilities.Capture`

**`Application/`**
25. `Services/Capabilities/WriteSetCapabilityAppService.cs`
26. `Mappers/Capabilities/WriteSetMapper.cs`

**`HttpApi/Controllers/Capabilities/`**
27. `WriteSetCapabilityController.cs`

---

## 5. Yasaklar

1. `IsAdvisoryOnly`'yi `false` yapan yol yazma.
2. Slot'u `finally` dışında düşürme; release'i atlama.
3. Yeni karşılaştırma motoru yazma — mevcut manager'lar üzerine otur.
4. Herhangi bir seviyede **exception fırlatma**; `Unavailable` + `Reasons` döndür.
5. Motor ayrımını `if`/`switch` ile çözme — resolver deseni.
6. Yazma yolu açma; salt-okunur değişmezi delme.
7. Servis içinde `private` iş metodu; karar/hesap/slot yönetimi serviste.
8. `[MapProperty]`; mapper'da gövde; elle property atama.
9. DTO'yu `Application`'a koyma; nested tip; yeni katman/klasör.
10. Geçmiş commit arkeolojisi; ara dilimlerde build/test.

---

## 6. Testler (son dilim)

| Test | Doğruladığı |
|---|---|
| `CapabilityProbeTests` | Yoklama sırası birebir §3.1; her dalda doğru `StrengthCode` + `Reasons` |
| `CapabilityProbeTests` | `wal_level != logical` → `Inferred`/`Unavailable`, **exception yok** |
| `CapabilityProbeTests` | Paylaşımlı sandbox → `Unavailable` + `SharedEnvironment` |
| `CapabilityProbeTests` | PostgreSQL dışı motor → `Unavailable` + `EngineNotSupported` |
| `SlotLifecycleTests` | Yakalama hata verse bile `ReleaseAsync` çağrılıyor — slot sızmıyor |
| `SlotLifecycleTests` | Release başarısızsa `SlotReleaseFailed` sonuca yazılıyor |
| `WriteSetAdvisoryTests` | Dört seviyede de `IsAdvisoryOnly == true` |
| `WriteSetNoiseTests` | Denetim kolonları sonuçtan filtreleniyor |

---

## 7. Paket sürümü ve baseline

`common.props`'tan sürüm yükselt, PackageValidation baseline'ını taşı, csproj'a sabit sürüm
yazma. Precedent: `#KBP-710`.

---

## 8. Bitiş ve devir

1. §5'in 10 maddesini kontrol et; son dilimi commit et.
2. Tek sefer: `dotnet build` → `dotnet test`.
3. `/abp-backend-dev` + `/backend-verify`.
4. **Raporda ayrıca belirt:** bu uç hazır olduğunda Test Module'deki
   `Application/Services/Bridge/WriteSetCapabilityService.cs` içindeki **ham Npgsql kodu
   silinecek** ve yerine bu yüzeye çağrı gelecek. O temizlik ayrı bir Test Module task'ıdır.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → **bir kez** dene; döngüde tekrarlama.
