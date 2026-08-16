# AJAN GÖREVİ — KBP-116 · Backend kapanışı: yetki, mühür ve paket erişimi

> [!INFO] Numara kaydı
> `KBP-113` ve `KBP-114` **kullanılmayacaktır** — kapsamları TASK-KBP-112'nin Dilim 3 ve 4'üydü.
> `KBP-115` canlı altı-an smoke koşumudur. Bu ticket bu yüzden **KBP-116**'dır ve
> *"backend uçtan uca yayın yapabilir"* iddiasının önündeki son kod borçlarını kapatır.
> TypeScript yazarlık ajanı (`ptn-test-agent`) bu ticket'ın kapsamı **değildir** ve hâlâ
> numarasızdır; numarayı ürün sahibi verir.

Tek görev, **dört derlenebilir dilim** ve **üç sahip eylemi**. **Her dosyayı yazmadan önce
§1'deki kapıdan geç.**

KBP-112 modülü *hedefe koşturulabilir* yaptı. Bu görev modülü **yayın yapılabilir ve
production'a açılabilir** hâle getirir. Bugün MCP yüzeyi authenticated ama yetkisiz bir token'a
açık, ajanın taşıdığı mühür alanları sınırda reddediliyor ve `SourceHash`'in hangi baytlardan
üretileceği hiçbir yerde yazılı değil.

**Bu görev .NET'e model getirmez.** Model TypeScript ajanında yaşar (ADR-0023); burası
deterministik derleyici ve doğrulayıcı olarak kalır (RULE-0005, `PackageBoundaryTests`).

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module
Branch  : KBP-116   (KBP-112 üzerinden; KBP-112 predev'e merge edilmemiştir)
Motor   : PostgreSQL
Commit  : #KBP-116 <type>: <past-tense English description>
Hedef   : C:\Users\mertb\RiderProjects\InventoryTrackingAutomation  (ilk gerçek SUT)
```

| Ön koşul | Durum |
|---|---|
| KBP-112'nin dört commit'i dalda olmalı | ✅ `06bc2d3` · `89d4d29` · `f267a07` · `7fa3aed` |
| Ölçülen taban (2026-08-16, KBP-112 sonrası) | ✅ **64 uç** · Release build **0 hata / 3 uyarı** · non-live **358/358** · live **2/2** · **8 migration** |
| `ptn-test-agent/` untracked | ⚠️ **Dokunma.** TypeScript ajanı bu görevin kapsamı değil |
| `docs/` ayrı bir Git deposudur | ⚠️ **Ürün commit'ine karıştırma.** `git add -f docs` çalıştırma |
| `ServiceShapeTests` · `ManagerReachabilityTests` · `ServiceContractTests` · `OutwardSurfaceTests` · `PackageBoundaryTests` · `ClientProxySurfaceTests` · `ToolCatalogTests` | ⛔ hepsi yeşil kalacak |

**Dosya bütçesi ≈32.** Dört dilim, dilim başına bir commit.
**Hiçbir dilim migration açmaz.** Açman gerektiğini düşünüyorsan **dur ve raporla**.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| AppService'e authorization | `house-profile.md` | `Services/Runs/TestEnvironmentAppService.cs` (`CheckPolicyAsync` ilk satırdadır) |
| Manager kararı / normalizasyon | `architecture.md` | `Managers/Catalog/TestScenarioManager.cs` → `ApplyDbSchemaFingerprint` |
| Sunucu tarafı mühür doldurma | `architecture.md` | `Services/Catalog/TestScenarioAppService.cs:174-179` (`CreateEntityAsync`) |
| DTO / validator | `contracts-mapping.md` | `FluentValidation/Catalog/TestScenarioMaterialSealDtoValidator.cs` |
| Rota / izin / kod / desen sabiti | `house-profile.md` → *Stable string ownership* | `Domain.Shared/Constants/**` · `Domain.Shared/Permissions/**` |
| Negatif authorization testi | `verification.md` | `Application.Tests/Composition/**` |

**Kanonik kararlar:** ADR-0007, ADR-0008, ADR-0014, ADR-0017, ADR-0020 (malzeme mührü),
RULE-0005, RULE-0006, RULE-0007, RULE-0008.

---

## 2. Ölçülen boşluk — 2026-08-16 kaynak taraması (KBP-112 sonrası)

### 2.1 Yedi açık madde

| # | Ne | Bugünkü kanıt | Sınıf |
|---|---|---|---|
| **1** | **MCP yetki asimetrisi** | `PtnBridgeAppService`'in **dokuz** public metodunun hiçbirinde `CheckPolicyAsync` yok. `/mcp` yalnız `RequireAuthorization()` taşır ve `PtnMcpTools` AppService'i doğrudan çağırır. `Bridge.*` izinleri **yalnız** `PtnBridgeController` attribute'larındadır. Sonuç: authenticated ama izinsiz token MCP'den ground/knowledge/validate/explain/profile/task çağırır | Security blocker |
| **2** | **Fingerprint tel biçimi ikiye ayrık** | `BusinessRuleFingerprintManager`, `ProfilePackFileManager`, `SchemaKnowledgeManager` `PtnBridgeSettingNames.FingerprintPrefix` ile `sha256:<64hex>` üretir. `TestScenarioMaterialSealDtoValidator` dört alanda düz `^[a-fA-F0-9]{64}$` ister. Sunucunun doldurduğu `DbSchemaFingerprint` `ApplyDbSchemaFingerprint` sayesinde **sorunsuzdur**; ajanın DTO'da taşıdığı `RulesFingerprint`/`SpecFingerprint`/`ProfileFingerprint` sınırda `HashInvalid` alır | Publication blocker |
| **3** | **`SourceHash` üretim sözleşmesi yok** | `CreateTestScenarioDto.SourceHash` zorunlu düz 64-hex'tir ve `HashPattern` ile doğrulanır, ama **hangi baytlardan** üretileceği (BOM, CRLF/LF, satır sonu boşluğu, sondaki newline) hiçbir yerde sabit değil. `CompiledHash` ayrı anlamdadır. Ajan uyduramaz; uydurursa `EnsureContentAvailableAsync` tekilliği yanlış çalışır | Publication blocker |
| **4** | **Mühür sunucuda tamamlanmıyor** | `TestScenarioAppService.CreateEntityAsync` yalnız `DbSchemaFingerprint`'i doldurur. `RulesFingerprint` ve `ProfileFingerprint` artık modül içinde okunabilir durumdadır (KBP-112: `IBusinessRuleSourcePort` + `BusinessRuleFingerprintManager`, `ProfilePackFileManager`) ama create/update yolunda kullanılmıyor | Publication blocker |
| **5** | **`SpecFingerprint` için kaynak yok** | `IApiOracleAppService`'te fingerprint okuyan metot yok; `SnapshotOperationInventoryDto` yalnız `SnapshotId`, `OutcomeCode`, `TotalCount`, `IsComplete`, `Items` taşır | **DUR VE SOR** |
| **6** | **RULE-0008 DMN kapsamı kodda yok** | `ScenarioPublicationGateManager.Evaluate` beş gate çalıştırır: `SchemaValidity`, `Derivability`, `AssertionCount`, `MaterialIntegrity`, `SourceDescriptionConsistency` | **DUR VE SOR** |
| **7** | **`SystemStandards.Abp.Authorization 1.0.0` hiçbir feed'de yok** | Yerel cache'teki kopyanın `.nupkg.metadata` `source` alanı bir **yerel publish klasörünü** gösterir. Temiz her klon / CI / ikinci geliştirici NU1101 alır | Sahip eylemi |

### 2.2 Sabitlenen kararlar

- **`sha256:` tel biçimidir; depolama düz 64-hex kalır.** Prefiks kendini tanımlayan bir
  sözleşmedir ve üç üretici + iki DTO onu yayınlar. Bu yüzden **üreticiler değiştirilmez**;
  mühür sınırı prefiksi kabul eder ve Manager'da soyar. `TestScenarioConsts.HashLength`,
  kolon uzunlukları ve mevcut satırlar **değişmez** → migration yok.
- **`SourceHash`'i sunucu hesaplar.** `CreateTestScenarioDto.SourceHash` **opsiyonele** iner.
  Boşsa Manager `SourceDocument`'tan hesaplar. Doluysa hesaplananla **birebir eşleşmelidir**;
  eşleşmezse `HashInvalid`. Bu bilinçli ve geriye uyumlu bir sözleşme daralmasıdır.
- **Kanonikleştirme kuralı Domain.Shared'da sabitlenir:** UTF-8, **BOM yok**, satır sonu `\n`,
  her satırın sonundaki boşluk kırpılır, belgenin sonundaki tüm boş satırlar kırpılır. Kural
  bir sabitte adlandırılır; inline yazılmaz.
- **Mühürü sunucu tamamlar, ajan doldurmaz.** `RulesFingerprint` ve `ProfileFingerprint`
  create/update yolunda `DbSchemaFingerprint` ile **aynı desende** doldurulur. Ajanın
  gönderdiği değer varsa sunucununkiyle eşleşmelidir; eşleşmezse `HashInvalid`.
- **`SpecFingerprint` bu ticket'ta üretilmez.** Modül içinde kaynağı yoktur ve bir fingerprint
  semantiği **uydurmak** ADR-0020 ile RULE-0006'yı yeniden açar. Gate bugünkü davranışını korur.
- **Yeni MCP tool açılmaz**, `ProtocolMax` **değişmez** (12), `PtnToolCodes` dosyasına dokunulmaz.
- **Yeni entity, tablo, migration, proje, katman yok.**
- **`NuGet.Config`'e dokunma.** Kimlik bilgisi temizliği sahip eylemidir; yanlış düzenleme
  restore'u kırar ve tüm kapıları kapatır.

---

## 3. Dilimler

### Dilim 1 — MCP yetki simetrisi (≈7 dosya)

**Kapattığı:** madde 1. Bu dilim olmadan production MCP açılmaz.

1. `PtnBridgeAppService`'in **dokuz** public metodunun her biri, gövdesinin **ilk satırında**
   `await CheckPolicyAsync(<izin>)` çağırır. İzinler `PtnBridgeController`'daki
   `[Authorize]` attribute'larıyla **birebir aynıdır**:

   | AppService metodu | İzin |
   |---|---|
   | `GroundAsync` | `TestModulePermissions.Bridge.Ground` |
   | `ExplainAsync` | `TestModulePermissions.Bridge.Explain` |
   | `ValidateAsync` | `TestModulePermissions.Bridge.Validate` |
   | `GetKnowledgeAsync` | `TestModulePermissions.Bridge.Knowledge` |
   | `GetToolCatalogAsync` | `TestModulePermissions.Bridge.Knowledge` |
   | `ResolveAgentProfileAsync` | `TestModulePermissions.Bridge.Profile` |
   | `CheckToolBudgetAsync` | `TestModulePermissions.Bridge.Profile` |
   | `MapTaskStatusAsync` | `TestModulePermissions.Bridge.Task` |
   | `SuggestOverlayPatchAsync` | `TestModulePermissions.Bridge.PatchSuggest` |

2. Controller attribute'ları **kaldırılmaz**. İki kapı da kalır; MCP yolu artık AppService
   kapısından geçer.
3. `GroundAsync` zaten ev bütçesinin üstündedir (~34 satır). Tek satır eklemek onu büyütür;
   **bölme, taşıma, refactor yapma** — `ServiceShapeTests` private metoda izin vermez ve
   `AuthoringSessionAppService → IPtnBridgeAppService` yönü ters delegasyonu DI döngüsüne
   çevirir. Sapmayı rapora yaz.
4. Negatif authorization testi: `Application.Tests/Composition/` altında, kimliği olan fakat
   `Bridge.*` izni olmayan bir kullanıcının `GroundAsync` çağrısının
   `AbpAuthorizationException` attığını doğrulayan test. Mevcut composition testlerinin izin
   verme desenini birebir kopyala.
5. Mevcut testler bu metotları izinsiz çağırıyorsa **testi silme veya zayıflatma**; test
   base'inin izin verme yolunu kullan (`TestEnvironmentAppService` testlerindeki desen).

**Kabul:** İzinsiz kimlik `GroundAsync`'te yetki hatası alır; izinli kimlik bugünkü sonucu alır.
`OutwardSurfaceTests` 64'te kalır, tool sayısı değişmez.

**Commit:** `#KBP-116 fix: enforced bridge permissions at the mcp service boundary`

---

### Dilim 2 — Mühür fingerprint tel biçiminin tekleştirilmesi (≈8 dosya)

**Kapattığı:** madde 2.

1. `TestScenarioConsts`'a mühür alanları için ayrı bir desen sabiti eklenir:
   `^(sha256:)?[a-fA-F0-9]{64}$`. Adı alanın işini söylesin. **`HashPattern` değişmez** —
   `SourceHash` ve `CompiledHash` düz hex kalır.
2. `TestScenarioMaterialSealDtoValidator`'daki dört kural yeni deseni kullanır.
3. `TestScenarioManager`: `ApplyDbSchemaFingerprint` içindeki prefiks soyma mantığı **tek bir
   adlandırılmış metoda** çıkarılır ve dört mühür alanının tamamı (`RulesFingerprint`,
   `SpecFingerprint`, `DbSchemaFingerprint`, `ProfileFingerprint`) `Normalize` yolunda ondan
   geçer. `ApplyDbSchemaFingerprint` davranışı **aynen korunur**.
4. Depolanan değer **her zaman prefikssiz düz 64-hex**'tir. Entity ve kolon şekli değişmez.
5. Testler (`Domain.Tests/Catalog/`): prefiksli mühür kabul edilir ve prefikssiz saklanır;
   prefikssiz mühür bugünkü gibi kabul edilir; 63/65 karakter ve `md5:` prefiksi reddedilir.

**Kabul:** `ptn_validate` / `AuthoringSourceDto` / `ProfilePackSummaryDto` çıktısındaki
`sha256:…` değeri hiçbir dönüşüm yapılmadan `CreateTestScenarioDto.MaterialSeal`'e konabilir
ve senaryo oluşur; veritabanındaki değer prefikssizdir.

**Commit:** `#KBP-116 fix: unified the material seal fingerprint wire format`

---

### Dilim 3 — `SourceHash` sunucu sözleşmesi (≈9 dosya)

**Kapattığı:** madde 3.

1. `Domain.Shared`'a kanonikleştirme sabitleri eklenir (satır sonu, kırpma kuralı adı).
   Inline string yazılmaz.
2. `TestScenarioManager`'a `SourceDocument`'tan kanonik `SourceHash` üreten public metot
   eklenir. Algoritma SHA-256, çıktı **düz lowercase 64-hex** (prefiks **yok** — bu alan mühür
   alanı değildir).
3. `CreateTestScenarioDto.SourceHash` **opsiyonel** olur; validator'daki `NotEmpty` kalkar,
   desen kuralı `When(dolu)` ile korunur. `UpdateTestScenarioDto` aynı şekilde.
4. `Normalize` yolunda: `SourceHash` boşsa hesaplanan değer kullanılır; doluysa hesaplananla
   karşılaştırılır, eşleşmezse `TestModuleScenarioErrorCodes.Validation.HashInvalid`.
5. Karşılaştırma `Ordinal`'dır; gelen değer trim + lowercase edilerek karşılaştırılır.
6. Testler: BOM'lu/BOM'suz, CRLF/LF, satır sonu boşluklu ve sondaki fazladan newline'lı **dört
   varyant aynı hash'i üretir**; yanlış `SourceHash` gönderimi `HashInvalid` alır; boş gönderim
   sunucu değerini yazar.
7. `EnsureContentAvailableAsync` tekilliği bu hesaplanan değerle çalışmaya devam eder —
   davranışı değiştirme.

**Kabul:** Ajan `SourceHash` hiç göndermeden Draft oluşturabiliyor; aynı belgeyi ikinci kez
göndermek bugünkü tekillik davranışını veriyor.

**Commit:** `#KBP-116 feat: created the server side scenario source hash contract`

---

### Dilim 4 — Mühürün sunucuda tamamlanması (≈8 dosya)

**Kapattığı:** madde 4.

1. `TestScenarioAppService.CreateEntityAsync` ve `UpdateEntityAsync`, `DbSchemaFingerprint`
   için kullandıkları **aynı deseni** iki alan daha için uygular:
   - `RulesFingerprint` ← `IBusinessRuleSourcePort.ReadAsync` + `BusinessRuleFingerprintManager.ComputeFingerprint`
   - `ProfileFingerprint` ← `ProfilePackFileManager` üzerinden çözülen paketin `ContentFingerprint`'i
   (profil anahtarı mühürde yoksa bu alan doldurulmaz — bugünkü gibi boş kalır ve
   `MaterialIntegrity` gate'i kararı verir).
2. Doldurma kararı ve karşılaştırma **Manager'da**; port/dosya I/O'su **AppService'te**.
   Domain'e `File`/`Directory`/`Process` **yazma**.
3. Ajanın gönderdiği değer varsa sunucununkiyle eşleşmelidir; eşleşmezse `HashInvalid`.
   Bu, ajanın bayat kural/profil ile senaryo mühürlemesini engeller (ADR-0020).
4. `SpecFingerprint`'e **dokunma**. Kaynağı yok; bugünkü davranış korunur.
5. Testler: kural dosyası uçtan değiştirildikten sonra oluşturulan senaryonun
   `RulesFingerprint`'i **yeni** baytları gösterir; bayat fingerprint gönderimi reddedilir.

**Kabul:** Ajan mühürde yalnız kimlikleri (`SpecSnapshotId`, `DbConnectionId`, profil anahtarı)
göndererek geçerli senaryo oluşturabiliyor; fingerprint alanlarını taşımak zorunda değil.

**Commit:** `#KBP-116 feat: created the server side material seal completion`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **Dilim 4** devredilir — Dilim 2 ve 3 ajanın mührü elle taşımasını zaten mümkün
kılar.
**Kesilmeyecekler: 1, 2, 3.** Dilim 1 güvenlik kapısıdır; 2 ve 3 olmadan hiçbir Draft
oluşturulamaz.

---

## 5. Yasaklar

1. `.NET` tarafına model istemcisi getirme (`IChatClient`, Ollama, OpenAI, Gemini).
2. Yeni MCP tool açma; `ProtocolMax`'ı büyütme; `PtnToolCodes.cs`'e dokunma.
3. Yeni entity, tablo, migration, proje, katman, `Infrastructure/`, `Helpers/`, `Utils/` açma.
4. `TestScenarioConsts.HashLength`, kolon uzunluğu veya EF configuration değiştirme.
5. Fingerprint **üreticilerini** değiştirme — `sha256:` prefiksi kalır.
6. `SpecFingerprint` için fingerprint semantiği uydurma — §2.2.
7. RULE-0008 DMN gate'ini kendi başına ekleme — §6 sahip eylemidir.
8. `NuGet.Config`'e dokunma.
9. `PtnBridgeController`'daki `[Authorize]` attribute'larını kaldırma.
10. Application servisine private iş metodu veya guard koyma (`ServiceShapeTests`).
11. `Domain/Managers/**` içine `Process`/`File`/`Directory` yazma.
12. Rota, izin, hata kodu, ayar adı, desen (regex) için inline string yazma.
13. Belirsizliği tahminle kapatma; cevapsız belirsizliği yayına geçirme (RULE-0006, RULE-0007).
14. `ptn-test-agent/` altına dokunma; `docs/` değişikliğini ürün commit'ine karıştırma.
15. Kırılan testi silme, `Skip` etme, assertion zayıflatma.
16. Ara dilimlerde build/test atlama; başarısız kapıdan sonra sonraki dilime geçme.

---

## 6. Sahip eylemleri — kod değil, karar

Bunlar **senin işin değildir**. Raporda "sahip bekliyor" olarak listele.

| # | Konu | Sahip | Sorulacak soru |
|---|---|---|---|
| S1 | `SystemStandards.Abp.Authorization 1.0.0` hiçbir feed'de yayımlı değil; yerel cache'ten çözülüyor | Paket sahibi | Nexus'a mı yayımlanacak, yoksa Notifications zinciri bu bağımlılıktan mı kurtarılacak? |
| S2 | Kök `NuGet.Config` düz metin `ClearTextPassword` taşıyor ve depoda izleniyor | Repo/güvenlik sahibi | Kimlik bilgisi ortam değişkenine mi taşınacak? Parola döndürülecek mi? |
| S3 | RULE-0008 DMN satır kapsamı yayın şartı sayıyor ama gate'te ölçülmüyor | Ürün sahibi | Kural mı gevşeyecek, altıncı gate mi eklenecek? |
| S4 | **`SpecFingerprint` için modül içinde kaynak yok ve yayın bunsuz açılmıyor.** `ScenarioPublicationGateManager.IsMaterialSealComplete` altı alanın **hepsini** dolu ister; `SpecFingerprint` boşken `MaterialIntegrity` gate'i düşer ve senaryo **Published olamaz**. Bu, canlı SUT koşumunun (KBP-115) önündeki **tek sert kilittir** | Checker sahibi | API Contract Checker snapshot fingerprint'i public sözleşmeye çıkaracak mı? Çıkmayacaksa operatörün elle koyduğu bir değer kabul edilip mühür bilinçli olarak zayıflatılacak mı? |

---

## 7. Kabul kriterleri

- `PtnBridgeAppService`'in **dokuz** metodunun tamamı `CheckPolicyAsync` ile korunuyor;
  izinsiz kimliğin reddedildiğini kanıtlayan negatif test var.
- `PtnBridgeController` `[Authorize]` attribute'ları duruyor.
- Prefiksli (`sha256:…`) mühür alanı uçtan kabul ediliyor ve veritabanına prefikssiz yazılıyor;
  geçersiz biçim hâlâ `HashInvalid` alıyor.
- `SourceHash` gönderilmeden Draft oluşturulabiliyor; BOM/CRLF/trailing-whitespace varyantları
  **aynı** hash'i üretiyor; yanlış gönderim reddediliyor.
- `RulesFingerprint` ve `ProfileFingerprint` sunucuda doldurulyor; bayat değer reddediliyor.
- `SpecFingerprint` davranışı **değişmedi**.
- **Migration üretilmedi** — sayı 8'de sabit.
- **Uç sayısı değişmedi: 64** (`OutwardSurfaceTests.ExpectedControllerActionCount`).
- **MCP tool sayısı değişmedi**; `ToolCatalogTests` ve `PackageBoundaryTests` yeşil.
- `dotnet build Ptn.TestModule.slnx -m:1 -c Release` → **0 hata**.
- `dotnet test --filter "Category!=LiveInfrastructure"` → 0 başarısız (taban **358**).
- `dotnet test --filter "Category=LiveInfrastructure"` → hâlâ yeşil (taban 2/2).

---

## 8. Bitiş

1. §5'in 16 maddesini kendi kodunda tek tek kontrol et.
2. Dört dilimi sırayla commit et; her dilim kendi build/test kapısını geçmeden sonrakine geçme.
3. Tek sefer: Test Module build → iki filtreli `dotnet test`.
4. `/backend-verify` gate'i; her commit öncesi başlık doğrulayıcısını koştur:
   `check-backend-diff.ps1 -CommitMessage "<subject>"`.
5. Raporda **zorunlu**: öncesi/sonrası uç sayısı ve test sayısı; negatif authorization testinin
   adı; prefiksli mühür kabul testinin adı; dört `SourceHash` varyant testinin adı;
   `GroundAsync` satır sayısı sapması; §6'daki dört sahip eyleminin durumu; her varsayım.

---

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| Canlı altı-an koşumu ve yeşil koşum kanıtı | **KBP-115** — canlı altyapı smoke |
| InventoryTrackingAutomation için spec snapshot, DB connection, Vault secret kaydı | **Kurulum turu** — kod değil |
| TypeScript yazarlık ajanı (`ptn-test-agent`) | **Numarasız** — ürün sahibi numara verir |
| `SpecFingerprint` kaynağı | Checker deposu — §6 S4 |
| RULE-0008 DMN gate'i | Ürün kararı — §6 S3 |
| `AbpPermissionManagement` kompozisyonu, Finding detay ucu | Ölçüldüğünde |
| UI | Headless runtime tamamlanana kadar bekler |
