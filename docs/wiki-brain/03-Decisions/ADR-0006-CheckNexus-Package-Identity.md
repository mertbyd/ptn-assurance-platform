---
id: ADR-0006
type: decision
status: accepted
title: CheckNexus package identity
created: 2026-08-11
updated: 2026-08-11
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs: []
rule_refs:
  - RULE-0001
---

# ADR-0006 — CheckNexus paket kimliği

## Bağlam

Geçici workspace veya şirket içi proje adlarını public paket kimliğine taşımak paketin yeniden kullanımını ve ürün ailesini belirsizleştiriyordu.

## Karar

Public checker paket ailesi `CheckNexus.ApiContracts*` ve `CheckNexus.DatabaseComparison*` adlarını kullanır. Ortak secret adapteri `CheckNexus.Vault` adını kullanır. Owner `mertbyd`’dir.

## Alternatifler

- `Pintern.SaaS.*`: geçici workspace/ürün topolojisini public kimliğe bağlar.
- Eski `Ptn.*` assembly adlarını public PackageId yapmak: paket ailesini belirsizleştirir.

## Sonuçlar ve riskler

İç namespace/assembly adları source uyumluluğu için kademeli değişebilir; public `PackageId` sürümleme ve consumer sözleşmesidir.
