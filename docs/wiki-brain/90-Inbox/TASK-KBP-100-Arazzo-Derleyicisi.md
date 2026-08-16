# AJAN GÖREVİ — KBP-100 · An 3-4: Arazzo doğrulama ve `x-checknexus-db` derleyicisi (TM-05)

Tek görev, **dört derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

Bu görev Blok 0'ın son maddesini kapatır ve **yayın kapısının kanıtını istemciden alır, makineye
verir.** Bugün `compiled_document`, `compiled_hash`, `IsSchemaValid` ve `AreAssertionsDerivable`
istemciden geliyor; yayın kapısı kendi doğrulamadığı bir kanıta bakıyor. Bu görev o kanıtı
sunucuda üretir.

> **Durum kaydı.** Derleyicinin **domain çekirdeği bu çalışma ağacında yazılmış ama commit
> edilmemiştir** (9 dosya, ~1137 satır, 4 test, 3 örnek YAML — hepsi `?? untracked`).
> Çekirdek derleniyor ve testleri var; **hiçbir yerden çağrılmıyor.** Bu görev onu commit'ler
> ve bağlar. Bu satır silinmez.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-100   (KBP-95 üzerinden — §2'ye bak)
Motor   : PostgreSQL
Lint    : redocly/cli:2.14.0  (Docker, SABIT sürüm — zaten sabitlenmiş)
Commit  : #KBP-100 <type>: <past-tense English description>
```

| Ön koşul | Durum |
|---|---|
| KBP-95 dört dilimi commit edilmiş | ✅ `8db755e`, `6919721`, `e67db45`, `f2fcb70`, `e658c18` |
| **Çalışma ağacı build'i** | ❌ **KIRIK** — 2 × `CS0246 HarArtifactContainer` (§3 Dilim 0) |
| `TestScenario` aggregate + `source/compiled` alanları | ✅ KBP-92 |
| `ScenarioPublicationGateManager` (5 kapı) | ✅ KBP-92 — **kanıtı istemciden alıyor** |
| `ProfilePackManager.GetValidated` / `ResolveConcept` | ✅ KBP-88/89 |
| `IProcessBoundaryPort` (ortak süreç sınırı) | ✅ KBP-95 `e658c18` |
| Türetilebilirlik yüzeyleri (API + DB) | ✅ `IApiOracleAppService`, `IDatabaseOracleAppService` |
| `ArazzoCompilerManager` + `ArazzoLintManager` + linter portu | ⚠️ **yazılmış, commit edilmemiş, bağlanmamış** |

**Dosya bütçesi ≈35.** Dört dilim, dilim başına bir commit. Testler son dilimde.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| Manager | `house-profile.md` → *Base classes* | `src/Ptn.TestModule.Domain/Managers/Catalog/ScenarioPublicationGateManager.cs` |
| Dış süreci çağıran servis | `house-profile.md` → *AppService has no private helpers* | `src/Ptn.TestModule.Application/Services/Runs/WorkflowRunnerService.cs` |
| Katalog AppService | `house-profile.md` | `src/Ptn.TestModule.Application/Services/Catalog/TestScenarioAppService.cs` |
| Port arayüzü | `layers-and-files.md` | `src/Ptn.TestModule.Domain/Interface/Runs/IWorkflowRunnerPort.cs` (biçim) |
| DTO + validator | `mapping.md` | `Application.Contracts/Dtos/Catalog/*` + `FluentValidation/Catalog/*` |
| Mapperly | `mapping.md` → *pure partial* | `src/Ptn.TestModule.Application/Mappers/Catalog/TestScenarioMapper.cs` |
| Sabit / kod kümesi | `house-profile.md` → *Stable strings* | `Domain.Shared/Constants/Compilation/ArazzoCompilationConsts.cs` |

**Kanonik kararlar:** `ADR-0015 §C` (**bu görevin anayasası** — DB assertion'ı gerçek Arazzo
adımına derleme), `ADR-0015 §A` (pinleme, kendi parser'ımız yok), `ADR-0015 §G` (XPath yasağı),
`ADR-0019 §B` (profil paketiyle adres çözümü), `ADR-0020 §A/§B` (profil paketi **malzemedir**;
beşinci yayın kapısı `sourceDescriptions`), `ADR-0014 §C` + `AUDIT-0002 BULGU-07` (hedef sürüm
**`1.0.1`**), `RULE-0006` (türetilemeyen assertion yayınlanamaz).

---

## 2. Sabitlenen kararlar — tartışmaya açık değil

### 2.1 Branch konumu

`KBP-100` dalının **kendine ait tek commit'i yoktur**; bugün `KBP-93` (`48b2fcd`) üzerinde
duruyor. PLAN-0005 §1 KBP-100'ü KBP-95'in **ön koşulu** saymıştı; pratikte KBP-95 elle yazılmış
belgeyle önce indi. Tarihi tersine çevirmek yok:

```
git branch -f KBP-100 KBP-95     ← benzersiz commit yok, kayıp yok
git checkout KBP-100
```

**Yeni depo, yeni dal ismi, rebase, force-push yok.** `KBP-95` dalına dokunulmaz.

### 2.2 Yayın kanıtının sahibi **makinedir** — bu görevin asıl işi

Bugünkü hal (kanıt istemciden):

```csharp
// PublishTestScenarioDto — istemci ne derse kapı ona inanıyor
public bool IsSchemaValid { get; set; }
public bool AreAssertionsDerivable { get; set; }
public List<Guid> SourceDescriptionSpecSnapshotIds { get; set; } = [];

// CreateTestScenarioDto / UpdateTestScenarioDto
public string CompiledDocument { get; set; }   // istemci derlenmiş belgeyi kendisi yolluyor
public string CompiledHash { get; set; }
```

Bu, `RULE-0006`'nın kapısını **kâğıt üstünde** bırakıyor: istemci `IsSchemaValid = true` yollayıp
runner'a keyfî bir `compiled_document` koşturtabilir. `ADR-0015 §C` derlemenin **yayın anında**
olmasını, `ARCH-0004` An 4'ün 1-2-3 numaralı kapılarını **"Makine"**ye vermesini şart koşar.

**Karar — beş kanıt alanı da sunucuda türetilir:**

| Alan | Bugün | Bu görevden sonra |
|---|---|---|
| `CompiledDocument` | istemci gönderir | `ArazzoCompilerManager.CompileAsync` üretir |
| `CompiledHash` | istemci gönderir | derleyici hesaplar (kanonik, deterministik) |
| `AssertionCount` | istemci gönderir | `CompiledAssertionCount`'tan gelir |
| `IsSchemaValid` | istemci gönderir | **gerçek** `redocly lint` çıkış kodundan gelir |
| `AreAssertionsDerivable` | istemci gönderir | API + DB türetilebilirlik yüzeylerinden gelir |
| `SourceDescriptionSpecSnapshotIds` | istemci gönderir | derlenmiş belgenin `sourceDescriptions`'ından çözülür |

İstemcinin yayın anında sahip olduğu tek şey **niyet ve onaydır**; kanıt değil.

**DTO sonucu:** yukarıdaki alanlar public girdi DTO'larından **çıkarılır**. `CreateTestScenarioDto`
ve `UpdateTestScenarioDto` yalnız `SourceDocument` + malzeme mührünü taşır. `PublishTestScenarioDto`
istemcinin gerçekten sahip olduğu alan kalmazsa **tamamen kaldırılır** ve
`PublishAsync(Guid id)` / `EvaluatePublicationAsync(Guid id)` imzasına inilir. İlgili
validator'lar da aynı pasta sadeleşir — **boş validator bırakma.**

> Bu **sözleşme kırıcı** bir değişikliktir ve bilinçlidir. Modül yayımlanmamıştır, `RemoteService`
> kapalıdır (`[RemoteService(IsEnabled = false)]`), tüketici yalnız kendi testlerimizdir.
> Kırılan her test **düzeltilir**, silinmez.

### 2.3 Derlemenin tetiklendiği yer

`ADR-0015 §C` *"yayın anında derlenir"* der. **İki çağrı noktası vardır ve ikisi de aynı
Application servisini çağırır:**

| Uç | Ne yapar | Kalıcılık |
|---|---|---|
| `EvaluatePublicationAsync` | derler, lint'ler, türetilebilirliği sorar, **kararı döndürür** | **yazmaz** — kuru koşum |
| `PublishAsync` | aynısını yapar, kapılar geçerse `compiled_document`'i **satıra yazar** | yazar |

İki uç arasında **kanıt kopyalanmaz**; `EvaluatePublicationAsync` sonucuna güvenip `PublishAsync`
tekrar derlemeyi atlamaz. Derleme deterministiktir; aynı girdi aynı `compiled_hash`'i verir.

### 2.4 Süreç sınırı zaten ortaktır

`redocly lint` çağrısı **yeni bir süreç sınırı açmaz.** KBP-95'in `e658c18` commit'i
`IProcessBoundaryPort` + `ProcessExecutionPlan` + `ProcessExecutionOutcome` üçlüsünü tam bu iş
için çıkardı; `ArazzoLintManager.CreatePlan`/`Interpret` zaten o sözleşmeye yazılmış.
`RedoclyArazzoDocumentLinter` yalnız planı alır, ortak sınırda koşar, çıktıyı Manager'a verir.

**Yeni `Process` kullanımı, yeni timeout mekanizması, yeni geçici klasör yönetimi yazma.**

### 2.5 Profil paketi malzemedir

Derleyici kavramı somut şema/tablo/anahtar adına `ProfilePackManager.ResolveConcept` ile çevirir
(ADR-0019 §B) ve **somut adı belgeye gömer**. Bu yüzden profil paketi `ADR-0020 §A`'da malzemedir
ve `ProfileFingerprint` malzeme mührünün parçasıdır — mühür zaten `IsMaterialSealComplete`'te
kontrol ediliyor. Profil paketi senaryonun mühürlü malzemesinden **sunucuda** çözülür; istemciden
profil paketi gövdesi kabul edilmez.

### 2.6 Hata sınıflandırması

| Durum | Sonuç |
|---|---|
| Belge boş / bütçe aşımı / YAML parse hatası | `InvalidDocument` |
| `arazzo:` alanı `1.0.1` değil | `UnsupportedVersion` |
| Herhangi bir `criteria` XPath tipinde | `XPathCriteriaUnsupported` — **lint'ten önce**, derleme anında |
| `x-checknexus-db` operasyonu sözlükte yok | `UnsupportedDatabaseOperation` |
| Kavram profil paketinde bağlı değil | `ConceptColumnNotBound` |
| Docker çıkış kodu 125-127 | `LintProcessFailed` — **şema geçersizliği değil** |
| Lint bütçesi aşıldı | `LintTimedOut` |
| Lint çıkış kodu ≠ 0 (ve ≠ 125-127) | Exception **değil** — `IsSchemaValid = false` + tanı |

Son satır önemlidir: **geçersiz belge bir iş sonucudur, bir altyapı hatası değildir.** Kapı
kodlu karar döndürür; exception atmaz.

---

## 3. Dilimler ve dosya manifestosu

### Dilim 0 — Ağaç onarımı (KBP-95 dalında, ≈6 dosya)

**Bu dilim `KBP-100`'e değil, `KBP-95` dalına yazılır.** Bugünkü çalışma ağacı derlenmiyor.

| # | Ne | Neden |
|---|---|---|
| 1 | `Application/Services/Runs/HarArtifactContainer.cs` **geri getirilir** | Silinmiş ama `HarArtifactService` iki yerde kullanıyor → `CS0246` × 2 |
| 2 | 4 × `Services/Runs/*.cs` boş satır silme **geri alınır** | `HarArtifactService`, `OracleDispatchService`, `TestRunAppService`, `WorkflowRunnerService` — depo biçimiyle çatışan gürültü |

`git checkout -- <yol>` ile geri alınır. **`Compilation/` dosyalarına ve `en/tr.json`'daki
`TestModule.Compilation:*` anahtarlarına dokunma** — onlar Dilim 1'e aittir.

`dotnet build Ptn.TestModule.slnx -m:1` → **0 hata** olduğu görülmeden Dilim 1'e geçilmez.

**Commit:** `#KBP-95 fix: restored the har artifact container and the run service formatting`

---

### Dilim 1 — Derleyici çekirdeğini commit'e al (≈13 dosya, çoğu yazılmış)

Branch `KBP-100`'e geçilir (§2.1). Aşağıdaki **untracked** dosyalar gözden geçirilip commit edilir:

| # | Dosya | Satır | Durum |
|---|---|---|---|
| 3 | `Domain.Shared/Constants/Compilation/ArazzoCompilationConsts.cs` | 83 | yazılmış |
| 4 | `Domain.Shared/ExceptionCodes/Compilation/TestModuleCompilationErrorCodes.cs` | 18 | yazılmış |
| 5 | `Domain/Models/Compilation/ArazzoCompilationResult.cs` | 12 | yazılmış |
| 6 | `Domain/Models/Compilation/ArazzoLintResult.cs` | 9 | yazılmış |
| 7 | `Domain/Interface/Compilation/IArazzoDocumentLinter.cs` | 13 | yazılmış |
| 8 | `Domain/Managers/Compilation/ArazzoCompilerManager.cs` | 664 | yazılmış |
| 9 | `Domain/Managers/Compilation/ArazzoLintManager.cs` | 98 | yazılmış |
| 10 | `Application/Services/Compilation/RedoclyArazzoDocumentLinter.cs` | 37 | yazılmış |
| 11 | `test/Domain.Tests/Compilation/ArazzoCompilerManagerTests.cs` | 203 | 4 test |
| 12-14 | `samples/arazzo/*.yaml` (3 dosya) | — | yazılmış |
| — | `Localization/TestModule/{en,tr}.json` `Compilation:*` anahtarları | 9 × 2 | yazılmış |

**Commit etmeden önce gözden geçir** — bunlar başka bir oturumda yazıldı, doğruluğu varsayılmaz:

- `ArazzoCompilerManager` **664 satırdır.** House limiti public use-case metodu başına 25 satır ve
  iki iç içe kontrol seviyesidir. Metot metot ölç; aşan varsa **adlandırılmış sorumluluk** olarak
  ayır (pass-through helper değil). Sınıfın tamamı 664 satır olabilir; **tek bir metodu** olamaz.
- `TestModuleDomainService` tabanı, namespace, yorum yoğunluğu ve DI biçimi kardeşlerle birebir mi.
- `Ptn.DatabaseChecker.Constants.*` doğrudan `using`'i: Domain katmanı checker paketine bakıyor.
  `ptn-test-module/AGENTS.md` checker DTO paketlerini **Application**'da tutuyor. **Bu bir sapmadır
  — ya Domain'den çıkar ya da AGENTS.md'de gerekçeli istisna olarak kayda geçer.** Sessiz bırakma.
- 4 test kabul kriterlerinin hepsini kapsıyor mu (§6).

**Commit:** `#KBP-100 feat: created the arazzo validation call and the database assertion step compiler`

---

### Dilim 2 — Yayın kapısını makine kanıtına bağla (≈12 dosya)

**`Application/Services/Compilation/`**

| # | Dosya | Sorumluluk |
|---|---|---|
| 15 | `ScenarioCompilationService.cs` | Düz orkestrasyon: profil paketini çöz → `CompileAsync` → türetilebilirliği iki yüzeye sor → kanıtı `ScenarioCompilationEvidence` olarak döndür. **Karar vermez, private iş gövdesi taşımaz.** |

**`Domain/Models/Compilation/`**

| # | Dosya | İçerik |
|---|---|---|
| 16 | `ScenarioCompilationEvidence.cs` | `CompiledDocument`, `CompiledHash`, `AssertionCount`, `IsSchemaValid`, `AreAssertionsDerivable`, `SourceDescriptionSpecSnapshotIds`, `LintDiagnostics` |

**Değişecek:**

| # | Dosya | Değişiklik |
|---|---|---|
| 17 | `ScenarioPublicationGateManager.cs` | `Evaluate(scenario, evidence)` — `TestScenarioPublishModel` yerine **makine kanıtını** alır; beş kapı aynen kalır |
| 18 | `TestScenarioAppService.cs` | `EvaluatePublicationAsync` / `PublishAsync` derlemeyi çağırır; `PublishAsync` kapılar geçerse derlenmiş belgeyi Manager'a yazdırır |
| 19 | `TestScenarioManager.cs` | `compiled_document`/`compiled_hash`/`assertion_count` **mutasyonunu Manager sahiplenir**; istemciden gelen değer artık yok |
| 20-22 | `CreateTestScenarioDto` · `UpdateTestScenarioDto` · `PublishTestScenarioDto` | §2.2'deki alan çıkarımı |
| 23-25 | Üç validator | Çıkarılan alanların kuralları silinir; **boş validator kalırsa dosya da silinir** |
| 26 | `TestScenarioMapper.cs` | Kaldırılan alanların eşlemeleri; Mapperly'yi **ignore eklemeden** yeşile al, RMG tanısını oku |

**Commit:** `#KBP-100 feat: derived the scenario publication evidence from the machine compiler`

---

### Dilim 3 — Testler (≈8 dosya/test)

| # | Test | Doğruladığı |
|---|---|---|
| 27 | `ArazzoCompilerManagerTests` | Aynı girdi + aynı profil → **aynı `compiled_hash`** (determinizm) |
| 28 | `ArazzoCompilerManagerTests` | `x-checknexus-db` üç operasyonun her biri **gerçek** DB Checker HTTP adımına derleniyor |
| 29 | `ArazzoCompilerManagerTests` | XPath criteria **lint'e gitmeden** reddediliyor |
| 30 | `ArazzoCompilerManagerTests` | `arazzo: 1.1` reddediliyor, `1.0.1` kabul ediliyor |
| 31 | `ArazzoLintManagerTests` | Çıkış kodu 125-127 → `LintProcessFailed`; çıkış kodu 1 → `IsSchemaValid = false`, **exception yok** |
| 32 | `ScenarioPublicationGateTests` | **İstemci `IsSchemaValid = true` iddia edemiyor** — kanıt yalnız derleyiciden |
| 33 | `ScenarioPublicationGateTests` | Lint kırmızıysa `SchemaValidity` kapısı düşüyor; türetilemeyen assertion `Derivability` kapısını düşürüyor (RULE-0006) |
| 34 | `ScenarioPublicationGateTests` | `sourceDescriptions`'tan çözülen snapshot kimlikleri satırdakiyle uyuşmazsa beşinci kapı düşüyor |

Mevcut `ScenarioPublicationGateTests` ve `TestScenarioCatalogTests` §2.2 yüzünden kırılacaktır.
**Düzeltilir, silinmez.** Silinen tek satır coverage bile raporda gerekçelenir.

**Commit:** `#KBP-100 test: created the arazzo compilation and machine publication evidence coverage`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **Dilim 1'in `Ptn.DatabaseChecker` using sapması** (§Dilim 1, üçüncü madde)
`KBP-99`'a devredilir — ama **kayda geçirilmeden değil**: `ptn-test-module/AGENTS.md`'ye açık
gerekçeli istisna satırı yazılır ve raporda bildirilir.

Kesilmeyecekler: Dilim 0'ın tamamı, §2.2'nin tamamı ve **#27, #32, #33** testleri.

---

## 5. Yasaklar

1. **Kendi Arazzo parser'ımızı yazma** — YAML okuma dışında (ADR-0015 §A).
2. Runner'ı veya `redocly/cli`'yi **fork'lama, plugin yazma** (ADR-0015 §C).
3. **Kendi DSL'imizi icat etme** — Arazzo + Overlay standarttır.
4. `redocly/cli` sürümünü **sabitlemeden** çağırma; `latest` yasak.
5. **Yeni süreç sınırı mekanizması yazma** — `IProcessBoundaryPort` kullanılır (§2.4).
6. `Domain/Managers/` dışında **yeni katman, yeni proje, `Infrastructure/`, `Engines/`, `Compilers/`** açma.
7. Yayın kanıtını **istemciden alma** — §2.2'nin tamamı.
8. Geçersiz belgede **exception atma** — kodlu kapı kararı döndürülür (§2.6 son satır).
9. Docker altyapı hatasını **şema geçersizliği** sayma (çıkış kodu 125-127).
10. XPath kontrolünü **lint'e havale etme** — derleme anında, kendi kapımızda.
11. Profil paketi gövdesini **istemciden kabul etme** — mühürlü malzemeden çözülür (§2.5).
12. **Model/LLM çağrısı ekleme** — An 3-4'te derleme deterministiktir (RULE-0005).
13. Checker AppService'ini **doğrudan** çağırma — mevcut Bridge yüzeyleri üzerinden.
14. Kırılan testi **silme veya `Skip` etme** — düzeltilir.
15. Mapperly'ye **kanıtlanmamış ignore** ekleme; önce ignoresuz derle, RMG tanısını oku.
16. Migration üretme — **bu görev şema değiştirmez.**
17. `KBP-95` dalına Dilim 0 dışında commit atma; force-push, rebase, yeni depo.
18. Ara dilimlerde build/test atlama — **Dilim 0'dan sonra her dilim yeşil kapanır.**

---

## 6. Kabul kriterleri

- `dotnet build Ptn.TestModule.slnx -m:1` → **0 hata** (Dilim 0'dan itibaren her dilimde).
- Elle yazılmış `x-checknexus-db` uzantılı belge, DB Checker'ın **gerçek** `POST /assertions/row|count|absent`
  adımını içeren geçerli bir Arazzo **`1.0.1`** belgesine derleniyor.
- Derleme **deterministik**: aynı girdi + aynı profil → aynı `compiled_hash`.
- XPath criteria içeren belge **derleme anında** reddediliyor (lint'e hiç gitmiyor).
- `redocly lint` **pinli imajla** ve ortak süreç sınırında koşuyor.
- Yayın kapısının **beş kanıtı da sunucuda** üretiliyor; istemci hiçbirini iddia edemiyor.
- Kavram → somut şema/tablo/kolon çözümü profil paketinden geliyor; bağlanmamış kavram reddediliyor.
- `sourceDescriptions` beşinci yayın kapısına çözülebiliyor (ADR-0020 §B/5).
- Docker altyapı hatası ile geçersiz belge **ayrı** sonuçlar üretiyor.
- Migration **üretilmiyor**.

---

## 7. Bitiş

1. §5'in 18 maddesini kendi kodunda tek tek kontrol et.
2. Dört dilimi sırayla commit et (Dilim 0 → `KBP-95`, Dilim 1-2-3 → `KBP-100`).
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → `dotnet test Ptn.TestModule.slnx --no-restore`
4. `/abp-backend-dev` mimari incelemesi + `/backend-verify` gate'i.
5. Raporda: dosya listesi, **redocly imaj sürümü**, `Ptn.DatabaseChecker` using kararı,
   kırılıp düzeltilen testler, kesilen madde varsa etkisi, yaptığın **her varsayım**.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → bir kez; döngüde tekrar etme.

---

## 8. Kapattığı wiki borcu

| Kayıt | Madde |
|---|---|
| `PLAN-0003 TM-05` | Arazzo doğrulama + `x-checknexus-db` derleyicisi |
| `PLAN-0003 TM-17` | Türetilebilirlik kapısının **makine tarafı** |
| `PLAN-0005 §5` | KBP-100'ün tamamı |
| `ADR-0015 §C` | DB adımı derlemesinin **ilk gerçek uygulaması** |
| `ADR-0019 §B` | Profil paketiyle adres çözümünün derlemede kullanılması |
| `ADR-0020 §B/5` | Beşinci yayın kapısının gerçek kanıta bağlanması |
| `RULE-0006` | Kapının kâğıttan **makineye** taşınması |

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| TM-10 test verisi sandbox'ı | Sonraki |
| TM-13/14 CTRF · JUnit · SARIF dışa aktarımı | Sonraki |
| TM-15 saklama, blob TTL koşumu | Sonraki |
| TM-16 OTel telemetrisi | Sonraki |
| TM-18 kuru koşum (`scenario.dryRun`) | Blok 3 |
| Yazarlık ajanı, MCP, tool bütçesi | Blok 3 — KBP-97 sonrası |
| Wiki senkronizasyonu | **KBP-97** |
| Vault paketleme | **KBP-98** |
| Host sabit sürüm borcu | **KBP-99** |
