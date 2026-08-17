---
id: GUIDE-0006
type: guide
status: active
title: UI kurulum kilavuzu — yigin, kod uretimi, klasor duzeni ve faz plani
updated: 2026-08-17
decision_refs:
  - ADR-0013
  - ADR-0023
  - ADR-0025
rule_refs:
  - RULE-0004
  - RULE-0005
---

# UI kurulum kılavuzu

> Bu sayfa *"nasıl kurulur"*dur. **Ne kurulacağı** [[01-Current/UI-Requirements-Truth|CURRENT-0007]],
> **hangi uç** [[04-Architecture/UI-Endpoint-Screen-Matrix|ARCH-0006]], **ajan yüzeyi**
> [[04-Architecture/UI-Agent-Experience|ARCH-0005]], **yığın kararı**
> [[03-Decisions/ADR-0025-Ui-Yigini-Ve-Uc-Kokenli-Yuzey-Siniri|ADR-0025]]'tedir.

## 0. Başlamadan önce okunacaklar

| Sıra | Belge | Ne verir |
|---|---|---|
| 1 | [[04-Architecture/Alti-An\|ARCH-0004]] | Ürün ne yapıyor — bir sayfa |
| 2 | **CURRENT-0007** | Gereksinimler, sözlükler, engeller |
| 3 | **ARCH-0005** | Ajan yüzeyi ve durum makinesi |
| 4 | **ARCH-0006** | Ekran–uç–izin matrisi |
| 5 | **ADR-0025** | Yığın ve köken sınırı |
| 6 | RULE-0005 · RULE-0006 · RULE-0007 | Ajanın yapamayacakları — UI'nin de yapamayacaklarıdır |

---

## 1. Depo yerleşimi

UI **ayrı bir deployable**dır ve `ptn-test-module` solution'ına dahil edilmez.

```
ptn-assurance-platform/
  ptn-test-module/     .NET composition host    ← dokunulmaz
  ptn-test-agent/      Node ajan (Fastify+SSE)  ← dokunulmaz
  ptn-assurance-ui/    YENI                     ← bu kilavuz
```

**Neden ayrı:** ADR-0023 §G ajan için aynı sınırı koyuyor; UI da aynı gerekçeyle ayrıdır.
`.NET` host'una statik dosya olarak gömülmesi bir deployment kararıdır ve bu kılavuzun
kapsamı dışındadır.

## 2. Klasör düzeni

```
src/
  app/(portal)/
    assurance/       runs, findings, health, dashboard
    authoring/       upload, chat, steps, publish
    api-contract/    sources, snapshots, checks
    database/        connections, discovery, comparison
    settings/        environments, lookups, profile-packs
  api/
    generated/       test-module.d.ts        ← URETILIR, elle duzenlenmez
    test-module/     openapi-fetch client + Result<T> interceptor
    agent/           contracts.ts + sse-client.ts   ← elle yazilir
    auth/            Authenticator istemcisi
  features/
    <alan>/          hooks, components, state   (RULE-0004 izolasyonu)
  shared/
    vocab/           kapali sozluk cevirileri
    ui/              tasarim sistemi
```

**Kural:** `features/*` birbirini import etmez. Paylaşılan şey `shared/`'a çıkar.

## 3. Üç istemci

### 3.1 Test Module istemcisi

```bash
npx openapi-typescript "$TEST_MODULE_ORIGIN/swagger/v1/swagger.json" -o ./src/api/generated/test-module.d.ts
```

> [!WARNING] Bu komut bugün çalışmaz
> Swagger isteği veritabanında `abp.AbpSettings` tablosu olmadığı için middleware'de **500**
> verir. Koşul kod değil **kurulum**dur: aynı veritabanına Authenticator migration'ları
> uygulanmış olmalıdır (CURRENT-0005, CURRENT-0001 blokaj 2). Faz 0 bu yüzden sahte
> şemayla yürür.

İnterceptor iki işi yapar ve **başka hiçbir şey yapmaz**:

1. `Result<T>` / `PagedResultDto<T>` zarfını açar.
2. Bearer token'ı ekler ve `401` durumunda refresh akışını tetikler.

### 3.2 Ajan istemcisi

OpenAPI yok; tipler `ptn-test-agent/src/contracts.ts` ile elle hizalanır (ADR-0025 §C).
SSE okuyucusu şu sözleşmeyi uygular:

- `event:` alanı olay tipidir; **bilinmeyen tip sessizce yok sayılır** (ileri uyumluluk).
- `input_required` ve `approval_required` **stream'i bitirir** — UI beklemez, kart açar.
- `AbortController` `cancel` ucuna değil, **hem** isteğe **hem** `POST /cancel` çağrısına
  bağlanır.

### 3.3 Auth istemcisi

`<AUTH_ORIGIN>` ayrı OIDC discovery kullanır. Auth çağrısı **hiçbir koşulda** Test Module
kökene gönderilmez (ADR-0013).

## 4. Kapalı sözlük çevirileri

`shared/vocab/` altında her sözlük için `kod → etiket` haritası tutulur. Üç kural:

1. Harita **eksiksiz** olmalı; bilinmeyen kod ham kod olarak gösterilir, gizlenmez.
2. Renk ve ikon da haritadadır — `Inconclusive` **kırmızı değildir** (CURRENT-0007 §4).
3. Lookup satırlarının `name`/`description`'ı backend'den gelir; harita yalnız
   **koda bağlı davranış** (renk, ikon, sıra) içindir.

## 5. Test stratejisi

| Katman | Ne test edilir |
|---|---|
| Sözleşme | UI ajan tipleri ↔ `ptn-test-agent/src/contracts.ts` hizalaması |
| Durum makinesi | Beş ajan durumunun buton matrisi (ARCH-0005 §3) — `409` üretilemez |
| Sözlük | Her kapalı kümenin tam kapsanması; eksik kod testte kırar |
| İzin | İzinsiz kullanıcıda buton yok **ve** istek yok |
| Kapı | `evaluate-publication` kırmızıyken `publish` çağrılamaz |
| Erişilebilirlik | SSE ile akan metin ekran okuyucuya `aria-live` ile bildirilir |

## 6. Faz planı

Fazlar **engellerden bağımsız başlayabilecek** işi öne alır (CURRENT-0007 §6).

| Faz | İçerik | Engel bağımlılığı |
|---|---|---|
| **F0** | Depo, tasarım sistemi, rota iskeleti, üç istemci kabuğu, sözlük haritaları, SSE okuyucu + durum makinesi — **sahte sunucuya karşı** | Yok |
| **F1** | Codegen bağlanır; okuma ekranları (1–8, 10, 19–22) gerçek backend'e | E-2, E-5 |
| **F2** | Ajan hattı: yükleme (11), sohbet (12), soru kartı (13), onay kartı (14) | **E-1, E-3** |
| **F3** | Yayın hattı: belge önizleme (16), kapı ekranı (17), DB adım editörü (15) | **E-4** |
| **F4** | Koşum yazma: tetikleme, iptal, ihracat, ortam bağlama (23), çelişki kartı (9) | E-1, E-7 |
| **F5** | Ayarlar, lookup CRUD (24), karantina/zamanlama (18) | E-1 |

**F2 ve F3 aynı anda başlamaz.** Ajan hattı çalışmadan yayın hattının girdisi olmaz.

### Faz kapısı

Her faz sonunda: build 0 hata · sözleşme testi yeşil · izin testi yeşil · yeni ekranların
kapalı sözlükleri eksiksiz. Kapı geçmeden sonraki faz başlamaz.

## 7. Yapılmayacaklar

| Yapılmaz | Neden |
|---|---|
| Serbest YAML/Arazzo editörü | `SourceHash` mührünü kırar (RESEARCH-0017 §6) |
| Kapalı soruya "diğer" seçeneği | Sunucu seçenek dışını reddeder (RULE-0007) |
| Assertion silme/gevşetme kısayolu | Zayıflama yasağı (RULE-0006, RESEARCH-0012 §3.2) |
| `difference-kinds` çakışan dört imzası | Belirsiz route eşleşmesi (ARCH-0006 §4.1) |
| `POST /api/emailing/emails` ekranı | Auth metadata'sı yok (ARCH-0006 §4.2) |
| `runs/webhook` istemcide tanımlama | `Anonymous`, sunucu-sunucu ucu |
| Model çıktısını hüküm olarak gösterme | RULE-0005 |
| Tipleri elle yazma (Test Module tarafı) | ADR-0025 §C |

## 8. Bakım

Backend uç, izin veya sözlük değiştiğinde: ARCH-0006 yeniden üretilir, codegen çalıştırılır,
sözlük haritaları eksiklik testinden geçirilir. Üçü aynı iş içinde yapılır; biri atlanırsa
UI sessizce eskir.
