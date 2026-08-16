---
id: ADR-0020
type: decision
status: accepted
title: Senaryo malzeme muhru — dort girdinin kimlik ve icerik baglamasi
created: 2026-08-14
updated: 2026-08-15
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs:
  - ADR-0014
  - ADR-0015
  - ADR-0016
  - ADR-0017
  - ADR-0018
  - ADR-0019
rule_refs:
  - RULE-0006
  - RULE-0007
---

# ADR-0020 — Senaryo malzeme mührü

> `test_scenarios` tablosuna **üç kolon** ekler. Yeni tablo açmaz; ADR-0016'nın
> **4 ana + 5 lookup** modeli aynen korunur.

## Bağlam

Bir senaryo sürümü **dört girdiden** üretilir:

```
senaryo.md ─┐
kurallar.md ─┼─→ [ajan + derleyici] ─→ senaryo sürümü (compiled Arazzo)
API snapshot ┤
DB şeması ───┘
```

Soru: koşum anında *"bu senaryo hâlâ üretildiği girdilere karşı geçerli mi"* nasıl kanıtlanır?

Mevcut şema bu soruya **yarım** cevap veriyor:

| Malzeme | Kimlik | İçerik mührü |
|---|---|---|
| senaryo.md → Arazzo | — | `source_hash` ✅ |
| kurallar.md | — | `rules_fingerprint` ✅ |
| API sözleşmesi | `spec_snapshot_id` ✅ | **yok** ❌ |
| DB şeması | **yok** ❌ | **yok** ❌ |

`test_runs` dört değeri de taşıyor (`spec_snapshot_id`, `db_connection_id`,
`spec_fingerprint`, `db_schema_fingerprint`) — yani **kayma koşumda tespit ediliyor, yazımda
değil.** Senaryonun bayatladığı ancak koşulduğunda anlaşılıyor.

## Global dayanak — üç bağımsız standart aynı şekle yakınsıyor

**1. Arazzo'nun kendi cevabı: `sourceDescriptions` zorunludur.**
Spesifikasyon bir Arazzo belgesinin **en az bir** `sourceDescriptions` girdisi taşımasını
**şart koşar**; bu alan workflow'ların hangi OpenAPI (veya AsyncAPI/Arazzo) belgelerine
dayandığını ilan eder ve *"programlama dilindeki namespace/import"* karşılığıdır.
**API tarafındaki bağlama standardın içindedir; bizim icat edeceğimiz bir alan değildir.**

**2. in-toto / SLSA provenance: subject + materials.**
Üretilen artefakt `subject`, üretimde tüketilen **her** girdi `materials` /
`resolvedDependencies` olarak **digest'iyle** kaydedilir. Desen net: *"bir artefakt,
üretildiği girdilerin **tamamının** hash'ini taşır"* — bazılarının değil.

**3. Pact Broker matrix:** doğrulama sonucu **tam sürüm çiftine** bağlanır; `can-i-deploy`
sorusu *"bu iki sürüm birlikte doğrulandı mı"*dır. Kimlik tek başına yetmez, **hangi
içerikle** doğrulandığı gerekir.

> **Ortak şekil:** artefakt = subject · girdi = material · her material **kimlik + içerik
> digest'i** taşır. Bizim eksiğimiz tam olarak "içerik digest'i" ve "DB tarafı material".

## Karar

### A. Dört malzeme mühürlenir

`test_catalog.test_scenarios` tablosuna eklenir:

```
spec_fingerprint       varchar(64)   -- API snapshot ICERIK muhru
db_connection_id       uuid          -- DB Checker kimligi (FK DEGIL)
db_schema_fingerprint  varchar(64)   -- DB semasi ICERIK muhru
profile_fingerprint    varchar(64)   -- profil paketi ICERIK muhru (ADR-0019 §B)
```

Profil paketi de bir malzemedir: `x-checknexus-db` **derleme anında** somut şema/tablo/anahtar
adına çevrilir (ADR-0015 §C), yani paket değişirse derlenmiş belgedeki adresler yanlışlanabilir.

**Kolon eklenir, tablo eklenmez.** Modüller arası anahtar yasağı korunur: `db_connection_id`
ve `spec_snapshot_id` düz `uuid`'dir, **FK değildir** (ADR-0015 §F).

### B. Yayın kapısı genişler — beşinci kontrol

RULE-0006'nın üç kapısına dördüncü ve beşinci eklenir:

4. **Malzeme bütünlüğü:** dört malzemenin **kimliği ve mührü** dolu olmalıdır. Boş bırakılan
   malzeme yayını **reddeder**.
5. **`sourceDescriptions` tutarlılığı:** derlenmiş belgenin `sourceDescriptions` girdileri
   `spec_snapshot_id`'ye çözülmelidir. Belge bir spec'e, satır başka bir spec'e işaret
   ediyorsa yayın **reddedilir**.

Beşinci kontrol bedavadır: alan zaten belgede ve **zorunludur**.

### C. Koşum kapısı: kayma `Failed` değil `Inconclusive`

Koşum anında dört mühür **yeniden hesaplanır** ve senaryonunkiyle karşılaştırılır.

| Durum | Sonuç |
|---|---|
| Dördü de tutuyor | Normal koşum |
| API veya DB mührü tutmuyor | **`Inconclusive`** + `failure_category = Technical`, kayan malzeme raporda adıyla |
| `rules_fingerprint` tutmuyor | **`Inconclusive`** — kural değişti, senaryo bayat |
| Profil mührü tutmuyor | **`Inconclusive`** — adresler yanlışlanmış olabilir |

**Kayma bir hata değildir, bir bilgi eksikliğidir.** `Failed` saymak yanlış alarmdır; Google'ın
ölçtüğü *"CI pass→fail geçişlerinin %84'ü flaky"* tuzağı tam buradadır (ADR-0016 risk tablosu).
`Skipped` saymak ise sessiz kapsam kaybıdır.

### D. Köprü malzemeyi üretir, senaryo saklar

`ptn_ground` yanıtı **malzeme mührünü** taşır: dört kimlik + dört digest. Ajan bunları
**yazmaz, taşır**; yayın anında satıra düşerler (RULE-0007 §1 — açık uçlu alan yok).

`ptn_validate` beşinci kontrolü (§B) çalıştırır ve sonucu `IsPublishable` ile birlikte döner.

## Alternatifler

- **Yalnız kimlik tutmak (bugünkü hâl):** snapshot yerinde durur ama içeriği değişebilir;
  Pact Broker'ın çözdüğü problem tam olarak budur.
- **Malzemeleri ayrı tabloya açmak:** hiçbir yerden FK almıyor, parent'ından bağımsız
  sorgulanmıyor, tekilleştirilmiyor — ADR-0016'nın üç kriteri de düşüyor.
- **Kaymayı `Failed` saymak:** yanlış alarm üretir; ekip kırmızıya güvenmeyi bırakır.
- **`sourceDescriptions`'ı yok saymak:** alan **zorunlu**; kontrol etmemek bedava bir kapıyı
  çöpe atmaktır.
- **Profil paketini malzeme saymamak:** derleme somut tablo adını gömüyor; paket kayarsa
  belge sessizce yanlış adresi vurur.

## Sonuçlar ve riskler

`test_scenarios`'a **4 kolon**; `PtnProfilePack`'e `SpecFingerprint` + `DbConnectionId`;
köprüye tek yeni model (`PtnMaterialSeal`); iki yayın kapısı kontrolü. **Yeni tablo, yeni
katman, yeni proje yok.**

| Risk | Önlem |
|---|---|
| Mühür hesabı pahalı olur | **Bugün doğru, yazıldığı gün değildi** — aşağıdaki nota bak. Şema mührü `ISchemaKnowledgeAppService.GetSchemaFingerprintAsync` (Test Module köprüsü) → `ISchemaDiscoveryAppService.GetSchemaFingerprintAsync` (DB Checker) zincirindedir; snapshot mührü API Checker'da hazırdır |
| Her küçük şema değişikliği senaryoları bayatlatır | Mühür **kanonik** hesaplanır (sıralı, denetim/istatistik alanı hariç); ilgisiz değişiklik mührü kaydırmaz |
| `Inconclusive` yığılır ve görmezden gelinir | Kapsam raporunda ayrı sayaç; bayat senaryo listesi tetikleyiciye bağlanır (`trigger_kind = ContractChange`) |
| Ajan mührü kendisi üretmeye çalışır | Mühür **checker'dan** gelir; köprü taşır, ajan yazamaz |

> [!NOTE] Kayıt düzeltmesi (2026-08-15) — risk tablosunun birinci satırı
> Bu ADR 2026-08-14'te yazıldığında *"şema mührü **zaten** `GetSchemaFingerprintAsync`'te"*
> diyordu. **O gün bu doğru değildi** — uç henüz yoktu, dolayısıyla "mühür hesabı pahalı olur"
> riskinin önlemi aslında mevcut olmayan bir yüzeye dayanıyordu.
>
> Uç **KBP-714 ile geldi** ve Database Checker `0.2.0-alpha.6` ile public oldu
> (`GET .../schema-discovery/{connectionId}/fingerprint`,
> [[01-Current/Checker-Packages-Truth|CURRENT-0002]]). Test Module tarafında
> `SchemaKnowledgeAppService.GetSchemaFingerprintAsync` bunu köprüye taşır ve
> `TestScenarioAppService` ile `ScenarioCompilationService` malzeme mührünü buradan doldurur.
>
> Yani **önlem artık gerçekten vardır**; satır bugün geçerlidir. Bu bir **kayıt düzeltmesidir**,
> karar değişikliği değildir: §A–§D'nin kararları aynen durmaktadır.
