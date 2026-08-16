# AJAN GÖREVİ — KBP-629 · API Contract Checker: sınır değer, negatif vaka ve adım zincirleme adayları

Tek görev, iki yüzey. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

---

## 0. Kimlik

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform\checkers\api-contract
Paket   : CheckNexus.ApiContracts  (nuget.org)
Branch  : KBP-629   (KBP-628 üzerinden)
Commit  : #KBP-629 <type>: <past-tense English description>
```

Derlenebilir dilimler, **en fazla 5 commit**, testler son dilimde. Public sözleşme
değişiyor → §7 sürüm/baseline adımı atlanamaz.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (aynı depoda) |
|---|---|---|
| Manager | `house-profile.md` → *Base classes* + *AppService has no private helpers* | `src/*.Domain/Managers/Conformance/RequestExampleBuilder.cs` |
| Şema okuma | — | `src/*.Domain/Managers/Comparison/SpecSchemaComparisonManager.cs` (NJsonSchema kullanımı) |
| Servis + arayüz | `house-profile.md` → *Contracts live in Application.Contracts* | `Services/Conformance/IResponseConformanceAppService.cs` |
| DTO / Validator | `mapping.md` | `Dtos/Conformance/**` |
| Sabit / kod kümesi | `house-profile.md` → *Stable strings* | `Domain.Shared/Constants/Conformance/**` |

**Kanonik dayanak:** `ADR-0018 §F` (sınır ve negatif vaka **mekaniktir**),
`RESEARCH-0015 §11.2-11.3`, `RULE-0007` (ajan tahmin etmez — aday **checker'dan** gelir).

---

## 2. Neden bu iş

`RESEARCH-0015`'in yazarlık soru kataloğunda üç soru **cevapsız** kaldı ve üçü de spec
tarafında:

| Soru | Bugün |
|---|---|
| **Sınır değerler** neler? | ❌ yok |
| **Negatif vaka** nasıl kurulur? | ❌ yok |
| Adım N'in çıktısı adım N+1'e **nasıl bağlanır**? | ⚠️ kısmen |

Üçü de **LLM'e sorulmaz** — JSON Schema kısıtlarından ve OpenAPI `links`'ten **mekanik**
olarak çıkar. Ajan bunları yazmaz, **checker'dan alır** (RULE-0007 §1: açık uçlu alan yok).

Bu görev bittiğinde yazarlık soru kataloğunun spec tarafı **tamamen** kapanır.

---

## 3. Yüzey A — sınır değer ve negatif vaka üretimi

```
POST /conformance/sample-sets
```

**Girdi:** `SnapshotId`, `OperationId` (veya `Method` + `Path`), `SampleKindCode`
(`Boundary` · `Negative` · `Both`), `MaxSamplesPerField`.

**Çıktı:** alan başına üretilen örnekler; her örnek **hangi kısıttan** doğduğunu taşır.

| Alan | Anlamı |
|---|---|
| `FieldPointer` | JSON Pointer — alanın adresi |
| `ConstraintCode` | `MinLength` · `MaxLength` · `Minimum` · `Maximum` · `Pattern` · `Enum` · `Required` · `Type` · `Format` |
| `SampleKindCode` | `Boundary` \| `Negative` |
| `PositionCode` | `BelowMin` · `AtMin` · `AboveMin` · `BelowMax` · `AtMax` · `AboveMax` · `Violation` |
| `Value` | üretilen değer (**redaksiyon politikasına tabi**) |
| `ExpectedOutcomeCode` | `ShouldAccept` \| `ShouldReject` |

### Üretim kuralları — deterministik

**Sınır (`Boundary`).** `minLength: 2, maxLength: 10` → uzunluklar **1, 2, 3, 9, 10, 11**.
Aynı desen `minimum`/`maximum` için sayısal eksende uygulanır. Sınırın **altı, tam üstü ve
üstü** — üçü birden.

**Negatif (`Negative`).** Her kısıtın **sistematik ihlali**:

| Kısıt | İhlal |
|---|---|
| `required` | alanı **teker teker** çıkar (her zorunlu alan için ayrı örnek) |
| `type` | yanlış tip gönder |
| `minLength`/`maxLength` | sınırı aş |
| `minimum`/`maximum` | aralığın dışına çık |
| `enum` | listede olmayan değer |
| `pattern` | desene uymayan değer |
| `format` | biçimi bozuk değer |

**Değişmezler:**

1. **Üretim şemadan çıkar.** Rastgele değer, uydurma alan, sabit örnek listesi **yok**.
2. **Her örnek gerekçesini taşır** (`ConstraintCode` + `PositionCode`) — ajan neden
   üretildiğini görebilmeli.
3. **Şema kısıtı yoksa örnek de yok.** Kısıtsız alan için sınır değer **uydurulmaz**;
   o alan çıktıda yer almaz.
4. **Bütçe.** `MaxSamplesPerField` tavanı; kombinatoryal patlama yok.
5. NJsonSchema **zaten bağımlılıktır** (ADR-0009) — yeni şema kütüphanesi **ekleme**.

---

## 4. Yüzey B — adım zincirleme adayları

```
POST /conformance/operation-links
```

**Girdi:** `SnapshotId`, `SourceOperationId`, `MaxCandidates`.

**Çıktı:** skorlu aday listesi.

| Alan | Anlamı |
|---|---|
| `TargetOperationId` | aday sonraki operasyon |
| `SourceCode` | **`DeclaredLink`** · `SchemaMatch` · `LocationHeader` |
| `ParameterMap[]` | kaynak yanıt pointer'ı → hedef parametre adı |
| `Score` | 0..1 |
| `RequiresHumanApproval` | **her zaman `true`** |

### Üç kaynak, güven sırasıyla

1. **`DeclaredLink`** — OpenAPI `links` nesnesi. Standart bunun için var; **beyan edilmişse
   en yüksek güven**.
2. **`SchemaMatch`** — kaynak yanıt şemasındaki alan ile hedef operasyonun parametre
   adı/tipi eşleşmesi.
3. **`LocationHeader`** — `201` yanıtında `Location` başlığı varsa, işaret ettiği yolun
   operasyonu aday olur. *(Schemathesis'in runtime keşif deseni.)*

**Değişmezler:**

1. **Aday üretilir, karar verilmez.** `RequiresHumanApproval` her zaman `true`
   (ADR-0018 §F: *"aday üretilir, insan onaylar"*).
2. **Eşik altı aday listelenmez.** Skor eşiğin altındaysa aday **çıktıya girmez** —
   RULE-0007 §2: *"konuyla ilgili ama yanlış bilgi, ilgisizden daha çok zarar verir."*
   Eşik `Domain.Shared` sabitidir.
3. **Uydurma yok.** Üç kaynağın hiçbiri eşleşmiyorsa liste **boş döner**; tahmini aday
   üretilmez.

---

## 5. Dosya manifestosu (≤32)

**`Domain.Shared/Constants/Conformance/`**
1. `SampleGenerationConsts.cs` — `MaxSamplesPerField`, `DefaultMaxCandidates`, `LinkScoreThreshold`
2. `Lookups/SampleKindCodes.cs` — `Boundary` `Negative` `Both` + `All`
3. `Lookups/ConstraintCodes.cs` — dokuz kod + `All`
4. `Lookups/SamplePositionCodes.cs` — yedi kod + `All`
5. `Lookups/OperationLinkSourceCodes.cs` — `DeclaredLink` `SchemaMatch` `LocationHeader` + `All`
6. `ExceptionCodes/` — mevcut sınıfa kodlar **ekle** (yeni dosya açma)

**`Domain/Models/Conformance/`**
7. `SampleSetRequest.cs`
8. `SampleSetResult.cs`
9. `FieldSample.cs`
10. `OperationLinkRequest.cs`
11. `OperationLinkResult.cs`
12. `OperationLinkCandidate.cs`
13. `OperationLinkParameterBinding.cs`

**`Domain/Managers/Conformance/`**
14. `BoundarySampleGenerator.cs` — şema kısıtından sınır ekseni üretir
15. `NegativeSampleGenerator.cs` — kısıt ihlallerini sistematik üretir
16. `SampleSetManager.cs` — iki üreticiyi orkestre eder, bütçeyi uygular, gerekçe alanlarını doldurur
17. `OperationLinkSuggester.cs` — üç kaynağı sırayla dener, skorlar, eşiği uygular

**`Application.Contracts/`**
18–24. `Dtos/Conformance/`: `SampleSetRequestDto`, `SampleSetResultDto`, `FieldSampleDto`,
   `OperationLinkRequestDto`, `OperationLinkResultDto`, `OperationLinkCandidateDto`,
   `OperationLinkParameterBindingDto`
25. `Services/Conformance/` — mevcut `IResponseConformanceAppService`'e **iki metot ekle**
   (`BuildSampleSetAsync`, `SuggestOperationLinksAsync`) — **yeni servis açma**;
   yüzey ailesi aynı (bkz. mevcut beş metotlu arayüz)
26–27. `FluentValidation/Conformance/` iki validator
28. `Permissions/` güncelle — `Conformance.GenerateSamples`, `Conformance.SuggestLinks`

**`Application/`**
29. `Services/Conformance/ResponseConformanceAppService.cs` — iki metot **ekle**; her biri
   `validator → manager → mapper`, ≤ 6 satır
30. `Mappers/Conformance/` — mevcut mapper'a partial bildirim **ekle** (yeni mapper açma)

**`HttpApi/Controllers/Conformance/`**
31. Mevcut controller'a iki uç **ekle** (yeni controller açma)

**Örnek/dok**
32. Snapshot fixture'ına kısıtlı bir şema örneği (test verisi)

---

## 6. Yasaklar

1. Rastgele/uydurma değer üretme — her örnek **şema kısıtından** doğar.
2. Kısıtsız alan için sınır değer üretme.
3. Eşik altı link adayını çıktıya koyma.
4. `RequiresHumanApproval`'i `false` yapan yol.
5. Yeni JSON şema kütüphanesi ekleme — NJsonSchema zaten bağımlılık (ADR-0009).
6. Yeni servis/controller/mapper dosyası açma — mevcut conformance ailesine **ekle**.
7. Servis içinde `private` iş metodu; üretim mantığını serviste.
8. `[MapProperty]`; mapper'da gövde; elle property atama.
9. DTO'yu `Application`'a koyma; nested tip; yeni katman/klasör.
10. Geçmiş commit arkeolojisi; ara dilimlerde build/test.

---

## 7. Testler (son dilim)

| Test | Doğruladığı |
|---|---|
| `BoundarySampleTests` | `minLength:2, maxLength:10` → **1,2,3,9,10,11** uzunlukları; fazlası yok, eksiği yok |
| `BoundarySampleTests` | Sayısal `minimum`/`maximum` için aynı üçlü desen |
| `BoundarySampleTests` | Kısıtsız alan çıktıda **yok** |
| `NegativeSampleTests` | Her zorunlu alan için **ayrı** eksik-alan örneği |
| `NegativeSampleTests` | `enum`/`pattern`/`type`/`format` ihlalleri üretiliyor, hepsi `ShouldReject` |
| `SampleBudgetTests` | `MaxSamplesPerField` aşılmıyor |
| `OperationLinkTests` | Beyan edilmiş `links` → `DeclaredLink`, en yüksek skor |
| `OperationLinkTests` | `Location` başlıklı `201` → `LocationHeader` adayı |
| `OperationLinkTests` | Eşik altı aday **listelenmiyor**; hiçbiri eşleşmezse liste **boş** |
| `OperationLinkTests` | Her adayda `RequiresHumanApproval == true` |

---

## 8. Paket sürümü ve baseline

`common.props`'tan sürüm yükselt, PackageValidation baseline'ını taşı, csproj'a sabit sürüm
yazma. Precedent: `#KBP-627`.

---

## 9. Bitiş

1. §6'nın 10 maddesini kontrol et; son dilimi commit et.
2. Tek sefer: `dotnet build` → `dotnet test`.
3. `/abp-backend-dev` + `/backend-verify`.
4. Raporda: yeni public metotlar, yeni sürüm, yaptığın **her varsayım**.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → **bir kez** dene; döngüde tekrarlama; tek engelde
10 dakikadan fazla harcama.
