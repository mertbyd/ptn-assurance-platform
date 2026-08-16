# AJAN GÖREVİ — KBP-711 · Database Checker: korelasyon kimliği ve batch eşleşme garantisi

Tek görev. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

---

## 0. Kimlik

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform\checkers\database-comparison
Paket   : CheckNexus.DatabaseComparison  (nuget.org — public paket kaynağı)
Branch  : KBP-711   (master üzerinden: git checkout master && git checkout -b KBP-711)
Commit  : #KBP-711 <type>: <past-tense English description>
```

**Bu bir NuGet paketidir.** Public sözleşme değişiyor — §7'deki sürüm ve baseline adımı
atlanamaz.

Commit politikası: derlenebilir dilimler, **en fazla 4 commit**, testler son dilimde.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (aynı depoda) |
|---|---|---|
| DTO | `mapping.md` → *DTOs* | `src/*.Application.Contracts/Dtos/Assertions/*.cs` |
| Validator | `mapping.md` → *Validation* | `src/*.Application.Contracts/FluentValidation/**` |
| Sabit | `house-profile.md` → *Stable strings* | `src/*.Domain.Shared/Constants/**/*Codes.cs` |
| Manager | `house-profile.md` → *Base classes* + *AppService has no private helpers* | `src/*.Domain/Managers/Assertions/RowAssertionManager.cs` |
| Mapper | `house-profile.md` → *Mapper files contain declarations only* | `src/*.Application/Mappers/**` |

**Kanonik karar:** `docs/wiki-brain/03-Decisions/ADR-0021-Checker-Korelasyon-Kimligi.md`
Dayanak: `AUDIT-0001` BULGU-01, **BULGU-02**.

---

## 2. Ne yapıyor

İki problem var ve ikincisi **veri bozar**:

1. **Adım kimliği yok.** DB assertion'ı dış runner tarafından sıradan bir HTTP adımı olarak
   çağrılıyor ve yanıt HAR'a düşüyor. Test Module o HAR girdisini senaryo adımına yalnız
   **konumla** bağlayabiliyor.
2. **Batch indekse bağlı.** `AssertBatchAsync` istek↔sonuç bağı olarak **liste indeksi**
   kullanıyor. Sunucu bir öğe düşürür veya sırayı değiştirirse **A'nın sonucu B'ye yazılır** —
   sessizce, ve teşhis yanlış tabloyu gösterir.

Bu görev opsiyonel `CorrelationRef` ekler, **echo** eder ve batch'te **öğe seviyesinde**
eşleşme garantisi kurar.

**Checker bu alanı yorumlamaz, saklamaz, karara katmaz — yalnız taşır.**

---

## 3. Dosya manifestosu

### `src/Ptn.DatabaseChecker.Domain.Shared/Constants/`
1. `Core/CorrelationConsts.cs` — `TraceIdLength = 32`, `MaxStepKeyLength = 128`,
   `TraceIdPattern = "^[0-9a-f]{32}$"`

### `src/Ptn.DatabaseChecker.Domain.Shared/ExceptionCodes/`
2. Mevcut validation kod sınıfına **üç kod ekle**: `CorrelationTraceIdInvalid`,
   `CorrelationStepKeyInvalid`, **`BatchResultCountMismatch`** (yeni dosya açma)

### `src/Ptn.DatabaseChecker.Application.Contracts/Dtos/Correlation/`
3. `CorrelationRefDto.cs`

```csharp
namespace Ptn.DatabaseChecker.Dtos.Correlation;

// islevi: Cagiranin trace ve adim kimligini checker cagrisi boyunca tasir.
// sistemdeki gorevi: Sonucun hangi senaryo adimina ait oldugunu konumdan bagimsiz kilar.
public sealed class CorrelationRefDto
{
    public string? TraceId { get; set; }
    public string? StepKey { get; set; }
}
```

> **Alan adları, tipleri ve JSON adları API Contract Checker'daki ikizle birebir aynı olmak
> zorundadır** (ADR-0021 §D). Değiştirme.

### Giriş DTO'larına alan ekle (**yeni dosya açma, mevcutları genişlet**)
4. `Dtos/Assertions/RowAssertionRequestDto.cs` → `public CorrelationRefDto? Correlation { get; set; }`
5. `Dtos/Diagnosis/DiagnoseRequestDto.cs` → aynı

### Sonuç DTO'larına echo alanı ekle
6. `Dtos/Assertions/RowAssertionResultDto.cs` → `public CorrelationRefDto? Correlation { get; set; }`
7. `Dtos/Diagnosis/DiagnosisReportDto.cs` → aynı, **`checknexus:correlation` JSON adıyla**
   (bu DTO'daki mevcut `checknexus:` desenine uy)

### `src/Ptn.DatabaseChecker.Application.Contracts/FluentValidation/Correlation/`
8. `CorrelationRefDtoValidator.cs` — `TraceId` verilmişse tam 32 küçük harf hex;
   `StepKey` verilmişse 1..128. Sınırlar `CorrelationConsts`'tan
9. Mevcut `RowAssertionRequestDtoValidator` ve diagnosis validator'ına
   `.SetValidator(...).When(x => x.Correlation is not null)` eklenir (**yeni dosya değil**)

### Batch garantisi — **manager'da**
10. `Domain/Managers/Assertions/RowAssertionManager.cs` (veya batch sahibi manager):
    - Her sonuç, karşılık gelen isteğin `Correlation`'ını **aynen** taşır
    - **Sonuç sayısı istek sayısına eşit olmak zorundadır**; değilse
      `BatchResultCountMismatch` koduyla `BusinessException`
    - Kısmi sonuçla sessizce dönme **yok**

### Echo'nun yazıldığı yer
11. Echo **manager**'da set edilir (servis değil). Servise `input.Correlation` kopyalayan
    satır **yazma**

---

## 4. Yasaklar

1. `CorrelationRef`'i **zorunlu** yapma — opsiyonel; verilmezse davranış birebir aynı.
2. Checker'ın bu alanı **saklaması**, kararına katması, loglaması.
3. Yeni tablo, yeni migration, yeni entity.
4. `[MapProperty]`, mapper'da gövdeli metot, elle property atama.
5. Servis içinde `private` iş metodu; echo'yu serviste set etme; batch sayı kontrolünü
   serviste yapma (**manager'ın işi**).
6. DTO'yu `Application`'a koyma — hepsi `Application.Contracts`.
7. Nested tip; dosya içinde ikinci tip.
8. `CorrelationRefDto`'nun alan adlarını API checker'daki ikizinden **farklı** yazma.
9. Serbest SQL taşıyan sözleşme; salt-okunur değişmezi (ADR-0007) delme.
10. Geçmiş commit arkeolojisi; ara dilimlerde build/test.

---

## 5. Testler (son dilim)

| Test | Doğruladığı |
|---|---|
| `CorrelationRefContractTests` | `CorrelationRefDto` alan kümesi + JSON adları beklenen sabit kümeyle **birebir** eşleşiyor (ikizlik garantisi) |
| `CorrelationEchoTests` | `AssertRowAsync` / `AssertCountAsync` / `AssertAbsentAsync` girişteki `Correlation`'ı sonuçta **aynen** döndürüyor |
| `CorrelationEchoTests` | Giriş `null` ise sonuç `null`; davranış değişmiyor |
| **`BatchCorrelationTests`** | 3 istekli batch'te her sonuç **kendi** `StepKey`'ini taşıyor — indeks değil kimlik |
| **`BatchCorrelationTests`** | Sonuç sayısı istek sayısından farklı olursa `BatchResultCountMismatch` fırlıyor; kısmi sonuç dönmüyor |
| `CorrelationValidationTests` | 31/33 karakter, büyük harf hex, boş `StepKey`, 129 karakter → validation hatası, doğru kodla |
| `DiagnosisReportWireTests` | `checknexus:correlation` anahtarı serileştirilmiş çıktıda var |

---

## 6. Paket sürümü ve doğrulama baseline'ı — **atlanamaz**

Precedent commit: `#KBP-710 chore: raised the package version to 0.2.0-alpha.3 and moved the
validation baseline`. Aynı deseni uygula:

1. `common.props` içindeki paket sürümünü yükselt (alpha ilerlet).
2. PackageValidation baseline'ını taşı.
3. Sürümü csproj'a sabit yazma.

---

## 7. Bitiş

1. §4'ün 10 maddesini kendi kodunda tek tek kontrol et.
2. Son dilimi commit et.
3. Tek sefer: `dotnet build` → `dotnet test`.
4. `/abp-backend-dev` mimari incelemesi + `/backend-verify` gate'i.
5. Raporda: değişen public sözleşme listesi, yeni sürüm numarası, yaptığın **her varsayım**.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`, ilk build restore etsin;
kilit hatasında `dotnet build-server shutdown` → **bir kez** dene; aynı komutu döngüde
tekrarlama; tek engelde 10 dakikadan fazla harcama.

---

## 8. Bu görevde **olmayan** iş

`AUDIT-0001` BULGU-03 (**DB assertion'ları için türetilebilirlik kapısı yok**) bu göreve
**dâhil değildir**. O ayrı ve daha büyük bir iştir: `DescribeTableAsync`'in verdiği yapıdan
*"hedef tablo var mı · kolonlar var mı · anahtar PK/unique mi · matcher kolon tipiyle uyumlu
mu"* sorularını cevaplayan yeni bir yüzey. Ayrı task olarak açılacak.
