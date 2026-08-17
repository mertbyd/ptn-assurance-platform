---
id: ARCH-0005
type: architecture
status: active
title: Ajan yuzeyi — dosya yuklemesinden sohbete, sohbetten yayina
updated: 2026-08-17
decision_refs:
  - ADR-0008
  - ADR-0013
  - ADR-0014
  - ADR-0017
  - ADR-0018
  - ADR-0020
  - ADR-0023
rule_refs:
  - RULE-0005
  - RULE-0006
  - RULE-0007
---

# Ajan yüzeyi — dosya yüklemesinden sohbete, sohbetten yayına

> **Bu sayfa UI'nin ajan tarafının tek giriş kapısıdır.** [[04-Architecture/Alti-An|ARCH-0004]]
> ürünün ne yaptığını anlatır; bu sayfa **An 1–4'ün tarayıcıda nasıl göründüğünü** ve hangi
> uçların bunu taşıdığını anlatır. Kanıt: `ptn-test-agent/src/**` ve
> `ptn-test-module/src/Ptn.TestModule.HttpApi/Controllers/**` (2026-08-17 okuması).

## 0. Bir cümlede

UI **iki ayrı sunucuya** konuşur: deterministik her şey **Test Module REST**'e (`Result<T>`
zarfı, ABP izinleri), model akışı **ajan HTTP/SSE**'ye gider. Ajan Test Module'e yalnız
**MCP** üzerinden erişir; UI ajanın MCP'sine hiç dokunmaz.

```
Tarayici (UI)
   │
   ├── REST + Bearer ──►  <TEST_MODULE_ORIGIN>   65 uc, Result<T>, ABP izinleri
   │                          ▲
   ├── SSE + JSON ─────►  <AGENT_ORIGIN>         5 uc, event akisi
   │                          └── MCP (Streamable HTTP, tek bearer) ──┘
   │
   └── OIDC ───────────►  <AUTH_ORIGIN>          login/refresh (ADR-0013)
```

**Kural:** aynı iş için iki yol varsa **deterministik olan** UI'nin kaydıdır. Ajan yalnız
öneri üretir; mühür, kapı ve yayın Test Module'dedir.

---

## 1. An 1 — Dosya yükleme: **iki ayrı yol vardır ve karıştırılamaz**

Ürün iki dosya bekler (ARCH-0004): `senaryo.md` (insanın anlattığı akış) ve `kurallar.md`
(iş kuralları). Bugün kodda **üç farklı hedef** var ve UI'nin hangisine ne göndereceği
yayın kapısını doğrudan belirler.

| Hedef | Uç | Ne olur | Mühre girer mi |
|---|---|---|---|
| **Test Module — kural kaynağı** | `POST /api/test-module/authoring/business-rules` | İçerik tek kanonik dosyaya yazılır, `sha256:` mührü döner | **Evet** — `RulesFingerprint` |
| **Test Module — profil paketi** | `POST /api/test-module/authoring/profile-packs` | YAML paket anahtardan türetilen dosyaya yazılır | **Evet** — `ProfileFingerprint` (blokaj 9, aşağıda) |
| **Ajan — prompt bağlamı** | `POST /api/agent/sessions/{id}/uploads` | İçerik yalnız model instruction'ına `<user-file>` olarak eklenir | **Hayır** |

> [!CAUTION] `kurallar.md`'yi yalnız ajana yüklemek sessiz bir hatadır
> Ajan yüklemesi belleğe (`session.uploads`, `Map`) yazar ve **hiçbir mühür üretmez**. Ajan
> oturum açılışında kuralları zaten MCP Resource `ptn://authoring/kurallar.md` üzerinden
> okur — yani **kanonik kaynak Test Module'dedir**. UI `kurallar.md`'yi önce
> `authoring/business-rules` ucuna yüklemez ve mührü senaryoya taşımazsa, `MaterialIntegrity`
> kapısı bayat/boş `RulesFingerprint` ile **`InvalidHash`** verir (KBP-116).

**UI akışı — doğru sıra:**

1. `POST authoring/business-rules` → dönen `Fingerprint` state'e alınır.
2. (Varsa) `POST authoring/profile-packs` → `GET authoring/profile-packs` ile doğrulanır.
3. `POST /api/agent/sessions` → ajan **yeni** kuralları MCP Resource'tan okur.
4. `senaryo.md` **yalnız** ajan oturumuna yüklenir — backend'de karşılığı yoktur (§6, boşluk 1).

### Yükleme sınırları (UI validasyonu bunları önden uygular)

| Sınır | Değer | Kaynak |
|---|---|---|
| Ajan dosya adı | **kapalı küme**: `senaryo.md` · `kurallar.md` | `contracts.ts` `UploadSchema` |
| Ajan içerik bütçesi | varsayılan **262.144 bayt** (256 KB), tavan 1 MB | `AGENT_UPLOAD_MAX_BYTES` |
| Ajan gövde limiti | **1.048.576 bayt** | Fastify `bodyLimit` |
| Biçim | **UTF-8 düz metin** — multipart yoktur | ADR-0023 §B |
| Test Module kanonikleştirme | `ptn-source-canonical-v1`: BOM kırpılır, CRLF→LF, satır sonu boşlukları ve sondaki boş satırlar kırpılır | KBP-116 |

UI dosyayı **kanonikleştirmez**; ham metni gönderir ve sunucunun döndürdüğü mührü kullanır.
İstemci kendi hash'ini hesaplayıp gönderirse sunucununkiyle eşleşmek zorundadır.

---

## 2. An 2–3 — Sohbet: ajan HTTP/SSE sözleşmesi

Ajan yüzeyi **beş uçtan** ibarettir (`ptn-test-agent/src/http/create-server.ts`).

| # | Uç | Gövde | Yanıt |
|---|---|---|---|
| 1 | `POST /api/agent/sessions` | `{ momentCode }` | `201` `{ id, momentCode, status, allowedToolCodes, maxTurns, tokenLimit }` |
| 2 | `POST /api/agent/sessions/{id}/messages` | `{ message, answers[] }` | `200` **`text/event-stream`** |
| 3 | `POST /api/agent/sessions/{id}/uploads` | `{ fileName, content }` | `204` |
| 4 | `POST /api/agent/sessions/{id}/cancel` | — | `204` |
| 5 | `POST /api/agent/sessions/{id}/approval` | `{ approved }` | `204` |

**Hata sözleşmesi** (Test Module'ün `Result<T>` zarfı **değildir**):

| Durum | Gövde | UI davranışı |
|---|---|---|
| `404` | `{ code: "session_not_found" }` | Oturum düştü — yeni oturum aç, taslağı koru |
| `400` | `{ code: "invalid_request" }` | Girdi şeması hatası — alan bazlı gösterilemez, jenerik uyarı |
| `409` | `{ code: "agent_state_conflict" }` | Durum makinesi ihlali (§3) — butonlar durumdan türetilmediği için oluşur |

`momentCode` kapalı kümedir: `Grounding` · `Drafting` · `Validation` · `Approval` ·
`Execution` · `Diagnosis` (`AgentMomentCodes`). UI serbest metin göndermez.

### SSE olay sözlüğü — yedi olay

`event:` alanı olay tipini, `data:` alanı JSON gövdeyi taşır.

| Olay | Gövde | UI ne yapar |
|---|---|---|
| `text_delta` | `{ delta }` | Balona token ekler |
| `tool_call` | `{ name }` | *"`ptn_ground` çağrılıyor…"* rozeti; ad kapalı `PtnToolCodes` kümesindendir |
| `input_required` | `{ questions[] }` | **Akış durur.** Kapalı soru kartı açılır (§4) |
| `approval_required` | `{ proposal }` | **Akış durur.** Adım onay kartı açılır (§5) |
| `completed` | `{ turns, tokens }` | Bütçe göstergesi güncellenir, giriş açılır |
| `cancelled` | — | Oturum kapanır |
| `error` | `{ code, message }` | `budget_exceeded` \| `agent_failure` |

> [!IMPORTANT] `input_required` ve `approval_required` **stream'i bitirir**
> Sunucu bu iki olaydan sonra `return` eder; bağlantı kapanır. UI "akış devam ediyor" sanıp
> beklememelidir. Devam, **yeni bir `messages` isteğiyle** (cevaplarla) ya da `approval`
> çağrısıyla olur.

`error.message` **kasıtlı olarak jenerik**tir; sağlayıcı ve secret ayrıntısı sızdırmaz
(ADR-0023 §F). UI kullanıcıya teknik detay vaat eden bir metin yazmaz.

---

## 3. Oturum durum makinesi — UI butonları buradan türetilir

Ajan oturumu beş durumludur (`SessionStore`):

```
        ┌───────── POST /messages ─────────┐
        │                                   ▼
     ready ◄──── completed / error ──── running
        ▲                                   │
        │                    ┌──────────────┼──────────────┐
        │                    ▼              ▼              ▼
        │            input_required  approval_required  cancelled
        │                    │              │
        └── /messages(cevap) ┘              └── /approval{approved:true} ──► ready
                                            └── /approval{approved:false} ─► cancelled
```

| Durum | `/messages` | `/uploads` | `/approval` | `/cancel` |
|---|---|---|---|---|
| `ready` | ✅ | ✅ | ❌ `409` | ✅ |
| `running` | ❌ `409` | ✅ | ❌ `409` | ✅ (abort) |
| `input_required` | ✅ **cevapla** | ✅ | ❌ `409` | ✅ |
| `approval_required` | ❌ `409` | ✅ | ✅ | ✅ |
| `cancelled` | ❌ `409` | ❌ `409` | ❌ `409` | — |

**UI kuralı:** gönder butonu `ready` ve `input_required` dışında **disabled**'dır. `409`
alınması bir UI hatasıdır, kullanıcı hatası değildir.

---

## 4. Kapalı soru kartı — ajanın tahmin etmediği yer

`input_required` geldiğinde ajan durur çünkü checker kanıt üretemedi (RULE-0007). Soru
gövdesi:

```json
{ "questionCode": "OPERATION_REFERENCE_REQUIRED",
  "prompt": "...", "options": ["...", "..."], "gapKindCode": "..." }
```

**UI sözleşmesi — dördü de zorunlu:**

1. Girdi **yalnız seçim**dir. Serbest metin kutusu **açılmaz** — sunucu
   `question.options.includes(selected)` kontrolünü zaten yapar ve dışarıdaki değeri reddeder.
2. **Bekleyen her soru tam bir kez** cevaplanır; eksik veya fazla cevap `409` verir.
3. `questionCode` kararlı kod olarak taşınır; UI onu **çevirir**, değiştirmez.
4. Cevaplar bir sonraki `POST /messages` gövdesinde `answers[]` olarak gider (en çok 20).

### Kod → ekran metni eşlemesi (`PtnOpenQuestionCodes`)

| Kod | Ne demek | Ekranda önerilen dil |
|---|---|---|
| `OPERATION_REFERENCE_REQUIRED` | Kanıt var ama operasyon seçilmemiş | "Bu adım hangi API operasyonuna düşüyor?" |
| `OPERATION_SELECTION_REQUIRED` | Birden çok aday, eşik altı | "Aday operasyonlardan birini seç" |
| `TABLE_SELECTION_REQUIRED` | Hedef tablo belirsiz | "Doğrulanacak tabloyu seç" |
| `ASSERTION_REFERENCE_REQUIRED` | Assertion adresi yok | "Hangi alan doğrulanacak?" |
| `EVIDENCE_UNAVAILABLE` | **Kanıt toplanamadı** | "Kanıt okunamadı" — ❌ *"yetki yok"* denmez (ADR-0019 §C) |
| `NOT_BOUND:<kavram>` | Kavram profile bağlanmamış | "'<kavram>' henüz şemaya bağlanmadı" |

---

## 5. Onay kartı — kademe 4 burada durur

Ajanın önerebileceği **tek yapı** budur (`StepProposalSchema`):

```json
{ "stepId": "createBooking", "operationReferenceId": "<uuid>",
  "requestBodyJson": "…", "assertionPaths": ["/data/id", "/data/status"] }
```

`assertionPaths` **en az 1, en çok 50**'dir — RULE-0006'nın istemci tarafındaki karşılığı.
`operationReferenceId` opak referanstır; ajan operasyon **adı** uyduramaz.

**Onay ekranının göstermesi zorunlu dört şey** (RESEARCH-0012 §5A.5):

| Alan | Kaynak |
|---|---|
| **Ne yapılacak** | `stepId` + opak referansın çözülmüş `method` + `path`'i (`AuthoringStepDto`) |
| **Neden** | Ajanın o tura ait metni + hangi `tool_call`'ların koştuğu |
| **Ne değişecek** | Oturumun `SourceDocument`'i önce/sonra farkı |
| **Nasıl geri alınır** | Oturum TTL'i içinde adım eklenmez; oturum atılır |

> [!WARNING] Ajanın onayı **yayın onayı değildir**
> `POST /approval` yalnız *"bu adım belgeye eklensin mi"* sorusudur. Senaryonun yayınlanması
> ayrı ve **insan** kapısıdır: `submit-for-approval` → `evaluate-publication` → `publish`
> (`Scenarios.Approve` **ve** `Scenarios.Publish` birlikte istenir). Ajanın tool kataloğunda
> yayınlama yoktur ve hiçbir otonomi seviyesinde otomatikleşmez (RULE-0005).

**Reddetme yıkıcıdır:** `{ approved: false }` oturumu `cancelled` yapar; oturum **geri
dönmez**. UI bunu "öneriyi düzelt" gibi göstermemeli, "oturumu kapat ve yeniden başla"
olarak göstermelidir.

---

## 6. Ajan → Test Module: adımın belgeye işlendiği yer

Ajanın önerisi onaylandıktan sonra belgeye **Test Module** yazar; ajan yazmaz.

| Ne | Uç | İzin |
|---|---|---|
| Oturum aç (gerçek grounding ile) | `POST /api/test-module/authoring/sessions` | `Scenarios.Create` |
| Oturumu oku | `GET  /api/test-module/authoring/sessions/{id}` | `Scenarios.Update` |
| Kapalı soruyu cevapla | `POST /api/test-module/authoring/sessions/{id}/answer` | `Scenarios.Update` |
| **API adımı** ekle | `POST /api/test-module/authoring/sessions/{id}/step` | `Scenarios.Update` |
| **DB adımı** ekle | `POST /api/test-module/authoring/sessions/{id}/database-step` | `Scenarios.Update` |

Bu oturum **ABP distributed cache**'tedir: `TestModuleAuthoringSessions`, **TTL 30 dakika**,
tenant anahtarı ayrık. Tablo, repository ve migration **yoktur**.

> [!CAUTION] TTL bir UI gereksinimidir
> 30 dakika dolduğunda oturum ve içindeki Arazzo belgesi **kaybolur**. UI kalan süreyi
> `AuthoringSessionDto.TtlMs` üzerinden göstermeli ve süre bitmeden kullanıcıyı
> `POST /api/test-module/scenarios` ile kalıcılaştırmaya yönlendirmelidir.

### İki oturum vardır ve aynı şey değildir

| | Ajan oturumu | Test Module yazarlık oturumu |
|---|---|---|
| Sahibi | `ptn-test-agent` (bellek, `Map`) | Test Module (distributed cache) |
| Ömrü | Process ömrü — **kalıcı değil** | **30 dk** TTL |
| Taşıdığı | Sohbet, tur/token bütçesi, bekleyen soru | Sorular, cevaplar, adımlar, **Arazzo belgesi** |
| Kimliği | `POST /api/agent/sessions` → `id` | `GroundRequestDto.SessionId` |

İkisi `ptn_ground` üzerinden bağlanır: `GroundRequestDto` `SessionId` + tek `ProposedStep`
taşır (KBP-112). **Yeni MCP tool açılmadı.** UI iki kimliği ayrı state'te tutmalıdır.

### DB adımı kapalı kümedir

`AddDatabaseAuthoringStepDto` opak `TableReferenceId` + `PtnDatabaseMatcherCodes`'tan bir
matcher ister. UI matcher açılır listesini **bu 11 koddan** kurar; serbest metin validator'da
reddedilir:

`Equals` · `NotEquals` · `IsNull` · `IsNotNull` · `GreaterThan` · `GreaterThanOrEqual` ·
`LessThan` · `LessThanOrEqual` · `MatchesRegex` · `OneOf` · `WithinTolerance`

---

## 7. Bütçe ve tur sınırı — ekranda görünmek zorundadır

| Kaynak | Alan | Nerede |
|---|---|---|
| Sunucu profili | `maxTurns`, `tokenLimit` | `POST /api/test-module/bridge/agent-profile` |
| İstemci tavanı | `AGENT_MAX_TURNS` (8), `AGENT_TOKEN_LIMIT` (16.000) | Ajan config |
| Etkin değer | **`min(sunucu, istemci)`** | `AuthoringAgent.startSession` |
| Anlık harcama | `completed` olayının `turns` / `tokens` alanları | SSE |

Sınır aşılırsa `error.code = "budget_exceeded"` gelir ve oturum `ready`'ye döner —
**sessiz devam yoktur** (RESEARCH-0012 §3.3). UI kalan turu bir sayaçla göstermeli, sıfıra
yaklaşırken kullanıcıyı uyarmalıdır.

Tool sayısı ayrıca sınırlıdır: aktif tool **≤ 7** (`PtnToolCodes.ActiveMax`), protokol
tavanı 12. `tools/list`'te **10** tool görünür; `ptn_patch_suggest` ve `ptn_patch_review`
`ReviewOnly` kümesindedir ve listeye girmez. UI tool rozetlerini
`tools/list ∩ ptn_profile.allowedToolCodes` kesişiminden kurar.

---

## 8. UI'yi bugün durduran boşluklar

> [!CAUTION] Bu bölüm karar değil **ölçüm**dür; her satır koddan doğrulandı (2026-08-17)

| # | Boşluk | Kanıt | UI'ye etkisi |
|---|---|---|---|
| 1 | **Ajan yüzeyinde kimlik doğrulama yok** | `create-server.ts` hiçbir uçta bearer/authorization okumaz | Ajan ucu tarayıcıya doğrudan açılamaz |
| 2 | **Tek paylaşılan MCP kimliği** | `PTN_MCP_BEARER_TOKEN` tek process env'inden gelir | **Tenant izolasyonu ajan sınırında kaybolur** — RESEARCH-0012 §4.6'nın *"ajan oturum açan kullanıcı adına çalışır"* iddiası kodda **yok** |
| 3 | **Oturum kalıcı değil** | `SessionStore` bir `Map`; `GET session` ucu yok | Sayfa yenileme, sekme değişimi ve ajan restart'ı sohbeti siler |
| 4 | **Oturum listesi yok** | `agent_sessions` tablosu hâlâ **tasarım** (RESEARCH-0012 §4.4) | "Geçmiş konuşmalar" ekranı kurulamaz; token faturalandırması ölçülemez |
| 5 | **`senaryo.md` backend'de yok** | Yalnız `senaryo.md`/`kurallar.md` ajan yüklemesi; Test Module'de karşılığı yalnız `business-rules` | Senaryo belgesi mühre kanıtla bağlanamaz |
| ~~6~~ | ✅ **`ProfileFingerprint` kapandı** (2026-08-17, `c7c7773`) | Sunucu profil paketinin `ContentFingerprint`'ini mühürlüyor | `MaterialIntegrity` kapısı artık elle değer istemiyor |
| 7 | **Yazarlık ve köprü port'ları DI'da kayıtlı değil** | `AuthoringSessionCacheService`, `ScenarioCompilationService`, `ProcessBoundaryService` ve altı kardeşi `[ExposeServices]` taşımıyor | `POST authoring/sessions` ve `bridge/*` **runtime'da çözülemez** — [[90-Inbox/AUDIT-0005-Backend-Teslim-Denetimi\|AUDIT-0005]] B-1 |
| 8 | Host `EnsureSharedAbpSchema` bayrağı olmadan **açılmıyor** | `TestModuleHttpApiHostModule.cs:256-259` | Kurulum önkoşulu; bayrak `true` olsa da Authenticator migration'ları gerekir |

**1, 2 ve 7 kapanmadan UI ajan ekranı üretime çıkamaz.** 3 ve 4 kapanmadan sohbet
"tek oturumluk" bir araç olarak kalır — bu ürün kararıdır, teknik engel değildir.

> [!NOTE] Kapandı (2026-08-17 kod doğrulaması)
> İzin yüzeyi artık compose ediliyor (`TestModuleHttpApiHostModule.cs:74-76`) ve `SpecFingerprint`
> sunucuda hesaplanıyor (`TestScenarioAppService.cs:196,220`). CURRENT-0001 blokaj tablosunun
> 1 ve 4 numaralı satırları eskidir.

---

## 9. Karşılaştırma — ajan yüzeyimiz sektör deseninin neresinde

Ayrıntı ve kaynaklar: [[90-Inbox/RESEARCH-0017-Ajan-Arayuzu-Desenleri-Ve-Referans-Uygulamalar|RESEARCH-0017]].

| Sektör deseni (AG-UI) | Bizde karşılığı | Fark |
|---|---|---|
| `RunStarted` / `RunFinished` | *(yok)* — `completed` var, başlangıç olayı yok | UI "başladı" durumunu istekten türetir |
| `TextMessageContent` | `text_delta` | Aynı |
| `ToolCallStart` / `Args` / `End` / `Result` | `tool_call` (yalnız ad) | **Tool sonucu UI'ya hiç akmaz** — bilinçli: ham kanıt tarayıcıya sızmaz (ADR-0018) |
| `StateSnapshot` / `StateDelta` | *(yok)* | Belge durumu Test Module `GET sessions/{id}` ile çekilir |
| Interrupt / approval | `input_required` + `approval_required` | Bizde **iki ayrı** kesinti: bilgi eksiği ve onay |
| `RunError` | `error` | Bizde mesaj kasıtlı jeneriktir |

**Hüküm:** olay kümemiz AG-UI'nin sadeleştirilmiş bir alt kümesidir ve kesinti deseni
birebir örtüşür. Protokolü **benimsemek zorunda değiliz**; ama `RunStarted` ve bir
`state` olayı eklemek UI'nin kendi başına türetmek zorunda kaldığı iki durumu ortadan
kaldırır — [[03-Decisions/ADR-0025-Ui-Yigini-Ve-Uc-Kokenli-Yuzey-Siniri|ADR-0025]] §D.
