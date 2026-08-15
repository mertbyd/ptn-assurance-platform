namespace CheckNexus.Vault;

// islevi: Mantiksal checker secret path'ini guvenli KV v2 data endpointine donusturur.
// sistemdeki gorevi: Mount ve path segmentlerini encode ederek arbitrary endpoint/path enjeksiyonunu provider sinirinda engeller.
internal static class VaultUriBuilder
{
    // KV v2 data endpointinin relative URI degerini kurar.
    public static string BuildDataPath(string mount, string path)
    {
        var encodedMount = Uri.EscapeDataString(mount);
        var encodedPath = string.Join('/', path
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Uri.EscapeDataString));

        if (string.IsNullOrWhiteSpace(encodedPath))
        {
            throw new ArgumentException("Secret path cannot be empty.", nameof(path));
        }

        return $"v1/{encodedMount}/data/{encodedPath}";
    }
}
