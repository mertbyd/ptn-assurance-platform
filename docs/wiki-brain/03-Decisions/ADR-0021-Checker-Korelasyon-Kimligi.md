---
id: ADR-0021
type: decision
status: accepted
title: Checker korelasyon kimligi — cagiran anahtari tasinir ve geri yansitilir
created: 2026-08-14
updated: 2026-08-14
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs:
  - ADR-0007
  - ADR-0015
  - ADR-0016
  - ADR-0018
rule_refs: []
---

# ADR-0021 — Checker korelasyon kimliği

> Dayanak: [[90-Inbox/AUDIT-0001-Checker-Interop-Bulgulari|AUDIT-0001]] BULGU-01, BULGU-02, BULGU-04.
> İki checker'ın **public sözleşmesini** değiştirir; Test Module'ün veri modelini değiştirmez.

## Bağlam

Wiki *"köprü `test_runs.trace_id`'dir"* diyor. Kod seviyesinde doğrulandı ki iki checker'ın
**hiçbir public giriş DTO'su** çağıranın verdiği bir kimliği kabul etmiyor ve **hiçbir sonuç
DTO'su** bir anahtar geri yansıtmıyor.

İki yerde kırılıyor:

1. **Runner'ın koştuğu DB adımı.** ADR-0015 §C uyarınca DB assertion'ı **dış runner** tarafından
   sıradan bir HTTP adımı olarak çağrılıyor; yanıt HAR'a düşüyor. Test Module o HAR girdisini
   senaryo adımına yalnız **konumla** bağlayabiliyor — çünkü ne istekte ne yanıtta adım kimliği
   var. Runner bizim trace bağlamımızı bilmiyor.
2. **Batch.** `AssertBatchAsync` istek↔sonuç bağı olarak **liste indeksi** kullanıyor. Sunucu
   bir öğe düşürür veya sırayı değiştirirse A'nın sonucu B'ye yazılır — **sessizce**.

## Karar

### A. Her public giriş DTO'su opsiyonel `CorrelationRef` taşır

```
CorrelationRef
  TraceId  : string?   W3C trace-id — 32 KUCUK HARF hex (16 bayt). Guid degil.
  StepKey  : string?   Cagiranin adim kimligi — <= 128 karakter.
```

**İkisi de opsiyoneldir.** Verilmezse davranış bugünküyle birebir aynıdır — geriye dönük
uyumluluk korunur.

### B. Her sonuç DTO'su aynı `CorrelationRef`'i **aynen geri yansıtır**

Checker onu **yorumlamaz, saklamaz, karara katmaz** — yalnız taşır. Bu, ADR-0007'nin
salt-okunur değişmezini bozmaz: yeni bir yazma yolu açılmıyor.

**Mekanizma neden işe yarıyor:** `RowAssertionResultDto` runner üzerinden HTTP yanıtı olarak
döner ve **HAR gövdesine** düşer. Echo edilen `StepKey` orada olduğu için Test Module HAR
girdisini senaryo adımına **kimlikle** bağlar, konumla değil.

### C. Batch korelasyonu öğe seviyesindedir

`AssertBatchAsync` her istek öğesinin `CorrelationRef`'ini karşılık gelen sonuç öğesinde
yansıtır. Ek olarak:

- Sonuç sayısı istek sayısına **eşit olmak zorundadır**; değilse checker hata döner.
- Çağıran taraf (köprü) eşitliği ayrıca doğrular; tutmuyorsa **tamamını `Unavailable`**
  işaretler — kısmi eşleşmeyle devam **etmez**.

### D. Tip her checker'da **ayrı ama birebir aynı** tanımlanır

İki checker'ın paylaşacağı bir sözleşme paketi **yoktur** (AUDIT-0001 §0) ve bu blokta
**açılmaz** — yeni paket, yeni sürümleme ve yeni bağımlılık yönü demek.

Bunun yerine her checker `CorrelationRefDto`'yu **kendi** `Application.Contracts`'ında tanımlar.
**Alan adları, tipleri, doğrulama kuralları ve JSON adları birebir aynı olmak zorundadır** —
köprünün adapter'ı ikisini tek modele 1:1 eşler.

Eşitlik **testle** korunur: her iki depoda, tipin alan kümesinin ve JSON adlarının beklenen
kümeyle karşılaştırıldığı bir sözleşme testi bulunur.

### E. Teşhis raporu tel formatı hizalanır

AUDIT-0001 BULGU-04: DB checker `DiagnosisReportDto`'da **9** `JsonPropertyName`
(`checknexus:identity`, `checknexus:hypotheses`, …) taşırken API checker'da **0** var.
Aynı kavram tel üzerinde farklı adla çıkıyor.

**API Contract Checker'ın `DiagnosisReportDto`'su aynı `checknexus:` adlarını alır.** Her iki
depoda serileştirilmiş çıktının anahtar kümesini sabitle karşılaştıran **sözleşme testi**
yazılır; serileştirici politikası değişirse test kırmızı olur.

### F. Doğrulama kuralları

| Alan | Kural |
|---|---|
| `TraceId` | Verilmişse **tam 32 karakter, `[0-9a-f]` küçük harf hex**. Aksi hâlde validation hatası |
| `StepKey` | Verilmişse `1..128` karakter, boş/whitespace olamaz |
| İkisi de | Opsiyonel; ikisi de yoksa `CorrelationRef` hiç gönderilmemiş sayılır |

Uzunluk sınırları **`Domain.Shared` sabitidir** ve validator ile aynı kaynaktan beslenir.

## Kapsam — hangi yüzeyler

**API Contract Checker (`KBP-628`):**
`ResponseConformanceDto`, `RequestConformanceDto`, `DiagnoseRequestDto` → giriş;
`ConformanceResultDto`, `DiagnosisReportDto` → echo. Ek olarak §E hizalaması.

**Database Checker (`KBP-711`):**
`RowAssertionRequestDto`, `DiagnoseRequestDto` → giriş;
`RowAssertionResultDto`, `DiagnosisReportDto` → echo. Ek olarak §C batch kuralı.

## Alternatifler

- **Ortak sözleşme paketi açmak:** yeni paket kimliği, yeni sürümleme, iki depo arasında yeni
  bağımlılık yönü. Tek bir küçük tip için orantısız (ADR-0006 paket kimliği disiplini).
- **`traceparent` HTTP başlığıyla taşımak:** batch içinde **adım ayrımı yapmaz** ve runner'ın
  başlığı iletmesini şart koşar. §A/§B'nin çözdüğü asıl problemi çözmez.
- **Korelasyonu köprünün belleğinde tutmak (bugünkü hâl):** rapor sonradan yeniden
  ilişkilendirilemez; HAR yolunda hiç çalışmaz.
- **Zorunlu alan yapmak:** mevcut tüketicileri kırar; opsiyonel + echo aynı faydayı verir.
- **Checker'ın `CorrelationRef`'i saklaması:** ADR-0007 salt-okunur değişmezine dokunur ve
  hiçbir soruyu cevaplamaz.

## Sonuçlar ve riskler

Her iki checker'da **bir yeni DTO + beş DTO'ya bir alan + echo + validator + sözleşme testi**.
Yeni tablo, yeni katman, yeni paket **yok**. Test Module tarafında değişiklik **yok** —
köprü alanı zaten `PtnFindingRef`/`PtnEvidence` üzerinden taşıyor.

| Risk | Önlem |
|---|---|
| İki tanım zamanla ayrışır | §D sözleşme testi; alan kümesi ve JSON adları sabitle karşılaştırılır |
| Echo unutulur, sessizce `null` döner | Sonuç DTO'su testinde echo doğrulanır |
| Batch sayı eşitsizliği fark edilmez | Checker hata döner **ve** köprü ayrıca doğrular (çift kapı) |
| `TraceId` yanlış formatta gelir | Validator 32 küçük harf hex şartını uygular |
| Geriye dönük uyumluluk | Alan opsiyonel; verilmezse davranış birebir aynı |
