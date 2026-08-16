---
id: ARCH-AUTH-CONSUMPTION
type: architecture
status: active
title: Auth tüketim modeli
updated: 2026-08-13
decision_refs:
  - ADR-0005
  - ADR-0012
  - ADR-0013
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0004
---

# Auth tüketim modeli

Bu sayfa Test Module composition hostunun Authenticator'ı nasıl tüketeceğini tanımlar.

> [!IMPORTANT] 2026-08-13 revizyonu — [[03-Decisions/ADR-0013-Test-Module-Resource-Server-Auth-Consumption|ADR-0013]]
> Test Module hostu **resource server**'dır: Authenticator katman paketlerini tip olarak alır,
> `AuthenticatorHttpApiModule`'ü compose **etmez**. Kimlik uçları ayrı deploy edilen Authenticator
> hostunda kalır ve doğrulama ABP JWT bearer ile yapılır. Aşağıdaki "Tek composition host"
> bölümünün Auth HttpApi/OpenIddict satırları bu karara göre okunur; paket zinciri, katman
> eşlemesi ve migration sahipliği değişmemiştir.

## Taban ve paket zinciri

```text
ABP base
  -> public Foundation base
      -> Authenticator base ve auth davranışı
          -> Assurance/Test Module composition host
```

İki bağımlılık aynı anda vardır:

| Bağ | Sorumluluğu |
|---|---|
| NuGet dependency | Assembly ve tipleri transitif getirir |
| ABP `DependsOn` | Module servisleri ve runtime davranışını etkinleştirir |

Authenticator her katmanda iki bağı da kurar. Consumer yalnız Authenticator paketini ve
module'ünü seçer; Foundation için doğrudan `PackageReference` veya `DependsOn` yazmaz.

## Katman eşlemesi

| Consumer katmanı | Doğrudan paket | Transitif Foundation paketi |
|---|---|---|
| Domain.Shared | `Authenticator.Domain.Shared` | `Nexum.Abp.Foundation.Domain.Shared` |
| Domain | `Authenticator.Domain` | `Nexum.Abp.Foundation.Domain` |
| Application.Contracts | `Authenticator.Application.Contracts` | `Nexum.Abp.Foundation.Application.Contracts` |
| Application | `Authenticator.Application` | `Nexum.Abp.Foundation.Application` |
| EntityFrameworkCore | `Authenticator.EntityFrameworkCore` | `Nexum.Abp.Foundation.EntityFrameworkCore` |
| HttpApi | `Authenticator.HttpApi` | `Nexum.Abp.Foundation.HttpApi` |
| Uzak HTTP client | `Authenticator.HttpApi.Client` | `Nexum.Abp.Foundation.HttpApi.Client` |

`Authenticator.EventHandler` ayrı bir Foundation katmanı uydurmaz; Auth Domain ve
Application bağımlılıkları üzerinden aynı grafa katılır.

## Sürüm gerçeği

| Aile | Durum | Kullanım kararı |
|---|---|---|
| Foundation `1.0.0`, 7 paket | nuget.org'da public ve registry'de doğrulandı | Auth `2.0.0` tarafından transitif taşınır |
| Authenticator public `1.x` | 6 paket `1.0.1`, HttpApi/EventHandler yalnız `1.0.0` | Yeni Test Module entegrasyonunda kullanılmaz |
| Authenticator `2.0.0`, 8 paket | **nuget.org'da public; 8/8 registry'den doğrulandı (2026-08-13)** | Test Module entegrasyonunun hedef sürümü |

Public `1.x` ile `2.0.0` karıştırılmaz. Authenticator `2.0.0` ABP **10.6.0** ve EF Core
**10.0.10** üzerine kuruludur; checker'lar, SystemStandards ve Emailing 10.3.0 ile derlenmiştir
ve NuGet grafiği 10.6.0'da birleşir. Consumer hostta tek ABP sürümü çözülür.

## Yetenek hostu ve kimlik hostu (ADR-0013)

```text
Tek UI
  -> Authenticator host              login / refresh / logout / tenant / selected-context
  |                                  OpenIddict server, sertifika, Identity yaşam döngüsü
  -> Test Module composition host    bearer doğrulama (Authority + Audience)
       -> Authenticator Application + EF Core (tip olarak; HttpApi compose EDİLMEZ)
       -> Test Module katmanları
       -> API Contract Checker
       -> Database Checker
       -> Notifications + Emailing
       -> Vault (`CheckNexus.Vault` 0.2.0-alpha.2, public)
```

Yetenekler tek hostta compose edilir; kimlik yüzeyi ayrı hosttadır. Tek issuer hâlâ
Authenticator'dır: ikinci issuer, ikinci `sub`, ikinci session veya ikinci token store
oluşmaz. Checker'ların source-only ince hostları production'da çalışmaz.

Authenticator hostu; OpenIddict server ve validation pipeline, sertifika, data protection,
fail-fast options ve environment secret wiring sahibidir. Test Module hostu Autofac/container,
JWT bearer doğrulama, CORS, cache ve kendi data protection uygulama adının sahibidir. Bu host
ayrıntıları Foundation veya checker paketlerine taşınmaz.

## Migration sahipliği

| Tablo/şema | Model ve migration sahibi | Uygulayan |
|---|---|---|
| Identity, OpenIddict, tenant, selected-context ve auth lookup'ları | Authenticator EF paketi/migration assembly'si | Authenticator hostu/migrator'ı |
| Checker tabloları | İlgili checker EF paketi/migration assembly'si | Test Module composition migrator |
| Test Module tabloları (`test_lookup`/`test_catalog`/`test_run`) | `Ptn.TestModule.EntityFrameworkCore` migration assembly'si | Test Module composition migrator |
| Notifications ve Emailing tabloları | İlgili paketin migration assembly'si | Test Module composition migrator |

Consumer Auth entity'leri için yeni migration üretmez. Authenticator migration assembly'sini
deterministik sırada uygular ve aynı tabloyu ikinci bir assembly'den oluşturmaz.

## Davranış koruması

Foundation ortak repository, manager, AppService, controller ve contract tabanlarını verir.
Authenticator bunun üzerine auth'a özgü audit, authorization, lookup pasifleştirme,
invalidation/hydration ve kararlı hata kodlarını ekler. Foundation'ın genel fiziksel delete
yüzeyi Auth lookup sözleşmesine açılmaz.

## Consumer kabul kapısı

`2.0.0` public olduğuna göre (2026-08-13) Test Module'de şu kanıtlar birlikte alınır:

1. Clean cache ile yalnız Authenticator direct package referansları kullanılarak restore edilir.
   *(Durum: restore geçti; clean-cache tekrarı yapılmadı — global cache kullanıldı.)*
2. Çözümlenen grafikte yedi Foundation `1.0.0` paketi transitif görünür. *(Doğrulanmadı.)*
3. Tek ABP 10.6 / .NET 10 / EF Core 10.0.10 grafiği kurulur. *(Build 0 hata; EF Core 10.0.10'da
   birleşti — daha düşük Sqlite/Proxies sürümü CS1705 üretiyordu, test projesinde hizalandı.)*
4. Auth Application ve EF module initialization/DI açılır. **HttpApi compose edilmez** (ADR-0013).
   *(Durum: host ayağa kalktı, module graph kuruldu, HTTP pipeline istek işledi.)*
5. Auth endpoint'leri bu hostun Swagger'ında **görünmez**; checker ince hostları production
   graph'ta yoktur.
6. Auth migration assembly'si Authenticator hostunda uygulanır; checker/Test Module
   migrationlarıyla çakışmaz. *(Doğrulanmadı — migration henüz üretilmedi.)*
7. Login, refresh, logout, selected-context ve authorization negatif/cross-tenant yolları
   gerçek token ile Test Module hostuna karşı geçer. *(Doğrulanmadı.)*
8. Eksik config (`AuthServer:Authority`/`Audience`) hostu fail-fast kapatır. *(Doğrulanmadı.)*

Bu kapılar geçmeden yalnız paketin restore edilmesi entegrasyonun tamamlandığı anlamına gelmez.
