---
id: ARCH-0003
type: current
status: active
title: Database schema and migration ownership
updated: 2026-08-13
decision_refs:
  - ADR-0002
  - ADR-0005
  - ADR-0012
rule_refs:
  - RULE-0002
---

# DB, şema ve migration sahipliği

## Temel model

Tek DB, tek şema demek değildir. Tek connection/database içinde her capability sahibi belli şemalar kullanabilir. Aynı tablo iki module migrationı tarafından oluşturulamaz.

| Alan | Varsayılan şemalar | Migration sahibi |
|---|---|---|
| API Contract Checker iş alanı | `checker` | API Contract Checker EF Core |
| API checker operator projeksiyonu | `operator` | Sahiplik consumer modelde doğrulanır; Identity tablosu değildir |
| Database Checker | `lookup`, `connection`, `definition`, `run`, `comparison` | Database Checker EF Core |
| DB checker operator projeksiyonu | `operator` | Sahiplik consumer modelde doğrulanır; Identity tablosu değildir |
| Test Module lookup'ları | `test_lookup` | Test Module EF Core — **5 lookup** ([[03-Decisions/ADR-0016-Kayit-Ve-Teshis-Veri-Modeli\|ADR-0016]]) |
| Test Module tanım dünyası | `test_catalog` | Test Module EF Core — **tek tablo:** `test_scenarios` (her satır bir sürüm) |
| Test Module koşum dünyası | `test_run` | Test Module EF Core — `test_runs`, `test_run_results`, `test_result_findings`; 90 gün saklama |
| Ortak email sözleşmesi | `email` | Notifications/Emailing owner; checker ikinci kez oluşturmaz |
| Identity/OpenIddict/tenant | Authenticator kararı (`abp`, `openiddict`, vb.) | Authenticator migration assembly'si; tek composition migrator uygular |

## Consumer composition ilkeleri

1. Her module kendi `DbProperties` değerini configuration’dan alır.
2. Tek migrator migration assembly’lerini açık ve deterministik sırayla çalıştırır.
3. Migration history tablo adları çakışmaz.
4. Package update migration gerektiriyorsa consumer release notunda uygulanma sırası belirtilir.
5. Aynı entity setinin iki DbContext’te owner olarak configure edilmesi engellenir.
6. Migration smoke gerçek provider üzerinde çalıştırılır ve üretilen SQL/model okunur.
7. Consumer Auth entity'lerinden migration üretmez; paketlenmiş Auth migration assembly'sini
   uygular. Foundation base paketleri Auth migration sahibi sayılmaz.

Şema değerlerini “temizlik” amacıyla kaldırmak modülün hem Test Module hem ortak host içinde kullanılabilme sözleşmesini bozar.

## Test Module modeli — 4 ana tablo + 5 lookup

Şema kaynağı: `04-Architecture/Test-Platform-Schema.dbml`. Toplam **9 tablo**
(eski 9 ana + 14 lookup modeli ADR-0016 ile yerine geçirilmiş, ADR-0011 silinmiştir).

```
test_lookup (5)              test_catalog (1)        test_run (3)
├ test_run_statuses          └ test_scenarios   ──►  ├ test_runs
├ test_outcome_statuses         (her satır bir       ├ test_run_results
├ test_failure_categories        sürümdür)           └ test_result_findings
├ test_trigger_kinds
└ test_scenario_states
```

**Ortam bağlaması tablo değildir:** mantıksal ad → adres eşlemesi ABP tenant-scoped `Setting`
olarak tutulur, koşum anında çözülür ve `test_runs` satırına snapshot olarak düşer
(ADR-0016 §G).

**Bölümleme yoktur.** Bölümlenmiş tablonun birincil anahtarı bölümleme kolonunu içermek
zorundadır; bu ABP'nin tek kolonlu `Guid` anahtar sözleşmesini kırar. Yerine zamanlanmış
parçalı silme.
