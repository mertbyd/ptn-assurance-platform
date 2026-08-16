---
id: RULE-0002
type: rule
status: active
title: Database schema and migration ownership
updated: 2026-08-13
severity: mandatory
scope: platform
sources: []
decision_refs:
  - ADR-0002
  - ADR-0005
  - ADR-0012
rule_refs: []
---

# RULE-0002 — DB şema ve migration sahipliği

## Kural

Şema sabitleri, `DbProperties` ve ortam override anahtarları kaldırılmaz. Her checker yalnız kendi iş tablolarını ve migrationlarını sahiplenir. Authenticator, Notifications/Emailing veya ABP Identity/OpenIddict tabloları checker migrationı tarafından ikinci kez oluşturulmaz.

Tek composition migrator, Authenticator'ın paketlenmiş migration assembly'sini uygular;
Test Module veya checker projeleri Auth entity'lerinden ikinci migration üretmez. Foundation
ortak base paketidir ve Auth tablo/migration sahipliğini devralmaz.

## Mevcut şema sözleşmeleri

- API Contract Checker: `operator`, `checker`, `email`
- Database Checker: `lookup`, `connection`, `definition`, `run`, `operator`, `comparison`, `email`

Bu adlar varsayılandır; consumer host kontrollü configuration ile değiştirebilir. Bir şemanın varlığı, o şemadaki her tablonun checker tarafından sahiplenildiği anlamına gelmez.

## Doğrulama

- EF configuration’lar ilgili `DbProperties` değerini kullanır.
- EF model değişikliğinde migration üretilir ve `Up/Down` gövdesi okunur.
- Consumer migration smoke testinde aynı tabloyu oluşturan ikinci migration bulunmaz.
- Migration history tablosu/assembly seçimi deterministiktir.

## İstisna süreci

Tablo sahipliği ancak composition mimarisi ve geri alma planını açıklayan yeni ADR ile taşınabilir.
