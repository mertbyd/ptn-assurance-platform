# AJAN GÖREVİ — KBP-90 · Şema sahipliği, lookup kataloğu ve ilk migration

Tek görev. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

**Bu görev bağımsızdır:** checker paketlerine dokunmaz, `Bridge/` klasörüne dokunmaz,
KBP-628/KBP-711 ile çakışmaz. Paralel yürütülebilir.

---

## 0. Kimlik

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-90   (KBP-89 üzerinden: git checkout KBP-89 && git checkout -b KBP-90)
Motor   : PostgreSQL
Commit  : #KBP-90 <type>: <past-tense English description>
```

Derlenebilir dilimler, **en fazla 4 commit**, testler son dilimde. Boş dosya, yer tutucu,
kullanılmayan using girmez.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| Lookup entity | `house-profile.md` → *Base classes* + *Entity data shell* | Foundation `LookupEntity<TKey>` |
| Manager | `house-profile.md` → *Base classes* | `nexum-abp-filemodule/.../Managers/Files/FileCategoryManager.cs` |
| Repository | `data-access.md` | `nexum-abp-filemodule/.../Repository/Files/EfCoreFileEntryRepository.cs` |
| EF Configuration | `data-access.md` | `checkers/api-contract/src/*.EntityFrameworkCore/Configurations/**` |
| Sabit / kod kümesi | `house-profile.md` → *Stable strings* | `checkers/api-contract/src/*.Domain.Shared/Constants/**/*Codes.cs` |
| Seed contributor | `data-access.md` | `checkers/*/src/*.Domain/Data/**` |

**Kanonik kararlar:** **`ADR-0016`** (bu görevin anayasası), `RULE-0002` (şema/migration
sahipliği). Şema kaynağı: **`docs/wiki-brain/04-Architecture/Test-Platform-Schema.dbml`** —
kolon adları, tipler, indeksler ve unique'ler **oradan alınır, uydurulmaz**.

> **Denetim notu (2026-08-14, tur 3):** DBML, ADR-0016 ile iç tutarlı bulundu — 4 ana + 5
> lookup, `IMultiTenant` dağılımı, `Cascade`/`Restrict` yönleri, unique ve indeks kümeleri
> eşleşiyor. Bu görev doğrulanmış bir model üzerine yazılıyor.

---

## 2. Ne yapıyor

Modülün üç şemasını sahiplenir, beş lookup'ı **Foundation yığınıyla** kurar, kod sözlüğünü
`Domain.Shared`'da sabitler ve ilk migration'ı üretir.

**Neden ilk:** yanlış kurulan şema sonraki her işte migration borcu yaratır (RULE-0002).

---

## 3. Foundation lookup yığını **hazır** — elle CRUD yazma

Foundation paketi şunları veriyor:

```csharp
Nexum.Abp.Foundation.Lookups.LookupEntity<TKey>        // Code, Name, Description
Nexum.Abp.Foundation.Managers.LookupManager<TEntity,TKey>
Nexum.Abp.Foundation.Application.Services.LookupAppService<
    TEntity,TKey,TDto,TCreateDto,TUpdateDto,TManager,TRepository>
Nexum.Abp.Foundation.Lookups.LookupDto<TKey>
   + LookupCreateDto · LookupUpdateDto · LookupListInput
   + LookupCreateModel · LookupUpdateModel
   + LookupCreateDtoValidator · LookupUpdateDtoValidator · LookupListInputValidator
   + LookupCreateModelMapper · LookupUpdateModelMapper
Nexum.Abp.Foundation.EntityFrameworkCore.EfCoreLookupRepository<TDbContext,TEntity,TKey>
Nexum.Abp.Foundation.Repositories.ILookupRepository<TEntity,TKey>
```

**Liste, sayfalama, CRUD gövdesi, validator, mapper yazılmaz.** Bir lookup'ın CRUD'ını elle
yazarsan iş reddedilir.

> **Dikkat — tek gerçek fark:** `LookupEntity<TKey>` `Code`/`Name`/`Description` veriyor ama
> **`IsActive` vermiyor**. DBML beş lookup'ta da `is_active boolean [not null, default: true]`
> istiyor. Bu yüzden her somut lookup entity'si `IPassivable` uygular ve `IsActive` alanını
> **kendisi** taşır. Ortak configuration bunu tek yerde bağlar.

---

## 4. Dosya manifestosu (≈30)

### `src/Ptn.TestModule.Domain.Shared/Constants/Runs/Lookups/`
> Bu kodların arkasında **gerçek lookup tablosu var**, o yüzden `Lookups/` alt klasörü
> **doğru** kullanımdır (köprüdeki durumun tersi).

1. `TestRunStatusCodes.cs` — `Pending` `Running` `Completed` `Cancelled` `Aborted` `TimedOut` + `All`
2. `TestOutcomeStatusCodes.cs` — `Passed` `Failed` `Broken` `Skipped` `Inconclusive` + `All`
3. `TestFailureCategoryCodes.cs` — `Contract` `Persistence` `Business` `Transport` `Technical` + `All`
4. `TestTriggerKindCodes.cs` — `Manual` `Scheduled` `Api` `Webhook` `ContractChange` + `All`
5. `TestScenarioStateCodes.cs` — `Draft` `PendingApproval` `Published` `Deprecated` + `All`

### `src/Ptn.TestModule.Domain.Shared/Constants/Runs/`
6. `TestModuleTableNames.cs` — dokuz tablo adı (DBML'den)
7. `TestLookupConsts.cs` — `MaxCodeLength = 64`, `MaxNameLength = 128`, `MaxDescriptionLength = 512`

### `src/Ptn.TestModule.Domain.Shared/ExceptionCodes/Runs/`
8. `TestModuleLookupErrorCodes.cs`

### `src/Ptn.TestModule.Domain/Entities/Lookups/`
> Beşi de `LookupEntity<Guid>` türer, `IPassivable` uygular, **`IMultiTenant` TAŞIMAZ**
> (global referans verisi — ADR-0016 §D).

9. `TestRunStatus.cs`
10. `TestOutcomeStatus.cs` — **ek alan `BreaksBuild`** (bool, ADR-0016 §F: politikayı koddan çıkarır)
11. `TestFailureCategory.cs`
12. `TestTriggerKind.cs`
13. `TestScenarioState.cs`

### `src/Ptn.TestModule.Domain/Interface/Lookups/`
14–18. Beş repository arayüzü — her biri `ILookupRepository<T, Guid>` türer, **başka üye eklemez**

### `src/Ptn.TestModule.Domain/Managers/Lookups/`
19–23. Beş manager — `LookupManager<T, Guid>` türer. Gövde **boş**; yalnız ctor ve gerekiyorsa
`AlreadyExistsErrorCode` override'ı

### `src/Ptn.TestModule.EntityFrameworkCore/Configurations/Lookups/`
24. `LookupEntityConfigurationBase.cs` — ortak: `code` unique + `MaxCodeLength`,
    `name`/`description` uzunlukları, `is_active` default `true`, şema `test_lookup`
25. `TestOutcomeStatusConfiguration.cs` — `breaks_build` kolonu
26. Kalan dört lookup configuration'ı (taban sınıftan türer, tablo adı verir)

### `src/Ptn.TestModule.EntityFrameworkCore/Repository/Lookups/`
27–31. Beş repository — `EfCoreLookupRepository<TestModuleDbContext, T, Guid>` türer

### `src/Ptn.TestModule.EntityFrameworkCore/EntityFrameworkCore/` (güncelle)
32. `TestModuleDbContext.cs` — beş `DbSet`
33. `TestModuleDbContextModelCreatingExtensions.cs` — şema adları **`TestModuleDbProperties`
    üzerinden**, configuration'dan ezilebilir (RULE-0002)

### `src/Ptn.TestModule.Domain/Data/`
34. `TestModuleDataSeedContributor.cs` — beş lookup'ın kod seti. **İdempotent**: iki kez
    koşunca çift satır yok. `breaks_build`: `Failed`/`Broken` → `true`, diğerleri → `false`

### Migration
35. `dotnet ef migrations add Initial_TestModuleSchema` — **yalnız üç şema**:
    `test_lookup`, `test_catalog`, `test_run`

---

## 5. Yasaklar

1. Lookup CRUD/liste/sayfalama gövdesi yazma — Foundation tabanı kullan.
2. Lookup'lara `IMultiTenant` ekleme (global veri).
3. Ana tablolarda `IMultiTenant` **unutma** (bu görevde ana tablo yok, ama DbContext'i
   hazırlarken kural aklında olsun).
4. Auth / Notification / checker tabloları için migration **üretme**.
5. Şema adını koda gömme — `TestModuleDbProperties` üzerinden.
6. Enum kullanma — lookup + `Domain.Shared` sabiti.
7. Nested tip; dosya içinde ikinci tip; `private` iş metodu serviste.
8. `[MapProperty]`; mapper'da gövde.
9. Yeni katman/klasör (`Helpers/`, `Engines/`, `Infrastructure/` …).
10. Geçmiş commit arkeolojisi; ara dilimlerde build/test.

---

## 6. Testler (son dilim)

| Test | Doğruladığı |
|---|---|
| `LookupSeedTests` | Seed **idempotent** — iki kez koşunca satır sayısı değişmiyor |
| `LookupSeedTests` | Beş lookup'ın kod kümesi `*Codes.All` ile **birebir** eşleşiyor |
| `BreaksBuildPolicyTests` | `Failed`/`Broken` → `true`, `Passed`/`Skipped`/`Inconclusive` → `false` |
| `SchemaOwnershipTests` | Üç şema adı configuration'dan ezilebiliyor |
| `LookupTenancyTests` | Lookup entity'lerinde `IMultiTenant` **yok** |
| `MigrationScopeTests` | Üretilen migration yalnız `test_lookup`/`test_catalog`/`test_run` şemalarına dokunuyor |

---

## 7. Kabul kriterleri

- Beş lookup tablosu oluşuyor; seed idempotent.
- `test_outcome_statuses.breaks_build` dolu ve politikayı taşıyor.
- Lookup CRUD'ı için **elle yazılmış tek satır gövde yok**.
- Üç şema `TestModuleDbProperties` üzerinden ezilebiliyor.
- Migration başka modülün tablosuna dokunmuyor.
- `IsActive` beş entity'de de var (`IPassivable`).

---

## 8. Bitiş

1. §5'in 10 maddesini kendi kodunda tek tek kontrol et.
2. Son dilimi commit et.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → `dotnet test Ptn.TestModule.slnx --no-restore`
4. `/abp-backend-dev` mimari incelemesi + `/backend-verify` gate'i.
5. Raporda: dosya listesi, migration adı, yaptığın **her varsayım**.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`, ilk build restore etsin;
kilit hatasında `dotnet build-server shutdown` → **bir kez** dene; aynı komutu döngüde
tekrarlama; tek engelde 10 dakikadan fazla harcama. Migration üretirken `--project` ve
`--startup-project` **açıkça** verilir.
