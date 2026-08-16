---
id: ADR-0010
type: decision
status: accepted
title: API Contracts 0.2 interface expansion
created: 2026-08-12
updated: 2026-08-12
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs:
  - ADR-0006
  - ADR-0009
rule_refs:
  - RULE-0001
---

# ADR-0010 — API Contracts 0.2 interface genişlemesi

## Bağlam

`0.2.0-alpha.1`, conformance/diagnosis ve finding geçmişi için mevcut public
`IContractCheckRunAppService`, `ISpecSnapshotAppService` ve
`IContractCheckRunRepository` arayüzlerine yeni metotlar ekler. DTO ve route eklemeleri
consumer çağıranlar için additive olsa da mevcut arayüzleri kendisi implement eden bir
consumer için yeni abstract üyeler binary/source kırığıdır. PackageValidation bu farkı
`0.1.0-alpha.5` baseline'ına karşı `CP0006` olarak doğru biçimde durdurur.

## Karar

Yeni metotlar `0.2.0-alpha.1` public sözleşmesinin bilinçli parçasıdır. Eski sürüm
değiştirilmez. Kırık yalnız aşağıdaki iki assembly ve PackageValidation tarafından üretilen
tam üye hedefleri için `CompatibilitySuppressions.xml` ile kabul edilir:

- `CheckNexus.ApiContracts.Application.Contracts`: dört AppService interface metodu
- `CheckNexus.ApiContracts.Domain`: dört repository interface metodu

Genel bir `CP0006` kapatma, `EnablePackageValidation=false` veya assembly düzeyinde geniş
suppression kullanılmaz. Yeni bir public kırık eklendiğinde mevcut dosyalar onu otomatik
olarak kabul etmez; pack kapısı tekrar hata verir ve ayrı değerlendirme ister.

`Finding` altı parametreli constructor'ı ile Database Comparison
`Ptn.DatabaseChecker.Comparison.PostgreSqlDatabaseConnectionTester` tipi bilinçli kırık
değildir. Bunlar 0.1.x binary sözleşmesini koruyan uyumluluk shim'leriyle geri getirilmiştir.

## Sonuçlar ve riskler

Standart ABP tüketicisi arayüzleri implement etmek yerine uygulama servislerini proxy/DI
üzerinden tükettiği için ana consumer akışı yeni metotları additive görür. Özel interface
implementasyonları 0.2'ye geçerken yeni üyeleri implement etmelidir. Package README ve
release notu bu yükseltme notunu taşır; stable release öncesi yayımlanmış paketle hedef-host
consumer smoke'u zorunludur.
