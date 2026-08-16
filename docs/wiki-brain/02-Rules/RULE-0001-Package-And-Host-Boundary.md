---
id: RULE-0001
type: rule
status: active
title: Package and host boundary
updated: 2026-08-11
severity: mandatory
scope: platform
sources: []
decision_refs:
  - ADR-0002
  - ADR-0003
rule_refs: []
---

# RULE-0001 — Paket ve host sınırı

## Kural

Controller, AppService, Manager, Repository, EF Core ve migration katmanları checker paketlerinde kalır. Executable host ve test projeleri paketlenmez.

## Gerekçe

Consumer host checker’ı gerçek ABP modülü olarak compose edebilmelidir. İnce checker hostu ise bağımsız geliştirme, Swagger, HTTP, DI ve migration doğrulaması için gereklidir; production’da ikinci runtime owner değildir.

## Doğrulama

- Composition `.csproj` Application + EntityFrameworkCore + HttpApi referanslarını taşır.
- Host ve test projelerinde `IsPackable=false` bulunur.
- `.nupkg` içinde executable host çıktısı veya test assembly’si yoktur.

## İstisna süreci

Sınır değişecekse yeni ADR açılır ve consumer host etkisi gösterilir.
