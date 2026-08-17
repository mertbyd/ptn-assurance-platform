---
id: ADR-0025
type: decision
status: proposed
title: UI yigini, uc kokenli yuzey siniri ve ajan olay sozlesmesi
created: 2026-08-17
updated: 2026-08-17
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs:
  - ADR-0013
  - ADR-0018
  - ADR-0023
rule_refs:
  - RULE-0005
  - RULE-0007
---

# ADR-0025 — UI yığını, üç kökenli yüzey sınırı ve ajan olay sözleşmesi

> [!IMPORTANT] Bu ADR **`proposed`** durumdadır
> İçeriği analizden türetilmiştir; ürün sahibi kabul etmeden `accepted` olmaz ve bağlayıcı
> değildir. Kabul edilirse `status` güncellenir ve [[00-Home|INDEX-0001]] `decision_refs`
> listesine eklenir (ADR-0001).

## Bağlam

Backend yüzeyi UI yazılabilecek olgunluğa yaklaştı: Test Module **65 uç**, iki checker
**125 action**, ajan **5 uç**. Buna karşılık üç şey karara bağlanmamış durumda ve her biri
ilk satır kod yazılmadan cevap ister:

1. UI hangi kökenlere, hangi zarf ve hangi kimlikle konuşacak?
2. Tipler nereden gelecek — ajan yüzeyi OpenAPI yayınlamıyor.
3. Ajan olay akışı bugünkü yedi olayla mı kalacak?

Eski karar kaydı [[04-Architecture/UI-Integration-Architecture|ARCH-0007]] "tek Swagger,
tek `schema.d.ts`, üç seçenekli portal" diyordu; o metin **Test Module'ün kimlik uçlarını
barındırdığı** varsayımına dayanıyor ve ADR-0013 ile çelişiyor
([[90-Inbox/AUDIT-0004-Ui-Oncesi-Wiki-Gerceklik-Denetimi|AUDIT-0004]] B3).

## Karar

### A. Üç köken, üç sözleşme — tek istemci yok

UI üç ayrı origin'e konuşur ve **her biri için ayrı istemci** kurulur.

| Köken | Zarf | Kimlik | İstemci |
|---|---|---|---|
| `<TEST_MODULE_ORIGIN>` | `Result<T>` / `PagedResultDto<T>` | Bearer + ABP izinleri | `openapi-fetch` + interceptor |
| `<AGENT_ORIGIN>` | düz JSON + `text/event-stream` | *(bkz. §E)* | elle yazılmış ince istemci + SSE okuyucu |
| `<AUTH_ORIGIN>` | Authenticator sözleşmesi | OIDC | Authenticator istemcisi |

Origin değerleri **derleme zamanında sabitlenmez**; runtime konfigürasyonundan gelir.
`localhost` veya port hiçbir kaynak dosyada bulunmaz.

**Gerekçe:** iki köken farklı hata biçimi kullanır (`Result` alanları vs `{ code }`) ve
farklı kimlik taşır. Tek istemci bunları birleştirirse hata işleme ya yanlış ya da
en küçük ortak paydaya iner.

### B. Yığın

| Katman | Seçim | Gerekçe |
|---|---|---|
| Çatı | React + App Router | Eski iki UI'ın ortak tabanı; göç maliyeti en düşük |
| Sunucu durumu | **TanStack Query** | Eski API Checker UI kuralının sürdürülmesi (CURRENT-0006) |
| İstemci durumu | **Zustand** | Aynı süreklilik; sohbet durumu için yeterli |
| Tip üretimi | **`openapi-typescript`** | RULE-0001 (UI) — tipler elle yazılmaz |
| HTTP | **`openapi-fetch`** | ~2 KB, statik tip çıkarımı, runtime kontrolü yok |
| Sorgu köprüsü | **`openapi-react-query`** | ~1 KB; kod üretmez, tip üretir |
| Sohbet kabuğu | hazır primitives | Mesaj listesi/streaming; **kesinti kartları kendi bileşenimiz** |

Kod üreten alternatifler (`Hey API`, `openapi-react-query-codegen`, `openapi-qraft`)
reddedildi: üretilen kod, üretilen tipten daha pahalı bakım gerektirir ve sürüm
yükseltmelerinde diff gürültüsü yaratır.

### C. Tip kaynağı — ajan yüzeyi elle yazılır ve Zod ile hizalanır

`openapi-typescript` yalnız `<TEST_MODULE_ORIGIN>` Swagger'ından üretir; çıktı
`src/api/generated/test-module.d.ts`'dir ve **elle düzenlenmez**.

Ajan yüzeyinin OpenAPI belgesi **yoktur** (Fastify + Zod, ADR-0023 §B). Bu yüzden ajan
tipleri `src/api/agent/contracts.ts` altında **elle yazılır** ve `ptn-test-agent/src/contracts.ts`
ile birebir hizalanır. Hizalama bir **test** ile korunur: ajan şeması değişirse UI testi kırılır.

> Alternatif — ajana OpenAPI ürettirmek — reddedilmedi, **ertelendi**: Fastify'da
> Zod→OpenAPI köprüsü mümkündür ve ileride Ö-2'yi ortadan kaldırır. Ölçülmüş ihtiyaç
> doğduğunda ayrı karar.

### D. Ajan olay sözleşmesi — yedi olay korunur, **iki olay eklenir**

Bugünkü yedi olay (`text_delta`, `tool_call`, `input_required`, `approval_required`,
`completed`, `cancelled`, `error`) korunur. AG-UI protokolü **benimsenmez**; sözlüğü
referans alınır (RESEARCH-0017 §1).

İki olay eklenir:

| Olay | Gövde | Neden |
|---|---|---|
| `run_started` | `{ turn, maxTurns }` | UI bugün "başladı"yı istekten türetiyor; ilk `text_delta` gecikirse ekran ölü görünüyor |
| `state_delta` | `{ stepCount, pendingQuestionCodes[] }` | Belge durumu bugün ayrı REST çağrısıyla çekiliyor; iki kaynak arasında yarış var |

**`tool_call_result` eklenmez.** Tool sonucu modele gider, tarayıcıya gitmez: ajan
checker'ın ham kanıtını görmez ve göstermez (ADR-0018). Bu, AG-UI'den bilinçli sapmadır.

### E. Ajan yüzeyinin kimliği — UI doğrudan bağlanmaz

Bugün ajan HTTP yüzeyinde **kimlik doğrulama yoktur** ve tek paylaşılan
`PTN_MCP_BEARER_TOKEN` kullanılır; bu, tenant izolasyonunu ajan sınırında düşürür
(ARCH-0005 §8, boşluk 1–2).

Karar: **tarayıcı ajana doğrudan bağlanmaz.** Ajan yüzeyi ancak şu ikisinden biri
sağlandıktan sonra UI'ya açılır:

1. Ajan, gelen bearer'ı doğrular ve **çağıranın kimliğiyle** MCP'ye bağlanır; ya da
2. Test Module tarafında kimlik doğrulayan ince bir ters vekil (reverse proxy) ajanın
   önüne konur.

Hangisi seçilirse seçilsin, **paylaşılan tek token ile çok kiracılı UI açılmaz.**

### F. İzin görünürlüğü — buton izinden türetilir

Her eylem butonu ilgili ABP permission'ına bağlanır ve izin yoksa **render edilmez**.
`403` alınması bir UI hatası olarak ele alınır. İzin kümesi
`GET /api/abp/application-configuration` üzerinden okunur.

`Anonymous` uçlar (`runs/webhook`, notification stream, Google callback) UI istemcisinde
**tanımlanmaz**.

### G. Zarf tek yerde açılır

`Result<T>` ve `PagedResultDto<T>` yalnız istemci interceptor'ında açılır; feature kodu
`T` görür. Ajan `{ code }` hataları ayrı bir eşleyiciden geçer. İki biçim
**birleştirilmez**.

## Alternatifler

- **Tek origin, tek istemci, tek `schema.d.ts`** (ARCH-0007 §2'nin eski önerisi). Ajan
  yüzeyi OpenAPI yayınlamadığı ve auth ayrı deploy edildiği için fiilen imkânsız.
- **AG-UI protokolünü benimsemek.** Olgun ve iyi belgelenmiş; ancak `ToolCallResult`
  merkezli tasarımı bizim kanıt gizleme kararımızla çelişir ve ajan tarafında yeni bir
  bağımlılık katmanı getirir.
- **CopilotKit gibi tam ajan UI çatısı.** Kesinti modelimiz (iki ayrı kesinti tipi) ve
  kapalı soru/onay kartlarımız çatının varsayımlarına oturmuyor.
- **Ajanı .NET host'a gömmek** ve tek origin'e inmek. ADR-0023 §G ile reddedildi;
  RULE-0005 sınırını bozar.
- **Arazzo editörü gömmek.** Serbest YAML düzenleme `SourceHash` mührünü kırar; yüzey
  okuyucu + diff olarak kalır (RESEARCH-0017 §6).

## Sonuçlar ve riskler

| Risk | Önlem |
|---|---|
| İki istemcinin hata işleme farkı UI'da tutarsız görünür | Ortak `AppError` iç tipi; iki eşleyici tek tipe indirger |
| Ajan şeması sessizce kayar | UI ↔ `contracts.ts` hizalama testi |
| Swagger üretilemiyor (blokaj E-2/E-5) | Faz 0 sahte sunucuya karşı yazılır; codegen faz 1'de bağlanır |
| Origin konfigürasyonu unutulur | Kaynak taramasında `localhost`/port literal'i yasak; CI kontrolü |
| İki yeni SSE olayı ajan sürümünü kırar | Olaylar **eklemedir**; bilinmeyen olay tipi UI'da sessizce yok sayılır |
| Onay ekranı ajan metnini öne çıkarır → aşırı güven | Kanıt birincil, ajan metni ikincil (CURRENT-0007 G-03) |

## Açık bıraktıkları

Bu ADR **şunları karara bağlamaz** ve bunlar ürün sahibinin kararıdır
(CURRENT-0007 §7): sohbet geçmişinin kalıcılığı (`agent_sessions`), otonomi seviyesi
ayarı, `senaryo.md`'nin mühre bağlanması ve RULE-0008 DMN kapsam kapısının UI'da
gösterilmesi.

## Kaynaklar

Ayrıntılı dış tarama ve kanıt tablosu:
[[90-Inbox/RESEARCH-0017-Ajan-Arayuzu-Desenleri-Ve-Referans-Uygulamalar|RESEARCH-0017]] §9.
