---
id: ADR-0002
type: decision
status: accepted
title: Authless checker package boundary
created: 2026-08-11
updated: 2026-08-11
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs: []
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0004
---

# ADR-0002 — Auth’suz checker paket sınırı

## Bağlam

Checker’ların hem Test Module içinde hem başka ortak composition hostlarda kullanılabilmesi gerekiyor. Auth ve notification sahipliğini her checker’a gömmek aynı issuer, tablo ve runtime sorumluluğunu tekrarlar.

## Karar

İki checker bağımsız ABP capability paketidir. Authenticator ve Notifications implementasyonları paket sınırının dışındadır. Controller, AppService, Manager, Repository, EF Core ve migration yetenekleri pakette kalır.

## Alternatifler

- Her checker’ı tam bağımsız SaaS hostu olarak paketlemek: çifte auth/DB/issuer sahipliği doğurur.
- Yalnız domain motorunu paketlemek: consumer tarafında HTTP, persistence ve migration davranışını yeniden kurdurur.

## Sonuçlar ve riskler

Consumer host module graph, permission bağlamı, connection ve migration orchestration sağlar. Checker paketi tekrar kullanılabilir kalır; composition doğrulaması consumer sorumluluğudur.
