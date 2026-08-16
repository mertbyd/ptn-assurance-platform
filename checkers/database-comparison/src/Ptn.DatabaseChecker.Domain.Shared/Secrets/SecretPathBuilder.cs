using System;

namespace Ptn.DatabaseChecker.Secrets;

// islevi: Bir baglantinin Vault KV v2 mantiksal yolunu (mount haric) kurar.
// sistemdeki gorevi: Multi-tenant izolasyonu path'e tasir; DatabaseConnection.VaultSecretPath bu ciktiyi saklar. Saf fonksiyon, ABP bagimliligi yok.
public static class SecretPathBuilder
{
    // Host (tenant'siz) kayitlar icin yol segmenti.
    private const string HostSegment = "host";

    // Baglanti sirlarinin yol segmenti.
    private const string ConnectionsSegment = "connections";

    // tenantId null ise "host", degilse tenant Guid'i kullanir; ciktida secret-store mount bilgisi yoktur.
    public static string Build(Guid? tenantId, Guid connectionId)
    {
        var tenantSegment = tenantId?.ToString() ?? HostSegment;
        return $"{tenantSegment}/{ConnectionsSegment}/{connectionId}";
    }
}
