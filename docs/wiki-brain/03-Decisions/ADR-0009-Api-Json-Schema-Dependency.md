---
id: ADR-0009
type: decision
status: accepted
title: API checker JSON Schema validator dependency
created: 2026-08-12
updated: 2026-08-12
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs:
  - ADR-0002
  - ADR-0006
rule_refs:
  - RULE-0001
---

# ADR-0009 — API checker JSON Schema doğrulayıcı bağımlılığı

## Bağlam

KBP-621 response/request conformance yüzeyi, OpenAPI modelini okumaktan farklı olarak
runtime JSON instance doğrulaması gerektirir. `Microsoft.OpenApi` belge okuma ve modelleme
yapar; bu doğrulamayı sağlamaz. Adaylar `JsonSchema.Net`, `Corvus.JsonSchema` ve
`NJsonSchema` idi. Seçilen tipler public `ResolvedSpecSchemaModel` yüzeyinde göründüğü için
bağımlılık yalnız bir implementasyon ayrıntısı değildir ve composition paketinden
consumer grafiğine transitif geçer.

## Karar

`CheckNexus.ApiContracts.Domain`, `NJsonSchema` 11.6.1'i doğrudan runtime bağımlılığı
olarak taşır. NuGet sözleşmesi 11.6.1'i asgari sürüm yapar; consumer'ın NuGet çözümlemesi
daha yüksek uyumlu bir sürüm seçebilir. Paket bu bağımlılığı `PrivateAssets` ile gizlemez
ve runtime assetlerini dışlamaz.

Dialect farkı mevcut `ISpecSchemaDialectComponent` resolver ailesinde tutulur:
Swagger 2.0 ve OpenAPI 3.0 uyarlamaları ayrı bileşenlerde kalır, OpenAPI 3.1 ise JSON
Schema 2020-12 URI'si ve JSON Schema doğrulama modu ile çalışır. Yeni bir doğrulayıcı
ancak mevcut motorun doğrulayamadığı somut bir dialect/uyumluluk vakası ve ikinci gerçek
uygulama oluşursa aynı resolver sınırına eklenir.

`0.1.0-alpha.5` değiştirilmez. Bu yeni public bağımlılık ve public yüzeyler iki checker
ailesinin ortak release adayı olan `0.2.0-alpha.1` ile çıkar. Paket doğrulama kapısı
`0.1.0-alpha.5` baseline'ına karşı çalışır; consumer smoke'u module initialization,
route/Swagger görünürlüğü ve EF model kurulumunu doğrular.

## Alternatifler

- `JsonSchema.Net` + OpenAPI vocabulary: Daha geniş vocabulary yönü güçlüdür; mevcut
  KBP-621 kodu ve testleriyle doğrulanmış ikinci motor ihtiyacı yoktur.
- `Corvus.JsonSchema`: Kod üretimi odaklı ek build zinciri getirir; runtime'da çözülen
  dinamik şemalar için mevcut akışa daha büyük entegrasyon maliyeti taşır.
- Bağımlılığı private yapmak: Public model NJsonSchema tipleri taşıdığı ve runtime
  doğrulama assembly'sine ihtiyaç duyduğu için consumer'da eksik asset üretir.
- Birden fazla doğrulayıcıyı birlikte taşımak: Paket grafiğini ve dialect kararını
  gereksiz büyütür; kanıtlanmış ikinci kullanım yoktur.

## Sonuçlar ve riskler

Consumer grafiğinde NJsonSchema sürüm çakışması mümkündür. Alpha consumer'lar restore
çıktısını ve runtime smoke'u kaydetmelidir; stable release öncesi hedef hostun tek paket
grafiğinde uyum kanıtı zorunludur. NJsonSchema public tiplerini ileride kaldırmak binary
uyumluluk kararıdır ve yeni sürüm ile PackageValidation değerlendirmesi gerektirir.
