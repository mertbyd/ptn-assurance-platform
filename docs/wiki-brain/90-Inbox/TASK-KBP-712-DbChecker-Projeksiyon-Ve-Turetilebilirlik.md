# AJAN GÖREVİ — KBP-712 · Database Checker: salt-okunur projeksiyon ve assertion türetilebilirlik kapısı

Tek görev, iki yüzey. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

---

## 0. Kimlik

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform\checkers\database-comparison
Paket   : CheckNexus.DatabaseComparison  (nuget.org)
Branch  : KBP-712   (KBP-711 üzerinden: git checkout KBP-711 && git checkout -b KBP-712)
Commit  : #KBP-712 <type>: <past-tense English description>
```

Derlenebilir dilimler, **en fazla 5 commit**, testler son dilimde. Boş dosya, yer tutucu,
kullanılmayan using girmez. Public sözleşme değişiyor → §7 sürüm/baseline adımı atlanamaz.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (aynı depoda) |
|---|---|---|
| Manager | `house-profile.md` → *Base classes* + *AppService has no private helpers* | `src/*.Domain/Managers/Assertions/RowAssertionManager.cs` |
| Repository (motor ayrımlı) | `data-access.md` | `src/*.EntityFrameworkCore/SchemaDiscovery/**` ve `Repository/Comparison/PostgreSql*` |
| Servis + arayüz | `house-profile.md` → *Contracts live in Application.Contracts* | `Services/SchemaDiscovery/ISchemaDiscoveryAppService.cs` |
| DTO / Validator | `mapping.md` | `Dtos/Assertions/**`, `FluentValidation/**` |
| Sabit / kod kümesi | `house-profile.md` → *Stable strings* | `Domain.Shared/Constants/**/*Codes.cs` |

**Kanonik kararlar:** `ADR-0007` (salt-okunur değişmez), **`ADR-0019 §F`** (projeksiyon ön koşulu),
`RULE-0006` (türetilebilirlik kapısı). Dayanak: `AUDIT-0001` **BULGU-06** ve **BULGU-03**.

---

## 2. Neden bu iş

**Yüzey A — projeksiyon.** Köprünün kanıt zinciri *"bu kullanıcının rolleri neler"* diye
soramıyor. Bugün DB Checker yalnız **beklenti doğruluyor** (`AssertRow/Count/Absent`) ve
**yapı anlatıyor** (`DescribeTable`, `GetSnapshot`). Değer okumanın tek yolu **kasten
başarısız olacak bir assertion yazmak** — bu bir tasarım kokusu, ürün yüzeyi değil.
Sonuç: 403 teşhis zincirinin her düğümü bugün `Unavailable` dönüyor. **Köprü yazıldı ama
bu yüzey olmadan ölü sermaye.**

**Yüzey B — türetilebilirlik.** RULE-0006 *"her assertion türetilebilir"* diyor, ama mekanizma
(`ValidateScenarioAssertionsAsync`) API Checker'da ve girdisi **JSON Pointer** — yalnız HTTP
gövdesini kapsıyor. `x-checknexus-db` ile yazılmış bir assertion **hiçbir kapıdan geçmeden**
yayınlanabiliyor. Kural yanlış güven veriyor.

---

## 3. Yüzey A — salt-okunur projeksiyon

### Sözleşme

```
POST /projections/rows
```

**Girdi** — adres + anahtar + kolon listesi. **Serbest SQL taşımaz** (assertion sözleşmesindeki
değişmez birebir korunur).

| Alan | Kural |
|---|---|
| `ConnectionId` | zorunlu |
| `SchemaName`, `TableName` | zorunlu; katalogdan doğrulanır |
| `KeyValues` | zorunlu, en az bir anahtar; `Dictionary<string,string?>` |
| `ProjectColumns` | zorunlu, en az bir kolon; **katalogda var olmayan kolon → hata** |
| `MaxRows` | opsiyonel; varsayılan ve tavan `Domain.Shared` sabitinden |
| `Correlation` | opsiyonel (KBP-711'de eklendi) |

**Çıktı**

| Alan | Anlamı |
|---|---|
| `OutcomeCode` | kapalı küme: `Projected` · `TableNotFound` · `ColumnNotFound` · `KeyNotUnique` · `NotAuthorized` · `Truncated` |
| `Rows` | `List<Dictionary<string,string?>>` — **redaksiyonlu** |
| `ObservedRowCount` | okunan satır sayısı |
| `Truncated` | tavan aşıldı mı |
| `Correlation` | echo |

### Değişmezler

1. **Salt-okunur.** Yalnız `SELECT`; yazma yolu **yok** (ADR-0007).
2. **Serbest SQL yok.** Sorgu adres + anahtar + kolon listesinden **kurulur**, dışarıdan
   alınmaz. Tüm değerler **parametrelidir**.
3. **Redaksiyon zorunlu.** Mevcut `FindingValueRedactor` deseni kullanılır; ham değer
   politikaya göre maskelenir. **Yeni redaksiyon mekanizması yazma.**
4. **Bütçe.** `MaxRows` tavanı aşılırsa `Truncated = true` — sessizce kesme yok.
5. **Katalog doğrulaması.** Tablo/kolon adları **katalogdan** doğrulanır; doğrulanmamış ad
   sorguya girmez.
6. **Motor ayrımı** mevcut resolver desenine gider (`EngineComponentResolver`), `if`/`switch`
   ile değil.

---

## 4. Yüzey B — assertion türetilebilirlik kapısı

### Sözleşme

```
POST /assertions/derivability
```

**Girdi:** `ConnectionId` + doğrulanacak assertion adresleri listesi
(`SchemaName`, `TableName`, `KeyColumns[]`, `ExpectedColumns[]`, `MatcherCode`, `CardinalityKindCode`).

**Çıktı:** her öğe için `{ tableRef, columnRef, outcomeCode }` — **API tarafındaki
`AssertionDerivabilityItemDto` ile aynı şekil** (`{jsonPointer, outcomeCode}`'un DB karşılığı).

### Kapının sorduğu dört soru

| # | Soru | Kaynak | Başarısızlık kodu |
|---|---|---|---|
| 1 | Hedef tablo var mı | katalog | `TableNotFound` |
| 2 | Beklenen kolonlar var mı | katalog | `ColumnNotFound` |
| 3 | Anahtar **PK veya unique** mi | `DescribeTable` | `KeyNotUnique` |
| 4 | Matcher kolon tipiyle uyumlu mu | `ColumnTypeConfidenceResolver` | `MatcherTypeMismatch` |

Hepsi geçerse `Derivable`. Kısmi geçiş **yok** — her öğe kendi sonucunu taşır.

**Yeni motor yazma:** dördü de mevcut yüzeylerin (`SchemaDiscovery` katalogu,
`DescribeTableAsync`, `ColumnTypeConfidenceResolver`) üzerine oturur.

---

## 5. Dosya manifestosu (≤35, sıra bağlayıcı)

**`Domain.Shared/Constants/`**
1. `Projections/ProjectionConsts.cs` — `DefaultMaxRows`, `MaxRowsCeiling`, `MaxProjectColumns`
2. `Projections/Lookups/ProjectionOutcomeCodes.cs` — altı kod + `All`
3. `Assertions/Lookups/AssertionDerivabilityCodes.cs` — `Derivable` `TableNotFound`
   `ColumnNotFound` `KeyNotUnique` `MatcherTypeMismatch` + `All`
4. `ExceptionCodes/` — mevcut validation sınıfına projeksiyon/türetilebilirlik kodları **ekle**
   (yeni dosya açma)

**`Domain/Models/`**
5. `Projections/ProjectionRequest.cs`
6. `Projections/ProjectionResult.cs`
7. `Projections/ProjectionRow.cs`
8. `Assertions/DerivabilityRequest.cs`
9. `Assertions/DerivabilityItem.cs`
10. `Assertions/DerivabilityResult.cs`

**`Domain/Interface/`**
11. `Projections/IProjectionRepository.cs` — motor bağımsız sözleşme

**`Domain/Managers/`**
12. `Projections/ProjectionManager.cs` — katalog doğrulaması → bütçe → repository → redaksiyon → outcome
13. `Assertions/AssertionDerivabilityManager.cs` — dört soruyu sırayla sorar, öğe başına sonuç üretir

**`EntityFrameworkCore/`**
14. `Repository/Projections/ProjectionRepositoryBase.cs` — parametreli `SELECT` kurulumu
15. `Repository/Projections/PostgreSqlProjectionRepository.cs`
16. `Repository/Projections/SqlServerProjectionRepository.cs` *(mevcut motor çiftini bozmamak için;
    kapsam dışı kalırsa `NotSupported` döner)*
17. Resolver kaydı — mevcut `EngineComponentResolver` deseni

**`Application.Contracts/`**
18–21. `Dtos/Projections/`: `ProjectionRequestDto`, `ProjectionResultDto`, `ProjectionRowDto`
22–24. `Dtos/Assertions/`: `DerivabilityRequestDto`, `DerivabilityItemDto`, `DerivabilityResultDto`
25. `Services/Projections/IProjectionAppService.cs`
26. `Services/Assertions/IAssertionDerivabilityAppService.cs`
27–28. `FluentValidation/Projections/ProjectionRequestDtoValidator.cs`,
    `FluentValidation/Assertions/DerivabilityRequestDtoValidator.cs`
29. `Permissions/` güncelle — `Projections.Execute`, `Assertions.ValidateDerivability`

**`Application/`**
30. `Services/Projections/ProjectionAppService.cs`
31. `Services/Assertions/AssertionDerivabilityAppService.cs`
32–33. `Mappers/Projections/ProjectionMapper.cs`, `Mappers/Assertions/DerivabilityMapper.cs`

**`HttpApi/Controllers/`**
34. `Projections/ProjectionController.cs`
35. `Assertions/` mevcut controller'a uç **ekle** (yeni dosya açma)

> 35'i aşarsan **SQL Server repository'sini** (madde 16) kes ve `NotSupported` bırak; kalan
> her şey tamamlanmalı.

---

## 6. Yasaklar

1. Serbest SQL taşıyan sözleşme; string birleştirmeyle sorgu kurma.
2. Yazma yolu açma (`INSERT`/`UPDATE`/`DELETE`/DDL) — ADR-0007 değişmezi.
3. Katalogda doğrulanmamış tablo/kolon adını sorguya sokma.
4. Yeni redaksiyon mekanizması yazma — mevcut `FindingValueRedactor` deseni.
5. Motor ayrımını `if`/`switch` ile çözme — resolver deseni.
6. Servis içinde `private` iş metodu; karar/hesap serviste.
7. `[MapProperty]`; mapper'da gövde; elle property atama.
8. DTO'yu `Application`'a koyma; arayüzsüz servis.
9. Nested tip; dosya içinde ikinci tip; yeni katman/klasör.
10. Geçmiş commit arkeolojisi; ara dilimlerde build/test.

---

## 7. Testler (son dilim)

| Test | Doğruladığı |
|---|---|
| `ProjectionReadOnlyTests` | Üretilen SQL yalnız `SELECT`; yazma anahtar kelimesi içermiyor |
| `ProjectionParameterTests` | Tüm anahtar değerleri **parametreli**; enjeksiyon denemesi literal olarak geçmiyor |
| `ProjectionCatalogTests` | Olmayan tablo → `TableNotFound`, olmayan kolon → `ColumnNotFound` |
| `ProjectionBudgetTests` | Tavan aşımında `Truncated = true`, satır sayısı tavanı geçmiyor |
| `ProjectionRedactionTests` | Hassas kolon değeri ham dönmüyor |
| `DerivabilityGateTests` | Dört sorunun her biri için doğru outcome; hepsi geçince `Derivable` |
| `DerivabilityGateTests` | PK/unique olmayan anahtar → `KeyNotUnique` |
| `DerivabilityShapeTests` | Çıktı şekli API tarafındaki derivability öğesiyle **hizalı** |

---

## 8. Paket sürümü ve baseline

Precedent: `#KBP-710 chore: raised the package version to 0.2.0-alpha.3 and moved the
validation baseline`. Sürümü `common.props`'tan yükselt, PackageValidation baseline'ını taşı,
csproj'a sabit sürüm yazma.

---

## 9. Bitiş

1. §6'nın 10 maddesini kendi kodunda tek tek kontrol et.
2. Son dilimi commit et.
3. Tek sefer: `dotnet build` → `dotnet test`.
4. `/abp-backend-dev` mimari incelemesi + `/backend-verify` gate'i.
5. Raporda: yeni public uçlar, yeni sürüm, yaptığın **her varsayım**.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → **bir kez** dene; döngüde tekrarlama; tek engelde
10 dakikadan fazla harcama.
