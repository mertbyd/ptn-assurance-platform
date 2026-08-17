using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CheckNexus.Vault;
using Microsoft.Extensions.Options;
using Piton.Emailing.Interface.Emailing;
using Piton.Emailing.Models.Emailing;
using Ptn.TestModule.Constants;
using Ptn.TestModule.ExceptionCodes.Emailing;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.TestModule.Emailing;

// islevi: Email provider secret portunu composition hostun ortak Vault KV v2 ayarlarina baglar.
// sistemdeki gorevi: SMTP ve OAuth secret'larini response, appsettings, log ve veritabanindan uzak tutar.
[Dependency(ReplaceServices = true)]
public sealed class VaultEmailSecretStore : IEmailSecretStore, ISingletonDependency
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly VaultOptions _options;

    public VaultEmailSecretStore(IHttpClientFactory httpClientFactory, IOptions<VaultOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<EmailSecretModel?> GetAsync(string path)
    {
        using var response = await SendAsync(HttpMethod.Get, path);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        EnsureSuccess(response);
        return await ReadSecretAsync(response);
    }

    public async Task SetAsync(string path, EmailSecretModel secret)
    {
        var data = new Dictionary<string, string?>
        {
            [TestModuleEmailingVaultConstants.SmtpUsernamePropertyName] = secret.SmtpUsername,
            [TestModuleEmailingVaultConstants.SmtpPasswordPropertyName] = secret.SmtpPassword,
            [TestModuleEmailingVaultConstants.GoogleClientSecretPropertyName] = secret.GoogleClientSecret,
            [TestModuleEmailingVaultConstants.GoogleRefreshTokenPropertyName] = secret.GoogleRefreshToken
        };
        var payload = new Dictionary<string, object>
        {
            [TestModuleEmailingVaultConstants.DataPropertyName] = data
        };

        using var response = await SendAsync(HttpMethod.Post, path, payload);
        EnsureSuccess(response);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? payload = null)
    {
        var client = _httpClientFactory.CreateClient(TestModuleEmailingVaultConstants.HttpClientName);
        client.BaseAddress = new Uri(EnsureTrailingSlash(_options.Address));
        client.Timeout = TimeSpan.FromSeconds(_options.RequestTimeoutSeconds);

        using var request = new HttpRequestMessage(method, BuildDataPath(_options.Mount, path));
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
            throw new BusinessException(TestModuleEmailingErrorCodes.VaultTimeout);
        }
        catch (HttpRequestException)
        {
            throw new BusinessException(TestModuleEmailingErrorCodes.VaultUnavailable);
        }
    }

    private void AddAuthenticationHeaders(HttpRequestMessage request)
    {
        if (_options.AuthenticationMode == VaultAuthenticationMode.Token)
        {
            request.Headers.TryAddWithoutValidation(
                TestModuleEmailingVaultConstants.TokenHeaderName,
                ResolveToken());
        }

        if (!string.IsNullOrWhiteSpace(_options.Namespace))
        {
            request.Headers.TryAddWithoutValidation(
                TestModuleEmailingVaultConstants.NamespaceHeaderName,
                _options.Namespace.Trim());
        }
    }

    private string ResolveToken()
    {
        if (!string.IsNullOrWhiteSpace(_options.Token))
        {
            return _options.Token.Trim();
        }

        if (!string.IsNullOrWhiteSpace(_options.TokenFile) && File.Exists(_options.TokenFile))
        {
            try
            {
                var token = File.ReadAllText(_options.TokenFile).Trim();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    return token;
                }
            }
            catch (IOException)
            {
                throw new BusinessException(TestModuleEmailingErrorCodes.VaultMissingToken);
            }
            catch (UnauthorizedAccessException)
            {
                throw new BusinessException(TestModuleEmailingErrorCodes.VaultMissingToken);
            }
        }

        throw new BusinessException(TestModuleEmailingErrorCodes.VaultMissingToken);
    }

    private static async Task<EmailSecretModel> ReadSecretAsync(HttpResponseMessage response)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            if (!document.RootElement.TryGetProperty(TestModuleEmailingVaultConstants.DataPropertyName, out var outer) ||
                !outer.TryGetProperty(TestModuleEmailingVaultConstants.DataPropertyName, out var data) ||
                data.ValueKind != JsonValueKind.Object)
            {
                throw new BusinessException(TestModuleEmailingErrorCodes.VaultInvalidPayload);
            }

            return new EmailSecretModel
            {
                SmtpUsername = ReadOptional(data, TestModuleEmailingVaultConstants.SmtpUsernamePropertyName),
                SmtpPassword = ReadOptional(data, TestModuleEmailingVaultConstants.SmtpPasswordPropertyName),
                GoogleClientSecret = ReadOptional(data, TestModuleEmailingVaultConstants.GoogleClientSecretPropertyName),
                GoogleRefreshToken = ReadOptional(data, TestModuleEmailingVaultConstants.GoogleRefreshTokenPropertyName)
            };
        }
        catch (JsonException)
        {
            throw new BusinessException(TestModuleEmailingErrorCodes.VaultInvalidPayload);
        }
    }

    private static string? ReadOptional(JsonElement data, string propertyName)
    {
        if (!data.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            throw new BusinessException(TestModuleEmailingErrorCodes.VaultInvalidPayload);
        }

        return value.GetString();
    }

    private static string BuildDataPath(string mount, string path)
    {
        if (IsNavigationSegment(mount) || string.IsNullOrWhiteSpace(path))
        {
            throw new BusinessException(TestModuleEmailingErrorCodes.VaultPathInvalid);
        }

        var segments = path
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0 || segments.Any(IsNavigationSegment))
        {
            throw new BusinessException(TestModuleEmailingErrorCodes.VaultPathInvalid);
        }

        var encodedMount = Uri.EscapeDataString(mount);
        var encodedPath = string.Join('/', segments.Select(Uri.EscapeDataString));
        return $"v1/{encodedMount}/data/{encodedPath}";
    }

    private static bool IsNavigationSegment(string value)
        => string.IsNullOrWhiteSpace(value) || value is "." or "..";

    private static string EnsureTrailingSlash(string address)
        => address.EndsWith('/') ? address : $"{address}/";

    private static void EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        // Vault response body bilincli olarak okunmaz: provider hata govdesi secret tasiyabilir.
        throw new BusinessException(TestModuleEmailingErrorCodes.VaultRequestFailed)
            .WithData(TestModuleEmailingVaultConstants.StatusCodeDataKey, (int)response.StatusCode);
    }
}
