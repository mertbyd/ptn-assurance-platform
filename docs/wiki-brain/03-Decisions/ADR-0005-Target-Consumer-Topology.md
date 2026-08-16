---
id: ADR-0005
type: decision
status: accepted
title: Target consumer topology
created: 2026-08-11
updated: 2026-08-11
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs:
  - ADR-0002
  - ADR-0004
rule_refs:
  - RULE-0002
  - RULE-0004
---

# ADR-0005 — Hedef consumer topolojisi

## Bağlam

Kullanıcı tek UI’dan API Contract Checker, Database Checker, test yetenekleri ve ileride MCP kullanmak istiyor. Birden fazla aktif host ve issuer aynı ürün içinde DB ve auth sahipliğini tekrarlar.

## Karar

Hedef Test Module composition’ında tek UI, tek deploy edilen host, tek mantıksal uygulama DB’si ve Authenticator tarafından sağlanan tek issuer kullanılır. Checker’lar ve Vault bu hostta modül olarak compose edilir. Aynı DB farklı sahibi belli şemalar içerebilir.

## Alternatifler

- Her capability ayrı servis/DB/issuer: bugünkü ürün ölçeğinde gereksiz operasyon ve dağıtık tutarlılık maliyeti.
- Checker’ları UI’dan ayrı hostlara çağırmak: paket olarak in-process yeniden kullanım hedefini karşılamaz.

## Sonuçlar ve riskler

Consumer host ortak dependency graph ve migration sırasını yönetmelidir. İleride bağımsız ölçekleme zorunlu olursa bu karar yeni ADR ile yeniden değerlendirilir.
