namespace CheckNexus.Vault;

// islevi: Merkezî Vault KV v2 adapter'inin baglanti ve authentication ayarlarini tasir.
// sistemdeki gorevi: Address, mount, timeout ve secret injection seceneklerini composition hostta tek tipli sozlesmeye baglar.
public sealed class VaultOptions
{
    public const string SectionName = "Vault";
    public const string DefaultMount = "pintern-dev";
    public const int DefaultRequestTimeoutSeconds = 10;

    public string Address { get; set; } = default!;

    public string Mount { get; set; } = DefaultMount;

    public VaultAuthenticationMode AuthenticationMode { get; set; } = VaultAuthenticationMode.Token;

    public string? Token { get; set; }

    public string? TokenFile { get; set; }

    public string? Namespace { get; set; }

    public int RequestTimeoutSeconds { get; set; } = DefaultRequestTimeoutSeconds;
}
