# AJAN GÖREVİ — KBP-105 · Yazarlık ajanı yüzeyi, MCP protokolü ve yama önerisi

Tek görev, **altı derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

Bu görev PLAN-0003'ün **Blok 3'ünü kapatır, Blok 6 ve 7'yi denetleyip kapatır** ve Blok 4'ün
son maddesini yazar: **TM-18, TM-20, TM-31, TM-24** + **TM-32..TM-40** ve **TM-41..TM-50**
denetimi. Bittiğinde PLAN-0003'ün ertelenmemiş her maddesi kaynaktadır.

**Bu görev bir model seçmez ve bir modele bağlanmaz.** MCP yüzeyi protokol tesisatıdır;
hangi sağlayıcının bağlanacağı (LLM mi, API key'li Gemini mi) **ayrı ve sonraki** karardır.
Yüzey her iki durumda da aynıdır. RULE-0005 zaten koşum anında model yasaklıyor.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-105   (KBP-104 **ve** KBP-106 üzerinden — ikisi de birleşmiş olmalı)
Motor   : PostgreSQL
Commit  : #KBP-105 <type>: <past-tense English description>
```

| Ön koşul | Durum |
|---|---|
| KBP-104 dilimleri commit edilmiş, build/test yeşil | ⚠️ **doğrula** |
| **KBP-106 dilimleri commit edilmiş** — dört checker yazarlık yüzeyi köprüde | ⚠️ **doğrula.** Bu görevin yazarlık dilimleri ACX-07 / DBX-06 / DBX-07 / ACX-08'in bağlı olduğunu varsayar |
| Köprü sözlüğü: 79 Bridge model, 53 Bridge DTO, 10 Bridge Manager, 6 Bridge servisi | ✅ KBP-88/89 |
| `ToolCatalogManager` + `PtnToolCodes` + `TestModuleBridgeErrorCodes.ToolBudgetExceeded` | ✅ KBP-88/89 — **katalog var, bütçe hattı yok** |
| `TestScenario.IsDryRun` alanı ve `test_runs.is_dry_run` | ✅ KBP-93 — **alan var, davranış yok** |
| `ScenarioPublicationGateManager` + türetilebilirlik kanıt alanları | ✅ KBP-92/100 |
| `TestRunResult.TakenBranchPath` | ✅ KBP-93 — Blok 7 TM-41'in karşılığı **zaten yerinde** |

**Dosya bütçesi ≈55.** Altı dilim, dilim başına bir commit.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| Köprü Manager'ı | `house-profile.md` → *Base classes* | `src/Ptn.TestModule.Domain/Managers/Bridge/ToolCatalogManager.cs` |
| Köprü servisi | `house-profile.md` → *An AppService has no private business helpers* | `src/Ptn.TestModule.Application/Services/Bridge/PtnBridgeAppService.cs` |
| Köprü DTO + validator | `contracts-mapping.md` | `Application.Contracts/Dtos/Bridge/Agent/*` + `FluentValidation/Bridge/*` |
| Köprü controller | `house-profile.md` → *Architectural spine* | `src/Ptn.TestModule.HttpApi/Controllers/Bridge/PtnBridgeController.cs` |
| İzin ailesi | `house-profile.md` → *Stable string ownership* | `Domain.Shared/Permissions/TestModulePermissions.Bridge.cs` |
| Tool kodu / hata kodu | aynı bölüm | `Domain.Shared/Constants/Bridge/PtnToolCodes.cs` · `ExceptionCodes/Bridge/TestModuleBridgeErrorCodes.cs` |
| Yayın kapısı | `house-profile.md` → *Base classes* | `src/Ptn.TestModule.Domain/Managers/Catalog/ScenarioPublicationGateManager.cs` |

**Kanonik kararlar:** **ADR-0008** (MCP yüzey yerleşimi: composition host, ≤12 tool, dört
kademeli izin, koşumda model yok), **RULE-0005** (ajan hakem değildir), **RULE-0006**
(türetilemeyen assertion yayınlanamaz), **RULE-0007** (ajan tahmin etmez, tool bütçesi),
ADR-0014 (yazarlık modeli, §A iş bilgisi Git'te + MCP `Resource`), ADR-0018/0019/0020
(köprü sözlüğü, profil paketi, malzeme mührü).

---

## 2. Sabitlenen kararlar — tartışmaya açık değil

### 2.1 MCP yüzeyi **composition host'tadır**, pakette değil

ADR-0008 bunu net yazıyor: paket MCP tipi veya endpoint **taşımaz**. Yüzey
`host/Ptn.TestModule.HttpApi.Host` altında doğar; Application.Contracts sınırındaki mevcut
AppService'leri çağırır. Checker paketlerine MCP tipi **sızdırılmaz**.

**≤12 tool.** Katalog `PtnToolCodes` sahipliğindedir ve bugün oradaki kodlar sayılır; toplam
12'yi aşıyorsa **yeni tool açılmaz**, birleştirilir. Kademe 4 tool'u katalogda **görünmez**
(PLAN-0003 TM-20).

### 2.2 Bu görev bir model çağırmaz

Yazılan hiçbir kod satırı bir LLM'e istek atmaz. MCP sunucusu **tool sunar**; tool'ları hangi
istemcinin çağırdığı yüzeyin dışındadır. Koşum hattına model sızması RULE-0005 ihlalidir ve
KBP-95'in reflection testi bunu zaten yakalıyor — **o test bozulmaz**.

Sağlayıcı seçimi (LLM / API key'li Gemini) bu görevden **sonra** yapılır ve yüzeyi değiştirmez.

### 2.3 TM-31 durum sözlüğü protokole **kayıpsız** oturur

| İç durum | MCP Task durumu |
|---|---|
| `Pending` | `working` |
| `Running` | `working` |
| onay bekliyor | **`input_required`** |
| `Passed` / `Failed` / `Broken` | `completed` |
| altyapı hatası | `failed` |
| iptal | `cancelled` |

`taskId` + `ttlMs` + `pollIntervalMs` taşınır. Onay akışı (`Draft → PendingApproval →
Published`, KBP-92) `input_required` ile protokole açılır — **yeni onay modeli yazılmaz**.

### 2.4 TM-18 kuru koşum — düzeltme **sözleşmeye** karşıdır

`scenario.dryRun`: `is_dry_run = true`, sağlık hesabına **girmez** (KBP-104'ün view'ı bunu
zaten dışlıyor). Kırmızıysa ajana **çelişki bildirimi** döner.

RULE-0005'in sınırı: bildirim *"gözlem şuydu, sözleşme bunu diyor, çelişki burada"* der.
**Ajana ne yapacağını söylemez, hükmü değiştirmez, gözlemi düzeltmez.** Bildirim
deterministik üretilir; model çağrısı yoktur.

### 2.5 TM-20 bütçe **an bazındadır**

Altı anın her biri için izinli tool alt kümesi, `maxTurns` ve token tavanı. Gerekçe RULE-0005:
tüm tool'lar aynı anda bağlamda durursa seçim doğruluğu düşer.

Profiller **ABP `Setting`** olarak taşınır — tablo açılmaz (ADR-0016 §G kalıbı).
Bütçe aşımı mevcut `TestModuleBridgeErrorCodes.ToolBudgetExceeded` ile reddedilir; **yeni hata
kodu ailesi açılmaz**.

### 2.6 TM-24 Overlay — **gerekçesiz yama yok**

Yama önerisi bir **OpenAPI Overlay dokümanıdır**; kendi DSL'imiz değil (PLAN-0003 "Kapsam dışı").
`finding_fingerprint` **NOT NULL** — hangi bulgudan doğduğu bilinmeyen yama önerilemez.
Uygulama **kademe 4**: öneri üretilir, incelenir, **otomatik uygulanmaz**.

### 2.7 Blok 6 ve 7 — **denetim dilimi**, körlemesine yazma dilimi değil

TM-32..TM-40 (köprü/token ekonomisi) ve TM-41..TM-50 (iş senaryosu yetenekleri) KBP-88/89 ile
**büyük ölçüde geldi**. Dilim 6'nın işi önce **madde madde ölçmek**, sonra yalnız gerçekten
eksik olanı yazmaktır.

Bilinen durum: TM-41'in `taken_branch_path` karşılığı **yerinde**; TM-46'nın `kurallar.md`
MCP `Resource` yüzeyi **yok**. Kalanı ölç, tabloyu rapora koy, boşluğu kapat.

**Var olanı yeniden yazma.** Ölçüm tablosu olmadan tek dosya açma.

---

## 3. Dilimler

### Dilim 1 — TM-18 kuru koşum ve çelişki bildirimi (≈8 dosya)

`DryRunContradictionManager` (Domain) + mevcut `TestRunAppService` üzerinden bildirim;
`is_dry_run` koşusu sağlık hesabına girmiyor; bildirim deterministik.

**Commit:** `#KBP-105 feat: created the dry run contradiction report for the authoring agent`

---

### Dilim 2 — TM-20 ajan profilleri ve tool bütçesi (≈10 dosya)

An bazlı izinli tool alt kümesi, `maxTurns`, token tavanı — hepsi ABP `Setting`.
`AgentProfileManager` + `ToolBudgetManager`; kademe 4 tool'u katalogdan **gizli**.

**Commit:** `#KBP-105 feat: created the agent profiles and the per moment tool budget`

---

### Dilim 3 — TM-31 MCP Tasks eşlemesi (≈8 dosya)

`taskId` + `ttlMs` + `pollIntervalMs`; §2.3'ün durum tablosu birebir; onay `input_required`.
`Application.Contracts` sınırında; host'a tool bağlanması Dilim 4'te.

**Commit:** `#KBP-105 feat: created the mcp task status mapping for long running work`

---

### Dilim 4 — MCP yüzeyi composition host'ta (≈12 dosya)

ADR-0008 §2.1: host altında MCP sunucusu, ≤12 tool, dört kademeli izin.
Her tool mevcut bir AppService'i çağırır — **yeni iş mantığı yazılmaz**.
`PtnToolCodes` katalogla birebir; 12 sınırı **testle** korunur.

**Commit:** `#KBP-105 feat: created the mcp tool surface on the composition host`

---

### Dilim 5 — TM-24 Overlay yama önerisi (≈9 dosya)

`OverlayPatchManager`; `finding_fingerprint` **NOT NULL**; öneri üretilir, uygulanmaz.
Overlay dokümanı standarttır — kendi formatımız yazılmaz.

**Commit:** `#KBP-105 feat: created the overlay patch suggestion bound to a finding fingerprint`

---

### Dilim 6 — Blok 6 ve 7 denetimi ve boşluk kapatma (≈8 dosya)

Önce **ölçüm tablosu** (§2.7), sonra yalnız eksik olan. Bilinen eksik: TM-46'nın `kurallar.md`
MCP `Resource` yüzeyi. Kalan boşluklar ölçümden çıkar.

Ayrıca kalıcı kapı: **12 tool sınırı**, **kademe 4 gizliliği**, **koşumda sıfır model çağrısı**
reflection testleriyle korunur.

**Commit:** `#KBP-105 test: created the agent surface gates and closed the measured bridge gaps`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **Dilim 5 (TM-24, L boyut)** ayrı bir ticket'a devredilir.
**Kesilmeyecekler: Dilim 1, 2, 3, 4 ve Dilim 6'nın ölçüm tablosu.**

---

## 5. Yasaklar

1. **Bir modele istek atan tek satır yazma** (§2.2, RULE-0005).
2. Koşum hattına model sokma — KBP-95'in reflection testi **bozulmaz**.
3. Checker paketine veya modül paketlerine MCP tipi/endpoint koyma (ADR-0008).
4. 12 tool sınırını aşma; kademe 4 tool'unu katalogda gösterme (§2.1).
5. SUT'un OpenAPI'sinden otomatik tool üretme (ADR-0008, PLAN-0003 "Kapsam dışı").
6. Kendi senaryo DSL'imizi veya kendi yama formatımızı icat etme (§2.6).
7. `finding_fingerprint`'i nullable yapma (§2.6).
8. Yamayı otomatik uygulama — kademe 4 (§2.6).
9. Ajan profillerini tablo olarak açma (§2.5).
10. Yeni hata kodu ailesi açma; `ToolBudgetExceeded` mevcut (§2.5).
11. Blok 6/7'de ölçüm tablosu olmadan dosya açma (§2.7).
12. Blok 8 iş bilgisi tablolarını açma — ertelendi, yeni ADR ister.
13. Application servisine private iş metodu veya guard koyma — `ServiceShapeTests` **kapıdır**.
14. Yeni proje, yeni katman, `Infrastructure/`, `Helpers/`, `Utils/` açma.
15. Kırılan testi silme, `Skip` etme, assertion zayıflatma.
16. `KBP-95..104` dallarına commit; force-push, rebase, amend.
17. Ara dilimlerde build/test atlama.

---

## 6. Kabul kriterleri

- Kuru koşum kırmızıyken **çelişki bildirimi** dönüyor; hüküm değişmiyor; sağlık hesabına girmiyor.
- Her an için izinli tool alt kümesi, `maxTurns` ve token tavanı **ayar** olarak çözülüyor.
- Bütçe aşımı `ToolBudgetExceeded` ile reddediliyor.
- MCP katalogu **≤12 tool**; kademe 4 tool'u katalogda **görünmüyor**; ikisi de **testle** korunuyor.
- Uzun koşu `working → completed/failed/cancelled` akıyor; onay `input_required` veriyor;
  `taskId`/`ttlMs`/`pollIntervalMs` taşınıyor.
- Yama önerisi Overlay dokümanı; `finding_fingerprint` **NOT NULL**; otomatik uygulanmıyor.
- Blok 6 ve 7 için **madde madde ölçüm tablosu** raporda; her eksik ya kapatılmış ya
  gerekçesiyle ertelenmiş.
- **Koşum hattında sıfır model çağrısı** — reflection testi hâlâ yeşil.
- `dotnet build Ptn.TestModule.slnx -m:1` → 0 hata.
- `dotnet test --filter "Category!=LiveInfrastructure"` → 0 başarısız.
- `dotnet test --filter "Category=LiveInfrastructure"` → KBP-103'ün kanıtı **hâlâ yeşil**.
- Migration: **şema değişikliği gerekmiyorsa üretilmez.** Üretilirse tam okunur.

---

## 7. Bitiş

1. §5'in 17 maddesini kendi kodunda tek tek kontrol et.
2. Altı dilimi sırayla commit et.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → iki filtreli `dotnet test`.
4. `/backend-verify` gate'i; her commit öncesi başlık doğrulayıcısını koştur.
5. Raporda **zorunlu**: 12 tool'un tam listesi ve kademesi; Blok 6/7 ölçüm tablosu
   (madde → durum → kanıt dosyası); model çağrısı olmadığının kanıtı; her varsayım.
6. **PLAN-0003'e durum kolonu ekle.** Wiki'nin en büyük yapısal eksiği bu: 57 TM maddesinin
   hiçbirinde ✅/❌ yok. Bu görev planı kapattığına göre defteri de kapatır.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → bir kez.

---

## 8. Kapattığı borç

| Kayıt | Madde |
|---|---|
| PLAN-0003 Blok 3 | **TM-18**, **TM-20**, **TM-31** *(TM-17, TM-19 zaten kapalı)* |
| PLAN-0003 Blok 4 | **TM-24** |
| PLAN-0003 Blok 6 | **TM-32..TM-40** denetimi ve kapanışı |
| PLAN-0003 Blok 7 | **TM-41..TM-50** denetimi ve kapanışı |
| Roadmap | *"Yapay zekâ tarafını devral: MCP sunucusu, 12 tool, ajan profilleri"* |
| Wiki | PLAN-0003'e durum kolonu |

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| **Model / sağlayıcı seçimi (LLM mi, API key'li Gemini mi)** | **Bu üç task bittikten sonraki karar.** Yüzey her iki durumda da aynı |
| Ürün içi sohbet arayüzü | UI iş kolu |
| TM-22b · TM-23 | **Ertelendi** — ölçülmüş ihtiyaç yok (PLAN-0003) |
| Blok 8 `TM-51..TM-59` | **Ertelendi** — ADR-0014 §A, yeni ADR ister |
| UI | Ayrı iş kolu |
