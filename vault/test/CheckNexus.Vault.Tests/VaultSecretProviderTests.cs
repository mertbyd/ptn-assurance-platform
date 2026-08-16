using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Ptn.ApiContractChecker.Models.Secrets;
using Ptn.DatabaseChecker.Models.Secrets;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace CheckNexus.Vault.Tests;

// islevi: Ortak Vault provider'inin iki checker portundaki KV v2 wire ve hata sozlesmesini kanitlar.
// sistemdeki gorevi: Secret sekli, path izolasyonu, token/proxy modu ve safe-error davranisinin package refactorlarinda bozulmasini engeller.
public sealed class VaultSecretProviderTests
{
    private const string TestToken = "local-limited-token";

    [Fact]
    public async Task Should_Write_Api_Credential_With_Token_Auth()
    {
        var handler = new StubVaultHttpMessageHandler(async request =>
        {
            request.Method.ShouldBe(HttpMethod.Post);
            request.RequestUri!.AbsolutePath.ShouldBe("/v1/pintern-dev/data/host/sources/42");
            ShouldMatchSecretWithoutDisclosure(
                request.Headers.GetValues("X-Vault-Token").Single(),
                TestToken);
            var json = await request.Content!.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var data = document.RootElement.GetProperty("data");
            data.GetProperty("headerName").GetString().ShouldBe("Authorization");
            ShouldMatchSecretWithoutDisclosure(
                data.GetProperty("headerValue").GetString()!,
                "Bearer canary-secret");
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });
        var provider = CreateProvider(handler);

        await provider.SetAsync("host/sources/42", new ApiCredentialModel
        {
            HeaderName = "Authorization",
            HeaderValue = "Bearer canary-secret"
        });
    }

    [Fact]
    public async Task Should_Read_Database_Credential_Without_Exposing_Provider_Payload()
    {
        var handler = new StubVaultHttpMessageHandler(request =>
        {
            request.Method.ShouldBe(HttpMethod.Get);
            request.RequestUri!.AbsolutePath.ShouldBe("/v1/pintern-dev/data/tenant-a/connections/84");
            return Task.FromResult(JsonResponse(
                "{\"data\":{\"data\":{\"username\":\"reader\",\"password\":\"canary-password\"}}}"));
        });
        var provider = CreateProvider(handler);

        var credential = await provider.GetDatabaseCredentialAsync("tenant-a/connections/84");

        credential.Username.ShouldBe("reader");
        ShouldMatchSecretWithoutDisclosure(credential.Password, "canary-password");
    }

    [Fact]
    public async Task Should_Not_Send_Token_Header_In_Agent_Proxy_Mode()
    {
        var handler = new StubVaultHttpMessageHandler(request =>
        {
            request.Headers.Contains("X-Vault-Token").ShouldBeFalse();
            return Task.FromResult(JsonResponse(
                "{\"data\":{\"data\":{\"headerName\":\"X-Api-Key\",\"headerValue\":\"canary\"}}}"));
        });
        var options = ValidOptions();
        options.AuthenticationMode = VaultAuthenticationMode.AgentProxy;
        options.Token = null;
        var provider = CreateProvider(handler, options);

        var credential = await provider.GetApiCredentialAsync("host/sources/42");

        credential.HeaderName.ShouldBe("X-Api-Key");
    }

    [Fact]
    public async Task Should_Translate_Api_Not_Found_To_Checker_Business_Code()
    {
        var handler = new StubVaultHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));
        var provider = CreateProvider(handler);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            provider.GetApiCredentialAsync("host/sources/missing"));

        exception.Code.ShouldBe(Ptn.ApiContractChecker.ExceptionCodes.SecretExceptionCodes.NotFound);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task Should_Translate_Provider_Failure_Without_Exposing_Response_Body(
        HttpStatusCode statusCode)
    {
        const string ProviderCanary = "provider-response-canary";
        var handler = new StubVaultHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(ProviderCanary)
            }));
        var provider = CreateProvider(handler);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            provider.GetDatabaseCredentialAsync("host/connections/denied"));

        exception.Code.ShouldBe(VaultExceptionCodes.RequestFailed);
        exception.Data["StatusCode"].ShouldBe((int)statusCode);
        exception.ToString().Contains(ProviderCanary, StringComparison.Ordinal).ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Translate_Timeout_Without_Exposing_Provider_Details()
    {
        const string ProviderCanary = "timeout-provider-canary";
        var handler = new StubVaultHttpMessageHandler(_ =>
            throw new TaskCanceledException(ProviderCanary));
        var provider = CreateProvider(handler);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            provider.GetApiCredentialAsync("host/sources/timeout"));

        exception.Code.ShouldBe(VaultExceptionCodes.Timeout);
        exception.ToString().Contains(ProviderCanary, StringComparison.Ordinal).ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Read_Token_From_File_For_Token_Authentication()
    {
        const string FileToken = "local-token-file-canary";
        var tokenFile = Path.Combine(
            Path.GetTempPath(),
            $"pintern-vault-token-{Guid.NewGuid():N}");
        await File.WriteAllTextAsync(tokenFile, FileToken);

        try
        {
            var handler = new StubVaultHttpMessageHandler(request =>
            {
                ShouldMatchSecretWithoutDisclosure(
                    request.Headers.GetValues("X-Vault-Token").Single(),
                    FileToken);
                return Task.FromResult(JsonResponse(
                    "{\"data\":{\"data\":{\"headerName\":\"X-Api-Key\",\"headerValue\":\"canary\"}}}"));
            });
            var options = ValidOptions();
            options.Token = null;
            options.TokenFile = tokenFile;
            var provider = CreateProvider(handler, options);

            await provider.GetApiCredentialAsync("host/sources/token-file");
        }
        finally
        {
            File.Delete(tokenFile);
        }
    }

    [Fact]
    public void Should_Reject_Token_Mode_Without_A_Secret_Source()
    {
        var options = ValidOptions();
        options.Token = null;
        options.TokenFile = null;

        var result = new VaultOptionsValidator().Validate(null, options);

        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("Vault:Token");
    }

    // Test provider'ini tek fake HTTP boundary ve typed options ile kurar.
    private static VaultSecretProvider CreateProvider(
        HttpMessageHandler handler,
        VaultOptions? options = null)
        => new(new StubHttpClientFactory(handler), Options.Create(options ?? ValidOptions()));

    // Token auth icin gecerli minimum local Vault ayarlarini verir.
    private static VaultOptions ValidOptions()
        => new()
        {
            Address = "http://127.0.0.1:8200",
            Mount = "pintern-dev",
            AuthenticationMode = VaultAuthenticationMode.Token,
            Token = TestToken,
            RequestTimeoutSeconds = 5
        };

    // KV v2 response body'sini JSON content olarak uretir.
    private static HttpResponseMessage JsonResponse(string json)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    // Secret esitligini beklenen veya gercek degeri assertion mesajina koymadan dogrular.
    private static void ShouldMatchSecretWithoutDisclosure(string actual, string expected)
        => CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(actual)),
                SHA256.HashData(Encoding.UTF8.GetBytes(expected)))
            .ShouldBeTrue("Secret values must match without disclosure.");
}
