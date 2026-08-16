# AJAN GÖREVİ — KBP-628 · API Contract Checker: korelasyon kimliği ve rapor tel hizalaması

Tek görev. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

---

## 0. Kimlik

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform\checkers\api-contract
Paket   : CheckNexus.ApiContracts  (nuget.org — public paket kaynağı)
Branch  : KBP-628   (master üzerinden: git checkout master && git checkout -b KBP-628)
Commit  : #KBP-628 <type>: <past-tense English description>
```

**Bu bir NuGet paketidir.** Public sözleşme değişiyor — §7'deki sürüm ve baseline adımı
atlanamaz.

Commit politikası: derlenebilir dilimler, **en fazla 4 commit**, testler son dilimde.
Boş dosya, yer tutucu, kullanılmayan using girmez.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (aynı depoda) |
|---|---|---|
| DTO | `mapping.md` → *DTOs* | `src/*.Application.Contracts/Dtos/Conformance/*.cs` |
| Validator | `mapping.md` → *Validation* | `src/*.Application.Contracts/FluentValidation/**` |
| Sabit | `house-profile.md` → *Stable strings* | `src/*.Domain.Shared/Constants/**/*Codes.cs` |
| Mapper | `house-profile.md` → *Mapper files contain declarations only* | `src/*.Application/Mappers/Diagnosis/DiagnosisMapper.cs` |
| Manager / servis | `house-profile.md` → *Base classes* + *AppService has no private helpers* | `src/*.Domain/Managers/Diagnosis/DiagnosisManager.cs` |

**Kanonik karar:** `docs/wiki-brain/03-Decisions/ADR-0021-Checker-Korelasyon-Kimligi.md`
Dayanak: `docs/wiki-brain/90-Inbox/AUDIT-0001-Checker-Interop-Bulgulari.md` BULGU-01, BULGU-04.

---

## 2. Ne yapıyor

Test Module bir assertion/teşhis çağrısı yaptığında **hangi senaryo adımına ait olduğunu**
söyleyemiyor; checker da cevabında bunu geri vermiyor. Sonuç: köprü, cevapları yalnız
**konumla** eşleştiriyor. Bu görev iki şeyi ekler:

1. Opsiyonel `CorrelationRef` (trace + adım kimliği) → giriş DTO'larında, **echo** ile
   sonuç DTO'larında.
2. Teşhis raporunun tel formatını Database Checker ile **hizalar** (`checknexus:` adları).

**Checker bu alanı yorumlamaz, saklamaz, karara katmaz — yalnız taşır.** Salt-okunur
değişmez (ADR-0007) bozulmaz.

---

## 3. Dosya manifestosu

### `src/Ptn.ApiContractChecker.Domain.Shared/Constants/Core/`
1. `CorrelationConsts.cs` — `TraceIdLength = 32`, `MaxStepKeyLength = 128`,
   `TraceIdPattern = "^[0-9a-f]{32}$"`

### `src/Ptn.ApiContractChecker.Domain.Shared/ExceptionCodes/`
2. Mevcut validation kod sınıfına **iki kod ekle**: `CorrelationTraceIdInvalid`,
   `CorrelationStepKeyInvalid` (yeni dosya açma — mevcut sözleşme sınıfını genişlet)

### `src/Ptn.ApiContractChecker.Application.Contracts/Dtos/Correlation/`
3. `CorrelationRefDto.cs`

```csharp
namespace Ptn.ApiContractChecker.Dtos.Correlation;

// islevi: Cagiranin trace ve adim kimligini checker cagrisi boyunca tasir.
// sistemdeki gorevi: Sonucun hangi senaryo adimina ait oldugunu konumdan bagimsiz kilar.
public sealed class CorrelationRefDto
{
    public string? TraceId { get; set; }
    public string? StepKey { get; set; }
}
```

> **Alan adları, tipleri ve JSON adları Database Checker'daki ikizle birebir aynı olmak
> zorundadır** (ADR-0021 §D). Değiştirme.

### Giriş DTO'larına alan ekle (**yeni dosya açma, mevcutları genişlet**)
4. `Dtos/Conformance/ResponseConformanceDto.cs` → `public CorrelationRefDto? Correlation { get; set; }`
5. `Dtos/Conformance/RequestConformanceDto.cs` → aynı
6. `Dtos/Diagnosis/DiagnoseRequestDto.cs` → aynı

### Sonuç DTO'larına echo alanı ekle
7. `Dtos/Conformance/ConformanceResultDto.cs` → `public CorrelationRefDto? Correlation { get; set; }`
8. `Dtos/Diagnosis/DiagnosisReportDto.cs` → aynı + **§4'teki tel hizalaması**

### `src/Ptn.ApiContractChecker.Application.Contracts/FluentValidation/Correlation/`
9. `CorrelationRefDtoValidator.cs` — `TraceId` verilmişse tam 32 küçük harf hex;
   `StepKey` verilmişse 1..128, boş/whitespace değil. Sınırlar `CorrelationConsts`'tan
10. Mevcut üç giriş validator'ına `RuleFor(x => x.Correlation).SetValidator(...).When(x => x.Correlation is not null)` eklenir (**yeni dosya değil**)

### `src/Ptn.ApiContractChecker.Application/Mappers/`
11. Mevcut mapper'lara echo eşlemesi — **`[MapProperty]` yok, gövde yok**; alan adı aynı
    olduğu için Mapperly kendiliğinden eşler. Yeni mapper dosyası **açma**

### Echo'nun yazıldığı yer
12. Echo **manager**'da set edilir (servis değil): `ResponseConformanceManager` ve
    `DiagnosisManager` sonucu kurarken `Correlation`'ı girdiden **aynen** taşır.
    Servise `input.Correlation` kopyalayan satır **yazma**

---

## 4. Teşhis raporu tel hizalaması (ADR-0021 §E)

Bugün: DB checker'ın `DiagnosisReportDto`'sunda **9** `JsonPropertyName`
(`checknexus:identity`, `checknexus:location`, `checknexus:hypotheses`,
`checknexus:nextChecks` …), API checker'da **0**. Aynı kavram tel üzerinde farklı adla çıkıyor.

**Yapılacak:** API checker'ın `DiagnosisReportDto`'suna **aynı** `JsonPropertyName` adlarını
ekle. Referans: `checkers/database-comparison/src/Ptn.DatabaseChecker.Application.Contracts/Dtos/Diagnosis/DiagnosisReportDto.cs`
— adları **oradan kopyala**, uydurma.

Kapsam: `type`, `title`, `status`, `detail`, `instance`, `checknexus:identity`,
`checknexus:location`, `checknexus:hypotheses`, `checknexus:nextChecks`.

---

## 5. Yasaklar

1. `CorrelationRef` alanını **zorunlu** yapma — opsiyonel; verilmezse davranış birebir aynı.
2. Checker'ın bu alanı **saklamasını**, kararına katmasını, loglamasını sağlama.
3. Yeni tablo, yeni migration, yeni entity.
4. `[MapProperty]`, mapper'da gövdeli metot, elle property atama.
5. Servis içinde `private` iş metodu; echo'yu serviste set etme.
6. DTO'yu `Application`'a koyma — hepsi `Application.Contracts`.
7. Nested tip; dosya içinde ikinci tip.
8. `CorrelationRefDto`'nun alan adlarını DB checker'daki ikizinden **farklı** yazma.
9. Geçmiş commit arkeolojisi; ilgisiz ağaç gezintisi.
10. Ara dilimlerde build/test.

---

## 6. Testler (son dilim)

| Test | Doğruladığı |
|---|---|
| `CorrelationRefContractTests` | `CorrelationRefDto` alan kümesi + JSON adları beklenen sabit kümeyle **birebir** eşleşiyor (ADR-0021 §D ikizlik garantisi) |
| `CorrelationEchoTests` | Girişte verilen `Correlation`, `ConformanceResultDto` ve `DiagnosisReportDto`'da **aynen** dönüyor |
| `CorrelationEchoTests` | Giriş `null` ise sonuçta da `null`; davranış değişmiyor |
| `CorrelationValidationTests` | 31/33 karakter, büyük harf hex, boş `StepKey`, 129 karakter `StepKey` → validation hatası; kod `CorrelationTraceIdInvalid`/`CorrelationStepKeyInvalid` |
| `DiagnosisReportWireTests` | Serileştirilmiş `DiagnosisReportDto`'nun anahtar kümesi `checknexus:` adlarını içeriyor ve sabitle **birebir** eşleşiyor |

---

## 7. Paket sürümü ve doğrulama baseline'ı — **atlanamaz**

Public sözleşme değişti. Depodaki precedent commit: `#KBP-627 chore: raised the package
version to 0.2.0-alpha.3 and moved the validation baseline`. Aynı deseni uygula:

1. `common.props` içindeki paket sürümünü yükselt (**alpha ilerlet**).
2. PackageValidation baseline'ını taşı — opsiyonel alan eklemek tüketici için kırıcı
   değildir, ama baseline eski sürümü işaret ettiği sürece uyarı üretir.
3. Sürümü csproj'a **sabit yazma**; `common.props` değişkeninden yönet.

---

## 8. Bitiş

1. §5'in 10 maddesini kendi kodunda tek tek kontrol et.
2. Son dilimi commit et.
3. Tek sefer: `dotnet build` → `dotnet test` (çözüm dosyası bu depoda).
4. `/abp-backend-dev` mimari incelemesi + `/backend-verify` gate'i.
5. Raporda: değişen public sözleşme listesi, yeni sürüm numarası, yaptığın **her varsayım**.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`, ilk build restore etsin;
kilit hatasında `dotnet build-server shutdown` → **bir kez** dene; aynı komutu döngüde
tekrarlama; tek engelde 10 dakikadan fazla harcama.
