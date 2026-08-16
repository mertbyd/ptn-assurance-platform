# AJAN GÖREVİ — KBP-89 · Köprü ajan yüzeyi, tool bütçesi ve yetenek seviyesi

Tek görev. **Her dosyayı yazmadan önce §1'deki kapıdan geç.** Kural hatırlamıyorsan yazma, önce
skill'e bak. Bu görevde yeniden icat edilecek hiçbir şey yok — her şeyin evde bir örneği var.

---

## 0. Kimlik

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-89            (git checkout KBP-88 && git checkout -b KBP-89)
Motor   : PostgreSQL
```

Commit: `#KBP-89 <type>: <past-tense English description>`. Derlenebilir dilimler, **en fazla 6
commit**, testler son dilimde. Boş dosya, yer tutucu, kullanılmayan using commit'e girmez.

---

## 1. YAZMA KAPISI — her dosya tipi için zorunlu

Bir dosya açmadan önce, o satırdaki referansı **oku**:

| Yazacağın dosya | Önce oku | Sonra bak (canlı örnek) |
|---|---|---|
| Manager | skill `house-profile.md` → *Base classes* + *AppService has no private helpers* | `ptn-api-contract-checker/src/*.Domain/Managers/Diagnosis/DiagnosisManager.cs` |
| AppService | skill `house-profile.md` → *Contracts live in Application.Contracts* | `ptn-api-contract-checker/src/*.Application/Services/Diagnosis/DiagnosisAppService.cs` |
| Servis arayüzü | aynı | `ptn-api-contract-checker/src/*.Application.Contracts/Services/Diagnosis/IDiagnosisAppService.cs` |
| DTO | skill `mapping.md` → *DTOs* | `ptn-api-contract-checker/src/*.Application.Contracts/Dtos/Diagnosis/*.cs` |
| Validator | skill `mapping.md` → *Validation* | `ptn-api-contract-checker/src/*.Application.Contracts/FluentValidation/**` |
| Mapper | skill `house-profile.md` → *Mapper files contain declarations only* | `ptn-api-contract-checker/src/*.Application/Mappers/Diagnosis/DiagnosisMapper.cs` |
| Controller | skill `layers-and-files.md` → *Controller* | `ptn-api-contract-checker/src/*.HttpApi/Controllers/**` |
| Sabit / kod kümesi | skill `house-profile.md` → *Stable strings* | `ptn-api-contract-checker/src/*.Domain.Shared/Constants/**/*Codes.cs` |
| Model | skill `house-profile.md` → *One type, one file* | `ptn-api-contract-checker/src/*.Domain/Models/**` |

**Referans depo:** `C:\Users\mertb\RiderProjects\ptn-api-contract-checker` — bu modülün
mimarisi oradan kopyalanır. Foundation base'leri için:
`C:\Users\mertb\RiderProjects\nexum-abp-foundation`, tüketici örneği
`C:\Users\mertb\RiderProjects\nexum-abp-filemodule`.

Kanonik kararlar: `docs/wiki-brain/03-Decisions/ADR-0018-*.md`, `ADR-0019-*.md`,
`02-Rules/RULE-0005`, `RULE-0007`.

---

## 2. Bu görev ne yapıyor

KBP-88 köprünün deterministik çekirdeğini kurdu ama **dışarıdan erişilemez**. Bu görev onu
ajana açar: ≤7 tool, kapalı seçimli girişler, `concise|detailed` yanıt, ve *"bu operasyon
DB'de neyi değiştiriyor"* sorusunun yetenek yoklamalı cevabı.

**Zincir bu görevde kapanır:** `Manager → AppService → Dto/Model/Mapper → FluentValidation →
Controller`. Bağlanmamış manager yarım iştir. Model/LLM çağrısı **yoktur**.

---

## 3. Kodun yeri — kesin tablo

| Proje / klasör | İçinde ne olur |
|---|---|
| `Domain.Shared/Constants/Bridge/` | Kapalı kod kümeleri, sınırlar, route/swagger sabitleri |
| `Domain.Shared/ExceptionCodes/Bridge/` | Hata kodları |
| `Domain/Models/Bridge/` | Davranışsız domain modelleri |
| `Domain/Interface/Bridge/` | Port arayüzleri |
| `Domain/Managers/Bridge/` | **BÜTÜN İŞ**: normalizasyon, redaksiyon, kod çevirisi, eşleme sözlüğü, fingerprint, kanonik sıralama, bütçe, eşik, hüküm, birleştirme |
| **`Application.Contracts/Dtos/Bridge/`** | **Tüm DTO'lar** |
| **`Application.Contracts/Services/Bridge/`** | **Tüm servis arayüzleri** (`I*AppService.cs`, `I*Service.cs`) |
| `Application.Contracts/FluentValidation/Bridge/` | Validator'lar |
| `Application.Contracts/Permissions/` | İzin sabitleri ve tanım sağlayıcı |
| `Application/Services/Bridge/` | AppService **ve** port uygulamaları — yalnız çağrı + delege |
| `Application/Mappers/Bridge/` | Mapperly `[Mapper] partial class` — yalnız bildirim |
| `HttpApi/Controllers/Bridge/` | Route + yetki + tek servis çağrısı |
| `EntityFrameworkCore/` | **Yalnız** `Configurations/`, `Repository/`, `EntityFrameworkCore/`, `Migrations/` |

**Yasak klasörler:** EF Core'da `Adapters/`, `Documents/`, `Mappers/`; her yerde `Helpers/`,
`Utils/`, `Engines/`, `Handlers/`, `Factories/`, `Infrastructure/`. EF Core csproj'unda
Mapperly bağımlılığı **olmayacak** — KBP-88'den kalmışsa **kaldır**.

> **DTO `Application`'a konmaz.** `HttpApi.Client`, izin tanımları, diğer modüller ve MCP
> tüketicisi **contracts** assembly'sini referanslar. DTO orada değilse tüketilemez.
> **Her servisin arayüzü `Application.Contracts/Services/` altındadır** — port uygulamaları
> dâhil.

---

## 4. "Bu kodu yazdıysan yanlış yerdesin" — gerçek vakalar

KBP-88'de bunların hepsi yapıldı. Tekrarı **ret** sebebidir.

| Gördüğün kod | Nerede yazılmıştı | Doğru yeri |
|---|---|---|
| `private static PtnAssertionResult Normalize(...)` | Servis | **Manager** |
| `private static string? Redact(string? value)` | Servis | **Manager** |
| `private static readonly IReadOnlyDictionary<string,string> OutcomeMap = {...}` | Servis | **Manager** |
| Kanonik sıralama + `JsonSerializer.Serialize` + `SHA256.HashData` (fingerprint) | Servis | **Manager** |
| `description.DbSchemaName = result.SchemaName;` (mapper'dan sonra elle düzeltme) | Servis | **Manager** — ya da adı hizala, Mapperly halletsin |
| `input.Assertion.SchemaName = request.Location.DbSchemaName ?? string.Empty;` | Servis | **Manager** |
| `input.ContractCheckRunId = request.ApiRunId; input.OperationId = ...;` (dört satır elle atama) | Servis | **Manager**, eşleme **Mapperly** |
| `if (!string.IsNullOrWhiteSpace(request.OutcomeCode)) { ... } else { ... }` (istek şekli seçimi) | Servis | **Manager** |
| `result.Keys = input.UniqueIndexes.Select(...).ToList(); result.Keys.Insert(0, ...)` | Mapper | **Manager** |
| `[MapProperty(nameof(X), nameof(Y))]` | Mapper | **Hiçbir yer** — adı hizala |
| `class XDto : RowAssertionRequestDto { public string DbSchemaName { get => SchemaName; } }` | Contracts | **Hiçbir yer** |
| DTO dosyası `Application/` altında | Application | **Application.Contracts/Dtos/** |
| `*Document` transport ikizi | Manager/adapter | **Hiçbir yer** — doğrudan modele deserialize |

**Kural tek cümle:** Servis **karar vermez, hesap yapmaz, çevirmez, düzeltmez**. Servis metodu
şu üç satırdır:

```csharp
// islevi: <use-case>
public async Task<XDto> DoAsync(XRequestDto input)
{
    await Validator.ValidateAndThrowAsync(input);
    var result = await Manager.DoAsync(Mapper.MapToModel(input));
    return Mapper.MapToDto(result);
}
```

`private` anahtar kelimesi yanlış yerdeki iş kodunu meşru yapmaz; yalnız incelemeden gizler.

---

## 5. Base zinciri

**Aggregate'i olan iş (CRUD)** → Foundation paketi:

```csharp
using Nexum.Abp.Foundation.Managers;                  // FoundationManager<TEntity, TKey>
using Nexum.Abp.Foundation.Application.Services;      // BaseApplicationService<...10 arg>
using Nexum.Abp.Foundation.EntityFrameworkCore;       // BaseEfCoreRepository<,,>
using Nexum.Abp.Foundation.Repositories;              // IBaseRepository<TEntity, TKey>
using Nexum.Abp.Foundation.Querying;                  // RepositoryQuery/Page/Order<T>
```

`FoundationManager` hazır veriyor: `EnsureExistsAsync`, `EnsureAllExistAsync`,
`EnsureUniqueAsync` (4 aşırı yükleme), `EnsureUniqueValuesAsync`, `EnsureDistinctValues`,
`NormalizeRequiredText`, `NormalizeOptionalText`, `EnsureEnumDefined`, `AlreadyExistsErrorCode`.
**Yeniden yazma.** Örnek: `FileCategoryManager : FoundationManager<FileCategory, Guid>`.

**Aggregate'i olmayan iş (köprü)** → modül base'i. `FoundationManager` `IAggregateRoot<TKey>`
ister, köprünün entity'si yok; zorlanmaz:

```csharp
public abstract class TestModuleDomainService : DomainService { }   // yoksa OLUŞTUR
public class FootprintCapabilityManager : TestModuleDomainService { }
public class PtnBridgeAppService : TestModuleAppService, IPtnBridgeAppService { }
```

**Yasak:** somut sınıfın doğrudan `DomainService` / `ApplicationService` / `EfCoreRepository<>`
türetmesi. **KBP-88 düzeltmesi:** `TestModuleDomainService` yoksa oluştur, KBP-88 manager'larını
ona kaydır.

---

## 6. Mapper kuralı

Mapper dosyası **yalnız** bildirim içerir:

```csharp
[Mapper]
public partial class PtnBridgeMapper
{
    public partial PtnExplainResultDto MapToDto(PtnChainResult model);
    public partial PtnExplanationNodeDto MapToDto(PtnExplanationNode model);
    public partial PtnEvidenceDto MapToDto(PtnEvidence model);
    public partial PtnCoverageReportDto MapToDto(PtnCoverageReport model);
}
```

Yasak: `[MapProperty]`, gövdeli metot, `if`, döngü, `.Select`, `.Insert`, iki kaynağı
birleştirme, eşleme sonrası düzeltme. Birleştirme ve sıralama **manager**'ın işidir.

Ad uyuşmazlığı **adı hizalayarak** çözülür. `DbSchemaName`/`ApiSchemaName` ayrımı yalnız
köprünün **kendi** konum ve rapor tiplerinde zorunludur (orada iki anlam aynı anda taşınır);
tek yönlü istek modelinde belirsizlik yoktur, checker'ın alan adı kullanılır.

---

## 7. Dosya manifestosu

**`Domain.Shared/Constants/Bridge/`**
1. `PtnToolCodes.cs` — 7 tool kodu + `ActiveMax = 7` + `All`
2. `PtnResponseFormatCodes.cs` — `concise` `detailed` + `All`
3. `PtnFootprintStrengthCodes.cs` — `Exact` `RowAddressed` `Inferred` `Unavailable` + `All`
4. `PtnBridgeRoutes.cs` — route şablonları + swagger grubu

**`Domain/`**
5. `TestModuleDomainService.cs` — modül manager base'i (§5)

**`Domain/Models/Bridge/`**
6. `PtnFootprintResult.cs` — `StrengthCode`, `Tables`, `Columns`, `RowDeltas`, `IsAdvisoryOnly` (**hep true**), `Reasons`
7. `PtnRowDelta.cs`
8. `PtnCapabilityLevel.cs` — `FootprintStrengthCode`, `HasLogicalDecoding`, `HasExclusiveSandbox`, `HasProjectionSurface`, `Reasons`
9. `PtnGroundingResult.cs`
10. `PtnClosedQuestion.cs` — `QuestionCode`, `Prompt`, `Options`, `GapKindCode`

**`Domain/Interface/Bridge/`**
11. `IWriteSetCapabilityPort.cs` — `ProbeCapabilityAsync`, `CaptureWriteSetAsync`, `ReleaseAsync`

**`Domain/Managers/Bridge/`**
12. `FootprintCapabilityManager.cs` — yetenek yoklar, strateji seçer, **`finally`'de `ReleaseAsync`**, seviye döner, paylaşımlı ortamda `Unavailable`
13. `ToolCatalogManager.cs` — aktif tool ≤ 7, toolset, dinamik keşif, **kademe 4 hariç**
14. `GroundingManager.cs` — tek çağrı zemini; eşik altı skorda **kapalı uçlu soru**
15. `SchemaFingerprintManager.cs` — kanonik sıralama + serileştirme + SHA-256 (**KBP-88'de servise yazılmıştı, buraya taşı**)

**`Application.Contracts/Dtos/Bridge/`**
16–27. `PtnGroundRequestDto`, `PtnGroundResultDto`, `PtnExplainRequestDto`, `PtnExplainResultDto`,
`PtnExplanationNodeDto`, `PtnEvidenceDto`, `PtnValidateRequestDto`, `PtnValidateResultDto`,
`PtnKnowledgeRequestDto`, `PtnKnowledgeResultDto`, `PtnCoverageReportDto`, `PtnToolCatalogDto`

Her giriş DTO'sunda operasyon/tablo/kolon/kod/scope alanı **serbest metin değil** — `Guid`
referans veya kapalı kod kümesinden değer. Tek istisna `StepIntent`, o da **asla** tablo/kolon
adı olarak kullanılmaz. Her yanıt DTO'sunda `ResponseFormat`; `concise`'de karar + kritik olgu
**başta**, ağır gövde `ResourceLink`.

**`Application.Contracts/Services/Bridge/`**
28. `IPtnBridgeAppService.cs` — `GroundAsync`, `ExplainAsync`, `ValidateAsync`, `GetKnowledgeAsync`, `GetToolCatalogAsync`
29. `IWriteSetCapabilityService.cs` — port uygulamasının dış sözleşmesi

**`Application.Contracts/FluentValidation/Bridge/`**
30–33. Dört request validator. `ResponseFormat` `PtnResponseFormatCodes.All` içinde olmalı.

**`Application/Services/Bridge/`**
34. `PtnBridgeAppService.cs` — `TestModuleAppService` türer; her metot **≤ 6 satır**
35. `WriteSetCapabilityService.cs` — PostgreSQL yoklaması (`SHOW wal_level`, replication yetkisi), **geçici** slot aç/tüket/düşür; karar ve hesap **manager**'da; başka motorda `Unavailable`

**`Application/Mappers/Bridge/`**
36. `PtnBridgeMapper.cs` — §6 biçiminde

**`HttpApi/Controllers/Bridge/`**
37. `PtnBridgeController.cs` — uç başına tek servis çağrısı

**`Domain/Models/Bridge/Profiles/` — ADR-0020 malzeme mührü**
38. `PtnMaterialSeal.cs` — `RulesFingerprint`, `SpecSnapshotId`, `SpecFingerprint`,
    `DbConnectionId`, `DbSchemaFingerprint`, `ProfileFingerprint`, `IsComplete`
39. `PtnProfilePack.cs` **düzeltmesi** — `SpecFingerprint` ve `DbConnectionId` alanları eklenir

`PtnGroundingResult` ve `PtnValidateResult` bu mührü taşır. Ajan mührü **yazmaz, taşır** —
değerler checker'dan gelir (`ISchemaKnowledgePort.GetSchemaFingerprintAsync`, API snapshot).

`ValidateAsync` iki ek kontrol yapar (ADR-0020 §B): (4) dört malzemenin kimlik+mührü dolu mu,
(5) derlenmiş belgenin **`sourceDescriptions`** girdileri `SpecSnapshotId`'ye çözülüyor mu.
İkisinden biri düşerse `IsPublishable = false`. Bu kontroller **manager**'da yaşar.

**Değişecek mevcutlar (≤5):** `TestModulePermissions.cs`, `TestModulePermissionDefinitionProvider.cs`,
`TestModuleDomainModule.cs`, `TestModuleApplicationModule.cs`, host MCP kaydı (varsa).

**KBP-88 temizliği (aynı dilimde):** EF Core'daki `Adapters/`/`Documents/`/`Mappers/` klasörlerini
sil, içeriği §3'teki yere taşı; EF Core csproj'undan Mapperly'yi kaldır; `Application` altındaki
DTO'ları `Application.Contracts/Dtos/Bridge/`'e taşı; arayüzü olmayan servislere
`Application.Contracts/Services/Bridge/` altında arayüz aç; servislerdeki `private` iş
metotlarını manager'a taşı.

---

## 8. Yasaklar — ihlal = ret

1. Servis içinde `private` iş metodu, eşleme sözlüğü, hesap, redaksiyon, fingerprint.
2. Eşleme sonrası elle alan düzeltmesi (`x.A = y.B;`).
3. `[MapProperty]`; mapper'da gövdeli metot.
4. DTO'nun `Application` altında olması; arayüzsüz servis.
5. Checker DTO'sundan türeyen alias DTO.
6. Sahip olunan modelin `*Document`/`*Payload` ikizi.
7. Nested tip; dosya içinde ikinci tip.
8. EF Core'da `Adapters/`/`Documents/`/`Mappers/` veya Mapperly bağımlılığı.
9. `Lookups/` klasörünü tablosu olmayan kodlar için açmak.
10. Doğrudan `DomainService`/`ApplicationService`/`EfCoreRepository<>` türetmek.
11. Boş dosya, yer tutucu, kullanılmayan using commit'lemek.
12. `git log` / eski revizyon arkeolojisi; yeni proje/katman; checker'ı doğrudan çağırmak;
    yeni tablo/migration; model çağrısı.

---

## 9. Testler (son dilim)

| Test | Doğruladığı |
|---|---|
| `ToolCatalogTests` | Aktif tool ≤ 7; kademe 4 eylemi katalogda yok |
| `ToolSchemaTests` | Giriş DTO'larında operasyon/tablo/kolon/kod/scope alanı serbest `string` değil |
| `BridgeTypeLayoutTests` | Nested tip yok; dosya başına tek public tip; DTO'lar `Application.Contracts` assembly'sinde |
| `BridgeSurfaceTests` | `Managers/Bridge/` altındaki her manager en az bir AppService metodundan erişilebiliyor |
| `FootprintCapabilityManagerTests` | `wal_level != logical` → `Inferred`/`Unavailable`, exception yok · paylaşımlı ortam → `Unavailable` · hata olsa bile `ReleaseAsync` çağrılıyor · dört seviyede `IsAdvisoryOnly == true` |
| `GroundingManagerTests` | Eşik altı skorda aday listesi yok, kapalı uçlu soru var |
| `MaterialSealTests` | Dört malzemeden biri eksikse `IsPublishable = false` · `sourceDescriptions` `SpecSnapshotId`'ye çözülmüyorsa `IsPublishable = false` · mühür alanlarının hiçbiri ajan girdisinden doldurulamıyor |

Boş veya yer tutucu test dosyası yazılmaz.

---

## 10. Komut hijyeni

- build/test çağrılarına **en az 600000 ms** timeout; kısa timeout MSBuild sürecini canlı
  bırakır ve Fody DLL kilidi doğurur.
- `dotnet build Ptn.TestModule.slnx -m:1`; ilk build restore etsin, sonrakiler `--no-restore`.
- Kilit hatasında: `dotnet build-server shutdown` → kalan süreçleri kapat → **bir kez** dene.
- Aynı komutu **döngüde tekrarlama**; iki denemede geçmiyorsa dur.
- Kilit/timeout'u **kod hatası sanma**.
- Tek engelde **10 dakikadan fazla** harcama; dur, tek paragraf rapor et, devam et.
- Ara dilimlerde build/test **çalıştırma**.

---

## 11. Bitiş

1. §4 tablosunu ve §8'in 12 maddesini **kendi kodunda tek tek** kontrol et.
2. Son dilimi commit et.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → `dotnet test Ptn.TestModule.slnx --no-restore`
4. `/abp-backend-dev` mimari incelemesi.
5. `/backend-verify` gate'i.
6. Bulunan sorun **aynı branch'te** düzeltilir.
7. Raporda: dosya listesi, açılamayan yüzeyler, yaptığın **her varsayım**.
