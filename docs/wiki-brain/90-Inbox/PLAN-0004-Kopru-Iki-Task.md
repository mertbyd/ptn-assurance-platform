---
id: PLAN-0004
type: plan
status: draft
title: Kopru katmani — iki task (KBP-88, KBP-89)
updated: 2026-08-14
decision_refs:
  - ADR-0007
  - ADR-0008
  - ADR-0014
  - ADR-0015
  - ADR-0016
  - ADR-0017
  - ADR-0018
  - ADR-0019
rule_refs:
  - RULE-0002
  - RULE-0005
  - RULE-0006
  - RULE-0007
  - RULE-0008
---

# PLAN-0004 — Köprü katmanı: iki task

Bu belge **yalnız köprü işini** kapsar. Modülün kalan 10 task'ı bu iki task bittikten sonra
yazılır. Kapsam kaynağı: ADR-0018 + **ADR-0019** + RESEARCH-0015/0016.

## Ortak kurallar (iki task için de bağlayıcı)

| Konu | Kural |
|---|---|
| **Branch** | Task 1 → `KBP-88` · Task 2 → `KBP-89`. Her task **kendi branch'inde**; branch mevcut template branch'inden açılır. |
| **Commit** | `#KBP-88 feat: created ...` / `#KBP-89 feat: created ...` — **past tense**, `created` (asla `added`). Bir task = bir commit. |
| **Dosya bütçesi** | Branch başına **≤ 35 dosya**. Aşarsa: listedeki **son** maddeler bir sonraki task'a kayar, iş bölünmez. |
| **Katman zinciri** | `Controller → AppService → Manager → Repository`. **ABP'de olmayan katman açılmaz.** Manager'da yapılacak iş için ayrı sınıf/klasör açılmaz. |
| **Dosya düzeni** | Ev standardı: `Entities/`, `Managers/`, `Interface/`, `Models/`, `Dtos/`, `Services/`, `FluentValidation/`, `Permissions/`, `Mappers/`, `Configurations/`, `Repository/`, `Controllers/`, `Constants/`, `ExceptionCodes/`, `Localization/`. Alt klasör **konu adıyla** (`Bridge/`). |
| **Yorum** | Her tip: `// islevi:` + `// sistemdeki gorevi:` çifti. Her authored metot: tek satır niyet yorumu. Stajyer okuyup anlayabilmeli. |
| **Sabitler** | Her anlamlı string (kod, route, swagger grubu, hata kodu, ayar anahtarı) `Domain.Shared` sahipli. |
| **Build/test** | **Ara adımda çalıştırılmaz.** Tüm iş bittiğinde tek sefer (`dotnet build` + `dotnet test` + skill incelemesi). |
| **Veri modeli** | ADR-0016 korunur: **yeni tablo açılmaz**. Profil paketi dosyadır. |
| **Motor** | **PostgreSQL.** Diğer motorlar `Unavailable` döner. |

> **Not — bilinçli sapma:** şirket sözleşmesi faz başına build/test gate'i ister. Bu iş için
> gate **tek sefere** alındı (kullanıcı kararı). Risk: KBP-88'deki bir derleme hatası
> KBP-89'in üzerine biner. Karşı önlem: KBP-88 **tek başına derlenebilir** olacak şekilde
> tasarlandı (hiçbir tipi KBP-89'e bağımlı değil).

---

# BÖLÜM A — YouTrack task metinleri

Bu bölüm doğrudan YouTrack'e yapıştırılmak içindir.

---

## 🌉 T1 — Köprü Sözlüğü, Portlar ve Kanıt Yolu Motoru

**Branch:** `KBP-88`

**Amaç:** İki checker'ı tek ajan sözlüğünde birleştiren deterministik köprü çekirdeğini kurmak;
teşhis ve yazarlık zincirlerini **elle kodlanmış akış olmadan**, veriden okunan kanıt yollarıyla
yürüten motoru yazmak.

**Yapılacaklar:**

- Tek ajan sözlüğünü `Domain.Shared` altında sahiplen: outcome, hipotez, kavram, düğüm türü,
  kanıt durumu, alaka, hüküm, kaynak checker, bağlama durumu kodları. İki checker'daki casing
  ve gramer çakışmalarını (`Passed`/`passed`, `Match`/`Matches`, `H-CD-01`/`RowNeverCreated`)
  köprüde **tek forma** eşle.
- `SchemaName` **ad çakışmasını yasakla**: köprü konum modelinde `apiSchemaName`,
  `dbSchemaName`, `dbTableName` ayrı alanlar olsun; `SchemaName` adında alan bulunmasın.
- Fingerprint'leri **birleştirme**; her zaman `{sourceChecker, fingerprint}` çifti olarak taşı.
- Dört checker portunu ve bir profil paketi sağlayıcısını tanımla; adapter'ları yaz. Adapter
  üç iş yapar: DTO çevirisi, **tek sözlüğe normalizasyon**, hata çevirisi.
- Profil paketini (kavram → tablo/kolon bağlaması + kanıt yolu tanımları + revision +
  şema parmak izi) dosyadan yükle, doğrula, kapsam oranını hesapla. Şema parmak izi
  tutmuyorsa ilgili bağlamaları `Proposed`'a düşür.
- Kanıt yolu motorunu yaz: tetikleyiciye uyan yolu bul, adımları sırayla yürüt, her adımda
  bir checker'dan **olgu** al, açıklama ağacı düğümü üret, hüküm ifadesini değerlendir.
- Kanıt durumunu **üç değerli** tut: `Observed` / `NotObserved` / **`Unavailable`**. Bağlanmamış
  kavram `NOT_BOUND`, tamamlanamayan zincir `Inconclusive` döner.
- Atlama ve kanıt bütçesi uygula; aşımda `Inconclusive`.
- Sözlük drift testi: checker kod sabitlerinin üye kümesi köprünün beklediği kümeyle
  karşılaştırılır; checker yeni kod eklerse test **kırmızı** olur.

**Teknik Notlar:**

- Adapter'lar `EntityFrameworkCore/Adapters/` altında yaşar; **yeni proje veya katman açılmaz**
  (ADR-0015 §F).
- Profil paketi **tablo değildir** — Git'te duran dosyadır, koşuda yalnız parmak izi kaydedilir
  (ADR-0016 modeli korunur).
- Kanıt yolu **veridir**; yeni teşhis sınıfı eklemek yeni `if` değil, yeni tanım girdisidir.
- Açıklama ağacı zincirin **yan ürünüdür**; ikinci bir akıl yürütme kurulmaz.
- Bu task'ta **model çağrısı yoktur** ve olmayacaktır.
- `DescribeTableAsync` maliyetlidir; yalnız yazarlık anında ve bütçeyle çağrılır.

**Çıktılar:** Tek ajan sözlüğü · dört port + adapter · profil paketi yükleyici + kapsam ölçümü ·
veri güdümlü kanıt yolu motoru · açıklama ağacı modeli · sözlük drift testi.

**Kabul Kriterleri:**

- Elle yazılmış bir profil paketi ve `access-denied-403` kanıt yolu ile: 403 sinyali verildiğinde
  motor `ScopeRequired → SubjectResolved → RoleHeld → GrantMatched` düğümlerini üretiyor ve
  `Confirmed` hükmü veriyor.
- İlgili tablo okunamadığında sonuç *"rol yok"* değil, **`Unavailable` + `Inconclusive`**.
- Profil paketinde bağlanmamış kavram varsa zincir `NOT_BOUND` döndürüyor ve kapsam raporu
  `bound/required` oranını taşıyor.
- Şema parmak izi değiştiğinde ilgili bağlamalar `Proposed` durumuna düşüyor.
- Köprü sözlüğünde `SchemaName` adında alan **yok**; testle doğrulanıyor.
- Checker kod sabitlerinden birine yeni üye eklendiğinde drift testi kırmızıya dönüyor.
- Kanıtı olmayan hiçbir düğüm sonuç ağacında kalmıyor.

---

## 🧭 T2 — Köprü Ajan Yüzeyi, Tool Bütçesi ve Yetenek Seviyesi

**Branch:** `KBP-89`

**Amaç:** T1'in deterministik çekirdeğini ajana **dar, tipli ve bütçeli** bir yüzeyden açmak;
*"bu operasyon DB'de neyi değiştiriyor"* sorusunu ortamın yeteneğini **yoklayarak** cevaplamak.

**Yapılacaklar:**

- Tool kataloğunu kur: aktif tool sayısı **≤ 7**. Bu task'ta `ptn_ground`, `ptn_explain`,
  `ptn_validate`, `ptn_knowledge` açılır; `ptn_run`, `ptn_result`, `ptn_impact` katalogda
  **yer tutar** ve koşum task'larında bağlanır.
- `ptn_ground` **tek çağrıdır**: operasyon bağı + istek örneği + tablo tanımı + (varsa) etki
  ayak izi tek sonuçta döner. Ara sonuçlar ajanın bağlamına **girmez**.
- Yanıt şekillendirmesi zorunlu: `responseFormat: concise | detailed`; ağır gövde
  `resource_link` ile verilir; hata mesajı **öğreten** biçimde yazılır.
- Tool şemaları **talep üzerine** açılır (toolset + dinamik keşif); hepsi aynı anda bağlamda
  durmaz.
- **Kademe 4 eylemi (yayınlama, yama uygulama) Tool olarak kaydedilmez.** Katalog testi bunu
  doğrular.
- Ajana dönen her alan **kapalı seçim** olur: operasyon, tablo, kolon, kod, scope alanları
  serbest metin değil, referans/enum'dur. Serbest metin alan taraması testle yapılır.
- Eşik altı operasyon adayları **listelenmez**; kapalı uçlu soru döner.
- Etki ayak izi yetenek çözümleyicisini yaz: `wal_level`, replication yetkisi ve sandbox
  tekilliği yoklanır; sonuç `Exact` / `RowAddressed` / `Inferred` / `Unavailable` seviyesi
  olarak döner. Dört seviye de **aynı sözleşmeyi** döndürür, yalnız `strengthCode` farklıdır.
- Replication slot **geçici** açılır ve koşum sonunda **garantili** düşürülür; düşürülemezse
  koşum `Broken` işaretlenir.
- Ayak izi **oracle değildir**: `Exact` dahil tüm seviyeler **öneri**dir; onaysız assertion
  üretimine giremez.
- Kapsam raporunu teşhis ve zemin yanıtlarının **başına** koy (özet-önce).
- İzinleri tanımla; okuma uçları kademe 1-2, ayak izi keşfi kademe 3'tür.

**Teknik Notlar:**

- Yüzey `Controller → AppService → Manager` zincirini kullanır; tek `IPtnBridgeAppService`
  altında dört metot (checker'daki `IResponseConformanceAppService` deseni).
- Ayak izi keşfi **tekil sandbox** ister; sıraya alma garantisi yoksa `Unavailable` döner.
- **PostgreSQL** hedeflenir; başka motorda `Unavailable`.
- `Ptn.TestModule` checker AppService'lerini doğrudan çağırmaz; T1'in portlarını çağırır.
- Model çağrısı bu task'ta da **yoktur**; yüzey ajan tarafından tüketilir, ajanı barındırmaz.

**Çıktılar:** ≤7 tool'lu ajan yüzeyi · `ptn_ground` tek çağrı zemini · açıklama ağacı raporu ·
yetenek seviyeli etki ayak izi · kapsam raporu · tool bütçesi ve kademe-4 testleri.

**Kabul Kriterleri:**

- Kayıtlı aktif tool sayısı ≤ 7; fazlası toolset arkasında ve testle doğrulanıyor.
- Katalogda kademe 4 eylemi **yok**; testle doğrulanıyor.
- Tool giriş şemalarında operasyon/tablo/kolon/kod alanı **serbest metin değil**; tarama testi
  geçiyor.
- `responseFormat: concise` yanıtı `detailed`'a göre belirgin biçimde küçük ve kritik olgu
  yanıtın **başında**.
- `wal_level = logical` olmayan ortamda ayak izi `Inferred` veya `Unavailable` döner; **hata
  fırlatmaz**.
- Ayak izi sonucu her zaman `strengthCode` taşır ve onaysız assertion üretimine giremez
  (yayın kapısı kontrolü).
- Paylaşımlı ortamda ayak izi keşfi çalışmaz, `Unavailable` döner.
- Replication slot koşum sonunda kalmıyor; bırakılan slot testte yakalanıyor.

---

# BÖLÜM B — Ajan task metinleri

Bu bölüm işi yapacak ajana verilir. Bölüm A'nın kapsamını **değiştirmez**, uygulama
sözleşmesini ekler.

---

## AJAN TASK 1 — `KBP-88`

### 0. Bağlam ve zorunlu okuma

```
Depo   : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül  : ptn-test-module
Branch : KBP-88   (mevcut template branch'inden açılır)
Commit : #KBP-88 feat: created the checker bridge vocabulary, oracle ports and evidence path engine
```

Kod yazmadan önce **sırayla** oku ve uygula:

1. `/abp-backend-dev` skill'i (zorunlu — C# + ABP işi)
2. `ptn-test-module/AGENTS.md` (modül değişmezleri)
3. `docs/wiki-brain/04-Architecture/Alti-An.md` (ürünün akışı)
4. `docs/wiki-brain/03-Decisions/ADR-0018-*.md` ve **`ADR-0019-*.md`** (bu task'ın kaynağı)
5. `docs/wiki-brain/02-Rules/RULE-0005 / 0006 / 0007 / 0008`
6. **Precedent kod:** `checkers/api-contract/src/.../Services/Conformance/IResponseConformanceAppService.cs`,
   `checkers/database-comparison/src/.../Services/SchemaDiscovery/ISchemaDiscoveryAppService.cs`,
   `checkers/*/src/*.Domain.Shared/Constants/**/*Codes.cs` (sabit dosyası biçimi),
   `checkers/api-contract/src/*.Application/Services/Sources/SpecSourceAppService.cs` (servis biçimi)

### 1. Dosya manifestosu (≤35)

> Sıra bağlayıcıdır. 35'i aşarsan **listenin sonundan** kes ve KBP-89'e taşı; hiçbir dosyayı
> yarım bırakma.

**`src/Ptn.TestModule.Domain.Shared/Constants/Bridge/`**

| # | Dosya | İçerik |
|---|---|---|
| 1 | `PtnBridgeConsts.cs` | Bütçe sınırları: `MaxHopCount`, `MaxEvidencePerNode`, `MaxNodeCount`, `MaxProfilePackBytes`, `MaxProjectionRows` |
| 2 | `Lookups/PtnConceptCodes.cs` | `Subject` `RoleAssignment` `PermissionGrant` `Resource` `ResourceOwnership` `TimeAnchor` `Quota` + `All` |
| 3 | `Lookups/PtnNodeKindCodes.cs` | `ScopeRequired` `SubjectResolved` `RoleHeld` `GrantMatched` `OperationBound` `RequestExampleBuilt` `TableDescribed` `KeyUnique` `AssertionDerivable` `FootprintObserved` |
| 4 | `Lookups/PtnEvidenceStateCodes.cs` | `Observed` `NotObserved` `Unavailable` |
| 5 | `Lookups/PtnRelevanceCodes.cs` | `High` `Normal` |
| 6 | `Lookups/PtnVerdictCodes.cs` | `Confirmed` `Likely` `Possible` `RuledOut` `Inconclusive` |
| 7 | `Lookups/PtnOutcomeCodes.cs` | Tek casing; iki checker'ın outcome kodlarının köprü formu |
| 8 | `Lookups/PtnHypothesisCodes.cs` | Tek gramer; `H-CD-01` ve `RowNeverCreated` gramerlerinin köprü formu |
| 9 | `Lookups/PtnSourceCheckerCodes.cs` | `ApiContract` `DatabaseComparison` `Runner` `Bridge` |
| 10 | `Lookups/PtnBindingStateCodes.cs` | `Proposed` `Approved` `Rejected` |

**`src/Ptn.TestModule.Domain.Shared/ExceptionCodes/Bridge/`**

| # | Dosya | İçerik |
|---|---|---|
| 11 | `TestModuleBridgeErrorCodes.cs` | `ProfilePackNotFound` `ProfilePackInvalid` `ProfileFingerprintMismatch` `ConceptNotBound` `EvidencePathNotFound` `HopBudgetExceeded` `EvidenceUnavailable` |

**`src/Ptn.TestModule.Domain/Models/Bridge/`** — *hepsi salt-veri modeli; davranış yok*

| # | Dosya | İçerik |
|---|---|---|
| 12 | `PtnAccessTuple.cs` | `Subject`, `Operation`, `RequiredPermissions[]`, `Context` |
| 13 | `PtnLocation.cs` | `ApiSchemaName`, `DbSchemaName`, `DbTableName`, `ColumnName`, `OperationId`, `JsonPointer` — **`SchemaName` adı yasak** |
| 14 | `PtnFindingRef.cs` | `SourceCheckerCode`, `Fingerprint` — çıplak fingerprint dışarı verilmez |
| 15 | `PtnEvidence.cs` | `ProbeKindCode`, `FactCode`, `ExpectedValue`, `ObservedValue`, `ObservedAtMs`, `Ref` (`PtnFindingRef`) |
| 16 | `PtnExplanationNode.cs` | `NodeKindCode`, `StateCode`, `RelevanceCode`, `Location`, `Evidence[]`, `Children[]` |
| 17 | `PtnChainResult.cs` | `PathKey`, `VerdictCode`, `Root` (`PtnExplanationNode`), `Coverage`, `HopCount`, `BudgetExceeded` |
| 18 | `PtnConceptBinding.cs` | `ConceptCode`, `SchemaName`, `TableName`, `ColumnMap`, `PatternCode`, `StateCode`, `ApprovedBy` |
| 19 | `PtnEvidencePathDefinition.cs` | `PathKey`, `Trigger`, `Steps[]` (**nested** `PtnEvidencePathStep`), `ConfirmedWhen`, `InconclusiveWhen` |
| 20 | `PtnProfilePack.cs` | `ProfileKey`, `Revision`, `DbSchemaFingerprint`, `SpecSnapshotId`, `Bindings[]`, `Paths[]`, `Fingerprint` |
| 21 | `PtnCoverageReport.cs` | `RequiredConcepts[]`, `BoundConcepts[]`, `BoundRatio`, `UnboundConcepts[]` |
| 22 | `PtnProjectionRequest.cs` | `SchemaName`, `TableName`, `KeyValues`, `ProjectColumns[]`, `MaxRows` — **serbest SQL yok** |
| 23 | `PtnProjectionResult.cs` | `StateCode`, `Rows[]` (redaksiyonlu), `ObservedRowCount`, `Truncated` |

**`src/Ptn.TestModule.Domain/Interface/Bridge/`**

| # | Dosya | Üyeler |
|---|---|---|
| 24 | `IApiOraclePort.cs` | `SuggestOperationBindingsAsync`, `BuildRequestExampleAsync`, `ValidateScenarioAssertionsAsync`, `AssertResponseAsync` |
| 25 | `IDatabaseOraclePort.cs` | `AssertRowAsync`, `AssertCountAsync`, `AssertAbsentAsync`, `AssertBatchAsync`, **`ProjectAsync`** |
| 26 | `IFailureDiagnosisPort.cs` | `DiagnoseApiAsync`, `DiagnoseDatabaseAsync` — **ikisi tek raporda birleşir** |
| 27 | `ISchemaKnowledgePort.cs` | `DescribeTableAsync`, `GetSnapshotAsync`, `GetSchemaFingerprintAsync` |
| 28 | `IProfilePackProvider.cs` | `LoadAsync(profileKey)`, `GetFingerprintAsync` |

> **`ProjectAsync` ön koşulu:** Database Checker'da salt-okunur projeksiyon ucu **henüz yok**
> (ADR-0019 §F). Adapter bu ucu çağırır; uç yoksa `PtnProjectionResult.StateCode =
> Unavailable` döner. **Kasten başarısız assertion yazarak veri okuma girişimi yasaktır.**

**`src/Ptn.TestModule.Domain/Managers/Bridge/`**

| # | Dosya | Sorumluluk |
|---|---|---|
| 29 | `ProfilePackManager.cs` | Paketi yükle · şema parmak izini karşılaştır · uyuşmazsa bağlamayı `Proposed`'a düşür · kavram çöz · kapsam raporu üret · bağlanmamış kavramda `ConceptNotBound` |
| 30 | `EvidenceChainManager.cs` | Tetikleyiciye uyan yolu seç · adımları sırayla yürüt · her adımda porttan olgu al · düğüm üret · alaka hesapla · bütçeyi uygula · hüküm ifadesini değerlendir · **kanıtsız düğümü düşür** |

**`src/Ptn.TestModule.EntityFrameworkCore/Adapters/`**

| # | Dosya | Sorumluluk |
|---|---|---|
| 31 | `ApiOracleAdapter.cs` | API Checker DTO ↔ köprü modeli · **tek sözlüğe normalizasyon** · hata çevirisi |
| 32 | `DatabaseOracleAdapter.cs` | DB Checker DTO ↔ köprü modeli · casing normalizasyonu · redaksiyon |
| 33 | `FailureDiagnosisAdapter.cs` | İki `DiagnosisReportDto`'yu tek köprü raporuna · `SchemaName` çakışmasını `PtnLocation`'a ayır |
| 34 | `SchemaKnowledgeAdapter.cs` | `DescribeTable`/`GetSnapshot` çevirisi · şema parmak izi hesabı |
| 35 | `ProfilePackFileProvider.cs` | Paketi dosyadan oku · şema doğrula · boyut sınırı · içerik hash'i |

**Değişecek mevcut dosyalar (manifesto dışı, ≤5):**
`TestModuleDomainModule.cs` (manager kaydı) · `TestModuleEntityFrameworkCoreModule.cs`
(adapter kaydı) · `TestModuleSettings.cs` + `TestModuleSettingDefinitionProvider.cs`
(profil paketi yolu ayarı) · `Localization/*.json` (hata mesajları).

### 2. Yazım kuralları

- **Manager'lar iş sahibidir.** Doğrulama, normalizasyon, bütçe, hüküm — hepsi manager'da.
  Bunlar için ayrı `Services/`, `Helpers/`, `Handlers/`, `Engines/` klasörü **açılmaz**.
- **Modeller veri kabuğudur.** Metot, `if`, hesap yok.
- **Adapter'lar akıl yürütmez.** Yalnız çeviri + normalizasyon + hata çevirisi.
- Metot sınırı: **25 satır**, **2 iç içe kontrol seviyesi**. Public metotlar sıralı adım gibi
  okunur.
- Üçten fazla ilişkili dönüş değeri → `Models/Bridge/` altında **adlandırılmış model**
  (tuple yasak).
- Motor/sağlayıcı farkı `if/switch` ile değil, mevcut resolver deseniyle çözülür.
- Her public giriş modeli için doğrulama; bu task'ta DTO yok, doğrulama **manager**'dadır.
- Yorum çifti zorunlu: `// islevi:` + `// sistemdeki gorevi:`.

### 3. Yasaklar

- Yeni proje, yeni katman, `Infrastructure/`, `EventHandlers/`, `Factories/` **açma**.
- Checker AppService'ini doğrudan çağırma — **yalnız port**.
- Checker tablosunu okuma, FK verme, ortak transaction açma.
- Yeni tablo, yeni migration, yeni entity **üretme** (bu task'ta entity yok).
- Model/LLM çağrısı **ekleme**.
- Serbest SQL taşıyan hiçbir sözleşme yazma.
- Ara adımda `dotnet build` / `dotnet test` / `restore` **çalıştırma**.

### 4. Bitiş tanımı

- 35 dosya manifestosu tamamlandı (veya taşan maddeler açıkça KBP-89'e devredildi).
- Kabul kriterleri (Bölüm A / T1) karşılandı.
- Commit atıldı: `#KBP-88 feat: created the checker bridge vocabulary, oracle ports and evidence path engine`
- **Build/test çalıştırılmadı** — KBP-89 bitiminde tek sefer.

---

## AJAN TASK 2 — `KBP-89`

### 0. Bağlam

```
Branch : KBP-89   (KBP-88 üzerine)
Commit : #KBP-89 feat: created the bridge agent surface with tool budget, explanation report and capability levels
```

Zorunlu okuma: Task 1'in listesi + `RULE-0007` (tool bütçesi) + `ADR-0019 §E/§G`.

### 1. Dosya manifestosu (≤35)

**`src/Ptn.TestModule.Domain.Shared/Constants/Bridge/`**

| # | Dosya | İçerik |
|---|---|---|
| 1 | `PtnToolCodes.cs` | `ptn_ground` `ptn_validate` `ptn_run` `ptn_result` `ptn_explain` `ptn_knowledge` `ptn_impact` + `ActiveMax = 7` |
| 2 | `PtnResponseFormatCodes.cs` | `concise` `detailed` |
| 3 | `Lookups/PtnFootprintStrengthCodes.cs` | `Exact` `RowAddressed` `Inferred` `Unavailable` |
| 4 | `PtnBridgeRoutes.cs` | Route şablonları + swagger grup adı |

**`src/Ptn.TestModule.Domain/Models/Bridge/`**

| # | Dosya | İçerik |
|---|---|---|
| 5 | `PtnFootprintResult.cs` | `StrengthCode`, `Tables[]`, `Columns[]`, `RowDeltas[]`, `IsAdvisoryOnly = true` |
| 6 | `PtnCapabilityLevel.cs` | `FootprintStrengthCode`, `HasLogicalDecoding`, `HasExclusiveSandbox`, `HasProjectionSurface`, `Reasons[]` |
| 7 | `PtnGroundingResult.cs` | `OperationBinding`, `RequestExample`, `TableDescription`, `Footprint`, `Coverage`, `Questions[]` |

**`src/Ptn.TestModule.Domain/Interface/Bridge/`**

| # | Dosya | Üyeler |
|---|---|---|
| 8 | `IWriteSetCapabilityPort.cs` | `ProbeCapabilityAsync`, `CaptureWriteSetAsync`, `ReleaseAsync` |

**`src/Ptn.TestModule.Domain/Managers/Bridge/`**

| # | Dosya | Sorumluluk |
|---|---|---|
| 9 | `FootprintCapabilityManager.cs` | Yeteneği yokla · strateji seç · slot yaşam döngüsü (geçici slot, **garantili düşürme**) · seviye döndür · paylaşımlı ortamda `Unavailable` |
| 10 | `ToolCatalogManager.cs` | Aktif tool ≤ 7 · toolset gruplaması · dinamik keşif · **kademe 4 eylemi katalogdan hariç** |
| 11 | `GroundingManager.cs` | `ptn_ground`'un tek çağrı zeminini kurar: bağ + örnek + tablo + ayak izi; eşik altı adayda **liste değil soru** üretir |

**`src/Ptn.TestModule.EntityFrameworkCore/Adapters/`**

| # | Dosya | Sorumluluk |
|---|---|---|
| 12 | `WriteSetCapabilityAdapter.cs` | PostgreSQL yetenek yoklaması (`wal_level`, replication yetkisi) + slot yönetimi; başka motorda `Unavailable` |

**`src/Ptn.TestModule.Application.Contracts/Dtos/Bridge/`**

| # | Dosya |
|---|---|
| 13 | `PtnGroundRequestDto.cs` |
| 14 | `PtnGroundResultDto.cs` |
| 15 | `PtnExplainRequestDto.cs` |
| 16 | `PtnExplainResultDto.cs` |
| 17 | `PtnExplanationNodeDto.cs` |
| 18 | `PtnEvidenceDto.cs` |
| 19 | `PtnValidateRequestDto.cs` |
| 20 | `PtnValidateResultDto.cs` |
| 21 | `PtnKnowledgeRequestDto.cs` |
| 22 | `PtnKnowledgeResultDto.cs` |
| 23 | `PtnCoverageReportDto.cs` |
| 24 | `PtnToolCatalogDto.cs` |

> Her giriş DTO'sunda operasyon/tablo/kolon/kod alanı **referans veya kapalı küme**;
> serbest metin alan **yok**. Her yanıt DTO'sunda `ResponseFormat` alanı bulunur ve
> ağır gövde `ResourceLink` ile verilir.

**`src/Ptn.TestModule.Application.Contracts/Services/Bridge/`**

| # | Dosya | Üyeler |
|---|---|---|
| 25 | `IPtnBridgeAppService.cs` | `GroundAsync`, `ExplainAsync`, `ValidateAsync`, `GetKnowledgeAsync`, `GetToolCatalogAsync` |

**`src/Ptn.TestModule.Application.Contracts/FluentValidation/Bridge/`**

| # | Dosya |
|---|---|
| 26 | `PtnGroundRequestDtoValidator.cs` |
| 27 | `PtnExplainRequestDtoValidator.cs` |
| 28 | `PtnValidateRequestDtoValidator.cs` |
| 29 | `PtnKnowledgeRequestDtoValidator.cs` |

**`src/Ptn.TestModule.Application/`**

| # | Dosya | Not |
|---|---|---|
| 30 | `Services/Bridge/PtnBridgeAppService.cs` | Düz orkestrasyon: doğrula → manager → eşle. **İş kuralı yok.** |
| 31 | `Mappers/Bridge/PtnBridgeMapper.cs` | Mapperly; audit-ignore dökümü **yok** |
| 32 | `Mappers/Bridge/PtnExplanationMapper.cs` | Ağaç → DTO ağacı |

**`src/Ptn.TestModule.HttpApi/Controllers/Bridge/`**

| # | Dosya | Not |
|---|---|---|
| 33 | `PtnBridgeController.cs` | Transport sarmalayıcı: route, yetki, status; **tek AppService çağrısı** |

**Değişecek mevcut dosyalar (≤5):** `TestModulePermissions.cs` ·
`TestModulePermissionDefinitionProvider.cs` · `TestModuleDomainModule.cs` ·
`TestModuleEntityFrameworkCoreModule.cs` · host MCP kaydı.

### 2. Yazım kuralları

Task 1'in kuralları aynen geçerli, ek olarak:

- **Controller** yalnız route/binding/yetki/status + tek AppService çağrısı taşır. İş dalı,
  manager erişimi, eşleme, exception politikası **yok**.
- **AppService** düz orkestrasyondur: `validator → manager → mapper`. Karar vermez.
- **Manager** karar verir: bütçe, eşik, yetenek seviyesi, soru üretimi.
- Mapperly **target-strict** varsayılanıyla çalışır; ABP audit alanı ignore listesi **yazılmaz**.
- Her DTO için FluentValidation; shape kuralları mevcut tipli validator tabanından gelir.

### 3. Yasaklar

Task 1'in tüm yasakları + :

- Kademe 4 eylemini (yayınlama, yama uygulama, karantina kaldırma) **Tool olarak kaydetme**.
- Eşik altı aday listesini ajana **dökme**.
- Ayak izini assertion üretiminde **onaysız kullanma**.
- Slot açık bırakma; `finally` ile garantili düşürme **zorunlu**.
- Ara adımda build/test/restore.

### 4. Bitiş tanımı

- Manifesto tamamlandı; kabul kriterleri (Bölüm A / T2) karşılandı.
- Commit: `#KBP-89 feat: created the bridge agent surface with tool budget, explanation report and capability levels`
- **Ancak bundan sonra**, tek sefer: `dotnet build Ptn.TestModule.slnx` →
  `dotnet test Ptn.TestModule.slnx` → `/abp-backend-dev` mimari incelemesi →
  `/backend-verify` gate'i.

---

# BÖLÜM C — Wiki kural kapsama matrisi

Kullanıcı şartı: *"wikideki her kural kapanacak, her araştırma uygulanacak."*

| Kaynak | Madde | Nerede karşılanıyor |
|---|---|---|
| ADR-0018 §A | Tek ajan sözlüğü, dört çakışmanın çözümü | T1 · dosya 2-10, 31-34 |
| ADR-0018 §A | Fingerprint birleştirilmez | T1 · `PtnFindingRef` |
| ADR-0018 §B | Aktif tool ≤ 7, toolset, `responseFormat` | T2 · `ToolCatalogManager`, DTO'lar |
| ADR-0018 §B | `ptn_ground` tek çağrı | T2 · `GroundingManager` |
| ADR-0018 §C | Kanıt zinciri süpervizörü | T1 · `EvidenceChainManager` |
| ADR-0018 §D | Alıntısız hipotez düşürülür | T1 · kanıtsız düğüm düşürme |
| ADR-0018 §E | Sözlük drift'i derlemede kırılır | T1 · drift testi |
| ADR-0018 §F | Ayak izi öneri, oracle değil | T2 · `IsAdvisoryOnly` + yayın kapısı |
| ADR-0018 §G | Spec boşluğu raporlanır | T2 · `ValidateAsync` kırmızı kart |
| **ADR-0019 §A** | Kanıt yolu **veridir** | T1 · `PtnEvidencePathDefinition` |
| **ADR-0019 §B** | Profil manifesti, `Proposed/Approved`, parmak izi mührü | T1 · `ProfilePackManager` |
| **ADR-0019 §C** | `NOT_BOUND` / `Unavailable` / `Inconclusive` birinci sınıf | T1 · üç değerli durum |
| **ADR-0019 §D** | Açıklama = zincirin yan ürünü | T1 · `PtnExplanationNode` |
| **ADR-0019 §E** | Dört seviyeli ayak izi + slot güvenliği | T2 · `FootprintCapabilityManager` |
| **ADR-0019 §F** | Projeksiyon yüzeyi ön koşulu | T1 · `ProjectAsync` + `Unavailable` düşüşü |
| **ADR-0019 §G** | Progressive disclosure | T2 · toolset dinamik keşfi |
| **ADR-0019 §H** | Graf motoru kurulmaz | T1 · ardışık probe |
| ADR-0015 §F | Port + adapter, yeni katman yok | T1 · `Interface/Bridge` + `Adapters/` |
| ADR-0016 | Yeni tablo yok | T1 · profil paketi **dosya** |
| RULE-0005 | Koşum/yargıda model yok, kademe 4 otomatikleşmez | T1+T2 · model çağrısı yok, katalog testi |
| RULE-0006 | Türetilemeyen assertion yayınlanamaz | T2 · `ValidateAsync` |
| RULE-0007 | Tahmin yok, ≤7 tool, açık uçlu alan yok, kademe 4 Tool değil | T2 · şema taraması + katalog testi |
| RULE-0008 | Çift yönlü kural kapsamı | **Kapsam dışı** — numarası henüz atanmamış DMN yazarlık task'ında kapanır; köprü yalnız kapsam raporu altyapısını verir |
| RULE-0002 | Şema/migration sahipliği | Bu iki task'ta migration üretilmez |

> **Açık kalan tek madde RULE-0008'dir** ve bilinçlidir: karar tablosu derleyicisi köprü işi
> değil, yazarlık hattı işidir (ADR-0017). Köprü ona `Coverage` modelini hazır verir.

---

# BÖLÜM D — İş bittikten sonra

1. `dotnet build Ptn.TestModule.slnx` — tek sefer
2. `dotnet test Ptn.TestModule.slnx` — tek sefer
3. `/abp-backend-dev` ile **mimari inceleme**: katman zinciri, dosya düzeni, manager sahipliği,
   yorum çifti, metot sınırları, Mapperly profili, Domain.Shared sabit sahipliği
4. `/backend-verify` gate'i: diff taraması, commit grameri, güvenlik ve veri sınırı
5. Bulgular düzeltilir; **düzeltme ayrı commit değil**, aynı branch'te amend/ek commit
   (şirket kuralına göre)
