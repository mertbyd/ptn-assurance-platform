---
id: RULE-0003
type: rule
status: active
title: Vault and secret boundary
updated: 2026-08-11
severity: mandatory
scope: platform
sources: []
decision_refs:
  - ADR-0004
rule_refs: []
---

# RULE-0003 — Vault ve secret sınırı

## Kural

Secret değerleri source code, `appsettings`, DB kolonları, DTO, log, exception metni, test fixture veya wiki içine yazılmaz. Checker’lar secret-store portunu tanımlar; gerçek providerı composition host seçer.

## Gerekçe

Tek adapter kod tekrarını önler. Ayrı path/policy ise capability ve tenant izolasyonunu korur.

## Doğrulama

- Tek `VaultSecretProvider` iki checker portuna kaydedilir.
- Username/password ve header name/value çiftleri all-or-nothing doğrulanır.
- Hata mesajı secret payloadını içermez.
- Local token yalnız güvenli enjeksiyon veya token file yoluyla verilir.

## İstisna süreci

Yeni provider eklenebilir; secret görünürlük ve sahiplik kuralı esnetilemez.
