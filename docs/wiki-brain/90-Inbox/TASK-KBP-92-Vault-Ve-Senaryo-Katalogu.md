# AJAN GÖREVİ — KBP-92 · Vault kompozisyonu ve senaryo kataloğu

İki bölümlü tek görev. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

**Bölüm A (Vault)** host kompozisyonudur, küçüktür ve hiçbir domain dosyasına dokunmaz.
**Bölüm B (senaryo kataloğu)** modülün ilk gerçek iş aggregate'idir.
İkisi tek dosyada bile kesişmez; A önce yapılır çünkü tek başına derlenebilir ve
uzun süren B'nin altında kalmaması gerekir.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-92   (KBP-91 üzerinden)
Motor   : PostgreSQL
Commit  : #KBP-92 <type>: <past-tense English description>
```

| Ön koşul | Durum |
|---|---|
| **KBP-91 commit edilmiş ve build/test yeşil** | **Zorunlu.** KBP-91 `common.props` ve host csproj'unu değiştiriyor; Bölüm A aynı iki dosyaya dokunur. Kirli ağaç üzerinde başlanmaz |
| `CheckNexus.Vault 0.2.0-alpha.2` nuget.org'da | ✅ 2026-08-14'te yayımlandı |
| **KBP-714** (şema parmak izi) | Bölüm B'nin **yayın kapısı 4**'ü için gerekli — §B.6'ya bak |

Derlenebilir dilimler, **en fazla 5 commit** (A için 1, B için ≤4), testler son dilimde.

> **Numaralandırma uyarısı.** Wiki'de `KBP-92` iki farklı işe atıfta bulunuyor:
> `PLAN-0004 Bölüm C` *"DMN task'ında (KBP-92)"*, `AUDIT-0001 BULGU-05` *"Test Module F3
> (KBP-92)"*. İkisi de artık geçersizdir: `KBP-92` bu görevdir. DMN derleyicisi ve ortam
> eşleşmesi ileri numaralara kayar. Bu satır silinmez; iki belge düzeltilecektir.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek — **bu depoda**) |
|---|---|---|
| Host module kompozisyonu | `layers-and-files.md` | `host/Ptn.TestModule.HttpApi.Host/TestModuleHttpApiHostModule.cs` |
| Entity (veri kabuğu) | `house-profile.md` → *Entity data shell* | `src/Ptn.TestModule.Domain/Entities/Lookups/TestOutcomeStatus.cs` |
| Manager | `house-profile.md` → *Base classes* | `src/Ptn.TestModule.Domain/Managers/Bridge/ProfilePackManager.cs` |
| Repository | `data-access.md` | `src/Ptn.TestModule.EntityFrameworkCore/Repository/Lookups/**` |
| EF Configuration | `data-access.md` | `src/Ptn.TestModule.EntityFrameworkCore/Configurations/Lookups/LookupEntityConfigurationBase.cs` |
| AppService | `house-profile.md` → *Contracts live in Application.Contracts* | `src/Ptn.TestModule.Application/Services/Bridge/DatabaseOracleAppService.cs` |
| DTO / Validator / Mapper | `mapping.md` | `src/Ptn.TestModule.Application.Contracts/**/Bridge/**` |
| Controller | `layers-and-files.md` → *Controller* | `src/Ptn.TestModule.HttpApi/Controllers/Bridge/**` |
| Sabit / kod kümesi | `house-profile.md` → *Stable strings* | `src/Ptn.TestModule.Domain.Shared/Constants/Runs/**` |

**Kanonik kararlar:** `ADR-0016` (veri modeli — B'nin anayasası), `ADR-0020` (malzeme mührü),
`ADR-0014 §C/§D` (iki belge, dört kapı), `RULE-0006` (yayın kapısı), `RULE-0002`
(şema/migration), `ADR-0004` + `RULE-0003` (Vault sınırı).

**Şema kaynağı:** `docs/wiki-brain/04-Architecture/Test-Platform-Schema.dbml` →
`test_catalog.test_scenarios`. Kolon adları, tipler, uzunluklar ve iki unique indeks
**oradan alınır, uydurulmaz.**

---

# BÖLÜM A — Vault kompozisyonu

## A.1 Ne yapıyor

`CheckNexus.Vault` bir Vault sunucusu değil, composition host içinde çalışan HashiCorp
KV v2 adapteridir (`CURRENT-0003`). Bugüne kadar **hiçbir hostta compose edilmedi** —
iki checker'ın ince hostlarında da yok, doğrulandı. Test Module ilk tüketicidir.

`Roadmap`'te açık duran madde: *"[ ] `CheckNexus.Vault` modülünü ekle ve iki portun aynı
singletona çözüldüğünü doğrula."* `CURRENT-0004` consumer kabul kapısı madde 8:
*"Checker secretları tek Vault adapterinden çözülür."*

## A.2 Paketin gerçek sözleşmesi — koddan okundu

`CheckNexusVaultModule.ConfigureServices` şunları kuruyor:

- `VaultOptions`'ı `"Vault"` section'ından bağlar (`VaultOptions.SectionName`)
- `VaultOptionsValidator`'ı `IValidateOptions<VaultOptions>` olarak kaydeder ve
  `ValidateOnStart()` ile **fail-fast** yapar
- `VaultConstants.HttpClientName` adlı named HTTP client'ı ekler
- `VaultSecretProvider`'ı **singleton** kaydeder
- **Aynı instance'ı** iki checker portuna bağlar:
  `Ptn.ApiContractChecker.Interface.Secrets.ISecretProvider` ve
  `Ptn.DatabaseChecker.Interface.Secrets.ISecretProvider`

`VaultOptions` alanları: `Address` (zorunlu), `Mount` (varsayılan `pintern-dev`),
`AuthenticationMode` (`Token` | `AgentProxy`), `Token`, `TokenFile`, `Namespace`,
`RequestTimeoutSeconds` (varsayılan 10).

## A.3 Dokunulacaklar (≈6)

| # | Dosya | Değişiklik |
|---|---|---|
| 1 | `common.props` | `<CheckNexusVaultVersion>0.2.0-alpha.2</CheckNexusVaultVersion>` — KBP-91'in açtığı iki değişkenin **yanına** |
| 2 | `host/.../Ptn.TestModule.HttpApi.Host.csproj` | `CheckNexus.Vault` `PackageReference`, capability paketleri ItemGroup'una |
| 3 | `host/.../TestModuleHttpApiHostModule.cs` | `DependsOn` listesine `typeof(CheckNexusVaultModule)` |
| 4 | `host/.../appsettings.json` | `Vault` section'ı — **secret değeri yok** |
| 5 | `host/.../appsettings.secrets.json` | Yalnız yapı; gerçek token **yazılmaz** |
| 6 | `test/.../VaultCompositionTests.cs` | İki portun aynı singleton'a çözüldüğü |

`appsettings.json`'a girecek olan (RULE-0003: secret değeri **koda ve config'e yazılmaz**):

```json
"Vault": {
  "Address": "http://127.0.0.1:8200",
  "Mount": "pintern-dev",
  "AuthenticationMode": "Token",
  "RequestTimeoutSeconds": 10
}
```

`Token` alanı `appsettings.json`'a **konmaz**. Yerel geliştirmede user-secrets veya
`TokenFile`; production'da `AgentProxy`. Bu iki cümle `PACKAGE-README` değil, host
`appsettings.json`'ının üstündeki yorumdur.

## A.4 Yasaklar (A)

1. Gerçek token, parola veya `Address` dışında bir secret değerini config'e/koda yazma.
2. `VaultOptions`'ı host tarafında yeniden tanımlama veya section adını sabit olarak
   kopyalama — `VaultOptions.SectionName` paketin sahipliğindedir.
3. Test Module'de ikinci bir `ISecretProvider` implementasyonu kaydetme.
4. Vault çağrısını domain/manager katmanına sızdırma — checker'lar kendi portlarını
   kendileri çağırır; Test Module yalnız **compose eder**.

## A.5 Kabul kriterleri (A)

- Host module graph'ı `CheckNexusVaultModule` ile açılıyor.
- İki checker `ISecretProvider` portu **aynı** `VaultSecretProvider` örneğine çözülüyor
  (testle kanıtlı — `CURRENT-0004` kapı 8).
- `Vault:Address` eksikken host **fail-fast** kapanıyor (`ValidateOnStart`).
- Hiçbir dosyada secret değeri yok; `appsettings.secrets.json` boş yapı.
- Swagger'da yeni uç görünmüyor — Vault yüzey açmaz, port doldurur.

**Commit:** `#KBP-92 feat: created the shared vault composition in the test module host`

---

# BÖLÜM B — Senaryo kataloğu (`test_catalog.test_scenarios`)

## B.1 Ne yapıyor

Modülün ilk iş aggregate'i. **Her satır bir sürümdür**; ayrı "senaryo başlığı" tablosu
yoktur çünkü hiçbir yerden FK almıyor ve tuttuğu her alan türetilebilir (`ADR-0016`
alternatifler bölümü).

İki belge saklanır (`ADR-0014 §C`): `source_document` (ajanın yazdığı, **insanın
onayladığı**) ve `compiled_document` (runner'ın koştuğu). Onay `source_hash`'e bağlanır —
derleyici yarın değişirse eski koşu yeniden üretilebilir kalır.

## B.2 Entity — veri kabuğu

`Domain/Entities/Catalog/TestScenario.cs` · `AuditedAggregateRoot<Guid>`, `IMultiTenant`
(`ADR-0016 §D/§E`). Alanların **tamamı `internal set`**; metot, `if`, hesap **yok**.

DBML'den gelen alanlar: `ScenarioKey`, `VersionNo`, `Title`, `Description`, `StateId`,
`SourceDocument`, `SourceHash`, `CompiledDocument`, `CompiledHash`, `RulesFingerprint`,
`SpecSnapshotId`, **`SpecFingerprint`**, **`DbConnectionId`**, **`DbSchemaFingerprint`**,
**`ProfileFingerprint`**, `AssertionCount`, `DerivabilityCode`, `AuthoredByAgent`,
`AgentModelRef`, `ApprovedBy`, `ApprovedAt`, `ApprovalBoundToHash`, `Notes`.

Kalın olan dördü `ADR-0020 §A`'nın eklediği malzeme mührüdür. `SpecSnapshotId` ve
`DbConnectionId` **düz `uuid`'dir, FK değildir** — modüller arası anahtar yasağı
(`ADR-0015 §F`).

## B.3 Manager sahipliği

`Domain/Managers/Catalog/TestScenarioManager.cs` · `FoundationManager<TestScenario, Guid>`
(imza doğrulandı: `FoundationManager<TEntity, TKey> : DomainService`).

Sahip olduğu iş: normalizasyon, sürüm numarası üretimi, `(scenario_key, version_no)` ve
`(scenario_key, source_hash)` benzersizliği, durum geçişi
`Draft → PendingApproval → Published → Deprecated`, onayın içerik hash'ine bağlanması.

`FoundationManager`'ın **hazır verdiklerini yeniden yazma**: `EnsureExistsAsync`,
`EnsureAllExistAsync`, `EnsureUniqueAsync`, `EnsureUniqueValuesAsync`,
`EnsureDistinctValues`, `NormalizeRequiredText`, `NormalizeOptionalText`,
`EnsureEnumDefined`, `AlreadyExistsErrorCode`.

## B.4 Yayın kapısı — ayrı manager

`Domain/Managers/Catalog/ScenarioPublicationGateManager.cs` **beş** kapıyı sırayla
değerlendirir ve `TestScenarioPublishDecision` döndürür (`IsPublishable`,
`FailedGateCodes[]`, `Warnings[]`):

| # | Kapı | Kaynak |
|---|---|---|
| 1 | Şema geçerliliği | `ADR-0014 §D` |
| 2 | Türetilebilirlik | `RULE-0006` |
| 3 | `AssertionCount > 0` | `RULE-0006` |
| 4 | **Malzeme bütünlüğü** — dört mühür dolu | `ADR-0020 §B/4` |
| 5 | **`sourceDescriptions` ↔ `SpecSnapshotId` tutarlılığı** | `ADR-0020 §B/5` |

Kapı düşerse `FailedGateCodes` dolar ve `Published`'a **geçilmez**. Kapı gevşetilmez,
uyarıya indirilmez.

## B.5 Katman zinciri

`Controller → AppService → Manager → Repository`. AppService
`BaseApplicationService<TestScenario, Guid, TestScenarioDto, TestScenarioListInput,
CreateTestScenarioDto, UpdateTestScenarioDto, TestScenarioCreateModel,
TestScenarioUpdateModel, TestScenarioManager, ITestScenarioRepository>` türer — **generic
sırası birebir budur**, upstream kaynaktan doğrulandı.

Repository `BaseEfCoreRepository<TestModuleDbContext, TestScenario, Guid>`; arayüzü
`IBaseRepository<TestScenario, Guid>` türer ve yalnız gerçek ihtiyacı ekler:
`FindLatestVersionAsync`, `FindPublishedAsync`, `GetNextVersionNoAsync`.

## B.6 KBP-714 bağımlılığı — sessizce geçilmeyecek madde

Kapı 4 dört malzemenin **mührünün dolu** olmasını şart koşar. `db_schema_fingerprint`'i
üretecek yüzey **bugün Database Checker'da yoktur** (doğrulandı: `SchemaFingerprint` araması
sıfır sonuç). `ADR-0020` risk tablosunun *"şema mührü zaten `GetSchemaFingerprintAsync`'te"*
satırı **yanlıştır**; `KBP-714` tam olarak bunu kapatıyor.

Sonucu açıkça yazılmalıdır: **KBP-714 inmeden, DB malzemesi taşıyan hiçbir senaryo
`Published` olamaz.** Bu bir hata değil, `ADR-0020 §B/4`'ün kendisidir — ve KBP-714'ün
neden kritik yolda olduğunun kanıtıdır.

**Uydurma çözüm yasak:** mührü boş geçme, sabit değer yazma, kapıyı uyarıya indirme,
"DB'ye dokunmayan senaryo" istisnası **icat etme**. Son madde meşru bir soru olabilir ama
`ADR-0020`'de yoktur; gerekiyorsa **ADR ile açılır**, kodda sessizce doğmaz.

## B.7 Dosyalar (≈29)

`Domain/Entities/Catalog/` → `TestScenario`
`Domain/Models/Catalog/` → `TestScenarioCreateModel`, `TestScenarioUpdateModel`,
`TestScenarioPublishModel`, `TestScenarioMaterialSeal`, `TestScenarioPublishDecision`
`Domain/Interface/Catalog/` → `ITestScenarioRepository`
`Domain/Managers/Catalog/` → `TestScenarioManager`, `ScenarioPublicationGateManager`
`Domain.Shared/Constants/Catalog/` → `TestScenarioConsts` (uzunluklar, hash = 64),
`Lookups/ScenarioGateCodes`; `ExceptionCodes/Catalog/TestModuleScenarioErrorCodes`
`EntityFrameworkCore/Configurations/Catalog/` → `TestScenarioConfiguration`
(iki unique indeks, `HasMaxLength` sabitlerden, `Restrict`)
`EntityFrameworkCore/Repository/Catalog/` → `EfCoreTestScenarioRepository`
`EntityFrameworkCore/` → `TestModuleDbContext` (DbSet) + migration
`Application.Contracts/Dtos/Catalog/` → 7 DTO
`Application.Contracts/Services/Catalog/` → `ITestScenarioAppService`
`Application.Contracts/FluentValidation/Catalog/` → 4 validator
`Application.Contracts/Permissions/` → `Scenarios.Default|Create|Update|Delete|Publish|Approve`
`Application/Services/Catalog/` → `TestScenarioAppService`
`Application/Mappers/Catalog/` → `TestScenarioMapper` (yalnız partial bildirimler)
`HttpApi/Controllers/Catalog/` → `TestScenarioController`

**Migration:** `dotnet ef migrations add TestScenarioCatalog` — yalnız `test_catalog`.

> **Dosya bütçesi.** A (≈6) + B (≈29) = **≈35**, sınırın tam üstünde. Aşarsan **B'nin
> listesinin sonundan** kes (controller ve rapor uçları) ve bir sonraki task'a devret;
> hiçbir dosyayı yarım bırakma.

## B.8 Yasaklar (B)

1. `test_scenario_versions` veya ikinci bir başlık tablosu **açma** (`ADR-0016`).
2. Checker tablosuna FK verme; `SpecSnapshotId`/`DbConnectionId` düz `uuid`.
3. Entity'ye metot, `if`, normalizasyon, geçiş, `throw` koyma.
4. Yayın kapısını gevşetme, uyarıya indirme, mührü sabitle doldurma.
5. `Published` durumuna yazan bir tool/uç ajana açma (`RULE-0005` — kademe 4).
6. Senaryoyu silme yolu açma — `Restrict`; `DeleteAsync` reddeder.
7. Enum kullanma; durum `test_scenario_states` lookup'ından gelir.
8. `[MapProperty]`, mapper'da gövde, serviste `private` iş metodu, nested tip.
9. Yeni katman/klasör (`Helpers/`, `Engines/`, `Infrastructure/`).
10. Ara dilimlerde build/test; geçmiş commit arkeolojisi.

## B.9 Testler (son dilim)

| Test | Doğruladığı |
|---|---|
| `ScenarioPublicationGateTests` | Türetilemeyen assertion → `Published` olamıyor |
| `ScenarioPublicationGateTests` | `AssertionCount = 0` → reddediliyor |
| `ScenarioPublicationGateTests` | **Dört malzemeden biri eksik → reddediliyor** |
| `ScenarioPublicationGateTests` | `sourceDescriptions` başka spec'e işaret ediyor → reddediliyor |
| `ScenarioPublicationGateTests` | Kapı düşünce `FailedGateCodes` **hangi kapı** olduğunu taşıyor |
| `TestScenarioManagerTests` | Onay `ApprovalBoundToHash`'e bağlı; belge değişince geçersiz |
| `TestScenarioManagerTests` | İki unique indeks çalışıyor; sürüm numarası artıyor |
| `TestScenarioManagerTests` | `DeleteAsync` reddediyor (`Restrict`) |
| `ScenarioTenancyTests` | Başka kiracının senaryosu görünmüyor |
| `MigrationScopeTests` (mevcut) | Migration yalnız `test_catalog`'a dokunuyor |

## B.10 Kabul kriterleri (B)

- `test_scenarios` tablosu DBML'deki kolon, uzunluk ve iki unique indeksle oluşuyor.
- Beş yayın kapısı sırayla çalışıyor; düşen kapı kodla raporlanıyor.
- Onay içerik hash'ine bağlı; belge değişince onay geçersiz.
- Senaryo silinmiyor; `IMultiTenant` dört tabloda da kuralına uygun.
- Migration yalnız `test_catalog` şemasına dokunuyor.
- Lookup CRUD'ı gibi taban sınıfın verdiği hiçbir gövde elle yazılmamış.

**Commit:** `#KBP-92 feat: created the scenario catalog aggregate with material sealing and publication gates`

---

## 2. Bitiş

1. A ve B'nin yasak listelerini kendi kodunda tek tek kontrol et.
2. Son dilimi commit et.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` →
   `dotnet test Ptn.TestModule.slnx --no-restore`
4. `/abp-backend-dev` mimari incelemesi + `/backend-verify` gate'i.
5. Raporda: dosya listesi, migration adı, KBP-714 bağımlılığının koddaki karşılığı,
   yaptığın **her varsayım**.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → **bir kez** dene; aynı komutu döngüde tekrarlama;
tek engelde 10 dakikadan fazla harcama. Migration üretirken `--project` ve
`--startup-project` açıkça verilir.

---

## 3. Bu görevin kapattığı wiki borcu

| Kayıt | Madde | Bölüm |
|---|---|---|
| `Roadmap` | *"[ ] `CheckNexus.Vault` modülünü ekle ve iki portun aynı singletona çözüldüğünü doğrula"* | A |
| `CURRENT-0004` kapı 8 | *"Checker secretları tek Vault adapterinden çözülür"* | A |
| `AUDIT-0003 #12` | ADR-0020 malzeme mührü uygulanmamış | B |
| `PLAN-0003 TM-03` | `test_scenarios` tablosu | B |
| `PLAN-0003 TM-19` | Onay akışı, `approval_bound_to_hash` | B |
| `RULE-0006` | Yayın kapısının ilk gerçek uygulaması | B |

**Kapanmayan, bilinçli:** `TM-17` türetilebilirlik kapısının checker tarafı KBP-91'de
bağlandı; `RULE-0008` (DMN karar tablosu kapsamı) yazarlık hattı işidir ve ayrı task'tır;
`ADR-0020 §C` koşum anı kayma tespiti `test_runs` ile gelir.
