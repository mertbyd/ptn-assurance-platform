---
id: ARCHIVE-0001
type: guide
status: active
title: Consolidated research and historical platform record
updated: 2026-08-11
decision_refs:
  - ADR-0001
rule_refs: []
---

# Birleştirilmiş araştırma ve tarihsel platform kaydı

> [!warning] Kanonik güncel bilgi değildir
> Bu dosya, önceki tek dosyalı wiki içindeki hiçbir araştırma, URL, kanıt veya tarihsel kararı kaybetmemek için dondurulmuştur. Güncel durum için [[../00-Home|00-Home]] ve `01-Current`; bağlayıcı kararlar için `03-Decisions` kullanılır. Bu arşivde artık var olmayan ana host, solution, `src`, `test` ve `eng` yollarına ilişkin tarihsel ifadeler bulunabilir.

# PTN Assurance Platform — Eski tek dosyalı kanonik kayıt

> **Durum:** Aktif ve kanonik  
> **Son doğrulama tarihi:** 2026-08-11  
> **Çalışma alanı:** `C:\Users\mertb\RiderProjects\ptn-assurance-platform`  
> **Kural:** Bu repository içinde ikinci bir wiki, araştırma notu, karar klasörü veya paralel yol haritası açılmaz. Yeni gerçek, karar, kaynak ve doğrulama sonucu bu dosyaya işlenir.

## 1. Bu dosya neyi çözüyor?

Bu çalışma yeni bir ABP modülü kurma işi değildir. Daha önce iki farklı klasöre, çok sayıda wiki sayfasına ve yerel paket feed'lerine dağılmış platform bilgisini tek bir Git çalışma alanında toplar.

Kanonik ürün yönü şudur:

```text
Tek UI
  -> Ptn.TestOrchestration tabanlı tek composition host
       -> Authenticator          (tek identity lifecycle ve tek OAuth/OIDC issuer)
       -> Notifications          (ortak bildirim yeteneği)
       -> API Contract Checker   (auth'suz iş yeteneği)
       -> Database Checker       (auth'suz iş yeteneği)
       -> Shared Vault Adapter   (tek secret-store entegrasyonu)
       -> Test Orchestrator      (checker'ları kullanan asıl test ürünü)
       -> MCP Adapter            (daha sonra, aynı Application.Contracts sınırında)

Tek deploy edilen host
Tek mantıksal uygulama veritabanı
Tek issuer
Modül başına açık şema ve migration sahipliği
```

Buradaki “tek veritabanı”, bütün tabloların aynı şemaya yığılması anlamına gelmez. Aynı PostgreSQL veritabanı içinde `abp`, `auth`, `checker`, `lookup`, `connection`, `definition`, `run` gibi sahibi belli şemalar kullanılabilir. Yasak olan, aynı Identity/OpenIddict/Emailing tablosunu iki modülün veya iki aktif hostun ikinci kez sahiplenmesidir.

## 2. Bilgi önceliği

Bir agent veya geliştirici çelişki gördüğünde şu sırayı kullanır:

1. Çalışan kod, migration, üretilmiş `.nupkg` içeriği ve resmî NuGet.org kaydı.
2. Bu dosyadaki “Güncel gerçek” ve “Kabul edilmiş karar” bölümleri.
3. Upstream repository'nin kendi current-truth ve accepted ADR kayıtları.
4. Bu dosyadaki araştırma sentezi ve öneriler.
5. Tarihsel/superseded notlar.

Bir internet kaynağı mimari kararı destekler; tek başına ürün kararı oluşturmaz. Yeni karar ancak kullanıcı açıkça seçtiğinde tarih ve sonuçlarıyla bu dosyadaki karar defterine eklenir.

## 3. Fiziksel çalışma alanı

```text
ptn-assurance-platform/
  src/                         Ptn.TestOrchestration ana host ve ABP katmanları
  test/                        Kalıcı platform, auth, EF ve composition kanıtları
  checkers/
    api-contract/              Auth'suz API Contract Checker kaynak/host/test katmanları
    database-comparison/       Auth'suz Database Checker kaynak/host/test katmanları
  vault/                       İki checker portunu tek adapter ile uygulayan Vault katmanı
  eng/local-packages/          Yalnız public olmayan zorunlu tüketim paketleri
  docs/
    PTN-ASSURANCE-PLATFORM-WIKI.md
  Ptn.AssurancePlatform.slnx   Bütün merkezi workspace'i açan Rider/.NET solution
```

### 3.1 Klasörlerin anlamı

- `src/` yeni bir modül değildir. Mevcut `Ptn.TestOrchestration.*` uygulamasının kaynak katmanlarıdır ve hedef composition host burada gelişir.
- `test/` deneme çöplüğü değildir. Auth surface, selected-context, EF composition ve Vault DI sözleşmesini kanıtlayan kalıcı testler burada tutulur.
- `checkers/` yapay bir `modules/engines` katmanı değildir. Yalnız aynı ürün ailesindeki iki bağımsız checker source tree'sini yan yana tutan fiziksel gruptur.
- Her checker'ın `host/Ptn.*.HttpApi.Host` projesi kalır. Bu hostlar geliştirme, Swagger, HTTP, EF ve migration smoke içindir; NuGet paketine girmez ve hedef production topolojisinde ikinci aktif runtime owner olmaz.
- `vault/` bir Vault sunucusunun kopyası değildir. Checker'ların ayrı `ISecretProvider` portlarını tek composition adapteriyle uygulayan paket kaynağı ve local doğrulama araçlarıdır.

### 3.2 Salt okunur upstream kaynaklar

Merkezi çalışma alanına taşınmayan repository'ler upstream doğrulama kaynağıdır:

| Kaynak | Yerel konum | Kullanım |
|---|---|---|
| Orijinal API Contract Checker | `C:\Users\mertb\RiderProjects\ptn-api-contract-checker` | Tarihsel kod, ayrıntılı motor wiki'si ve karşılaştırma kuralları; doğrudan değiştirilmez |
| Orijinal Database Checker | `C:\Users\mertb\Documents\Codex\2026-07-06\bi\ptn-database-comparison-api` | T12 kanıtları, DB motoru invariant'ları ve migration geçmişi; doğrudan değiştirilmez |
| Authenticator | `C:\Users\mertb\RiderProjects\pintern-authenticator-latest-api` | Tek issuer ve identity owner; yayımlanan `Authenticator.*` paketlerinin upstream'i |
| Notifications | `C:\Users\mertb\RiderProjects\pintern-notifications` | Aktif geliştirme repository'si; release kapısı kapanana kadar merkezi workspace tarafından değiştirilmez |

Test hostunun kullanıcı tarafından belirlenen şablon soyu, şirket içi `ptn-contract-checker-api` repository'sinin `KBP-61` dalıdır. Private remote URL bu public-safe wikiye kopyalanmaz; kod eklemeden önce şablon davranışı yerel/erişim kontrollü kaynaktan yeniden doğrulanır.

## 4. Güncel gerçek — 2026-08-11

### 4.1 Yetenek matrisi

| Yetenek | Bugünkü durum | Merkezi hedefteki rol |
|---|---|---|
| Test Orchestration | Ana host ve ilk auth/composition altyapısı var; gerçek TestPlan/TestRun/TestStep/Binding/Evidence modeli henüz yok | Tek deploy edilen composition host ve test ürünü |
| API Contract Checker | Auth/notification/operator implementasyonundan arındırıldı; source, host, test ve 8 public NuGet paketi var | OpenAPI ingestion, snapshot ve contract-diff yeteneği |
| Database Checker | Auth/notification/operator implementasyonundan arındırıldı; source, host, test ve 8 public NuGet paketi var | Şema, migration ve seçili veri karşılaştırma yeteneği |
| Shared Vault | Adapter, unit/failure testleri, gerçek KV v2 smoke ve persistent local Vault kanıtı var; NuGet.org'a yayımlanmadı | İki checker secret portu için tek singleton adapter |
| Authenticator | 8 katman paketi `1.0.0` olarak public; merkezi Identity/OpenIddict owner kararı kabul edilmiş | Tek issuer, user/role/tenant/OU/selected-context sahibi |
| Notifications | Çalışan SSE + email fan-out dilimleri var; upstream geliştirme aktif | Ortak business notification capability |
| MCP | Uygulanmadı | Repository/secret yerine Application.Contracts üzerinden sınırlı adapter |

### 4.2 Public NuGet gerçeği

2026-08-11 tarihinde resmî NuGet V3 uçlarından tekrar doğrulanan paketler:

#### API Contract Checker — `0.1.0-alpha.5`

- `CheckNexus.ApiContracts`
- `CheckNexus.ApiContracts.Application`
- `CheckNexus.ApiContracts.Application.Contracts`
- `CheckNexus.ApiContracts.Domain`
- `CheckNexus.ApiContracts.Domain.Shared`
- `CheckNexus.ApiContracts.EntityFrameworkCore`
- `CheckNexus.ApiContracts.HttpApi`
- `CheckNexus.ApiContracts.HttpApi.Client`

#### Database Checker — `0.1.0-alpha.5`

- `CheckNexus.DatabaseComparison`
- `CheckNexus.DatabaseComparison.Application`
- `CheckNexus.DatabaseComparison.Application.Contracts`
- `CheckNexus.DatabaseComparison.Domain`
- `CheckNexus.DatabaseComparison.Domain.Shared`
- `CheckNexus.DatabaseComparison.EntityFrameworkCore`
- `CheckNexus.DatabaseComparison.HttpApi`
- `CheckNexus.DatabaseComparison.HttpApi.Client`

#### Authenticator — `1.0.0`

- `Authenticator.Domain.Shared`
- `Authenticator.Domain`
- `Authenticator.Application.Contracts`
- `Authenticator.Application`
- `Authenticator.EntityFrameworkCore`
- `Authenticator.HttpApi`
- `Authenticator.HttpApi.Client`
- `Authenticator.EventHandler`

`CheckNexus.Vault` aynı doğrulamada NuGet.org üzerinde `404` dönmüştür; public değildir. Merkezi host için tek yerel `CheckNexus.Vault.0.1.0-alpha.5.nupkg` kopyası tutulur. Public olan 16 checker paketinin yerel kopyaları feed karmaşası yaratmaması için tutulmaz.

NuGet kayıtları:

- <https://www.nuget.org/packages/CheckNexus.ApiContracts/0.1.0-alpha.5>
- <https://www.nuget.org/packages/CheckNexus.DatabaseComparison/0.1.0-alpha.5>
- <https://www.nuget.org/packages/Authenticator.Application/1.0.0>
- Resmî V3 service index: <https://api.nuget.org/v3/index.json>

### 4.3 Sürüm grafiği riski

- Checker paketleri bugün ABP `10.3.0` çizgisinde üretilmiştir.
- Test Module ve Authenticator ABP `10.6.0` çizgisindedir.
- Notifications kaynak durumu ABP `10.3.0` ile derlenmiş olmakla birlikte hedef `10.6.0` consumer graph'ında uyumluluk smoke'u geçmiştir.

Bu nedenle “paket restore oldu” tek başına release kanıtı değildir. Composition hostta bütün `Volo.Abp.*` paketlerinin tek sürüm grafiğine çözüldüğü, module initialization, DI, route, EF model ve migration smoke ile kanıtlanmalıdır. Sonraki checker sürümünde doğrudan `10.6.0` hizalaması değerlendirilir; aynı `0.1.0-alpha.5` içeriği değiştirilerek yeniden yayımlanmaz.

## 5. Kabul edilmiş mimari kararlar

### D-001 — Kanonik workspace

**Karar sahibi:** mertbyd  
**Tarih:** 2026-08-11  
**Karar:** Kanonik geliştirme alanı `ptn-assurance-platform` olur. Eski Test Module Git geçmişi bu kökte korunur; temiz checker/Vault source tree'leri aynı köke alınır. İki ayrı “platform” klasörü aktif kaynak olarak yaşamaz.

### D-002 — Tek UI, tek host, tek DB, tek issuer

**Karar sahibi:** mertbyd  
**Tarih:** 2026-08-11  
**Karar:** Üretim hedefi modüler monolittir. Tek UI yalnız tek composition hosta bağlanır. Authenticator bu host içindeki tek Identity/OpenIddict issuer'dır. Checker doğrulama hostları production'da ikinci owner olarak çalıştırılmaz.

### D-003 — Checker package/host sınırı

**Karar sahibi:** mertbyd  
**Tarih:** 2026-08-11  
**Karar:** Checker NuGet paketlerinde Controller, AppService, Manager, Repository, EF Core ve migration katmanları kalır. Her checker'ın ince hostu source tree ve solution içinde kalır fakat `IsPackable=false` olur. Host executable bir NuGet paketi değildir.

### D-004 — Ortak Vault

**Karar sahibi:** mertbyd  
**Tarih:** 2026-08-11  
**Karar:** Checker başına ayrı Vault sunucusu veya ayrı SDK implementasyonu kurulmaz. Tek Vault deployment/cluster, ortam ve tenant/path/policy sınırlarıyla ayrılır; composition host iki checker portunu tek adapter instance'ına bağlar.

### D-005 — Şema ve migration sahipliği

**Karar sahibi:** mertbyd  
**Tarih:** 2026-08-11  
**Karar:** Şema configuration key'leri kaldırılmaz. Checker EF/migration katmanı yalnız kendi iş tablolarını sahiplenir; Authenticator Identity/OpenIddict/tenant tablolarını, Notifications/Piton.Emailing kendi tablolarını sahiplenir. Tek DbMigrator bütün migration assembly'lerini deterministik sırayla çalıştırır.

### D-006 — Notifications kalıcılık sınırı

**Kaynak karar:** Notifications upstream accepted ADR  
**Karar:** Notification intent, inbox, read-state, outbox, retry veya replay tablosu yoktur. UOW commit sonrası process-local SSE + email fan-out vardır. Backplane kararı verilene kadar deployment single-instance olmak zorundadır.

### D-007 — Test Orchestration ana ürün olur

**Karar sahibi:** mertbyd  
**Tarih:** 2026-08-11  
**Karar:** Ayrı bir “ortak modül” daha yaratmak yerine mevcut Test Orchestration hostu composition hosta evrilir. Checker motorlarını kopyalamaz; NuGet/Application.Contracts üzerinden orkestre eder. Tek UI'da checker işleri, test senaryoları, bildirimler ve daha sonra MCP sunulur.

## 6. Superseded veya yasak varsayımlar

Aşağıdaki fikirler güncel plan değildir:

- Her checker'ın kendi auth, Identity, OpenIddict, operator veya tenant lifecycle'ını taşıması.
- Her checker'ın kendi notification transportunu, SMTP akışını veya recipient tablolarını taşıması.
- Her checker için ayrı Vault sunucusu ya da ayrı VaultSharp client'ı yazılması.
- Host projelerinin NuGet paketine konması veya hostun silinmesi.
- Checker migration'larının `AbpUsers`, OpenIddict veya EmailTemplates tablolarını yeniden üretmesi.
- Test Orchestration'ın Authenticator'dan bağımsız ikinci issuer olarak kalması.
- Aynı DB/job store üzerinde iki aktif scheduler/runtime owner çalıştırılması.
- `engines/` ve `modules/` gibi anlamı belirsiz ara klasörler kurulması.
- Motorları Test Module içine kaynak kopyala-yapıştır ile gömmek.
- Test sonucu için AI'ın tek başına pass/fail oracle olması.
- MCP'ye ham SQL, serbest URL fetch, secret veya repository erişimi açılması.
- Sırf “dosya çok” diye davranış kanıtlayan testlerin silinmesi.

## 7. API Contract Checker gerçeği

### 7.1 Sorumluluk

API Contract Checker canlı OpenAPI/Swagger dokümanlarını kontrollü HTTP istemcisiyle alır, kanonik modele indirger, snapshot geçmişi tutar ve iki snapshot arasında yönlü breaking/non-breaking/docs-only bulgular üretir.

İş zinciri korunur:

```text
Controller -> AppService -> Manager -> Repository -> EF Core / migration
```

Checker token üretmez, Identity/OpenIddict kurmaz, operator/recipient yönetmez, e-posta/SSE taşımaz ve Vault SDK'sı içermez. Yalnız `ISecretProvider` portunu ve entity içinde secret referansını bilir. Secret value DTO, log, event, job argument veya test evidence'a çıkmaz.

### 7.2 Persistence

`20260811091047_InitialApiContractCheckerModule` migration'ı yalnız `checker` şemasında 10 tablo üretir:

- `spec_sources`
- `spec_documents`
- `spec_contents`
- `spec_snapshots`
- `contract_check_runs`
- `spec_formats`
- `check_run_statuses`
- `difference_severities`
- `difference_directions`
- `difference_kinds`

Tenant bağlamında kayıt tenant-shared; host/tenant-less bağlamda kullanıcı `CreatorId` sınırındadır. Background job içinde kullanıcı claim'i yoksa tenant filtresi korunur, host kullanıcı filtresi uygulanmaz. Ağır owned `findings` JSON yalnız detay yolunda materialize edilir.

### 7.3 Çalıştırma

- HTTP tetik kısa UOW içinde Pending run yazar ve job kuyruğuna verir.
- Fetch/parse/compare uzun dış I/O sırasında UOW açık tutulmaz.
- Running/Completed/Failed geçişleri ayrı kısa UOW'lerde yazılır.
- Terminal durumda `ContractCheckRunStatusChangedEto` yayımlanır.
- Kalıcı job store ve gerçek Vault adapteri composition host tarafından sağlanır.

### 7.4 İnce host

`checkers/api-contract/host/Ptn.ApiContractChecker.HttpApi.Host` source içinde zorunludur. Swagger/HTTP/EF/migration smoke ve bağımsız geliştirme içindir. Dış Authority/Audience ile JWT doğrular; issuer değildir; paketlenmez.

## 8. Database Checker gerçeği

### 8.1 Sorumluluk

Database Checker PostgreSQL ve SQL Server hedeflerinde bağlantı doğrulama, schema discovery, şema farkı, EF migration history farkı ve kontrollü seçili veri karşılaştırması yapar. Test Orchestrator için asıl değeri motoru yeniden yazmak değil; küçük Application.Contracts projeksiyonları ve hedefli assertion akışları sağlamaktır.

İş zinciri korunur:

```text
Controller -> AppService -> Manager -> Repository -> EF Core / migration
```

### 8.2 Persistence ve görünürlük

`20260811092216_InitialDatabaseCheckerModule` yalnız checker iş şemalarında 11 lookup/connection/definition/run tablosu üretir. Identity, OpenIddict, EmailTemplates, operator veya recipient tablosu üretmez.

- Tenant bağlamında connection, definition ve run tenant-shared'dır.
- Host/tenant-less bağlamda görünürlük `CreatorId` ile kullanıcıya özeldir.
- Connection credential username/password çifti Vault'ta all-or-nothing saklanır; DTO'ya parola veya gerçek `VaultSecretPath` çıkmaz.
- `IPassivable` global filter varsayılan açıktır. Repository'de tekrar `.Where(x => x.IsActive)` yazılmaz. Tek filtreyi aşmak için `IDataFilter<IPassivable>.Disable()` kullanılır; `IgnoreQueryFilters()` kullanılmaz.
- `ComparisonRun.Findings` ve rapor JSON'u list/header sorgularında materialize edilmez; yalnız detail path yükler.
- EF katalog sınıfları `pg_catalog`/`sys.*` tablolarını migration modeline sızdırmaz.

### 8.3 Motor DI invariant'ı

Engine component sınıf adı, interface adının başındaki `I` çıkarılmış haliyle bitmelidir. Örnek: `PostgreSqlSchemaReader : ISchemaReader`. Aksi durumda ABP conventional DI bileşeni interface altında açığa çıkarmayabilir ve resolver sessizce `UnsupportedEngine` yoluna düşer.

### 8.4 İnce host

`checkers/database-comparison/host/Ptn.DatabaseChecker.HttpApi.Host` source içinde zorunludur. Bağımsız Swagger/HTTP/EF/migration doğrulaması içindir; production'da composition hostun yanında ikinci scheduler/runtime owner olarak açılmaz ve NuGet'e girmez.

## 9. Authenticator gerçeği ve entegrasyon sınırı

### 9.1 Sahiplik

Authenticator şu alanların tek sahibidir:

- ABP Identity user, role, session ve OrganizationUnit hiyerarşisi.
- ABP OpenIddict application/scope/authorization/token store ve issuance.
- ABP TenantManagement tenant/named connection store.
- ABP FeatureManagement tenant feature değerleri.
- Application scope kataloğu, selected-context membership/role/evidence version.
- Account kayıt, e-posta doğrulama, parola kurtarma ve ilgili güvenlik politikaları.

Composition host `Authenticator.Application`, `Authenticator.EntityFrameworkCore` ve issuer host wiring'ini yalnız bir kez yükler. Sadece token doğrulayacak ayrı bir client olsaydı `HttpApi.Client` + transitif contracts yeterli olurdu; hedef topolojide ayrı deploy yoktur.

### 9.2 Güncel yorum

Kullanıcı kararı Authenticator ürün diliminin bitip `1.0.0` paketleriyle yayımlandığıdır. Upstream current-truth sayfasında 2026-08-11 itibarıyla selected-context token claim, bazı tenant lifecycle ve canlı consumer smoke maddeleri hâlâ “açık” yazılıdır. Bu çelişki şöyle yönetilir:

- Paket yayın gerçeği ve tek-issuer kararı geçerlidir.
- Eski upstream backlog satırları otomatik olarak merkezi platform gereksinimi sayılmaz.
- Composition entegrasyonu başlamadan önce `1.0.0` paket içeriği, claim sözleşmesi ve canlı login/refresh/logout/selected-context turu yeniden ölçülür.
- Test Orchestration'daki mevcut yerel issuer kodu, Authenticator canlı paritesi kanıtlanmadan silinmez; parite geçince ikinci issuer bırakmayacak şekilde kaldırılır.

### 9.3 Güvenlik tabanı

- Interactive public client authorization code + PKCE + refresh kullanır.
- Implicit grant kapalıdır.
- Password grant yalnız kabul edilmiş ADR kapsamındaki first-party confidential client içindir.
- Production signing/encryption certificate ve Data Protection key ring ayrı secret sınırlarıdır.
- Tenant/OU/scope selected-context claim'i grant değildir; authoritative store ve operation permission her requestte fail-closed doğrulanır.

## 10. Notifications gerçeği ve entegrasyon sınırı

### 10.1 Çalışan akış

```text
business transaction
  -> secret-free typed notification intent
  -> address + payload allowlist doğrulaması
  -> UOW commit
  -> in-process fan-out
       -> live.sse (bounded, best effort)
       -> email    (Piton.Emailing, severity kuralına göre)
```

Mevcut endpoint ailesi intent publish, outcome read ve `text/event-stream` stream yüzeyini içerir. Payload yalnız kapalı allowlist'teki `OpaqueRunSummaryV1` alanlarını taşır. Adres `(tenantId, organizationUnitId, applicationScopeId)` üçlüsüdür ve kısmi adres fail-closed reddedilir.

### 10.2 Değişmezler

- Notification persistence, inbox, read-state, outbox, durable retry ve replay yoktur.
- E-posta render/persistence/SMTP/secret-store işi `Piton.Emailing` paketlerinindir; Notifications bunu yeniden yazmaz.
- Process-local SSE registry nedeniyle backplane seçilene kadar tek instance zorunludur.
- Checker yalnız terminal business event'i yayımlar; Notifications adapteri bu olayı delivery intent'e çevirir.

### 10.3 Release öncesi açıklar

- Aktif upstream dalındaki SSE ticket/bearer entegrasyonunun tamamlanması.
- Gerçek recipient resolver sahipliği.
- Business template seed sahibi ve gerçek SMTP smoke.
- Multi-instance backplane/topoloji kararı.
- Kategori × kanal kullanıcı/tenant preference kapsamı.
- Capacity, heartbeat ve reconnect SLO'ları.
- Boş Notifications EF projesinin gerçekten gerekli olup olmadığı.

Notifications aktif dirty repository'den source kopyalanmaz. Release kapısı kapandıktan sonra public veya kontrollü paket olarak composition hosta eklenir.

## 11. Ortak Vault gerçeği

### 11.1 Neden tek Vault?

Paketlerin ayrı NuGet olması secret backend'in de ayrı olması gerektiği anlamına gelmez. Global standart, workload'ların secret değerini değil referansını taşıması ve merkezi secret manager'ın path/policy/identity ile ayrıştırılmasıdır. Ayrı Vault cluster ancak farklı güvenlik alanı, regülasyon, blast radius veya bağımsız operasyon sahibi gerektiriyorsa anlamlıdır.

Hedef:

```text
API Contract Checker ISecretProvider ----\
                                      -> CheckNexus.Vault singleton -> tek Vault
Database Checker ISecretProvider -------/                              /env/tenant/capability
```

### 11.2 Kanıtlanmış local durum

2026-08-11 kanıtı:

- HashiCorp Vault `2.0.3` local container initialized, unsealed ve healthy.
- File storage named volume üzerinde persistent.
- Gerçek KV v2 write/read/delete smoke iki credential ailesi için geçti.
- Runtime policy yalnız checker path ailelerine sınırlandı; `sys/*` ve ilgisiz path erişimi reddedildi.
- 403, 5xx, timeout ve eksik token yolları fail-closed/redacted test edildi.
- İki checker portunun aynı `VaultSecretProvider` singleton instance'ına çözüldüğü composition testi geçti.
- Root token runtime token olarak kullanılmadı; token/unseal key/secret value wiki veya agent çıktısına yazılmadı.

Local unseal key geliştiricinin password manager'ında, application token .NET user-secrets sınırındadır. Değerleri bu dosyaya, `appsettings`, log, test output veya Git'e yazmak yasaktır.

### 11.3 Production için henüz karar olmayanlar

- Kubernetes/AWS/Azure/on-prem deployment ortamı.
- Community/HCP/Enterprise ve HA topolojisi.
- Workload identity: platform identity, Agent/Proxy veya AppRole.
- TLS trust ve certificate sahibi.
- Auto-unseal/KMS sahibi.
- Environment/tenant namespace ve policy ayrımı.
- Audit device ve retention.
- Backup/restore, revoke, rotation ve break-glass runbook sahibi.

Local compose kanıtı bu production kararlarını otomatik vermez.

## 12. Test Orchestrator ürün sınırı

Checker'lar bilgi motorudur; Test Orchestrator eylem ve kanıt motorudur. Hedef akış:

```text
OpenAPI operation projection
  + DB schema/data projection
  + explicit API-DB binding
  + auth context/profile
  -> versioned TestPlan
  -> TestRun
       -> HTTP action
       -> response/schema assertion
       -> extracted variable/correlation
       -> targeted DB assertion or bounded polling
       -> async/SSE/email evidence
  -> deterministic result + redacted evidence
  -> notification
```

### 12.1 Henüz olmayan kalıcı model

Aşağıdaki adlar araştırma sonucu güçlü adaydır, uygulanmış gerçek değildir:

- `TestPlan`
- `TestRun`
- `TestStep`
- `ApiDbBinding`
- `TestVariable`
- `Assertion`
- `EvidenceReference`

İlk vertical slice seçilmeden toplu generic engine yazılmaz. Önerilen ilk slice: bir API operation'ını çağır, response'dan ID çıkar, belirli timeout içinde tek DB kaydını doğrula, secret-free sonucu kaydet ve terminal event yayımla.

### 12.2 Test oracle katmanları

Tek bir oracle bütün doğruluğu kanıtlamaz:

1. Transport: HTTP status/header/content-type.
2. Contract: OpenAPI schema ve documented response.
3. Domain: business invariant ve state transition.
4. Persistence: hedefli schema/row/constraint doğrulaması.
5. Async: correlation + deadline + cardinality.
6. Security: permission, tenant/selected-context ve negatif yollar.
7. Non-functional: timeout, retry, rate, capacity ve leakage.

AI aday senaryo, mapping ve eksik kapsam önerebilir; pass/fail kararı deterministic oracle ve açık policy'den gelir.

### 12.3 Zero-retention önerisi

Müşteri schema/verisinin platform DB, cache, queue, blob, log veya telemetry'ye kalıcı yazılmaması güçlü bir güvenlik hedefidir fakat bütün ürün için kabul edilmiş mutlak karar değildir. Uygulama diliminde şu sınıflar ayrı tanımlanmalıdır:

- metadata tutulabilir mi?
- hash tutulabilir mi?
- redacted evidence ne kadar süre tutulur?
- raw response/row hiçbir zaman tutulmayacak mı?
- hata ayıklama için controlled capture var mı?

Karar verilene kadar secret, credential, raw müşteri satırı ve sınırsız payload kalıcılaştırılmaz.

## 13. Checker gelistirme kurallari

Bu bölüm taşınan checker source tree'lerindeki agent skill'lerin dayandığı ortak kuralları tek yerde tutar.

1. Mimari zincir `Controller -> AppService -> Manager -> Repository` olarak korunur.
2. Domain.Shared kararlı permission, error, setting, route, schema/table, claim, event ve lookup code string'lerinin sahibidir.
3. Entity davranış orkestrasyonu taşımaz; manager invariant ve business doğrulama sahibidir.
4. Mapperly entity/model/DTO mapping sahibidir. Manuel mapping yalnız açık ve belgeli istisnada kullanılır.
5. Her public input DTO için FluentValidation vardır; DB-backed uniqueness/ownership manager'dadır.
6. AppService atomik use-case orkestrasyonudur; uzun dış I/O açık UOW içinde tutulmaz.
7. Aynı akış ikinci kez oluşuyorsa doğru base/hook'a çıkarılır; ilk kullanım için soyutlama uydurulmaz.
8. EF configuration DbContext içine inline yazılmaz; bir type bir dosyada tutulur.
9. EF model değişikliği migration üretmeyi ve migration'ın `Up/Down` gövdesini okumayı zorunlu kılar.
10. `IPassivable` global filter ve tenant/host visibility invariant'ları korunur.
11. Engine component isimlendirme conventional DI ile uyumlu olmalıdır.
12. Secret value Domain/Application/DTO/event/job/evidence/log'a sızmaz; port Domain'de, provider implementation composition sınırındadır.
13. Her sınıfta `// islevi:` ve `// sistemdeki gorevi:` yorum çifti; authored metodun üstünde kısa amaç yorumu bulunur.
14. Auth, notification veya operator özelliği checker içine geri eklenmez; ilgili capability paketine adapter yazılır.
15. Host source içinde ve solution'da kalır, `IsPackable=false` kalır.
16. Generated `bin`, `obj`, log, PDB ve host config NuGet paketine girmez.

## 14. Global araştırma sentezi

### 14.1 Modüler monolit ve NuGet

ABP'nin module yapısı aynı process içindeki bağımlılık ve initialization sırasını yönetebilir. NuGet dağıtım sınırıdır; executable ownership sınırı değildir. Bu nedenle reusable layer package'ları yayınlanabilirken production host tek kalabilir. Meta paket kolay tüketim sağlar; katman paketleri transitif dependency graph'ı taşır. Host, DbMigrator ve test projeleri pack edilmez.

### 14.2 EF Core ve tek DB

Tek DB'de modül başına schema/migration assembly kullanılabilir. Global static `DbProperties` değerlerini birden fazla modülün yarışarak ayarlaması risklidir; gerçek owner composition sırasında bir kez uygular. Aynı migration history ve aynı tablo setini iki aktif migrator/runtime sahiplenmemelidir. Clean database migration smoke ve model snapshot incelemesi release kapısıdır.

### 14.3 OAuth/OIDC

Tek kullanıcı evreninde iki issuer aynı `sub`, session, logout, refresh ve revocation semantiğini böler. Merkezi issuer; audience/scope/resource ayrımıyla bütün capability endpointlerini korur. Selected-context client girdisi değil, doğrulanmış access token + authoritative authorization kararıdır. PKCE, kısa ömürlü access token, signing key rollover ve fail-closed discovery/validation operasyonu gerekir.

### 14.4 Vault

Secret manager centralize edilir; erişim workload identity, least-privilege policy, environment/tenant path ve audit ile ayrıştırılır. Root token, tracked secret, sınırsız wildcard, secret fallback ve raw error body yasaktır. Dynamic database credentials gelecekte değerlendirilebilir; mevcut static credential portunu gizlice değiştiren bir refactor değildir.

### 14.5 API test üretimi

OpenAPI contract, tek endpoint'in şekil bilgisini verir; iş workflow'u ve state transition vermez. Stateful/property-based araçlar ve Arazzo benzeri workflow tanımları senaryo üretimini güçlendirir. Parser/diff motoru ile canlı API runner ayrı sorumluluklardır. Dredd, Schemathesis, RESTler, EvoMaster, Specmatic, Prism, oasdiff ve openapi-diff fikir/proof oracle'larıdır; hiçbiri doğrudan ürün mimarisini tek başına belirlemez.

### 14.6 API–DB doğrulaması

OpenAPI alanı ile DB kolonunu HTTP methodundan otomatik ve kesin eşlemek mümkün değildir. Explicit, versioned binding gerekir. Hedefli `find-by-key`, bounded polling, transaction isolation ve deterministic cleanup büyük full-table diff'ten daha güvenli test primitive'leridir. Testcontainers gibi ephemeral dependency araçları izolasyon sağlar; production müşteri DB'sinde yazma izni ayrı policy gerektirir.

### 14.7 Asenkron bildirim testi

SSE reconnect replay değildir. “Listen before act”, correlation ID, deadline ve cardinality assertion gerekir. Process-local channel backpressure açık policy taşır. E-posta kanıtı gerçek kişiye gitmeyen SMTP sink ve template/recipient sahibiyle yapılmalıdır.

### 14.8 MCP ve AI

MCP yüzeyi yüksek seviyeli, allowlist'li application operation sunmalıdır. Ham repository, SQL, filesystem, Vault token veya serbest URL fetch açılmaz. Prompt injection ve tool output poisoning veri sınırı olarak ele alınır. İnsan onayı mutating veya yüksek etkili akışlarda policy ile belirlenir; model cevabı authorization veya test oracle yerine geçmez.

## 15. Yol haritası

### Faz 0 — Workspace konsolidasyonu

- [x] Mevcut Test Module Git kökünü `ptn-assurance-platform` adına taşı.
- [x] Auth'suz checker source/host/test tree'lerini `checkers/` altında birleştir.
- [x] Shared Vault source/test/local compose tree'sini köke taşı.
- [ ] Tek `Ptn.AssurancePlatform.slnx` oluştur ve bütün projeleri ekle.
- [ ] Public checker paket kopyalarını local feed'den kaldır; yalnız public olmayan Vault ve zorunlu authorization helper paketlerini tut.
- [ ] Tek kanonik wikiye bütün kaynak URL'lerini kayıpsız taşı.
- [ ] Eski wiki ağaçları, nested `.git`, `bin/obj/Logs`, şablon sample testleri ve duplicate artefaktları doğrulama sonrası kaldır.
- [ ] Full restore/build/test/scanner kapısını çalıştır.

### Faz 1 — Dependency ve package release disiplini

1. Central hostu public `CheckNexus.* 0.1.0-alpha.5` paketleriyle isolated restore et.
2. `CheckNexus.Vault` için public/private feed kararını ver; yayımlanırsa yeni sürüm üret.
3. Checker'ları ABP `10.6.0` çizgisinde yeniden doğrula ve gerektiğinde yeni pre-release sürüm çıkar.
4. Paketlerde exact internal dependency, README/license/owner metadata, vulnerability audit ve clean-cache consumer smoke uygula.
5. Aynı NuGet sürümünü farklı içerikle asla yeniden yayımlama.

### Faz 2 — Authenticator'ı tek issuer olarak compose et

1. `Authenticator.* 1.0.0` paket içeriğini ve transitive graph'ı incele.
2. Composition hostta Authenticator module/EF/HttpApi/EventHandler katmanlarını bir kez yükle.
3. Tek DbContext/migration strategy veya açık multi-DbContext aynı-DB strategy seçimini smoke ile kanıtla.
4. Login, refresh, logout, invite, password recovery ve selected-context access-token turunu canlı çalıştır.
5. Parite geçtikten sonra Test Orchestration'daki duplicate local issuer/OpenIddict ownership kodunu ve migration'ını güvenli migration planıyla kaldır.
6. İkinci `AbpUsers`, OpenIddict application/token veya signing key owner kalmadığını kanıtla.

### Faz 3 — Notifications release ve composition

1. Upstream aktif çalışmanın release kapısını bekle; dirty source kopyalama.
2. SSE ticket/bearer ve selected-context consumer smoke'u kapat.
3. Piton.Emailing template seed, recipient resolver ve gerçek SMTP sink smoke'u ekle.
4. Checker terminal ETO'larını Notifications intent adapterine bağla.
5. Backplane kararı yoksa deployment manifestinde single-instance kısıtını açık tut.

### Faz 4 — Checker composition ve tek DB kanıtı

1. İki public checker meta paketini hostta yükle.
2. Route, permission, localization ve conventional controller çakışması olmadığını doğrula.
3. Tek clean PostgreSQL DB'ye Authenticator + checker + gerekli Emailing migration'larını deterministik sırayla uygula.
4. Migration'ın yalnız sahibi olduğu şemaları oluşturduğunu ve duplicate platform tablosu olmadığını incele.
5. Ortak Vault adapterinin iki secret portuna singleton çözüldüğünü ve fail-closed davrandığını doğrula.
6. Background job/scheduler için tek aktif runtime owner seç.

### Faz 5 — İlk gerçek Test Orchestrator vertical slice

1. Tek bir güvenli use-case seç: API call -> ID extract -> bounded DB assertion -> redacted evidence.
2. Controller -> AppService -> Manager -> Repository zinciri ve Mapperly/validator/migration/test setini tamamla.
3. API ve DB checker'dan küçük projection contracts iste; domain/repository kopyalama.
4. Auth profile, tenant/selected-context ve read-only/mutating policy'yi açık tanımla.
5. Terminal sonucu Notifications üzerinden yayınla.

### Faz 6 — Tek UI

Tek navigation ve ortak authorization ile şu yüzeyler sunulur:

- API sources/documents/snapshots/runs/diffs.
- DB connections/definitions/runs/findings/reports.
- Test plans/runs/steps/evidence.
- Notification live status/outcome.
- Vault secret value göstermeyen connection/source credential yönetimi.
- Auth session/tenant/OU/scope yönetimi.

UI hiçbir zaman token, password, connection string veya gerçek Vault path/value göstermez.

### Faz 7 — MCP

MCP ancak Application.Contracts ve authorization matrisi kararlı olduğunda eklenir. İlk araçlar read-only discovery ve run status olabilir. Mutating test execution ayrı permission, scope, rate limit, audit, confirmation ve redaction kapısı ister.

## 16. Release ve doğrulama kapıları

Bir agent “tamamlandı” demeden aşağıdakileri raporlar:

| Kapı | Beklenen kanıt |
|---|---|
| Workspace | Tek root Git, nested `.git` yok, eski iki aktif platform klasörü yok |
| Wiki | Tek kanonik wiki; eski dış URL kümesi eksi yeni URL kümesi boş |
| Source integrity | Checker Controller/AppService/Manager/Repository/EF/migration ve hostları mevcut |
| Package | 16 public checker package restore; Vault tek local veya kararlı feed |
| Solution | Root solution bütün central projeleri açıyor |
| Build | Release full solution build, hata yok |
| Tests | Platform + API + DB + Vault kalıcı testleri geçiyor |
| Auth | Tek issuer/Identity/OpenIddict owner ve negatif auth yolları |
| EF | Clean DB migration, duplicate tablo yok, migration'lar okunmuş |
| Vault | Secret-free status, least privilege, fail-closed ve singleton DI |
| Scanner | `backend-verify` deterministic scanner; bulgular düzeltilmiş veya somut gerekçeli |
| Hygiene | `bin/obj/Logs`, sample scaffold, duplicate local packages ve temp kaynaklar yok |

Tarihsel doğrulama kanıtları — yeni consolidation sonrası yeniden çalıştırılmalıdır:

- API Contract Checker: önceki kayıtta `254/254`, daha yeni current sayfasında `258` test.
- Database Checker: `78/78`.
- Vault: `10/10` unit/failure + `1/1` gerçek KV v2 smoke.
- Test Module: `15/15` consolidation öncesi.
- Checker ince host health ve Swagger: HTTP `200/200`.
- Checker migration'ları: API 10 tablo, DB 11 tablo; platform tablosu yok.

Sayı farkı gördüğünde en yeni gerçek test discovery/run çıktısı yazılır; eski sayı “başarısızlık” sayılmaz fakat kanıt tarihi belirtilir.

## 17. Agent çalışma protokolü

1. Önce root `AGENTS.md`, sonra bu dosyanın ilgili bölümü okunur.
2. C# işinde repository/user-level `abp-backend-dev`; kapanışta `backend-verify` uygulanır.
3. API comparison engine işinde `checkers/api-contract/.agents/skills` altındaki ilgili skill okunur.
4. Authenticator ve Notifications upstream dirty çalışma alanlarına merkezi task kapsamında yazılmaz.
5. Yeni wiki/README/karar raporu açılmaz. Paket README yalnız NuGet package artifact gereksinimiyse checker kökünde kalabilir.
6. Yeni karar bu dosyadaki karar defterine owner, tarih, alternatif ve sonuçla eklenir.
7. Source registry'ye yeni internet kaynağı eklendiğinde URL, konu ve mümkünse origin açıklanır.
8. Secret-bearing command output capture edilmez. Token, unseal key, password, connection string ve müşteri verisi istenmez.
9. EF model değişirse migration üretilip tamamen okunur.
10. Package değişirse sürüm artırılır, pack içeriği incelenir, clean-cache consumer testi yapılır.
11. Host pack edilmez; source/test/migration davranış kanıtları “temizlik” adıyla silinmez.
12. Final cevap; değişen yerleri, test/build sayılarını, silinen materialin geri alınabilirliğini ve kalan riskleri açıkça söyler.

## 18. Kaynak/provenance defteri

| Ağaç | Taşıma öncesi referans | Not |
|---|---|---|
| Test Orchestration Git tabanı | `b184519f2f7bfb4bc4f751ee8b780cd584fb1ed8` / `main` | Dirty auth/checker/Vault composition değişiklikleri korunarak merkezi köke taşındı |
| Temiz API checker paket ağacı | `c245507e5783d841316abf05199fe68f6f4a083d` / `codex/saas-module-packaging` | Auth/notification/operator sökümü ve yeni migration/paket çalışması dirty tree olarak merkezi Git'e alınır |
| Temiz Database checker paket ağacı | `c136f670559ec232e3ea89bd4b2b30671fd2abe9` / `codex/saas-module-packaging` | Auth/notification/operator sökümü ve yeni migration/paket çalışması dirty tree olarak merkezi Git'e alınır |
| Authenticator upstream | `0589f9077a962bea40c0124ccf64234b05cdce20` / `KBP-0027` | Taşıma sırasında clean ve salt okunur |
| Notifications upstream | `621b6dacce20cfcb7e7bfc8698d6a73ff16ac0bc` / `KBP-N06` | Aktif dirty çalışma; merkezi task dokunmaz |

Nested `.git` klasörleri merkezi Git içinde bırakılmaz; yukarıdaki provenance ve orijinal upstream repository'ler kurtarma kaynağıdır.

## 19. Dış kaynak kataloğu

Bu bölüm eski Test Module, checker, Authenticator, Notifications ve Database Checker wiki/docs ağaçlarındaki bütün dış HTTP(S) araştırma kaynaklarından mekanik olarak üretilir. Aynı URL birden fazla belgede geçtiyse tek satırda origin listesi birleştirilir. `localhost` ve `127.0.0.1` operasyon endpointleri internet araştırma kaynağı sayılmaz; local Vault bölümünde ayrıca belgelenmiştir.

<!-- SOURCE-CATALOG:BEGIN -->
Toplam **305** benzersiz dış kaynak korunmuştur.

### abp.io

- <https://abp.io/architecture/modular-monolith> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`, `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://abp.io/docs/10.0/framework/architecture/multi-tenancy> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`
- <https://abp.io/docs/10.0/framework/fundamentals/authorization> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`
- <https://abp.io/docs/10.0/framework/infrastructure/data-filtering> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`
- <https://abp.io/docs/10.0/modules/openiddict> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`
- <https://abp.io/docs/10.4/framework/api-development/integration-services> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://abp.io/docs/10.6/framework/api-development/dynamic-csharp-clients?LanguageCode=en> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://abp.io/docs/10.6/framework/architecture/modularity/basics?LanguageCode=en> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`, `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://abp.io/docs/10.6/framework/fundamentals/localization?LanguageCode=en> — köken: `notifications-wiki/05-Research/DESIGN-QUESTION-MATRIX.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://abp.io/docs/10.6/framework/infrastructure/background-jobs?LanguageCode=en> — köken: `notifications-wiki/03-Decisions/ADR-0002-No-Notification-Persistence.md`, `notifications-wiki/05-Research/DESIGN-QUESTION-MATRIX.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`, `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://abp.io/docs/10.6/framework/infrastructure/event-bus/distributed?LanguageCode=en> — köken: `notifications-wiki/03-Decisions/ADR-0002-No-Notification-Persistence.md`, `notifications-wiki/05-Research/DESIGN-QUESTION-MATRIX.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`, `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://abp.io/docs/10.6/framework/infrastructure/text-templating?LanguageCode=en> — köken: `notifications-wiki/04-Architecture/NOTIFICATION-ARCHITECTURE.md`, `notifications-wiki/05-Research/DESIGN-QUESTION-MATRIX.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://abp.io/docs/10.6/modules/setting-management?LanguageCode=en> — köken: `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://abp.io/docs/10.6/solution-templates/application-module> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`, `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://abp.io/docs/7.4/framework/infrastructure/settings> — köken: `notifications-wiki/05-Research/DESIGN-QUESTION-MATRIX.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://abp.io/docs/api/8.3/Volo.Abp.EntityFrameworkCore.DistributedEvents.OutgoingEventRecord.html> — köken: `notifications-wiki/05-Research/DESIGN-QUESTION-MATRIX.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://abp.io/docs/api/9.1/Volo.Abp.EventBus.Distributed.OutgoingEventInfo.html> — köken: `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://abp.io/docs/api/abp/7.3/Volo.Abp.Uow.IUnitOfWork.html> — köken: `notifications-wiki/04-Architecture/NOTIFICATION-ARCHITECTURE.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://abp.io/docs/latest/framework/architecture/best-practices/domain-services> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://abp.io/docs/latest/framework/architecture/domain-driven-design/> — köken: `api-package-wiki/02-Rules/RULE-0003-Fixed-Folder-Map.md`, `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/02-Rules/RULE-0003-Fixed-Folder-Map.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://abp.io/docs/latest/framework/architecture/domain-driven-design/unit-of-work> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://abp.io/docs/latest/framework/architecture/modularity/basics> — köken: `test-wiki/04-Arastirma/01-Internet-Arastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://abp.io/docs/latest/framework/architecture/multi-tenancy> — köken: `database-upstream-docs/T1/05-ortam-ve-multitenancy-dogrulama.md`, `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://abp.io/docs/latest/framework/fundamentals/connection-strings> — köken: `database-upstream-docs/T1/05-ortam-ve-multitenancy-dogrulama.md`, `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://abp.io/docs/latest/framework/infrastructure/audit-logging> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://abp.io/docs/latest/framework/infrastructure/background-jobs> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`, `test-wiki/04-Arastirma/01-Internet-Arastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://abp.io/docs/latest/framework/infrastructure/data-filtering> — köken: `database-upstream-docs/T1/05-ortam-ve-multitenancy-dogrulama.md`
- <https://abp.io/docs/latest/framework/infrastructure/emailing> — köken: `notifications-wiki/04-Architecture/NOTIFICATION-ARCHITECTURE.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://abp.io/docs/latest/framework/infrastructure/event-bus/distributed> — köken: `test-wiki/04-Arastirma/01-Internet-Arastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://abp.io/docs/latest/framework/infrastructure/settings> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://abp.io/docs/latest/framework/infrastructure/timing> — köken: `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://abp.io/docs/latest/framework/real-time/signalr> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://abp.io/docs/latest/modules/account> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`
- <https://abp.io/docs/latest/modules/audit-logging?LanguageCode=en> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`
- <https://abp.io/docs/latest/modules/identity> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`
- <https://abp.io/docs/latest/modules/tenant-management> — köken: `database-upstream-docs/T1/05-ortam-ve-multitenancy-dogrulama.md`
- <https://abp.io/docs/latest/SignalR-Integration> — köken: `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://abp.io/docs/latest/testing/integration-tests> — köken: `test-wiki/04-Arastirma/04-Veritabani-Izolasyonu-ve-API-DB-Dogrulama.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://abp.io/docs/latest/testing/overall> — köken: `test-wiki/04-Arastirma/04-Veritabani-Izolasyonu-ve-API-DB-Dogrulama.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### airc.nist.gov

- <https://airc.nist.gov/airmf-resources/airmf/5-sec-core/> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### aka.ms

- <https://aka.ms/opensource/security/bounty> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/SECURITY.md`
- <https://aka.ms/opensource/security/cvd> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/SECURITY.md`
- <https://aka.ms/opensource/security/definition> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/SECURITY.md`
- <https://aka.ms/opensource/security/msrc> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/SECURITY.md`
- <https://aka.ms/opensource/security/pgpkey> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/SECURITY.md`

### api.nuget.org

- <https://api.nuget.org/v3-flatcontainer/piton.emailing.domain/index.json> — köken: `notifications-wiki/05-Research/DESIGN-QUESTION-MATRIX.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`

### apinotes.io

- <https://apinotes.io/blog/openapi-diff-detect-breaking-changes-between-api-versions> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`

### arxiv.org

- <https://arxiv.org/abs/1912.09686> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`
- <https://arxiv.org/abs/2005.03320> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`, `test-wiki/04-Arastirma/15-Derin-Bulgular-Problem-Karar-Agaci-ve-Etkiler.md`
- <https://arxiv.org/abs/2108.08209> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/10-Problem-Haritasi-ve-Net-Tanim.md`, `test-wiki/04-Arastirma/12-Kapsam-Test-Verisi-Durum-ve-Degisim.md`, `test-wiki/04-Arastirma/15-Derin-Bulgular-Problem-Karar-Agaci-ve-Etkiler.md`
- <https://arxiv.org/abs/2204.12148> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`, `test-wiki/04-Arastirma/10-Problem-Haritasi-ve-Net-Tanim.md`, `test-wiki/04-Arastirma/15-Derin-Bulgular-Problem-Karar-Agaci-ve-Etkiler.md`
- <https://arxiv.org/abs/2212.14604> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`
- <https://arxiv.org/abs/2411.07098> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/10-Problem-Haritasi-ve-Net-Tanim.md`, `test-wiki/04-Arastirma/14-AI-Destekli-Test-Uretimi-ve-Dogrulama.md`, `test-wiki/04-Arastirma/15-Derin-Bulgular-Problem-Karar-Agaci-ve-Etkiler.md`
- <https://arxiv.org/abs/2412.14137> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/14-AI-Destekli-Test-Uretimi-ve-Dogrulama.md`

### aspire.dev

- <https://aspire.dev/testing/overview/> — köken: `test-wiki/04-Arastirma/04-Veritabani-Izolasyonu-ve-API-DB-Dogrulama.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### aws.amazon.com

- <https://aws.amazon.com/builders-library/making-retries-safe-with-idempotent-APIs/> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://aws.amazon.com/builders-library/timeouts-retries-and-backoff-with-jitter/> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/10-Problem-Haritasi-ve-Net-Tanim.md`, `test-wiki/04-Arastirma/15-Derin-Bulgular-Problem-Karar-Agaci-ve-Etkiler.md`

### cheatsheetseries.owasp.org

- <https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://cheatsheetseries.owasp.org/cheatsheets/MCP_Security_Cheat_Sheet.html> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/14-AI-Destekli-Test-Uretimi-ve-Dogrulama.md`, `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://cheatsheetseries.owasp.org/cheatsheets/Server_Side_Request_Forgery_Prevention_Cheat_Sheet.html> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`

### citrusframework.org

- <https://citrusframework.org/citrus/reference/html/index.html> — köken: `test-wiki/04-Arastirma/04-Veritabani-Izolasyonu-ve-API-DB-Dogrulama.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### codeception.com

- <https://codeception.com/docs/modules/Db> — köken: `test-wiki/04-Arastirma/04-Veritabani-Izolasyonu-ve-API-DB-Dogrulama.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### committee.iso.org

- <https://committee.iso.org/sites/jtc1sc7/home/projects/flagship-standards/isoiecieee-29119-series.html> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`

### conf.researchr.org

- <https://conf.researchr.org/details/issta-2020/issta-2020-papers/21/Differential-Regression-Testing-for-REST-APIs> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`, `test-wiki/04-Arastirma/11-Test-Oracle-ve-Dogruluk-Katmanlari.md`

### csrc.nist.gov

- <https://csrc.nist.gov/Projects/automated-combinatorial-testing-for-software/downloadable-tools> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`
- <https://csrc.nist.gov/pubs/journal/2015/03/combinatorial-coverage-as-an-aspect-of-test-qualit/final> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`, `test-wiki/04-Arastirma/12-Kapsam-Test-Verisi-Durum-ve-Degisim.md`
- <https://csrc.nist.gov/pubs/sp/800/188/final> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/10-Problem-Haritasi-ve-Net-Tanim.md`, `test-wiki/04-Arastirma/12-Kapsam-Test-Verisi-Durum-ve-Degisim.md`

### datatracker.ietf.org

- <https://datatracker.ietf.org/doc/html/rfc7636> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`
- <https://datatracker.ietf.org/doc/html/rfc9700> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`

### dblp.org

- <https://dblp.org/rec/conf/icfp/ClaessenH00> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`

### deepwiki.com

- <https://deepwiki.com/sqlalchemy/sqlalchemy/4-database-dialects> — köken: `database-upstream-docs/T1/02-nesne-modeli-ortak-model.md`

### developer.hashicorp.com

- <https://developer.hashicorp.com/vault/docs/agent-and-proxy/autoauth> — köken: `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://developer.hashicorp.com/vault/docs/agent-and-proxy/proxy/apiproxy> — köken: `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://developer.hashicorp.com/vault/docs/audit> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://developer.hashicorp.com/vault/docs/audit/best-practices> — köken: `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://developer.hashicorp.com/vault/docs/auth/approle> — köken: `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://developer.hashicorp.com/vault/docs/auth/approle/approle-pattern> — köken: `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://developer.hashicorp.com/vault/docs/concepts/auth> — köken: `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://developer.hashicorp.com/vault/docs/concepts/lease> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://developer.hashicorp.com/vault/docs/concepts/policies> — köken: `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://developer.hashicorp.com/vault/docs/concepts/production-hardening> — köken: `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://developer.hashicorp.com/vault/docs/enterprise/namespaces> — köken: `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://developer.hashicorp.com/vault/docs/enterprise/namespaces/namespace-structure> — köken: `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://developer.hashicorp.com/vault/docs/secrets/databases> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`, `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://developer.hashicorp.com/vault/docs/secrets/kv/kv-v2> — köken: `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://developer.hashicorp.com/vault/docs/updates/release-notes> — köken: `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`

### developer.mozilla.org

- <https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`

### developers.bump.sh

- <https://developers.bump.sh/doc/workspace/operation/operation-post-diffs> — köken: `api-package-wiki/03-Decisions/ADR-0008-On-Demand-Async-Execution.md`, `api-upstream-wiki/03-Decisions/ADR-0008-On-Demand-Async-Execution.md`

### discovery.ucl.ac.uk

- <https://discovery.ucl.ac.uk/id/eprint/1471263/> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`, `test-wiki/04-Arastirma/11-Test-Oracle-ve-Dogruluk-Katmanlari.md`, `test-wiki/04-Arastirma/15-Derin-Bulgular-Problem-Karar-Agaci-ve-Etkiler.md`

### docs.aws.amazon.com

- <https://docs.aws.amazon.com/dms/latest/sql-server-to-aurora-postgresql-migration-playbook/chap-sql-server-aurora-pg.sql.datatypes.html> — köken: `database-upstream-docs/T1/02-nesne-modeli-ortak-model.md`

### docs.confluent.io

- <https://docs.confluent.io/platform/current/schema-registry/develop/api.html> — köken: `api-package-wiki/03-Decisions/ADR-0008-On-Demand-Async-Execution.md`, `api-upstream-wiki/03-Decisions/ADR-0008-On-Demand-Async-Execution.md`
- <https://docs.confluent.io/platform/current/schema-registry/fundamentals/schema-evolution.html> — köken: `api-package-wiki/03-Decisions/ADR-0007-Data-Model.md`, `api-upstream-wiki/03-Decisions/ADR-0007-Data-Model.md`

### docs.datadoghq.com

- <https://docs.datadoghq.com/monitors/notify/> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`

### docs.github.com

- <https://docs.github.com/en/account-and-profile/how-tos/notifications/configuring-notifications> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`

### docs.greatexpectations.io

- <https://docs.greatexpectations.io/docs/0.18/reference/learn/expectations/result_format/> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### docs.jentic.com

- <https://docs.jentic.com/getting-started/arazzo-runner/> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### docs.pact.io

- <https://docs.pact.io/> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://docs.pact.io/getting_started/how_pact_works> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### docs.soda.io

- <https://docs.soda.io/soda-cl/failed-row-samples.html> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### docs.specmatic.io

- <https://docs.specmatic.io/features/specmatic_mcp> — köken: `test-wiki/04-Arastirma/05-MCP-AI-ve-Guvenli-Test-Arayuzu.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://docs.specmatic.io/getting_started/mcp_auto_test.html> — köken: `test-wiki/04-Arastirma/05-MCP-AI-ve-Guvenli-Test-Arayuzu.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://docs.specmatic.io/home> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/03-Workflow-Stateful-ve-Arazzo.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://docs.specmatic.io/references/open-source-vs-enterprise> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### docs.sqlalchemy.org

- <https://docs.sqlalchemy.org/en/20/core/type_basics.html> — köken: `database-upstream-docs/T1/02-nesne-modeli-ortak-model.md`

### docs.stepci.com

- <https://docs.stepci.com/> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://docs.stepci.com/guides/testing-http.html> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://docs.stepci.com/reference/workflow-syntax> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### documentation.openiddict.com

- <https://documentation.openiddict.com/guides/getting-started/implementing-token-validation-in-your-apis> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://documentation.openiddict.com/integrations/aspnet-core> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`

### documentation.red-gate.com

- <https://documentation.red-gate.com/sc/getting-started/licensing> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`

### dotnet.testcontainers.org

- <https://dotnet.testcontainers.org/> — köken: `test-wiki/04-Arastirma/04-Veritabani-Izolasyonu-ve-API-DB-Dogrulama.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### dredd.org

- <https://dredd.org/en/latest/quickstart.html> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### eprints.whiterose.ac.uk

- <https://eprints.whiterose.ac.uk/id/eprint/110335/> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`

### eur-lex.europa.eu

- <https://eur-lex.europa.eu/legal-content/EN/TXT/?uri=CELEX:32016R0679> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### forum.liquibase.org

- <https://forum.liquibase.org/t/still-no-diff-for-stored-procedures/4462> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`

### genai.owasp.org

- <https://genai.owasp.org/resource/cheatsheet-a-practical-guide-for-securely-using-third-party-mcp-servers-1-0/> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### github.com

- <https://github.com/advisories/GHSA-v5pm-xwqc-g5wc> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://github.com/aspnet> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/SECURITY.md`
- <https://github.com/asyncapi/spec/blob/master/spec/asyncapi.md> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/13-Asenkron-Notification-SSE-Email-Testleri.md`
- <https://github.com/axllent/mailpit> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/13-Asenkron-Notification-SSE-Email-Testleri.md`
- <https://github.com/Azure> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/SECURITY.md`
- <https://github.com/cloudevents/spec/blob/main/cloudevents/spec.md> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/13-Asenkron-Notification-SSE-Email-Testleri.md`, `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://github.com/datafold/data-diff> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`
- <https://github.com/djrobstep/migra> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`
- <https://github.com/dotnet> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/SECURITY.md`
- <https://github.com/eulerto/pgquarrel> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`
- <https://github.com/fordfrog/apgdiff> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`
- <https://github.com/jbogard/Respawn> — köken: `test-wiki/04-Arastirma/04-Veritabani-Izolasyonu-ve-API-DB-Dogrulama.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://github.com/jentic/arazzo-engine> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/03-Workflow-Stateful-ve-Arazzo.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://github.com/keploy/keploy> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://github.com/liquibase/liquibase/issues/2693> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`
- <https://github.com/Microsoft> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/SECURITY.md`
- <https://github.com/microsoft/DacFx> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`
- <https://github.com/microsoft/OpenAPI.NET/blob/main/docs/upgrade-guide-2.md> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://github.com/microsoft/restler-fuzzer> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/10-Problem-Haritasi-ve-Net-Tanim.md`
- <https://github.com/microsoft/sql-server-samples> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/README.md`
- <https://github.com/Microsoft/sql-server-samples/releases> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/README.md`
- <https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorksDW-data-warehouse-install-script.zip> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/samples/databases/adventure-works/README.md`
- <https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorks-oltp-install-script.zip> — köken: `database-upstream-docs/SampleDatabases/PostgreSQL/AdventureWorks/AdventureWorks-for-Postgres/README.md`, `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/samples/databases/adventure-works/README.md`
- <https://github.com/Microsoft/sql-server-samples/releases/tag/adventureworks> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/README.md`, `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/samples/databases/adventure-works/README.md`
- <https://github.com/Microsoft/sql-server-samples/releases/tag/in-memory-oltp-demo-v1.0> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/README.md`
- <https://github.com/Microsoft/sql-server-samples/releases/tag/iot-smart-grid-v1.0> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/README.md`
- <https://github.com/Microsoft/sql-server-samples/releases/tag/wide-world-importers-v1.0> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/README.md`
- <https://github.com/Microsoft/sql-server-samples/tree/master/samples/databases/adventure-works/data-warehouse-install-script> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/samples/databases/adventure-works/README.md`
- <https://github.com/Microsoft/sql-server-samples/tree/master/samples/databases/adventure-works/oltp-install-script> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/samples/databases/adventure-works/README.md`
- <https://github.com/modelcontextprotocol/inspector> — köken: `test-wiki/04-Arastirma/05-MCP-AI-ve-Guvenli-Test-Arayuzu.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://github.com/OAI/Arazzo-Specification> — köken: `test-wiki/04-Arastirma/03-Workflow-Stateful-ve-Arazzo.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://github.com/oasdiff/oasdiff> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`, `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://github.com/oasdiff/oasdiff/blob/main/docs/BREAKING-CHANGES.md> — köken: `api-package-wiki/02-Rules/RULE-0007-Breaking-Change-Direction.md`, `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/02-Rules/RULE-0007-Breaking-Change-Direction.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://github.com/oasdiff/oasdiff-action> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://github.com/Orange-OpenSource/hurl> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://github.com/postgresql-tools/migra> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`
- <https://github.com/schemathesis/schemathesis> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://github.com/selab-gatech/AutoRestTest/> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://github.com/SeUniVr/RestTestGen> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`, `test-wiki/04-Arastirma/10-Problem-Haritasi-ve-Net-Tanim.md`, `test-wiki/04-Arastirma/15-Derin-Bulgular-Problem-Karar-Agaci-ve-Etkiler.md`
- <https://github.com/specmatic/specmatic> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://github.com/stoplightio/prism> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://github.com/stoplightio/spectral> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://github.com/strefethen/arazzo-cli> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/03-Workflow-Stateful-ve-Arazzo.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://github.com/stripe/pg-schema-diff> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`
- <https://github.com/WebFuzzing/evomaster> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://github.com/xamarin> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/SECURITY.md`

### grafana.com

- <https://grafana.com/docs/k6/latest/> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://grafana.com/docs/k6/latest/testing-guides/api-load-testing/> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### html.spec.whatwg.org

- <https://html.spec.whatwg.org/dev/server-sent-events.html> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/13-Asenkron-Notification-SSE-Email-Testleri.md`
- <https://html.spec.whatwg.org/multipage/server-sent-events.html> — köken: `notifications-wiki/04-Architecture/NOTIFICATION-ARCHITECTURE.md`, `notifications-wiki/05-Research/DESIGN-QUESTION-MATRIX.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`

### idus.us.es

- <https://idus.us.es/bitstreams/9470581a-1490-40f6-b5bc-3b7c34cbdbc1/download> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`, `test-wiki/04-Arastirma/11-Test-Oracle-ve-Dogruluk-Katmanlari.md`
- <https://idus.us.es/items/e2306761-742b-47c8-a460-e20fddbf59f7> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### istqb-glossary.page

- <https://istqb-glossary.page/> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`

### json-schema.org

- <https://json-schema.org/draft/2020-12/json-schema-validation> — köken: `test-wiki/04-Arastirma/01-Internet-Arastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### learn.microsoft.com

- <https://learn.microsoft.com/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0> — köken: `api-package-wiki/02-Rules/RULE-0006-Spec-Identity-And-Normalization.md`, `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/02-Rules/RULE-0006-Spec-Identity-And-Normalization.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://learn.microsoft.com/aspnet/core/fundamentals/openapi/overview?view=aspnetcore-10.0> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0?view=aspnetcore-10.0> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://learn.microsoft.com/dotnet/core/resilience/> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://learn.microsoft.com/dotnet/core/resilience/http-resilience> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/request-response> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`
- <https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`
- <https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`, `authenticator-wiki/06-Runbooks/AUTH-SIGNING-MATERIAL-ROLLOVER-AND-OUTAGE.md`
- <https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-10.0> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`
- <https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/implementation/key-storage-providers?view=aspnetcore-10.0> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`
- <https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-10.0> — köken: `notifications-wiki/05-Research/DESIGN-QUESTION-MATRIX.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-10.0> — köken: `notifications-wiki/05-Research/DESIGN-QUESTION-MATRIX.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://learn.microsoft.com/en-us/aspnet/core/signalr/scale?view=aspnetcore-10.0> — köken: `notifications-wiki/04-Architecture/NOTIFICATION-ARCHITECTURE.md`, `notifications-wiki/05-Research/DESIGN-QUESTION-MATRIX.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests> — köken: `test-wiki/04-Arastirma/04-Veritabani-Izolasyonu-ve-API-DB-Dogrulama.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://learn.microsoft.com/en-us/azure/architecture/databases/guide/transactional-out-box-cosmos> — köken: `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/13-Asenkron-Notification-SSE-Email-Testleri.md`
- <https://learn.microsoft.com/en-us/azure/architecture/guide/architecture-styles/event-driven> — köken: `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/10-Problem-Haritasi-ve-Net-Tanim.md`
- <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.typedresults.serversentevents?view=aspnetcore-10.0> — köken: `notifications-wiki/05-Research/DESIGN-QUESTION-MATRIX.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.protocols.configurationmanager-1.getconfigurationasync?view=msal-web-dotnet-latest> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`, `authenticator-wiki/06-Runbooks/AUTH-SIGNING-MATERIAL-ROLLOVER-AND-OUTAGE.md`
- <https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.baseconfigurationmanager?view=msal-web-dotnet-latest> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`, `authenticator-wiki/06-Runbooks/AUTH-SIGNING-MATERIAL-ROLLOVER-AND-OUTAGE.md`
- <https://learn.microsoft.com/en-us/dotnet/api/system.data.common.dbconnection.getschema> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonunknownderivedtypehandling?view=net-10.0> — köken: `notifications-wiki/05-Research/DESIGN-QUESTION-MATRIX.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://learn.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/direct-client-to-microservice-communication-versus-the-api-gateway-pattern> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://learn.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/integration-event-based-microservice-communications> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/13-Asenkron-Notification-SSE-Email-Testleri.md`
- <https://learn.microsoft.com/en-us/dotnet/core/extensions/channels> — köken: `notifications-wiki/04-Architecture/NOTIFICATION-ARCHITECTURE.md`, `notifications-wiki/05-Research/DESIGN-QUESTION-MATRIX.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://learn.microsoft.com/en-us/dotnet/core/testing/mutation-testing> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`, `test-wiki/04-Arastirma/12-Kapsam-Test-Verisi-Durum-ve-Degisim.md`, `test-wiki/04-Arastirma/15-Derin-Bulgular-Problem-Karar-Agaci-ve-Etkiler.md`
- <https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-pack> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-data-commandbehavior> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism> — köken: `notifications-wiki/04-Architecture/NOTIFICATION-ARCHITECTURE.md`, `notifications-wiki/05-Research/DESIGN-QUESTION-MATRIX.md`, `notifications-wiki/05-Research/OFFICIAL-SOURCE-REGISTER.md`
- <https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/> — köken: `database-upstream-docs/T1/04-efmigrationshistory-okuma.md`, `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/history-table> — köken: `database-upstream-docs/T1/04-efmigrationshistory-okuma.md`, `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`, `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/projects> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://learn.microsoft.com/en-us/ef/core/managing-schemas/scaffolding/> — köken: `database-upstream-docs/T1/01-metadata-kaynaklari.md`
- <https://learn.microsoft.com/en-us/nuget/concepts/dependency-resolution> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`, `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing> — köken: `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://learn.microsoft.com/en-us/nuget/reference/cli-reference/cli-ref-delete> — köken: `test-wiki/04-Arastirma/22-Merkezi-Vault-Composition-ve-NuGet-Tuketim-Onerisi.md`
- <https://learn.microsoft.com/en-us/nuget/reference/nuspec> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://learn.microsoft.com/en-us/openapi/openapi.net/overview> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://learn.microsoft.com/en-us/sql/relational-databases/collations/collation-and-unicode-support> — köken: `database-upstream-docs/T1/04-efmigrationshistory-okuma.md`
- <https://learn.microsoft.com/en-us/sql/relational-databases/security/row-level-security> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://learn.microsoft.com/en-us/sql/relational-databases/security/sql-server-security-best-practices> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://learn.microsoft.com/en-us/sql/relational-databases/system-catalog-views/system-catalog-views-transact-sql> — köken: `database-upstream-docs/T1/01-metadata-kaynaklari.md`
- <https://learn.microsoft.com/en-us/sql/relational-databases/system-information-schema-views/system-information-schema-views-transact-sql> — köken: `database-upstream-docs/T1/01-metadata-kaynaklari.md`
- <https://learn.microsoft.com/en-us/sql/tools/sql-database-projects/concepts/schema-comparison> — köken: `database-upstream-docs/T1/02-nesne-modeli-ortak-model.md`
- <https://learn.microsoft.com/en-us/sql/tools/sqlpackage/release-notes-sqlpackage> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`
- <https://learn.microsoft.com/en-us/sql/t-sql/data-types/data-types-transact-sql> — köken: `database-upstream-docs/T1/02-nesne-modeli-ortak-model.md`
- <https://learn.microsoft.com/en-us/sql/t-sql/functions/object-definition-transact-sql> — köken: `database-upstream-docs/T1/01-metadata-kaynaklari.md`
- <https://learn.microsoft.com/sql/relational-databases/backup-restore/restore-a-database-backup-using-ssms> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/samples/databases/adventure-works/README.md`

### learn.openapis.org

- <https://learn.openapis.org/specification/security.html> — köken: `test-wiki/04-Arastirma/15-Derin-Bulgular-Problem-Karar-Agaci-ve-Etkiler.md`

### modelcontextprotocol.io

- <https://modelcontextprotocol.io/docs/tutorials/security/authorization> — köken: `test-wiki/04-Arastirma/05-MCP-AI-ve-Guvenli-Test-Arayuzu.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://modelcontextprotocol.io/docs/tutorials/security/security_best_practices> — köken: `test-wiki/04-Arastirma/05-MCP-AI-ve-Guvenli-Test-Arayuzu.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/14-AI-Destekli-Test-Uretimi-ve-Dogrulama.md`, `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://modelcontextprotocol.io/specification/2025-11-25/basic/authorization> — köken: `test-wiki/04-Arastirma/05-MCP-AI-ve-Guvenli-Test-Arayuzu.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/14-AI-Destekli-Test-Uretimi-ve-Dogrulama.md`, `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://modelcontextprotocol.io/specification/2025-11-25/basic/utilities/tasks> — köken: `test-wiki/04-Arastirma/05-MCP-AI-ve-Guvenli-Test-Arayuzu.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://modelcontextprotocol.io/specification/2025-11-25/schema> — köken: `test-wiki/04-Arastirma/05-MCP-AI-ve-Guvenli-Test-Arayuzu.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://modelcontextprotocol.io/specification/2025-11-25/server/tools> — köken: `test-wiki/04-Arastirma/05-MCP-AI-ve-Guvenli-Test-Arayuzu.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`

### msrc.microsoft.com

- <https://msrc.microsoft.com/create-report](https://aka.ms/opensource/security/create-report> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/SECURITY.md`

### nbomber.com

- <https://nbomber.com/> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### openid.net

- <https://openid.net/specs/openid-connect-core-1_0-18.html> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`

### opensource.microsoft.com

- <https://opensource.microsoft.com/> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/SECURITY.md`
- <https://opensource.microsoft.com/codeofconduct/> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/README.md`
- <https://opensource.microsoft.com/codeofconduct/faq/> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/README.md`

### opentelemetry.io

- <https://opentelemetry.io/docs/specs/otel/trace/api/> — köken: `test-wiki/04-Arastirma/05-MCP-AI-ve-Guvenli-Test-Arayuzu.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://opentelemetry.io/docs/specs/semconv/db/database-spans/> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://opentelemetry.io/docs/specs/semconv/db/sql/> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://opentelemetry.io/docs/specs/semconv/general/trace/> — köken: `test-wiki/04-Arastirma/05-MCP-AI-ve-Guvenli-Test-Arayuzu.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### owasp.org

- <https://owasp.org/API-Security/editions/2023/en/0x10-api-security-risks/> — köken: `test-wiki/04-Arastirma/01-Internet-Arastirmasi.md`
- <https://owasp.org/API-Security/editions/2023/en/0x11-t10/> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://owasp.org/API-Security/editions/2023/en/0xa1-broken-object-level-authorization/> — köken: `test-wiki/04-Arastirma/10-Problem-Haritasi-ve-Net-Tanim.md`, `test-wiki/04-Arastirma/15-Derin-Bulgular-Problem-Karar-Agaci-ve-Etkiler.md`, `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://owasp.org/API-Security/editions/2023/en/0xa5-broken-function-level-authorization/> — köken: `test-wiki/04-Arastirma/10-Problem-Haritasi-ve-Net-Tanim.md`, `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`

### owaspsamm.org

- <https://owaspsamm.org/model/operations/operational-management/stream-a/> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/10-Problem-Haritasi-ve-Net-Tanim.md`

### philmcminn.com

- <https://philmcminn.com/publications/barr2015.pdf> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### piton.com.tr

- <https://piton.com.tr/> — köken: `checker-workspace-wiki/01-Current/CURRENT-0001-CHECKER-MODULE-TRUTH.md`, `test-wiki/05-Uygulama/CURRENT-0002-Checker-Package-And-Composition-Truth.md`, `test-wiki/05-Uygulama/TASK-0002-Merkezi-Vault-Local-Kurulum-ve-Composition-Dogrulamasi.md`

### pmc.ncbi.nlm.nih.gov

- <https://pmc.ncbi.nlm.nih.gov/articles/PMC8400446/> — köken: `test-wiki/04-Arastirma/12-Kapsam-Test-Verisi-Durum-ve-Degisim.md`

### profs.scienze.univr.it

- <https://profs.scienze.univr.it/~ceccato/papers/2020/icst2020api.pdf> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### redocly.com

- <https://redocly.com/docs/respect> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/03-Workflow-Stateful-ve-Arazzo.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://redocly.com/docs/respect/v1/commands/respect> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### research.aalto.fi

- <https://research.aalto.fi/en/publications/prompt-engineering-in-llms-for-automated-unit-test-generation-a-l/> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/10-Problem-Haritasi-ve-Net-Tanim.md`, `test-wiki/04-Arastirma/14-AI-Destekli-Test-Uretimi-ve-Dogrulama.md`

### research.ibm.com

- <https://research.ibm.com/publications/llamaresttest-effective-rest-api-testing-with-small-language-models> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/10-Problem-Haritasi-ve-Net-Tanim.md`, `test-wiki/04-Arastirma/14-AI-Destekli-Test-Uretimi-ve-Dogrulama.md`

### schemathesis.github.io

- <https://schemathesis.github.io/schemathesis/explanations/stateful/> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://schemathesis.github.io/schemathesis/reference/checks/> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### schemathesis.readthedocs.io

- <https://schemathesis.readthedocs.io/en/latest/explanations/data-generation/> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`
- <https://schemathesis.readthedocs.io/en/stable/> — köken: `test-wiki/04-Arastirma/01-Internet-Arastirmasi.md`, `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://schemathesis.readthedocs.io/en/stable/explanations/stateful/> — köken: `test-wiki/04-Arastirma/01-Internet-Arastirmasi.md`
- <https://schemathesis.readthedocs.io/en/stable/reference/checks/> — köken: `test-wiki/04-Arastirma/01-Internet-Arastirmasi.md`

### spec.openapis.org

- <https://spec.openapis.org/arazzo/latest.html> — köken: `test-wiki/04-Arastirma/01-Internet-Arastirmasi.md`, `test-wiki/04-Arastirma/03-Workflow-Stateful-ve-Arazzo.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/13-Asenkron-Notification-SSE-Email-Testleri.md`, `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://spec.openapis.org/oas/latest.html> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`
- <https://spec.openapis.org/oas/v3.1.1.html> — köken: `api-package-wiki/02-Rules/RULE-0006-Spec-Identity-And-Normalization.md`, `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/02-Rules/RULE-0006-Spec-Identity-And-Normalization.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://spec.openapis.org/oas/v3.2.0.html> — köken: `test-wiki/04-Arastirma/01-Internet-Arastirmasi.md`, `test-wiki/04-Arastirma/03-Workflow-Stateful-ve-Arazzo.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/10-Problem-Haritasi-ve-Net-Tanim.md`
- <https://spec.openapis.org/overlay/latest.html> — köken: `test-wiki/04-Arastirma/03-Workflow-Stateful-ve-Arazzo.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### stackoverflow.com

- <https://stackoverflow.com/questions/23289006/on-windows-git-error-sparse-checkout-leaves-no-entry-on-the-working-directory> — köken: `database-upstream-docs/SampleDatabases/SQLServer/AdventureWorks/sql-server-samples/README.md`

### support.abp.io

- <https://support.abp.io/QA/Questions/2785/Consume-external-REST-API-to-get-data> — köken: `api-package-wiki/02-Rules/RULE-0003-Fixed-Folder-Map.md`, `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/02-Rules/RULE-0003-Fixed-Folder-Map.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`

### testing.googleblog.com

- <https://testing.googleblog.com/2020/12/test-flakiness-one-of-main-challenges.html> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/12-Kapsam-Test-Verisi-Durum-ve-Degisim.md`

### www.alation.com

- <https://www.alation.com/blog/canonical-data-models-explained-benefits-tools-getting-started/> — köken: `database-upstream-docs/T1/02-nesne-modeli-ortak-model.md`

### www.apicur.io

- <https://www.apicur.io/registry/docs/apicurio-registry/3.1.x/getting-started/assembly-artifact-reference.html> — köken: `api-package-wiki/03-Decisions/ADR-0007-Data-Model.md`, `api-upstream-wiki/03-Decisions/ADR-0007-Data-Model.md`

### www.asyncapi.com

- <https://www.asyncapi.com/docs/reference/specification/v3.0.0> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`

### www.bytebase.com

- <https://www.bytebase.com/blog/postgres-case-sensitivity/> — köken: `database-upstream-docs/T1/02-nesne-modeli-ortak-model.md`
- <https://www.bytebase.com/blog/top-postgres-schema-compare-tools/> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`

### www.cncf.io

- <https://www.cncf.io/projects/cloudevents/> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`

### www.datafold.com

- <https://www.datafold.com/blog/sunsetting-open-source-data-diff/> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`

### www.iso.org

- <https://www.iso.org/standard/79428.html> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`

### www.microsoft.com

- <https://www.microsoft.com/en-us/research/publication/rest-ler-automatic-intelligent-rest-api-fuzzing/> — köken: `test-wiki/04-Arastirma/01-Internet-Arastirmasi.md`, `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`

### www.nist.gov

- <https://www.nist.gov/itl/ai-risk-management-framework> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/14-AI-Destekli-Test-Uretimi-ve-Dogrulama.md`

### www.npgsql.org

- <https://www.npgsql.org/efcore/> — köken: `database-upstream-docs/T1/04-efmigrationshistory-okuma.md`

### www.nuget.org

- <https://www.nuget.org/packages/Criteo.OpenApi.Comparator> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://www.nuget.org/packages/Microsoft.Extensions.Http.Resilience/10.8.0> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://www.nuget.org/packages/Microsoft.OpenApi.YamlReader> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://www.nuget.org/packages/Microsoft.OpenApi/> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`
- <https://www.nuget.org/packages/microsoft.sqlserver.dacfx/> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`
- <https://www.nuget.org/packages/Swashbuckle.AspNetCore.SwaggerGen/10.0.1> — köken: `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`

### www.oasdiff.com

- <https://www.oasdiff.com/docs/breaking-changes> — köken: `api-package-wiki/02-Rules/RULE-0007-Breaking-Change-Direction.md`, `api-package-wiki/05-Operations/Source-Registry.md`, `api-upstream-wiki/02-Rules/RULE-0007-Breaking-Change-Direction.md`, `api-upstream-wiki/05-Operations/Source-Registry.md`

### www.postgresql.org

- <https://www.postgresql.org/about/news/pgcompare-community-v100-released-free-postgresql-schema-comparison-for-faster-safer-deployments-3115/> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`
- <https://www.postgresql.org/docs/current/catalogs.html> — köken: `database-upstream-docs/T1/01-metadata-kaynaklari.md`
- <https://www.postgresql.org/docs/current/datatype.html> — köken: `database-upstream-docs/T1/02-nesne-modeli-ortak-model.md`
- <https://www.postgresql.org/docs/current/ddl-rowsecurity.html> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://www.postgresql.org/docs/current/functions-info.html> — köken: `database-upstream-docs/T1/01-metadata-kaynaklari.md`
- <https://www.postgresql.org/docs/current/information-schema.html> — köken: `database-upstream-docs/T1/01-metadata-kaynaklari.md`
- <https://www.postgresql.org/docs/current/rules-materializedviews.html> — köken: `database-upstream-docs/T1/02-nesne-modeli-ortak-model.md`
- <https://www.postgresql.org/docs/current/runtime-config-client.html> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://www.postgresql.org/docs/current/sql-createindex.html> — köken: `database-upstream-docs/T1/02-nesne-modeli-ortak-model.md`
- <https://www.postgresql.org/docs/current/sql-set-transaction.html> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://www.postgresql.org/docs/current/sql-syntax-lexical.html> — köken: `database-upstream-docs/T1/02-nesne-modeli-ortak-model.md`, `database-upstream-docs/T1/04-efmigrationshistory-okuma.md`
- <https://www.postgresql.org/docs/current/transaction-iso.html> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/12-Kapsam-Test-Verisi-Durum-ve-Degisim.md`

### www.red-gate.com

- <https://www.red-gate.com/products/sql-compare/pricing/> — köken: `database-upstream-docs/T1/03-build-vs-buy-karari.md`

### www.rfc-editor.org

- <https://www.rfc-editor.org/rfc/rfc2104> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/16-Veri-Tutmayan-Checker-Mimarisi.md`
- <https://www.rfc-editor.org/rfc/rfc8785> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/16-Veri-Tutmayan-Checker-Mimarisi.md`
- <https://www.rfc-editor.org/rfc/rfc9110.html> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/10-Problem-Haritasi-ve-Net-Tanim.md`, `test-wiki/04-Arastirma/12-Kapsam-Test-Verisi-Durum-ve-Degisim.md`, `test-wiki/04-Arastirma/15-Derin-Bulgular-Problem-Karar-Agaci-ve-Etkiler.md`
- <https://www.rfc-editor.org/rfc/rfc9700.html> — köken: `authenticator-wiki/05-Research/Official-Source-Register.md`

### www.schemacrawler.com

- <https://www.schemacrawler.com/> — köken: `database-upstream-docs/T1/01-metadata-kaynaklari.md`

### www.sciencedirect.com

- <https://www.sciencedirect.com/science/article/pii/S2352711024003686> — köken: `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`, `test-wiki/04-Arastirma/09-Test-Kavramlari-Sade-Rehber.md`

### www.w3.org

- <https://www.w3.org/TR/trace-context/> — köken: `test-wiki/04-Arastirma/21-Global-Kaynak-Dogrulamasi-ve-Composition-Fizibilitesi.md`

### www.zaproxy.org

- <https://www.zaproxy.org/docs/automate/automation-framework/> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
- <https://www.zaproxy.org/docs/desktop/addons/openapi-support/automation/> — köken: `test-wiki/04-Arastirma/02-Referans-Proje-Karsilastirmasi.md`, `test-wiki/04-Arastirma/08-Kaynak-Katalogu.md`
<!-- SOURCE-CATALOG:END -->

## 20. Bu dosyayı güncelleme şablonu

Yeni task sonunda ayrı rapor açmak yerine ilgili current/decision/roadmap bölümünü güncelle ve şu kısa kaydı ekle:

```text
### Uygulama sonucu — YYYY-MM-DD
- Kapsam:
- Değişen gerçek:
- Package sürümleri:
- Build/test/scanner sonucu:
- Migration/DB sonucu:
- Secret güvenliği sonucu:
- Temizlenen geçici/duplicate kaynaklar:
- Açık risk veya sonraki ilk adım:
```

Kanıtı olmayan sonuç “tamamlandı” diye yazılmaz.
