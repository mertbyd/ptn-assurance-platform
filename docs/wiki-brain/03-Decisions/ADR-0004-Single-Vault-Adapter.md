---
id: ADR-0004
type: decision
status: accepted
title: Single Vault adapter for checker ports
created: 2026-08-11
updated: 2026-08-11
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs: []
rule_refs:
  - RULE-0003
---

# ADR-0004 — İki checker için tek Vault adapteri

## Bağlam

API credential ve database credential biçimleri farklıdır fakat ikisi aynı secret-store operasyon modelini kullanır. İki SDK/adapter kopyası auth, retry, hata ve config davranışını ayrıştırır.

## Karar

`CheckNexus.Vault` içindeki tek `VaultSecretProvider`, iki checker’ın ayrı `ISecretProvider` arayüzünü uygular. Aynı singleton iki porta explicit olarak kaydedilir. Tek Vault deployment path/policy ile ayrıştırılır.

## Alternatifler

- Checker başına ayrı adapter: kod ve operasyon tekrarı.
- Checker’ın Vault SDK’sına doğrudan bağımlı olması: domain portunu altyapıya bağlar.

## Sonuçlar ve riskler

Vault sözleşmesi consumer hostta tek yerde configure edilir. Policy tasarımı zayıfsa tek deployment geniş erişim riski taşır; çözüm ayrı adapter değil least-privilege policy’dir.
