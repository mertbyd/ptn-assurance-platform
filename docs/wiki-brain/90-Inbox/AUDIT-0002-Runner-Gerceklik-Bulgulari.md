---
id: AUDIT-0002
type: audit
status: open
title: Wiki-arac denetimi — tur 2: Redocly Respect gercekligi
created: 2026-08-14
updated: 2026-08-14
decision_refs:
  - ADR-0014
  - ADR-0015
  - ADR-0018
rule_refs: []
---

# AUDIT-0002 — Runner gerçekliği: ADR-0015 iddiaları araca karşı

> **Neden bu eksen:** ADR-0015'in tamamı Redocly Respect'in davranışına dayanıyor ve araç
> **hiç doğrulanmamıştı**. Yanlışsa koşum katmanı baştan yazılır.
>
> **Yöntem:** ADR-0015 ve ADR-0014'ün araç hakkındaki her iddiası, Redocly'nin resmî
> dokümantasyonuna ve deposuna karşı tek tek test edildi.

---

## 1. Doğrulanan iddialar ✅

| Wiki iddiası | Sonuç |
|---|---|
| Lisans **MIT** | ✅ GitHub API: `spdx_id = MIT` |
| Docker imajı `redocly/cli` | ✅ `docker run --rm -v $PWD:/spec redocly/cli lint openapi.yaml` |
| Arazzo lint **aynı CLI'da** | ✅ `lint` komutu Arazzo dosyalarını doğruluyor |
| `--har-output` (HAR 1.2) | ✅ *"Path for the `har` file for saving logs"* |
| `--json-output` | ✅ |
| `--execution-timeout` | ✅ varsayılan **1 saat**, ms cinsinden |
| `--max-fetch-timeout` | ✅ varsayılan **40 saniye**, ms cinsinden |
| `--no-secrets-masking` **asla açılmaz** | ✅ Bayrak var; varsayılan maskeleme `format: password` alanlarını **ve** `x-security` token/auth başlıklarını `********` yapıyor — terminal **ve** dosya çıktısında |
| Dört kontrol adı | ✅ `STATUS_CODE_CHECK`, `SCHEMA_CHECK`, `SUCCESS_CRITERIA_CHECK`, `CONTENT_TYPE_CHECK` |
| Severity değerleri | ✅ `error` / `warn` / `off` |

**ADR-0015'in ana kararı — "runner dışarıda, MIT, HAR üretiyor" — sağlam.** Aşağıdaki
bulgular mekanizma ayrıntılarındadır, kararın kendisinde değil.

---

## BULGU-07 — Arazzo sürüm uyuşmazlığı · **yüksek** · *karar gerektirir*

**Wiki iddiası.** `ADR-0014 §C`: *"Çıktı **Arazzo 1.1** dokümanıdır."* `ARCH-0004` An 3 aynı.
`ADR-0015 §A`: *"Arazzo 1.1 `channelPath`/`action`/`correlationId` ile async adımı zaten
tanımlamıştır."*

**Gerçek — üç ayrı kaynak, üç farklı ayrıntı:**

| Kaynak | Ne diyor |
|---|---|
| Redocly CLI README (resmî) | *"Supports OpenAPI 3.2, 3.1, 3.0 … AsyncAPI 3.0 and 2.6, **Arazzo 1.0**."* |
| Changelog | `lint` komutuna **Arazzo 1.1.0 sözdizimi doğrulama** desteği eklendi |
| `generate-arazzo` | Ürettiği dosyalarda `arazzo: **1.0.1**` |

Yani: **lint 1.1'i tanıyor, üretici 1.0.1 yazıyor, `respect`'in 1.1 belgesini koştuğu
doğrulanamıyor.** README hâlâ "Arazzo 1.0" diyor.

**Etki.** Ajanın ürettiği belge `arazzo: 1.1.0` başlığıyla çıkarsa, koşum katmanı
**doğrulanmamış bir varsayıma** dayanır. Lint geçse bile `respect` reddedebilir; bu ancak
ilk gerçek koşumda anlaşılır.

**Öneri.** Hedef sürüm **`1.0.1`**'e çekilsin:

- Redocly'nin **kendi üreticisi** 1.0.1 yazıyor — araçla en uyumlu taban budur.
- Bugün 1.1'e ihtiyacımız **yok**: `sourceDescriptions` (zorunlu), `x-` uzantıları,
  `successCriteria` tipleri (`simple`/`regex`/`jsonpath`), `onSuccess`/`onFailure`/`goto`
  hepsi 1.0'da var.
- 1.1'e ihtiyaç duyan tek şey **async adım** (`channelPath`/`action`/`correlationId`) ve o
  zaten ADR-0015'te *"ölçülmüş ihtiyaç doğarsa ikinci adapter"* olarak ertelenmiş.

**Kapatma şartı:** `respect` bir `arazzo: 1.1.0` belgesini kabul ediyor mu — **gerçek koşumla**
ölçülür. Ölçülene kadar üretim `1.0.1`'dir. Karar ADR-0014 §C ve ARCH-0004'te güncellenmeli.

---

## BULGU-08 — Severity mekanizması yanlış adlandırılmış · **orta** · *düzeltildi*

**Wiki iddiası.** `ADR-0015 §E`: *"Respect'in kendi kontrolleri **`REDOCLY_CLI_RESPECT_SEVERITY`**
ile ayarlanır."*

**Gerçek.** Dokümantasyonda böyle bir ortam değişkeni **yok**. Severity **CLI bayrağıyla**
ve **JSON nesnesi** olarak veriliyor:

```bash
respect test.yaml --severity='{"STATUS_CODE_CHECK":"warn"}'
```

**Etki.** ADR-0015 §E'nin *"iki hakem çelişmesin"* mekanizması doğru ama **çağırma biçimi
yanlış yazılmış**. Bu hâliyle uygulanmaya çalışılırsa env değişkeni etkisiz kalır ve
`SCHEMA_CHECK` **`error`** olarak koşar — yani Respect, API Contract Checker'ın kayıt
sahibi olduğu hükmü **kendi başına** verir. ADR-0015 §E'nin engellemek istediği şeyin ta
kendisi olur.

**Ek risk.** Dokümantasyon **varsayılan severity'leri belirtmiyor**. Yani "varsayılan zaten
warn'dır" varsayımı yapılamaz; dört kontrolün severity'si **açıkça** verilmelidir.

**Düzeltme.** ADR-0015 §E güncellendi: mekanizma `--severity` JSON bayrağıdır ve dört
kontrolün severity'si **her koşumda açıkça** set edilir.

---

## BULGU-09 — Girdi mekanizması: iddia kısmen yanlış · **düşük** · *düzeltildi*

**Wiki iddiası.** `ADR-0015 §G`: *"Girdiler CLI bayrağıyla değil, env değişkeni veya
**girdi dosyasıyla** verilir; secret process listesinde görünmez."*

**Gerçek.** Üç yol dokümante:

1. `--input userEmail=name@redocly.com --input userPassword=12345` (CLI bayrağı)
2. `REDOCLY_CLI_RESPECT_INPUT='userEmail=...,userPassword=...'` (**ortam değişkeni — var** ✅)
3. İç içe değer biçimi (her iki yolda da)

**"Girdi dosyası" yolu dokümante değil.**

**Etki.** Güvenlik kuralının **özü doğru ve uygulanabilir**: env değişkeni yolu gerçekten var,
bu yüzden secret'ı CLI bayrağına koymaktan kaçınabiliyoruz. Yalnız "girdi dosyası"
alternatifi kayıtta yanlış duruyor ve uygulayıcıyı olmayan bir yola sokar.

**Düzeltme.** ADR-0015 §G güncellendi: tek desteklenen güvenli yol
**`REDOCLY_CLI_RESPECT_INPUT`** ortam değişkenidir.

---

## BULGU-10 — Maskeleme kapsamı kayıtta eksik · **düşük**

Varsayılan maskeleme yalnız "token"ları değil, şu ikisini kapsıyor:
**(a)** OpenAPI'de `format: password` ile tanımlı **her değer**,
**(b)** `x-security` içindeki token ve authentication başlıkları.

Ve maskeleme **hem terminal logunda hem dosya çıktısında** uygulanıyor — yani **HAR
artefaktımız da maskeli**.

**Etki — olumlu ama kayıtlı olmalı:** ADR-0016 §I *"token, parola, connection string
tutulmaz"* diyor; runner bunu **kendi tarafında** zaten sağlıyor. Ama bu, redaksiyonun
tek savunma hattı **olmadığı** anlamına gelmez: maskeleme `format: password` bildirimi olan
alanlar için çalışır; spec'te bildirilmemiş bir secret alanı **maskelenmez**. Bizim ACL
redaksiyonumuz (ADR-0016 §I) yerinde kalır.

---

## 2. Özet

| # | Bulgu | Ciddiyet | Durum |
|---|---|---|---|
| 07 | Arazzo 1.1 hedefi doğrulanmamış; araç 1.0/1.0.1 | Yüksek | **Karar bekliyor** |
| 08 | `REDOCLY_CLI_RESPECT_SEVERITY` diye bir şey yok | Orta | ADR-0015 §E düzeltildi |
| 09 | "Girdi dosyası" yolu yok; env değişkeni var | Düşük | ADR-0015 §G düzeltildi |
| 10 | Maskeleme kapsamı kayıtta eksik | Düşük | Bu belgede kayıtlı |

**ADR-0015'in ana kararı ayakta.** Düzeltilenler mekanizma ayrıntıları; kalan tek gerçek
risk **Arazzo sürümü**.

---

## 3. Sıradaki turlar

| Tur | Eksen | Ne aranacak |
|---|---|---|
| 3 | Veri modeli | ADR-0016 iddiaları ↔ `Test-Platform-Schema.dbml` iç tutarlılığı |
| 4 | Auth ve secret | ADR-0012/0013 ↔ host kompozisyonu ve csproj gerçeği |
| 5 | Paketleme | ADR-0002/0003/0006 ↔ `common.props`, PackageValidation suppression'ları |
| 6 | Yazarlık hattı | ADR-0017'nin DMN motoru ve `Microsoft.Extensions.AI` iddiaları |
| 7 | Köprü kodu | ADR-0018/0019/0020 ↔ KBP-88/89 çıktısı |
