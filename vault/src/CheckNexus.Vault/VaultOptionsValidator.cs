using Microsoft.Extensions.Options;

namespace CheckNexus.Vault;

// islevi: Vault ayarlarinin kullanilmadan once yapisal olarak gecerli olmasini denetler.
// sistemdeki gorevi: Eksik address/mount/token veya gecersiz timeout ile hostun belirsiz runtime hatasina dusmesini engeller.
public sealed class VaultOptionsValidator : IValidateOptions<VaultOptions>
{
    // Vault typed options sozlesmesini fail-fast olarak denetler.
    public ValidateOptionsResult Validate(string? name, VaultOptions options)
    {
        var failures = new List<string>();

        if (!Uri.TryCreate(options.Address, UriKind.Absolute, out var address) ||
            (address.Scheme != Uri.UriSchemeHttp && address.Scheme != Uri.UriSchemeHttps))
        {
            failures.Add("Vault:Address must be an absolute HTTP or HTTPS URI.");
        }

        if (string.IsNullOrWhiteSpace(options.Mount) || options.Mount.Contains('/'))
        {
            failures.Add("Vault:Mount must be a single non-empty KV v2 mount segment.");
        }

        if (options.RequestTimeoutSeconds <= 0)
        {
            failures.Add("Vault:RequestTimeoutSeconds must be greater than zero.");
        }

        if (options.AuthenticationMode == VaultAuthenticationMode.Token &&
            string.IsNullOrWhiteSpace(options.Token) &&
            string.IsNullOrWhiteSpace(options.TokenFile))
        {
            failures.Add("Vault token authentication requires Vault:Token or Vault:TokenFile.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
