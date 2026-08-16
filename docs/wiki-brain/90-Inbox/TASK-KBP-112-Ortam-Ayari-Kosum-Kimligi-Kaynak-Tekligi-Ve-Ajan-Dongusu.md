# AJAN GÖREVİ — KBP-112 · Ortam ayarı, koşum kimliği, kaynak tekliği ve ajan döngüsü

> [!INFO] Kapsam birleştirmesi
> Bu görev, 2026-08-16 boşluk taramasında ayrı ayrı numaralanan **KBP-112a, KBP-112b, KBP-113
> ve KBP-114** kalemlerini **tek ticket'ın dört dilimi** olarak taşır. Numara sürüklenmesi
> yaşanmasın diye kayıt açıktır: `KBP-113` ve `KBP-114` numaraları **kullanılmayacaktır**;
> kapsamları bu belgenin Dilim 3 ve Dilim 4'üdür. (Aynı soğurma deseninin önceki örneği:
> KBP-94 → KBP-93, KBP-96 → KBP-95.)

Tek görev, **dört derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

KBP-109 modülü *ulaşılabilir*, KBP-110 *kendi kendine döner*, KBP-111 *yazarlık yapabilir*
yaptı. Bu görev modülü **gerçek bir hedef sisteme karşı koşturulabilir** hâle getirir.

Bugün modül kendi testlerinde yeşil, fakat dışarıdaki hiçbir yazılımı test edemez: ortam
bağlaması yazılamıyor, koşum hedefe kimlik doğrulayamıyor, runner hedefe ağdan ulaşamıyor,
ajanın okuduğu iş kuralı ile mühürlenen iş kuralı iki ayrı bayt kümesi ve ajanın önerdiği
adım sunucuya hiç yazılamıyor.

**Bu görev .NET'e model getirmez.** Model TypeScript ajanında yaşar (ADR-0023); burası
deterministik derleyici ve doğrulayıcı olarak kalır (RULE-0005, `PackageBoundaryTests`).

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform   (branch predev)
Modül   : ptn-test-module
Branch  : KBP-112   (predev üzerinden)
Motor   : PostgreSQL
Commit  : #KBP-112 <type>: <past-tense English description>
Hedef   : C:\Users\mertb\RiderProjects\InventoryTrackingAutomation  (ilk gerçek SUT)
```

| Ön koşul | Durum |
|---|---|
| KBP-111 `predev`'e merge edilmiş olmalı | ✅ `257aaa7` |
| Ölçülen taban (2026-08-16) | ✅ 58 uç · Release build **0 hata / 3 uyarı** · non-live **337/337** · 8 migration |
| Çalışma kopyasında `PtnMcpTools.cs` üzerinde **sana ait olmayan** boş-satır silmesi var | ⚠️ **İlk iş:** `git checkout -- ptn-test-module/host/Ptn.TestModule.HttpApi.Host/Mcp/PtnMcpTools.cs`. Bu değişiklik bir formatter kazasıdır, commit edilmez |
| `ptn-test-agent/` untracked | ⚠️ **Dokunma.** TypeScript ajanı bu görevin kapsamı değil |
| `ServiceShapeTests` · `ManagerReachabilityTests` · `ServiceContractTests` · `OutwardSurfaceTests` · `PackageBoundaryTests` · `ClientProxySurfaceTests` | ⛔ hepsi yeşil kalacak |

**Dosya bütçesi ≈60.** Dört dilim, dilim başına bir commit.
**Dilim 1 ve 3 migration sorusu açar** — §2.2'deki kapıdan geç.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| AppService / Controller / DTO / validator | `house-profile.md` | `Services/Runs/TestFindingAppService.cs` · `Controllers/Runs/TestFindingController.cs` |
| Manager kararı | `architecture.md` | `Managers/Runs/RunEnvironmentBindingManager.cs` (aynı dosyayı genişleteceksin) |
| Ayar tanımı | `house-profile.md` → *Stable string ownership* | `Domain/Settings/TestModuleSettingDefinitionProvider.cs` |
| Süreç/docker sınırı | `architecture.md` → *Capability port ve adapter* | `Managers/Runs/WorkflowRunPlanner.cs` · `Services/Shared/ProcessBoundaryService.cs` |
| Dosya okuma/yazma sınırı | `house-profile.md` → *External files and wire payloads* | `Services/Bridge/BusinessRuleSourceService.cs` |
| MCP yüzeyi | ADR-0008 | `host/Mcp/PtnMcpTools.cs` · `host/Mcp/BusinessRulesResource.cs` |
| Rota / izin / kod sabiti | `house-profile.md` → *Stable string ownership* | `Domain.Shared/Constants/**` |
| Host modül kompozisyonu | `infrastructure-bootstrap.md` | `host/TestModuleHttpApiHostModule.cs` |

**Kanonik kararlar:** ADR-0007 (checker salt-okunur), ADR-0008 (MCP yerleşimi), ADR-0013
(resource server), ADR-0014 (yazarlık, iş bilgisi Git'te), ADR-0015 §A/§C/§E/§G (koşum sınırı),
ADR-0016 (4 tablo + 5 lookup), ADR-0019 (profil paketi), ADR-0020 (malzeme mührü),
ADR-0023 (TypeScript ajan sınırı), RULE-0002, RULE-0005, RULE-0006, RULE-0007.

---

## 2. Ölçülen boşluk — 2026-08-16 kaynak taraması

### 2.1 Yedi açık madde

| # | Ne | Bugünkü kanıt |
|---|---|---|
| **1** | **Ayar yazılamıyor** | `AbpSettingManagement*` paketleri Authenticator üzerinden **transitif geliyor** (`dotnet list package --include-transitive` 5 paket gösteriyor) ama host `DependsOn` listesinde **modül compose edilmemiş**. `appsettings.json`'da `Settings:` bölümü de yok. Sonuç: 20 ayarın tamamı yalnız kod varsayılanına düşüyor, tenant başına değer imkânsız |
| **2** | **Ortam bağlaması yazma ucu yok** | `TestEnvironmentController` yalnız `GetList` + `{key}/sandbox/reset` taşıyor. `RunEnvironmentBindingManager` yalnız `ResolveAsync`/`ListAsync` okuyor. `EnvironmentBindings` varsayılanı `"{}"` → **her koşum `EnvironmentNotBound` ile ölüyor** |
| **3** | **Runner hedefe kimlik doğrulayamıyor** | `TestRunExecutionManager.BuildInputs` runner'a yalnız `baseUrl`, `environmentKey`, `specSnapshotId`, `dbConnectionId`, `traceId` veriyor. Token/credential kanalı **yok**. `SecretRef` yalnız `database` bölümünde ve DB Checker'a gidiyor. Korumalı hiçbir uç koşulamaz |
| **4** | **Runner container'ı hedefe ulaşamıyor** | `WorkflowRunPlanner.BuildArguments` docker argümanlarını kuruyor; `--network` ve `--add-host` **yok**. SUT `http://localhost:5000`'de, container içinden `localhost` container'ın kendisi |
| **5** | **İş kuralının iki kaynağı var** | `BusinessRulesResource.Read()` **assembly'ye gömülü** `Ptn.TestModule.Authoring.kurallar.md`'yi okuyor; `BusinessRuleSourceService.ReadAsync()` **diskten**, `BusinessRulesPath` ayarından okuyor. Dosya değişip host derlenmezse ajan eski kuralı okur, yeni kural mühürlenir. Ayrıca Resource `static` — tenant farkındalığı yok |
| **6** | **Profil paketi ve kural dosyası yüklenemiyor** | `ProfilePackPath` varsayılanı `samples/profiles`; host content root'u `host/Ptn.TestModule.HttpApi.Host`, `samples/` ise bir üst dizinde → **yol çözülmüyor**. Yükleme/listeleme ucu da yok |
| **7** | **Ajan önerdiği adımı sunucuya yazamıyor** | Yazarlık oturumunun dört ucu (`sessions`, `/answer`, `/step`, `GET`) yalnız REST. MCP'de karşılığı yok. Ajan `approval_required` yayınlayıp duruyor; onaylanan adım `AddAuthoringStep`'e hiç gitmiyor. Yazarlık döngüsü kapanmıyor |

### 2.2 Sabitlenen kararlar

- **Ayar tablosunun sahibi bu modül DEĞİLDİR — önce doğrula.** `abp.AbpSettings`, `abp` şemasındaki
  Identity/PermissionManagement tablolarıyla aynı ailedendir ve `ptn-test-module/AGENTS.md`
  *"Auth, Notification, Emailing ve checker tabloları için migration üretilmez"* diyor.
  **Yapılacak:** `C:\Users\mertb\RiderProjects\pintern-authenticator-latest-api` içinde
  `AbpSettings` migration'ı var mı bak. **Varsa** bu modül yalnız modülü compose eder,
  `ConfigureSettingManagement()` çağırmaz ve **migration üretmez**. **Yoksa dur ve raporla** —
  framework tablosunun sahipliği RULE-0002 kararıdır, sessizce üstlenilmez.
- **Ayar yüzeyi ABP'nin kendisidir.** `AbpSettingManagementHttpApiModule` compose edilince
  `/api/setting-management/settings` uçları gelir; **paralel bir ayar CRUD'u yazılmaz.**
  Ortam bağlaması bunun istisnasıdır: JSON haritasının şeması ve `environmentKey` eşleşme
  kuralı domain kararıdır, ham JSON'u UI'a yazdırmak bu kuralı kaçırır.
- **Ortam bağlaması tablo DEĞİLDİR.** ADR-0016 §G korunur: değer `ISettingManager` üzerinden
  tenant-scoped ayara yazılır. Yeni entity, yeni tablo, yeni migration **yok**.
- **Sır değeri DTO'ya, log'a, `RunnerRef`'e, exception data'sına ve HAR'a girmez.** Runner'a
  yalnız tek ortam değişkeni üzerinden geçer (AUDIT-0002 BULGU-09 kanalı korunur).
  **HAR redaksiyonu bu görevin parçasıdır** — §3 Dilim 2.
- **Yeni MCP tool açılmaz.** `PtnToolCodes.Governed` bugün 12 kod taşıyor ve
  `ProtocolMax = 12` **doludur**. Oturum yeteneği KBP-111 §2.2'nin kurduğu desenle
  **mevcut `ptn_ground` arkasına** bağlanır. `ProtocolMax`'ı büyütmek RULE-0007 token
  ekonomisini ve ADR-0023 §C'yi yeniden açar → **dur ve sor**.
- **LLM final belgeyi yazmaz.** Ajan turda tek adım önerir; birleştirme ve Arazzo 1.0.1
  üretimi `ArazzoCompilerManager`'dadır. `TargetVersion = "1.0.1"` sabittir.
- **Checker'a yazma yok, checker tablosu okuma yok, FK yok, ortak transaction yok** (ADR-0007, ADR-0015 §F).
- **Test Module'de migration üretilmez** — Dilim 1'in doğrulaması aksini gösterirse dur ve raporla.

---

## 3. Dilimler

### Dilim 1 — Ayar altyapısı ve ortam bağlaması CRUD (≈18 dosya)

**Kapattığı:** madde 1 ve 2. Bu dilim olmadan diğer üçü doğrulanamaz.

1. Host `TestModuleHttpApiHostModule` `DependsOn` listesine `AbpSettingManagementApplicationModule`,
   `AbpSettingManagementEntityFrameworkCoreModule`, `AbpSettingManagementHttpApiModule` eklenir.
   Paketler zaten transitif; `common.props`'a yeni sürüm değişkeni **eklenmez**.
2. §2.2'nin `AbpSettings` sahiplik doğrulaması yapılır ve sonucu rapora yazılır.
3. `RunEnvironmentBindingManager` **yazma tarafını** kazanır: `environmentKey` tekilliği,
   `api.environmentKey == database.environmentKey == key` kapısı, `baseUrl` mutlak URL kuralı,
   `specSnapshotId`/`dbConnectionId` boş-Guid reddi, haritanın kararlı serileştirmesi.
   Karar Manager'da; `ISettingManager` I/O'su AppService'te.
4. `ITestEnvironmentAppService` `CreateAsync` / `UpdateAsync` / `DeleteAsync` kazanır.
5. `TestEnvironmentController`: `POST`, `PUT {key}`, `DELETE {key}`. Rotalar
   `TestEnvironmentRoutes` sabitlerinden.
6. `CreateTestEnvironmentBindingDto` · `UpdateTestEnvironmentBindingDto` + FluentValidation
   validator'ları. Mapping `Mappers/Runs/TestRunMapper.cs` partial'ına eklenir.
7. Yeni izin: `TestModulePermissions.Runs.ManageEnvironments`.

**Kabul:** Boş ayardan başlayıp `POST /api/test-module/environments` ile bağlama yazılıyor,
`GET` onu geri veriyor, `TestRunAppService.TriggerAsync` artık `EnvironmentNotBound` atmıyor.

**Commit:** `#KBP-112 feat: created the setting management composition and the environment binding write surface`

---

### Dilim 2 — Koşum kimliği ve runner ağ sınırı (≈14 dosya)

**Kapattığı:** madde 3 ve 4. Gerçek bir SUT'a ilk yeşil koşumun ön şartı.

1. `TestRunEnvironmentBinding` modeli `api.secretRef` kazanır; `RunEnvironmentBindingManager.ReadBinding`
   onu okur. Bugün `secretRef` yalnız `database` bölümünde.
2. Koşum anında API sırrı mevcut Vault portundan çözülür ve `BuildInputs` çıktısına
   **yalnız runner'ın okuyacağı girdi olarak** eklenir. Yeni girdi anahtarları
   `WorkflowRunnerConsts.Inputs` altında sahiplenilir. Değer hiçbir DTO'ya, `test_runs`
   satırına, log'a veya exception `WithData`'sına yazılmaz.
3. **HAR redaksiyonu.** `HarInterpreter` / `HarArtifactService` yolunda, blob'a yazılmadan önce
   `Authorization`, `Cookie`, `Set-Cookie` ve API-key başlıkları maskelenir. Bugün HAR ham
   başlıkları taşıyor; kimlik eklenince bearer token BLOB deposuna düşer. Maskeleme kuralı
   Manager'da, blob I/O'su serviste kalır.
4. Runner ağ sınırı: `TestModule.Runs.RunnerNetworkMode` ve `TestModule.Runs.RunnerExtraHosts`
   ayarları tanımlanır (`TestModuleSettingDefinitionProvider`), `WorkflowRunPlanner.BuildArguments`
   bunları `--network` ve `--add-host` olarak kurar. Ayar boşsa bugünkü davranış birebir korunur.
5. `ArazzoLintManager`'ın docker çağrısı aynı ayarları kullanmaz — lint dış ağ istemez, dokunma.

**Kabul:** `RunnerExtraHosts = "host.docker.internal:host-gateway"` ile kurulan bir koşumda
runner container'ı host üzerindeki SUT'a ulaşıyor; üretilen HAR'da `Authorization` başlığı
maskelenmiş görünüyor.

**Commit:** `#KBP-112 feat: created the runner credential channel and the container network boundary`

---

### Dilim 3 — İş kuralı ve profil paketi kaynak tekliği (≈16 dosya)

**Kapattığı:** madde 5 ve 6. Ajanın okuduğu bayt ile mühürlenen bayt aynı olur.

1. Host `.csproj`'daki `EmbeddedResource` satırları (`agent-policy.md`, `kurallar.md`) **kaldırılır**.
2. `BusinessRulesResource` gömülü akış yerine `IBusinessRuleSourcePort`'u DI ile alır —
   `PtnMcpTools`'un `IPtnBridgeAppService service` parametresi aldığı desenin aynısı.
   `AgentPolicyResource` aynı şekilde ayarlı yoldan okuyan bir porta bağlanır.
3. `ProfilePackPath` varsayılanı çözülebilir hâle getirilir (host content root'una göre) ve
   `samples/profiles/acme-ticketing.yaml` örneği host altına taşınır **veya** varsayılan
   düzeltilir. Hangisini seçtiğini rapora yaz.
4. Yükleme uçları: `POST api/test-module/authoring/business-rules` ve
   `POST api/test-module/authoring/profile-packs`. Kabul yalnız UTF-8 metin, ad sabit
   (`kurallar.md` / `<key>.yaml`), boyut bütçesi `BusinessRuleFingerprintManager.EnsureWithinBudget`
   kapısından geçer, yol `EnsureSourceIsAddressable` ile kök dışına çıkamaz.
5. `GET api/test-module/authoring/profile-packs` — yüklü paketleri ve kapsama raporunu listeler.
6. Yeni izin ailesi: `TestModulePermissions.Bridge.ManageSources`.

**Kabul:** `kurallar.md` uçtan yüklendiğinde host yeniden derlenmeden hem MCP Resource'u hem
`rules_fingerprint` **aynı** içeriği görüyor; bunu doğrulayan test var.

**Commit:** `#KBP-112 feat: created the single authoring source of truth and its upload surface`

---

### Dilim 4 — Yazarlık oturumunun ajana açılması (≈12 dosya)

**Kapattığı:** madde 7. Ajan döngüsü kapanır.

`ProtocolMax` **değişmez**, yeni tool **açılmaz**. Oturum yeteneği `ptn_ground`'un arkasına bağlanır:

1. `GroundRequestDto` opsiyonel `SessionId` ve opsiyonel tek `ProposedStep` alanı kazanır.
   `SessionId` boşsa bugünkü davranış birebir korunur; doluysa oturum okunur, kapalı soruların
   cevabı bağlama katılır.
2. `ProposedStep` doluysa `AuthoringSessionManager` adımı **mekanik olarak** oturumdaki belgeye
   ekler — bugünkü `POST sessions/{id}/step` ile **aynı** Manager yolu; ikinci bir birleştirme
   kuralı yazılmaz.
3. `GroundResultDto` `SessionId`, güncel adım sayısı ve bekleyen kapalı soruları döndürür.
4. `AgentProfileManager`'ın an profili değişmez; `ptn_ground` zaten `Active` kümesinde.
5. Serbest metin operasyon/tablo/kolon adı **taşınmaz** — aday listesi kapalı küme kalır (RULE-0007).

> [!WARNING] Alternatif yol ADR ister
> Ayrı `ptn_session` / `ptn_step` tool'ları açmak `ProtocolMax`'ı 12'den büyütür ve
> ADR-0023 §C'nin *"ajanın tek erişim yolu `/mcp`"* + RULE-0007 tool bütçesini yeniden açar.
> Bu yolu daha doğru buluyorsan **dur ve sor**; kendi başına açma.

**Kabul:** Ajan tek MCP tool'uyla oturum açıp, kapalı soruyu alıp, cevabı geçirip, tek adım
önerip belgeye eklenmesini görebiliyor. `PackageBoundaryTests` yeşil, tool sayısı **değişmedi**.

**Commit:** `#KBP-112 feat: created the authoring session path behind the existing ground tool`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **Dilim 4** devredilir — tek başına anlamlıdır ve InventoryTrackingAutomation'a
karşı ilk koşumu bloke etmez.
**Kesilmeyecekler: 1, 2, 3.** Dilim 1 olmadan hiçbiri doğrulanamaz; 2 olmadan korumalı uç
koşulamaz; 3 olmadan mühür yanlış bayta bağlanır.

---

## 5. Yasaklar

1. `.NET` tarafına model istemcisi getirme (`IChatClient`, Ollama, OpenAI, Gemini).
2. Yeni MCP tool açma; `ProtocolMax`'ı sessizce büyütme — §2.2.
3. Ortam bağlaması için tablo/entity/migration açma; ADR-0016'nın 4 tablo modelini bozma.
4. `abp.AbpSettings` migration'ını doğrulama yapmadan üstlenme — §2.2.
5. ABP'nin kendi setting-management uçlarına paralel bir ayar CRUD'u yazma.
6. Sır değerini DTO, log, `RunnerRef`, exception `WithData`, test fixture veya **HAR**'a yazma.
7. Checker tablosunu okuma, checker'a FK verme, ortak transaction açma.
8. Yükleme ucunda serbest dosya adı veya kök dışına çıkabilen yol kabul etme.
9. Belirsizliği tahminle kapatma; cevapsız belirsizliği yayına geçirme (RULE-0006, RULE-0007).
10. Application servisine private iş metodu veya guard koyma (`ServiceShapeTests`).
11. `Domain/Managers/**` içine `Process`/`File`/`Directory` yazma.
12. Yeni proje, yeni katman, `Infrastructure/`, `Helpers/`, `Utils/` açma.
13. Rota, izin, hata kodu, ayar adı, docker bayrağı için inline string yazma.
14. `ptn-test-agent/` altına dokunma; `docs/` değişikliğini ürün commit'ine karıştırma.
15. Kırılan testi silme, `Skip` etme, assertion zayıflatma.
16. Ara dilimlerde build/test atlama; başarısız kapıdan sonra sonraki dilime geçme.

---

## 6. Kabul kriterleri

- `AbpSettingManagement` compose edildi; `AbpSettings` sahipliği **doğrulandı ve raporlandı**.
- Ortam bağlaması uçtan yazılıyor, okunuyor, siliniyor; `environmentKey` eşleşme kapısı negatif testte tutuyor.
- Koşum girdilerine API sırrı giriyor; sırrın DTO/log/HAR/`RunnerRef`'e sızmadığını kanıtlayan test var.
- HAR'da `Authorization` başlığı maskeleniyor.
- `RunnerNetworkMode`/`RunnerExtraHosts` docker argümanlarına geçiyor; ayar boşken argüman listesi **birebir eskisi**.
- `kurallar.md` tek kaynaktan okunuyor; gömülü kaynak kaldırıldı; MCP Resource ile `rules_fingerprint` aynı baytı görüyor.
- Profil paketi yolu çözülüyor; yükleme ve listeleme uçları çalışıyor; kök dışı yol reddediliyor.
- `ptn_ground` oturum kimliğiyle çağrılabiliyor ve tek adım öneriyi belgeye ekliyor.
- **MCP tool sayısı değişmedi**; `PackageBoundaryTests` ve `ProtocolMax` testi yeşil.
- **Test Module'de migration üretilmedi** (veya §2.2 doğrulaması aksini gösterdi ve raporlandı).
- `dotnet build Ptn.TestModule.slnx -m:1 -c Release` → **0 hata**.
- `dotnet test --filter "Category!=LiveInfrastructure"` → 0 başarısız (taban **337**).
- `dotnet test --filter "Category=LiveInfrastructure"` → hâlâ yeşil (taban 2/2).

**Beklenen uç sayısı: 58 → 66** (ortam 3, kaynak 3, ABP setting-management uçları hariç tutulursa).

---

## 7. Bitiş

1. §5'in 16 maddesini kendi kodunda tek tek kontrol et.
2. Dört dilimi sırayla commit et; her dilim kendi build/test kapısını geçmeden sonrakine geçme.
3. Tek sefer: Test Module build → iki filtreli `dotnet test`.
4. `/backend-verify` gate'i; her commit öncesi başlık doğrulayıcısını koştur:
   `check-backend-diff.ps1 -CommitMessage "<subject>"`.
5. Raporda **zorunlu**: öncesi/sonrası uç sayısı; `AbpSettings` sahiplik doğrulamasının sonucu;
   sırrın sızmadığını gösteren testin adı; HAR maskeleme kanıtı; docker argüman listesinin
   ayar boşken değişmediğinin kanıtı; profil paketi yolu için seçtiğin çözüm; her varsayım.

---

## 8. Kapattığı borç

| Kayıt | Madde |
|---|---|
| Boşluk taraması B1 | Ortam bağlaması yazma ucu |
| Boşluk taraması B2 | `AbpSettingManagement` kompozisyonu |
| Boşluk taraması C-bloker 1 | Runner kimlik kanalı |
| Boşluk taraması C-bloker 2 | Runner ağ sınırı |
| Boşluk taraması A3 / B3 / B4 | İş kuralı ve profil paketi kaynak tekliği |
| Boşluk taraması A1 | Ajanın adımı sunucuya yazamaması |
| `UI-Backend-Controller-Catalog` | `abp.AbpSettings` yokluğundan gelen Swagger 500'ü |

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| TS ajanının kendi `SessionStore`'unu sunucu oturumuna indirgemesi | **Ajan deposu** — `ptn-test-agent`, yapay zekâ geliştiricisine devredilir |
| `ptn-test-agent` birim/entegrasyon testleri (`tests/` klasörü yok) | Aynı devir |
| InventoryTrackingAutomation için spec snapshot, DB connection, Vault secret kaydı | **Kurulum turu** — kod değil, `docker pull redocly/cli:2.14.0` ile başlar |
| Canlı altı-an koşumu ve yeşil koşum kanıtı | **KBP-115** — canlı altyapı smoke |
| Finding detay ucu, `AbpPermissionManagement` kompozisyonu | Ölçüldüğünde |
| İki checker'ın `/api/lookups/difference-kinds` route çakışması | Ayrı görev — checker depoları |
| `POST /api/emailing/emails` authorization metadata'sı | Ayrı görev — Emailing sahibi |
