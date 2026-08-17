---
id: ARCH-0007
type: architecture
status: active
title: Birleşik UI Portalı ve API Entegrasyon Mimarisi (Derin Analiz ve Uç Matrisi)
updated: 2026-08-17
decision_refs:
  - CURRENT-0005
  - CURRENT-0006
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0003
  - RULE-0004
---

# Birleşik UI Portalı ve API Entegrasyon Mimarisi (Derin Analiz)

> [!WARNING] Kapsam — bu belge **eski UI analizidir**, hedef mimari değildir (2026-08-17)
> Kimliği `ARCH-0003` idi ve `04-Architecture/Database-Ownership.md` ile çakışıyordu;
> **`ARCH-0007`'ye alındı** (içerik değişmedi —
> [[90-Inbox/AUDIT-0004-Ui-Oncesi-Wiki-Gerceklik-Denetimi|AUDIT-0004]] §F).
>
> Bugün geçerli hedef mimari şudur:
> **ekran–uç–izin matrisi** [[04-Architecture/UI-Endpoint-Screen-Matrix|ARCH-0006]] ·
> **ajan yüzeyi** [[04-Architecture/UI-Agent-Experience|ARCH-0005]] ·
> **yığın ve köken sınırı** [[03-Decisions/ADR-0025-Ui-Yigini-Ve-Uc-Kokenli-Yuzey-Siniri|ADR-0025]] ·
> **gereksinimler** [[01-Current/UI-Requirements-Truth|CURRENT-0007]].
>
> Aşağıdaki üç bölüm **eskidir** ve olduğu gibi uygulanamaz (AUDIT-0004 §B):
> - **§2** "tek `gen:api`, tek `schema.d.ts`" — bugün **üç köken** var ve ajan yüzeyi
>   OpenAPI yayınlamıyor.
> - **§4** uç matrisi eski UI envanteridir; `/api/recipients`, `/api/email/sender`,
>   `/api/comparison-recipients`, `/api/runs/comparison-runs/*`, `/api/operators` ve
>   `/api/multi-tenancy/*` **bugünkü runtime kataloğunda yoktur** (CURRENT-0005).
> - **§5A** Test Module'ün `Volo.Abp.Identity`/`TenantManagement` yüklediğini ve UI'nin
>   `/api/identity/users`'ı Test Module hostuna göndereceğini söyler. **Yanlıştır:**
>   Test Module bir **resource server**dır, auth controller sayısı **0**'dır ve kimlik
>   ekranları `<AUTH_ORIGIN>`'e gider ([[03-Decisions/ADR-0013-Test-Module-Resource-Server-Auth-Consumption|ADR-0013]]).
>
> Belgenin kalıcı değeri **§1 ve §3**'tedir: iki eski UI'ın paradigma farkı ve feature
> izolasyonu kuralı. Bunlar hâlâ geçerlidir.

Bu belge, bağımsız modüllerden (`CheckNexus.ApiContracts`, `CheckNexus.DatabaseComparison`) beslenen ve `Ptn.TestModule` hostunda birleşen Assurance Platform'un, kullanıcı arayüzünde (UI) kod ve mimari seviyesinde nasıl entegre edileceğini tanımlar.

## 1. Eski UI'lar Arasındaki Paradigma Farkı ve Birleşme Stratejisi

Eski `database-checker-admin-ui` (Codex/2026-07-06) ve yeni `ptn-api-contract-checker-admin-ui` projeleri incelendiğinde, API tüketim katmanında kritik bir farklılık vardır:
*   **Database Checker UI:** API servisleri generic nesneler olarak elle yazılmış (`createCrudService`, `createReadService`) factory fonksiyonlarıyla oluşturulmuştur. DTO tipleri manuel olarak TypeScript'te tanımlanmıştır.
*   **API Contract Checker UI:** Kesin kural (RULE-0001) gereği tipler elle yazılmaz. `openapi-typescript` kullanılarak Swagger JSON'dan `components["schemas"]` üzerinden otomatik üretilir. İstekler TanStack React Query ile, state'ler Zustand ile yönetilir.

**Yeni Birleşik UI Stratejisi:** Yeni UI, güncel ve daha katı standartlara sahip olan **API Contract Checker UI** kurallarını baz alacaktır. Database Checker ekranları yeni sisteme taşınırken eski generic servis yapısı tamamen terk edilecek, tüm API uçları (aşağıdaki kapsamlı matriste belirtilenler) OpenAPI kod üretimine geçirilecektir.

## 2. API Kod Üretimi ve Tekil Swagger Bağlamı

1.  **Tek `gen:api` Scripti:** UI projesi, `openapi-typescript http://localhost:5000/swagger/v1/swagger.json -o ./src/api/generated/schema.d.ts` komutunu çalıştırarak tüm platformun DTO'larını tek bir `schema.d.ts` dosyasına indirir.
2.  **Tekil API İstemcisi:** Tüm uçlar aynı `ResultEnvelope<T>` zarfıyla yanıt döner. İstemci (Axios), RULE-0002 gereği bu zarfı sadece interceptor'da açar.

## 3. UI Bileşen (Feature) İzolasyonu ve 3 Seçenekli Portal

Kod seviyesindeki izolasyon, `src/features/*` (RULE-0004) kuralına dayandırılacaktır. Birleşmiş uygulamanın App Router rotaları şu şekilde tasarlanmalıdır:
*   `src/app/(portal)/api-contract-checker/`
*   `src/app/(portal)/database-checker/`
*   `src/app/(portal)/test-module/`

---

## 4. Kapsamlı API Uçları Matrisi (Tüketilen Kaynaklar)

Her iki eski UI da son derece geniş kapsamlı bir API yüzeyini tüketmektedir. Eski sistemlerde kullanılan ve yeni platforma Typescript üretimi ile bağlanacak olan **tüm aktif API uçlarının detaylı haritası** aşağıdadır. Hiçbir uç atlanmamıştır.

### A. API Contract Checker'ın Tükettiği Uçlar
*   **Sources (Kaynak Yönetimi):**
    *   `POST /api/sources`, `GET /api/sources/{id}`, `GET /api/sources` (List)
    *   `POST /api/sources/{id}/passivate`, `PUT /api/sources/{id}`
    *   `POST /api/sources/{id}/test` (Reachability Test)
    *   `POST /api/sources/{id}/documents/{documentId}/monitoring`
    *   `POST /api/sources/{id}/documents/{documentId}/snapshot`
    *   `GET /api/sources/{id}/documents/{documentId}/snapshots` (Geçmiş belge anlık görüntüleri)
*   **Snapshots (Anlık Görüntü Detayları):**
    *   `GET /api/snapshots/{id}`
*   **Checks (Test Koşumları):**
    *   `POST /api/checks`, `GET /api/checks`, `GET /api/checks/{id}`
    *   `GET /api/checks/{id}/status`, `GET /api/checks/{id}/report`
*   **Email & Recipients:**
    *   `GET /api/recipients`, `POST /api/recipients`, `GET /api/recipients/{id}`
    *   `PUT /api/recipients/{id}`, `POST /api/recipients/{id}/passivate`
    *   `GET /api/email-templates`, `POST /api/email-templates`, `PUT /api/email-templates/{id}`, `DELETE /api/email-templates/{id}`
    *   `GET /api/email/sender`, `PUT /api/email/sender`, `DELETE /api/email/sender`, `POST /api/email/sender/test`
*   **Lookups:**
    *   `/api/lookups/spec-formats`, `/api/lookups/difference-directions`, `/api/lookups/difference-kinds`, `/api/lookups/difference-severities`, `/api/lookups/check-run-statuses`

### B. Database Checker'ın Tükettiği Uçlar
*   **Database Connections (Bağlantı Yönetimi):**
    *   `GET /api/connections/database-connections`, `POST /api/connections/database-connections`
    *   `GET /api/connections/database-connections/{id}`, `PUT /api/connections/database-connections/{id}`, `DELETE /api/connections/database-connections/{id}`
    *   `POST /api/connections/database-connections/{id}/passivate`, `POST /api/connections/database-connections/{id}/test-connection`
*   **Schema Discovery (Keşif ve Önizleme):**
    *   `GET /api/comparison/schema-discovery/{connectionId}/schemas`
    *   `GET /api/comparison/schema-discovery/{connectionId}/objects`
    *   `GET /api/comparison/schema-discovery/{connectionId}/snapshot`
*   **Schema Comparison & Definitions (Tanımlar ve Karşılaştırma):**
    *   `GET /api/definitions/comparison-definitions`, `POST /api/definitions/comparison-definitions`, vb.
    *   `POST /api/comparison/schema-comparison`
*   **Runs (Çalıştırılan Karşılaştırmalar):**
    *   `GET /api/runs/comparison-runs`, `POST /api/runs/comparison-runs/execute`
    *   `GET /api/runs/comparison-runs/{id}/detail`, `GET /api/runs/comparison-runs/{id}/report`
    *   `POST /api/runs/comparison-runs/{id}/resend-email`
*   **Email & Notifications (Veritabanı İzleme):**
    *   `GET /api/comparison-recipients`, `POST /api/comparison-recipients`, vb.
    *   `GET /api/email/notification-settings`, `PUT /api/email/notification-settings`
*   **Lookups:**
    *   `/api/lookups/database-engines`, `/api/lookups/comparison-types`, `/api/lookups/comparison-run-statuses`, `/api/lookups/scope-kinds`, `/api/lookups/schema-object-types`, `/api/lookups/difference-kinds`, `/api/lookups/comparison-confidences`, `/api/lookups/report-formats`

### C. Ortak (Tenant, Kullanıcı ve Rol Yönetimi) Uçlar
Her iki uygulama da arka planda aynı **ABP Framework Kimlik / Tenant** uçlarını kullanır.
*   **Multi-Tenancy:**
    *   `GET /api/multi-tenancy/tenants`, `POST /api/multi-tenancy/tenants`
    *   `GET /api/abp/multi-tenancy/tenants/by-name/{name}`
    *   `GET /api/multi-tenancy/tenants/{tenantId}/users` (Tenant bazlı izole kullanıcı yönetimi)
*   **Identity & Permissions:**
    *   `GET /api/identity/users`, `POST /api/identity/users`, `PUT /api/identity/users/{id}`, `DELETE /api/identity/users/{id}`
    *   `GET /api/identity/roles`, `GET /api/identity/roles/all`
    *   `GET /api/permission-management/permissions`, `PUT /api/permission-management/permissions`
    *   `GET /api/operators`

---

## 5. Çıkarılan Çapraz Kesit (Cross-Cutting) Modüllerin Yönetimi: Auth, Tenant ve Email

Eski UI projelerindeki geniş API matrisinde görüldüğü üzere, her iki Checker uygulaması da kendi içinde *Kimlik, Tenant ve E-posta* (Identity, Multi-Tenancy, Email) API'lerini doğrudan tüketmekteydi. Ancak yeni CheckNexus mimarisinde (ve `checkers` klasöründeki güncel paketlerde) bu özellikler **tamamen sökülmüştür.**

Bunun nedeni, bu özelliklerin Checker'ların (domain) değil, **Ana Platformun (Test Module / Assurance Platform)** sorumluluğu olmasıdır. Yeni UI entegrasyonunda bu sökülen modüller şu şekilde kullanılacaktır:

### A. Auth, Identity ve Tenant Yönetimi
*   **Eski Durum:** Her modül kendi `/api/identity` veya `/api/multi-tenancy` uçlarını barındırıyor ve kendi veritabanında saklıyordu.
*   **Yeni Durum:** `ptn-test-module` kompozisyon hostu, ABP'nin standart `Volo.Abp.Identity` ve `Volo.Abp.TenantManagement` paketlerini kendi üzerine yükler (Host).
*   **UI Entegrasyonu:** UI, yine `apiClient.get('/api/identity/users')` çağrısını yapmaya devam edecektir. Ancak bu istek artık API Contract veya DB Checker modüllerine değil, doğrudan **Test Module Host'una** gidecek ve oradaki merkezi kimlik deposundan (Authenticator / Core) yanıtlanacaktır. UI tarafındaki tüm Kullanıcı (User), Rol (Role) ve Şirket (Tenant) ekranları, doğrudan ana Test Modülü Swagger tanımı üzerinden çalışmaya devam edecektir. 

### B. Email ve Bildirim Yönetimi
*   **Eski Durum:** API Contract Checker `/api/recipients` ve `/api/email-templates`, DB Checker ise `/api/comparison-recipients` ve `/api/email/notification-settings` uçlarına sahipti.
*   **Yeni Durum:** Checker'ların içinden doğrudan SMTP üzerinden e-posta atma sorumluluğu çıkartılmıştır. Test Modülü platformu, ortak bir **Notification (Bildirim) / Authoring** altyapısı sunar.
*   **UI Entegrasyonu:** Yeni UI projesinde, *"Koşum bittiğinde kimlere mail gitsin?"* yeteneği artık `checkers` paketlerinden değil, Test Modülü'nün merkezi altyapısı üzerinden yönetilecektir. Yeni e-posta veya bildirim ayarlama uçları (gelecekte eklenecek olan `/api/test-module/notifications` vb. uçlar) kullanılarak, farklı checker'ların (ister API ister DB) uyarıları ortak bir mesajlaşma/olay (event) kuyruğundan gönderilecektir. UI, e-posta ayarlarını modüllerin kendi içinden değil, portalın **Genel Ayarlar (Test Module Core)** menüsü altından yönetecektir.

Özetle, sökülen bu modüller **kaybolmamış, ortak platform hostuna (Test Module) taşınmıştır.** UI açısından tek değişen şey, bu istekleri atarken mantıksal olarak modül sayfalarının altında değil, "Sistem/Platform Ayarları" adı altında merkezi menülere yerleştirilmeleri gerektiğidir.
