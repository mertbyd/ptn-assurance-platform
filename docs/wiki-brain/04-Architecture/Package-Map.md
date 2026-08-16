---
id: ARCH-0002
type: current
status: active
title: Checker package and host map
updated: 2026-08-13
decision_refs:
  - ADR-0002
  - ADR-0003
  - ADR-0006
  - ADR-0009
  - ADR-0012
rule_refs:
  - RULE-0001
---

# Paket ve host haritası

## Katman grafiği

```text
Domain.Shared
  -> Domain
      -> Application
Application.Contracts
  -> Application
Application + EntityFrameworkCore + HttpApi
  -> CheckNexus.<Capability> composition package

HttpApi.Client -> uzak consumer proxy paketi
HttpApi.Host   -> source-only doğrulama hostu, IsPackable=false
Test projects  -> source-only doğrulama, IsPackable=false
```

## Authenticator ve Foundation grafiği

```text
ABP
  -> Nexum.Abp.Foundation.<Layer> 1.0.0
      -> Authenticator.<Layer> 2.0.0
          -> Test Module/Assurance composition host
```

Foundation yedi katmanda publictir. Authenticator Domain.Shared, Domain,
Application.Contracts, Application, EntityFrameworkCore, HttpApi ve HttpApi.Client
paketleri eşlenen Foundation paketini nuspec ve ABP module dependency olarak taşır.
Consumer Foundation paketlerini doğrudan referanslamaz. EventHandler Auth Domain/Application
grafiği üzerinden gelir. Auth `2.0.0` bu sayfanın tarihinde doğrulanmış local adaydır;
public yayın tamamlanmadan target consumer sürümü sayılmaz.

## Composition paketleri

| Capability | Ana PackageId | Source proje |
|---|---|---|
| API Contract Checker | `CheckNexus.ApiContracts` | `checkers/api-contract/src/CheckNexus.ApiContracts` |
| Database Checker | `CheckNexus.DatabaseComparison` | `checkers/database-comparison/src/CheckNexus.DatabaseComparison` |
| Ortak Vault | `CheckNexus.Vault` | `vault/src/CheckNexus.Vault` |

Ana checker paketleri consumer için kolay giriş noktasıdır. İleri seviye consumer yalnız gerekli katman paketlerini de referanslayabilir; migration/HTTP yüzeyi gerekiyorsa doğru modül bağımlılıkları ayrıca kurulmalıdır.

API Contract Checker'ın `Domain` paketi runtime JSON Schema doğrulaması için
`NJsonSchema` 11.6.1'i public dependency olarak taşır. Composition paketi bu bağımlılığı
transitif geçirir; seçim ve consumer uyumluluk yükümlülüğü ADR-0009'dadır.

## Host amacı

| Host | Amaç | Production owner mı? | Packable mı? |
|---|---|---:|---:|
| `Ptn.ApiContractChecker.HttpApi.Host` | Swagger, DI, HTTP, EF/migration smoke | Hayır | Hayır |
| `Ptn.DatabaseChecker.HttpApi.Host` | Swagger, DI, HTTP, EF/migration smoke | Hayır | Hayır |
