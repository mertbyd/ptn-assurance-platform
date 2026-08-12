using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Ptn.ApiContractChecker.Models.Secrets;
using Ptn.DatabaseChecker.Models.Secrets;
using Volo.Abp;
using ApiSecretProvider = Ptn.ApiContractChecker.Interface.Secrets.ISecretProvider;
using DatabaseSecretProvider = Ptn.DatabaseChecker.Interface.Secrets.ISecretProvider;

namespace CheckNexus.Vault;

// islevi: Iki checker secret portunu tek HashiCorp Vault KV v2 HTTP adapter'i ile uygular.
// sistemdeki gorevi: Secret degerini yalniz provider belleğinde tutar; token/proxy, wire payload ve provider hatalarini checker katmanlarindan izole eder.
public sealed class VaultSecretProvider : ApiSecretProvider, DatabaseSecretProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly VaultOptions _options;

    public VaultSecretProvider(IHttpClientFactory httpClientFactory, IOptions<VaultOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    // API kaynagi header credential'ini Vault'tan cozer.
    public async Task<ApiCredentialModel> GetApiCredentialAsync(string path)
    {
        var data = await ReadSecretDataAsync(path, Ptn.ApiContractChecker.ExceptionCodes.SecretExceptionCodes.NotFound);
        return new ApiCredentialModel
        {
            HeaderName = ReadRequiredString(data, VaultConstants.HeaderNamePropertyName),
            HeaderValue = ReadRequiredString(data, VaultConstants.HeaderValuePropertyName)
        };
    }

    // API kaynagi header credential'ini KV v2'ye yeni versiyon olarak yazar.
    public Task SetAsync(string path, ApiCredentialModel credential)
        => WriteSecretDataAsync(path, new Dictionary<string, string>
        {
            [VaultConstants.HeaderNamePropertyName] = credential.HeaderName,
            [VaultConstants.HeaderValuePropertyName] = credential.HeaderValue
        });

    // Database username/password ciftini Vault'tan cozer.
    public async Task<DatabaseCredentialModel> GetDatabaseCredentialAsync(string path)
    {
        var data = await ReadSecretDataAsync(path, Ptn.DatabaseChecker.ExceptionCodes.SecretExceptionCodes.NotFound);
        return new DatabaseCredentialModel
        {
            Username = ReadRequiredString(data, VaultConstants.UsernamePropertyName),
            Password = ReadRequiredString(data, VaultConstants.PasswordPropertyName)
        };
    }

    // Database username/password ciftini KV v2'ye yeni versiyon olarak yazar.
    public Task SetAsync(string path, DatabaseCredentialModel credential)
        => WriteSecretDataAsync(path, new Dictionary<string, string>
        {
            [VaultConstants.UsernamePropertyName] = credential.Username,
            [VaultConstants.PasswordPropertyName] = credential.Password
        });

    // Checker'in mantiksal path'indeki son KV v2 secret versiyonunu soft-delete eder.
    public async Task DeleteAsync(string path)
    {
        using var response = await SendAsync(HttpMethod.Delete, path);
        EnsureSuccess(response);
    }

    // KV v2 read response icindeki asil secret sozlugunu secret degerini loglamadan okur.
    private async Task<JsonElement> ReadSecretDataAsync(string path, string notFoundCode)
    {
        using var response = await SendAsync(HttpMethod.Get, path);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new BusinessException(notFoundCode);
        }

        EnsureSuccess(response);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        if (!document.RootElement.TryGetProperty(VaultConstants.DataPropertyName, out var responseData) ||
            !responseData.TryGetProperty(VaultConstants.DataPropertyName, out var secretData) ||
            secretData.ValueKind != JsonValueKind.Object)
        {
            throw new BusinessException(VaultExceptionCodes.InvalidPayload);
        }

        return secretData.Clone();
    }

    // Secret sozlugunu Vault KV v2 write envelope'i icinde gonderir.
    private async Task WriteSecretDataAsync(string path, IReadOnlyDictionary<string, string> data)
    {
        var payload = new Dictionary<string, object>
        {
            [VaultConstants.DataPropertyName] = data
        };

        using var response = await SendAsync(HttpMethod.Post, path, payload);
        EnsureSuccess(response);
    }

    // Vault istegini token veya Agent Proxy moduna gore guvenli header'larla gonderir.
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? payload = null)
    {
        var client = _httpClientFactory.CreateClient(VaultConstants.HttpClientName);
        client.BaseAddress = new Uri(EnsureTrailingSlash(_options.Address));
        client.Timeout = TimeSpan.FromSeconds(_options.RequestTimeoutSeconds);

        using var request = new HttpRequestMessage(method, VaultUriBuilder.BuildDataPath(_options.Mount, path));
        AddAuthenticationHeaders(request);
        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload);
        }

        try
        {
            return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }
        catch (TaskCanceledException)
        {
            throw new BusinessException(VaultExceptionCodes.Timeout);
        }
        catch (HttpRequestException)
        {
            throw new BusinessException(VaultExceptionCodes.Unavailable);
        }
    }

    // Direct-token modunda token'i, her iki modda varsa namespace'i request'e ekler.
    private void AddAuthenticationHeaders(HttpRequestMessage request)
    {
        if (_options.AuthenticationMode == VaultAuthenticationMode.Token)
        {
            request.Headers.TryAddWithoutValidation(VaultConstants.TokenHeaderName, ResolveToken());
        }

        if (!string.IsNullOrWhiteSpace(_options.Namespace))
        {
            request.Headers.TryAddWithoutValidation(VaultConstants.NamespaceHeaderName, _options.Namespace.Trim());
        }
    }

    // Config veya mounted file icindeki token'i her istekte yeniden cozer.
    private string ResolveToken()
    {
        if (!string.IsNullOrWhiteSpace(_options.Token))
        {
            return _options.Token.Trim();
        }

        if (!string.IsNullOrWhiteSpace(_options.TokenFile) && File.Exists(_options.TokenFile))
        {
            var token = File.ReadAllText(_options.TokenFile).Trim();
            if (!string.IsNullOrWhiteSpace(token))
            {
                return token;
            }
        }

        throw new BusinessException(VaultExceptionCodes.MissingToken);
    }

    // Vault payload'undaki zorunlu string alani secret degerini hata mesajina koymadan okur.
    private static string ReadRequiredString(JsonElement data, string propertyName)
    {
        if (!data.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            throw new BusinessException(VaultExceptionCodes.InvalidPayload);
        }

        return property.GetString()!;
    }

    // Base address'in relative KV endpointlerle guvenli birlesmesi icin trailing slash ekler.
    private static string EnsureTrailingSlash(string address)
        => address.EndsWith('/') ? address : $"{address}/";

    // Basarisiz provider status'unu response body okumadan guvenli hata koduna cevirir.
    private static void EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new BusinessException(VaultExceptionCodes.RequestFailed)
            .WithData("StatusCode", (int)response.StatusCode);
    }
}
