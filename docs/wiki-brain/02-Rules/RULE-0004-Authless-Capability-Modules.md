---
id: RULE-0004
type: rule
status: active
title: Authless capability modules
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

# RULE-0004 — Auth’suz capability modülleri

## Kural

API Contract Checker ve Database Checker kendi issuer, login, user/role/tenant yaşam döngüsü veya ikinci Identity/OpenIddict tablosu oluşturmaz. Kimlik ve authorization bağlamı consumer hosttan gelir.

Checker paketleri Authenticator veya Foundation paketlerini kendi capability graph'ına
eklemez. Test Module composition host ihtiyaç duyduğu Authenticator katmanlarını doğrudan
alır; Foundation katmanları yalnız Authenticator üzerinden transitif gelir.

Test Module hostu Auth **Application ve EntityFrameworkCore** yüzeylerini tip olarak compose
eder; **`AuthenticatorHttpApiModule` compose edilmez** ve kimlik uçları ayrı deploy edilen
Authenticator hostunda kalır ([[03-Decisions/ADR-0013-Test-Module-Resource-Server-Auth-Consumption|ADR-0013]]).
Doğrulama ABP JWT bearer ile yapılır.

## Gerekçe

Aynı modül hem Test Module içinde hem başka ortak hostlarda kullanılabilir. Kimlik sahipliğinin pakete gömülmemesi tek issuer ve tek DB hedefindeki çifte sahipliği engeller.

## Doğrulama

- Checker composition paketleri Authenticator executable hostunu veya Foundation direct reference'ını taşımaz.
- Test Module package graph'ta Authenticator direct, Foundation transitive görünür.
- Test Module hostunun Swagger'ında auth ucu görünmez; `AuthenticatorHttpApiModule` `DependsOn` zincirinde değildir.
- Consumer host permissions/policies için tek kimlik bağlamı sağlar.
- Checker migrationları Identity/OpenIddict tabloları üretmez.

## İstisna süreci

Bağımsız checker hostu yalnız doğrulama hostudur; production kimlik sahibi yapılması ayrı ürün ve yeni ADR gerektirir.
