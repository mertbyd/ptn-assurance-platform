---
id: GUIDE-0007
type: guide
status: active
title: Yerel yigin runbook'u — iki host, tek veritabani, calisan token
updated: 2026-08-17
decision_refs:
  - ADR-0005
  - ADR-0012
  - ADR-0013
rule_refs:
  - RULE-0002
  - RULE-0003
---

# Yerel yığın runbook'u

> Bu sayfa **"kendi bilgisayarımda nasıl çalıştırırım"** sorusunun tek cevabıdır.
> Mimari niyet [[04-Architecture/Auth-Consumption-Model|ARCH-AUTH-CONSUMPTION]] ve
> [[03-Decisions/ADR-0013-Test-Module-Resource-Server-Auth-Consumption|ADR-0013]]'tedir.
> Buradaki her satır ya çalışan bir dosyadan ya da `Authenticator 2.0.0` paketinin
> kendi IL'inden çıkarıldı; **kanıt** sütunu kaynağı söyler.

## 0. Resim: ne çalışıyor

```text
PostgreSQL 5432
  └── tek veritabani
        ├── abp.*            <- sahibi Authenticator (Identity, OpenIddict, Setting, Permission)
        ├── auth.*           <- sahibi Authenticator (selected-context)
        └── test_*.*         <- sahibi Test Module

Authenticator host   https://localhost:44323   kimlik + token uretir
Test Module host     https://localhost:44366   yalniz bearer dogrular (resource server)
```

**Sıra değişmez:** önce Authenticator (şemayı ve client'ları o kurar), sonra Test Module.
Ters sırada Test Module açılır ama ilk ayar/izin isteğinde 500 verir.

---

## 1. Önkoşullar

| Ne | Neden | Zorunlu mu |
|---|---|---|
| .NET 10 SDK | iki host da `net10.0` | Evet |
| PostgreSQL (localhost:5432, `postgres/postgres`) | `appsettings.json` bu bağlantıyı yazar | Evet |
| `Pintern.Authenticator` deposu (ayrı repo) | kimlik hostu bu depodadır, pakette host yoktur | Evet |
| Docker | yalnız `RedoclyLintLiveTests` (`redocly/cli:2.14.0`) için | Hayır |
| Vault | `Vault:Token` verilmezse yerel akış çalışır | Hayır |

---

## 2. Authenticator hostu — client'ları **config** doğurur

Paketin `AuthenticatorOpenIddictDataSeedContributor` sınıfı client/scope kaydını
**yalnız configuration'dan** üretir; varsayılan değeri yoktur. Anahtar eksikse kayıt oluşmaz.

| Anahtar | Ne üretir | Kanıt |
|---|---|---|
| `AuthServer:RequiredScope` | scope adı | `ReconcileScopeAsync` |
| `AuthServer:Audience` | scope'un resource'u | `ReconcileScopeAsync` |
| `AuthServer:Authority` | swagger client'ın redirect kökü | `ReconcileSwaggerClientAsync` |
| `DeveloperApiClient:ClientId` | public swagger client | `ReconcileSwaggerClientAsync` |
| `DeveloperApiClient:ScopeDescription` | scope görünen adı | `ReconcileScopeAsync` |
| `FirstPartyClient:ClientId` | confidential login client | `ReconcileFirstPartyClientAsync` |
| `FirstPartyClient:ClientSecret` | client secret | `ReconcileFirstPartyClientAsync` |
| `AdminUserName` · `AdminEmail` · `AdminPassword` | bootstrap SuperAdmin | `EnsureBootstrapUserAsync` |

> `Admin*` üçlüsü `DataSeedContext` özelliğidir; **host** doldurur, paket doldurmaz.
> Authenticator deposundaki `RUNBOOK-0005` bu üçlünün hangi ayardan geldiğini söyler.
> Üçünden biri verilip diğeri boş bırakılırsa seed hata verir; ya üçü birden ya hiçbiri.

Secret dosyaya yazılmaz (ADR-0007). Authenticator deposunda bir kez:

```bash
dotnet user-secrets set "FirstPartyClient:ClientSecret" "<secret>" --project host/Pintern.Authenticator.HttpApi.Host
```

Sonra:

```bash
dotnet run --project host/Pintern.Authenticator.HttpApi.Host
```

### 2.1 Seed'in ürettiği **tam** liste

| Client | Tip | İzinli grant'lar |
|---|---|---|
| `DeveloperApiClient:ClientId` | public | `authorization_code`, `refresh_token`, `pkce` zorunlu |
| `FirstPartyClient:ClientId` | confidential | **`password`**, `refresh_token` |

> [!WARNING] `2.0.0` sürümünde `client_credentials` **seed edilmiyordu**.
> Discovery belgesindeki `client_credentials` sunucunun yeteneğidir, client'ın izni değil;
> `2.0.0` IL'inde `gt:client_credentials` dizesi hiç geçmez. **`2.1.0` ile kapandı**:
> `ResourceServers:Registrations` listesine giren her kayıt kendi scope + audience'ını alır,
> `MachineClientId` verilirse yalnız `client_credentials` ve o tek scope'u taşıyan confidential
> makine istemcisi seed edilir. Ayrıntı: Authenticator deposunun README'si.

### 2.2 Tek scope sınırı

`ReconcileScopeAsync` **tek** scope upsert eder: adı `AuthServer:RequiredScope`, resource'u
`AuthServer:Audience`. Yani bir Authenticator örneği kendi config'iyle **tek** API audience'ı
kaydeder. Test Module'ün `TestModule` audience'ı için iki seçenek vardır:

1. **Yerel geliştirme:** issuer'ı `AuthServer:RequiredScope=TestModule`,
   `AuthServer:Audience=TestModule` ile çalıştırın.
2. **Kalıcı çözüm:** Authenticator deposunda ikinci bir scope seed'i açılır. Bu depodan yapılamaz.

---

## 3. Token alma

```bash
curl -k -X POST https://localhost:44323/connect/token -d grant_type=password -d client_id=<FirstPartyClient:ClientId> -d client_secret=<secret> -d username=<AdminUserName> -d password=<AdminPassword> -d scope="TestModule offline_access"
```

`invalid_client` alıyorsanız client seed'i oluşmamıştır (§2). `unsupported_grant_type`
alıyorsanız `client_credentials` denemişsinizdir (§2.1).

---

## 4. Test Module hostu

```bash
dotnet run --project ptn-test-module/host/Ptn.TestModule.HttpApi.Host
```

Checked-in `appsettings.json` yerel geliştirme için hazırdır:

| Ayar | Değer | Neden |
|---|---|---|
| `AuthServer:Authority` | `https://localhost:44323` | discovery'deki `issuer` ile **birebir** aynı olmalı; iki checker hostu da bu değeri taşır |
| `AuthServer:Audience` | `TestModule` | issuer'daki `AuthServer:Audience` ile aynı olmalı |
| `Database:EnsureSharedAbpSchema` | `true` | `false`/eksik ise host `InvalidOperationException` ile **hiç açılmaz** |
| `Database:AutoMigrate` · `SeedOnStartup` | `true` | `test_*` şemasını ve lookup verisini host kurar; ayrı migrator projesi yoktur |

Production'da üçü de ortam değişkeniyle ezilir: `Database__AutoMigrate=false`,
`Database__SeedOnStartup=false`, `AuthServer__Authority=<gercek issuer>`.

Vault token'ı dosyaya yazılmaz (RULE-0003):

```bash
dotnet user-secrets set "Vault:Token" "<token>" --project ptn-test-module/host/Ptn.TestModule.HttpApi.Host
```

### 4.1 `EntityFrameworkCore:Schemas` içindeki `Volo.Abp.*` satırları

Bu satırlar **Test Module tarafında etkisizdir**. Authenticator'ın migration'ı tabloları
`abp` şemasına sabit yazar (`CreateTable("OpenIddictApplications", …, "abp")`), config'ten
okumaz. `"Volo.Abp.OpenIddict": "openiddict"` satırı üç hostta da aynıdır ve hiçbir şeyi
değiştirmez; tabloları `abp` şemasında aramak doğrudur. Bu satır bir hata değil, **ölü ayardır**.

### 4.2 Kolon adları

Authenticator `abp` tablolarını snake_case kolonlarla kurar (`id`, `client_id`). Test Module
hostu bu yüzden bütün DbContext'lere `UseSnakeCaseNamingConvention()` uygular (`2ef0c92`).
Model tarafında ikinci bir "Id → id" düzeltmesi **yazılmaz**; o iş bu ayarındır.

---

## 5. İzinler

`AuthenticatorIdentityDataSeedContributor` SuperAdmin rolüne **Authenticator hostunda tanımlı**
bütün izinleri verir (`SeedAllSidePermissionsAsync`). `TestModule.*` izinleri Test Module
hostunda tanımlıdır, dolayısıyla bu seed onları **kapsamaz**.

Makine istemcisinin token'ında kullanıcı yoktur; ABP yetkiyi `ClientPermissionValueProvider`
(`"C"`) ile **`client_id` üzerinden** okur. Grant'ı bu host yazar — izinleri tanımlayan taraf
burasıdır. `appsettings.json`:

```json
"AgentClients": {
  "Registrations": [
    {
      "ClientId": "TestModule_Agent",
      "Permissions": [ "TestModule.Bridge.Ground", "TestModule.Scenarios.Create", "TestModule.Scenarios.Update" ]
    }
  ]
}
```

`Database:SeedOnStartup` açıkken host bu grant'ları `abp.AbpPermissionGrants` tablosuna
idempotent yazar. Liste yalnız **bu modülün** izinlerini kabul eder: yabancı ya da hatalı
yazılmış bir izin adı host'u açılışta durdurur, çünkü sessizce yazılmayan bir grant sonradan
sebebi anlaşılmayan 403 olarak döner. Onay gibi ayrıcalıklı izinleri (`Scenarios.Approve`)
listeye koymayın; ajanın yetkisi listeye ne yazdığınız kadardır.

Semptom: token geçerli ama uç **403** dönüyorsa eksik olan token değil, grant'tır.

---

## 6. Sorun → sebep → çözüm

| Belirti | Sebep | Çözüm |
|---|---|---|
| Host açılışta `Host environment is not configured for shared ABP schema` | `Database:EnsureSharedAbpSchema` yok/false | §4 |
| `/health` 200 ama ayar/izin ucu 500 | Authenticator migration'ları uygulanmamış | önce Authenticator'ı çalıştırın |
| Token `400 invalid_client` | client seed'i yok — issuer'a config verilmemiş | §2 |
| Token `400 unsupported_grant_type` | `client_credentials` denendi | §2.1, `password` kullanın |
| Uç `401` | `Authority` ≠ discovery `issuer` (ör. 44314 ↔ 44323) | §4 |
| Uç `403` | izin grant'ı yok | §5 |
| Uç `500` + `DependencyResolutionException` | port `[ExposeServices]` taşımıyordu | `f23ee3c` ile kapandı; `CapabilityPortWiringTests` nöbetçidir |
| `RedoclyLintLiveTests` kırmızı | Docker kapalı veya `redocly/cli:2.14.0` yok | `docker pull redocly/cli:2.14.0` |

---

## 7. Kabul kanıtı

```bash
curl -k https://localhost:44323/.well-known/openid-configuration   # issuer dogru mu
curl -k https://localhost:44366/health                             # 200 Healthy
curl -k -H "Authorization: Bearer <token>" https://localhost:44366/mcp   # 401 degil
```

Üçü de geçtiyse yığın ayaktadır. `/mcp` hâlâ 401 veriyorsa sorun token'ın kendisindedir:
`AuthServer:Audience` ile token'ın `aud` claim'ini karşılaştırın.
