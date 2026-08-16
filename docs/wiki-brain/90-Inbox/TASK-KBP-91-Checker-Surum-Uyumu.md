# AJAN GÖREVİ — KBP-91 · Yayımlanan checker sürümlerine uyum ve yüzey devri

Tek görev. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

**Bu görev bir yükseltmedir, yeni özellik değildir.** İki checker ailesi nuget.org'da
yayımlandı; Test Module hâlâ `0.2.0-alpha.2`'ye bağlı. Bu görev bağı günceller, yayımlanan
yüzeylerin devrini yapar ve checker sınırını ihlal eden mevcut kodu kaldırır.

---

## 0. Kimlik

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-91   (KBP-90 üzerinden: git checkout KBP-90 && git checkout -b KBP-91)
Motor   : PostgreSQL
Commit  : #KBP-91 <type>: <past-tense English description>
```

Derlenebilir dilimler, **en fazla 4 commit**, testler son dilimde. Boş dosya, yer tutucu,
kullanılmayan using girmez.

> **Numaralandırma notu.** `TASK-KBP-90-94-Ajan-Prompt.md` (askıya alınmış) `KBP-91`'i
> `test_scenarios` aggregate'ine ayırmıştı. Kullanıcı 2026-08-14'te `KBP-91`'i bu yükseltmeye
> tahsis etti. Senaryo aggregate'i ve sonrası **bir numara kayar**; o belge zaten denetim
> bulgularına göre yeniden yazılacak. Bu satır silinmez.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| AppService | `house-profile.md` → *Contracts live in Application.Contracts* | `src/Ptn.TestModule.Application/Services/Bridge/DatabaseOracleAppService.cs` |
| Manager | `house-profile.md` → *Base classes* | `src/Ptn.TestModule.Domain/Managers/Bridge/DatabaseOracleManager.cs` |
| DTO / model | `mapping.md` → *DTOs* | `src/Ptn.TestModule.Application.Contracts/Dtos/Bridge/**` |
| Validator | `mapping.md` → *Validation* | `src/Ptn.TestModule.Application.Contracts/FluentValidation/Bridge/**` |
| Mapper | `house-profile.md` → *Mapper files contain declarations only* | `src/Ptn.TestModule.Application/Mappers/Bridge/DatabaseOracleMapper.cs` |
| Sabit / kod kümesi | `house-profile.md` → *Stable strings* | `src/Ptn.TestModule.Domain.Shared/Constants/Bridge/**` |

**Kanonik kararlar:** `ADR-0015 §F` (modül sınırı — bu görevin anayasası), `ADR-0019 §E/§F`
(ayak izi seviyeleri, projeksiyon ön koşulu), `ADR-0021` (korelasyon kimliği),
`ADR-0018 §E` (sözlük drift kapısı), `RULE-0006` (türetilebilirlik).

**Yasak kaynak:** `.claude/rules/verify-patterns.json` — Bridge kodu için seçilmiş profil
`EntityFrameworkCore/Adapters|Documents|Mappers` altına yazmayı reddeder. Port/adapter
katmanı bu modülde **yoktur**; entegrasyon `Application` servisleri + `Domain` manager'ları
üzerinden yürür.

---

## 2. Yayımlanan gerçek (2026-08-14, nuget.org)

| Aile | Yayımlı sürüm | Modülün bağlı olduğu |
|---|---|---|
| `CheckNexus.ApiContracts*` (8 paket) | **`0.2.0-alpha.5`** | `0.2.0-alpha.2` |
| `CheckNexus.DatabaseComparison*` (8 paket) | **`0.2.0-alpha.6`** | `0.2.0-alpha.2` |

**İki aile artık farklı sürümde.** `common.props` bunu bugün ifade edemiyor:

```xml
<CheckNexusVersion>0.2.0-alpha.2</CheckNexusVersion>   <!-- tek değişken, iki aile -->
```

Bu değişkeni kullanan altı `PackageReference` var: `Domain` (2), `Application` (3),
`HttpApi.Host` (2), `Domain.Tests` (2).

---

## 3. Ne değişti — kod seviyesinde doğrulandı

### 3.1 Database Checker `alpha.6` ile gelen yeni public yüzeyler

| Yüzey | Üyeler |
|---|---|
| `IWriteSetCapabilityAppService` | `ProbeAsync`, `CaptureAsync`, `ReleaseAsync` |
| `IProjectionAppService` | `ProjectRowsAsync` |
| `IAssertionDerivabilityAppService` | `ValidateDerivabilityAsync` |

Yeni DTO'lar: `CapabilityLevelDto`, `CapabilityProbeRequestDto`, `WriteSetCaptureRequestDto`,
`WriteSetResultDto`, `WriteSetTableDeltaDto`, `ProjectionRequestDto`, `ProjectionResultDto`,
`ProjectionRowDto`, `DerivabilityRequestDto`, `DerivabilityResultDto`, `DerivabilityItemDto`,
`DerivabilityAddressDto`, `CorrelationRefDto`.

Yeni kod kümeleri: `FootprintStrengthCodes`, `CapabilityReasonCodes`,
`ProjectionOutcomeCodes`, `AssertionDerivabilityCodes` (**DB tarafı**), `WriteSetConsts`,
`ProjectionConsts`, `CorrelationConsts`. `DifferenceSeverityCodes`'a `Ranked` eklendi.

### 3.2 API Contract Checker `alpha.5` ile gelen yeni public yüzeyler

`IResponseConformanceAppService`'e iki üye eklendi (sample-set ve operation-link).
Yeni DTO'lar: `SampleSetRequestDto`/`SampleSetResultDto`/`FieldSampleDto`,
`OperationLinkRequestDto`/`OperationLinkResultDto`/`OperationLinkCandidateDto`/
`OperationLinkParameterBindingDto`, `CorrelationRefDto`.
Yeni kod kümeleri: `SampleKindCodes`, `SamplePositionCodes`, `SampleExpectedOutcomeCodes`,
`OperationLinkSourceCodes`, `ConstraintCodes`, `SampleGenerationConsts`, `CorrelationConsts`.

### 3.3 İki tarafta ortak

- Giriş DTO'larına opsiyonel `Correlation` alanı; sonuç DTO'ları **aynen geri yansıtıyor**
  (ADR-0021). `RowAssertionRequestDto`/`RowAssertionResultDto`, `ResponseConformanceDto`,
  `RequestConformanceDto`, `DiagnoseRequestDto`, iki `DiagnosisReportDto`.
- İki `*RunStatusChangedEto` bulgu özeti taşıyor (`NewFindingCount`, `MaxSeverityCode`).
- API `DiagnosisReportDto` artık `checknexus:` `JsonPropertyName`'lerini taşıyor
  (AUDIT-0001 BULGU-04 kapandı).

**Eklemelerin tamamı additive ve opsiyonel.** Modül bu arayüzleri *implement etmiyor*,
*tüketiyor*; derleme kırığı beklenmiyor. Beklenti testle doğrulanır, varsayılmaz.

---

## 4. Kaldırılacak ihlal — bu görevin asıl işi

`src/Ptn.TestModule.Application/Services/Bridge/WriteSetCapabilityService.cs` bugün:

- `Npgsql` ile **müşterinin hedef veritabanına kendi bağlantısını açıyor**;
- `IDatabaseConnectionRepository` (Database Checker'ın **repository**'si) okuyor;
- `DatabaseConnectionInfoFactory` ve `Models.Comparison` (checker **Domain** katmanı) alıyor;
- `PtnWriteSetSql` altında ham PostgreSQL replication-slot SQL'i tutuyor
  (`pg_create_logical_replication_slot`, `pg_logical_slot_get_changes`, `pg_drop_replication_slot`).

**ADR-0015 §F:** *"Checker tablosu okuma / FK / ortak transaction — **Yasak**."*
**ADR-0007:** hedefe bağlanma, emniyet profili, `READ ONLY` transaction ve değer redaksiyonu
Database Checker'ın işidir.

KBP-713 bu yeteneği checker'da **yayımladı**. Modülün kopyası artık ikinci bir sahip ve
ikinci bir güvenlik yüzeyi. Kaldırılır.

Doğrulanan kapsam: checker `Domain` namespace'lerini kullanan **tek dosya** bu servistir.

---

## 5. Yapılacaklar

### 5.1 Sürüm değişkenini böl

`common.props` içindeki `CheckNexusVersion` **iki değişkene** ayrılır:

```xml
<CheckNexusApiContractsVersion>0.2.0-alpha.5</CheckNexusApiContractsVersion>
<CheckNexusDatabaseComparisonVersion>0.2.0-alpha.6</CheckNexusDatabaseComparisonVersion>
```

Altı `PackageReference` ailesine göre doğru değişkene bağlanır. Eski değişken **kalmaz**;
csproj'a sabit sürüm yazılmaz (`ptn-test-module/AGENTS.md`).

### 5.2 Yazma kümesi yeteneğini checker'a devret

| Silinecek | Yerine |
|---|---|
| `Services/Bridge/WriteSetCapabilityService.cs` | `IWriteSetCapabilityAppService` çağrısı |
| `Constants/Bridge/PtnWriteSetSql.cs` | — (SQL checker'da) |
| `Ptn.TestModule.Application` → `Npgsql` PackageReference | — |
| `Ptn.TestModule.Application` → `CheckNexus.DatabaseComparison.Domain` PackageReference | `.Application.Contracts` yeterli |

Korunanlar: `IWriteSetCapabilityService` sözleşmesi, `PtnCapabilityLevel`,
`PtnFootprintResult`, `PtnCapabilityLevelDto`, `PtnFootprintResultDto`,
`PtnFootprintStrengthCodes`, `FootprintCapabilityManager`.

`FootprintCapabilityManager` **yetenek yoklamasını yapmaz** — checker'ın döndürdüğü
`CapabilityLevelDto`'yu köprü sözlüğüne çevirir, bütçeyi ve `IsAdvisoryOnly` sınırını uygular.
Servis düz orkestrasyon olur: çağır → manager → map.

`PtnFootprintStrengthCodes` ile checker'ın `FootprintStrengthCodes` değerleri **birebir
aynıdır** (`Exact` / `RowAddressed` / `Inferred` / `Unavailable`); eşleme 1:1'dir,
`[MapProperty]` gerekmez.

### 5.3 Projeksiyon düğümünü bağla

`DatabaseOracleManager.CreateUnavailableProjection()` bugün sabit `Unavailable` dönüyor —
ADR-0019 §F'nin *"yüzey gelene kadar"* yer tutucusu. Yüzey geldi.

`IProjectionAppService.ProjectRowsAsync` bağlanır. `Unavailable` yolu **silinmez**: checker
`ProjectionOutcomeCodes` ile okunamama bildirdiğinde ve başka motorda aynı sonuç üretilir
(ADR-0019 §C — *"kanıt toplanamadı"*, *"yetki yok"* değil).

### 5.4 DB türetilebilirlik kapısını bağla

`IAssertionDerivabilityAppService.ValidateDerivabilityAsync` köprünün doğrulama yoluna
eklenir. RULE-0006 bugün yalnız API tarafını kapsıyor (AUDIT-0001 BULGU-03); `x-checknexus-db`
assertion'ları kapısız. Bu bağlama o boşluğu kapatır.

### 5.5 Korelasyonu taşı

Modülde bugün `TraceId`/`StepKey`/`Correlation` geçen **tek dosya yok**. Checker'a giden her
çağrı `CorrelationRef` taşır ve sonuçtaki echo doğrulanır. Batch'te sonuç sayısı istek
sayısına eşit değilse **tamamı `Unavailable`** işaretlenir (ADR-0021 §C).

### 5.6 Sözlük drift kapısını genişlet

`test/Ptn.TestModule.Domain.Tests/Bridge/VocabularyDriftTests.cs` bugün yedi kod kümesini
pinliyor. Yeni yayımlanan kümeler kapsam dışında — yani ADR-0018 §E'nin *"sessiz drift
imkânsız"* garantisi bu kümeler için **çalışmıyor**.

Eklenecek pinler: `FootprintStrengthCodes`, `CapabilityReasonCodes`, `ProjectionOutcomeCodes`,
DB `AssertionDerivabilityCodes`, `OperationLinkSourceCodes`, `SampleKindCodes`,
`SamplePositionCodes`, `SampleExpectedOutcomeCodes`, `ConstraintCodes`.

---

## 6. Yasaklar

1. Checker `Domain` / `EntityFrameworkCore` katmanına referans **verme** — yalnız
   `Application.Contracts` ve `Domain.Shared`.
2. Checker repository'si okuma, checker tablosuna FK, ortak transaction.
3. Modülde ham SQL, ham `Npgsql` bağlantısı, replication slot yönetimi.
4. `EntityFrameworkCore/Adapters|Documents|Mappers` altına dosya.
5. Yeni tablo, yeni migration, yeni entity (bu görevde entity yok).
6. Model/LLM çağrısı.
7. Ayak izini onaysız assertion üretiminde kullanma — her seviye `IsAdvisoryOnly`.
8. csproj'a sabit sürüm yazma.
9. Serbest metin alan; operasyon/tablo/kolon/kod alanları kapalı küme kalır (RULE-0007).
10. Ara dilimlerde build/test; geçmiş commit arkeolojisi.

---

## 7. Testler (son dilim)

| Test | Doğruladığı |
|---|---|
| `VocabularyDriftTests` (genişletilmiş) | Dokuz yeni kod kümesi sabit kümeyle birebir |
| `WriteSetCapabilityTests` | Yetenek sonucu checker'dan geliyor; modülde bağlantı açılmıyor |
| `WriteSetCapabilityTests` | `wal_level` uygun değilken `Inferred`/`Unavailable`; **hata fırlatmıyor** |
| `ProjectionBindingTests` | Checker `Unavailable` dönünce zincir `Inconclusive`; *"yetki yok"* denmiyor |
| `DerivabilityGateTests` | Türetilemeyen DB assertion'ı yayın kapısını düşürüyor |
| `CorrelationEchoTests` | Gönderilen `{TraceId, StepKey}` sonuçta aynen dönüyor |
| `CorrelationEchoTests` | Batch sonuç sayısı ≠ istek sayısı → tamamı `Unavailable` |
| `PackageBoundaryTests` | `Ptn.TestModule.Application` grafiğinde `Npgsql` ve checker `Domain` **yok** |

---

## 8. Kabul kriterleri

- İki aile kendi sürüm değişkeninden çözülüyor; `alpha.5` / `alpha.6` restore ediliyor.
- `WriteSetCapabilityService` ve `PtnWriteSetSql` **silinmiş**; yetenek checker'dan geliyor.
- `Ptn.TestModule.Application` artık `Npgsql` ve `CheckNexus.DatabaseComparison.Domain`
  referansı **taşımıyor**.
- Projeksiyon ve DB türetilebilirlik yüzeyleri bağlı; `Unavailable` yolu korunuyor.
- Checker'a giden her çağrı `CorrelationRef` taşıyor ve echo doğrulanıyor.
- Drift testi dokuz yeni kümeyi pinliyor.
- Migration üretilmedi.
- `dotnet build` 0 hata; tüm testler yeşil.

---

## 9. Bitiş

1. §6'nın 10 maddesini kendi kodunda tek tek kontrol et.
2. Son dilimi commit et.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → `dotnet test Ptn.TestModule.slnx --no-restore`
4. `/abp-backend-dev` mimari incelemesi + `/backend-verify` gate'i.
5. Raporda: dosya listesi, silinen dosyalar, yaptığın **her varsayım**.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`, ilk build restore etsin;
kilit hatasında `dotnet build-server shutdown` → **bir kez** dene; aynı komutu döngüde
tekrarlama; tek engelde 10 dakikadan fazla harcama.

---

## 10. Bu görevin kapattığı wiki borcu

| Kayıt | Madde | Nasıl kapanıyor |
|---|---|---|
| AUDIT-0003 #01 | Korelasyon kimliği yok | §5.5 |
| AUDIT-0003 #03 | DB türetilebilirlik kapısı yok | §5.4 |
| AUDIT-0003 #06 | Projeksiyon yüzeyi yok | §5.3 |
| AUDIT-0003 sapma notu | *"KBP-628/711 sonrası Test Module derlemesi bir kez kontrol edilmeli"* | §7 build/test |
| ADR-0019 §E | Ayak izi sahipliği | §5.2 — sahip checker |
| ADR-0015 §F | Checker repository okuma yasağı | §5.2 — ihlal kaldırıldı |

**Kapanmayan, bilinçli:** ADR-0020 malzeme mührü (AUDIT #12) senaryo aggregate'ine aittir;
AUDIT #05 (ortam eşleşmesi) koşum görevine; AUDIT #11 (host csproj sabit Serilog sürümü)
ve AUDIT #13 (`SchemaName` yasağının kapsamı) ayrı kararlardır.
