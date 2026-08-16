---
id: PLAN-0005
type: plan
status: draft
title: KBP-93 ile paralel yurutulebilecek is kumesi
updated: 2026-08-16
decision_refs:
  - ADR-0006
  - ADR-0007
  - ADR-0015
  - ADR-0016
  - ADR-0018
  - ADR-0019
  - ADR-0020
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0006
---

# PLAN-0005 — Paralel iş kümesi

> [!WARNING] Tarihsel plan
> Bu belge işlerin planlandığı andaki durumu korur; güncel paket ve uygulama gerçeği için
> `01-Current` sayfaları esastır. Wiki 2026-08-15'ten beri ayrı `docs/.git` deposunun `main`
> dalındadır. API Contracts bugün `alpha.7`, Database Comparison `alpha.8` olarak publictir.
> Aşağıdaki KBP-715 metni artık tamamlanmış `alpha.7` geçişinin tarihsel görev tarifidir.

Bu belge **[[90-Inbox/TASK-KBP-93-Kosum-Kayit-Modeli|KBP-93]] ile aynı anda yürütülebilecek**
işleri tanımlar. Her madde YouTrack task metni seviyesindedir; ajan manifestosu iş
alındığında yazılır (PLAN-0004'ün Bölüm A / Bölüm B ayrımı).

> **Numara uyarısı.** Aşağıdaki `KBP-9x` numaraları **önerilir**, YouTrack'te teyit edilmelidir.
> `KBP-94/95/96` KBP-93'ün §9'unda **sıralı zincire ayrılmıştır**; paralel işler onlarla
> çakışmamalıdır. Checker numaraları kendi hatlarını sürdürür (`api-contract` 6xx,
> `database-comparison` 7xx).

---

## 1. Çakışma matrisi — neden paralel güvenli

| Task | Depo / kök | Dokunduğu dosya kümesi | KBP-93 ile kesişim |
|---|---|---|---|
| **KBP-93** *(referans)* | `ptn-test-module` | `**/Runs/**`, `DbContext`, `Settings`, `Localization`, yeni migration | — |
| **KBP-97** | `docs/wiki-brain` | Yalnız Markdown | **Yok** |
| **KBP-98** | `vault` | `CheckNexus.Vault.csproj`, `release-manifest.json` | **Yok** |
| **KBP-99** | `ptn-test-module` | `host/*.csproj`, `common.props`, `Domain.Tests/Bridge/**` | **Yok** — KBP-93 host'a, `common.props`'a ve `Bridge/` testlerine dokunmuyor |
| **KBP-100** | `ptn-test-module` | `Domain/Managers/Compilation/**`, `Domain.Shared/Constants/Compilation/**`, `Domain.Tests/Compilation/**` | **Yok** — ayrı konu klasörü |
| **KBP-715** | `checkers/database-comparison` | Ayrı Git deposu | **Yok** |
| **KBP-716** | `checkers/database-comparison` | Ayrı Git deposu | **Yok** — ama **KBP-715 ile sürüm hattı çakışır**, sıralı |

**Tek gerçek kısıt:** `Localization/TestModule/{en,tr}.json` üç task tarafından da genişletilebilir
(KBP-93, KBP-99, KBP-100). Bu dosyalar **ekleme-only** olduğu için merge çakışması küçüktür;
yine de aynı anda iki dal açılacaksa **anahtarlar konu önekiyle** yazılır (`Run:`, `Bridge:`,
`Compilation:`).

**Gerçekleşen sıra:** Planlama varsayımının tersine KBP-95 önce, TASK-KBP-95'in izin verdiği
elle yazılmış Arazzo belgeleriyle tamamlandı; KBP-100 sonradan derleyici yüzeyini ekledi.

---

## 2. KBP-97 — Wiki senkronizasyonu

**Tip:** `docs` · **Depo/dal:** `docs/.git` · `main` · **Boyut:** S

**Amaç:** `01-Current` ve `LEDGER-0001` katmanını koda ve registry'ye hizalamak. Denetim serisi
13 bulgu üretti; bunların **doküman tarafı** hiç kapanmadı ve wiki'nin kendi yetki sırası
(*"çalışan kod > Current"*) bugün ihlal ediliyor.

**Yapılacaklar:**

| # | Kayıt | Düzeltme |
|---|---|---|
| 1 | `CURRENT-0001` yetenek tablosu | **Tamamlandı; sonra yeniden ilerledi.** API bugün `0.2.0-alpha.7`, DB `0.2.0-alpha.8`; source, registry ve consumer hizalı |
| 2 | `CURRENT-0001` Test Module satırı | *"`test_catalog` ve `test_run` tabloları henüz yok"* — `test_catalog` KBP-92 ile geldi |
| 3 | `CURRENT-0002` Database Checker bölümü | `alpha.6` yüzeyleri eksik: `IProjectionAppService`, `IAssertionDerivabilityAppService`, `IWriteSetCapabilityAppService`, şema fingerprint ucu, `CorrelationRefDto` |
| 4 | `LEDGER-0001` | **DB `alpha.3–alpha.6` kaydı yok**; **`CheckNexus.Vault 0.2.0-alpha.2` kaydı hiç yok**; *"DB common.props hâlâ alpha.2"* uyarısı bayat |
| 5 | `ARCH-0001` | *"Auth HttpApi aynı composition hostta açılır"* → **ADR-0013 tersini söylüyor**; *"composition host bu klasörde yok"* → 2026-08-13'ten beri var |
| 6 | `Test-Platform-Schema.dbml` | Project notu *"Arazzo 1.1"* → **`1.0.1`** (BULGU-07); lookup bölümüne `CURRENT-0001`'deki bilinçli sapmaya çapraz not |
| 7 | `ADR-0020` risk tablosu | *"şema mührü zaten `GetSchemaFingerprintAsync`'te"* satırı yanlıştı; **KBP-714 ile artık doğru** — satır yeniden yazılır |
| 8 | `AUDIT-0003` özet tablosu | **#03 ve #06 kapandı** (KBP-712 + KBP-91). Açık kalanlar: #05 → KBP-93, #11 → KBP-99, #13 → §4 kararı |
| 9 | `BACKLOG-0001` | **ACX-07 kapandı** (KBP-629 `OperationLinkSuggester`), **DBX-06 kapandı** (`TableDescriptionDto.ForeignKeyNeighbors`, gelen+giden). ACX-06 Test Module listesine taşınır (defterin kendi önerisi) |
| 10 | `CURRENT-0004` + `Roadmap` | KBP-90/91/92 kapanışları; kabul kapısı 8 ✅; *"TM-01..TM-59 başlamadı"* yanlış |
| 11 | `Inbox.md` | *"Vault public mi şirket içi feed mi"* sorusu **cevaplandı** — kapatılanlara taşınır |
| 12 | `00-Home` | `decision_refs`'e **ADR-0020, ADR-0021**; köprü task'ları *"KBP-87, KBP-88"* → **KBP-88, KBP-89**; belge sayaçları (16 research / 4 plan / 3 audit / 12 task) |
| 13 | `GUIDE-0005` | Sayaç; katalog tablosuna **RESEARCH-0016 satırı** |
| 14 | `RULE-0006` | **İki kapı** olarak yeniden yazılır: sözleşme türetilebilirliği (API `ValidateScenarioAssertionsAsync`) **+ şema türetilebilirliği** (DB `ValidateDerivabilityAsync`). AUDIT-0001 BULGU-03'ün metin tarafı |

**Yasaklar:** Araştırma belgelerini düzeltme — *"araştırma belgesi o günkü düşünceyi kaydeder"*
(GUIDE-0005 §5). Çelişki indekse not edilir, belgeye değil. Silinen ADR-0011'e yapılan tarihsel
atıflar **silinmez**.

**Kabul:** Her düzeltilen sayfanın `updated` alanı yenilenir; `id`/`decision_refs`/`rule_refs`
tutarlıdır; hiçbir ADR sessizce yeniden yazılmaz (değişen karar yeni ADR ister — buradaki 14
madde **kayıt düzeltmesidir**, karar değişikliği değil).

**Commit:** Wiki deposunun düz `docs:` commit grameri kullanılır; kaynak deposunun `#KBP-*`
başlığı wiki commit'ine uygulanmaz.

---

## 3. KBP-98 — Vault paketleme sertleştirme

**Tip:** `chore` · **Branch:** `KBP-98` · **Boyut:** S

**Amaç:** `CheckNexus.Vault`'u checker ailesinin kanıtlanmış release kalıbına hizalamak.
Backend-verify bu oturumda iki sapma buldu ve paket **zaten yayımlanmış** durumda.

**Yapılacaklar:**

1. **SourceLink koruma bendini ekle.** Checker `common.props` kalıbı:
   ```xml
   <PtnSourceRepositoryMetadataAvailable Condition="'$(CI)' == 'true'">true</...>
   <PtnSourceRepositoryMetadataAvailable Condition="'$(PtnSourceRepositoryMetadataAvailable)' == ''">false</...>
   <EnableSourceControlManagerQueries Condition="... != 'true'">false</...>
   <EnableSourceLink Condition="... != 'true'">false</...>
   <EmbedUntrackedSources Condition="... == 'true'">true</...>
   ```
   Bugün Vault csproj'unda **bu bent yok**; `Microsoft.SourceLink.GitHub` + `PublishRepositoryUrl=true`
   korumasız duruyor. `LEDGER-0001` bu bendi, yayımlanmış 16 checker paketinin *"var olmayan
   commit'e işaret etmemesinin"* sebebi olarak kaydediyor.
2. **`PackageValidationBaselineVersion` ver.** `EnablePackageValidation=true` baseline'sız
   çalışıyor; sözleşme kırığı **aranmıyor**. Yayımlanmış `0.2.0-alpha.2` artık geçerli baseline'dır.
3. **Sürümü ilerlet.** `0.2.0-alpha.2` immutable'dır; kaynak değiştiğine göre bir sonraki
   prerelease seçilir ve `release-manifest.json`'ın `immutableVersions`'ına `0.2.0-alpha.2` yazılır
   (DB checker manifestindeki kalıp).
4. **Yayımlanmış `0.2.0-alpha.2` nupkg'sini denetle:** `.nuspec`'inde `commit` özniteliği var mı?
   Varsa ve o commit origin'de yoksa **`LEDGER-0001`'e bulgu olarak yazılır** (paket geri çekilmez —
   yayımlanan sürüm immutable'dır).

**Yasaklar:** Yayımlanmış `0.2.0-alpha.2`'yi farklı içerikle tekrar push etme. Vault'u checker
`common.props`'una bağlama — ayrı sürüm hattıdır, yalnız **kalıp** kopyalanır.

**Kabul:** CI dışı `dotnet pack` repository/commit metadata **damgalamıyor**; baseline özelliği
manifest ve csproj'da tutarlı; `dotnet build` 0 uyarı; 10/10 Vault testi geçiyor.

**Commit:** `#KBP-98 chore: hardened the vault package release wiring with sourcelink and validation baseline gates`

---

## 4. KBP-99 — Test Module küçük borçları

**Tip:** `chore` + `test` · **Branch:** `KBP-99` · **Boyut:** S

**Amaç:** Denetim serisinin iki açık maddesini kapatmak.

**4.1 · BULGU-11 — host csproj'unda sabit sürüm**

`ptn-test-module/AGENTS.md`: *"Sürümler `common.props` içindeki değişkenlerden yönetilir;
csproj'a sabit sürüm yazılmaz."* İki satır bunu deliyor:

```
host/Ptn.TestModule.HttpApi.Host.csproj:30  Serilog.AspNetCore   Version="9.0.0"
host/Ptn.TestModule.HttpApi.Host.csproj:31  Serilog.Sinks.Async  Version="2.1.0"
```

Etki düşük (host paketlenmiyor — RULE-0001) ama kural istisnasız yazılmış ve **sessiz sürüm
sürüklenmesi** riski taşıyor. İki sürüm `common.props`'a değişken olarak taşınır.

**4.2 · BULGU-13 — `SchemaName` yasağının kapsamı · KARAR GEREKİYOR**

ADR-0018 yasağı *"köprü sözlüğünde `SchemaName` adında alan bulunmamalı"* diye **geniş** yazılmış;
uygulama **dar** yorumlamış:

| Tip | `SchemaName` |
|---|---|
| `PtnLocation` | ❌ yok — `ApiSchemaName`/`DbSchemaName`/`DbTableName` ayrı ✅ |
| `PtnCheckerTableDescription` · `PtnDatabaseAssertionRequest` · `PtnDatabaseAssertionSignal` | ✅ var |

ADR'nin **asıl koruduğu yer sağlam**: çakışma yalnız `PtnLocation`'da anlamlıydı (iki anlam aynı
anda taşınıyor). Kalan üç tip tek yönlü, yalnız DB tarafına giden modeller; ad hizalaması
Mapperly'yi `[MapProperty]`'siz tutuyor.

> **Seçenek (a) — önerilen:** ADR-0018 metni **konum ve rapor tiplerine** daraltılır; kod olduğu
> gibi kalır; drift testi yalnız o aileyi tarar.
> **Seçenek (b):** üç tip yeniden adlandırılır ve `[MapProperty]` kabul edilir — **mapper saflığı
> kuralını deler.**

(a) seçilirse: ADR metni **KBP-97'de**, drift testinin kapsam daraltması **burada**.

**Kabul:** Host csproj'unda sabit sürüm kalmadı; drift testi kapsamı ADR metniyle birebir aynı;
`dotnet build` 0 uyarı.

**Commit:** `#KBP-99 chore: moved host package versions to common props and scoped the bridge naming drift test`

---

## 5. KBP-100 — Arazzo doğrulama ve `x-checknexus-db` derleyicisi (TM-05)

**Tip:** `feat` · **Branch:** `KBP-100` · **Boyut:** M · **Kritik yol**

**Amaç:** Blok 0'ın son maddesini kapatmak. **`KBP-95` (runner adapter) buna bağlıdır** — runner
derlenmiş belge olmadan koşamaz.

**Yapılacaklar:**

- **`redocly lint` çağrısı** süreç sınırında; Arazzo şema geçerliliği yayın kapısı 1'i besler.
  Sabit sürümlü `redocly/cli` imajı, ADR-0015 §A'nın pinleme kuralı.
- **`x-checknexus-db` derleyicisi:** uzantıyı Database Checker'ın `POST /assertions/row|count|absent`
  ucuna giden **gerçek bir Arazzo adımına** derler (ADR-0015 §C). Çıktı `compiled_document`,
  girdi `source_document` — ikisi de KBP-92'nin `TestScenario` aggregate'inde **zaten var**.
- **Profil paketiyle adres çözümü:** uzantıdaki kavram, `PtnProfilePack` bağlamasıyla somut
  şema/tablo/anahtar adına çevrilir (ADR-0019 §B). Bu yüzden profil paketi **malzemedir**
  (ADR-0020 §A) — derleme somut adı gömer.
- **XPath criteria yasağı** yayın kapısında lint ile engellenir (ADR-0015 §G). Respect
  desteklemiyor.
- **Hedef sürüm `1.0.1`** — 1.1 **değil** (AUDIT-0002 / BULGU-07). Üretilen belgenin
  `arazzo:` alanı bu değeri taşır.
- `sourceDescriptions` üretimi ADR-0020 §B/5'in beşinci yayın kapısına çözülebilir olmalıdır.

**Yasaklar:** Kendi Arazzo parser'ımızı yazma (ADR-0015 §A). Runner'ı fork'lama veya plugin
yazma (§C). Kendi DSL'imizi icat etme. `Domain/Managers/` dışında yeni katman açma.

**Kabul:** Elle yazılmış `x-checknexus-db` uzantılı bir belge, DB Checker'ın gerçek HTTP adımını
içeren geçerli bir Arazzo `1.0.1` belgesine derleniyor; `redocly lint` temiz; XPath içeren belge
kapıda **reddediliyor**; derleme deterministik (aynı girdi → aynı `compiled_hash`).

**Commit:** `#KBP-100 feat: created the arazzo validation call and the database assertion step compiler`

---

## 6. KBP-715 — Database Checker `0.2.0-alpha.7` yayını

> [!SUCCESS] Tamamlandı ve aşıldı
> `alpha.7` sekiz PackageId için yayımlandı; ardından Database Checker `alpha.8`e yükseldi ve
> Test Module consumer'ı da `alpha.8`e hizalandı. Aşağıdaki metin tarihsel görev tarifidir.

**Tip:** `chore` · **Depo:** `checkers/database-comparison` · **Boyut:** S

**Amaç:** KBP-712/713/714 ile gelen yüzeyleri tüketilebilir yapmak. Bugün durum:

```
common.props           Version = 0.2.0-alpha.7 · baseline = 0.2.0-alpha.6
release manifest       immutableVersions = [alpha.2, alpha.6]      ← alpha.7 YOK
artifacts/release      alpha.7 nupkg + snupkg üretilmiş            ← push EDİLMEMİŞ
```

Kaynak yayımlanmış `alpha.6`'dan **ileride** ve Test Module `common.props`'u `alpha.6`'ya bağlı.
`alpha.7`'nin taşıdığı şema parmak izi ucu (KBP-714) `ADR-0020` malzeme mührünün kritik yolunda.

**Yapılacaklar:** `GUIDE-0003` playbook'u — pack kapısı, PackageValidation `alpha.6` baseline'ına
karşı, 8 `.nupkg` + 8 `.snupkg`, registry preflight, push, **push sonrası V3 flat-container ile
8/8 PackageId doğrulaması**, `release-manifest.json`'a `alpha.7` immutable kaydı,
**`LEDGER-0001`'e tam kayıt** (KBP-97 ile koordine).

**Yasaklar:** Yayımlanmış `alpha.6`'yı farklı içerikle tekrar push etme. Bilinmeyen çalışma
dizininden relative `-File .\scripts\...` çağırma (00-Home uyarısı).

**Kabul:** 8/8 PackageId registry'de `0.2.0-alpha.7`; ledger kaydı tam; Test Module
`common.props`'unun `CheckNexusDatabaseComparisonVersion` yükseltmesi **ayrı** bir işe bırakılır
(bu task consumer'a dokunmaz).

**Commit:** `#KBP-715 chore: published the database checker 0.2.0-alpha.7 package family`

---

## 7. KBP-716 — DBX-07 şema lint yüzeyi *(opsiyonel, Sınıf C)*

**Tip:** `feat` · **Depo:** `checkers/database-comparison` · **Boyut:** S–M
**Sıra:** KBP-715'ten **sonra** (aynı sürüm hattı)

**Amaç:** `BACKLOG-0001` DBX-07 — *"PK yok"*, *"unique yok"*, *"generated kolon"* uyarıları.
Yayın kapısında *"bu tabloya anahtarla assertion yazamazsın"* demek, koşumda `KeyNotUnique`
almaktan ucuzdur. `RULE-0006`'nın doğrulama maddesi bunu zaten istiyor.

**Not:** Defterin diğer iki Sınıf C maddesi **kapanmıştır** ve KBP-97'de işaretlenecektir —
**ACX-07** (KBP-629 `OperationLinkSuggester`) ve **DBX-06** (`TableDescriptionDto.ForeignKeyNeighbors`,
gelen+giden komşu). Açık kalan tek fırsat maddesi DBX-07 ile **ACX-08**'dir (uygunluk profilinin
senaryo başına geçersiz kılınması).

**Kabul:** Salt-okunur; `ADR-0007` değişmezini bozmuyor; bütçeli; yeni sürüm + baseline adımı
atlanmıyor.

---

## 8. Önerilen yürütme

| Şerit | Sıra |
|---|---|
| **A — ana hat** | KBP-93 → KBP-94 → KBP-95 → KBP-96 |
| **B — derleyici** | **KBP-100** *(KBP-95'in ön koşulu; A ile paralel başlar)* |
| **C — checker** | KBP-715 → KBP-716 |
| **D — borç** | KBP-97 · KBP-98 · KBP-99 *(birbirinden de bağımsız)* |

Tek koordinasyon noktası: **KBP-97 ile KBP-715** aynı ledger bölümüne yazar. KBP-715 önce
biterse ledger kaydını kendi commit'ine alır ve KBP-97 yalnız doğrular.
