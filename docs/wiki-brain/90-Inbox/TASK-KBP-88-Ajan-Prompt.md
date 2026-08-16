# AJAN GÖREVİ — KBP-88 · Köprü sözlüğü, oracle portları ve kanıt yolu motoru

Bu dosyanın tamamı tek bir ajan görevidir. Baştan sona oku, sonra kodu yaz. Soru sorma, varsayım
yapma: cevabı burada yoksa `docs/wiki-brain` altındaki ilgili ADR'de vardır.

---

## 0. Kimlik

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-88            (KBP-87 üzerinden açılır: git checkout KBP-87 && git checkout -b KBP-88)
Commit  : #KBP-88 feat: created the checker bridge vocabulary, oracle ports and evidence path engine
Motor   : PostgreSQL
Dil     : Kod ve yorumlar bu depodaki mevcut biçime uyar (yorumlar Türkçe, ASCII)
```

**Bir görev = bir branch = bir commit.** Commit mesajı tek cümledir, yukarıdaki satırın aynısıdır.

---

## 1. Kod yazmadan önce zorunlu okuma

Sırayla:

1. `/abp-backend-dev` skill'ini çalıştır (C# + ABP işi olduğu için zorunlu).
2. `ptn-test-module/AGENTS.md` — modül değişmezleri.
3. `docs/wiki-brain/04-Architecture/Alti-An.md` — ürünün uçtan uca akışı.
4. `docs/wiki-brain/03-Decisions/ADR-0018-Checker-Koprusu-Tek-Sozluk-Tool-Butcesi-Ve-Kanit-Zinciri.md`
5. `docs/wiki-brain/03-Decisions/ADR-0019-Generic-Kopru-Profil-Paketi-Kanit-Yolu-Ve-Yetenek-Seviyesi.md`
   — **bu görevin birincil kaynağıdır.**
6. `docs/wiki-brain/02-Rules/RULE-0005`, `RULE-0006`, `RULE-0007`.
7. `docs/wiki-brain/03-Decisions/ADR-0015-Kosum-Siniri-Dis-Arazzo-Runner.md` §F (port/adapter kuralı).

**Precedent kod — biçimi buradan kopyalayacaksın, tasarımı değil:**

| Ne için | Dosya |
|---|---|
| Sabit (`*Codes`) dosyası biçimi | `checkers/api-contract/src/Ptn.ApiContractChecker.Domain.Shared/Constants/Conformance/Lookups/ConformanceOutcomeCodes.cs` |
| Port/AppService arayüz biçimi | `checkers/database-comparison/src/Ptn.DatabaseChecker.Application.Contracts/Services/SchemaDiscovery/ISchemaDiscoveryAppService.cs` |
| Servis biçimi ve yorum yoğunluğu | `checkers/api-contract/src/Ptn.ApiContractChecker.Application/Services/Sources/SpecSourceAppService.cs` |
| Nested tip kullanımı | `checkers/database-comparison/src/Ptn.DatabaseChecker.Application.Contracts/Dtos/Diagnosis/DiagnoseRequestDto.cs` |
| Karşı taraf DTO'ları (adapter bunları çevirecek) | `.../Dtos/Diagnosis/*.cs` ve `.../Dtos/Conformance/*.cs` her iki checker'da |

---

## 2. Bu görev ne yapıyor (tek paragraf)

Test Module iki checker'a soru sorar. İki checker'ın sözlüğü çakışıyor (`Passed` / `passed`,
`Match` / `Matches`, `SchemaName` iki farklı anlamda). Ajan bu ham sözlüğü **hiçbir zaman
görmemeli**. Bu görev, iki checker'ı **tek ajan sözlüğüne** normalize eden köprü çekirdeğini
kurar ve *"403 neden geldi"* / *"bu adım nereye oturuyor"* gibi soruları **elle kodlanmış akış
olmadan**, dosyadan okunan **kanıt yolu tanımlarıyla** yürüten motoru yazar.

**Bu görevde model/LLM çağrısı yoktur.** Katman tamamen deterministiktir.

---

## 3. Değişmez mimari kurallar

### 3.1 Katman zinciri
`Controller → AppService → Manager → Repository/Port`. Bu görevde Controller ve AppService
**yoktur** (KBP-89'da gelecek). Yazılan her şey `Domain.Shared`, `Domain` ve
`EntityFrameworkCore/Adapters` içindedir.

### 3.2 Sorumluluk
| Katman | Yapar | Asla yapmaz |
|---|---|---|
| `Domain.Shared/Constants` | Kapalı kod kümeleri, sınırlar, hata kodları | Davranış |
| `Domain/Models` | **Veri kabuğu** — property'ler | Metot, `if`, hesap |
| `Domain/Interface` | Port sözleşmesi | Uygulama |
| `Domain/Managers` | **Tüm iş**: doğrulama, normalizasyon, bütçe, hüküm, karar | Doğrudan checker çağrısı, EF, HTTP |
| `EntityFrameworkCore/Adapters` | DTO çevirisi + **tek sözlüğe normalizasyon** + hata çevirisi | Karar vermek, akıl yürütmek, iş kuralı |

### 3.3 Yasaklanan hareketler

**Bu altı madde ihlal edilirse iş reddedilir:**

1. **Nested tip yok.** Her tip kendi dosyasında, kendi klasöründe. Trigger, step, row, item,
   summary, detail — hepsi **kardeş dosyadır**, iç sınıf değil.
2. **Dosya içinde `private`/`internal` yardımcı, transport, document, wire veya row sınıfı
   açma.** Manager, adapter, servis veya mapper dosyasının içinde ikinci bir tip bulunmaz.
3. **Elle tek tek eşleme yok.** Property property atama yasaktır; onu bir private metoda veya
   "çeviri" yardımcısına saklamak da yasaktır. Eşleme **Mapperly**'nindir. Eşleme zor
   geliyorsa eksik olan şey ilişkidir: modele navigation property ekle, repository `Include`
   ile doldur, Mapperly düzleştirsin.
4. **Sahip olduğun bir modelin `*Document` / `*Payload` ikizini üretme.** Dış dosya doğrudan
   domain modeline deserialize edilir.
5. **`Lookups/` klasörü** yalnız gerçek lookup tablosu olan kodlar içindir. `Models/`,
   `Dtos/`, `Entities/` altında `Lookups/` **asla** açılmaz.
6. **Geçmiş commit'e bakma.** `git log`, eski revizyon diff'i, ilgisiz ağaç gezintisi yok.
   Görevin adını verdiği dosyaları, en yakın kardeş uygulamayı ve kuralları oku; cevap yoksa
   varsayımını raporda yaz ve ilerle. Token ve süre maliyettir.

Ayrıca:

- **Yeni proje, yeni katman açma.** `Infrastructure/`, `EventHandlers/`, `Handlers/`,
  `Factories/`, `Engines/`, `Helpers/`, `Utils/`, `Core/` klasörü **açılmaz**.
- Manager'ın yapacağı işi başka bir yere taşımak için sınıf **icat etme**.
- Checker AppService'ini doğrudan çağırma — **yalnız port üzerinden**.
- Checker tablosunu okuma, checker tablosuna FK verme, ortak transaction açma.
- Yeni entity, yeni tablo, yeni migration **üretme** (bu görevde entity yoktur).
- Serbest SQL taşıyan sözleşme yazma.
- Model/LLM istemcisi ekleme.
- **Ara adımda `dotnet build` / `dotnet test` / `dotnet restore` çalıştırma.** Doğrulama
  tüm iş bittiğinde tek sefer yapılacak.

### 3.4 Yazım kuralları
- Her tip iki satır yorumla başlar:
  ```csharp
  // islevi: <bu tip ne yapar>
  // sistemdeki gorevi: <sistemde neyi mumkun kilar / neyi engeller>
  ```
- Her authored metot tek satır niyet yorumu alır (`// <ne yapiyor ve neden>`).
- Yorumlar bir stajyerin okuyup anlayacağı netlikte olur; kodu tekrar etmez, **kararı** anlatır.
- Metot **≤ 25 satır**, **≤ 2 iç içe kontrol seviyesi**.
- Public metot sıralı adım gibi okunur; dal gövdesi, sorgu kurulumu, elle eşleme içermez.
- Üçten fazla ilişkili dönüş değeri → `Models/Bridge/` altında **adlandırılmış model**.
  **Tuple yasak.**
- Motor/sağlayıcı farkı `if`/`switch` ile değil, resolver deseniyle çözülür.
- Her anlamlı string `Domain.Shared` altında sahiplenilir; sınıf içinde string literal kalmaz.
- `nameof`, `const`, `IReadOnlyCollection<string> All` kullanımı precedent dosyalardaki gibidir.

---

## 4. Yazılacak dosyalar (35 dosya, sıra bağlayıcı)

> 35'i aşarsan **listenin sonundan** kes ve KBP-89'a devret. Hiçbir dosyayı yarım bırakma.

### 4.1 `src/Ptn.TestModule.Domain.Shared/Constants/Bridge/`
Namespace: `Ptn.TestModule.Constants.Bridge`

> **`Lookups/` alt klasörü açılmaz.** `Lookups/` yalnız **gerçek lookup tablosu** olan kodlar
> içindir; köprü kodlarının arkasında tablo yoktur (ADR-0016 §F). Hepsi doğrudan
> `Constants/Bridge/` altındadır.

| # | Dosya | İçerik (tam) |
|---|---|---|
| 1 | `PtnBridgeConsts.cs` | `MaxHopCount = 6`, `MaxEvidencePerNode = 3`, `MaxNodeCount = 32`, `MaxProjectionRows = 25`, `MaxProfilePackBytes = 262144`, `MaxReportBytes = 4096` |
| 2 | `PtnConceptCodes.cs` | `Subject`, `RoleAssignment`, `PermissionGrant`, `Resource`, `ResourceOwnership`, `TimeAnchor`, `Quota` + `All` |
| 3 | `PtnNodeKindCodes.cs` | `ScopeRequired`, `SubjectResolved`, `RoleHeld`, `GrantMatched`, `OperationBound`, `RequestExampleBuilt`, `TableDescribed`, `KeyUnique`, `AssertionDerivable`, `FootprintObserved` + `All` |
| 4 | `PtnEvidenceStateCodes.cs` | `Observed`, `NotObserved`, `Unavailable` + `All` |
| 5 | `PtnRelevanceCodes.cs` | `High`, `Normal` + `All` |
| 6 | `PtnVerdictCodes.cs` | `Confirmed`, `Likely`, `Possible`, `RuledOut`, `Inconclusive` + `All` |
| 7 | `PtnOutcomeCodes.cs` | Tek casing (**PascalCase**). İki checker'ın outcome kodlarının köprü formu + `All` |
| 8 | `PtnHypothesisCodes.cs` | Tek gramer. API checker'ın `H-xx-nn` ve DB checker'ın `RowNeverCreated` gramerlerinin köprü formu + `All` |
| 9 | `PtnSourceCheckerCodes.cs` | `ApiContract`, `DatabaseComparison`, `Runner`, `Bridge` + `All` |
| 10 | `PtnBindingStateCodes.cs` | `Proposed`, `Approved`, `Rejected` + `All` |

**7 ve 8 için yöntem:** iki checker'ın `Domain.Shared/Constants/**/*Codes.cs` dosyalarını aç,
üye kümelerini çıkar, köprü formunu yaz ve **eşleme tablosunu adapter'da** kur. Köprü formu
kaynak kodların birleşimi değildir; **anlamca eşdeğer tek kümedir.**

**Biçim örneği (birebir bu şekilde yaz):**
```csharp
namespace Ptn.TestModule.Constants.Bridge;

// islevi: Kanit dugumunun uc degerli gozlem durumunu tanimlar.
// sistemdeki gorevi: "okunamadi" ile "yok" ayrimini zorunlu kilar; bu ayrim olmadan kopru
// yanlis teshis uretir (ADR-0019 §C).
public static class PtnEvidenceStateCodes
{
    public const string Observed = "Observed";
    public const string NotObserved = "NotObserved";
    public const string Unavailable = "Unavailable";

    public static IReadOnlyCollection<string> All { get; } = [Observed, NotObserved, Unavailable];
}
```

### 4.2 `src/Ptn.TestModule.Domain.Shared/ExceptionCodes/Bridge/`
Namespace: `Ptn.TestModule.ExceptionCodes.Bridge`

| # | Dosya | Kodlar |
|---|---|---|
| 11 | `TestModuleBridgeErrorCodes.cs` | `ProfilePackNotFound`, `ProfilePackInvalid`, `ProfileFingerprintMismatch`, `ConceptNotBound`, `EvidencePathNotFound`, `HopBudgetExceeded`, `EvidenceUnavailable`, `CheckerCallFailed` |

Her kodun `Localization/TestModule/tr.json` + `en.json` karşılığı yazılır.

### 4.3 `src/Ptn.TestModule.Domain/Models/Bridge/`
Namespace: `Ptn.TestModule.Models.Bridge` · **hepsi salt-veri; metot yok**

| # | Dosya | Üyeler |
|---|---|---|
| 12 | `PtnAccessTuple.cs` | `string SubjectRef`, `string OperationId`, `List<string> RequiredPermissions`, `Dictionary<string,string?> Context` |
| 13 | `PtnLocation.cs` | `string? ApiSchemaName`, `string? DbSchemaName`, `string? DbTableName`, `string? ColumnName`, `string? OperationId`, `string? Method`, `string? Path`, `string? JsonPointer` — **`SchemaName` adlı alan yasak** |
| 14 | `PtnFindingRef.cs` | `string SourceCheckerCode`, `string Fingerprint` |
| 15 | `PtnEvidence.cs` | `string ProbeKindCode`, `string FactCode`, `string? ExpectedValue`, `string? ObservedValue`, `long? ObservedAtMs`, `PtnFindingRef? Ref` |
| 16 | `PtnExplanationNode.cs` | `string NodeKindCode`, `string StateCode`, `string RelevanceCode`, `PtnLocation Location`, `List<PtnEvidence> Evidence`, `List<PtnExplanationNode> Children` |
| 17 | `PtnChainResult.cs` | `string PathKey`, `string VerdictCode`, `PtnExplanationNode? Root`, `PtnCoverageReport Coverage`, `int HopCount`, `bool BudgetExceeded`, `List<string> OpenQuestions` |
| 18 | `PtnConceptBinding.cs` | `string ConceptCode`, `string SchemaNameValue`*, `string TableName`, `Dictionary<string,string> ColumnMap`, `string PatternCode`, `string StateCode`, `string? ApprovedBy` |
| 19 | `PtnEvidencePathDefinition.cs` | `string PathKey`, `PtnEvidencePathTrigger Trigger`, `List<PtnEvidencePathStep> Steps`, `string ConfirmedWhen`, `string InconclusiveWhen` |
| 19a | `PtnEvidencePathTrigger.cs` | `List<int> StatusCodes`, `List<string> OperationIds` — **ayrı dosya, nested değil** |
| 19b | `PtnEvidencePathStep.cs` | `string NodeKindCode`, `string SourceCode`, `string? ConceptCode`, `string? JoinFromNodeKindCode`, `Dictionary<string,string?> Parameters` — **ayrı dosya, nested değil** |
| 20 | `PtnProfilePack.cs` | `string ProfileKey`, `string Revision`, `string DbSchemaFingerprint`, `Guid? SpecSnapshotId`, `List<PtnConceptBinding> Bindings`, `List<PtnEvidencePathDefinition> Paths`, `string ContentFingerprint` |
| 21 | `PtnCoverageReport.cs` | `List<string> RequiredConcepts`, `List<string> BoundConcepts`, `List<string> UnboundConcepts`, `int BoundCount`, `int RequiredCount` |
| 22 | `PtnProjectionRequest.cs` | `Guid ConnectionId`, `string SchemaNameValue`*, `string TableName`, `Dictionary<string,string?> KeyValues`, `List<string> ProjectColumns`, `int MaxRows` |
| 23 | `PtnProjectionResult.cs` | `string StateCode`, `List<Dictionary<string,string?>> Rows`, `long ObservedRowCount`, `bool Truncated` |

\* `SchemaNameValue`: DB şeması taşıyan alanlarda ad `DbSchemaName` olur; ancak `PtnConceptBinding`
ve `PtnProjectionRequest` yalnız veritabanı tarafını temsil ettiği için orada da `DbSchemaName`
kullan. **Hiçbir köprü tipinde `SchemaName` adlı alan bulunmayacak** — bu bir testle doğrulanır.

### 4.4 `src/Ptn.TestModule.Domain/Interface/Bridge/`
Namespace: `Ptn.TestModule.Interface.Bridge`

| # | Dosya | Üyeler (tam imza) |
|---|---|---|
| 24 | `IApiOraclePort.cs` | `Task<PtnOperationBinding> SuggestOperationBindingAsync(PtnOperationQuery query, CancellationToken ct)` · `Task<PtnRequestExample> BuildRequestExampleAsync(...)` · `Task<PtnDerivabilityResult> ValidateAssertionsAsync(...)` · `Task<PtnConformanceResult> AssertResponseAsync(...)` |
| 25 | `IDatabaseOraclePort.cs` | `Task<PtnAssertionResult> AssertRowAsync(...)` · `AssertCountAsync` · `AssertAbsentAsync` · `AssertBatchAsync` · **`Task<PtnProjectionResult> ProjectAsync(PtnProjectionRequest request, CancellationToken ct)`** |
| 26 | `IFailureDiagnosisPort.cs` | `Task<PtnDiagnosisReport> DiagnoseApiAsync(...)` · `Task<PtnDiagnosisReport> DiagnoseDatabaseAsync(...)` |
| 27 | `ISchemaKnowledgePort.cs` | `Task<PtnTableDescription> DescribeTableAsync(...)` · `Task<PtnSchemaSnapshot> GetSnapshotAsync(...)` · `Task<string> GetSchemaFingerprintAsync(Guid connectionId, CancellationToken ct)` |
| 28 | `IProfilePackProvider.cs` | `Task<PtnProfilePack> LoadAsync(string profileKey, CancellationToken ct)` |

> **Portların döndürdüğü `Ptn*` tipleri**, listede olmayanlar dâhil, `Models/Bridge/` altında
> **aynı dosya bütçesi içinde** açılır. Bütçe dolarsa port yüzeyini **daralt** (örneğin
> `AssertBatchAsync`'i KBP-89'a bırak) — asla checker DTO'sunu doğrudan sızdırma.

> **`ProjectAsync` ön koşulu:** Database Checker'da salt-okunur projeksiyon ucu **henüz yok**
> (ADR-0019 §F). Adapter bu ucu çağırır; uç yoksa `PtnProjectionResult.StateCode =
> PtnEvidenceStateCodes.Unavailable` döner. **Kasten başarısız assertion yazarak veri okumak
> kesinlikle yasaktır.**

### 4.5 `src/Ptn.TestModule.Domain/Managers/Bridge/`
Namespace: `Ptn.TestModule.Managers.Bridge` · `DomainService` türetir (precedent'e uy)

| # | Dosya | Public yüzey ve sorumluluk |
|---|---|---|
| 29 | `ProfilePackManager.cs` | `Task<PtnProfilePack> GetValidatedAsync(string profileKey, Guid connectionId, CancellationToken ct)` — paketi yükler, boyut sınırını uygular, şema parmak izini karşılaştırır, **uyuşmazsa ilgili bağlamaları `Proposed`'a düşürür**; `PtnConceptBinding ResolveConcept(PtnProfilePack pack, string conceptCode)` — bağlanmamışsa `ConceptNotBound` fırlatır; `PtnCoverageReport BuildCoverage(PtnProfilePack pack, IReadOnlyCollection<string> required)` |
| 30 | `EvidenceChainManager.cs` | `Task<PtnChainResult> RunAsync(PtnAccessTuple tuple, string profileKey, CancellationToken ct)` — tetikleyiciye uyan yolu seçer, adımları **sırayla** yürütür, her adımda porttan olgu alır, düğüm üretir, alaka hesaplar, bütçeyi uygular, hüküm ifadesini değerlendirir, **kanıtsız düğümü düşürür** |

`EvidenceChainManager.RunAsync` gövdesi **sıralı adım** gibi okunur:
```csharp
// islevi: Tetikleyiciye uyan kanit yolunu yurutur ve aciklama agacini uretir.
public async Task<PtnChainResult> RunAsync(PtnAccessTuple tuple, string profileKey, CancellationToken ct)
{
    var pack = await ProfilePackManager.GetValidatedAsync(profileKey, tuple.ConnectionId, ct);
    var path = SelectPath(pack, tuple);
    var nodes = await WalkAsync(pack, path, tuple, ct);
    var kept = DropUnsupportedNodes(nodes);
    return BuildResult(path, kept, ProfilePackManager.BuildCoverage(pack, path.RequiredConcepts));
}
```
Her yardımcı metot `private` ve ≤ 25 satırdır; hiçbiri ayrı dosyaya taşınmaz.

**Hüküm kuralı (mekanik):**
- Yolun `ConfirmedWhen` ifadesi sağlanıyorsa `Confirmed`.
- Herhangi bir düğüm `Unavailable` ise → **`Inconclusive`** (diğer koşullara bakılmaz).
- Bağlanmamış kavram varsa → `Inconclusive` + `OpenQuestions`'a kapalı uçlu soru eklenir.
- Bütçe aşıldıysa → `Inconclusive` + `BudgetExceeded = true`.
- Kanıtı (`Evidence.Count == 0`) olan düğüm **ağaçta bırakılmaz** (ADR-0018 §D).

### 4.6 `src/Ptn.TestModule.EntityFrameworkCore/Adapters/`
Namespace: `Ptn.TestModule.Adapters`

| # | Dosya | Sorumluluk |
|---|---|---|
| 31 | `ApiOracleAdapter.cs` | `IApiOraclePort` uygular; `IResponseConformanceAppService` çağırır; outcome kodunu `PtnOutcomeCodes`'a çevirir; `ObjectReferenceDto.SchemaName` → `PtnLocation.ApiSchemaName` |
| 32 | `DatabaseOracleAdapter.cs` | `IDatabaseOraclePort` uygular; `IDatabaseAssertionAppService` çağırır; casing normalizasyonu; `ProjectAsync` yoksa `Unavailable`; değer redaksiyonu |
| 33 | `FailureDiagnosisAdapter.cs` | `IFailureDiagnosisPort` uygular; **iki** checker'ın `DiagnosisReportDto`'sunu tek köprü raporuna çevirir; `LocationDto.SchemaName` → `PtnLocation.DbSchemaName`; hipotez kodunu `PtnHypothesisCodes`'a eşler; her hipotez `PtnFindingRef` taşır |
| 34 | `SchemaKnowledgeAdapter.cs` | `ISchemaKnowledgePort` uygular; `ISchemaDiscoveryAppService` çağırır; şema parmak izini hesaplar (kanonik sıralama + SHA-256) |
| 35 | `ProfilePackFileProvider.cs` | `IProfilePackProvider` uygular. **Yalnız I/O:** ayarla verilen yoldan dosyayı okur, boyut sınırını uygular ve serializer'ı **doğrudan `PtnProfilePack` modeline** deserialize eder. Doğrulama, `ContentFingerprint` hesabı ve her kural `ProfilePackManager`'dadır |

**Adapter kuralı:** her adapter yalnız çevirir. `if (outcome == "passed") { ... karar ... }`
gibi bir satır adapter'da **bulunmaz**; eşleme tablosu `static readonly Dictionary` olur.

### 4.7 Değişecek mevcut dosyalar (manifesto dışı, ≤ 5)
- `src/Ptn.TestModule.Domain/TestModuleDomainModule.cs` — iki manager kaydı
- `src/Ptn.TestModule.EntityFrameworkCore/EntityFrameworkCore/TestModuleEntityFrameworkCoreModule.cs` — beş adapter kaydı
- `src/Ptn.TestModule.Domain/Settings/TestModuleSettings.cs` + `TestModuleSettingDefinitionProvider.cs` — `ProfilePackPath` ayarı
- `src/Ptn.TestModule.Domain.Shared/Localization/TestModule/tr.json` ve `en.json` — hata mesajları

---

## 5. Profil paketi dosya formatı

Paket **veritabanı tablosu değildir**; Git'te duran bir dosyadır (ADR-0019 §B).
`ProfilePackFileProvider` bu şekli okur:

```yaml
profileKey: acme-ticketing
revision: 1
dbSchemaFingerprint: "sha256:..."
bindings:
  - conceptCode: Subject
    dbSchemaName: identity
    tableName: users
    columnMap: { identity: id, naturalKey: email }
    patternCode: SE
    stateCode: Approved
    approvedBy: mertbyd
  - conceptCode: RoleAssignment
    dbSchemaName: identity
    tableName: user_roles
    columnMap: { subject: user_id, role: role_id }
    patternCode: SRa
    stateCode: Approved
  - conceptCode: PermissionGrant
    dbSchemaName: identity
    tableName: role_permission_grants
    columnMap: { role: role_id, permission: permission_name }
    patternCode: SRR
    stateCode: Approved
paths:
  - pathKey: access-denied-403
    trigger: { statusCodes: [401, 403] }
    steps:
      - nodeKind: ScopeRequired
        source: api.failureIdentity
      - nodeKind: SubjectResolved
        source: db.projection
        concept: Subject
      - nodeKind: RoleHeld
        source: db.projection
        concept: RoleAssignment
        joinFrom: SubjectResolved
      - nodeKind: GrantMatched
        source: db.projection
        concept: PermissionGrant
        joinFrom: RoleHeld
    confirmedWhen: "ScopeRequired.observed && !GrantMatched.containsAny(ScopeRequired.values)"
    inconclusiveWhen: "any(step.state == Unavailable)"
```

`confirmedWhen` / `inconclusiveWhen` **serbest kod değildir**: yalnız yukarıdaki üç yapı
desteklenir (`X.observed`, `X.containsAny(Y.values)`, `any(step.state == Z)`) ve
`EvidenceChainManager` içinde kapalı bir değerlendiriciyle çözülür. Yeni yapı gerekirse
görevi durdur ve sor — **ifade dili genişletme.**

Depoya örnek paket olarak `ptn-test-module/samples/profiles/acme-ticketing.yaml` eklenir
(bu dosya 35'lik bütçenin dışındadır, veri dosyasıdır).

---

## 6. Yazılacak testler

`test/Ptn.TestModule.Domain.Tests/Bridge/` altında:

| Test | Doğruladığı |
|---|---|
| `BridgeVocabularyTests` | Köprü tiplerinde `SchemaName` adlı **hiçbir** property yok (reflection ile tarama) |
| `VocabularyDriftTests` | `ConformanceOutcomeCodes.All`, `AssertionDerivabilityCodes.All`, `HypothesisKindCodes.All`, `DiagnosisConfidenceCodes.All` üye kümeleri köprünün beklediği kümeyle **birebir** eşleşiyor; checker yeni kod eklerse test kırmızı |
| `EvidenceChainManagerTests` | 403 sinyali + örnek paket → dört düğüm + `Confirmed` |
| `EvidenceChainManagerTests` | Projeksiyon `Unavailable` → sonuç `Inconclusive`, **`RuledOut` değil** |
| `EvidenceChainManagerTests` | Kanıtsız düğüm ağaçta kalmıyor |
| `EvidenceChainManagerTests` | Atlama bütçesi aşımı → `Inconclusive` + `BudgetExceeded` |
| `ProfilePackManagerTests` | Şema parmak izi uyuşmazlığı → bağlamalar `Proposed` |
| `ProfilePackManagerTests` | Bağlanmamış kavram → `ConceptNotBound` + kapsam raporunda `UnboundConcepts` |

Testler `ptn-test-module` içindeki mevcut test tabanlarını kullanır; yeni test projesi açılmaz.

---

## 7. Bitiş

1. Dosya sayısını doğrula (≤ 35 + ≤ 5 mevcut dosya değişikliği + örnek yaml).
2. Bölüm 6'daki testlerin hepsi yazılmış olmalı.
3. Tek commit at:
   ```
   #KBP-88 feat: created the checker bridge vocabulary, oracle ports and evidence path engine
   ```
4. **Build/test çalıştırma.** Doğrulama KBP-89 bittikten sonra tek sefer yapılacak.
5. Raporunda şunları yaz: yazılan dosya listesi, bütçe nedeniyle KBP-89'a devredilen maddeler,
   ve karşılaşılan her belirsizlik (varsayım yaptıysan **hangi varsayımı yaptığını açıkça yaz**).
