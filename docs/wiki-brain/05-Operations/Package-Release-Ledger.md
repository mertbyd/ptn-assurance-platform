---
id: LEDGER-0001
type: guide
status: active
title: Package release ledger
updated: 2026-08-16
decision_refs:
  - ADR-0006
  - ADR-0009
  - ADR-0010
  - ADR-0012
rule_refs:
  - RULE-0001
---

# Paket yayın defteri

> [!IMPORTANT]
> Yeni sürüm hazırlamadan önce [[NuGet-Package-Release-Playbook|GUIDE-0003]] açılır.
> Relative script yolu yalnız doğrulanmış repository kökünde kullanılır.

## `0.1.0-alpha.5` — 2026-08-11

**Owner:** `mertbyd`  
**Registry:** NuGet.org  
**Durum:** Yayımlandı

### API Contract Checker

- `CheckNexus.ApiContracts`
- `CheckNexus.ApiContracts.Application`
- `CheckNexus.ApiContracts.Application.Contracts`
- `CheckNexus.ApiContracts.Domain`
- `CheckNexus.ApiContracts.Domain.Shared`
- `CheckNexus.ApiContracts.EntityFrameworkCore`
- `CheckNexus.ApiContracts.HttpApi`
- `CheckNexus.ApiContracts.HttpApi.Client`

### Database Checker

- `CheckNexus.DatabaseComparison`
- `CheckNexus.DatabaseComparison.Application`
- `CheckNexus.DatabaseComparison.Application.Contracts`
- `CheckNexus.DatabaseComparison.Domain`
- `CheckNexus.DatabaseComparison.Domain.Shared`
- `CheckNexus.DatabaseComparison.EntityFrameworkCore`
- `CheckNexus.DatabaseComparison.HttpApi`
- `CheckNexus.DatabaseComparison.HttpApi.Client`

### Vault

`CheckNexus.Vault` source sürümü `0.1.0-alpha.5`tir ancak bu tarihte NuGet.org public kaydı yoktur.

## `0.2.0-alpha.1` — 2026-08-12

**Owner:** `mertbyd`  
**Registry:** NuGet.org  
**Durum:** Yayımlandı; iki sekizli ailenin bütün PackageId'leri registry'de doğrulandı

- API Contract Checker ve Database Checker sekizli aileleri aynı sürüme hizalandı.
- PackageValidation baseline'ı her PackageId için `0.1.0-alpha.5` olarak sabitlendi.
- Repository metadata, Azure Repos SourceLink, deterministic CI build ve `.snupkg`
  üretimi ortak paket kapısına alındı.
- API ailesinin yeni public/transitif `NJsonSchema` 11.6.1 bağımlılığı ADR-0009 ile
  kabul edildi.
- API ailesinin mevcut AppService/repository interface'lerine eklenen sekiz üyesi ADR-0010
  ile bilinçli 0.2 kırığı olarak kaydedildi; suppression yalnız tam `CP0006` üye hedeflerini
  kapsar. Eski `Finding` ctor'u ve Database PostgreSQL tester tipi shim ile korundu.
- Alpha kapısı mevcut ince test hostlarında module initialization, route/Swagger
  görünürlüğü ve EF model smoke'unu kapsar.
- Stable release; consumer Test Module'deki migration, Vault, tenant ve authorization
  kapıları kaydedilmeden açılamaz.

## `0.2.0-alpha.2` — 2026-08-12

**Owner:** `mertbyd`  
**Registry:** NuGet.org  
**Durum:** Yayımlandı; push sonrası 16/16 PackageId flat-container sorgusuyla doğrulandı  
**PackageValidation baseline:** `0.2.0-alpha.1`

İki sekizli aile (`CheckNexus.ApiContracts*` ve `CheckNexus.DatabaseComparison*`)
`0.2.0-alpha.1` bölümündeki PackageId listesinin aynısıyla yayımlandı.

### Bu sürümle gelen public sözleşmeler

- **Database Checker:** `FindingDto.Address` altı fingerprint adres bileşenini
  (`SourceEngineCode → TargetEngineCode → SchemaName → ObjectTypeCode → ObjectName → ChildName`)
  yayınlar; gösterim grameri `schema.object[.child]`; fingerprint girdisi gösterim adresinden
  üretilmez (`N` / `V{UTF-16-length}:{value}`), altı bileşeni `KindCode` ve normalize delta izler,
  çıktı uppercase SHA-256.
- **API Contract Checker:** sekiz bileşenli `FindingAddressDto`
  (`OperationId → HttpMethod → Path → SchemaName → PropertyPath → ParameterName → ResponseStatus → MediaType`);
  fingerprint sırası `KindCode → DirectionCode → sekiz adres bileşeni → OldDelta → NewDelta`,
  bileşenler `{UTF-16-length}:{value}` olarak `|` ile birleşir.
- **İki tarafta bakım-anı filtreleri:** `Guid? SinceRunId` ve en çok 100 adet exact 64-hex
  `Fingerprints`; empty Guid, blank/geçersiz hash, case-insensitive duplicate ve sınır aşımı
  FluentValidation ile reddedilir. Referans run aynı tenant içinde daha eski ve `Completed`
  olmak zorundadır; legacy null fingerprint `New` sayılmaz.
- Persisted shape değişmedi; **migration üretilmedi**; yeni controller veya route açılmadı.

### Kapılar

- Database 169/169, API 291/291 test; composition smoke API 1/1, Database 1/1.
- Backend scanner temiz (26 değişmiş C# dosyası); attributes-free build'lerde RMG diagnostic yok.
- Clean-cache consumer restore/build 0 uyarı, 0 hata (`0.2.0-alpha.1` üzerinde alınmıştır;
  `0.2.0-alpha.2` üzerinde tekrarlanacaktır).
- API ve Database Domain'de yalnız gözlenen yeni repository üyelerine yönelik tam `CP0006`
  suppression'ları kaldı; eski `ClassifyAsync(Guid)` overload'u korunarak `CP0002` kırığı giderildi.

### Bilinen borç — **kapandı (2026-08-13)**

> [!NOTE]
> Önceki kayıt şuydu: "Bu sürümün kaynağı sürüm kontrolünde değildir… Paketler SourceLink
> repository metadata taşır fakat işaret edilen commit mevcut değildir."
> Borç kapandı ve **kaydın ikinci cümlesi yanlıştı**; düzeltmesi aşağıdadır.

**Kaynak artık sürüm kontrolünde.** İki modülün gerçek geliştirme geçmişi upstream
worktree'lerinden **aktarıldı** (uydurulmadı): `api-contract` 27 dal / 55 commit, orijinal
SHA'larla; `database-comparison` 16 yerel dal + 17 remote-tracking ref / 94 commit. Workspace
ağacı ile upstream ucu arasındaki fark her modülde **ayrı ve gerçek** bir commit'tir. Ayrıntı
her modülün `PROVENANCE.md` dosyasındadır.

**SourceLink iddiası hiç yanlış olmadı.** Yayımlanmış 16 paketin `.nuspec`'i tek tek okundu:
**16/16 yalnız repository URL taşıyor, `commit` özniteliği yok.** Hiçbir paket var olmayan ya
da yanlış bir commit'e işaret etmiyor, dolayısıyla geri alınacak bir iddia da yok. Bu tesadüf
değil: `common.props` CI dışı derlemede SourceLink'i kapatıyor
(`EnableSourceLink = false` when `PtnSourceRepositoryMetadataAvailable != true`).

**Etiket.** `v0.2.0-alpha.2` iki modülde de import commit'ine atıldı; **kanıtla**: ağaçtan
`dotnet pack -c Release` ile üretilen paketler yayımlanmış kopyalarla karşılaştırıldı ve her
`lib/` assembly'si SHA-256 düzeyinde **16/16 birebir eşleşti**. `0.1.0-alpha.5` ve
`0.2.0-alpha.1` **etiketlenmedi**: alpha.1'in 16 paketinin assembly byte'ları alpha.2'den
farklı (yani bu ağaç onu üretmiş olamaz) ve alpha.5'in yerel bir artefaktı hiç yok.

### Açık uyarı — Database Checker sürüm çakışması riski · **kapandı (2026-08-15)**

> [!NOTE]
> Önceki kayıt şuydu: *"Database Checker'ın `common.props` sürümü hâlâ `0.2.0-alpha.2`…
> yeni prerelease seçilmeli ve `PackageValidationBaselineVersion` `0.2.0-alpha.2` yapılmalıdır."*
> **Bu uyarı bayattır ve istediği iş yapılmıştır.**

API Contract Checker tarafındaki risk `0.2.0-alpha.5` yayınıyla kapandı. Database Checker
tarafında da çakışma riski kalmadı: kaynak `common.props` bugün **`0.2.0-alpha.8`**,
`PackageValidationBaselineVersion` **`0.2.0-alpha.7`**'dir ve release manifest yayımlanmış
`alpha.2`/`alpha.6`/`alpha.7`/`alpha.8` sürümlerini immutable olarak kaydeder.

## API Contract Checker `0.2.0-alpha.5` — 2026-08-14

**Owner:** `mertbyd`

**Registry:** NuGet.org

**Durum:** Yayımlandı; push sonrası 8/8 PackageId flat-container sorgusuyla doğrulandı

**PackageValidation baseline:** `0.2.0-alpha.2`
**Kaynak commit:** `51a42ae677a11d20f425346dc0a92fef48bbf7fa`

### Paket ailesi

- `CheckNexus.ApiContracts.Domain.Shared`
- `CheckNexus.ApiContracts.Domain`
- `CheckNexus.ApiContracts.Application.Contracts`
- `CheckNexus.ApiContracts.Application`
- `CheckNexus.ApiContracts.EntityFrameworkCore`
- `CheckNexus.ApiContracts.HttpApi`
- `CheckNexus.ApiContracts.HttpApi.Client`
- `CheckNexus.ApiContracts`

### Bu sürümle gelen public sözleşmeler

- `POST /conformance/sample-sets`: request şemasından deterministik sınır ve negatif alan
  örnekleri; alan bütçesi ve mevcut retention/redaction politikası.
- `POST /conformance/operation-links`: declared link (`1.0`), exact şema eşleşmesi (`0.8`) ve
  tekil `201 Location` örneği (`0.7`) kaynaklı adaylar; `0.65` eşik ve zorunlu insan onayı.
- Yeni `GenerateSamples` ve `SuggestLinks` permission'ları; OpenAPI response link ve header
  example kanıtlarının snapshot projeksiyonunda korunması.
- Persisted shape değişmedi; migration üretilmedi.

### Kapılar ve yayın kanıtı

- Release build 0 uyarı, 0 hata; Application 25/25, Domain 141/141, EF Core 145/145,
  toplam **311/311** test başarılı.
- Backend scanner 44 değişmiş backend dosyasını temiz raporladı; task-scoped format ve
  `git diff --check` kapıları geçti.
- PackageValidation iki yeni AppService interface metodunu yalnız exact-member `CP0006`
  suppression'larıyla kabul etti.
- 8 `.nupkg` + 8 `.snupkg` doğrulandı ve NuGet.org'a push edildi.
- Push sonrası NuGet V3 flat-container sorgusunda `0.2.0-alpha.5` **8/8 PackageId** için
  son sürüm olarak doğrulandı.

## API Contract Checker `0.2.0-alpha.7` — 2026-08-16

- **Owner:** `mertbyd`
- **Registry:** NuGet.org
- **Durum:** Yayımlandı; push sonrası 8/8 PackageId flat-container sorgusuyla doğrulandı
- **PackageValidation baseline:** `0.2.0-alpha.2`
- **Kaynak commit'leri:** `a3fcf87`, `d76ae6b`, `1565ef1`, `30aa9ea`

### Bu sürümle gelen public sözleşme

- Snapshot operasyon envanteri `ListOperationsAsync` ve `GET .../{id}/operations` ile
  sayfalı, kapalı-küme filtreli ve byte bütçeli açıldı.
- Hafif liste satırı yalnız `OperationId`, method, path ve iki şema referansını taşır.
- Envanter snapshot belgesinden hesaplanır; tablo, kolon veya migration eklenmedi.
- Boş yayımlanmış immutable `0.2.0-alpha.6` yeniden kullanılmadı; kaynak `alpha.5`ten
  doğrudan `alpha.7`ye çıkarıldı.

### Kapılar ve yayın kanıtı

- Manifest tabanlı skill motorunun pushesiz gate'i restore, Release build, **322/322** test,
  PackageValidation ve sekiz paketin assembly içeriğini doğruladı.
- **8 `.nupkg` + 8 `.snupkg`** üretildi; scoped key kullanıcı tarafından güvenli prompt'a
  girildi ve push ayrıca onaylandı.
- Push sonrası NuGet V3 flat-container sorgusunda `0.2.0-alpha.7` **8/8 PackageId** için
  doğrulandı.
- Test Module consumer pini `60d3f5d` ile `alpha.7`ye yükseltildi; Release build 0 hata,
  test **316/316** geçti.

## Database Checker `0.2.0-alpha.6`

**Owner:** `mertbyd`  
**Registry:** NuGet.org  
**Durum:** Yayımlandı — release manifest `immutableVersions` kaydıyla doğrulanmıştır  
**PackageValidation baseline:** `0.2.0-alpha.2`

`0.2.0-alpha.2`'den sonra Database ailesinin ikinci public sürümüdür ve
[[01-Current/Checker-Packages-Truth|CURRENT-0002]]'de listelenen `alpha.6` yüzeylerini taşır:
`IProjectionAppService`, `IAssertionDerivabilityAppService`, `IWriteSetCapabilityAppService`,
`GetSchemaFingerprintAsync` şema parmak izi ucu ve `CorrelationRefDto`. Bu yüzeyler
AUDIT-0003 **#03** ve **#06** bulgularını kapatır (KBP-712/713/714) ve ADR-0020 malzeme
mührünün DB tarafını besler.

**Tarihsel kanıt:** `checkers/database-comparison/scripts/database-comparison.release.json`
bu sürümü immutable listesinde taşır; sekizli aile
`artifacts/kbp-713-alpha6/` altında `.nupkg` + `.snupkg` olarak duruyor. Test Module
bu sürümü daha sonra `alpha.8` ile yükseltmiştir.

> **Doğrulandı — `alpha.3` / `alpha.4` / `alpha.5` public değildir (2026-08-15).** NuGet V3
> flat-container sorgusu sekiz PackageId için public sürümleri `alpha.1`, `alpha.2`, `alpha.6`,
> `alpha.7`, `alpha.8` olarak döndürdü. Yerel `alpha.5` artefaktı yayın kanıtı değildir.

### `0.2.0-alpha.7` — public tarihsel baseline

NuGet V3 flat-container sorgusu `alpha.7`yi sekiz PackageId'nin tamamında doğruladı. Bu sürüm
`alpha.8` PackageValidation baseline'ıdır ve release manifestte immutable olarak kayıtlıdır.

## Database Checker `0.2.0-alpha.8` — 2026-08-15

**Owner:** `mertbyd`

**Registry:** NuGet.org

**Durum:** Yayımlandı; push sonrası 8/8 PackageId flat-container sorgusuyla doğrulandı

**PackageValidation baseline:** `0.2.0-alpha.7`

### Kapılar ve yayın kanıtı

- Skill'in manifest tabanlı yayın motoru önce `-Push` olmadan, sonra push öncesinde aynı tam
  gate'i yeniden çalıştırdı.
- Release build geçti; çalıştırılabilir test projelerinde **228/228** test başarılı oldu.
- Sekiz exact proje PackageValidation ile paketlendi; **8 `.nupkg` + 8 `.snupkg`** için ID,
  version, dependency, repository metadata, README ve yasak host/test/config içeriği denetlendi.
- NuGet.org exact-version preflight `alpha.8`in boş olduğunu doğruladı; push sonrasında sekiz
  PackageId'nin tamamında `alpha.8` son sürüm olarak okundu.
- Test Module `CheckNexusDatabaseComparisonVersion = 0.2.0-alpha.8` ile güncellendi. O günkü
  API Contracts `alpha.5` ve Vault `alpha.2` ile consumer build 0 hata, test **163/163** geçti.
  API Contracts daha sonra `alpha.7`ye yükseltildi; güncel consumer sonucu **316/316**'dır.

## CheckNexus.Vault `0.2.0-alpha.2`

**Owner:** `mertbyd`  
**Registry:** NuGet.org  
**Durum:** Yayımlandı — tek PackageId (`CheckNexus.Vault`)

Vault adapterinin ilk public kaydıdır ve bu defterde **hiç kayıtlı değildi**; bu boşluk
2026-08-15'te kapatılmıştır. `0.1.0-alpha.5` bölümü *"bu tarihte NuGet.org public kaydı
yoktur"* diyor ve o cümle o tarih için doğrudur — sonrasında yayımlanan `0.2.0-alpha.2`
kaydı düşülmemişti. [[01-Current/Platform-Truth|CURRENT-0001]] ve
[[01-Current/Vault-Truth|CURRENT-0003]] sürümü public sayıyordu; defter ile Current sayfası
arasındaki bu çelişki artık yok.

Paket iki checker'ın `0.2.0-alpha.2` `Domain` paketine bağımlıdır (`release-manifest.json`
`requiredDependencies`) ve Test Module hostunda compose edilmiştir; `common.props`'ta
`CheckNexusVaultVersion = 0.2.0-alpha.2`.

> **Kalan borç — KBP-98.** Vault `release-manifest.json` yayımlanmış `0.2.0-alpha.2`yi artık
> immutable olarak işaretler; csproj repository metadata, GitHub SourceLink, symbol package ve
> `EnablePackageValidation` taşır. PackageValidation hâlâ açık bir baseline sürümü olmadan
> çalışır; stable release öncesi compatibility politikası ayrıca netleştirilmelidir.

## Notifications `0.1.0-alpha.1` — 2026-08-12

**Owner:** `mertbyd`  
**Registry:** NuGet.org  
**Durum:** Altı PackageId yayımlandı ve registry'de doğrulandı

- `Pintern.SaaS.Notifications.Domain.Shared`
- `Pintern.SaaS.Notifications.Domain`
- `Pintern.SaaS.Notifications.Application.Contracts`
- `Pintern.SaaS.Notifications.Application`
- `Pintern.SaaS.Notifications.EntityFrameworkCore`
- `Pintern.SaaS.Notifications.HttpApi`

Authenticator public envanteri için
[[NuGet-Package-Release-Playbook|GUIDE-0003]] esas alınır.

## Foundation `1.0.0` — 2026-08-13

**Owner:** `mertbyd`

**Registry:** NuGet.org

**Durum:** Yayımlandı; yedi PackageId registry'de doğrulandı

- `Nexum.Abp.Foundation.Domain.Shared`
- `Nexum.Abp.Foundation.Domain`
- `Nexum.Abp.Foundation.Application.Contracts`
- `Nexum.Abp.Foundation.Application`
- `Nexum.Abp.Foundation.EntityFrameworkCore`
- `Nexum.Abp.Foundation.HttpApi`
- `Nexum.Abp.Foundation.HttpApi.Client`

## Authenticator `2.0.0` — 2026-08-13

**Owner:** `mertbyd`

**Registry:** NuGet.org

**Durum:** Yayımlandı; sekiz PackageId registry'de doğrulandı

- Sekiz PackageId tek immutable major sürüme hizalandı.
- Her katman eşlenen Foundation `1.0.0` paketini NuGet ve ABP module dependency olarak taşır.
- Public base zinciri değiştiği için `1.x` üzerine yazılmadı; API compatibility farkı yeni
  major sürümle yönetildi ve suppression eklenmedi.
- Release build 0 uyarı/0 hata; 215/215 test geçti.
- 8 `.nupkg` + 8 `.snupkg` metadata, README, repository type, exact dependency ve yasak
  içerik açısından doğrulandı.
- Yalnız Authenticator direct reference kullanan clean consumer yedi Foundation paketini
  transitif çözerek ABP application initialization/shutdown smoke'unu geçti.
- NuGet.org exact `2.0.0` preflight'ta sekiz PackageId de boştu; onaylı push sonrasında
  sekiz PackageId'nin tamamı registry'de doğrulandı.

## Authenticator `2.1.0` — 2026-08-17

**Owner:** `mertbyd`

**Registry:** NuGet.org

**Durum:** Yayımlandı; sekiz katman + host tool paketi doğrulandı

- Issuer artık `ResourceServers:Registrations` listesindeki her kayıt için scope ve audience
  üretir. `MachineClientId` verilen kayıt, yalnız token/revocation ucu, `client_credentials`
  grant'ı ve kendi tek scope'unu taşıyan confidential makine istemcisi alır; password grant,
  refresh token ve kullanıcı scope'ları verilmez.
- **Consumer etkisi:** `2.0.0` hiçbir client'a `client_credentials` vermiyordu; ayrı deploy
  edilen bir resource server bu sürüm olmadan eşleşen `aud` ile token alamaz.
- Makine istemcisi tanımlıyken secret zorunludur; eksikse host fail-fast kapanır.
- Release build 0 hata/0 uyarı; 218/218 test geçti.
- `Authenticator.HttpApi.Host` dotnet tool paketi de aynı sürüme çıktı — hostu tool olarak
  çalıştıran ortamlar güncellemeden yeni seed davranışını almaz.

## CheckNexus `0.2.0-alpha.9` — 2026-08-17

**Owner:** `mertbyd`

**Registry:** NuGet.org

**Durum:** Yayımlandı; iki sekizli aile aynı sürüme hizalandı, 16 nupkg + 16 snupkg

- **Kırıcı değişiklik:** lookup rotaları modül önekine taşındı —
  `api/lookups/*` → `api/api-contract/lookups/*` ve `api/database-comparison/lookups/*`.
- Sebep: iki aile ortak `api/lookups` isim alanını sahipsiz paylaşıyordu. Tek composition
  hostta compose edildiklerinde `difference-kinds` çakışıyor, Swagger üretimi
  `SwaggerGeneratorException` ile düşüyor ve o rota gerçek isteklerde belirsiz kalıyordu.
- `ConflictingActionsResolver` kullanılmadı; o uçlardan birini sessizce gizlerdi.
- Test: api-contract 322/322, database-comparison 228/228.
- Consumer Test Module `0.2.0-alpha.9`'a alındı; 389/389 test geçti ve temiz klon restore'u
  yalnız nuget.org kaynağıyla 370 paketi çözdü.

## Değişmez yayın kuralları

- Yayımlanan sürüm immutable kabul edilir.
- Aynı PackageId + Version farklı içerikle tekrar kullanılmaz.
- Paket ailesi mümkün olduğunca aynı release sürümünde tutulur.
- Release notu migration, config, secret ve consumer breaking change bilgisini açıkça taşır.
- Public package için local feed kopyası kanonik registry’nin önüne geçirilmez.
