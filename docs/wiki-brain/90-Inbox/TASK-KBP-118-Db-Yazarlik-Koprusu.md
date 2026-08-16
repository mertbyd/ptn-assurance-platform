# AJAN GÖREVİ — KBP-118 · DB yazarlık köprüsü

> [!INFO] Neden bu ticket var
> Compiler `x-checknexus-db` adımlarını **derleyebiliyor**, ama yazarlık oturumunun tipli bir DB
> adımı üretecek yolu **yok**. `AddAuthoringStepDto` yalnız `StepId`, `OperationReferenceId`,
> `RequestBodyJson`, `AssertionPaths` taşıyor; `AuthoringSessionController` dört rota taşıyor
> (`POST` · `GET {id}` · `answer` · `step`). Sonuç: ürünün ayırt edici yarısı — **API + DB çift
> oracle** — yazarlık hattına bağlı değil. Ajan bugün `databaseAssertions: []` göndermek zorunda.

Tek görev, **üç derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

**Bu görev .NET'e model getirmez** (ADR-0023, RULE-0005). LLM serbest tablo/kolon/matcher/bağlantı
değeri **yazmaz**; yalnız backend'in döndürdüğü kapalı kümeden seçer (RULE-0007).

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module
Branch  : KBP-118   (KBP-117 üzerinden)
Motor   : PostgreSQL
Commit  : #KBP-118 <type>: <past-tense English description>
```

| Ön koşul | Durum |
|---|---|
| KBP-117 dalda ve yeşil | ✅ (taban: **375** non-live test) |
| `ptn-test-agent/` untracked | ⚠️ **Dokunma** |
| `docs/` ayrı depo | ⚠️ Ürün commit'ine karıştırma |
| `ServiceShapeTests` · `ManagerReachabilityTests` · `ServiceContractTests` · `OutwardSurfaceTests` · `PackageBoundaryTests` · `ClientProxySurfaceTests` · `ToolCatalogTests` · `BridgeAuthorizationTests` | ⛔ yeşil kalacak |

**Dosya bütçesi ≈24.** Üç dilim, dilim başına bir commit. **Migration yok.**

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku | Sonra bak (canlı örnek) |
|---|---|---|
| DTO / validator | `contracts-mapping.md` | `Dtos/Authoring/AddAuthoringStepDto.cs` · `FluentValidation/Authoring/**` |
| Controller action | `house-profile.md` | `Controllers/Authoring/AuthoringSessionController.cs` (dört rota) |
| Oturum kararı | `architecture.md` | `Managers/Authoring/AuthoringSessionManager.cs` → `AddStep` |
| Arazzo örgüsü | `architecture.md` | `Managers/Compilation/ArazzoCompilerManager.cs` → `x-checknexus-db` yolu |
| Grounding çıktısı | `house-profile.md` | `Dtos/Bridge/GroundResultDto.cs` · `Dtos/Bridge/Database/TableDescriptionDto.cs` |
| Checker DB yüzeyi | ADR-0007 | `IDatabaseOracleAppService.ValidateDerivabilityAsync` · `Mappers/Bridge/DatabaseOracleMapper.cs` |
| Rota / kod / sözlük sabiti | `house-profile.md` → *Stable string ownership* | `Domain.Shared/Constants/**` |

**Kanonik kararlar:** ADR-0007 (checker salt-okunur), ADR-0014, ADR-0015 §C (DB adımı Arazzo'ya
derlenir), ADR-0017, ADR-0020, RULE-0005, RULE-0006, RULE-0007.

---

## 2. Ölçülen boşluk

| # | Ne | Bugünkü kanıt |
|---|---|---|
| **1** | **Grounding DB adayını kapalı küme olarak vermiyor** | `GroundResultDto.TableDescription` zengin (`DbSchemaName`, `TableName`, `Columns`, `PrimaryKey`, `UniqueIndexes`, `ForeignKeyNeighbors`, `LintWarnings`) ama **opak `TableReferenceId` yok**, **`AllowedMatchers` yok**, assertable alanlar ayrı kapalı liste değil. Ajan serbest metin yazmadan seçim yapamıyor |
| **2** | **Tipli DB adımı sözleşmesi yok** | `AddAuthoringStepDto` yalnız API alanları taşıyor; `AuthoringSessionController`'da DB rotası yok |
| **3** | **Adım belgeye örülmüyor** | `ArazzoCompilerManager` elle yazılmış `x-checknexus-db`'yi derliyor, ama oturumdan gelen tipli DB adımını üreten yol yok. `ptn_validate` çağrısında `databaseAssertions` hep boş |

### 2.1 Sabitlenen kararlar

- **Serbest metin taşınmaz.** Tablo, kolon, matcher ve bağlantı **opak referans veya kapalı kod**
  olarak gider. Ajan `tableReferenceId` + `assertableField` + `matcherCode` **seçer**, yazmaz.
- **Referans oturuma bağlıdır.** `AuthoringSessionManager`, verilen `TableReferenceId`'nin o
  oturumun grounding adaylarında bulunduğunu doğrular — `AddStep`'in `OperationReferenceId` için
  yaptığının aynısı. Bulunmazsa `AuthoringOperationNotGrounded` ailesinden kararlı kod.
- **Matcher kümesi Domain.Shared'da yaşar.** Yeni bir kapalı sözlük sabiti açılır; inline string yok.
- **Checker'a yazma yok, checker tablosu okuma yok, FK yok, ortak transaction yok** (ADR-0007).
- **Yeni entity, tablo, migration yok.** Oturum bugünkü gibi distributed cache'te kalır.
- **Yeni MCP tool açılmaz**, `ProtocolMax` **12** kalır, `PtnToolCodes.cs`'e dokunulmaz.
  DB adayları mevcut `ptn_ground` cevabında taşınır.

---

## 3. Dilimler

### Dilim 1 — Grounding'in kapalı DB adaylarını yayınlaması (≈8 dosya)

**Kapattığı:** madde 1.

1. `Domain.Shared`'a kapalı matcher sözlüğü eklenir (ör. `Equals`, `NotNull`, `GreaterThan` —
   kümeyi `IDatabaseOracleAppService`'in gerçekten desteklediğiyle sınırla, uydurma).
2. `TableDescriptionDto` (veya `GroundResultDto` altında yeni bir `DatabaseBindingDto`) şunları
   kazanır: opak **`TableReferenceId`**, kapalı **`AssertableFields`**, kapalı **`AllowedMatchers`**,
   **`KeyCandidates`**. Hangisini seçtiğini rapora yaz; **yeni DTO açacaksan precedent göster**.
3. `TableReferenceId` deterministik ve oturum içinde çözülebilir olmalıdır; ham şema/tablo adı
   ajan girdisine geri açılmaz.
4. `GroundingManager` bu adayları mevcut `ResolveTableBinding` yolundan üretir — ikinci bir
   çözümleme kuralı yazılmaz.
5. Testler: aday listesi kararlı sırada · `AllowedMatchers` kapalı küme dışına çıkmıyor ·
   tablo çözülemediğinde bağlama boş dönüyor ve kapalı soru üretiliyor.

**Kabul:** `ptn_ground` cevabı, ajanın serbest metin yazmadan bir DB assertion adayı seçmesine
yetiyor.

**Commit:** `#KBP-118 feat: created the closed database binding surface on grounding`

---

### Dilim 2 — Tipli DB yazarlık adımı (≈9 dosya)

**Kapattığı:** madde 2.

1. `AddDatabaseAuthoringStepDto` — `Dtos/Authoring/` altında, `AddAuthoringStepDto`'nun kardeşi.
   Taşıdığı alanlar **yalnız kapalı referanslar**: `StepId`, `TableReferenceId`, `OperationCode`
   (assertRow / assertCount / assertAbsent — mevcut `IDatabaseOracleAppService` yüzeyiyle sınırlı),
   `KeyBindings` (alan → kaynak adım çıktısı referansı), `Expectations` (`assertableField` +
   `matcherCode` + değer referansı), opsiyonel `TimeoutMs` / `PollIntervalMs`.
2. Repository-native FluentValidation validator'ı: `StepId` boş değil, `TableReferenceId` boş Guid
   değil, `OperationCode` kapalı kümede, her `Expectation` kapalı alan ve kapalı matcher taşıyor,
   bütçe alanları pozitif.
3. `IAuthoringSessionAppService` `AddDatabaseStepAsync` kazanır; `AuthoringSessionController`
   `POST` rotası — rota sabiti `AuthoringSessionRoutes`'a eklenir (`step` deseninin aynısı).
4. `AuthoringSessionManager` `AddDatabaseStep` kazanır: referans grounding adaylarında mı,
   `Expectations` boş mu, aynı `StepId` iki kez mi — kararlar **Manager'da**.
5. Mapping mevcut `Mappers/Authoring/**` partial'ına eklenir.
6. Testler: grounded olmayan `TableReferenceId` reddediliyor · kapalı küme dışı matcher
   reddediliyor · `Expectations` boşken adım kabul edilmiyor · geçerli adım oturuma yazılıyor.

**Kabul:** Ajan tek çağrıyla tipli bir DB assertion adımı ekleyebiliyor; hiçbir alanda serbest
tablo/kolon/matcher metni taşınmıyor.

**Commit:** `#KBP-118 feat: created the typed database authoring step surface`

---

### Dilim 3 — Adımın Arazzo belgesine örülmesi (≈7 dosya)

**Kapattığı:** madde 3.

1. `ArazzoCompilerManager`, oturumdaki DB adımlarını **mevcut** `x-checknexus-db` derleme yolundan
   geçirir — ikinci bir derleyici yazılmaz. `TargetVersion = "1.0.1"` sabittir.
2. Belge her adımda bugünkü gibi **mekanik olarak yeniden üretilir**; DB adımı API adımlarıyla
   kararlı sırada yerleşir.
3. `ptn_validate` yolunda `databaseAssertions` artık oturumdaki DB adımlarından doldurulur ve
   `IDatabaseOracleAppService.ValidateDerivabilityAsync`'e gider. `DatabaseDerivability` gate'i
   gerçek adımlarla çalışır.
4. `assertion_count` DB adımlarını da sayar; `AssertionCount` gate'i bozulmaz.
5. Testler: DB adımı eklenmiş oturumun derlenmiş belgesinde `x-checknexus-db` uzantısı var ·
   `ptn_validate` `databaseAssertions`'ı boş göndermiyor · yalnız API adımı olan oturum
   bugünkü çıktısını **birebir** koruyor.

**Kabul:** Bir API adımı + bir DB adımı içeren oturum, `isSchemaValid: true` ve
`databaseDerivability.allDerivable: true` üretebiliyor.

**Commit:** `#KBP-118 feat: created the database step compilation into the arazzo document`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **Dilim 3** devredilir; 1 ve 2 tek başlarına ajanın DB adımını seçip göndermesini
sağlar. **Kesilmeyecekler: 1, 2.**

---

## 5. Yasaklar

1. `.NET` tarafına model istemcisi getirme.
2. Yeni MCP tool açma; `ProtocolMax`'ı büyütme; `PtnToolCodes.cs`'e dokunma.
3. Yeni entity, tablo, migration, proje, katman, `Infrastructure/`, `Helpers/`, `Utils/` açma.
4. Serbest tablo, kolon, matcher, şema veya connection string'i public girdiye açma (RULE-0007).
5. İkinci bir Arazzo derleyicisi veya ikinci bir tablo çözümleme kuralı yazma.
6. Checker tablosunu okuma, checker'a FK verme, ortak transaction açma (ADR-0007).
7. Application servisine private iş metodu veya guard koyma (`ServiceShapeTests`).
8. `Domain/Managers/**` içine `Process`/`File`/`Directory`/SQL yazma.
9. Rota, izin, hata kodu, matcher kodu, uzantı adı için inline string yazma.
10. Belirsizliği tahminle kapatma (RULE-0006, RULE-0007).
11. `ptn-test-agent/` altına dokunma; `docs/` değişikliğini ürün commit'ine karıştırma.
12. Kırılan testi silme, `Skip` etme, assertion zayıflatma, testi geçsin diye üretim kodu değiştirme.
13. Ara dilimlerde build/test atlama; başarısız kapıdan sonra sonraki dilime geçme.

---

## 6. Kabul kriterleri

- `ptn_ground` cevabı opak `TableReferenceId`, kapalı `AssertableFields` ve `AllowedMatchers` taşıyor.
- Grounded olmayan `TableReferenceId` ve kapalı küme dışı matcher reddediliyor; negatif testleri var.
- `POST api/test-module/authoring/sessions/{id}/database-step` tipli adımı kabul ediyor.
- Derlenmiş belgede `x-checknexus-db` uzantısı görünüyor; yalnız API adımı olan oturumun çıktısı **değişmiyor**.
- `ptn_validate` `databaseAssertions`'ı gerçek adımlarla dolduruyor; `DatabaseDerivability` gate'i çalışıyor.
- **Migration üretilmedi.** **MCP tool sayısı değişmedi.** `ToolCatalogTests` ve `PackageBoundaryTests` yeşil.
- Uç sayısı **64 → 65** (`OutwardSurfaceTests.ExpectedControllerActionCount`).
- `dotnet build Ptn.TestModule.slnx -m:1 -c Release` → **0 hata**.
- `dotnet test --filter "Category!=LiveInfrastructure"` → 0 başarısız (taban **375**).
- `dotnet test --filter "Category=LiveInfrastructure"` → yeşil (taban 2/2).

---

## 7. Bitiş

1. §5'in 13 maddesini kendi kodunda tek tek kontrol et.
2. Üç dilimi sırayla commit et; kapı geçmeden sonrakine geçme.
3. Tek sefer: build → iki filtreli `dotnet test`.
4. `/backend-verify`; her commit öncesi `check-backend-diff.ps1 -CommitMessage "<subject>"`.
   **Not:** `TestScenarioManager.cs` üzerindeki **13** `[ENTITY]` bulgusu bilinen false
   positive'dir; yeni bulgu ekleyip eklemediğini bu sayıyla karşılaştır.
5. Commit başlığı **`#` ile başlar**: `#KBP-118 ...`. `#` olmadan issue tracker bağlayamaz.
6. Raporda **zorunlu**: seçtiğin DTO yerleşimi ve precedent'i; matcher sözlüğünün kaynağı;
   `TableReferenceId`'nin nasıl türetildiği; öncesi/sonrası uç ve test sayısı; scanner bulgu
   sayısı; her varsayım.

---

## 8. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| TypeScript ajanının bu sözleşmeye bağlanması | Ajan deposu — numarasız |
| Canlı altı-an koşumu | **KBP-115** |
| `Pintern.SaaS.Notifications.*` yeniden paketleme | Paket sahibi — bkz. Inbox |
| UI devri (finding detay ucu, katalog rejenerasyonu) | Ayrı ticket |
