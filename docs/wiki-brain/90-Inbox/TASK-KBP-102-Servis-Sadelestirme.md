# AJAN GÖREVİ — KBP-102 · Servis sadeleştirme: her private üye ve her validasyon Manager'a

Tek görev, **beş derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

Bu görev yeni yetenek eklemez. **19 Application servisindeki 100 private üyeyi ve 13 validasyon
noktasını** doğru sahibine taşır. Bitişinde hiçbir Application servisinde private iş metodu ve
hiçbir validasyon kalmaz.

**Kullanıcı talimatı (2026-08-15, aktif konuşma):** *"private metodları, validasyonları,
helper'ları hepsini Manager'a çek. Tüm servislerde bir tane validasyon görmeyeceğim, hepsi
Manager'da."* Bu talimat `abp-coding-standards` §1 precedence sırasının **1. maddesidir** ve
house varsayılanlarının üstündedir.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-102   (KBP-101 üzerinden)
Motor   : PostgreSQL
Commit  : #KBP-102 <type>: <past-tense English description>
```

| Ön koşul | Durum |
|---|---|
| KBP-101 dört dilimi commit edilmiş, build/test yeşil | ⚠️ **doğrula** (son commit `fb5c93f`) |
| `ManagerReachabilityTests` + `ServiceContractTests` | ✅ KBP-101 — bu görev onları **bozmamalı** |
| Domain Manager kalıbı | ✅ `Domain/Managers/**` — 28 Manager |

**Dosya bütçesi ≈45.** Beş dilim, dilim başına bir commit. Testler son dilimde.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| Manager'a taşınan iş | `house-profile.md` → *An AppService has no private business helpers — a Manager does* | `src/Ptn.TestModule.Domain/Managers/Runs/WorkflowRunPlanner.cs` |
| Sadeleşen servis | aynı bölüm | `src/Ptn.TestModule.Application/Services/Bridge/WriteSetCapabilityAppService.cs` (3 private — en temizi) |
| Manager tabanı | `house-profile.md` → *Base classes* | `src/Ptn.TestModule.Domain/Managers/Runs/TestRunManager.cs` |
| Mapperly | `house-profile.md` → *Mapper files contain declarations only* | `src/Ptn.TestModule.Application/Mappers/Catalog/TestScenarioMapper.cs` |
| Model | `house-profile.md` → *One type, one file* | `src/Ptn.TestModule.Domain/Models/Runs/*` |

**Kanonik kaynak — bu görevin anayasası**, `house-profile.md`:

> *"An AppService method is an ordered list of calls: validate, invoke the Manager, map, return.
> It carries no private helper that normalizes, redacts, hashes, canonicalizes, sorts, classifies,
> plans an invocation, interprets a result, translates a code, holds a lookup dictionary, or fixes
> up fields after a mapper ran. ... A `private` modifier does not make misplaced business code
> acceptable; it only hides it from review."*

---

## 2. Sabitlenen kararlar — tartışmaya açık değil

### 2.1 Ölçülen envanter — bu görevin tamamı budur

2026-08-15 taraması, `src/Ptn.TestModule.Application/Services/**`:

| Servis | private üye | satır |
|---|---|---|
| `Bridge/PtnBridgeAppService` | **16** | 123 |
| `Shared/ProcessBoundaryService` | **12** | 237 |
| `Compilation/ScenarioCompilationService` | **12** | 124 |
| `Bridge/DatabaseOracleAppService` | 9 | 114 |
| `Runs/OracleDispatchService` | 9 | 106 |
| `Runs/TestRunAppService` | 8 | 184 |
| `Bridge/ApiOracleAppService` | 6 | 86 |
| `Bridge/FailureDiagnosisAppService` | 5 | 78 |
| `Bridge/SchemaKnowledgeAppService` · `Catalog/TestScenarioAppService` · `Runs/WorkflowRunnerService` | 4 | 65 / 175 / 67 |
| `Bridge/WriteSetCapabilityAppService` | 3 | 62 |
| `Compilation/RedoclyArazzoDocumentLinter` | 2 | 37 |
| `Lookups/*AppService` (5 adet) · `Runs/HarArtifactService` | 1 | 42 / 59 |
| **TOPLAM** | **100** | — |

**Validasyon/guard noktaları (13):** `ScenarioCompilationService:58` · `HarArtifactService:31,32,46,56`
· `WorkflowRunnerService:42,50,61` · `ProcessBoundaryService:27,66,146,151,170`

`PtnBridgeAppService` 123 satırda 16 private metotla **servis adı taşıyan bir Manager'dır**;
`house-profile.md`'nin *"a Manager wearing a service's name"* tarifi birebir odur.

### 2.2 Hedef — her servis için istisnasız

Bitişte her Application servisi şu üç şeyden ibarettir:

1. bağımlılık enjeksiyonu;
2. public metotlar — **sıralı çağrı listesi**;
3. hiçbir şey.

**Sıfır private metot. Sıfır private static alan/sözlük. Sıfır guard. Sıfır `throw`.
Sıfır `if` ile iş kararı. Sıfır mapper sonrası elle atama.**

Public metot gövdesinde kalabilecek tek şey: Manager çağrısı, Mapperly çağrısı, repository
çağrısı, dış sözleşme çağrısı ve framework I/O çağrısı — hepsi düz sırada.

### 2.3 `ProcessBoundaryService` — kullanıcı talimatı uygulanır, sınır raporlanır

`house-profile.md` boundary mechanic'lerin serviste kalabileceğini söyler. **Kullanıcı bunu
açıkça geçersiz kıldı** ve talimat precedence #1'dir. Uygula.

**Ama şu ayrım korunur, çünkü tersi katman yönünü kırar:** `Domain/Managers/**` içine
`Process`, `File`, `Directory`, `Path.GetTempPath` **çağrısı yazılmaz.** Domain filesystem ve
process I/O'suna bağlanamaz.

Bu yüzden taşıma **saf hesaplama** olarak yapılır:

| Bugün serviste | Nereye | Neden |
|---|---|---|
| `ArgumentNullException.ThrowIfNull(plan)` ve tüm guard'lar | **Manager** — `EnsureValid(plan)` | validasyon |
| workspace yolu üretimi (temp kök + ad + guid) | **Manager** — saf `string` hesabı | karar |
| oluşturulacak üst klasör listesi | **Manager** — saf liste hesabı | karar |
| argüman token çözümü + env sözlüğü kurulumu | **Manager** — saf `ProcessStartDescriptor` modeli | plan yorumu |
| çıkış/timeout hata kodu seçimi | **Manager** | sınıflandırma |
| `Directory.CreateDirectory` · `File.WriteAllTextAsync` · `Process.Start` · `WaitForExitAsync` · `Kill` · `File.ReadAllTextAsync` · `Directory.Delete` | **serviste kalır** | çıplak framework çağrısı |

Kalan çağrılar **public metot gövdesine düz sırada** yazılır — private helper'a sarılmaz.
Manager yolları ve descriptor'ı hazır verdiği için gövde kısalır.

**Raporda zorunlu:** serviste kalan her framework çağrısı tek tek listelenir. Kullanıcı listeyi
görüp ikinci bir tur isteyebilir.

### 2.4 Manager'a taşınan iş nasıl yerleşir

`house-profile.md` Manager'ın doğru şeklini tarif ediyor:

> *"small, individually named private methods each own one rule or one step; they compose into
> **one public main method** that reads as ordered named steps; the AppService calls that main
> method and nothing else from the Manager's internals."*

Yani private metotlar **yok olmaz, yer değiştirir.** Manager'da private kalırlar ve **tek public
ana metotta** birleşirler. Servis o tek metodu çağırır.

**Yasak:** taşınan her private metodu Manager'da `public` yapmak. Servisin Manager'ın iç
adımlarını tek tek çağırması, işi taşımak değil sadece kaydırmaktır.

### 2.5 Yeni Manager açma kuralı

Önce **mevcut Manager'a** taşı. Yeni Manager yalnız şu durumda açılır: taşınan iş mevcut hiçbir
Manager'ın sorumluluğuna girmiyor **ve** adı tek bir sorumluluğu anlatıyor.

`ProcessBoundaryService`'in saf hesabı için yeni bir Manager gerekir (bugün sahibi yok) —
`Domain/Managers/Shared/` altında, `ProcessBoundaryConsts` ile aynı konu ailesinde.
**Başka yeni Manager açmadan önce mevcut 28 Manager'ı tara.**

### 2.6 Lookup servislerindeki tek private

Beş `Lookups/*AppService` KBP-101'de yazıldı ve her birinde 1 private üye var. Bunlar büyük
ihtimalle Mapperly örneği veya küçük bir eşleme; **önce ne olduklarına bak.** Mapperly örneği
`private static readonly` ise bu depo kalıbıdır (`TestScenarioAppService:35`) ve **kalır** —
o bir iş helper'ı değil, mapper örneğidir. Kalıbı doğrula, körlemesine taşıma.

---

## 3. Dilimler ve dosya manifestosu

Sıra **en yoğun servisten** başlar; her dilim yeşil kapanır.

### Dilim 1 — Bridge ailesi (≈16 dosya)

`PtnBridgeAppService` (16) · `DatabaseOracleAppService` (9) · `ApiOracleAppService` (6) ·
`FailureDiagnosisAppService` (5) · `SchemaKnowledgeAppService` (4) · `WriteSetCapabilityAppService` (3)
= **43 private üye**.

Hedef Manager'lar mevcut: `Managers/Bridge/PtnBridgeManager`? — **yoksa** `GroundingManager`,
`ApiOracleManager`, `DatabaseOracleManager`, `FailureDiagnosisManager`, `SchemaKnowledgeManager`,
`FootprintCapabilityManager`, `EvidenceChainManager`, `ProfilePackManager` arasında doğru sahibi
bul. `PtnBridgeAppService`'in 16 üyesi büyük ihtimalle **birden çok** Manager'a dağılır.

**Commit:** `#KBP-102 refactor: moved the bridge service helpers and guards into their managers`

---

### Dilim 2 — Koşum ve derleme ailesi (≈14 dosya)

`OracleDispatchService` (9) · `TestRunAppService` (8) · `WorkflowRunnerService` (4) ·
`HarArtifactService` (1) · `ScenarioCompilationService` (12) · `RedoclyArazzoDocumentLinter` (2)
= **36 private üye** + 8 guard noktası.

Hedef Manager'lar: `OracleDispatchManager`, `RunOutcomeResolver`, `TestRunManager`,
`TestRunResultManager`, `TestRunExecutionManager`, `WorkflowRunPlanner`, `HarInterpreter`,
`ArazzoCompilerManager`, `ArazzoLintManager`.

`HarArtifactService`'in 4 guard'ı → blob adı üreten/doğrulayan Manager'a.

**Commit:** `#KBP-102 refactor: moved the run and compilation service helpers into their managers`

---

### Dilim 3 — Katalog ve lookup (≈8 dosya)

`TestScenarioAppService` (4) · 5 × `Lookups/*AppService` (1'er) = **9 private üye**.

§2.6'yı uygula: Mapperly örneği kalır, iş helper'ı taşınır.

**Commit:** `#KBP-102 refactor: moved the catalog and lookup service helpers into their managers`

---

### Dilim 4 — Süreç sınırı (≈6 dosya)

`ProcessBoundaryService` (12 private + 5 guard). §2.3'ün tablosunu birebir uygula.

| # | Dosya | Ne |
|---|---|---|
| 1 | `Domain/Managers/Shared/ProcessPlanManager.cs` | **yeni** — validasyon, yol hesabı, descriptor kurulumu, hata kodu seçimi |
| 2 | `Domain/Models/Shared/ProcessStartDescriptor.cs` | **yeni** — çözülmüş executable, argüman listesi, env sözlüğü, workspace kökü |
| 3 | `Domain/Models/Shared/ProcessWorkspaceLayout.cs` | **yeni** — workspace kökü + oluşturulacak klasör listesi + artefakt yolları |
| 4 | `Application/Services/Shared/ProcessBoundaryService.cs` | **sıfır private, sıfır guard**; yalnız çıplak I/O çağrıları |

`Domain/Managers/**` içine `Process`/`File`/`Directory`/`Path.GetTempPath` **yazılmaz** (§2.3).
Yol hesabı `string`/`Path.Combine` ile yapılabilir; `Path.Combine` saf hesaptır, `GetTempPath`
ortam okumasıdır — temp kökü servisten Manager'a **parametre olarak** geçer.

**Commit:** `#KBP-102 refactor: moved the process boundary planning and guards into a manager`

---

### Dilim 5 — Kalıcı kapı ve testler (≈6 test)

| # | Test | Doğruladığı |
|---|---|---|
| 5 | `ServiceShapeTests` | `Application/Services/**` altındaki **hiçbir** tipte `private` metot yok — reflection ile taranır, **regresyon kapısı** |
| 6 | `ServiceShapeTests` | Mapperly örneği `private static readonly` alanı istisna olarak tanınır (§2.6) |
| 7 | `ProcessPlanManagerTests` | Geçersiz plan reddediliyor; workspace yolu deterministik değil (her çağrı farklı guid) |
| 8 | `ProcessPlanManagerTests` | Argüman token'ı çözülüyor; env sözlüğü descriptor'a geçiyor |
| 9 | `ProcessBoundaryServiceTests` | Timeout'ta `TimeoutErrorCode`, başlatma hatasında `StartFailureErrorCode` — davranış **değişmemiş** |
| 10 | mevcut tüm testler | **Davranış korundu** — bu bir refactor'dur, yeni iş yok |

**#5 bu görevin asıl kalıcı değeridir:** bir daha servise private iş metodu **derlenmeden**
yakalanır.

**Commit:** `#KBP-102 test: created the service shape gate and process plan coverage`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **Dilim 4** (`ProcessBoundaryService`) bir sonraki göreve devredilir — en tartışmalı
ve en izole olan odur. Kesilmeyecekler: **Dilim 1, 2, 3 ve #5 testi.**

---

## 5. Yasaklar

1. **Davranış değiştirme.** Bu bir refactor'dur; hiçbir hata kodu, sıra, dönüş değeri veya
   dış çağrı değişmez.
2. Taşınan private metodu Manager'da **`public` yapma** (§2.4). Manager tek public ana metot açar.
3. `Domain/Managers/**` içine `Process`, `File`, `Directory`, `Path.GetTempPath` çağrısı yazma (§2.3).
4. Mevcut Manager'a bakmadan **yeni Manager açma** (§2.5). 28 Manager'ı önce tara.
5. Mapperly örneği alanını iş helper'ı sanıp taşıma (§2.6).
6. Mapper dosyasına gövde, `[MapProperty]`, LINQ veya kanıtlanmamış ignore koyma.
7. Yeni proje, yeni katman, `Infrastructure/`, `Helpers/`, `Utils/` açma.
8. Nested tip yazma — bir tip bir dosya.
9. Kırılan testi silme, `Skip` etme veya assertion'ı zayıflatma.
10. KBP-101'in `ManagerReachabilityTests` ve `ServiceContractTests` kapılarını bozma.
11. Migration üretme — **bu görev şema değiştirmez.**
12. `KBP-95` / `KBP-99` / `KBP-100` / `KBP-101` dallarına commit atma; force-push, rebase, amend.
13. Ara dilimlerde build/test atlama — **her dilim yeşil kapanır.**

---

## 6. Kabul kriterleri

- `Application/Services/**` altında **sıfır private metot** (Mapperly örneği alanı hariç, §2.6).
- `Application/Services/**` altında **sıfır guard, sıfır `throw`, sıfır iş kararı `if`'i**.
- Her servis public metodu: sıralı çağrı listesi, 25 satır ve iki iç içe kontrol seviyesi içinde.
- Taşınan her iş, Manager'da **private** kalıyor ve **tek public ana metotta** birleşiyor.
- `Domain/Managers/**` içinde `Process`/`File`/`Directory` çağrısı **yok**.
- `ServiceShapeTests` kapısı yeşil ve bir daha private iş metodunu **derlenmeden** yakalıyor.
- **Davranış değişmedi**: mevcut tüm testler tek satır assertion değişikliği olmadan geçiyor.
- `dotnet build Ptn.TestModule.slnx -m:1` → 0 hata; `dotnet test` → 0 başarısız.
- Migration **üretilmiyor**.

---

## 7. Bitiş

1. §5'in 13 maddesini kendi kodunda tek tek kontrol et.
2. Beş dilimi sırayla commit et.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → `dotnet test Ptn.TestModule.slnx --no-restore`
4. `/backend-verify` gate'i; her commit öncesi başlık doğrulayıcısını koştur.
5. Raporda **zorunlu**: taşınan 100 üyenin **her biri** için `servis → hedef Manager` satırı;
   `ProcessBoundaryService`'te kalan her framework çağrısının listesi (§2.3); açtığın her yeni
   Manager için gerekçe; davranışın değişmediğinin kanıtı; yaptığın **her varsayım**.

**Bilinen tarayıcı false positive'i — düzeltmeye kalkma:** `check-backend-diff.ps1`,
`Domain/Managers/Catalog/TestScenarioManager.cs` içindeki `Ensure*`/`Normalize*` metotları için
`[ENTITY]` bulgusu üretir. O dosya bir Manager'dır ve metotlar tam yerindedir. Bu görev
Manager'lara **daha çok** private metot taşıyacağı için bu sayı **artacaktır** — beklenen budur,
raporla ve refactor etme.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → bir kez; döngüde tekrar etme.

---

## 8. Kapattığı borç

| Kayıt | Madde |
|---|---|
| `house-profile.md` *An AppService has no private business helpers* | 19 servis, 100 private üye |
| Kullanıcı talimatı (2026-08-15) | *"Tüm servislerde bir tane validasyon görmeyeceğim"* |
| — | `ServiceShapeTests` ile kalıcı regresyon kapısı |

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| Yeni yetenek, yeni uç, yeni DTO | Yok — bu saf refactor |
| Checker depolarındaki servisler | Ayrı depolar, ayrı ticket |
| Vault paketleme | **KBP-98** |
| Canlı altyapı smoke | Ayrı iş |
