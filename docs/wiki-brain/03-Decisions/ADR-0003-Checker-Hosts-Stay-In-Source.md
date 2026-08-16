---
id: ADR-0003
type: decision
status: accepted
title: Checker hosts remain source-only verification hosts
created: 2026-08-11
updated: 2026-08-11
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs:
  - ADR-0002
rule_refs:
  - RULE-0001
---

# ADR-0003 — Checker hostları source içinde kalır

## Bağlam

Paketin HTTP, Swagger, DI, EF ve migration davranışının bağımsız doğrulanması gerekir. Hostu kaldırmak bu kanıt ortamını yok eder; hostu paketlemek ise executable uygulamayı library paketi gibi dağıtır.

## Karar

Her checker’ın `host/Ptn.*.HttpApi.Host` projesi source tree’de kalır ve `IsPackable=false` olur. Production hedefinde consumer composition host çalışır; checker hostları ikinci aktif owner olmaz.

## Alternatifler

- Hostu silmek: geliştirme ve smoke doğrulamasını zorlaştırır.
- Hostu NuGet’e koymak: package/library sınırını bozar.

## Sonuçlar ve riskler

Host config’i örnek ve doğrulama amaçlıdır. Consumer’ın production config’i veya secretı değildir.
