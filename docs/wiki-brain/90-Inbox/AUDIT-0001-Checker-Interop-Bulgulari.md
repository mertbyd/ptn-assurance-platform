---
id: AUDIT-0001
type: audit
status: open
title: Wiki-kod denetimi — tur 1: iki checker arasindaki bag
created: 2026-08-14
updated: 2026-08-14
decision_refs:
  - ADR-0006
  - ADR-0007
  - ADR-0015
  - ADR-0016
  - ADR-0018
  - ADR-0019
  - ADR-0020
rule_refs:
  - RULE-0006
---

# AUDIT-0001 — Wiki iddiaları koda karşı: tur 1

> **Yöntem:** wiki'nin kod seviyesinde **doğrulanabilir** her iddiası alınır, iki checker'ın
> gerçek `Application.Contracts` yüzeyine karşı test edilir. Tahmin yok; her bulgunun kod
> kanıtı vardır.
>
> **Bu tur:** iki checker arasındaki bağ ekseni. Kalan eksenler §7'de.

---

## 0. Yapısal kök: paylaşılan sözleşme paketi yok

```
api-contract/src/CheckNexus.ApiContracts/          → yalnız ApiContractCheckerModule.cs
database-comparison/src/CheckNexus.DatabaseComparison/ → yalnız DatabaseCheckerModule.cs
```

İki paket **birbirini referanslamıyor** ve **ortak bir soyutlama paketi yok**. Yani iki
checker'ın paylaşacağı bir tip için **yer yoktur**. Aşağıdaki bulguların çoğu bunun sonucudur.

Bu bilinçli bir karar olabilir (ADR-0006 paket kimliği, ADR-0007 bağımsızlık) — ama **karar
olarak yazılmamış**. Kayıt olmadığı için her yeni ihtiyaçta yeniden tartışılıyor.

---

## BULGU-01 — Korelasyon kimliği yok · **yüksek**

**Wiki iddiası.** `ARCH-0004` ve `ADR-0016`: *"Köprü `test_runs.trace_id`'dir (W3C, 32 hex)."*
`ADR-0018 §C`: köprü kanıtı **alanlar arasında** ilişkilendirir.

**Gerçek.** İki checker'ın **hiçbir public giriş DTO'su** çağıranın verdiği bir korelasyon
kimliği kabul etmiyor:

| DTO | Korelasyon alanı |
|---|---|
| `RowAssertionRequestDto` | **yok** |
| `ResponseConformanceDto` | **yok** |
| `DiagnoseRequestDto` (api) | yalnız `ContractCheckRunId` — **checker'ın kendi koşusu** |
| `DiagnoseRequestDto` (db) | yalnız `ConnectionId` |

Sonuç DTO'ları da hiçbir anahtar **geri yansıtmıyor** (`RowAssertionResultDto`:
`OutcomeCode`, `Passed`, `ObservedRowCount`, `ObservedAtMs`, `AttemptCount`,
`FailedExpectations`, `RowSummary`).

**Etki.** İki yerde kırılıyor:

1. **Kanıt zinciri.** Bir API teşhisi ile bir DB gözlemi, yalnız köprünün **o anki bellek
   içi defteri** sayesinde eşleşiyor. Kalıcı bir bağ yok; rapor sonradan yeniden
   ilişkilendirilemez.
2. **Runner'ın koştuğu DB adımı.** ADR-0015 §C uyarınca DB assertion'ı **dış runner**
   sıradan bir HTTP adımı olarak çağırıyor. Yanıt HAR'a düşüyor. Test Module o HAR girdisini
   senaryo adımına **konumla** (sıra/ad) bağlamak zorunda — çünkü istekte de yanıtta da
   taşınan bir adım kimliği yok. **`trace_id` köprüsü bu yolda çalışmıyor:** runner bizim
   trace bağlamımızı bilmiyor.

**Düzeltme.** İki seçenek, ikisi de checker işi (PLAN-0001 / PLAN-0002):

- **A (tercih):** dört giriş DTO'suna opsiyonel `CorrelationRef { TraceId, StepKey }` eklenir
  ve **sonuç DTO'sunda aynen geri yansıtılır**. Checker onu yorumlamaz, taşır.
- **B:** Test Module derleme anında Arazzo adımına `traceparent` başlığı gömer; checker
  başlığı okur ve rapora yazar. A'dan zayıf: batch içinde adım ayrımı yapmaz.

**Kapanana kadar:** köprü HAR eşlemesini **adım adıyla** yapar ve bunun kırılgan olduğunu
raporda `Inconclusive` gerekçesi olarak taşır.

---

## BULGU-02 — Batch sonuçları indekse bağlı · **orta**

**Gerçek.** `AssertBatchAsync(List<RowAssertionRequestDto>) → List<RowAssertionResultDto>`.
İstek ile sonuç arasındaki tek bağ **liste indeksi**.

**Etki.** Sunucu bir öğeyi düşürür, sırayı değiştirir veya kısmi sonuç dönerse eşleşme
**sessizce kayar**: A adımının sonucu B adımına yazılır. Bu, yanlış teşhisin en sinsi türü —
deterministik motordan gelmiş gibi görünür.

**Düzeltme.** BULGU-01'in `CorrelationRef`'i batch öğesi seviyesinde de taşınır ve sonuçta
yansıtılır. Ara çözüm: köprü batch'i **tek öğeye indirger** (maliyetli ama güvenli) veya
sonuç sayısı ≠ istek sayısı ise **tamamını `Unavailable`** işaretler.

---

## BULGU-03 — DB assertion'ları için türetilebilirlik kapısı yok · **yüksek**

**Wiki iddiası.** `RULE-0006`: *"Her assertion türetilebilir. `ValidateScenarioAssertionsAsync`
her assertion için `{jsonPointer, outcomeCode}` döndürür; türetilemeyen tek assertion varsa
yayın reddedilir."*

**Gerçek.** `ValidateScenarioAssertionsAsync` **API Contract Checker'dadır** ve girdisi
JSON Pointer'dır — yani **yalnız HTTP yanıt gövdesi** assertion'larını kapsar.
Database Checker tarafında türetilebilirlik kavramı **hiç yok** (`Derivab*` geçen dosya: **0**).

**Etki.** `x-checknexus-db` ile yazılmış bir assertion (satır var mı, kolon değeri, cardinality)
**hiçbir kapıdan geçmeden** yayınlanabiliyor. RULE-0006 metni "her assertion" diyor ama
mekanizma yarısını kapsıyor. Bu, kuralın **yanlış güven** vermesi demek.

**Düzeltme.** İki parça:

1. **Kural metni düzeltilir:** RULE-0006 iki kapı tanımlar — *sözleşme türetilebilirliği*
   (API, `ValidateScenarioAssertionsAsync`) ve *şema türetilebilirliği* (DB).
2. **DB kapısı kurulur:** `DescribeTableAsync` zaten tablo/kolon/anahtar veriyor. Kapı şunu
   sorar: hedef tablo var mı · kolonlar var mı · anahtar **PK veya unique** mi · matcher
   kolon tipiyle uyumlu mu. Sonuç `{tableRef, columnRef, outcomeCode}` — API tarafıyla
   **aynı şekil**.

`ADR-0007` zaten `KeyNotUnique` outcome'ını tanımlıyor; eksik olan **yayın anında** sormak.

---

## BULGU-04 — Aynı RFC 9457 sözleşmesi, iki farklı tel formatı · **orta**

**Wiki iddiası.** `ARCH-0004`, `ADR-0018`: her iki checker **RFC 9457 raporu** üretir ve köprü
tek sözlükte sunar.

**Gerçek.**

| | `JsonPropertyName` sayısı |
|---|---|
| `Ptn.DatabaseChecker...DiagnosisReportDto` | **9** — `checknexus:identity`, `checknexus:hypotheses`, `checknexus:nextChecks` … |
| `Ptn.ApiContractChecker...DiagnosisReportDto` | **0** |

DB checker uzantı adlarını **açıkça** yazıyor; API checker serileştiricinin varsayılan
politikasına bırakıyor. Aynı kavram tel üzerinde **farklı adla** çıkıyor.

**Etki.** MCP/HTTP tüketicisi (ve köprünün adapter'ı) iki farklı şekle karşı yazmak zorunda.
Serileştirici politikası değişirse API tarafı **sessizce** kayar — sözleşme testi yok.

**Düzeltme.** API checker'ın DTO'suna aynı `checknexus:` adları eklenir **veya** ikisi de
Domain.Shared'da tanımlı sabit adlardan beslenir. Her hâlde **sözleşme testi** yazılır:
serileştirilmiş çıktının anahtar kümesi sabitle karşılaştırılır.

---

## BULGU-05 — `spec_snapshot_id` ↔ `db_connection_id` tutarlılığı doğrulanmıyor · **orta**

**Wiki iddiası.** `ADR-0016 §G`: ortam bağlaması ABP `Setting`; mantıksal ad →
`baseUrl` / `specSnapshotId` / `dbConnectionId` / `secretRef`.

**Gerçek.** Ayar ikisini **yan yana** koyuyor ama hiçbir yerde *"bu snapshot ile bu bağlantı
aynı çalışan sistemi mi tarif ediyor"* diye sorulmuyor. ADR-0020 ikisini senaryo satırında
**mühürlüyor** — ama mühür "aynı ortam" demiyor, "yazım anında bunlardı" diyor.

**Etki.** Yanlış eşleştirilmiş bir ortam ayarı (staging API + prod DB) **sessizce** koşar.
Assertion'lar tutarsız çıkar ve teşhis yanlış yeri gösterir. Bu, üretimde veri bozan
sınıftan bir hata.

**Düzeltme.** Ortam çözümünde bir kez, **bağlama kurulurken** doğrulama: snapshot'ın
`servers[]` girdisi ile bağlantının hedef adresi/veritabanı adı arasında **açık bir eşleşme
kaydı** aranır (ayarda `environmentKey` her ikisinde de aynı olmalı). Eşleşmiyorsa koşum
**başlamaz** — `Inconclusive` değil, **reddedilir**; çünkü bu bir yapılandırma hatasıdır,
bilgi eksikliği değil.

---

## BULGU-06 — Salt-okunur projeksiyon yüzeyi yok · **yüksek** · *kayıtlı*

`ADR-0019 §F`'de zaten karara bağlandı. Burada tekrar ediliyor çünkü BULGU-01 ve BULGU-03
ile aynı kök nedene bağlı: **Test Module'ün checker'a soru sorma yüzeyi dar.**

Bugün DB Checker yalnız *beklenti doğrular* ve *yapı anlatır*; *"bu satırların değerleri ne"*
sorusunun cevabı yok. Kanıt zinciri bu yüzden `Unavailable` dönüyor.

---

## 6. Özet ve sahiplik

| # | Bulgu | Ciddiyet | Sahip |
|---|---|---|---|
| 01 | Korelasyon kimliği yok | Yüksek | PLAN-0001 + PLAN-0002 (checker) |
| 02 | Batch indeks korelasyonu | Orta | PLAN-0001 (db checker) |
| 03 | DB assertion türetilebilirlik kapısı yok | Yüksek | RULE-0006 revizyonu + PLAN-0001 |
| 04 | İki farklı RFC 9457 tel formatı | Orta | PLAN-0002 (api checker) + sözleşme testi |
| 05 | Ortam eşleşmesi doğrulanmıyor | Orta | Numarası henüz atanmamış Test Module koşum task'ı |
| 06 | Projeksiyon yüzeyi yok | Yüksek | PLAN-0001 (kayıtlı, ADR-0019 §F) |

**Kök neden ikiye iniyor:**
**(a)** iki checker'ın paylaşacağı bir sözleşme yeri yok (§0);
**(b)** Test Module'ün checker'a **soru sorma** yüzeyi, **doğrulama** yüzeyinden dar.

---

## 7. Bu turda taranmayan eksenler

Denetim **tamamlanmadı**. Sıradaki turlar:

| Tur | Eksen | Ne aranacak |
|---|---|---|
| 2 | Auth ve secret | ADR-0012/0013 iddiaları ↔ host kompozisyonu; Vault yolu ve redaksiyon sınırı |
| 3 | Paketleme ve sürüm | ADR-0002/0003/0006 ↔ csproj/`common.props`; PackageValidation suppression'ları |
| 4 | Veri modeli | ADR-0016 ↔ `Test-Platform-Schema.dbml` ↔ üretilecek EF configuration'ları |
| 5 | Koşum sınırı | ADR-0015 iddiaları ↔ Respect'in gerçek CLI yüzeyi (sürüm pinleme, `--har-output` davranışı) |
| 6 | Yazarlık hattı | ADR-0017 ↔ DMN motoru ve `Microsoft.Extensions.AI` gerçekliği |
| 7 | Köprü kodu | ADR-0018/0019/0020 ↔ KBP-88/89 çıktısı |
