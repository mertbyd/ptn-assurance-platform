using System;

namespace Ptn.ApiContractChecker.Secrets;

// islevi: Bir spec kaynaginin Vault KV v2 mantiksal yolunu (mount haric) kurar.
// sistemdeki gorevi: Multi-tenant izolasyonu path'e tasir; SpecSource.VaultSecretPath bu ciktiyi saklar. Saf fonksiyon, ABP bagimliligi yok.
public static class SecretPathBuilder
{
    // Host (tenant'siz) kayitlar icin yol segmenti.
    private const string HostSegment = "host";

    // Spec kaynagi sirlarinin yol segmenti.
    private const string SourcesSegment = "sources";

    // tenantId null ise "host", degilse tenant Guid'i kullanir; ciktida secret-store mount bilgisi yoktur.
    public static string Build(Guid? tenantId, Guid specSourceId)
    {
        var tenantSegment = tenantId?.ToString() ?? HostSegment;
        return $"{tenantSegment}/{SourcesSegment}/{specSourceId}";
    }
}
