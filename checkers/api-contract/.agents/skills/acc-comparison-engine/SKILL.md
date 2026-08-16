---
name: acc-comparison-engine
description: Build and modify the OpenAPI comparison engine in ApiContractChecker — canonical model, identity matching, normalization, operation-surface and schema diffing, rename detection, and breaking-change classification by direction and severity. Use for any work on reading two specs and producing findings, adding a new difference rule, or adding support for a new OpenAPI version.
---

# Karşılaştırma motoru

Motor tek yönlü bir boru hattıdır. Adımların sırası **değişmez**; bir adımın işini
başka adımda yapmak yalancı fark üretir.

```text
oku -> kanonik modele indirge -> normalize et -> kimlik esle -> karsilastir -> siniflandir -> bulgu
```

| Adım | Sorumluluk | Nerede |
|---|---|---|
| oku | Ham metni `OpenApiDocument`'a çevir | `EntityFrameworkCore` format bileşeni |
| indirge | Sürüm farklarını (2.0 / 3.0 / 3.1) tek modele topla | `Domain/Models/Comparison` |
| normalize | Anlam taşımayan farkları **yok et** | `Domain/Managers/Comparison` |
| kimlik eşle | İki taraftaki "aynı nesne"yi bul | `Domain/Managers/Comparison` |
| karşılaştır | Alan alan fark üret | `Domain/Managers/Comparison` |
| sınıflandır | Yön + şiddet ata | `Domain/Managers/Comparison` |

## Pazarlıksız iki kural

- **Kimlik ve normalizasyon:** [kanonik checker kuralları](../../../../../docs/PTN-ASSURANCE-PLATFORM-WIKI.md#13-checker-gelistirme-kurallari)
- **Yön ve şiddet:** [kanonik checker kuralları](../../../../../docs/PTN-ASSURANCE-PLATFORM-WIKI.md#13-checker-gelistirme-kurallari)

Bu iki sayfayı **okumadan** motora dokunma. Buradaki prosedür onları tekrar etmez.

## Motorun kalbi: asimetri

Aynı değişiklik iki tarafta zıt sonuç verir. Kod yazarken her karşılaştırma
metodunun adında yön geçmelidir — `CompareRequestProperty`, `CompareResponseProperty`
gibi. Yön parametresi alan tek bir "generic" metod yazma: iki taraf **farklı
kurallara** tabidir, tek gövdede birleştirmek kuralları karıştırır.

| Değişiklik | İstek tarafı | Yanıt tarafı |
|---|---|---|
| Alan zorunlu oldu | **Breaking** | NonBreaking |
| Alan opsiyonel oldu | NonBreaking | **Breaking** |
| Nullable kaldırıldı | **Breaking** | NonBreaking |
| Nullable eklendi | NonBreaking | **Breaking** |
| Enum değeri silindi | **Breaking** | NonBreaking |
| Enum değeri eklendi | NonBreaking | **Breaking** (istemci bilinmeyen değeri karşılamaz) |
| Tip değişti | **Breaking** | **Breaking** |

`default` değeri olması, alanın zorunlu olmasını **kurtarmaz** — varsayılan sunucu
tarafı bir yedektir, atlanmış alanı geçerli kılmaz.

## Yeni fark kuralı eklerken

1. `oasdiff` kataloğunda karşılığı var mı bak — varsa **aynı kod adını** kullan
   (`new-required-request-property` gibi). Yoksa yeni kodu RULE-0007'ye gerekçesiyle yaz.
2. Kodu `DifferenceKind` lookup'ına seed et
   ([`../acc-lookup-recipe/SKILL.md`](../acc-lookup-recipe/SKILL.md)).
3. **Pozitif ve negatif** birer birim testi yaz. Negatif test, kuralın yanlışlıkla
   tetiklenmediğini kanıtlar; asıl değerli olan odur.
4. CI oracle karşılaştırmasını çalıştır; sapmayı açıkla veya düzelt.

## Performans sınırları

- Karşılaştırma **bellek içi**dir; hiçbir adımda repository çağrısı yoktur.
- İki tarafı da tek geçişte sözlüğe indeksle; iç içe `foreach` ile karşı tarafta
  arama yapma (O(n²)).
- Normalizasyon **bir kez** çalışır ve sonucu taşınır; her karşılaştırma metodunda
  tekrar normalize etme.
- Uzun iş UOW tutmaz — [kanonik API Contract Checker gerçeği](../../../../../docs/PTN-ASSURANCE-PLATFORM-WIKI.md#7-api-contract-checker-gercegi).

## Referanslar

| Ne zaman | Aç |
|---|---|
| `Microsoft.OpenApi` API'siyle çalışırken | [`references/microsoft-openapi-gotchas.md`](references/microsoft-openapi-gotchas.md) |
| Yeni format / motor bileşeni eklerken | [`references/engine-component-recipe.md`](references/engine-component-recipe.md) |
| Paket sürümü, kaynak taraf gerçeği | [Kanonik wiki](../../../../../docs/PTN-ASSURANCE-PLATFORM-WIKI.md#7-api-contract-checker-gercegi) |
