# Ptn.TestModule

Test Module composition host'unun ve modul katmanlarinin kaynak agacidir. ABP CLI 10.6.0
`module` sablonundan (`--no-ui --dbms postgresql`) uretildi, ardindan ev paketleriyle baglandi.

## Yapi

```text
host/
  Ptn.TestModule.Host.Shared        multi-tenancy sabiti gibi host-genelinde paylasilan degerler
  Ptn.TestModule.HttpApi.Host       composition host: yetenekleri birlestirir, pipeline'i kurar
src/
  Ptn.TestModule.Domain.Shared      sabitler, sema adlari, hata kodlari, localization
  Ptn.TestModule.Domain             entity, manager ve domain sozlesmeleri
  Ptn.TestModule.Application.Contracts  DTO, AppService arayuzu, permission, FluentValidation
  Ptn.TestModule.Application        use-case orkestrasyonu ve Mapperly eslemeleri
  Ptn.TestModule.EntityFrameworkCore  DbContext, repository, migration ve checker adapter'lari
  Ptn.TestModule.HttpApi            ince controller yuzeyi
  Ptn.TestModule.HttpApi.Client     uzak tuketiciler icin ABP proxy istemcisi
test/
  Ptn.TestModule.TestBase                     ortak test altyapisi
  Ptn.TestModule.Domain.Tests                 domain testleri
  Ptn.TestModule.Application.Tests            uygulama testleri
  Ptn.TestModule.EntityFrameworkCore.Tests    Sqlite uzerinde EF testleri
  Ptn.TestModule.HttpApi.Client.ConsoleTestApp  elle HTTP dogrulama araci
```

## Baglanan ev paketleri

| Katman | Paket |
|---|---|
| Domain.Shared | `Authenticator.Domain.Shared` 2.2.0 · `Pintern.SaaS.Notifications.Domain.Shared` |
| Domain | `Authenticator.Domain` · `Pintern.SaaS.Notifications.Domain` |
| Application.Contracts | `Authenticator.Application.Contracts` · `Pintern.SaaS.Notifications.Application.Contracts` · `SystemStandards.Core` · `SystemStandards.Validation` |
| Application | `Authenticator.Application` · `Pintern.SaaS.Notifications.Application` |
| EntityFrameworkCore | `Authenticator.EntityFrameworkCore` · `Pintern.SaaS.Notifications.EntityFrameworkCore` · `CheckNexus.ApiContracts.Application.Contracts` · `CheckNexus.DatabaseComparison.Application.Contracts` |
| HttpApi | `SystemStandards.Abp` · `SystemStandards.Core` |
| HttpApi.Host | `CheckNexus.ApiContracts` · `CheckNexus.DatabaseComparison` · `Pintern.SaaS.Notifications.HttpApi` · `Piton.Emailing.Application/HttpApi/Infrastructure` · `SystemStandards.AspNetCore` |

`Nexum.Abp.Foundation.*` 1.0.0 **transitif** gelir; dogrudan `PackageReference` yazilmaz (ADR-0012).
`CheckNexus.Vault` 0.2.0-alpha.2 hostta compose edilir; checker ve Emailing secret degerleri
yalniz Vault referansi uzerinden cozulur.

ABP tabani **10.6.0**'dir: Authenticator 2.2.0 ailesi bu surume baglidir. Checker'lar,
SystemStandards ve Emailing 10.3.0 ile derlenmistir; NuGet grafigi 10.6.0'da birlesir.

## Auth tuketim modeli

`ptn-payment-management-api` ile ayni desen: Authenticator katman paketleri **tip** olarak alinir,
`Authenticator.HttpApi` modulu compose **edilmez**. Kimlik uclari (login, refresh, logout,
selected-context) ayri deploy edilen Authenticator host'unda kalir; bu host yalniz bearer token
dogrular (`AuthServer:Authority` + `AuthServer:Audience`). Karar kaydi: ADR-0013.

## Sema sahipligi

`test_lookup`, `test_catalog`, `test_run` (ADR-0016 §A). Sema adlari
`EntityFrameworkCore:Schemas` bolumunden ezilebilir. Bu modul yalniz kendi tablolarinin
migration sahibidir; Auth, Notification, Emailing ve checker tablolari kendi paketlerinin
migration assembly'lerine aittir (RULE-0002).

## Calistirma

```bash
dotnet build Ptn.TestModule.slnx
```

Host'u ayaga kaldirmadan once PostgreSQL gerekir; `appsettings.json` icindeki
`ConnectionStrings:Default` degerini ayarlayin. Gelistirmede `Redis:IsEnabled` `false`'tur;
production'da Redis acilir ve data protection anahtarlari Redis'te saklanir.

```bash
dotnet run --project host/Ptn.TestModule.HttpApi.Host
```

Bu host tek basina yeterli degildir: paylasilan `abp` semasini ve token'i uretecek Authenticator
hostu once calismalidir. Iki hostun sirasi, issuer ayarlari, token alma komutu ve
belirti-sebep-cozum tablosu icin `docs/wiki-brain/05-Operations/Local-Stack-Runbook.md`
(GUIDE-0007) okunur.

## Bilinen acik

`Pintern.SaaS.Notifications.*` paketleri `SystemStandards.Abp.Authorization` 1.0.0'a bagimlidir;
o surum nuget.org'da yoktur. NuGet bagimlilik surumunu taban kabul ettigi icin yayimda olan
2.1.0'i cozer ve `NU1603` uyarisi verir. Restore bu yuzden calisir: 2026-08-17'de bos bir paket
klasorune yalniz `NuGet.Config`'teki nuget.org kaynagiyla yapilan temiz restore 370 paketi
indirdi ve 0 hata dondu. Uyariyi kapatmak icin ya 1.0.0 yayimlanir ya da Notifications ailesi
2.1.0 tabanina alinir; ikisi de bu deponun disindadir.

`EntityFrameworkCore.Tests` bagimliligi `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 `NU1903` guvenlik
uyarisi verir (GHSA-2m69-gcr7-jv3q). Uretime cikmaz ama uyari-hata kapisi olan bir CI'da build'i
kirar.
