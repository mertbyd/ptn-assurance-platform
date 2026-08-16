---
id: GUIDE-0003
type: guide
status: active
title: NuGet package family release playbook
updated: 2026-08-16
decision_refs:
  - ADR-0006
  - ADR-0009
  - ADR-0010
  - ADR-0012
rule_refs:
  - RULE-0001
---

# NuGet paket ailesi güncelleme ve yayın playbook'u

> [!IMPORTANT] HER PAKET GÜNCELLEMESİNDE ÖNCE BU SAYFAYI AÇ
> Yayınlanan `PackageId + Version` değiştirilemez. Aynı sürümle farklı binary
> üretilmez. Script bilinmeyen çalışma dizininden `-File .\scripts\...` ile çağrılmaz;
> önce mutlak yol çözülür ve `Test-Path` ile doğrulanır. İlk koşu her zaman `-Push` olmadan yapılır.

## Hatasız komut kalıbı

Az önce görülen `-File .\scripts\... does not exist` hatasının nedeni PowerShell'in
`C:\Users\mertb` altında olması, scriptin ise repository içinde bulunmasıydı. Geçerli
kalıp çalışma dizininden bağımsızdır:

```powershell
$repositoryRoot = "C:\Users\mertb\RiderProjects\ptn-assurance-platform"
$releaseScript = Join-Path $repositoryRoot "scripts\publish-checkers.ps1"
$nextVersion = "0.2.0-alpha.2" # Yalnız örnek; registry ve değişiklik türüne göre seç.

if (-not (Test-Path -LiteralPath $releaseScript -PathType Leaf)) {
    throw "Release script bulunamadı: $releaseScript"
}

# 1. Zorunlu dry run: restore + build + test + pack + içerik kontrolü, push yok.
powershell -NoProfile -ExecutionPolicy Bypass `
    -File $releaseScript `
    -Version $nextVersion

# 2. Ancak dry run yeşil ve yayın açıkça onaylıysa:
powershell -NoProfile -ExecutionPolicy Bypass `
    -File $releaseScript `
    -Version $nextVersion `
    -Push
```

Tek checker ailesi için `-Family ApiContracts` veya `-Family DatabaseComparison`
eklenebilir. Relative komut ancak önce repository köküne `Set-Location -LiteralPath` ile
geçilmiş ve script doğrulanmışsa kullanılır.

### Hazır mutlak-yol çağrıları

Aşağıdaki çağrılar PowerShell hangi klasörde açılırsa açılsın aynı scripti bulur. `<YENİ-SÜRÜM>`
yerine registry'de bulunmayan, değişiklik türüne uygun yeni sürüm yazılır. Önce ilk komut
çalıştırılır; aynı komuta `-Push` ancak dry run yeşil kaldıktan ve yayın ayrıca onaylandıktan
sonra eklenir.

```powershell
# API Contracts — key prompt: CheckNexus.ApiContracts*
powershell -NoProfile -ExecutionPolicy Bypass `
  -File "C:\Users\mertb\RiderProjects\ptn-assurance-platform\scripts\publish-checkers.ps1" `
  -Family ApiContracts -Version "<YENİ-SÜRÜM>"

# Database Comparison — key prompt: CheckNexus.DatabaseComparison*
powershell -NoProfile -ExecutionPolicy Bypass `
  -File "C:\Users\mertb\RiderProjects\ptn-assurance-platform\scripts\publish-checkers.ps1" `
  -Family DatabaseComparison -Version "<YENİ-SÜRÜM>"

# Notifications — key prompt: Pintern.SaaS.Notifications.*
powershell -NoProfile -ExecutionPolicy Bypass `
  -File "C:\Users\mertb\RiderProjects\pintern-notifications\scripts\publish-notifications.ps1" `
  -Version "<YENİ-SÜRÜM>"

# Authenticator — key prompt: Authenticator.*
Set-Location -LiteralPath "C:\Users\mertb\RiderProjects\pintern-authenticator-latest-api"
powershell -NoProfile -ExecutionPolicy Bypass `
  -File "C:\Users\mertb\RiderProjects\pintern-authenticator-latest-api\scripts\publish-authenticator-packages.ps1" `
  -Version "<YENİ-SÜRÜM>"
```

İki checker aynı sürümde birlikte yayımlanacaksa `-Family` verilmez. Script iki aileyi
dependency sırasıyla doğrular ve push aşamasında **API Contracts**, ardından **Database
Comparison** için iki ayrı, adı açıkça yazılan API key ister.

## Değişmez release sırası

1. Gerçek repository kökünü, solution'ı, central props'u ve tam `PackageId` listesini bul.
2. NuGet.org'da her PackageId'nin son sürümünü registry API üzerinden doğrula.
3. Değişen içerik için yeni SemVer seç; yayımlanmış sürümü asla yeniden kullanma.
4. `common.props`/merkezi props üzerinde package metadata, PackageValidation baseline,
   repository URL, SourceLink, CI build ve `.snupkg` kapılarını doğrula.
5. Explicit source ile restore, Release build ve tüm testleri çalıştır.
6. Alpha için en az module initialization + route/Swagger + EF model ince-host smoke'u çalıştır.
7. Solution'ı değil, manifestteki tam paket projelerini dependency sırasıyla pack et.
8. Tam `.nupkg`/`.snupkg` sayısını; nuspec ID/version/repository/README/dependency bilgilerini;
   host/test/appsettings sızıntısını denetle.
9. `-Push` olmadan kanıtı kaydet.
10. Push öncesi exact version preflight yap. Aile bazında `evet` iste, sonra doğru aile adı
    ve glob ile `SecureString` API key sor.
11. Yayın sonrası bütün PackageId'leri registry'de yeniden doğrula ve LEDGER-0001'i güncelle.

Stable release için bunlara ek olarak hedef consumer'da migration, Vault/secret, tenant,
authorization ve gerçek paket restore smoke'u zorunludur.

## Dinamik script sözleşmesi

Yeni bir modül geldiğinde yayın motoru kopyalanıp içine yeni `if/switch` yazılmaz. Aile,
manifest verisi olarak eklenir:

```json
{
  "name": "ExampleModule",
  "displayName": "Example Module",
  "root": "C:/absolute/repository/path",
  "solution": "Example.Module.slnx",
  "version": "0.2.0-alpha.1",
  "immutableVersions": ["0.1.0-alpha.5"],
  "keyName": "example-module-push",
  "packageGlob": "Example.Module*",
  "owner": "mertbyd",
  "hasSymbols": true,
  "packages": [
    { "id": "Example.Module.Domain.Shared", "project": "src/Example.Module.Domain.Shared/Example.Module.Domain.Shared.csproj" },
    { "id": "Example.Module", "project": "src/Example.Module/Example.Module.csproj" }
  ]
}
```

Paket sırası dependency-first, composition-package-last'tır. Host ve test projeleri manifestte
yer almaz. API key hiçbir zaman manifestte bulunmaz. Kişisel Codex skill'i
`$nuget-family-release`, bu şemayı ve manifest tabanlı
`scripts/publish-nuget-families.ps1` motorunu taşır.

Dinamik motor da mutlak yol ile çağrılır:

```powershell
$engine = "C:\Users\mertb\.codex\skills\nuget-family-release\scripts\publish-nuget-families.ps1"
$manifest = "C:\absolute\path\release-manifest.json"

if (-not (Test-Path -LiteralPath $engine -PathType Leaf)) { throw "Motor bulunamadı: $engine" }
if (-not (Test-Path -LiteralPath $manifest -PathType Leaf)) { throw "Manifest bulunamadı: $manifest" }

# Aileleri ve paket sayılarını göster.
powershell -NoProfile -ExecutionPolicy Bypass -File $engine -ManifestPath $manifest -List

# Tek aile dry run; push için yalnız ayrıca onaylandıktan sonra -Push eklenir.
powershell -NoProfile -ExecutionPolicy Bypass -File $engine `
  -ManifestPath $manifest -Family ExampleModule -Version "<YENİ-SÜRÜM>"
```

## Güncel aile kataloğu

Checker/Vault satırları en son 2026-08-16'da NuGet V3 flat-container ile doğrulandı;
diğer aileler kendi ledger kayıt tarihlerini korur.

| Aile | Public sürüm | Paket | Scoped-key glob | Key adı |
|---|---:|---:|---|---|
| Notifications | `0.1.0-alpha.1` | 6 | `Pintern.SaaS.Notifications.*` | `pintern-notifications-push` |
| API Contracts | `0.2.0-alpha.7` | 8 | `CheckNexus.ApiContracts*` | `checknexus-api-contracts-push` |
| Database Comparison | `0.2.0-alpha.8` | 8 | `CheckNexus.DatabaseComparison*` | `checknexus-database-comparison-push` |
| Vault | `0.2.0-alpha.2` | 1 | `CheckNexus.Vault` | `checknexus-vault-push` |
| Foundation | `1.0.0` public | 7 | `Nexum.Abp.Foundation.*` | Foundation'a özel scoped key |
| Authenticator | `2.0.0` public | 8 | `Authenticator.*` | `authenticator-push` |

Var olan ailelerin key scope'u `Push only new package versions`, owner'ı `mertbyd`,
unlist/relist yetkisi kapalıdır. İlk kez oluşturulacak PackageId için geçici olarak
`Push new packages and package versions` gerekir; ilk yayın sonrası daraltılır.

### Notifications — 6 paket

- `Pintern.SaaS.Notifications.Domain.Shared`
- `Pintern.SaaS.Notifications.Domain`
- `Pintern.SaaS.Notifications.Application.Contracts`
- `Pintern.SaaS.Notifications.Application`
- `Pintern.SaaS.Notifications.EntityFrameworkCore`
- `Pintern.SaaS.Notifications.HttpApi`

Repository: `C:\Users\mertb\RiderProjects\pintern-notifications`  
Script: `scripts\publish-notifications.ps1`

### API Contracts — 8 paket

- `CheckNexus.ApiContracts.Domain.Shared`
- `CheckNexus.ApiContracts.Domain`
- `CheckNexus.ApiContracts.Application.Contracts`
- `CheckNexus.ApiContracts.Application`
- `CheckNexus.ApiContracts.EntityFrameworkCore`
- `CheckNexus.ApiContracts.HttpApi`
- `CheckNexus.ApiContracts.HttpApi.Client`
- `CheckNexus.ApiContracts`

Repository: `C:\Users\mertb\RiderProjects\ptn-assurance-platform`  
Script seçici: `scripts\publish-checkers.ps1 -Family ApiContracts`

### Database Comparison — 8 paket

- `CheckNexus.DatabaseComparison.Domain.Shared`
- `CheckNexus.DatabaseComparison.Domain`
- `CheckNexus.DatabaseComparison.Application.Contracts`
- `CheckNexus.DatabaseComparison.Application`
- `CheckNexus.DatabaseComparison.EntityFrameworkCore`
- `CheckNexus.DatabaseComparison.HttpApi`
- `CheckNexus.DatabaseComparison.HttpApi.Client`
- `CheckNexus.DatabaseComparison`

Repository: `C:\Users\mertb\RiderProjects\ptn-assurance-platform`  
Manifest: `checkers\database-comparison\scripts\database-comparison.release.json`

Yayın motoru: `$nuget-family-release/scripts/publish-nuget-families.ps1 -Family DatabaseComparison`

### Vault — public `0.2.0-alpha.2`

- `CheckNexus.Vault`

Repository: `C:\Users\mertb\RiderProjects\ptn-assurance-platform`

Manifest: `vault\release-manifest.json`

Yayın motoru: `$nuget-family-release/scripts/publish-nuget-families.ps1 -Family Vault`

### Foundation — public `1.0.0`

- `Nexum.Abp.Foundation.Domain.Shared`
- `Nexum.Abp.Foundation.Domain`
- `Nexum.Abp.Foundation.Application.Contracts`
- `Nexum.Abp.Foundation.Application`
- `Nexum.Abp.Foundation.EntityFrameworkCore`
- `Nexum.Abp.Foundation.HttpApi`
- `Nexum.Abp.Foundation.HttpApi.Client`

Yedi PackageId `1.0.0` ile nuget.org flat-container API'sinde doğrulandı. Authenticator
`2.0.0` bu aileyi katman eşlemeli public dependency olarak taşır.

### Authenticator — public `2.0.0`

Tarihsel `1.0.1` seviyesindeki altılı:

- `Authenticator.Domain.Shared`
- `Authenticator.Domain`
- `Authenticator.Application.Contracts`
- `Authenticator.Application`
- `Authenticator.EntityFrameworkCore`
- `Authenticator.HttpApi.Client`

Repository: `C:\Users\mertb\RiderProjects\pintern-authenticator-latest-api`

Eski `1.x` registry grafiği hizasızdı: yukarıdaki altılı `1.0.1`, `Authenticator.HttpApi` ve
`Authenticator.EventHandler` yalnız `1.0.0` seviyesindeydi. Güncel tüketici hedefi bu aile değildir.

Sekizli aile kararı `2.0.0` için kapanmıştır. Manifest-driven script, exact package/dependency
incelemesi, Release build, 215/215 test, 8 nupkg + 8 snupkg ve clean consumer module
initialization smoke geçmiştir. `2.0.0` sekiz PackageId'nin tamamında NuGet.org'da publictir
ve registry'de doğrulanmıştır. Gelecek yayınlar interaktif scoped-key prompt'u ister.
Auth deposunda canonical remote tanımlı değilse
URL uydurulmaz; gerçek branch/commit ve repository type metadata'sı korunur. Foundation
repository URL'si Auth paket metadata'sında kullanılamaz.

## API key prompt standardı

Prompt mutlaka hangi key'in istendiğini söyler:

```text
API Contracts [CheckNexus.ApiContracts*] için nuget.org PUSH API key
Database Comparison [CheckNexus.DatabaseComparison*] için nuget.org PUSH API key
Notifications [Pintern.SaaS.Notifications.*] için nuget.org PUSH API key
Authenticator [Authenticator.*] için nuget.org PUSH API key
```

Key chat'e yapıştırılmaz, parametre olarak alınmaz, source/config/history/log içine yazılmaz.
Script `Read-Host -AsSecureString` kullanır; BSTR'ı `finally` içinde sıfırlar ve SecureString'i
dispose eder.

## Kısmi yayın kurtarması

Normal yayın exact version registry'de varsa durur. `--skip-duplicate` normal sürümleme
mekanizması değildir. Yalnız bir aile yarıda kaldıysa önce her PackageId/version registry'den
tek tek doğrulanır; değişmemiş aynı paket byte'larıyla açık `ResumePartial` modu kullanılır.
Eksik symbol paketi, karşılık gelen `.nupkg` mevcut olduktan sonra ayrıca yeniden gönderilebilir.
