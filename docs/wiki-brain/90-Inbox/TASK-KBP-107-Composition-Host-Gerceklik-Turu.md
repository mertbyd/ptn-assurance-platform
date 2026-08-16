# AJAN GÖREVİ — KBP-107 · Composition host gerçeklik turu: migration, token ve yeşil koşum

Tek görev, **dört derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

Bu görev KBP-103 Dilim 3'ün devamıdır. Dilim 3 iki **gerçek** mimari blokajla durduruldu ve
sahte E2E kanıtı üretilmedi — doğru davranış. Bu görev önce iki blokajı kaldırır, sonra
Dilim 3'ü tamamlar.

Kapattığı şey yalnız bir test değil: **consumer kabul kapısı 6, 7 ve 10** (Integration-Readiness-Truth)
bugüne kadar hiç koşmadı. Üçü de bu görevde kapanır.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-107   (KBP-103 üzerinden)
Motor   : PostgreSQL
Commit  : #KBP-107 <type>: <past-tense English description>
```

| Ön koşul | Durum |
|---|---|
| KBP-103 Dilim 1–2 commit edilmiş | ✅ `349a4d6`, `14ff49d` — build 0 hata, 170/170 + canlı 2/2 |
| `redocly/cli:2.14.0` yerelde | ✅ digest `sha256:f96b920a…` |
| Canlı test altyapısı ve `Category=LiveInfrastructure` filtresi | ✅ KBP-103 Dilim 2 kurdu — **kalıbı kopyala, yeniden icat etme** |
| **ADR-0022** kabul edildi | ✅ 2026-08-15 — Dilim 3'ün anayasası |

**Dosya bütçesi ≈35.** Dört dilim, dilim başına bir commit.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| Migration orkestrasyonu | `infrastructure-bootstrap.md` → *composition host migration* | `host/Ptn.TestModule.HttpApi.Host/TestModuleHttpApiHostModule.cs:255` (bugünkü **kusurlu** hâli) |
| Canlı test | KBP-103 Dilim 2'nin kalıbı | `test/Ptn.TestModule.Application.Tests/LiveInfrastructure/RedoclyLintLiveTests.cs` |
| Derleyici değişikliği | `house-profile.md` → *Base classes* | `src/Ptn.TestModule.Domain/Managers/Compilation/ArazzoCompilerManager.cs` |
| HAR yorumlama | aynı bölüm | `src/Ptn.TestModule.Domain/Managers/Runs/HarInterpreter.cs` |
| Sabit sahipliği | `house-profile.md` → *Stable string ownership* | `Domain.Shared/Constants/Runs/WorkflowRunnerConsts.cs` |

**Kanonik kararlar:** **ADR-0022** (SUT adım korelasyonu — bu görevin Dilim 3 anayasası),
ADR-0021 (checker korelasyonu — **değişmez**), ADR-0013 (Test Module resource server'dır),
ADR-0015 (koşum sınırı), RULE-0002 (migration sahipliği).

---

## 2. Ölçülen blokajlar — bu görevin ilk yarısı budur

### 2.1 Migration/seed asimetrisi

`TestModuleHttpApiHostModule.cs:266-279`:

```csharp
if (autoMigrate)   { await scope...GetRequiredService<TestModuleDbContext>().Database.MigrateAsync(); }
if (seedOnStartup) { await scope...GetRequiredService<IDataSeeder>().SeedAsync(); }
```

**Migration tek context'e kapsanmış, seed tüm graph'a yayılıyor.** `IDataSeeder.SeedAsync()`
Authenticator/Identity/OpenIddict/Notifications contributor'larını da çağırıyor; onların
tabloları hiç oluşturulmadı → temiz PostgreSQL'de `42P01 undefined_table`.

Satır 254'teki yorum ilkeyi doğru yazıyor (*"her modül kendi migration assembly'sinin
sahibidir"*) — eksik olan o assembly'leri **koşturan** orkestrasyon.

### 2.2 SUT adımları bağlanamıyor

`HarInterpreter.cs:111` `stepKey`'i yalnız **yanıt gövdesinden** okuyor. Sıradan SUT yanıtı
echo etmez → `StepKey = null` → `Inconclusive`.

**ADR-0022 bunu çözdü.** Uygulaması Dilim 3'tedir.

### 2.3 Sabitlenen kararlar

- **Yeni `DbMigrator` projesi açılır.** Gerekçe *"daha temiz görünüyor"* değil, **açık
  gereksinim**: consumer kabul kapısı 6 ve 7. ABP'nin composition host kalıbı budur;
  host'un `PostApplicationInitialization`'ında tüm graph'ı migrate etmek başlangıç sırasını
  ve idempotency'yi belirsizleştirir.
- **Seed asimetrisi kapatılır.** `seedOnStartup`, migration tamamlanmadan **çalışmaz**;
  iki bayrak birbirine bağlanır.
- **`autoMigrate` üretim varsayılanı `false` kalır.** Bu görev bayrağın anlamını değiştirmez.
- **Kimlik/OpenIddict tablolarını yalnız Authenticator sahiplenir** (kapı 7). Test Module
  migration assembly'si onlara **dokunmaz**; migrator yalnız sırayı kurar.
- **Token turu gerçek olur.** ADR-0013: Test Module **resource server**'dır, `HttpApi`
  compose edilmez. Token Authenticator host'undan alınır; Test Module yalnız JWT bearer
  doğrular. Sahte token, test-only authentication handler veya `AllowAnonymous` **kullanılmaz**
  — kapı 10 tam olarak bunu kanıtlamak içindir.
- **Migration üretilmez.** Bu görev şema değiştirmez; yalnız var olan migration'ları koşturur.

---

## 3. Dilimler

### Dilim 1 — Migration orkestrasyonu (≈10 dosya) · **kapı 6 + 7**

| # | Dosya | Ne |
|---|---|---|
| 1 | `host/Ptn.TestModule.DbMigrator/**` | **yeni proje** — graph'taki her modülün `DbContext`'ini bağımlılık sırasına göre migrate eder, sonra seed eder |
| 2 | `host/.../TestModuleHttpApiHostModule.cs` | **düzenle** — §2.3'ün seed/migration bağı; asimetri kapanır |
| 3 | `Domain.Shared/Constants/TestModuleConfigurationKeys.cs` | **düzenle** — gerekirse migrator anahtarı |

**Kabul:** temiz bir PostgreSQL'de migrator koşar, **tüm** modül tabloları oluşur, seed
`42P01` almadan tamamlanır, ikinci koşuda idempotent kalır.

**Commit:** `#KBP-107 feat: created the composition host database migrator`

---

### Dilim 2 — Gerçek token turu (≈8 dosya) · **kapı 10**

Authenticator host'u ayağa kalkar; login → refresh → selected-context → logout turu **gerçek**
token ile koşar; Test Module'ün izinli lookup ucu (`TestModulePermissions.Lookups.Default`)
o token'la **200** döner, token'sız **401** döner.

Sahte handler yok, `AllowAnonymous` yok.

**Commit:** `#KBP-107 test: created the live token round against the resource server`

---

### Dilim 3 — ADR-0022 korelasyonu ve **yeşil uçtan uca koşum** (≈12 dosya)

| # | Dosya | Ne |
|---|---|---|
| 1 | `Domain.Shared/Constants/Runs/WorkflowRunnerConsts.cs` | **düzenle** — header adı sabiti (ADR-0022 §E) |
| 2 | `Domain/Managers/Compilation/ArazzoCompilerManager.cs` | **düzenle** — her adıma header parametresi enjekte (§A) |
| 3 | `Domain/Managers/Runs/HarInterpreter.cs` | **düzenle** — önce istek header'ı, sonra yanıt echo'su (§B); çelişkide bağlanmamış (§C) |
| 4 | `test/.../LiveInfrastructure/TestRunGreenPathLiveTests.cs` | **yeni** — KBP-103'ün tamamlanmamış Dilim 3'ü |
| 5 | `test/.../LiveInfrastructure/Fixtures/lookup-readback.arazzo.yaml` | **yeni** — elle yazılmış senaryo |

Zincir eksiksiz koşar: `ScenarioCompilationService` → `WorkflowRunnerService` → gerçek runner
→ HAR → `HarInterpreter` → `OracleDispatchManager` → `RunOutcomeResolver` →
`TestRunResultManager` → `test_run_results` satırı.

**Kabul:** `StatusCode` ve `OutcomeCode` terminalde **Passed**; `HasUnboundEntries = false`;
**hiçbir entry `Inconclusive` değil**; `ExecuteTestRunJob` gerçekten kuyruktan koştu;
**sıfır model çağrısı** (mevcut reflection testi korunur).

**Commit:** `#KBP-107 feat: created the request header step correlation and proved the green end to end run`

---

### Dilim 4 — Determinizm ve regresyon kapıları (≈5 test)

| # | Test | Doğruladığı |
|---|---|---|
| 1 | `ArazzoCompilerManagerTests` | Aynı girdi → aynı `compiled_hash` (enjeksiyondan **sonra** da) |
| 2 | `HarInterpreterTests` | İstek header'ı öncelikli; yanıt echo'su yedek; çelişkide bağlanmamış |
| 3 | `HarInterpreterTests` | Bildirilmemiş anahtar hâlâ reddediliyor (ADR-0022 §D) |
| 4 | `MigrationOrchestrationTests` | Migrator idempotent; ikinci koşu no-op |
| 5 | mevcut tüm testler | Davranış korundu |

**Commit:** `#KBP-107 test: created the correlation and migration orchestration gates`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **Dilim 2** devredilebilir *(kapı 10 ayrı kanıtlanabilir)*.
**Kesilmeyecekler: Dilim 1, 3, 4.** Dilim 1 olmadan Dilim 3 zaten koşmaz.

---

## 5. Yasaklar

1. **Sahte E2E kanıtı üretme.** Mock HAR, stub runner veya kurgu yeşil sonuç yasak — KBP-103
   tam da bunu reddettiği için durdu.
2. Sahte token, test-only authentication handler veya `AllowAnonymous` ile kapı 10'u geçme (§2.3).
3. Konum/sıra tabanlı HAR eşlemesi (ADR-0021 §C, ADR-0022).
4. SUT'tan echo talep eden bir sözleşme yazma (ADR-0022 alternatifler).
5. **ADR-0021'i değiştirme.** Değişmedi; ADR-0022 boşluğu doldurur.
6. Kendi Arazzo uzantımızı veya DSL'imizi icat etme — header standart `in: header` parametresidir.
7. Identity/OpenIddict tablolarını Test Module migration assembly'sine sokma (kapı 7).
8. **Migration üretme.** Bu görev şema değiştirmez (§2.3).
9. Application servisine private iş metodu veya guard koyma — `ServiceShapeTests` **kapıdır**.
10. `Domain/Managers/**` içine `Process`/`File`/`Directory` yazma (KBP-102 kuralı).
11. Yeni katman, `Infrastructure/`, `Helpers/`, `Utils/` açma. *(`DbMigrator` projesi §2.3'te
    açık gereksinimle gerekçelendirilmiştir — tek istisna odur.)*
12. Canlı testi varsayılan `dotnet test` koşusuna sızdırma.
13. Kırılan testi silme, `Skip` etme, assertion zayıflatma.
14. `KBP-95..106` dallarına commit; force-push, rebase, amend.
15. Ara dilimlerde build/test atlama.

---

## 6. Kabul kriterleri

- Temiz PostgreSQL'de migrator **tüm** modül tablolarını kuruyor; seed `42P01` almıyor;
  ikinci koşu idempotent. **(kapı 6)**
- Identity/OpenIddict tablolarını yalnız Authenticator sahipleniyor. **(kapı 7)**
- Gerçek token turu geçiyor; izinli uç token'la **200**, token'sız **401**. **(kapı 10)**
- Elle yazılmış Arazzo senaryosu **uçtan uca yeşil** koşuyor; `test_run_results` satırı `Passed`.
- `HasUnboundEntries = false`; **hiçbir entry `Inconclusive` değil**.
- Enjeksiyondan sonra da aynı girdi → aynı `compiled_hash`.
- **Sıfır model çağrısı** — reflection testi yeşil.
- Migration **üretilmedi**.
- `dotnet build Ptn.TestModule.slnx -m:1` → 0 hata.
- `dotnet test --filter "Category!=LiveInfrastructure"` → 0 başarısız.
- `dotnet test --filter "Category=LiveInfrastructure"` → **hepsi geçiyor**, `Skip` yok.

---

## 7. Bitiş

1. §5'in 15 maddesini kendi kodunda tek tek kontrol et.
2. Dört dilimi sırayla commit et.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → iki filtreli `dotnet test`.
4. `/backend-verify` gate'i; her commit öncesi başlık doğrulayıcısını koştur.
5. Raporda **zorunlu**: migrator'ın temiz veritabanındaki tam çıktısı; token turunun gerçek
   HTTP durum kodları; yeşil koşumun HAR'ından **istek header'ı** alıntısı; `compiled_hash`
   determinizm kanıtı; ayağa kalkan her konteyner; `Skip` edilen her test ve sebebi;
   her varsayım.
6. Kapı 6, 7, 10'u Integration-Readiness-Truth'ta işaretle.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → bir kez.

---

## 8. Kapattığı borç

| Kayıt | Madde |
|---|---|
| Integration-Readiness-Truth | Consumer kabul kapısı **6**, **7**, **10** |
| PLAN-0003 Blok 1 | *"elle yazılmış Arazzo senaryosu uçtan uca yeşil koşuyor"* |
| Roadmap | *"Tek DB'de şema/migration sahipliği smoke'u"* · *"Gerçek token ile login/refresh/logout turu"* |
| HANDOFF §5 | KBP-95'in kanıtsız kabul kriteri |
| ADR-0022 | Uygulaması |
| KBP-103 | Durdurulan Dilim 3 |

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| TM-10 sandbox · TM-11 eşzamanlılık · TM-15 saklama | **KBP-108** — Dilim 3'e bağlı değil, paralel koşar |
| Rapor, artefakt, ihracat, operasyon | **KBP-104** |
| Checker yazarlık yüzeyleri | **KBP-106** |
| Ajan yüzeyi, MCP, Overlay | **KBP-105** |
| DB checker adımının canlı koşumu | Ayrı iş — kendi bağlantı profilini ister |
| LLM / model sağlayıcı seçimi | Kod tarafı bittikten sonraki karar |
