# AJAN GÖREVİ — KBP-117 · Backend kapanışı: yetki deposu, migration orkestrasyonu ve mühür kaynağı

> [!INFO] Numara kaydı
> `KBP-113` / `KBP-114` yakıldı (TASK-KBP-112 §Kapsam). `KBP-115` canlı altı-an smoke'udur.
> `KBP-116` yetki sınırı, fingerprint tel biçimi ve `SourceHash` sözleşmesini kapattı.
> **Bu ticket backend'in son kod borcudur.** Bittiğinde KBP-115 koşulabilir hâle gelir.
> TypeScript yazarlık ajanı (`ptn-test-agent`) hâlâ numarasızdır ve bu ticket'ın kapsamı değildir.

Tek görev, **üç derlenebilir dilim** ve **bir karar kapısı**. **Her dosyayı yazmadan önce
§1'deki kapıdan geç.**

KBP-116 MCP yüzeyini izne bağladı. Fakat bu host'ta **izin verilebilecek bir yer yok**:
`AbpPermissionManagement` compose edilmemiş. Yani bugün ajan, kapanan güvenlik açığının
diğer tarafında kilitli kalır. Aynı şekilde boş bir veritabanında `abp.*` tablolarını kimse
uygulamıyor. Bu görev backend'i **kurulabilir** hâle getirir.

**Bu görev .NET'e model getirmez** (ADR-0023, RULE-0005).

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module
Branch  : KBP-117   (KBP-116 üzerinden)
Motor   : PostgreSQL
Commit  : #KBP-117 <type>: <past-tense English description>
Hedef   : C:\Users\mertb\RiderProjects\InventoryTrackingAutomation  (ilk gerçek SUT)
```

| Ön koşul | Durum |
|---|---|
| KBP-116'nın beş commit'i dalda | ✅ `e5ba0f0` · `9fa6d7e` · `2662769` · `263eee0` · `c7b5208` |
| Ölçülen taban (2026-08-16, KBP-116 sonrası) | ✅ **64 uç** · Release **0 hata / 3 uyarı** · non-live **373/373** · live **2/2** · **8 migration** |
| `ptn-test-agent/` untracked | ⚠️ **Dokunma** |
| `docs/` ayrı Git deposu | ⚠️ Ürün commit'ine karıştırma |
| `ServiceShapeTests` · `ManagerReachabilityTests` · `ServiceContractTests` · `OutwardSurfaceTests` · `PackageBoundaryTests` · `ClientProxySurfaceTests` · `ToolCatalogTests` · `BridgeAuthorizationTests` | ⛔ hepsi yeşil kalacak |

**Dosya bütçesi ≈20.** Üç dilim, dilim başına bir commit.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| Host modül kompozisyonu | `infrastructure-bootstrap.md` | `host/TestModuleHttpApiHostModule.cs` → KBP-112'nin `AbpSettingManagement` üçlüsü (satır 66-69) |
| Migration/seed sınırı | `data-access.md` | `EntityFrameworkCore/TestModuleEntityFrameworkCoreModule.cs` → `MigrateAndSeedAsync` |
| Yetki testi | `verification.md` | `Application.Tests/Composition/BridgeAuthorizationTests.cs` · `TestBase/TestModuleTestBaseModule.cs` |
| Rota / izin / ayar sabiti | `house-profile.md` → *Stable string ownership* | `Domain.Shared/Constants/**` · `Domain.Shared/Permissions/**` |

**Kanonik kararlar:** ADR-0005 (Authenticator ayrı deploy), ADR-0012, ADR-0013, ADR-0016,
ADR-0020, **RULE-0002 (migration sahipliği)**, RULE-0005, RULE-0007.

---

## 2. Ölçülen boşluk — 2026-08-16, KBP-116 sonrası

### 2.1 Dört açık madde

| # | Ne | Bugünkü kanıt | Sınıf |
|---|---|---|---|
| **1** | **İzin verilebilecek yer yok** | `TestModuleHttpApiHostModule` `DependsOn` listesinde (satır 49-72) hiçbir `AbpPermissionManagement*` modülü yok. Buna karşılık `appsettings.json:38` `"Volo.Abp.PermissionManagement": "abp"` şema eşlemesini **zaten taşıyor** — yani şema yapılandırılmış, modül compose edilmemiş. KBP-116 Dilim 1 MCP'yi `Bridge.*` iznine bağladığı için ajan artık **hiçbir tool çağıramaz** | Kurulum blocker |
| **2** | **Davranışsal yetki testi yok** | `BridgeAuthorizationTests` bir **kaynak-metin taramasıdır**: `PtnBridgeAppService.cs`'i regex ile okuyup her gövdenin `await CheckPolicyAsync(` ile başladığını doğrular. İzinsiz kimliğin gerçekten reddedildiğini kanıtlamaz; `TestModuleTestBaseModule.cs:27` **`AddAlwaysAllowAuthorization()`** çağırdığı için tüm test süreci yetkiyi baypas eder. Ayrıca `Bridge_app_service_method_count_should_match_controller_authorize_count` adına rağmen controller'ı hiç okumaz, sabit `9` bekler | Test borcu |
| **3** | **`abp.*` tablolarını kimse uygulamıyor** | `TestModuleEntityFrameworkCoreModule.MigrateAndSeedAsync` yalnız `TestModuleDbContext.Database.MigrateAsync()` çağırır — sekiz Test Module migration'ı (`test_lookup`, `test_catalog`, `test_run`). `abp.AbpSettings` (KBP-112 ayar yüzeyi) ve `abp.AbpPermissionGrants` (madde 1) **Authenticator'ın DbContext'ine aittir** ve bu host'ta uygulanmaz. Boş veritabanında ayar ve izin yüzeyi çalışmaz | Kurulum blocker |
| **4** | **`SpecFingerprint` üreticisi yok** | `ScenarioPublicationGateManager.IsMaterialSealComplete` altı alanın hepsini dolu ister. `IApiOracleAppService`'te fingerprint okuyucu yok; `SnapshotOperationInventoryDto` yalnız `SnapshotId`/`OutcomeCode`/`TotalCount`/`IsComplete`/`Items` taşır. **Hiçbir senaryo `Published` olamaz** | **DUR VE SOR** |

### 2.2 Sabitlenen kararlar

- **`abp.AbpPermissionGrants` sahipliği `AbpSettings` ile aynı desendedir.** Önce
  `pintern-authenticator-latest-api` içinde migration'ı olduğunu doğrula (`AbpSettings` için
  `20260809140749_Initial.cs:165`'te bulunmuştu). **Varsa** bu modül yalnız compose eder,
  `ConfigurePermissionManagement()` çağırmaz, migration üretmez. **Yoksa dur ve raporla.**
- **Paralel bir izin CRUD'u yazılmaz.** `AbpPermissionManagementHttpApiModule` compose edilince
  `/api/permission-management/permissions` uçları gelir; `OutwardSurfaceTests` sayımına dâhil
  değildir (ABP'nin kendi uçları, `AbpSettingManagement` ile aynı muamele).
- **Migration orkestrasyonu Test Module'ün sorumluluğu değildir.** `MigrateAndSeedAsync`
  yabancı bir `DbContext`'i migrate etmez. Doğru çözüm **kurulum sırasıdır**: Authenticator
  aynı veritabanına önce deploy edilir. Bu görev bunu **belgeler ve başlangıçta doğrular**,
  sessizce üstlenmez (RULE-0002).
- **`AddAlwaysAllowAuthorization()` kaldırılmaz.** Mevcut 373 testin tamamı ona dayanıyor.
  Davranışsal yetki testi **kendi izole test modülünü** kurar.
- **Yeni MCP tool açılmaz**, `ProtocolMax` **12** kalır, `PtnToolCodes.cs`'e dokunulmaz.
- **Yeni entity, tablo, Test Module migration'ı, proje, katman yok.**
- **`NuGet.Config`'e dokunma** — §5 sahip eylemidir.

---

## 3. Dilimler

### Dilim 1 — İzin deposunun compose edilmesi (≈6 dosya)

**Kapattığı:** madde 1. Bu dilim olmadan ajan hiçbir MCP tool'u çağıramaz.

1. §2.2'nin `AbpPermissionGrants` sahiplik doğrulaması yapılır ve **sonucu rapora yazılır**.
2. `TestModuleHttpApiHostModule` `DependsOn` listesine `AbpPermissionManagementApplicationModule`,
   `AbpPermissionManagementEntityFrameworkCoreModule`, `AbpPermissionManagementHttpApiModule`
   eklenir — `AbpSettingManagement` üçlüsünün **hemen altına**, aynı yorum desenıyle.
3. Eksik paket varsa host `.csproj`'a eklenir ve gerekçesi yorum olarak yazılır
   (KBP-112'nin `Volo.Abp.SettingManagement.HttpApi` satırındaki desen). `common.props`'a
   yeni sürüm değişkeni **eklenmez**.
4. `ConfigurePermissionManagement()` **çağrılmaz**, migration **üretilmez**.
5. `appsettings.json:38`'deki mevcut şema eşlemesi **korunur**; yeni bölüm açılmaz.

**Kabul:** Host modül grafiği açılıyor; `OutwardSurfaceTests` **64**'te kalıyor (ABP'nin kendi
izin uçları sayıma girmez); migration sayısı **8**'de sabit.

**Commit:** `#KBP-117 feat: created the permission management composition`

---

### Dilim 2 — Davranışsal yetki kapısı (≈6 dosya)

**Kapattığı:** madde 2.

1. `Application.Tests` altında **izole bir test modülü** kurulur: `AddAlwaysAllowAuthorization()`
   çağırmayan, `ICurrentPrincipalAccessor`'ı izinsiz bir kimlikle dolduran bir startup modülü.
   Klasör precedent'i `Application.Tests/Composition/`'dır; yeni klasör açma.
2. Negatif test: izinsiz kimlik `PtnBridgeAppService.GroundAsync` çağırdığında
   `AbpAuthorizationException` alır.
3. Pozitif test: `Bridge.Ground` izni verilmiş kimlik aynı çağrıda yetki hatası **almaz**
   (girdi doğrulama hatası kabul edilebilir — yetki kapısının geçildiğini kanıtlar).
4. `Bridge_app_service_method_count_should_match_controller_authorize_count` düzeltilir:
   sabit `9` yerine `PtnBridgeController`'daki `[Authorize]` attribute sayısı reflection ile
   okunur ve AppService metot sayısıyla karşılaştırılır. Ad ile davranış eşleşmelidir.
5. Mevcut kaynak-tarama testi **silinmez**; şekil regresyonunu o korumaya devam eder.

**Kabul:** İzinsiz kimlik reddediliyor, izinli kimlik geçiyor; metot-sayısı testi artık
controller'ı gerçekten okuyor. Diğer 373 test etkilenmiyor.

**Commit:** `#KBP-117 test: created the negative authorization gate for the bridge surface`

---

### Dilim 3 — Kurulum ön koşulunun doğrulanması (≈6 dosya)

**Kapattığı:** madde 3. Kod yabancı migration uygulamaz; **eksikliği erken ve anlaşılır biçimde bildirir.**

1. `TestModuleEntityFrameworkCoreModule` başlangıç yolunda, `MigrateAndSeedAsync` **sonrasında**,
   beklenen ABP tablolarının varlığı kontrol edilir (`abp.AbpSettings`, `abp.AbpPermissionGrants`).
2. Eksikse **açık ve tek** bir başlangıç hatası atılır: hangi tablonun eksik olduğunu ve
   Authenticator migration'larının aynı veritabanına önce uygulanması gerektiğini söyleyen mesaj.
   Mesaj metni `Domain.Shared` sabitinde yaşar; inline string yazma.
3. Kontrol, mevcut `TestModuleConfigurationKeys` ailesine eklenen bir bayrakla kapatılabilir
   olmalıdır (varsayılan **açık**). Bayrak adı sabitler dosyasında yaşar.
4. Sorgu ham SQL ile `information_schema`'ya bakar ve **EF Core provider sınırında** kalır —
   `Domain/Managers/**` içine yazma.
5. `README` veya host `appsettings` örneğine kurulum sırası **yazılmaz**; bu bilgi wiki'nin işidir.

**Kabul:** Authenticator tabloları olmayan boş bir veritabanında host, anlaşılır tek bir hata
ile durur; tablolar varken bugünkü davranışı birebir korur.

**Commit:** `#KBP-117 feat: created the startup guard for the shared abp schema`

---

### Dilim 4 — `SpecFingerprint`'in checker'dan okunması (≈8 dosya)

**Kapattığı:** madde 4. Yayın kilidini açan dilim budur.

> [!IMPORTANT] Bu madde önce "karar kapısı" diye kaydedilmişti — **yanlıştı**
> Kaynak checker'da zaten public. İlk tarama yalnız Test Module'ün dar tüketici portu
> `IApiOracleAppService`'e bakmış, checker'ın kendi snapshot servisini atlamıştı. Ürün kararı
> gerekmiyor; bu bir kod dilimidir.

1. Kaynak: `ISpecSnapshotAppService.GetAsync(snapshotId)` → `SpecSnapshotDetailDto.SpecContent`.
   Kullanılacak alan **`CanonicalHash`**'tir (biçim gürültüsü elenmiş kanonik metnin SHA-256'sı),
   `RawHash` değil — biçim değişikliği mühür kaymasına yol açmamalıdır. Seçimi rapora yaz.
2. `TestScenarioManager`'a `ApplySpecFingerprint(seal, fingerprint)` eklenir —
   `ApplyRulesFingerprint` ile **birebir aynı şekilde**: prefiks her iki tarafta soyulur, istemci
   değer taşıyorsa eşleşme şartı aranır, eşleşmezse `TestModuleScenarioErrorCodes.InvalidHash`.
3. `TestScenarioAppService.CreateEntityAsync` / `UpdateEntityAsync`, `SpecSnapshotId` doluysa
   snapshot'ı okur ve değeri Manager'a verir — `DbSchemaFingerprint` bloğunun aynısı.
   **Port okuması AppService'te, karşılaştırma Manager'da.**
4. Koşum tarafı: `TestRunAppService` bugün `SpecFingerprint`'i **çağırandan** alıyor
   (`TestRunAppService.cs:210`). Aynı kaynaktan sunucu tarafında çözülür ki
   `RunMaterialDriftManager` gerçek kaymayı görsün. Çağıranın gönderdiği alan
   **kaldırılmaz**; doluysa eşleşme şartı aranır.
5. Snapshot bulunamazsa alan **boş bırakılır** ve `MaterialIntegrity` dürüstçe düşer;
   uydurma değer yazılmaz (ADR-0020).
6. Testler: sunucu değeri dolduruyor · prefiksli/prefikssiz eşleşen istemci değeri kabul ediliyor ·
   bayat değer `InvalidHash` alıyor · snapshot yokken alan boş kalıyor.

**Kabul:** Mühürde yalnız `SpecSnapshotId`, `DbConnectionId` ve belge gönderen bir istemci
`Published` durumuna kadar gidebiliyor; `MaterialIntegrity` gate'i gerçek baytlarla geçiyor.

**Commit:** `#KBP-117 feat: created the spec fingerprint binding from the snapshot`

---

## 5. Sahip eylemleri — kod değil, karar

| # | Konu | Sahip |
|---|---|---|
| S1 | `SystemStandards.Abp.Authorization 1.0.0` hiçbir feed'de yayımlı değil; yerel cache'teki kopyanın kaynağı bir **yerel publish klasörü**. Temiz klon / CI / ikinci geliştirici NU1101 alır | Paket sahibi |
| S2 | Kök `NuGet.Config` düz metin `ClearTextPassword` taşıyor ve depoda izleniyor | Repo/güvenlik sahibi |
| S3 | RULE-0008 DMN satır kapsamı yayın şartı sayıyor; `ScenarioPublicationGateManager` beş gate çalıştırıyor, DMN ölçülmüyor | Ürün sahibi |
| S4 | ~~`SpecFingerprint`~~ — **kapandı**, karar değil kod: Dilim 4 | — |
| S5 | `KBP-112` ve `KBP-116` dalları `predev`'e merge edilmedi | Backend sahibi |

---

## 6. Yasaklar

1. `.NET` tarafına model istemcisi getirme.
2. Yeni MCP tool açma; `ProtocolMax`'ı büyütme; `PtnToolCodes.cs`'e dokunma.
3. Yeni entity, tablo, Test Module migration'ı, proje, katman, `Infrastructure/`, `Helpers/`, `Utils/` açma.
4. Yabancı bir `DbContext`'i migrate etme; `abp.*` tabloları için migration üretme (RULE-0002).
5. `ConfigurePermissionManagement()` veya `ConfigureSettingManagement()` çağırma.
6. ABP'nin izin/ayar uçlarına paralel CRUD yazma.
7. `AddAlwaysAllowAuthorization()`'ı kaldırma veya mevcut testleri ona bağlı olmaktan çıkarma.
8. `SpecFingerprint` için yerel bir fingerprint semantiği **uydurma**; değeri checker'ın
   `SpecContent.CanonicalHash`'inden oku — Dilim 4.
9. `NuGet.Config`'e dokunma.
10. Application servisine private iş metodu veya guard koyma (`ServiceShapeTests`).
11. `Domain/Managers/**` içine `Process`/`File`/`Directory` veya SQL yazma.
12. Rota, izin, hata kodu, ayar adı, tablo adı, mesaj metni için inline string yazma.
13. Belirsizliği tahminle kapatma (RULE-0006, RULE-0007).
14. `ptn-test-agent/` altına dokunma; `docs/` değişikliğini ürün commit'ine karıştırma.
15. Kırılan testi silme, `Skip` etme, assertion zayıflatma, testi "geçsin diye" mock'la baypas etme.
16. Ara dilimlerde build/test atlama; başarısız kapıdan sonra sonraki dilime geçme.

---

## 7. Kabul kriterleri

- `AbpPermissionManagement` compose edildi; `AbpPermissionGrants` sahipliği **doğrulandı ve raporlandı**.
- `ConfigurePermissionManagement()` çağrılmadı; **Test Module'de migration üretilmedi** (8'de sabit).
- İzinsiz kimlik `GroundAsync`'te `AbpAuthorizationException` alıyor; izinli kimlik yetki kapısını geçiyor.
- Metot-sayısı testi controller'daki `[Authorize]` sayısını gerçekten okuyor.
- Authenticator tabloları olmayan veritabanında host anlaşılır tek bir hatayla duruyor; tablolar varken davranış değişmiyor.
- `SpecFingerprint` snapshot'ın `CanonicalHash`'inden sunucuda doluyor; bayat istemci değeri `InvalidHash` alıyor; snapshot yokken alan boş kalıyor.
- **`MaterialIntegrity` gate'i gerçek baytlarla geçilebiliyor** — yayın kilidi açık.
- **Uç sayısı 64** (`OutwardSurfaceTests`); ABP'nin kendi izin/ayar uçları sayıma girmiyor.
- **MCP tool sayısı değişmedi**; `ToolCatalogTests` ve `PackageBoundaryTests` yeşil.
- `dotnet build Ptn.TestModule.slnx -m:1 -c Release` → **0 hata**.
- `dotnet test --filter "Category!=LiveInfrastructure"` → 0 başarısız (taban **373**).
- `dotnet test --filter "Category=LiveInfrastructure"` → yeşil (taban 2/2).

---

## 8. Bitiş

1. §6'nın 16 maddesini kendi kodunda tek tek kontrol et.
2. Üç dilimi sırayla commit et; her dilim kendi build/test kapısını geçmeden sonrakine geçme.
3. Tek sefer: Test Module build → iki filtreli `dotnet test`.
4. `/backend-verify` gate'i; her commit öncesi
   `check-backend-diff.ps1 -CommitMessage "<subject>"` koştur.
   **Not:** `TestScenarioManager.cs` üzerinde 13 adet `[ENTITY]` bulgusu **bilinen false
   positive**'dir — işaretlenen `Ensure*`/`Normalize*` metotları zaten Manager'ın içindedir.
   Yeni bulgu ekleyip eklemediğini bu sayıyla karşılaştır.
5. Raporda **zorunlu**: `AbpPermissionGrants` sahiplik doğrulamasının sonucu; negatif yetki
   testinin adı; başlangıç guard'ının hata mesajı sabiti; öncesi/sonrası uç ve test sayısı;
   scanner bulgu sayısı; §4 ve §5'in durumu; her varsayım.

---

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| Canlı altı-an koşumu ve ITA'ya karşı ilk yeşil koşum | **KBP-115** — bu ticket bitince |
| ITA için spec snapshot, DB connection, Vault secret kaydı; `docker pull redocly/cli:2.14.0` | **Kurulum turu** — kod değil |
| TypeScript yazarlık ajanı ve eval harness'ı | **Numarasız** — ürün sahibi numara verir |
| `SpecFingerprint` kaynağı, RULE-0008 DMN gate'i | §4 ve §5 |
| Finding detay ucu, UI | Ölçüldüğünde |
