# AJAN GÖREVİ — KBP-111 · Yazarlık hattının gerçek verilere bağlanması

> [!INFO] İlerleme — 2026-08-16
> Dilim 1 (`415219e`), Dilim 5 (`45f76ad`) ve Dilim 6 (`27f4388`) tamamlandı. Dilim 2'nin
> checker ayağı KBP-630 ile dört commit'te tamamlandı; `CheckNexus.ApiContracts`
> `0.2.0-alpha.7` **8/8 PackageId** olarak yayımlandı ve Test Module pini `60d3f5d` ile
> güncellendi. **Kalan uygulama:** Dilim 3, 4, 7 ve 8. Operasyon envanteri artık checker'da
> vardır; fakat `ptn_ground` ve coverage henüz bu yüzeye bağlanmadığı için tüketici borcu açıktır.

Tek görev, **sekiz derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

KBP-109 modülü *ulaşılabilir*, KBP-110 *kendi kendine döner* yaptı. Bu görev modülü
**yazarlık yapabilir** hâle getirir: bugün `ptn_ground` ve `ptn_validate` hiçbir koşulda
işe yarar cevap üretemiyor — koşulsuz `Inconclusive` dönüyorlar. Bu görev o iki aracın
arkasını gerçek checker verilerine ve gerçek yayın kapısına bağlar, eksik iki yeteneği
(operasyon envanteri, business invariant) ekler ve yazarlık oturumunu sunucuya taşır.

**Bu görev .NET'e model getirmez.** Model TypeScript ajanında yaşar; burası deterministik
derleyici ve doğrulayıcı olarak kalır (RULE-0005, `PackageBoundaryTests`).

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform   (branch predev)
Modül   : ptn-test-module + checkers/api-contract
Branch  : KBP-111   (predev üzerinden)
Motor   : PostgreSQL
Commit  : #KBP-111 <type>: <past-tense English description>
```

| Ön koşul | Durum |
|---|---|
| **KBP-110 `predev`'e merge edilmiş olmalı** | ✅ `b48c824` ile merge edildi |
| KBP-110 dalında ölçülen taban | ✅ 53 uç · build 0 hata / 3 uyarı (NU1903) · non-live **295/295** · live **2/2** · 2 migration |
| Çalışma kopyası KBP-110 dalına dönmüş olmalı | ⚠️ tur içinde `predev`'e geçmiş; işe başlamadan **branch'i doğrula** |
| `ServiceShapeTests` · `ManagerReachabilityTests` · `ServiceContractTests` · `OutwardSurfaceTests` · `PackageBoundaryTests` | ⛔ hepsi yeşil kalacak |

**Dosya bütçesi ≈75.** Sekiz dilim, dilim başına bir commit.
**Test Module'de migration üretilmez** (§2.2). Checker tarafında **paket sürümü** işi vardır.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| AppService / Controller / DTO / validator | `house-profile.md` | `Services/Runs/TestFindingAppService.cs` · `Controllers/Runs/TestFindingController.cs` (KBP-109) |
| Manager kararı | `architecture.md` | `Managers/Bridge/GroundingManager.cs` · `Managers/Catalog/ScenarioPublicationGateManager.cs` |
| Checker'a soru sorma | ADR-0015 §F, ADR-0007 | `Services/Bridge/DatabaseOracleAppService.cs` (port deseni) |
| Olay dinleme | ADR-0015 §F | `EventHandlers/Runs/ContractChangeTriggerHandler.cs` (KBP-110) |
| Checker tarafında yeni uç | `checkers/api-contract/AGENTS.md` + `.claude/rules/` + `.agents/skills/acc-vertical-slice/SKILL.md` | `Controllers/Snapshots/SpecSnapshotController.cs` |
| Paket sürümü ve yayın | `/nuget-family-release` | `common.props` · `05-Operations/NuGet-Package-Release-Playbook.md` |
| Rota / izin / kod sabiti | `house-profile.md` → *Stable string ownership* | `Domain.Shared/Constants/**` |

**Kanonik kararlar:** ADR-0007 (checker salt-okunur), ADR-0008 (MCP yüzey yerleşimi),
ADR-0014 (yazarlık modeli, iş bilgisi Git'te), ADR-0016 (4 tablo + 5 lookup),
ADR-0017 (assertion kaynakları ve belirsizlik kapısı), RULE-0005, RULE-0006, RULE-0007.
**Dayanak araştırma:** RESEARCH-0009 (§M-1..M-4 invariant desenleri), RESEARCH-0014.

---

## 2. Başlangıçta ölçülen boşluk — 2026-08-15 kaynak taraması

Bu bölüm görevin başlangıç fotoğrafıdır. Güncel ilerleme için belgenin başındaki kutu esastır;
özellikle operasyon envanteri (#3) ve business invariant (#6) artık uygulanmıştır.

### 2.1 Sekiz açık madde

| # | Ne | Bugünkü kanıt |
|---|---|---|
| **1** | **`ptn_ground` boş** | `GroundingManager.Ground()` hiçbir dala girmeden `DecisionCode = Inconclusive`, `CriticalFactCode = EvidenceUnavailable` ve tek kapalı soru döndürüyor. `PtnBridgeAppService.GroundAsync` profil + fingerprint + capability yüklüyor ama `SuggestOperationBindingsAsync` / `BuildRequestExampleAsync` **hiç çağrılmıyor**. Operasyon arama ve request örneği kodu yazılmamış |
| **2** | **`ptn_validate` boş** | `GroundingManager.Validate()` koşulsuz `IsPublishable = false` + `Inconclusive`. Tek gerçek bağlantı DB türetilebilirliği — sonucu yanıta ekleniyor ama **hükmü değiştirmiyor**. API türetilebilirliği, compile-preview, lint ve `ScenarioPublicationGateManager` devrede değil |
| **3** | **Operasyon envanteri yok** | `ISpecSnapshotAppService` yalnız `FindOperationAsync` (tek operasyon), `DescribeSchemaAsync`, `GetAuthoringResultAsync` sunuyor. Bir snapshot'ın operasyon listesini veren yüzey **yok** |
| **4** | **`GroundRequest` operasyonu baştan istiyor** | `SpecSnapshotId`, `ConnectionId`, `OperationReferenceId` — üçü de nullable değil. Doğal dilden başlanamıyor; hangi operasyonun kullanılacağı **bilinmeden** çağrı kurulamıyor |
| **5** | **İki kural kaynağı karışmış** | `host/Authoring/kurallar.md` (0.5 KB) **ajan politikası** taşıyor: "ajan hakem değildir", "türetilemeyen assertion yayımlanmaz"… Test edilecek yazılımın iş kuralları için kaynak **yok**. `TestScenario.RulesFingerprint` kolonu var ama besleyen kaynak yok |
| **6** | **Business invariant değerlendirici yok** | Kodda `BusinessInvariant` / `invariants/check` **sıfır eşleşme**. RESEARCH-0009 deseni tanımlıyor: M-1 Korunum, M-2 Delta, M-3 Tutarlılık, M-4 Tekillik. `stokSonra == stokÖnce - 1` Arazzo criteria ile ifade edilemiyor |
| **7** | **Yazarlık oturumu yok** | `McpTaskStatusManager` `input_required` durumunu **eşliyor** ama soru-cevap döngüsünü tutan bir oturum durumu yok. Kullanıcının cevabı hiçbir yere yazılmıyor; sonraki tur onu göremiyor |
| **8** | **Kapsam paydası bilinmiyor** | `GET api/test-module/coverage` payı veriyor, payda `DenominatorState = "Unknown"` + `SnapshotOperationInventoryUnavailable`. #3 kapanınca bağlanacak |

### 2.2 Sabitlenen kararlar

- **.NET'e model çağrısı girmez.** Hiçbir dilim `IChatClient`, Ollama, OpenAI veya Gemini
  istemcisi getirmez. `PackageBoundaryTests.Mcp_protocol_should_stay_in_the_host_without_a_model_client`
  kapıdır. Doğal dil → adım niyeti çıkarımı **TypeScript ajanının** işidir; bu görev o ajanın
  seçim yapacağı **kapalı listeyi** üretir.
- **Yeni MCP tool açılmaz.** `PtnToolCodes.ProtocolMax = 12` doludur (10 kayıtlı + 2 gizli
  kademe-4). Yeni yazarlık yetenekleri **mevcut** `ptn_ground` / `ptn_validate` / `ptn_knowledge`
  araçlarının arkasına bağlanır. Yeni tool gerektiğini düşünüyorsan **dur ve sor** — o karar
  `ProtocolMax`'ı ve token ekonomisini (RULE-0007) yeniden açar.
- **`GroundRequest` genişler, gevşemez.** `OperationReferenceId` **nullable** olur; boşsa
  `StepIntent` + snapshot ile **aday listesi** döner (`Inconclusive` + kapalı soru korunur,
  ama artık soru **gerçek adaylarla** doludur). Doluysa bugünkü kesin yol çalışır.
  Serbest metin operasyon adı, tablo adı veya kolon adı **taşınmaz** — aday listesi kapalı
  kümedir. Ajan tahmin etmez (RULE-0007); seçenekleri **checker** üretir.
- **Checker'a yazma yok, tablo okuma yok.** Operasyon envanteri **api-contract checker'ın
  kendi dikey dilimi** olarak yazılır; Test Module onu kendi port/AppService'i üzerinden
  çağırır (ADR-0007, ADR-0015 §F). Checker'da yazılan kod `checkers/api-contract/AGENTS.md`
  ve `acc-vertical-slice` skill'ine tabidir.
- **Checker değişikliği sürüm işidir.** Bu kapı KBP-630 ile tamamlandı:
  `CheckNexus.ApiContracts 0.2.0-alpha.7` publictir ve Test Module aynı sürüme pinlidir.
  `/nuget-family-release` gate'i yeni sürümlerde de geçerlidir.
- **Yayın kapısı tek sahiplidir.** `ptn_validate` kendi kuralını **yazmaz**; mevcut
  `ScenarioPublicationGateManager` + `ScenarioCompilationService` + `IApiOracleAppService.ValidateScenarioAssertionsAsync`
  + `IDatabaseOracleAppService.ValidateDerivabilityAsync` çağrılır ve sonuçları **birleştirilir**.
  İkinci bir kapı kuralı doğarsa **dur ve raporla**.
- **İki kural kaynağı ayrılır.** Mevcut dosya `agent-policy.md` olur (içerik aynı, MCP Resource
  olarak kalır). Yeni `kurallar.md` **test edilecek yazılımın iş kurallarıdır**, ayrı Resource
  olarak sunulur, `rules_fingerprint`'i besler. İkisi **asla** aynı Resource'ta birleşmez.
- **Oturum durumu tablo değildir.** ADR-0016'nın 4 tablo modeli korunur. Yazarlık oturumu
  ABP **dağıtık cache**'inde TTL ile yaşar (host'ta Redis kayıtlı; `McpTaskStatus`'un
  `ttlMs` sözleşmesiyle aynı ömür). Modülde `IDistributedCache<T>` kullanımı **yok** — ilk
  kullanım bu görevde gerekçelenir ve tek yerde toplanır. Kalıcı oturum geçmişi istenirse
  ADR-0016 değişikliği gerekir → **dur ve sor**.
- **LLM final belgeyi yazmaz.** Ajan turda **tek adım** önerir; birleştirme, sıralama ve
  Arazzo 1.0.1 belgesinin üretimi .NET tarafındadır (`ArazzoCompilerManager`).
  `ArazzoCompilationConsts.TargetVersion = "1.0.1"` sabittir.
- **Belirsizlikte tahmin yok.** Kapalı uçlu soru üretilir, cevap oturuma yazılır, sonraki tur
  onu okur. Cevapsız belirsizlik yayına **geçemez** (ADR-0017, RULE-0006).
- **Test Module'de migration üretilmez.** `RulesFingerprint` kolonu **zaten var**. Şema
  değişikliği gerektiğini düşünüyorsan dur ve raporla.

---

## 3. Dilimler

### Dilim 1 — Kapanış artıkları (≈8 dosya)

KBP-109/110'dan sarkan üç kalem:

- `Ptn.TestModule.HttpApi.Client` **proxy'leri yok** — projede yalnız modül dosyası var.
  53 ucun istemci yüzeyi kurulur; `TestModuleRemoteServiceConsts` grup adı tutarlı kalır.
- **Şema adlandırma tuzağı**: `test_runs."TenantId"` PascalCase, `test_runs.test_key`
  snake_case — aynı tabloda. ABP taban kolonları convention'a girmiyor. Kural checked-in
  hâle gelir (`.claude/rules/` veya `AGENTS.md`): *ham SQL yazan her iş ABP taban kolonlarını
  tırnaklı PascalCase adresler.* Ham SQL'i olan tek yer bugün `ScenarioHealthView` migration'ı.
- KBP-110'un wiki güncellemesi ürün kaynak commit'ine girmez. `docs/` artık ayrı Git
  deposudur; wiki değişikliği onun `main` dalında, kaynak kod commit'inden bağımsız izlenir.

**Commit:** `#KBP-111 feat: created the http api client proxies and the raw sql naming rule`

---

### Dilim 2 — Snapshot operasyon envanteri · **checker deposu** (≈12 dosya + sürüm)

**Durum: tamamlandı.** Checker uygulaması KBP-630 commit'leriyle, tüketici pini `60d3f5d` ile
geldi. Checker testleri 322/322; consumer testleri 316/316 geçti.

`checkers/api-contract` içinde `ListOperationsAsync(snapshotId, input)`:
sayfalı, filtreli; her satır `OperationId`, HTTP method, path, request/response şema referansı.
Modül kuralları (`AGENTS.md`, `.claude/rules/`, `acc-vertical-slice`) bağlayıcıdır.

Ardından `CheckNexus.ApiContracts` yeni prerelease sürüme çıkar ve `common.props` pini güncellenir.

**Commit:** `#KBP-111 feat: created the snapshot operation inventory surface`

---

### Dilim 3 — `ptn_ground` gerçek bağlantı (≈12 dosya)

`OperationReferenceId` nullable olur. Boşken: envanterden **aday operasyon listesi** +
`BuildRequestExampleAsync` örneği + `DescribeTableAsync` tablo bilgisi toplanır ve kapalı
soru **gerçek seçeneklerle** döner. Doluyken bugünkü kesin yol korunur.
Karar `GroundingManager`'da; I/O AppService'lerde; serbest metin taşınmaz.

**Commit:** `#KBP-111 feat: created the real grounding evidence path`

---

### Dilim 4 — `ptn_validate` gerçek yayın kapısı (≈10 dosya)

`ValidateAsync` şunları **çağırır ve birleştirir**: compile-preview (KBP-109),
`ValidateScenarioAssertionsAsync` (API), `ValidateDerivabilityAsync` (DB),
`ScenarioPublicationGateManager` kapıları, `assertion_count > 0`, malzeme mühürleri.
`IsPublishable` artık **gerçek** karardır; `Inconclusive` yalnız kanıt eksikken döner.

**Commit:** `#KBP-111 feat: created the real publication gate behind the validate tool`

---

### Dilim 5 — İki kural kaynağının ayrılması (≈8 dosya)

`agent-policy.md` (mevcut içerik) + yeni `kurallar.md` (iş kuralları) ayrı MCP Resource'lar.
İş kuralı kaynağı `rules_fingerprint` üretir ve `TestScenario.RulesFingerprint`'e bağlanır.
Kaynak yolu ayardan gelir (`ProfilePackPath` kalıbı). Kural dosyası **yorumlanmaz**, adreslenir.

**Commit:** `#KBP-111 feat: created the separate business rule resource and fingerprint binding`

---

### Dilim 6 — Business invariant değerlendirici (≈10 dosya)

`IBusinessInvariantPort` + `POST api/test-module/invariants/check` + Manager.
Desenler RESEARCH-0009'dan: `Conservation`, `Delta`, `Consistency`, `Uniqueness` — kodlar
Domain.Shared'da kapalı küme. Alan bilgisinden bağımsız, saf aritmetik/karşılaştırma.
Girdi/çıktı: `{patternCode, left, right, delta}` → `{passed, reasonCode}`.

**Commit:** `#KBP-111 feat: created the business invariant evaluator`

---

### Dilim 7 — Yazarlık oturumu ve tek-adım şeması (≈14 dosya)

Cache + TTL'li oturum: başlat, kapalı soruyu döndür, cevabı yaz, sonraki turda oku.
`POST authoring/sessions` · `POST authoring/sessions/{id}/answer` ·
`GET authoring/sessions/{id}` · `POST authoring/sessions/{id}/step`.
`step` ucu **tek** Arazzo adımı alır, doğrular ve oturumdaki belgeye **mekanik** ekler;
belge üretimi `ArazzoCompilerManager`'dadır. Ajan final YAML yazmaz.

**Commit:** `#KBP-111 feat: created the authoring session and single step merge surface`

---

### Dilim 8 — Kapsam paydası ve kapanış kapıları (≈6 dosya)

`coverage` ucu envanterden gerçek paydayı alır; `DenominatorState = "Known"` döner.
`OutwardSurfaceTests` yeni uçlarla yeşil; `PackageBoundaryTests` hâlâ modelsiz;
`ProtocolMax` değişmediğini doğrulayan test. PLAN-0003 ve `Platform-Truth.md` güncellenir
(commit edilmez — Dilim 1).

**Commit:** `#KBP-111 test: created the coverage denominator and authoring surface gates`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **Dilim 6** (invariant) devredilir — tek başına anlamlıdır ve yazarlık
döngüsünü bloke etmez.
**Kesilmeyecekler: 2, 3, 4, 7.** Bunlar olmadan TypeScript ajanı test edilemez;
Dilim 2 olmadan 3 ve 8 zaten kurulamaz.

---

## 5. Yasaklar

1. `.NET` tarafına model istemcisi getirme (`IChatClient`, Ollama, OpenAI, Gemini) — §2.2.
2. Yeni MCP tool açma; `ProtocolMax`'ı sessizce büyütme — §2.2.
3. Checker tablosunu okuma, checker'a FK verme, ortak transaction açma.
4. Checker paketini sürüm yükseltmeden yeni yüzeye bağlama.
5. `ptn_validate` içine ikinci bir yayın kapısı kuralı yazma — mevcut Manager'lar çağrılır.
6. Aday listesi yerine serbest metin operasyon/tablo/kolon adı taşıma.
7. Belirsizliği tahminle kapatma; cevapsız belirsizliği yayına geçirme (RULE-0006, RULE-0007).
8. LLM'e final Arazzo belgesi yazdırma; birleştirmeyi ajana bırakma.
9. `agent-policy.md` ile `kurallar.md`'yi tek Resource'ta birleştirme.
10. Yazarlık oturumu için tablo açma; ADR-0016'nın 4 tablo modelini bozma.
11. Test Module'de migration üretme — gerekiyorsa dur ve raporla.
12. Application servisine private iş metodu veya guard koyma (`ServiceShapeTests`).
13. `Domain/Managers/**` içine `Process`/`File`/`Directory` yazma.
14. Yeni proje, yeni katman, `Infrastructure/`, `Helpers/`, `Utils/` açma.
15. Rota, izin, hata kodu, desen kodu için inline string yazma.
16. Kırılan testi silme, `Skip` etme, assertion zayıflatma.
17. Ara dilimlerde build/test atlama; başarısız kapıdan sonra sonraki dilime geçme.

---

## 6. Kabul kriterleri

- `ptn_ground` operasyon referansı **verilmeden** çağrıldığında gerçek aday listesi ve request
  örneği taşıyan kapalı soru döndürüyor; uydurulmuş operasyon adı **yok**.
- `ptn_validate` gerçek `IsPublishable` kararı üretiyor; compile, lint, API ve DB
  türetilebilirliği ile assertion sayısı kararı etkiliyor.
- Snapshot operasyon envanteri sayfalı dönüyor; checker paketi yeni sürüme çıktı ve pin güncellendi.
- `coverage` paydası **Known**; "140 operasyonun kaçı" sorusu cevaplanıyor.
- İki kural kaynağı ayrı; iş kuralı dosyası `rules_fingerprint` üretiyor.
- Invariant değerlendirici dört deseni doğru hesaplıyor; alan bilgisi taşımıyor.
- Yazarlık oturumu soruyu soruyor, cevabı saklıyor, sonraki tur okuyor; TTL dolduğunda temizleniyor.
- Tek adım önerisi doğrulanıp belgeye ekleniyor; final belge .NET tarafında üretiliyor.
- MCP tool sayısı **değişmedi**; `PackageBoundaryTests` yeşil.
- Test Module'de migration **üretilmedi**.
- `dotnet build Ptn.TestModule.slnx -m:1` → 0 hata; iki checker solution'ı da derleniyor.
- `dotnet test --filter "Category!=LiveInfrastructure"` → 0 başarısız (taban **295**).
- `dotnet test --filter "Category=LiveInfrastructure"` → **hâlâ yeşil** (taban 2/2).

**Beklenen uç sayısı: 53 → 58** (authoring 4, invariants 1) + checker tarafında 1.

---

## 7. Bitiş

1. §5'in 17 maddesini kendi kodunda tek tek kontrol et.
2. Sekiz dilimi sırayla commit et; checker sürümünü ayrı commit'le.
3. Tek sefer: her iki checker + Test Module build → iki filtreli `dotnet test`.
4. `/backend-verify` gate'i; her commit öncesi başlık doğrulayıcısını koştur.
5. Raporda **zorunlu**: öncesi/sonrası uç sayısı; checker paket sürümü ve pin; `ptn_ground`
   ve `ptn_validate`'in gerçek bir snapshot üzerindeki **örnek çıktısı**; oturum TTL değeri;
   her varsayım.

---

## 8. Kapattığı borç

| Kayıt | Madde |
|---|---|
| `GroundingManager.Ground` | Koşulsuz `Inconclusive` — grounding hiç çalışmıyordu |
| `GroundingManager.Validate` | Koşulsuz `IsPublishable = false` — yayın kapısı bağlı değildi |
| PLAN-0003 TM-61 | Kapsam raporunun paydası |
| PLAN-0003 TM-46 / ADR-0014 §A | İş kuralı kaynağının ajana taşınması |
| RESEARCH-0009 M-1..M-4 | Business invariant değerlendirici |
| ADR-0017 | Belirsizlik kapısının oturumla kapanması |
| KBP-109 Dilim 8 | Teslim edilmemiş istemci proxy'leri |

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| Ollama/model bağlantısı, MCP client, chat arayüzü, streaming, Zod, dosya yükleme | ~~KBP-112~~ → **numarasız** (aşağıdaki nota bak) |
| Model tool-seçme F1 ölçümü ve değerlendirme harness'ı | ~~KBP-113~~ → **numarasız** (aşağıdaki nota bak) |
| TM-22b adım adres indeksi, TM-23 etki analizi | Ölçüldüğünde |
| Blok 8 iş bilgisi tabloları | Açılmayacak — Git + MCP Resource |
| Kademe-4 yama uygulaması | Açılmayacak — RULE-0005 |
| Canlı yeşil koşum kanıtı (iptal, indirme, zamanlanmış tetikleme) | İlk gerçek kullanım |

> [!WARNING] Numara sürüklenmesi düzeltmesi — 2026-08-16
> Bu tablo yazıldığında `KBP-112` TypeScript ajanına, `KBP-113` eval harness'ına ayrılmıştı.
> Sonradan **KBP-112 .NET ticket'ı olarak planlandı ve teslim edildi**
> ([[90-Inbox/TASK-KBP-112-Ortam-Ayari-Kosum-Kimligi-Kaynak-Tekligi-Ve-Ajan-Dongusu|TASK-KBP-112]]),
> `KBP-113` ve `KBP-114` numaraları **yakıldı**, `KBP-115` canlı smoke'a,
> `KBP-116` backend kapanışına
> ([[90-Inbox/TASK-KBP-116-Backend-Kapanisi-Yetki-Muhur-Ve-Paket-Erisimi|TASK-KBP-116]]) verildi.
> **Sonuç: TypeScript ajanının ve eval harness'ının numarası yoktur.** Ürün sahibi numara
> verene kadar bu iki iş numarasız anılır. AI geliştiricisinin `KBP-112/1…10` ve `KBP-113/1`
> dilim adlandırması bu yüzden geçersizdir.
