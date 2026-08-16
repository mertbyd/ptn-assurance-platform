# AJAN GÖREVİ — KBP-630 · Snapshot operasyon envanteri ve alpha.7 yayını

> [!SUCCESS] Tamamlandı — 2026-08-16
> Dört KBP-630 commit'i (`a3fcf87`, `d76ae6b`, `1565ef1`, `30aa9ea`) checker yüzeyini,
> testlerini ve `alpha.7` paketlemesini tamamladı. **322/322** checker testi geçti; sekiz
> `.nupkg` + sekiz `.snupkg` içerik denetiminden geçti. Kullanıcının daha sonra verdiği açık
> yayın onayıyla paket ailesi NuGet.org'a push edildi ve `alpha.7` **8/8 PackageId** için
> doğrulandı. Test Module pini `60d3f5d` ile güncellendi; Release build 0 hata ve
> **316/316** test sonucu alındı.

Tek görev, **beş dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

Bu görev KBP-111 Dilim 2'nin checker ayağıdır. Test Module'ün `ptn_ground`'u doğal dilden
operasyon seçemiyor çünkü bir snapshot'ın **operasyon envanterini** veren yüzey yok. Aynı
eksik, kapsam raporunun paydasını da bilinmez bırakıyor. Bu görev o yüzeyi açar, paketi
`0.2.0-alpha.7` olarak hazırlar ve tüketici pinini günceller.

İlk uygulama yetkisi paketleme ve incelemeyle sınırlıydı. Yayın, kod dilimleri bittikten sonra
kullanıcının ayrı ve açık onayıyla skill'in manifest tabanlı motoru üzerinden yapıldı.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform\checkers\api-contract
          (AYRI git deposu — ana depo değil)
Branch  : KBP-630   (KBP-629 üzerinden)
Ticket  : KBP-630
Sürüm   : 0.2.0-alpha.5 → 0.2.0-alpha.7
Commit  : #KBP-630 <type>: <past-tense English description>
```

| Ön koşul | Durum |
|---|---|
| Checker deposu ayrı `.git`, branch `KBP-629`, çalışma ağacı **temiz** | ✅ doğrulandı |
| `KBP-629` ucu `51a42ae` | ✅ |
| `.agents/skills/acc-vertical-slice/SKILL.md` mevcut | ✅ **bu görevin yazım sırasını o belirler** |
| Kaynak sürüm `common.props` → `0.2.0-alpha.5` | ✅ depoda alpha.6 izi yok |
| **`0.2.0-alpha.6` nuget.org'a boş içerikle gitti** (kullanıcı 22:04'te HTTP 200 ile doğruladı) | ⛔ **immutable, kullanılamaz, atlanır** |
| Ana depo `KBP-111` dalında başlangıç tabanı: `415219e`, `45f76ad`, `27f4388`; test 314/314 | ✅ tarihsel başlangıç kanıtı |
| Ana depodaki pin `CheckNexusApiContractsVersion` = `0.2.0-alpha.7` | ✅ `60d3f5d` ile tamamlandı |

**Dosya bütçesi ≈18.** Beş dilim. **Migration üretilmez** — envanter, snapshot belgesinden
hesaplanır, tablo açmaz.

---

## 1. YAZMA KAPISI

**Yazım sırası `acc-vertical-slice` skill'inindir: Domain.Shared → Domain →
Application.Contracts → Application → HttpApi → test. Her katmandan sonra derle.**

| Yazacağın | Canlı kardeş |
|---|---|
| Manager hesabı | `Domain/Managers/Snapshots/SpecSnapshotAuthoringManager.cs` → `BuildOperation` (**satır 102**, `private static`, `OperationSummaryResult` döner) |
| AppService metodu | `Application/Services/Snapshots/SpecSnapshotAppService.cs` → `FindOperationAsync` (**satır 66**) |
| Sayfalama tabanı | Aynı sınıf **satır 23-24**: `SpecSnapshotAppService : EntityReadAppServiceBase<...>` — taban hazır, yeniden kurma |
| Filtreli sayfalı girdi | `Dtos/Runs/GetContractCheckRunsInput.cs` · `GetContractCheckFindingsInput.cs` |
| **Bütçeli** sayfalı sonuç | `Dtos/Runs/FindingPagedResultDto.cs` — `RequestedMaxResultCount` · `EffectiveMaxResultCount` · `IsTruncated` · `ResponseBytes` |
| Başlık/detay ayrımı | `Dtos/Runs/ContractCheckRunHeaderDto.cs` (hafif) ↔ `ContractCheckRunDetailDto.cs` (ağır) |
| Rota sabiti | `Domain.Shared/Constants/Core/ApiContractCheckerRoutes.cs` |
| Paket ve sürüm | `common.props` · `/nuget-family-release` |

Modül kuralları bağlayıcıdır: `checkers/api-contract/AGENTS.md`, `.claude/rules/`,
`.agents/skills/acc-vertical-slice/SKILL.md`. Bunlar kök `AGENTS.md`'yi **özelleştirir**.

---

## 2. Ölçülen boşluk ve sabitlenen kararlar

### 2.1 Başlangıçtaki boşluk — kapandı

`ISpecSnapshotAppService` bugün üç metot sunuyor: `FindOperationAsync` (**tek** operasyon),
`DescribeSchemaAsync`, `GetAuthoringResultAsync`. Bir snapshot'taki operasyonların
**listesini** veren yüzey yok. Bunun iki tüketicisi var:

1. `ptn_ground` — doğal dilden operasyon seçemiyor, aday listesi üretemiyor;
2. Test Module `GET api/test-module/coverage` — paydası `DenominatorState = "Unknown"`.

### 2.2 Kararlar

- **Liste satırı hafiftir.** `OperationSummaryDto` bir **detay** sözleşmesidir
  (`RequiredParameters`, `ResponseFields`, `SecurityRequirements`, `IsTruncated`, `ResultRef`).
  Liste ucu onu **döndürmez**. Yeni satır DTO'su yalnız şunları taşır: `OperationId`,
  `Method`, `Path`, request şema referansı, response şema referansı. Kalıp:
  `ContractCheckRunHeaderDto` ↔ `ContractCheckRunDetailDto`.
- **Sonuç bütçelidir.** Sayfalı sonuç `FindingPagedResultDto` kalıbını izler:
  istenen/etkin sayfa boyutu, `IsTruncated`, yanıt boyutu. Ajan yüzeyinde sınırsız liste yok.
- **Filtre kapalı kümedir.** `MethodCode` (HTTP metot kapalı listesi), `PathPrefix`
  (sınırlı uzunluk), `HasRequestBody` (bool?). **Serbest metin arama yok** — ajan tahmin
  etmez, seçenekleri checker üretir (RULE-0007).
- **Envanter belgeden hesaplanır.** Kaynak, snapshot'ın kendi belgesidir; tablo, kolon veya
  migration **açılmaz**. Hesap `SpecSnapshotAuthoringManager` içindedir; AppService yalnız
  yükler, çağırır, eşler.
- **`0.2.0-alpha.6` atlanır.** Boş yayımlandı ve NuGet sürümleri immutable'dır. Kaynak
  `alpha.5`'ten doğrudan `alpha.7`'ye çıkar.
- **`PackageValidationBaselineVersion` `0.2.0-alpha.2` olarak KALIR.** Baseline'ı alpha.6'ya
  yükseltmek doğrulamayı **boş pakete** karşı koşturur ve yayını kilitler. Dokunma.
- **Paket ailesi sekiz projedir.** `CheckNexus.ApiContracts` + `.Application` +
  `.Application.Contracts` + `.Domain` + `.Domain.Shared` + `.EntityFrameworkCore` +
  `.HttpApi` + `.HttpApi.Client`. alpha.6 kazası bu ailede yaşandı; **her `.nupkg`'in içi
  tek tek açılıp doğrulanacak** — yalnız `.nuspec` okumak yetmez.
- **Yayın ayrı onay kapısıdır.** Kod görevi push yapmaz; kullanıcı sonradan açık onay verirse
  skill'in dry-run, exact-version preflight ve güvenli key prompt kapılarından sonra yayınlanır.
- **Tüketici pini ayrı depodadır.** `ptn-test-module/common.props` güncellemesi ana depoda,
  `KBP-111` dalında, **ayrı commit** olarak yapılır ve yalnız paket erişilebilir olduğunda
  derlenir.

---

## 3. Dilimler

### Dilim 1 — Domain.Shared ve Domain (≈5 dosya)

Rota sabiti, filtre alan adları ve metot kodu kapalı kümesi `Domain.Shared`'a;
envanter modeli ve `SpecSnapshotAuthoringManager`'a liste hesabı. `BuildOperation`'ın
ürettiği ağır özet **yeniden kullanılmaz**; satır için ayrı ve hafif projeksiyon yazılır.
Katman sonunda derle.

**Commit:** `#KBP-630 feat: created the snapshot operation inventory model`

---

### Dilim 2 — Application.Contracts ve Application (≈7 dosya)

`ListOperationsAsync(Guid snapshotId, ListSnapshotOperationsInput input)` arayüze eklenir;
girdi DTO'su + FluentValidation validator'ı + satır DTO'su + bütçeli sayfalı sonuç DTO'su +
Mapperly bildirimi + `SpecSnapshotAppService` gerçeklemesi. `EntityReadAppServiceBase`
sayfalama tabanı yeniden kurulmaz. Katman sonunda derle.

**Commit:** `#KBP-630 feat: created the paged snapshot operation listing`

---

### Dilim 3 — HttpApi ve testler (≈6 dosya)

`SpecSnapshotController` üzerine `GET .../{id}/operations`; rota ve Swagger grubu sabitlerden.
Testler: sayfalama sınırı, filtre kapalı kümesi, bütçe/`IsTruncated` davranışı, bilinmeyen
snapshot reddi, ağır alanların satırda **bulunmadığı**.

**Commit:** `#KBP-630 test: created the operation inventory surface coverage`

---

### Dilim 4 — Sürüm ve paketleme (≈2 dosya)

`common.props` → `<Version>0.2.0-alpha.7</Version>`. Baseline **değişmez**.
`dotnet pack` (Release), bu dilimde **push yok**. Sekiz `.nupkg`'in **her biri** açılır ve içinde
beklenen `lib/**/*.dll` bulunduğu doğrulanır; `.snupkg`'ler üretilmiş olmalı.
alpha.6 kazasının tekrarı burada yakalanır.

Kullanıcıya teslim: paket yolları + her paketin boyutu ve içerik özeti.

**Commit:** `#KBP-630 chore: raised the package family version to 0.2.0-alpha.7`

---

### Dilim 5 — Tüketici pini · **ana depo** (≈1 dosya)

**Tamamlandı:** kullanıcı push ettikten sonra ana depo `KBP-111` dalında
`ptn-test-module/common.props` → `CheckNexusApiContractsVersion` = `0.2.0-alpha.7`.
Restore + Release build + test kapısı 0 hata ve **316/316** test ile yeşil kaldı.
Paket erişilemiyorsa **dur ve raporla** — pin yarım bırakılmaz.

**Commit:** `#KBP-111 chore: raised the api contract checker pin to 0.2.0-alpha.7`

---

## 4. Kesme bölgesi

Kesme uygulanmadı. Paket erişilebilirliği doğrulandıktan sonra Dilim 5 tamamlandı.

---

## 5. Yasaklar

1. Kod dilimlerinde `nuget push` / `dotnet nuget push` / `-Push` çalıştırma; yayın yalnız
   ayrı kullanıcı onayı ve release skill kapılarıyla yapılır.
2. `0.2.0-alpha.6`'yı yeniden kullanma, üzerine yazmaya çalışma (immutable).
3. `PackageValidationBaselineVersion`'ı değiştirme (§2.2).
4. Liste satırında `RequiredParameters`, `ResponseFields`, `SecurityRequirements` veya
   `ResultRef` döndürme — satır hafiftir.
5. Liste ucuna serbest metin arama ekleme; sayfa boyutunu sınırsız bırakma.
6. Envanter için tablo, kolon veya migration açma.
7. `EntityReadAppServiceBase`'in sayfalama akışını yeniden kurma.
8. `acc-vertical-slice`'ın katman sırasını atlama; katman sonunda derlemeden ilerleme.
9. Ana depo ile checker deposunu tek commit'te karıştırma — **iki ayrı git deposu**.
10. Test Module tarafında checker tablosunu okuma veya checker'a FK verme.
11. Yeni proje, katman, `Infrastructure/`, `Helpers/`, `Utils/` açma.
12. Rota, filtre alanı, metot kodu için inline string yazma.
13. Kırılan testi silme, `Skip` etme, assertion zayıflatma.

---

## 6. Kabul kriterleri

- `GET .../{id}/operations` sayfalı ve filtreli dönüyor; satır yalnız `OperationId`, method,
  path ve iki şema referansı taşıyor.
- Sayfa bütçesi `FindingPagedResultDto` kalıbıyla raporlanıyor; `IsTruncated` doğru.
- Bilinmeyen snapshot ve kapalı küme dışı metot kodu reddediliyor.
- Migration **üretilmedi**.
- Checker solution'ı 0 hata derleniyor; checker testleri yeşil.
- `common.props` sürümü `0.2.0-alpha.7`; baseline hâlâ `0.2.0-alpha.2`.
- Sekiz `.nupkg` üretildi, **her birinin içi açılıp** DLL varlığı doğrulandı; `.snupkg`'ler var.
- Push ayrı kullanıcı onayıyla yapıldı; `alpha.7` sekiz PackageId'nin tamamında doğrulandı.
- Ana depo pini `alpha.7`; Release build 0 hata, test **316/316**.

---

## 7. Bitiş

1. §5'in 13 maddesini kendi kodunda tek tek kontrol et.
2. Dilim 1-4'ü checker deposunda, Dilim 5'i ana depoda commit et.
3. `/backend-verify` gate'i; her commit öncesi başlık doğrulayıcısını koştur.
4. Raporda **zorunlu**: sekiz paketin adı, boyutu ve içerik doğrulaması; alpha.6 ile
   farkın kanıtı; örnek bir snapshot üzerinde envanterin **ilk sayfası**; her varsayım.

---

## 8. Kapattığı borç

| Kayıt | Madde |
|---|---|
| KBP-111 Dilim 2 | Checker ayağı — `ptn_ground`'un aday listesi buna bağlı |
| PLAN-0003 TM-61 | Kapsam raporunun paydası |
| — | alpha.6 boş yayın kazasının tekrarını önleyen içerik doğrulaması |

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| `ptn_ground`'un envanteri kullanması | **KBP-111 Dilim 3** — ana depo |
| Kapsam paydasının bağlanması | **KBP-111 Dilim 8** — ana depo |
| Paketin nuget.org'a itilmesi | **Tamamlandı** — ayrı kullanıcı onayı + release skill |
| DB Checker tarafında simetrik envanter | Ölçüldüğünde |
