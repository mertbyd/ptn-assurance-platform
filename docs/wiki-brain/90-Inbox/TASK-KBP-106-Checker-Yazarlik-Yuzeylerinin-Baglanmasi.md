# AJAN GÖREVİ — KBP-106 · Checker yazarlık yüzeylerinin köprüye bağlanması

Tek görev, **beş derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

Bu görev **checker'lara tek satır yazmaz.** Checker'lar Test Module'ün talebi üzerine üç
yazarlık yüzeyi üretti ve **yayımladı**; köprü onları **hiç tüketmedi**. Dördüncü yüzey
(`ProfileCode`) tüketiliyor ama **sabite pinlenmiş** durumda.

Bunlar ajanın *"iyi Arazzo dosyası"* yazma kalitesini doğrudan belirleyen dört yüzeydir.
Bugün ajan bunları göremediği için **tahmin ediyor** — RULE-0007 bunu yasaklıyor.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-106   (KBP-103 üzerinden)
Motor   : PostgreSQL
Commit  : #KBP-106 <type>: <past-tense English description>
```

> **Paralellik.** Bu görev yalnız `**/Bridge/**` dokunur; KBP-104 `**/Runs/**` dokunur.
> **Aynı anda koşabilirler** — ama iki ajan aynı checkout'ta `git commit` çalıştıramaz
> (HANDOFF §0 paralellik kuralı). Ayrı worktree kullan veya sıraya al.
>
> **KBP-105'ten önce bitmeli.** KBP-105'in yazarlık dilimleri bu dört yüzeyin bağlı
> olduğunu varsayar.

| Ön koşul | Durum |
|---|---|
| KBP-103 Dilim 1–2 commit edilmiş | ✅ `349a4d6`, `14ff49d` — **yeşil E2E koşumuna ihtiyaç yok**, bu görev saf köprü kablolamasıdır |
| Köprü sözlüğü ve drift kapıları | ✅ KBP-88/89/99 — `BridgeVocabularyTests`, `VocabularyDriftTests` |
| API Contracts `0.2.0-alpha.5` **yayımlı**, 8/8 registry doğrulandı | ✅ 2026-08-14 — **ACX-07 bu sürümün içinde** |
| Database Comparison `0.2.0-alpha.8` **yayımlı**, 8/8 registry doğrulandı | ✅ 2026-08-15 — **DBX-06 ve DBX-07 bu sürümün içinde** |
| Test Module `common.props` her ikisine hizalı | ✅ KBP-103 Dilim 1 |

**Yeni paket sürümü GEREKMİYOR.** Dört yüzey de restore edilmiş paketlerde **zaten var**.

**Dosya bütçesi ≈40.** Beş dilim, dilim başına bir commit. **Migration üretilmez.**

### 0.1 Checker deposu hijyeni — bu görevin dışında, tek commit

`checkers/database-comparison` deposunda commitlenmemiş iş var:

```
M scripts/database-comparison.release.json    alpha.8 immutable mühürlendi
                                              requiredDependencies -> versionContains pinleri
```

İçerik doğru ve KBP-716'nın devamı. **O depoda** kendi commit grameriyle kapatılır:

`#KBP-716 chore: sealed the alpha.8 release and pinned the manifest dependency versions`

Bu tek commit dışında **checker depolarına dokunma.**

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| Köprü Manager'ı | `house-profile.md` → *An AppService has no private business helpers* | `src/Ptn.TestModule.Domain/Managers/Bridge/SchemaKnowledgeManager.cs` |
| Köprü servisi | aynı bölüm | `src/Ptn.TestModule.Application/Services/Bridge/DatabaseOracleAppService.cs` |
| Köprü modeli | `house-profile.md` → *One type, one file* | `src/Ptn.TestModule.Domain/Models/Bridge/Database/TableDescription.cs` |
| Köprü DTO + validator | `contracts-mapping.md` | `Application.Contracts/Dtos/Bridge/Database/*` + `FluentValidation/Bridge/*` |
| Mapperly | `house-profile.md` → *Mapper files contain declarations only* | `src/Ptn.TestModule.Application/Mappers/Bridge/PtnBridgeMapper.cs` |
| Sözlük sabiti | `house-profile.md` → *Stable string ownership* | `Domain.Shared/Constants/Bridge/Vocabulary/*` |
| Controller ucu | `house-profile.md` → *Architectural spine* | `src/Ptn.TestModule.HttpApi/Controllers/Bridge/PtnBridgeController.cs` |

**Kanonik kararlar:** **ADR-0018** (tek köprü sözlüğü, ≤12 tool, kanıt zinciri),
**ADR-0019** (profil paketi, kanıt yolu, yetenek seviyesi), **ADR-0007** (checker salt-okunur),
**RULE-0006** (türetilemeyen assertion yayınlanamaz), **RULE-0007** (ajan tahmin etmez),
BACKLOG-0001 Sınıf C.

**Önce oku, sonra yaz:** `test/Ptn.TestModule.Domain.Tests/Bridge/BridgeVocabularyTests.cs` ve
`VocabularyDriftTests.cs`. Yeni köprü tipi bu iki kapıdan geçmek zorundadır; adlandırma
sözleşmesini **testten** öğren, tahmin etme.

---

## 2. Ölçülen boşluk — bu görevin tamamı budur

2026-08-15 taraması, `ptn-test-module/src` genelinde:

| # | Checker yüzeyi | Nerede yayımlı | Köprüdeki durumu |
|---|---|---|---|
| **ACX-07** | `OperationLinkSuggester` — OpenAPI `links` tabanlı üretici→tüketici zinciri | API `alpha.5` (KBP-629) | ❌ **tek referans yok** |
| **DBX-06** | `TableDescriptionDto.ForeignKeyNeighbors` — 1 seviye komşu, gelen+giden | DB `alpha.8` | ❌ **tek referans yok** |
| **DBX-07** | `SchemaLintWarningDto` + `SchemaLintWarningCodes` — "PK yok / unique yok / generated" | DB `alpha.8` (KBP-716) | ❌ **tek referans yok** |
| **ACX-08** | `ResponseConformanceDto.ProfileCode` — `Strict` / `Runtime` / `Lenient` | API `alpha.5` | 🟡 **sabite pinli** |

**ACX-08 kanıtı** — `Domain/Managers/Bridge/ApiOracleManager.cs:50`:

```csharp
ProfileCode = PtnApiOracleRequestCodes.RuntimeProfile,
```

Checker üç profili **çağrı başına** kabul ediyor ve validator üçünü de geçiriyor. Köprü
`Runtime`'a sabitlediği için kritik senaryoda `Strict`, keşif senaryosunda `Lenient`
**istenemiyor**. BACKLOG-0001 ACX-08'in gerekçesi birebir buydu.

### 2.1 Bu dört madde neden *"iyi Arazzo"* demek

| Yüzey | Ajanın bugünkü hali | Bağlandıktan sonra |
|---|---|---|
| ACX-07 | Çok adımlı senaryonun iskeletini **tahmin ediyor** | Spec'in `links`'inden **türetiyor** |
| DBX-06 | `db.binding.suggest` komşuluk bilmiyor | FK yönüyle doğru tabloyu öneriyor |
| DBX-07 | Anahtarsız tabloya assertion yazıyor, **koşumda** `KeyNotUnique` yiyor | **Yayın kapısında** uyarı alıyor |
| ACX-08 | Her senaryoda aynı gürültü seviyesi | Kritikte sıkı, keşifte gevşek |

RULE-0007: *ajan tahmin etmez.* Bugün dördünde de tahmin ediyor.

### 2.2 Sabitlenen kararlar

- **Checker'a tek satır yazılmaz.** Dört yüzey de public pakette; iş yalnız tüketimdir.
- **Yeni paket sürümü çıkarılmaz.** `common.props` KBP-103 Dilim 1'de hizalandı.
- **Yeni tool açılmaz.** ADR-0018 ≤12 sınırı geçerlidir; yeni bilgi **mevcut** tool'ların
  yanıtını zenginleştirir. Katalog 12'ye dayalıysa birleştir, ekleme.
- **Lint uyarısı hüküm değildir.** DBX-07 çıktısı yayın kapısında **uyarı** üretir;
  senaryoyu tek başına reddetmez. Reddi RULE-0006'nın türetilebilirlik kapısı verir.
- **Profil çözümü Manager'ın işidir.** `ProfileCode` senaryodan gelir, servis onu taşımaz,
  seçimi Manager yapar (KBP-102 kuralı).
- **Migration üretilmez.** Senaryo başına profil `TestScenario`'nun mevcut alanlarından
  çözülür; yeni kolon gerekiyorsa **dur ve raporla** — şema değişikliği bu görevin dışıdır.

---

## 3. Dilimler

### Dilim 1 — ACX-07 operasyon zinciri önerisi (≈10 dosya)

`OperationLinkSuggester` çıktısı köprüye: Domain model + Manager + Application servisi +
DTO + validator + Mapperly + mevcut bir tool yanıtına bağlama.

Ajan çok adımlı senaryonun iskeletini spec'ten alır. Sözlük kapıları yeşil kalır.

**Commit:** `#KBP-106 feat: created the operation link chain surface for scenario authoring`

---

### Dilim 2 — DBX-06 FK komşuluk grafiği (≈8 dosya)

`TableDescriptionDto.ForeignKeyNeighbors` mevcut `TableDescription` modeline ve
`SchemaKnowledgeManager` yüzeyine taşınır; gelen ve giden yön **ayrı** korunur.

`db.binding.suggest` önerisi komşuluk bilgisiyle sıralanır.

**Commit:** `#KBP-106 feat: created the foreign key neighbor surface for binding suggestions`

---

### Dilim 3 — DBX-07 şema lint uyarıları yayın kapısında (≈9 dosya)

`SchemaLintWarningDto` köprüye alınır ve **`ScenarioPublicationGateManager`'a uyarı olarak**
bağlanır (§2.2 — hüküm değil, uyarı).

Ajan *"bu tabloya anahtarla assertion yazamazsın"* cevabını **yayın anında** alır.

**Commit:** `#KBP-106 feat: created the schema lint warning path into the publication gate`

---

### Dilim 4 — ACX-08 uygunluk profili senaryo başına (≈7 dosya)

`ApiOracleManager.cs:50`'deki `Runtime` pini kalkar. Profil senaryodan çözülür;
geçersiz kod validator'da reddedilir; **varsayılan `Runtime` kalır** (geriye dönük uyumlu).

`ConformanceProfileCodes`'un üç değeri Domain.Shared sabitine yansıtılır — inline string yazılmaz.

**Commit:** `#KBP-106 feat: created the per scenario conformance profile resolution`

---

### Dilim 5 — Kapılar ve testler (≈6 test)

| # | Test | Doğruladığı |
|---|---|---|
| 1 | `BridgeVocabularyTests` | Dört yeni yüzey sözlük sözleşmesine uyuyor |
| 2 | `VocabularyDriftTests` | ADR-0018 kapsamı korunuyor |
| 3 | `ToolCatalogTests` | Katalog **hâlâ ≤12 tool** |
| 4 | `ConformanceProfileTests` | Üç profil çözülüyor; geçersiz kod reddediliyor; varsayılan `Runtime` |
| 5 | `SchemaLintGateTests` | Lint uyarısı **tek başına** senaryoyu reddetmiyor |
| 6 | `PackageBoundaryTests` | Sürüm pinleri değişmedi — **yeni sürüm çıkarılmadı** |

**Commit:** `#KBP-106 test: created the bridge authoring surface gates`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **Dilim 2** devredilir — üçü içinde ajan kalitesine katkısı en dolaylı olan odur.
**Kesilmeyecekler: Dilim 1, 3, 4 ve Dilim 5.**

---

## 5. Yasaklar

1. **Checker deposuna kod yazma.** Dört yüzey de public pakette (§2.2). Tek istisna §0.1'in manifest commit'i.
2. Yeni checker paket sürümü çıkarma; `common.props` sürümlerini değiştirme.
3. Yeni tool açma; ≤12 sınırını aşma (ADR-0018).
4. Lint uyarısını **red** hükmüne çevirme (§2.2).
5. `ProfileCode`'u başka bir sabite pinleme; varsayılanı `Runtime` dışına taşıma.
6. Köprü sözlüğüne testlere bakmadan tip ekleme (§1).
7. `[MapProperty]` ile mapper saflığını delme — ad hizalaması tercih edilir (KBP-99 kararı).
8. Application servisine private iş metodu veya guard koyma — `ServiceShapeTests` **kapıdır**.
9. Yeni proje, yeni katman, `Infrastructure/`, `Helpers/`, `Utils/` açma.
10. **Migration üretme.** Şema değişikliği gerekiyorsa **dur ve raporla** (§2.2).
11. Checker'a yazma yetkisi verme (ADR-0007).
12. Kırılan testi silme, `Skip` etme, assertion zayıflatma.
13. `KBP-95..105` dallarına commit; force-push, rebase, amend.
14. Ara dilimlerde build/test atlama.

---

## 6. Kabul kriterleri

- Dört yüzeyin **hepsi** köprüden okunabiliyor; `grep` ile her biri en az bir Domain modeli,
  bir Manager ve bir Application.Contracts sözleşmesinde görünüyor.
- Ajan çok adımlı senaryo iskeletini **spec'in `links`'inden** alıyor, tahmin etmiyor.
- `db.binding.suggest` FK komşuluğunu kullanıyor; gelen ve giden yön ayrı.
- Anahtarsız tabloya assertion yazan senaryo **yayın kapısında uyarı** alıyor; uyarı tek
  başına reddetmiyor.
- Üç uygunluk profili senaryo başına çözülüyor; geçersiz kod reddediliyor; varsayılan `Runtime`.
- `ApiOracleManager.cs`'de sabit `ProfileCode` **kalmadı**.
- Tool kataloğu **≤12**.
- **Yeni paket sürümü çıkarılmadı**, `common.props` değişmedi — `PackageBoundaryTests` kanıtlıyor.
- **Migration üretilmedi.**
- `dotnet build Ptn.TestModule.slnx -m:1` → 0 hata.
- `dotnet test --filter "Category!=LiveInfrastructure"` → 0 başarısız.
- `dotnet test --filter "Category=LiveInfrastructure"` → KBP-103'ün kanıtı **hâlâ yeşil**.

---

## 7. Bitiş

1. §5'in 14 maddesini kendi kodunda tek tek kontrol et.
2. Beş dilimi sırayla commit et; §0.1'in commit'ini **DB checker deposunda** ayrıca at.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → iki filtreli `dotnet test`.
4. `/backend-verify` gate'i; her commit öncesi başlık doğrulayıcısını koştur.
5. Raporda **zorunlu**: dört yüzeyin **öncesi/sonrası** grep kanıtı; tool kataloğunun son
   sayısı ve listesi; profil çözümünün üç değeri için birer örnek; şema değişikliği
   gerektiğini düşündüğün her nokta; her varsayım.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → bir kez.

---

## 8. Kapattığı borç

| Kayıt | Madde |
|---|---|
| BACKLOG-0001 Sınıf C | **ACX-07**, **ACX-08**, **DBX-06**, **DBX-07** — dördü de tüketim tarafında |
| RULE-0007 | Ajanın dört noktada tahmin etmesi |
| Kullanıcı hedefi (2026-08-15) | *"ajan daha iyi Arazzo dosyası yazıp runner'a soksun"* |
| `checkers/database-comparison` | Commitlenmemiş release manifest'i (§0.1) |

Bu görev bittiğinde **BACKLOG-0001'de açık madde kalmaz** ve defter kapatılabilir.

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| Checker'ların **kıyaslama** tarafı (DB↔DB karşılaştırma, PLAN-0001/PLAN-0002 motor maddeleri) | **Kapsam dışı** — kullanıcı kararı 2026-08-15: yalnız Test Module'ün tükettiği yüzeyler |
| ACX-06 / ACC-18..22 MCP bütçe kapıları | **KBP-105** — BACKLOG-0001 engel notu bunu checker'dan Test Module'e taşıyor |
| Rapor, artefakt, operasyon | **KBP-104** |
| Ajan profilleri, MCP yüzeyi, Overlay yaması | **KBP-105** |
| LLM / model sağlayıcı seçimi | Dört task bittikten sonraki karar |
