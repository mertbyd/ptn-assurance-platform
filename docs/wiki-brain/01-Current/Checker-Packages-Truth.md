---
id: CURRENT-0002
type: current
status: active
title: Checker package current truth
updated: 2026-08-16
decision_refs:
  - ADR-0002
  - ADR-0003
  - ADR-0006
  - ADR-0007
  - ADR-0009
  - ADR-0010
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0004
---

# Checker paketlerinin güncel gerçeği

## Paket sınırı

Her checker bir ABP capability modülüdür. Authenticator, issuer veya kullanıcı yaşam döngüsünün sahibi değildir. Consumer hostun tenant/user bağlamını ve authorization politikasını kullanır.

Composition paketleri Application, EntityFrameworkCore ve HttpApi projelerini transitif olarak taşır. Bu nedenle Controller, AppService, Manager, Repository, provider adapterleri, EF configuration ve migration katmanları korunur.

## API Contract Checker — public `0.2.0-alpha.7`

- `CheckNexus.ApiContracts`
- `CheckNexus.ApiContracts.Application`
- `CheckNexus.ApiContracts.Application.Contracts`
- `CheckNexus.ApiContracts.Domain`
- `CheckNexus.ApiContracts.Domain.Shared`
- `CheckNexus.ApiContracts.EntityFrameworkCore`
- `CheckNexus.ApiContracts.HttpApi`
- `CheckNexus.ApiContracts.HttpApi.Client`

Ana paket `CheckNexus.ApiContracts`; Application, EntityFrameworkCore ve HttpApi katmanlarını referanslar. İnce `Ptn.ApiContractChecker.HttpApi.Host` projesi `IsPackable=false` değerindedir.

## Database Checker — public `0.2.0-alpha.8`

- `CheckNexus.DatabaseComparison`
- `CheckNexus.DatabaseComparison.Application`
- `CheckNexus.DatabaseComparison.Application.Contracts`
- `CheckNexus.DatabaseComparison.Domain`
- `CheckNexus.DatabaseComparison.Domain.Shared`
- `CheckNexus.DatabaseComparison.EntityFrameworkCore`
- `CheckNexus.DatabaseComparison.HttpApi`
- `CheckNexus.DatabaseComparison.HttpApi.Client`

Ana paket `CheckNexus.DatabaseComparison`; Application, EntityFrameworkCore ve HttpApi katmanlarını referanslar. İnce `Ptn.DatabaseChecker.HttpApi.Host` projesi `IsPackable=false` değerindedir.

## Public kayıt

İki aile de `mertbyd` sahibi altında NuGet.org'a yayımlanmıştır. API Contract Checker'ın son
public sürümü **`0.2.0-alpha.7`** (2026-08-16), Database Checker'ın son public sürümü
**`0.2.0-alpha.8`** (2026-08-15)'dir. API ailesinin PackageValidation baseline'ı
`0.2.0-alpha.2`, Database ailesinin baseline'ı **`0.2.0-alpha.7`**'dir. Yayımlanan hiçbir sürüm farklı binary
ile tekrar yayımlanmaz; sonraki geliştirme yeni prerelease veya stable sürüm üretir.

> [!NOTE] Kaynak, registry ve Test Module hizalı
> `checkers/database-comparison/common.props`, NuGet.org'daki sekiz PackageId ve Test Module
> `common.props` **`0.2.0-alpha.8`** seviyesindedir. Release manifest `alpha.2`, `alpha.6`,
> `alpha.7` ve `alpha.8` sürümlerini immutable olarak kaydeder. API Contracts kaynak,
> registry ve Test Module consumer'ında `alpha.7`; Vault consumer sürümü `alpha.2`dir.

## `0.2.0-alpha.1` public release

İki aile source'ta ve NuGet.org'da aynı `0.2.0-alpha.1` sürümündedir.
Her sekizli paket ailesi `0.1.0-alpha.5` baseline'ına karşı PackageValidation,
deterministik CI build, repository metadata, Azure Repos SourceLink ve `.snupkg`
üretim kapılarını ortak `common.props` üzerinden taşır.

API Contract Checker'ın KBP-621 JSON Schema doğrulaması
[[03-Decisions/ADR-0009-Api-Json-Schema-Dependency|ADR-0009]] uyarınca
`NJsonSchema` 11.6.1'i public/transitif runtime bağımlılığı olarak getirir. Consumer
restore grafiği bu bağımlılığı dışlayamaz; sürüm uyumu ince-host ve hedef-host smoke'u
ile doğrulanır.

Mevcut AppService ve repository interface'lerine eklenen sekiz üye, interface'i kendisi
implement eden consumer'lar için bilinçli 0.2 kırığıdır. PackageValidation kabulü
[[03-Decisions/ADR-0010-Api-Contracts-Interface-Expansion|ADR-0010]] uyarınca yalnız iki
assembly'deki tam `CP0006` üye hedefleriyle sınırlandırılmıştır. Yeni bir public kırık pack
kapısını yeniden durdurur.

## `0.2.0-alpha.2` public release — 2026-08-12

Bu tarihte iki ailenin de son public ve immutable sürümü `0.2.0-alpha.2` idi. Bu sürümle gelen sözleşmeler:

- Database Checker `FindingDto.Address` ile altı fingerprint adres bileşenini yayınlar;
  `FindingQueryInput` artık `SinceRunId` ve en çok 100 SHA-256 `Fingerprints` alır.
- API Contract Checker sekiz bileşenli `FindingAddressDto` sözleşmesini exact fingerprint
  grameriyle belgeler; findings query aynı bounded bakım-anı filtrelerini taşır.
- İki tarafta referans run tenant/tanım ilişkisi Manager/Repository zincirinde doğrulanır,
  null legacy fingerprint New sayılmaz ve count/page aynı server-side seçimi kullanır.
- Persisted shape değişmedi; migration üretilmedi.

Yayın 2026-08-12'de yapıldı: 16 `.nupkg` + 16 `.snupkg` NuGet.org'a push edildi ve
push sonrası NuGet V3 flat-container sorgusuyla **16/16 PackageId** için `0.2.0-alpha.2`
sürümü doğrulandı. PackageValidation baseline'ı `0.2.0-alpha.1`'dir; API ve Database
Domain'de yalnız gözlenen yeni repository üyelerine yönelik tam `CP0006` suppression'ları
kaldı, eski `ClassifyAsync(Guid)` overload'u korunarak `CP0002` kırığı giderildi.
Yayın öncesi kapılar: Database 169/169, API 291/291 test; iki composition smoke 1/1;
clean-cache consumer restore/build 0 uyarı. Kayıt için
[[05-Operations/Package-Release-Ledger|LEDGER-0001]].

## `0.2.0-alpha.5` API Contract Checker public release — 2026-08-14

API Contract Checker'ın sekizli paket ailesi `0.2.0-alpha.5` olarak yayımlandı. Bu sürüm:

- `POST /conformance/sample-sets` ile request şemasından deterministik sınır ve negatif alan
  örnekleri üretir; alan başına bütçe uygular ve değerleri mevcut retention/redaction
  politikasından geçirir.
- `POST /conformance/operation-links` ile yalnız mekanik kanıta dayanan adaylar üretir:
  declared OpenAPI link `1.0`, exact response-property/target-parameter şema eşleşmesi `0.8`,
  tekil çözülen `201 Location` örneği `0.7`; eşik `0.65` ve tüm adaylarda insan onayı zorunludur.
- OpenAPI snapshot projeksiyonunda response `links` bildirimlerini ve header `example`
  değerlerini korur; `GenerateSamples` ve `SuggestLinks` permission'larını public sözleşmeye ekler.
- Persisted shape'i değiştirmez; migration üretilmedi.

Release build 0 uyarı/0 hata, testler 311/311, backend scanner 44/44 değişmiş backend dosyası
temiz ve PackageValidation başarılıdır. Paketleme kapısı 8 `.nupkg` + 8 `.snupkg` doğruladı;
push sonrası NuGet V3 flat-container sorgusuyla **8/8 PackageId** için `0.2.0-alpha.5`
doğrulandı. Baseline `0.2.0-alpha.2`'dir; iki yeni AppService interface metodu yalnız exact-member
`CP0006` suppression'larıyla sınırlandı. Kaynak commit:
`51a42ae677a11d20f425346dc0a92fef48bbf7fa`.

## `0.2.0-alpha.7` API Contract Checker public release — 2026-08-16

KBP-630, snapshot belgesinden hesaplanan sayfalı operasyon envanterini açtı:
`ListOperationsAsync` ve `GET .../{id}/operations`. Satır yalnız operasyon kimliği, HTTP
metodu, path ve request/response şema referanslarını taşır; filtreler kapalı kümedir ve sonuç
sayfa/yanıt byte bütçesini raporlar. Persisted shape değişmedi, migration üretilmedi.

Kaynak dört ayrı KBP-630 commit'iyle tamamlandı: `a3fcf87`, `d76ae6b`, `1565ef1`, `30aa9ea`.
Release build 0 hata verdi ve **322/322** test geçti. Manifest tabanlı skill motorunun pushesiz
gate'i sekiz `.nupkg` ile sekiz `.snupkg` dosyasını, PackageValidation'ı ve paket içindeki
assembly'leri doğruladı. `0.2.0-alpha.6` boş/immutable olduğundan atlandı; baseline
`0.2.0-alpha.2` olarak korundu. Push sonrası NuGet V3 flat-container sorgusunda `alpha.7`
**8/8 PackageId** için doğrulandı.

Test Module pini `60d3f5d` ile `alpha.7`ye yükseltildi. Consumer Release build 0 hata verdi;
Domain 215/215, Application 74/74 ve EF Core 27/27 olmak üzere **316/316** test geçti.

## `0.2.0-alpha.8` Database Checker public release — 2026-08-15

Database Checker'ın sekizli paket ailesi skill'in manifest tabanlı yayın motoruyla
`0.2.0-alpha.8` olarak yayımlandı. Zorunlu pushesiz gate ve push öncesi tekrarlanan gate;
restore, Release build, **228/228** test, PackageValidation, nuspec dependency/README/repository
metadata denetimi ve yasak host/test/config içeriği taramasını geçti. **8 `.nupkg` + 8
`.snupkg`** üretildi; push sonrası NuGet V3 flat-container sorgusunda `alpha.8` **8/8
PackageId** için son sürüm olarak doğrulandı. Baseline `0.2.0-alpha.7`'dir.

Test Module `CheckNexusDatabaseComparisonVersion` değerini `alpha.8`e yükseltti. Aynı consumer
doğrulamasında o tarihteki API Contracts `alpha.5` ve Vault `alpha.2` ile build 0 hata verdi;
Domain 90/90, Application 46/46 ve EF Core 27/27 olmak üzere **163/163** test geçti. API
Contracts pini daha sonra `alpha.7`ye yükseltildi; güncel consumer kapısı yukarıda kayıtlıdır.

## Bilinçli olarak pakete girmeyenler

- Executable host projeleri
- Test projeleri ve TestBase
- Yerel secret veya ortam config’i
- Authenticator ve Notifications implementasyonları
- Consumer Test Module UI/host kodu

Hostların source tree’de kalması paket sınırıyla çelişmez: hostlar Swagger, HTTP, DI, EF model ve migration smoke doğrulaması sağlar.

## Database Checker — oracle yüzeyi (public `0.2.0-alpha.8`)

[[03-Decisions/ADR-0007-Checker-Oracle-Surface|ADR-0007]] uyarınca Database Checker artık
karşılaştırma dışında iki salt-okunur tüketici yüzeyi taşır:

| Yüzey | Endpoint | Ne döner |
|---|---|---|
| Assertion | `POST api/comparison/assertions/{row\|count\|absent\|batch}` | `AssertionOutcomeCode` + gözlem + başarısız beklentiler (~200 bayt) |
| Teşhis | `POST api/comparison/diagnosis` | RFC 9457 + sıralı hipotezler ve kanıt (≤ 4 KB) |
| Tablo tanımı | `GET .../schema-discovery/.../describe` | kolon/PK/unique/FK komşuları (yazım anı) |

Ek olarak hedef bağlantılar emniyet profili taşır (TLS modu, sertifika doğrulaması,
statement/lock timeout, `READ ONLY` transaction, `ApplicationName`) ve veri farkı değerleri
`ValueRetentionMode` politikasından geçer (varsayılan `None`).

### `alpha.3`–`alpha.6` ile eklenen yüzeyler

`0.2.0-alpha.2`'den sonra gelen ve bu sayfada uzun süre kayıtsız kalan public sözleşmeler
(kaynakta doğrulandı, 2026-08-15):

| Yüzey | Tip / uç | Ne için |
|---|---|---|
| Projeksiyon | `IProjectionAppService` · `api/comparison/projections` | Salt-okunur projeksiyon yüzeyi — AUDIT-0003 **#06**'nın kapanışı (KBP-712) |
| Assertion türetilebilirliği | `IAssertionDerivabilityAppService` · `ValidateDerivabilityAsync` · `.../assertions/derivability` | Şema tarafının türetilebilirlik kapısı — AUDIT-0003 **#03**'ün kapanışı; RULE-0006'nın ikinci kapısı |
| Yazma kümesi yeteneği | `IWriteSetCapabilityAppService` · `capabilities/write-set/{probe\|capture\|release}` | Etki ayak izi yeteneğinin yoklanması (ADR-0019 §E) |
| Şema parmak izi | `ISchemaDiscoveryAppService.GetSchemaFingerprintAsync` · `GET .../schema-discovery/{connectionId}/fingerprint` | ADR-0020 malzeme mührünün DB tarafı (KBP-714) |
| Korelasyon | `CorrelationRefDto` | Her çağrıda taşınan ve sonuçta aynen yansıtılan `{TraceId, StepKey}` çifti (ADR-0021, KBP-711) |

Bu yüzeylerin ilk public kümesi `0.2.0-alpha.6` ile açılmıştır. Son kaynak durumu
`0.2.0-alpha.8` paket ailesiyle NuGet.org'da yayımlıdır ve Test Module tarafından tüketilir.
