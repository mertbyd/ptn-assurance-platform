using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ptn.ApiContractChecker.Models.Secrets;
using Ptn.DatabaseChecker.Models.Secrets;
using Shouldly;
using Xunit;

namespace CheckNexus.Vault.Tests;

// islevi: Acikca etkinlestirildiginde iki checker secret sozlesmesini gercek bir Vault KV v2 servisiyle round-trip dogrular.
// sistemdeki gorevi: HTTP adapterinin gercek Vault wire protokolu, path ve payload sekliyle uyumunu stub testlerinin otesinde kanitlar.
public sealed class VaultLiveSmokeTests
{
    private const string EnabledEnvironmentVariable = "PINTERN_VAULT_SMOKE_ENABLED";
    private const string AddressEnvironmentVariable = "PINTERN_VAULT_SMOKE_ADDRESS";
    private const string TokenEnvironmentVariable = "PINTERN_VAULT_SMOKE_TOKEN";

    [Fact]
    [Trait("Category", "LiveVault")]
    public async Task Should_Round_Trip_Both_Checker_Credentials_When_Enabled()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable(EnabledEnvironmentVariable),
                bool.TrueString,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var address = ReadRequiredEnvironmentVariable(AddressEnvironmentVariable);
        var token = ReadRequiredEnvironmentVariable(TokenEnvironmentVariable);
        var pathSuffix = Guid.NewGuid().ToString("N");
        var apiPath = $"smoke/sources/{pathSuffix}";
        var databasePath = $"smoke/connections/{pathSuffix}";

        var services = new ServiceCollection();
        services.AddHttpClient();
        await using var serviceProvider = services.BuildServiceProvider();
        var provider = new VaultSecretProvider(
            serviceProvider.GetRequiredService<IHttpClientFactory>(),
            Options.Create(new VaultOptions
            {
                Address = address,
                Mount = "pintern-dev",
                AuthenticationMode = VaultAuthenticationMode.Token,
                Token = token,
                RequestTimeoutSeconds = 10
            }));

        try
        {
            await provider.SetAsync(apiPath, new ApiCredentialModel
            {
                HeaderName = "X-Smoke-Key",
                HeaderValue = "api-smoke-secret"
            });
            await provider.SetAsync(databasePath, new DatabaseCredentialModel
            {
                Username = "smoke_reader",
                Password = "database-smoke-secret"
            });

            var apiCredential = await provider.GetApiCredentialAsync(apiPath);
            var databaseCredential = await provider.GetDatabaseCredentialAsync(databasePath);

            apiCredential.HeaderName.ShouldBe("X-Smoke-Key");
            ShouldMatchSecretWithoutDisclosure(apiCredential.HeaderValue, "api-smoke-secret");
            databaseCredential.Username.ShouldBe("smoke_reader");
            ShouldMatchSecretWithoutDisclosure(databaseCredential.Password, "database-smoke-secret");
        }
        finally
        {
            await Task.WhenAll(
                provider.DeleteAsync(apiPath),
                provider.DeleteAsync(databasePath));
        }
    }

    // Smoke konfigurasyonundaki secret degerini hata mesajina eklemeden zorunlu olarak okur.
    private static string ReadRequiredEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{name} must be supplied for the live Vault smoke test.");
        }

        return value;
    }

    // Live secret esitligini degerleri test evidence'a koymadan dogrular.
    private static void ShouldMatchSecretWithoutDisclosure(string actual, string expected)
        => CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(actual)),
                SHA256.HashData(Encoding.UTF8.GetBytes(expected)))
            .ShouldBeTrue("Secret values must match without disclosure.");
}
