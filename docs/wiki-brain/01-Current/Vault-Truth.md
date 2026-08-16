---
id: CURRENT-0003
type: current
status: active
title: Shared Vault adapter current truth
updated: 2026-08-15
decision_refs:
  - ADR-0004
rule_refs:
  - RULE-0003
---
![[Ekran görüntüsü 2026-08-11 154707.png]]
# Ortak Vault güncel gerçeği

`CheckNexus.Vault` bir Vault sunucusu değildir. Consumer composition host içinde çalışan HashiCorp Vault KV v2 adapteridir.

## Kanıtlanan sözleşme

- `CheckNexusVaultModule`, typed `VaultOptions` ayarlarını `Vault` section’ından okur.
- Ayarlar startup sırasında `VaultOptionsValidator` ile fail-fast doğrulanır.
- `VaultSecretProvider` tek singleton instance olarak kaydedilir.
- Aynı instance API Contract Checker ve Database Checker’ın ayrı `ISecretProvider` portlarına bağlanır.
- API credential sözleşmesi `HeaderName` + `HeaderValue` çiftidir.
- Database credential sözleşmesi `Username` + `Password` çiftidir.
- KV v2 HTTP yolu tek adapter tarafından kurulur.
- `Token` ve `AgentProxy` authentication modları vardır.

## Sahiplik sınırı

Checker paketleri secret değerini DB’ye, DTO’ya veya loga yazmaz. DB yalnız secret path/referansını tutabilir. Vault deployment, auth method, policy, mount ve rotation operasyon altyapısının sorumluluğudur.

Tek Vault kullanmak bütün secretların aynı policy ile okunması anlamına gelmez. Ortam, tenant, capability ve secret türü path/policy sınırlarıyla ayrılır.

## Dağıtım durumu

Paket kimliği `CheckNexus.Vault`, consumer sürümü `0.2.0-alpha.2`dir ve NuGet.org'da publictir.
Tek PackageId registry'de doğrulanmış, release manifestte immutable olarak işaretlenmiş ve
Test Module `common.props` ile aynı sürüme hizalanmıştır. Test Module hostu paketi compose eder;
iki checker secret portu aynı singleton adaptere çözülür. `config`, `policies`,
`docker-compose.local.yml`, `Initialize-LocalVault.ps1` ve `Test-VaultAdapter.ps1` yerel
geliştirme/doğrulama varlıklarıdır; production secretı değildir.
