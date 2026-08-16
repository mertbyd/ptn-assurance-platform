---
name: acc-lookup-recipe
description: Add a new lookup table end to end in ApiContractChecker — entity, immutable stable code constants, seed contributor, EF configuration, DbSet, DTOs, validators, AppService and passivating controller. Use whenever a classification, status, kind, format or type value needs to be persisted, because every enum in this repository becomes a lookup.
---

# Lookup ekleme tarifi

Kural: **her enum bir lookup olur**
([kanonik checker kuralları](../../../../../docs/PTN-ASSURANCE-PLATFORM-WIKI.md#13-checker-gelistirme-kurallari)).
Okuma/create/görünür-alan update/pasifleştirme tabandan geldiği için iş 6 dosyaya
iner — **tabanı kopyalayarak yeni yönetim akışı yazma**.

## Dosyalar

| # | Dosya | İçerik |
|---|---|---|
| 1 | `Domain.Shared/Constants/{Alan}/Lookups/{Ad}Codes.cs` | Kararlı kod sabitleri |
| 2 | `Domain/Entities/Lookups/{Ad}.cs` | `LookupEntity` türer, yalnız ctor |
| 3 | `Domain/Data/{Ad}DataSeedContributor.cs` | Kod → satır, **idempotent** |
| 4 | `EntityFrameworkCore/Configurations/Lookups/{Ad}Configuration.cs` | Tablo + unique `Code` index |
| 5 | `EntityFrameworkCore/…DbContext.cs` (+ arayüz) | `DbSet<{Ad}>` |
| 6 | DTO + validator + AppService + controller | Hepsi tabandan türer |

Migration: EF modeli değiştiği için **üret ve oku**.

## 1 — Kararlı kodlar

```csharp
namespace Ptn.ApiContractChecker.Constants.{Alan}.Lookups;

// islevi: {Ad} lookup satirlarinin kararli kodlarini kod tarafina baglar.
// sistemdeki gorevi: Seed idempotansinin ve derleme zamani guvenliginin koprusudur; veri ile kod arasindaki tek sozlesme.
public static class {Ad}Codes
{
    public const string Pending = "pending";
    public const string Running = "running";
}
```

Kod **kararlıdır**: bir kez yayımlandıktan sonra değişmez. Görünen ad değişebilir,
kod değişmez — geçmiş kayıtlar ona bağlıdır.

## 2 — Entity

```csharp
namespace Ptn.ApiContractChecker.Entities.Lookups;

// islevi: <bu siniflandirmanin alan anlami>.
// sistemdeki gorevi: <kim buna FK/Code ile baglanir>.
public class {Ad} : LookupEntity
{
    // EF Core materializasyonu icin parametresiz ctor.
    protected {Ad}() { }

    // Ortak lookup alanlarini taban ctor'a devreder; concrete tip yalniz kimligini verir.
    public {Ad}(Guid id, string code, string name, string? description = null, bool isActive = true)
        : base(id, code, name, description, isActive) { }
}
```

`Code` / `Name` / `Description` / `IsActive` **tabandan gelir** — yeniden tanımlama.
Tabandaki `Code` setter'ı private'tır: yalnız ctor/EF materialization belirler,
update akışı değiştiremez.

## 3 — Seed katkıcısı

```csharp
// islevi: {Ad} lookup satirlarini kararli kodlarindan uretir.
// sistemdeki gorevi: Bos veya eksik veritabaninda sistemin calisabilir hale gelmesini saglar; iki kez calistirildiginda satir cogaltmaz.
```

- Idempotans: var olan kodu **tekrar eklemez**, yoksa ekler.
- Seed edilmemiş bir kod, kullanıcı hatası değil **sistem/yapılandırma** hatasıdır;
  kodu id'ye çözerken bulunamazsa `InvalidOperation` fırlat, `NotFound` değil.

## 4 — EF configuration

```csharp
builder.ToTable(ApiContractCheckerTableNames.{Ad}s, ApiContractCheckerDbProperties.CheckerSchema);
builder.ConfigureByConvention();
builder.Property(x => x.Code).IsRequired().HasMaxLength(LookupConsts.MaxCodeLength);
builder.HasIndex(x => x.Code).IsUnique();
```

Lookup'lar kiracıya **ait değildir** — sistem sözlüğüdür, `IMultiTenant`
uygulamazlar.

## 5–6 — Yönetim yüzeyi

DTO'lar jenerik `LookupCommonDto` / `LookupCreateDto` / `LookupUpdateDto` üzerinden
türer; AppService `LookupCrudAppService<…>`, controller `LookupControllerBase`
tabanından gelir. İzinler `ApiContractCheckerPermissions.Lookups` altındadır —
her lookup için yeni izin ağacı açma.

- `LookupCreateDto` `Code` taşır; `LookupUpdateDto` **taşımaz**. Mapperly update
  DTO'sunu `LookupUpdateModel`'a çevirir; `LookupManager.Update` modeli
  `LookupEntity.Update` metoduna teslim eder. Entity invariant alanlarına Mapperly
  doğrudan yazmaz.
- Controller `DELETE` açmaz. `POST {id}/passivate` manager üzerinden
  `LookupEntity.Passivate()` çağırır.
- Pasifleştirme yanıtı id ile yeniden okunmaz; `IPassivable` filtresi 404
  üreteceği için kaydedilmiş/yüklü entity doğrudan DTO'ya eşlenir.
- Concrete Mapperly yalnız entity → DTO, create DTO → create model ve update DTO
  → update model eşlemelerini sahiplenir. `LookupEntity` hedefli update eşlemesi
  eklenmez; private setter'lar domain davranışının dışarıdan aşılmasını engeller.

## Owned JSON'dan referans

Bulgu ve rapor gibi owned JSON modelleri lookup'a **FK ile değil kararlı `Code`
string'i ile** referans verir. Sebep: JSON gövdesi kendi kendine yeterli kalmalı,
okunması için join gerekmemeli.

## Sık hata

- Somut lookup'a `Code`/`Name` alanını yeniden tanımlamak → taban zaten veriyor.
- Lookup'a `IMultiTenant` eklemek → sistem sözlüğü kiracıya ait değildir.
- Seed'i idempotent yazmamak → ikinci açılışta satır çoğalır.
- Kodu sonradan değiştirmek → geçmiş kayıtların anlamı bozulur.
- Update DTO/modeline `Code` koymak → "kararlı" sözleşmeyi API üzerinden bozar.
- Mapperly ile update modelini doğrudan entity'ye yazmak → invariant mutasyonunu
  domain davranışının dışına çıkarır; Manager üzerinden `LookupEntity.Update`
  çağır.
- Lookup için fiziksel `DELETE` açmak → geçmiş FK/owned JSON kodlarının anlamını
  kaybettirir; yalnız pasifleştirme kullan.
- Enum'u "sadece bu sefer" kalıcılaştırmak → RULE-0008 ihlali.
