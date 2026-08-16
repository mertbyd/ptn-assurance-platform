---
id: ADR-0014
type: decision
status: accepted
title: Senaryo yazarlik modeli — ajan sorar, uydurmaz; turetilemeyen assertion yayinlanamaz
created: 2026-08-13
updated: 2026-08-13
owners:
  - mertbyd
supersedes:
superseded_by: null
decision_refs:
  - ADR-0007
  - ADR-0008
rule_refs:
  - RULE-0005
  - RULE-0006
---

# ADR-0014 — Senaryo yazarlık modeli ve türetilebilirlik kapısı

> Silinen ADR-0011'in **ajan sınırları** bölümünü yerine geçirir ve somutlaştırır.
> Veri modeli için ADR-0016, koşum ve modül entegrasyonu için ADR-0015 geçerlidir.
> Uygulama mekaniği **ADR-0017**'dedir.

## Bağlam

Ürünün girişi iki dosyadır: `senaryo.md` (insanın anlattığı iş akışı) ve `kurallar.md`
(iş kuralları). Çıktısı çalıştırılabilir bir Arazzo dokümanıdır. Aradaki dönüşümü yapay zekâ
yapar.

İki ölçüm bu dönüşümün nasıl kurulacağını belirliyor
([[90-Inbox/RESEARCH-0013-Runner-Oracle-Ayrimi-Ve-Ajan-Yazarlik-Kaniti|RESEARCH-0013]] §3):

- **B7:** mevcut koddan üretilen testler uygulamanın davranışını doğrulamaya optimize olur,
  niyeti değil; spesifikasyondan üretilenler anlamlı ölçüde daha fazla hata yakalar.
- **B8:** ajan geri bildiriminin %70-77'si `print`, assertion değil; assertion'ların yalnız
  %3-8'i ilişkisel/aralık kontrolüdür.

Bu iki bulgu birlikte şunu söyler: **ajanı serbest bırakırsan çalışan ama hiçbir şey
doğrulamayan test üretir.**

## Karar

### A. Ajanın yazım anındaki tek girdileri

Ajan **çalışan sistemin davranışını görmez.** Girdileri üç tanedir:

| Girdi | Kaynak | Ne verir |
|---|---|---|
| `kurallar.md` | MCP `Resource` — sürüm kontrolündeki dosya | Niyet: iş kuralları, sınır değerler |
| OpenAPI snapshot | API Contract Checker | Sözleşme: operasyonlar, şemalar |
| DB şeması | Database Checker | Yapı: tablo, kolon, anahtar |

`kurallar.md` **veritabanı tablosu değildir.** MCP `Resource` primitive'i salt-okunur bağlam
için tasarlanmıştır; kural dokümanı Git'te durur, koşu satırında yalnız `rules_fingerprint`
tutulur.

### B. Ajan uydurmaz, sorar

Yazarlık tool'ları mevcut checker AppService'lerine bağlanır. Yeni motor yazılmaz:

| Soru | Yüzey |
|---|---|
| Bu iş adımı hangi operasyona düşüyor? | `SuggestOperationBindingsAsync` |
| Geçerli istek gövdesi nedir? | `BuildRequestExampleAsync` |
| Bu assertion sözleşmeden türetilebilir mi? | `ValidateScenarioAssertionsAsync` |
| Hangi tablo/kolon var, anahtar PK/unique mi? | `DescribeTableAsync` |
| Hedef şemanın fotoğrafı | `GetSnapshotAsync` |

Ajan 40 sayfa dokümanı bağlamına almaz; **sorgular**.

### C. Çıktı formatı: Arazzo + `x-checknexus-db`

> [!IMPORTANT] Sürüm düzeltmesi (2026-08-14, AUDIT-0002 / BULGU-07)
> Hedef sürüm **`1.0.1`**'dir, 1.1 değil. Redocly CLI README'si "Arazzo 1.0" diyor,
> `generate-arazzo` `arazzo: 1.0.1` üretiyor ve **`respect`'in 1.1 belgesi koştuğu
> doğrulanamadı**. Bugün ihtiyacımız olan her şey 1.0'da var: `sourceDescriptions` (zorunlu),
> `x-` uzantıları, `successCriteria` tipleri, `onSuccess`/`onFailure`/`goto`.
> 1.1 yalnız **async adım** (`channelPath`/`action`/`correlationId`) için gerekli ve o zaten
> ertelenmiş. `respect` bir 1.1 belgesini gerçek koşumla kabul ettiğinde bu karar
> yeniden açılır.

Kendi DSL'imiz yoktur. Model formatı zaten bilir ve OpenAPI Initiative standardıdır.

Veritabanı doğrulaması `x-checknexus-db` uzantısıyla yazılır (Arazzo spec'i Step Object'te
`x-` uzantısına açıkça izin verir). **Yayın anında Test Module bunu gerçek bir Arazzo adımına
derler:** Database Checker'ın `POST /assertions/row` endpoint'ine giden sıradan bir HTTP adımı
(ADR-0015 §C).

Bu yüzden iki belge saklanır:

| Belge | Rolü |
|---|---|
| `source_document` | Ajanın yazdığı, insanın **incelediği ve onayladığı** |
| `compiled_document` | Runner'ın **koştuğu** |

Onay `source_hash`'e bağlanır. Derleyici yarın değişirse eski koşu yeniden üretilebilir kalır.

### D. Dört kapı — ajanın durduğu yer

Sırayla:

1. **Şema geçerliliği** — `redocly lint` (Arazzo desteği aynı CLI'da)
2. **Türetilebilirlik** — `ValidateScenarioAssertionsAsync`, her assertion için
   `{jsonPointer, outcomeCode}`. Türetilemeyen assertion **yayınlanamaz** (RULE-0006)
3. **Zayıflama kapısı** — `assertion_count = 0` olan adım yayınlanamaz; assertion azaltan
   veya matcher gevşeten değişiklik ayrıca işaretlenir
4. **`dryRun` + insan onayı** — tek sefer koşar, `is_dry_run = true`, sağlık hesabına girmez.
   `Draft → PendingApproval → Published`

**Yayınlama kademe 4'tür: hiçbir otonomi seviyesinde otomatikleşmez.** `Published` durumuna
yazan tool ajanın kataloğunda **yoktur**.

### E. Düzeltme yönü

İteratif düzeltme geçerli test oranını %24'ten %70+'a çıkarıyor (B9). Ama yön kritiktir:

- **Sözleşmeye karşı düzeltme serbesttir** — kapı 1, 2, 3'ün geri bildirimi ajana verilir.
- **Gözlenen davranışa karşı düzeltme yasaktır** — `dryRun` kırmızıysa ajana sonuç
  verilmez, **çelişki bildirimi** döner; kararı insan verir (RULE-0005).

Bu çizgi bizi B7'nin ölçtüğü kör noktanın dışında tutan tek şeydir.

### F. Ölçüm yükümlülüğü

`test_scenarios.authored_by_agent` ve `agent_model_ref` kolonları zorunludur. Model başına
`derivability_code` dağılımı ve onay kabul oranı bu iki kolondan çıkar. B8 ölçülmeden
"ajan işe yarıyor" denemez.

## Alternatifler

- **Ajanı çalışan sisteme bakarak yazdırmak:** B7'nin ölçtüğü kör nokta; test uygulamanın
  hatasını doğru kabul eder.
- **`dryRun` sonucunu ajana geri beslemek:** ölçülmüş şekilde oracle'ı bozar; ajan
  assertion'ı hataya uyacak şekilde gevşetir.
- **`kurallar.md` için ayrı tablo (`business_knowledge`):** MCP `Resource` ve Git zaten
  sürümleme, erişim ve salt-okunurluk veriyor. Tablo ölçülmüş bir ihtiyaç değil.
- **Kendi senaryo DSL'imiz:** model Arazzo'yu zaten biliyor; DSL öğretmenin token maliyeti
  ve bakım yükü karşılıksız.
- **Türetilebilirlik kapısını uyarıya indirmek:** B8'in ölçtüğü zayıf assertion doğrudan
  üretime geçer.

## Sonuçlar ve riskler

Ajan An 1-2-3-4'te vardır; **An 5 ve An 6'da hiç yoktur.**

| Risk | Önlem |
|---|---|
| Ajan türetilebilir ama anlamsız assertion yazar | `rule_ref` zorunluluğu ile kapsam raporu: *"BR-015 için hiç bulgu üreten senaryo yok"* |
| Kural dokümanı belirsizse çıktı kalitesi düşer | `rules_fingerprint` ile hangi kural sürümüne karşı yazıldığı kayıtlı; ölçüm belirsiz gereksinimde 26-40 puan düşüş gösteriyor |
| Model değişimi kaliteyi sessizce değiştirir | `agent_model_ref` ile model başına kabul oranı karşılaştırılır |
| İnsan onayı darboğaz olur | Kapı 1-2-3 makine; insana yalnız kapı 4 kalır |
