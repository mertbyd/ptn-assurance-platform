---
id: AUDIT-0004
type: audit
status: draft
title: UI oncesi wiki gerceklik denetimi — depo siniri, ajan runtime ve UI mimarisi capraz kontrolu
updated: 2026-08-17
decision_refs:
  - ADR-0013
  - ADR-0016
  - ADR-0023
  - ADR-0024
rule_refs:
  - RULE-0002
  - RULE-0005
---

# UI öncesi wiki gerçeklik denetimi

> UI yazımı için wiki genişletilirken **her iddia koda ve Git'e karşı kontrol edildi**.
> Bu sayfa yalnız **tutmayan** satırları kaydeder. Kanıt tarihi **2026-08-17**.
>
> Wiki kuralı gereği (ADR-0001) hiçbir karar bu sayfada sessizce yeniden yazılmaz;
> düzeltme ilgili ADR/Current sayfasının **sahibinin** işidir.

## A. Depo sınırı — ADR-0024 fiilen uygulanmıyor

ADR-0024 ve [[01-Current/Platform-Truth|CURRENT-0001]] "Sürüm kontrolü sınırı" bölümü
şunu söylüyor: *"Kök depo yalnız `ptn-test-module/` kaynağını, `.gitignore`, `NuGet.Config`
ve `README.md` dosyalarını izler."* ve *"`docs/` … kendi `main` dalı ve geçmişi olan
**ayrı wiki deposudur**"*, *"kök depoda `git add -f docs` çalıştırılmaz"*.

**Ölçüm (`git ls-files`, kök depo, dal `predev`):**

| Yol | Wiki ne diyor | Gerçek |
|---|---|---|
| `docs/` | Ayrı Git deposu, kök depo izlemiyor | **130 dosya izleniyor**; `docs/.git` **yok** |
| `checkers/` | Ayrı ve bağımsız depolar, izlenmiyor | **1.477 dosya izleniyor** |
| `vault/` | 2026-08-16'da takipten çıkarıldı (`23dd372`) | **21 dosya izleniyor** |
| `scripts/` | Ignored | **1 dosya izleniyor** |
| `AGENTS.md` · `CLAUDE.md` | Ignored | **İkisi de izleniyor** |
| `ptn-test-agent/` | Yalnız `ADR-0001-…md` izleniyor; paket kaynağı commit edilmedi | **19 dosya izleniyor** (`src/`, `package.json`, `pnpm-lock.yaml` dâhil) |

**Sebep açık:** `2fc2630 #KBP-118 chore: tracked the complete handover set in the repository`
ve `af20c53 #KBP-118 fix: restored the api-contract secret sources dropped by an unanchored
ignore rule`. Yani bu **kasıtlı bir devir kararı**dır; ADR-0024 ise onu hâlâ yasaklıyor.

| # | Bulgu | Sınıf |
|---|---|---|
| **A1** | ADR-0024 ile fiili depo durumu **çelişiyor**. Ya ADR yeni bir kararla `supersede` edilmeli ya da izleme geri alınmalı. Şu hâliyle ajanlar ve ekip **hangi sınırın geçerli olduğunu bilemez** | **Karar borcu** |
| **A2** | CURRENT-0001'in `ptn-test-agent` satırı ("kaynak commit edilmedi") **eskidir**; kaynak izleniyor. UI ekibi ajanı okunabilir kabul etmelidir | Kayıt düzeltmesi |
| **A3** | `NuGet.Config` düz metin `ClearTextPassword` taşıyor (CURRENT-0001 blokaj 7) ve depo artık **çok daha geniş** izliyor. Depo public'se maruziyet A1 ile birlikte büyüdü | **Security** |

## B. UI mimarisi kaydı — ARCH-0007 (eski ARCH-0003) güncel değil

`04-Architecture/UI-Integration-Architecture.md`, iki eski admin UI'ının API tüketimini
analiz eden değerli bir belgedir; ancak **hedef mimari** olarak okunursa UI ekibini yanlış
yönlendirir.

| # | Bulgu | Kanıt | Sınıf |
|---|---|---|---|
| **B1** | **Kimlik çifti**: dosya `id: ARCH-0003` taşıyordu; aynı kimlik `04-Architecture/Database-Ownership.md`'de de vardı ve [[00-Home\|INDEX-0001]] ARCH-0003'ü Database-Ownership'e bağlıyor. **Bu denetimde `ARCH-0007`'ye alındı** — kimlik çakışması bir defekttir, karar değil | frontmatter | Düzeltildi |
| **B2** | `decision_refs` alanı ADR yerine `CURRENT-0005`/`CURRENT-0006` içeriyor; şablon ADR bekliyor | frontmatter · `99-Templates` | Biçim |
| **B3** | §5A: *"`ptn-test-module` … `Volo.Abp.Identity` ve `Volo.Abp.TenantManagement` paketlerini kendi üzerine yükler"*, *"UI `apiClient.get('/api/identity/users')` … doğrudan **Test Module Host'una** gidecek"*. **Yanlış:** Test Module bir resource server'dır, `Authenticator.HttpApi` compose etmez ve runtime kataloğunda auth controller sayısı **0**'dır | ADR-0013 · CURRENT-0005 | **Mimari çelişki** |
| **B4** | §4 uç matrisi bugün var olmayan uçları hedef gibi listeliyor: `/api/recipients`, `/api/email/sender`, `/api/comparison-recipients`, `/api/email/notification-settings`, `/api/runs/comparison-runs/*`, `/api/operators`, `/api/multi-tenancy/*`. Bunlar **eski UI envanteridir**; §5 zaten "sökülmüştür" diyor ama §4 aksini ima ediyor | CURRENT-0005 | Okuma tuzağı |
| **B5** | §2: *"tek `gen:api` scripti … tüm platformun DTO'larını tek `schema.d.ts`'e indirir"*. Bugün **üç köken** var ve ajan yüzeyi OpenAPI **yayınlamıyor** | ADR-0023 §B · ARCH-0006 §0 | Eskimiş varsayım |

**Öneri:** ARCH-0007'nin başına, belgenin **eski UI analizi** olduğunu ve hedef mimarinin
ARCH-0006 + ADR-0025 olduğunu söyleyen bir kapsam kutusu eklenmeli. Bu denetim belgeyi
**yeniden yazmadı**; sahibinin kararıdır.

## C. Ajan runtime — ADR-0023'ün sapma notu artık kendisi eski

ADR-0023 §D'de 2026-08-16 tarihli bir `WARNING` var: *"`ptn-test-agent` içinde
`OpenAIModelAdapter` **yoktur**. Kurulan adapter'lar `OllamaModelAdapter` (yerel qwen3:8b)
ve `FakeModelAdapter`'dır; `openai` SDK'sı bağımlılık listesinde değildir."*

**Ölçüm:**

| İddia | Gerçek (2026-08-17) |
|---|---|
| `OpenAIModelAdapter` yok | `src/model/openai-model-adapter.ts` **var** |
| `OllamaModelAdapter` kullanılıyor | Ollama adapter'ı **yok** |
| `openai` SDK bağımlılıkta değil | `package.json` → `"openai": "7.4.0"` **var** |
| `OPENAI_API_KEY` uygulanmıyor | `config.ts` `OPENAI_API_KEY`'i **zorunlu** kılıyor |

| # | Bulgu | Sınıf |
|---|---|---|
| **C1** | Kod ADR'ye **yakınsadı**; sapma notu kaldırılmalı veya "kapandı" olarak işaretlenmeli. Şu hâliyle ajan tarafını devralan ekip yanlış runtime bekler | Kayıt düzeltmesi |
| **C2** | ADR-0023 §A `>=24 <25` ve pnpm 11.19.0 diyor; `package.json` **birebir uyuyor**. Sapma yok | ✅ |
| **C3** | Doğrulama durumu: `tests/` klasörü hâlâ **yok** (`package.json` `vitest` script'i tanımlı ama test dosyası yok). CURRENT-0001'in "test yok" satırı **hâlâ doğru** | Test borcu |

## D. Ajan güvenlik iddiası ile kod arasındaki fark

RESEARCH-0012 §4.6 şunu yazıyor: *"Kiracı sızıntısı → Ajan oturum açan kullanıcı adına
çalışır; ABP izinleri ve tenant filtresi geçerli."*

| # | Bulgu | Kanıt | Sınıf |
|---|---|---|---|
| **D1** | Ajan HTTP yüzeyinde **hiçbir uçta** kimlik doğrulaması yok | `src/http/create-server.ts` — beş uçta da authorization okuması yok | **Security** |
| **D2** | MCP'ye **tek paylaşılan** bearer ile bağlanılıyor; çağıranın kimliği taşınmıyor | `config.ts` `PTN_MCP_BEARER_TOKEN` (process env) · `server.ts` tek `SdkMcpGateway` | **Security — tenant izolasyonu** |
| **D3** | Oturum belleğe yazılıyor (`Map`), kalıcılık ve liste ucu yok → denetim ve token faturalandırması ölçülemiyor | `src/session/session-store.ts` | Ürün kararı (CURRENT-0007 S-1) |

RESEARCH-0012 kanonik değildir; ama §4.6 bir **güvenlik iddiası** olarak okunuyor ve
bugün karşılanmıyor. UI ajan ekranı bu üçü kapanmadan üretime çıkmamalıdır
(ARCH-0005 §8).

## E. Sayım tutarsızlıkları

| # | Konu | Kayıtlarda | Ölçüm |
|---|---|---|---|
| **E1** | Test Module uç sayısı | CURRENT-0005: **54** (KBP-111 öncesi, kendi uyarısı var) · aynı uyarı güncel kapıyı **64** diyor | Kaynak: **65** action; `OutwardSurfaceTests.cs:17` `ExpectedControllerActionCount = 65` |
| **E2** | `agent_sessions` tablosu | RESEARCH-0012 §4.4 onu "9. tablo" olarak öneriyor | ADR-0016 modeli **4 ana + 5 lookup**; Research-Index §4 `agent_sessions` için "**Yok**" diyor. **ADR kazanır**; sohbet kalıcılığı hâlâ **açık ürün sorusudur** |

E1 için CURRENT-0005'in kendi kuralı geçerlidir: **sayılar elle düzeltilmez**, katalog
composition host ayağa kaldırılıp ApiExplorer yeniden okunarak üretilir. Bu denetim o
sayfaya dokunmadı; kaynak-kodu kanıtı ayrı bir sayfada tutuldu
([[04-Architecture/UI-Endpoint-Screen-Matrix|ARCH-0006]]).

## F. Bu denetimde yapılan tek düzeltme

`04-Architecture/UI-Integration-Architecture.md` kimliği `ARCH-0003` → **`ARCH-0007`**.
Gerekçe: iki belge aynı kimliği taşıyamaz ve `00-Home` ARCH-0003'ü Database-Ownership'e
bağlıyor. Bu bir **kimlik defekti** düzeltmesidir; belgenin içeriği, kararları veya
durumu değiştirilmedi.

## G. Sahibine düşen kararlar

| # | Karar | Bağlı bulgu |
|---|---|---|
| 1 | Depo sınırı: ADR-0024 `supersede` mi edilecek, izleme mi geri alınacak? | A1 |
| 2 | Genişlemiş izlemeyle birlikte `NuGet.Config` kimlik bilgisi ne olacak? | A3 |
| 3 | ARCH-0007'ye kapsam kutusu eklenecek mi? | B3–B5 |
| 4 | ADR-0023 sapma notu kapatılacak mı? | C1 |
| 5 | Ajan kimlik doğrulaması hangi yolla çözülecek (ajanda mı, vekilde mi)? | D1–D2, ADR-0025 §E |
| 6 | Sohbet oturumu kalıcı olacak mı? | E2, CURRENT-0007 S-1 |
