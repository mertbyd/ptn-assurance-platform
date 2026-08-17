---
id: ARCH-0006
type: architecture
status: active
title: UI ekran-uc-izin matrisi — hangi ekran hangi ucu hangi izinle cagirir
updated: 2026-08-17
decision_refs:
  - ADR-0013
  - ADR-0016
  - ADR-0020
  - ADR-0023
rule_refs:
  - RULE-0001
  - RULE-0004
---

# UI ekran–uç–izin matrisi

> [!IMPORTANT] Kanıt sınıfı — bu sayfa CURRENT-0005'in yerine geçmez
> [[01-Current/UI-Backend-Controller-Catalog|CURRENT-0005]] **runtime** kanıtıdır
> (`IApiDescriptionGroupCollectionProvider`) ve Test Module için **54** uç sayar; o sayım
> KBP-111 öncesine aittir ve elle düzeltilmez. Bu sayfa **kaynak kodu** kanıtıdır: 17
> controller dosyasının `[Http*]` + `[Authorize]` attribute'ları ve `Domain.Shared`
> route sabitleri okunarak üretildi (2026-08-17). Test Module tarafı **65 action**tır ve
> `OutwardSurfaceTests.ExpectedControllerActionCount = 65` ile birebir tutar.
>
> Checker, Emailing, Notifications ve ABP tarafı için tek kanıt hâlâ CURRENT-0005'tir;
> bu sayfa onları **tekrarlamaz**, ekrana bağlar.

## 0. Üç köken

UI hiçbir port veya localhost değeri sabitlemez.

| Köken | Ne sunar | Zarf | Kimlik |
|---|---|---|---|
| `<TEST_MODULE_ORIGIN>` | Test Module 65 + checker/Emailing/Notifications/ABP uçları | `Result<T>` · `PagedResultDto<T>` | Bearer (ABP izinleri) |
| `<AGENT_ORIGIN>` | 5 ajan ucu, biri SSE | Düz JSON · `text/event-stream` | **Bugün yok** — ARCH-0005 §8 boşluk 1 |
| `<AUTH_ORIGIN>` | login · refresh · logout · selected-context · tenant · organization-unit | Authenticator sözleşmesi | OIDC |

> [!CAUTION] Auth çağrısını Test Module köküne yönlendirmek yanlıştır
> Test Module bir **resource server**dır; `Authenticator.HttpApi` compose edilmez ve bu
> hostta **0** auth controller'ı vardır (ADR-0013). Kullanıcı, rol ve tenant ekranları
> `<AUTH_ORIGIN>`'e gider. [[04-Architecture/UI-Integration-Architecture|ARCH-0007]] §5A bunun
> tersini söyler ve **eskidir** — [[90-Inbox/AUDIT-0004-Ui-Oncesi-Wiki-Gerceklik-Denetimi|AUDIT-0004]] B3.

---

## 1. Portal iskeleti — dört alan

```
(portal)
├── /assurance          Panolar, koşum, bulgu, sağlık        ← Test Module runs
├── /authoring          Yükleme → sohbet → adım → yayın      ← Test Module authoring + AGENT
├── /api-contract       Kaynak, snapshot, check, uygunluk    ← API Contract Checker
└── /database           Bağlantı, keşif, karşılaştırma       ← Database Checker
    /settings           Ortam, lookup, profil paketi, e-posta
```

Kod izolasyonu `src/features/*`; rota izolasyonu yukarıdaki dört segment.
Ayrıntı: [[05-Operations/UI-Build-Guide|GUIDE-0006]].

---

## 2. Test Module — 65 uç, ekran bazında

### 2.1 Yazarlık oturumu — `AuthoringSessionController` (5)

| Ekran | Method | Route | İzin |
|---|---|---|---|
| Yazarlık sihirbazı — başlat | `POST` | `api/test-module/authoring/sessions` | `TestModule.Scenarios.Create` |
| Yazarlık sihirbazı — durum | `GET` | `…/sessions/{id}` | `TestModule.Scenarios.Update` |
| Kapalı soru kartı | `POST` | `…/sessions/{id}/answer` | `TestModule.Scenarios.Update` |
| Adım onayı — API | `POST` | `…/sessions/{id}/step` | `TestModule.Scenarios.Update` |
| Adım onayı — DB | `POST` | `…/sessions/{id}/database-step` | `TestModule.Scenarios.Update` |

Swagger grubu `test-module-authoring`. `AuthoringSessionDto` UI'nin tek durum kaynağıdır:
`Questions`, `Answers`, `Steps`, `DatabaseSteps`, `SourceDocument`, `TtlMs`.

### 2.2 Yazarlık malzemesi — `AuthoringSourceController` (3)

| Ekran | Method | Route | İzin |
|---|---|---|---|
| Kural yükleme | `POST` | `api/test-module/authoring/business-rules` | `TestModule.Bridge.ManageSources` |
| Profil paketi yükleme | `POST` | `api/test-module/authoring/profile-packs` | `TestModule.Bridge.ManageSources` |
| Profil paketi listesi | `GET` | `api/test-module/authoring/profile-packs` | `TestModule.Bridge.ManageSources` |

`ProfilePackSummaryDto` kapsama sayaçlarını verir: `BindingCount` /
`ApprovedBindingCount` / `EvidencePathCount`. **Ekran bu üçünü oran olarak gösterir** —
"kavramın ne kadarı şemaya bağlı" sorusunun tek cevabı budur.

### 2.3 Köprü — `PtnBridgeController` (9) + `BusinessInvariantController` (1)

| Ekran | Method | Route | İzin |
|---|---|---|---|
| Zemin (aday operasyon/tablo) | `POST` | `api/test-module/bridge/ground` | `TestModule.Bridge.Ground` |
| Teşhis anlatımı | `POST` | `…/bridge/explain` | `TestModule.Bridge.Explain` |
| Yayın öncesi doğrulama | `POST` | `…/bridge/validate` | `TestModule.Bridge.Validate` |
| Kavram sözlüğü | `POST` | `…/bridge/knowledge` | `TestModule.Bridge.Knowledge` |
| Tool rozetleri | `GET` | `…/bridge/tools` | `TestModule.Bridge.Knowledge` |
| An profili (bütçe göstergesi) | `POST` | `…/bridge/agent-profile` | `TestModule.Bridge.Profile` |
| Bütçe kapısı | `POST` | `…/bridge/tool-budget` | `TestModule.Bridge.Profile` |
| Uzun iş yoklaması | `POST` | `…/bridge/task-status` | `TestModule.Bridge.Task` |
| Yama önerisi (**salt inceleme**) | `POST` | `…/bridge/overlay-suggestion` | `TestModule.Bridge.PatchSuggest` |
| İş değişmezi kontrolü | `POST` | `api/test-module/invariants/check` | `TestModule.Bridge.Invariant` |

> `overlay-suggestion` **kademe 4**'tür. `OverlayPatchSuggestionDto.Applied` alanı önerinin
> uygulanmadığını taşır; UI bu ekranı "uygula" değil **"incele ve dışa aktar"** olarak kurar.

### 2.4 Senaryo kataloğu — `TestScenarioController` (13) + `ScenarioCoverageController` (1)

| Ekran | Method | Route | İzin |
|---|---|---|---|
| Senaryo listesi | `GET` | `api/test-module/scenarios` | `TestModule.Scenarios` |
| Senaryo detayı | `GET` | `…/scenarios/{id}` | `TestModule.Scenarios` |
| Kalıcılaştır | `POST` | `api/test-module/scenarios` | `TestModule.Scenarios.Create` |
| Düzenle | `PUT` | `…/scenarios/{id}` | `TestModule.Scenarios.Update` |
| Sil | `DELETE` | `…/scenarios/{id}` | `TestModule.Scenarios.Delete` |
| **Derleme önizleme** | `POST` | `…/scenarios/compile-preview` | `TestModule.Scenarios.Update` |
| Onaya sun | `POST` | `…/scenarios/{id}/submit-for-approval` | `TestModule.Scenarios.Update` |
| **Kapı değerlendirme** | `POST` | `…/scenarios/{id}/evaluate-publication` | `TestModule.Scenarios.Publish` |
| **Yayınla** | `POST` | `…/scenarios/{id}/publish` | `Scenarios.Approve` **+** `Scenarios.Publish` |
| Kullanımdan kaldır | `POST` | `…/scenarios/{id}/deprecate` | `TestModule.Scenarios.Update` |
| Karantinaya al | `POST` | `…/scenarios/{id}/quarantine` | `TestModule.Scenarios.Quarantine` |
| Karantinayı kaldır | `POST` | `…/scenarios/{id}/quarantine/release` | `TestModule.Scenarios.Quarantine` |
| Zamanlama (cron) | `PUT` | `…/scenarios/{id}/schedule` | `TestModule.Scenarios.Schedule` |
| Kapsam raporu | `GET` | `api/test-module/coverage` | `TestModule.Scenarios` |

**Yayın ekranının tasarım kuralı:** `publish` butonu **asla ilk eylem değildir**. Sıra
`compile-preview` → `evaluate-publication` → (yeşilse) `publish`'tir.
`TestScenarioPublishDecisionDto.FailedGateCodes` beş kapalı koddan gelir ve ekranda
**kod bazında** gösterilir:

| Gate kodu | Ekranda ne yazar | Nasıl düzelir |
|---|---|---|
| `SchemaValidity` | Arazzo şeması geçersiz | `compile-preview` lint çıktısı |
| `Derivability` | Assertion sözleşmeden türetilemiyor | Adımı düzelt veya kaldır |
| `AssertionCount` | Adımda beklenti yok | En az bir beklenti ekle (RULE-0006) |
| `MaterialIntegrity` | Malzeme mührü eksik/bayat | **Bugün elle değer ister** — CURRENT-0001 blokaj 9 |
| `SourceDescriptionConsistency` | Kaynak adresleri tutmuyor | `ApiSourceUrl` / `DatabaseSourceUrl` |

### 2.5 Koşum — `TestRunController` (15)

| Ekran | Method | Route | İzin |
|---|---|---|---|
| Koşum listesi | `GET` | `api/test-module/runs` | `TestModule.Runs.View` |
| Koşum detayı | `GET` | `…/runs/{id}` | `TestModule.Runs.View` |
| Koşum oluştur | `POST` | `api/test-module/runs` | `TestModule.Runs.Trigger` |
| Tetikle (kuyruk) | `POST` | `…/runs/trigger` | `TestModule.Runs.Trigger` |
| Webhook tetikleme | `POST` | `…/runs/webhook` | **`Anonymous`** — paylaşılan sır; sır boşken `403` |
| Claim | `POST` | `…/runs/{id}/start` | `TestModule.Runs.Start` |
| Terminal sonuç yaz | `POST` | `…/runs/{id}/terminal` | `TestModule.Runs.WriteResult` |
| İptal | `POST` | `…/runs/{id}/cancel` | `TestModule.Runs.Cancel` |
| Rapor | `GET` | `…/runs/{id}/report` | `TestModule.Runs.View` |
| **Kuru koşum çelişkisi** | `GET` | `…/runs/{id}/dry-run-contradiction` | `TestModule.Runs.View` |
| HAR gövdesi | `GET` | `…/runs/{id}/har` | `TestModule.Runs.View` |
| İhracat | `POST` | `…/runs/{id}/export` | `TestModule.Runs.Export` |
| Sonuç | `GET` | `…/runs/results/{id}` | `TestModule.Runs.View` |
| Artefakt bağları | `GET` | `…/runs/results/{id}/artifacts` | `TestModule.Runs.View` |
| Artefakt gövdesi | `GET` | `…/runs/results/{id}/artifacts/{format}` | `TestModule.Runs.View` |

`{format}` kapalı kümedir: `Ctrf` · `JUnit` · `Sarif` (`RunArtifactFormatCodes`).

> [!IMPORTANT] `dry-run-contradiction` bir onay ekranıdır, hata ekranı değil
> Kuru koşum kırmızıysa ajana düzeltme yetkisi **verilmez** (RULE-0005, RESEARCH-0012 §3.1).
> Ekran iki seçenek sunar: *"senaryo yanlış"* veya *"bu gerçek bir hata"*. Üçüncü bir
> seçenek — assertion'ı zayıflatmak — UI'da **bulunmaz**.

### 2.6 Ortam, bulgu, sağlık (8)

| Ekran | Method | Route | İzin |
|---|---|---|---|
| Ortam listesi | `GET` | `api/test-module/environments` | `TestModule.Runs.View` |
| Ortam bağla | `POST` | `api/test-module/environments` | `TestModule.Runs.ManageEnvironments` |
| Ortam güncelle | `PUT` | `…/environments/{key}` | `TestModule.Runs.ManageEnvironments` |
| Ortam kaldır | `DELETE` | `…/environments/{key}` | `TestModule.Runs.ManageEnvironments` |
| Sandbox sıfırla | `POST` | `…/environments/{key}/sandbox/reset` | `TestModule.Runs.SandboxReset` |
| Bulgu listesi | `GET` | `api/test-module/findings` | `TestModule.Runs.View` |
| Sağlık listesi | `GET` | `api/test-module/scenario-health` | `TestModule.Runs.View` |
| Sağlık detayı | `GET` | `…/scenario-health/{scenarioKey}` | `TestModule.Runs.View` |

`TestEnvironmentBindingDto` **sır değeri taşımaz**; `api.secretRef` Vault'tan çözülür ve
runner'a tek ortam değişkeni olarak geçer (KBP-112). UI ortam formunda **parola alanı
göstermez**, yalnız referans anahtarı alır.

### 2.7 Lookup okuma (10)

Beş lookup × (`GET` liste + `GET {id}`), hepsi `TestModule.Lookups`:
`run-statuses` · `outcome-statuses` · `failure-categories` · `trigger-kinds` ·
`scenario-states`. **Yazma ucu yoktur** — UI bu beşi salt-okunur gösterir, "yeni ekle"
butonu koymaz.

> Checker lookup'ları tam CRUD'dur ve **ayrı** ekranlardır. Rotalar `0.2.0-alpha.9` ile modül
> önekine taşındı: `api/api-contract/lookups/*` ve `api/database-comparison/lookups/*`.
> Eski `api/lookups/*` yolları **çalışmaz**.

---

## 3. Ajan kökeni — 5 uç

Tam sözleşme: [[04-Architecture/UI-Agent-Experience|ARCH-0005]] §2.

| Ekran | Method | Route | Yanıt |
|---|---|---|---|
| Sohbeti başlat | `POST` | `api/agent/sessions` | `201` JSON |
| Mesaj gönder | `POST` | `api/agent/sessions/{id}/messages` | **SSE** |
| Dosya yükle | `POST` | `api/agent/sessions/{id}/uploads` | `204` |
| İptal | `POST` | `api/agent/sessions/{id}/cancel` | `204` |
| Adım onayı | `POST` | `api/agent/sessions/{id}/approval` | `204` |

---

## 4. Checker ekranları — CURRENT-0005'e bağlanma

| Ekran | Sahip | Kök route | Kanıt |
|---|---|---|---|
| Spec kaynakları / snapshot / check | API Contract Checker | `/api/sources` · `/api/snapshots` · `/api/checks` | CURRENT-0005 §API Contract Checker |
| Uygunluk ve türetilebilirlik | API Contract Checker | `/api/contract-checks/conformance/*` | aynı |
| Sözleşme teşhisi | API Contract Checker | `/api/contract-checks/diagnosis` | aynı |
| DB bağlantı / keşif / karşılaştırma | Database Checker | `/api/connections/*` · `/api/comparison/*` · `/api/definitions/*` | CURRENT-0005 §Database Checker |
| Yazma kümesi yeteneği | Database Checker | `/capabilities/write-set/*` | aynı |
| Canlı bildirim | Notifications | `/api/notifications/*` | aynı |

### 4.1 Çakışan dört imza — kapandı

İki checker `difference-kinds` rotasını ortak `api/lookups` isim alanında paylaşıyordu; Swagger
üretimi düşüyor, rota belirsiz kalıyordu. `0.2.0-alpha.9` ile her aile kendi önekini aldı:

| Aile | Yeni rota |
|---|---|
| API Contract Checker | `api/api-contract/lookups/difference-kinds` |
| Database Checker | `api/database-comparison/lookups/difference-kinds` |

İkisi de artık çağrılabilir; UI hangi ailenin fark türünü gösterdiğini rota önekinden seçer.

### 4.2 E-posta yüzeyi hostta yok

`POST /api/emailing/emails` hiçbir yetki kontrolü taşımıyordu — ne controller'da `[Authorize]`,
ne `EmailAppService.SendAsync`'te `CheckPolicy`. Emailing HTTP modülü bu yüzden artık compose
**edilmiyor** (`3fd78aa`); şablon ve sağlayıcı uçları da hostta yoktur. E-posta ekranı ancak
paket yetkili bir gönderim ucu yayımladıktan sonra planlanır.

---

## 5. Zarf ve hata — tek yerde açılır

| Köken | Başarı (2xx) | Sayfalama | Hata (4xx/5xx) |
|---|---|---|---|
| Test Module + checker | `Result<T>` | `PagedResultDto<T>` | **ABP `RemoteServiceErrorResponse`** — `{ error: { code, message, details, validationErrors } }` |
| Ajan | düz JSON | — | `{ code }` (§3) |

> [!IMPORTANT] Test Module'de hata **`Result<T>` içinde gelmez**
> Kaynak taramasında `Result.Fail(...)` benzeri hiçbir çağrı yoktur; tek üretim yolları
> `T`'den örtük dönüşüm ve dört `Result.NoContent()`'tir. Yani sözleşme şudur:
> **2xx → `Result<T>`**, **2xx dışı → ABP hata nesnesi**. FluentValidation ihlalleri
> `validationErrors` dizisinde, iş hataları `error.code` içinde kararlı hata koduyla gelir.
> RFC 9457 raporu bir **teşhis içeriğidir** (`report` uçlarının gövdesinde), transport hatası
> değildir. UI interceptor'ı bu iki şekli ayrı ayrı eşler.

Zarf **yalnız istemci interceptor'ında** açılır; feature kodu `T`'yi görür. İki kökenin
hata biçimi farklı olduğu için **iki ayrı istemci** kurulur (GUIDE-0006 §3).

## 6. Yenileme kuralı

Test Module tarafı değiştiğinde bu sayfa `Controllers/**` + `Domain.Shared/Constants/**`
yeniden okunarak güncellenir ve `OutwardSurfaceTests.ExpectedControllerActionCount` ile
karşılaştırılır. İki sayı tutmuyorsa **sayfa değil kod** doğrudur; sayfa yeniden üretilir.
Checker tarafı için tek kanıt runtime ApiExplorer'dır (CURRENT-0005).
