---
id: CURRENT-0006
type: current
status: active
title: Eski UI İş Hacimleri ve API Kullanım Haritası
updated: 2026-08-16
---

# Eski UI İş Hacimleri ve API Kullanım Haritası

Bu sayfa, eski bağımsız modül arayüzlerinde (UI) iş hacimlerinin (business volume) ve API uçlarının nasıl kullanıldığını belgeler. (Auth, Tenant veya Email gibi cross-cutting uçlar hariç tutulmuştur.) Bu doküman, arayüzlerin CheckNexus ve Ptn.TestModule hostuna (Assurance Platform) göçü sırasında yol gösterici olması amacıyla oluşturulmuştur.

## 1. Database Checker Admin UI

Bu modül, veritabanı şema keşfi ve karşılaştırma süreçlerini yönetir. Temel iş akışları ve kullandığı servisler şunlardır:

### Temel Servisler ve Kullanım Alanları
*   **`connections.service.ts`**:
    *   **Kullanıldığı Yerler:** `connections-workspace.tsx`, `dashboard-overview.tsx`, `schema-comparison-workspace.tsx`, `schema-discovery-workspace.tsx`
    *   **İşlevi:** Hedef ve kaynak veritabanı bağlantı dizesi tanımlarının (connection string) yönetilmesi, panoda (dashboard) listelenmesi ve keşif/karşılaştırma operasyonlarında kaynak/hedef olarak seçilmesi.
*   **`runs.service.ts`**:
    *   **Kullanıldığı Yerler:** `runs-workspace.tsx`, `dashboard-overview.tsx`, `schema-comparison-workspace.tsx`
    *   **İşlevi:** Önceden çalıştırılmış şema karşılaştırma görevlerinin (runs) listelenmesi, özet durumlarının (başarılı, başarısız, farklılık bulundu vb.) dashboard'da ve özel çalışma alanında (runs workspace) görüntülenmesi.
*   **`definitions.service.ts`** & **`schema-comparison.service.ts`**:
    *   **Kullanıldığı Yerler:** `schema-comparison-workspace.tsx`
    *   **İşlevi:** İki veritabanı arasındaki (örneğin dev ile prod) farklılıkların bulunması, tablo/sütun/indeks farklılık raporlarının oluşturulması ve bu karşılaştırma tanımlarının kaydedilip çalıştırılması.
*   **`schema-discovery.service.ts`**:
    *   **Kullanıldığı Yerler:** `schema-discovery-workspace.tsx`, `schema-comparison-workspace.tsx`
    *   **İşlevi:** Bir veritabanı bağlantısına gidilerek mevcut tablo ve sütun yapılarının anlık olarak (discovery) çıkarılıp UI üzerinde sunulması.

## 2. API Contract Checker Admin UI

Bu modül, OpenAPI (Swagger) kontratlarının izlenmesi, snapshot (anlık görüntü) alınması ve değişikliklerin tespiti (checks) süreçlerini yönetir.

### Temel Servisler ve Kullanım Alanları
*   **`sources.api.ts`**: (En yoğun kullanılan API ucu)
    *   **Kullanıldığı Yerler:** `sources-page-view.tsx`, `source-card.tsx`, `monitoring-dialog.tsx`, `document-contract-workspace.tsx`, `check-report-screen.tsx`, `comparison-review-step.tsx`, `snapshot-route-resolver.tsx` vb.
    *   **İşlevi:** API spesifikasyon kaynaklarının (Swagger URL'leri, JSON dokümanları vb.) eklenmesi, izleme (monitoring) konfigürasyonlarının yapılması ve kontroller sırasında karşılaştırma kaynağı olarak seçilmesi.
*   **`snapshots.api.ts`**:
    *   **Kullanıldığı Yerler:** `use-snapshot-query.ts`, `check-report-screen.tsx`, `snapshot-selection-card.tsx`, `snapshot-timeline-picker.tsx`
    *   **İşlevi:** İzlenen bir kaynaktan geçmiş bir zamanda alınmış spesifikasyon görüntüsünün (snapshot) getirilmesi. Geçmiş kontrat ile güncel kontrat arasında farklılık analizi yapılırken baz noktası sağlar.
*   **`checks.api.ts`**:
    *   **Kullanıldığı Yerler:** `use-check-queries.ts`, `check-history-panel.tsx`, `check-status-panel.tsx`, `differences-view.tsx`, `recent-checks.tsx`, `report-breakdown.tsx`
    *   **İşlevi:** Kontrat karşılaştırmasının (check) tetiklenmesi, çalışan bir testin durumunun sorgulanması (status) ve test sonucunda ortaya çıkan endpoint farklılıklarının (finding - yeni endpoint eklendi, parametre değişti vs.) UI'da farklılık kartları/listeleri halinde raporlanması.
*   **`lookups.api.ts`**:
    *   **Kullanıldığı Yerler:** `lookups-page-view.tsx`, `lookup-form-dialog.tsx`, `lookup-list.tsx`
    *   **İşlevi:** Sistemde statik veya tanımlanabilir liste değerlerinin (lookups) yönetimi.
