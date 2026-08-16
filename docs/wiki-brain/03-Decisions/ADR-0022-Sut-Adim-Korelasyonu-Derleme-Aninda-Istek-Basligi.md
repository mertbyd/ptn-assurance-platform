---
id: ADR-0022
type: decision
status: accepted
title: SUT adim korelasyonu — derleme aninda enjekte edilen istek basligi
created: 2026-08-15
updated: 2026-08-15
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs:
  - ADR-0007
  - ADR-0015
  - ADR-0020
  - ADR-0021
rule_refs:
  - RULE-0007
---

# ADR-0022 — SUT adım korelasyonu

> Dayanak: KBP-103 Dilim 3 blokajı (2026-08-15). [[03-Decisions/ADR-0021-Checker-Korelasyon-Kimligi|ADR-0021]]'i
> **değiştirmez**; kapsamadığı bir boşluğu doldurur. Checker sözleşmelerine dokunmaz.

## Bağlam

ADR-0021 checker çağrılarının korelasyonunu çözdü: her public giriş DTO'su opsiyonel
`CorrelationRef` taşır, her sonuç DTO'su aynen geri yansıtır, echo HAR gövdesine düşer.
Kapsam bölümü **yalnız checker DTO'larını** sayar.

**Sıradan SUT adımları bu mekanizmanın dışında kaldı.** Bir Arazzo senaryosunun adımlarının
çoğu test edilen sisteme gider — üçüncü taraf bir sisteme. O sistem `stepKey` echo etmez ve
etmesi de **istenemez**; ürünün çalışma modeli SUT'tan hiçbir şey talep edemeyeceğimiz
varsayımına dayanır.

Kod bu boşluğu fazla katı bir kuralla doldurmuştu.
`Domain/Managers/Runs/HarInterpreter.cs` `stepKey`'i **yalnız yanıt gövdesinden** okur:

```csharp
StepKey = ResolveStepKey(ReadStepKey(responseBody), declaredStepKeys)
```

Sonuç: SUT'a giden her başarılı `200` yanıt `StepKey = null` kalıyor,
`HasUnboundEntries = true` oluyor ve hüküm `Inconclusive`'e düşüyor. KBP-103 Dilim 3
tam olarak burada durdu.

Konum veya sıra tabanlı eşleme ADR-0021'in reddettiği şeydir (§C batch gerekçesi:
*"sunucu bir öğe düşürür veya sırayı değiştirirse A'nın sonucu B'ye yazılır — sessizce"*).
O yola dönülmez.

## Karar

### A. Korelasyon **yanıttan değil istekten** çözülür

Derlenmiş Arazzo belgesindeki **her adım**, derleme anında bir header parametresi taşır:

```
X-CheckNexus-Step-Key: <stepKey>
```

Bu, Arazzo'nun kendi `parameters` mekanizmasıdır (`in: header`) — **yeni uzantı, yeni DSL
veya runner değişikliği yoktur**. ADR-0015 §A'nın *"kendi parser'ımızı yazmıyoruz"* sınırı
korunur; belge standart Arazzo `1.0.1` olmaya devam eder.

Enjeksiyonun sahibi `Domain/Managers/Compilation/ArazzoCompilerManager`'dır (KBP-100).
`compiled_document` zaten **makine türevi**dir; bu alan da öyle olur.

### B. `HarInterpreter` önce isteği, sonra yanıtı okur

Çözüm sırası:

1. HAR entry'sinin **istek header'ındaki** `X-CheckNexus-Step-Key`;
2. bulunamazsa yanıt gövdesindeki ADR-0021 echo'su (checker adımları için);
3. ikisi de yoksa `StepKey = null` ve entry **bağlanmamış** sayılır.

HAR 1.2 `entry.request.headers` dizisini **zorunlu** tutar; okuma garanti altındadır.

Konum tabanlı eşleme **hiçbir adımda** kullanılmaz. `Ordinal` yalnız raporlama alanıdır
ve öyle kalır.

### C. Checker adımlarında iki kaynak birbirini doğrular

Checker'a giden adım hem enjekte edilmiş istek header'ını hem ADR-0021 echo'sunu taşır.
İkisi **çelişirse** entry bağlanmamış sayılır ve bulgu raporlanır — sessizce birine
güvenilmez. Bu, ADR-0021 §C'nin çift kapı mantığının HAR yolundaki karşılığıdır.

### D. Bildirilmemiş anahtar kabul edilmez

Mevcut `ResolveStepKey` davranışı korunur: çözülen anahtar `WorkflowDocumentFacts.StepKeys`
kümesinde **bildirilmiş** olmak zorundadır. Header enjeksiyonu bu kapıyı gevşetmez.

### E. Sabit sahipliği

Header adı ve uzunluk sınırı `Domain.Shared` sabitidir; inline string yazılmaz.
`StepKey` uzunluk sınırı ADR-0021 §F ile **aynı kaynaktan** beslenir (≤128 karakter).

## Alternatifler

- **SUT'a echo sözleşmesi dayatmak.** Kendi host'umuzda mümkün, üçüncü taraf SUT'ta imkânsız.
  Ürünün *"herhangi bir sistemi test edebilirim"* iddiasını kırar. **Reddedildi.**
- **ADR-0021'i yeniden yazmak.** ADR-0021 bu konuyu hiç kapsamıyor; değiştirilecek bir karar
  yok. Kayıt geçmişini bulanıklaştırır. **Reddedildi** (ADR-0001 yönetişimi: değişen karar
  yeni ADR ister, değişmeyen karar dokunulmaz kalır).
- **Konum/sıra tabanlı eşleme.** ADR-0021 §C'nin adıyla reddettiği şey. **Reddedildi.**
- **`traceparent` başlığı.** ADR-0021'in kendi alternatifler bölümünde reddedildi: batch
  içinde adım ayrımı yapmaz. Burada da adım seviyesi gerekiyor. **Reddedildi.**
- **Query string'e `stepKey` eklemek.** SUT'un rotasını ve imzasını kirletir; bazı sistemler
  bilinmeyen parametreyi reddeder; önbellek anahtarını bozar. **Reddedildi.**

## Sonuçlar ve riskler

Değişim yüzeyi dardır: `ArazzoCompilerManager`'da enjeksiyon, `HarInterpreter`'da okuma sırası,
bir `Domain.Shared` sabiti, testler. **Yeni tablo, yeni katman, yeni paket, checker değişikliği
yok.**

| Risk | Önlem |
|---|---|
| **`compiled_hash` değişir** — enjeksiyon belgeyi değiştirir, yayımlanmış senaryoların `approval_bound_to_hash` bağı kopar (KBP-92 / TM-19) | Bilinçli kabul. Bugün üretimde yayımlanmış senaryo **yok**; maliyet sıfır. Kural doğru çalışıyor demektir — belge değişti, onay yenilenir |
| Proxy veya gateway bilinmeyen header'ı düşürür | Entry bağlanmamış kalır ve `HasUnboundEntries` bunu **görünür** yapar; sessiz yanlış eşleme olmaz |
| İki kaynak çelişir | §C — entry bağlanmamış sayılır ve raporlanır |
| SUT header'ı loglar, gizli veri sızar | `stepKey` senaryo yazarının verdiği kimliktir, sır değildir; değer saklama politikası kapsamı dışında |
| Enjeksiyon determinizmi bozulur | `stepKey` derleme girdisinden gelir; aynı girdi → aynı `compiled_hash` invariant'ı (KBP-100 kabul ölçütü) testle korunur |

## Uygulama

`KBP-107` Dilim 3. Kabul: elle yazılmış bir Arazzo senaryosunun **her** entry'si adım
kimliğiyle bağlanıyor, `HasUnboundEntries = false`, hüküm `Inconclusive` değil.
