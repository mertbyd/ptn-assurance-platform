using Microsoft.Extensions.Options;
using Ptn.ApiContractChecker.Configuration;
using Ptn.ApiContractChecker.Constants.Sources;
using Ptn.ApiContractChecker.ExceptionCodes.Sources;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Models.Sources;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Adapters.Sources;

// islevi: Canli spec govdesini ortak HTTP tasimasiyla cekip guvenilmeyen icerik guard'larini sirayla uygular.
// sistemdeki gorevi: ISpecFetcherClient domain kontratini resilience ve sinirli stream okuma ayrintilariyla gerceklestirir.
[DisableConventionalRegistration]
public class SpecFetcherClient : ISpecFetcherClient
{
    private readonly HttpClient _httpClient;
    private readonly SpecSourceHttpRequestFactory _requestFactory;
    private readonly SpecFetchOptions _options;

    public SpecFetcherClient(
        HttpClient httpClient,
        SpecSourceHttpRequestFactory requestFactory,
        IOptions<SpecFetchOptions> options)
    {
        _httpClient = httpClient;
        _requestFactory = requestFactory;
        _options = options.Value;
    }

    // Credential'i cozer, govdeyi dort guard'dan sirayla gecirir ve ham bayti dondurur.
    // Tasima hatasi burada yakalanmaz: HttpClient exception'i ABP exception middleware'ine gider,
    // arka plan isi yolunda da isin yeniden denenmesini saglar. Sarmalayici katman eklenmez.
    public async Task<SpecFetchResultModel> FetchAsync(SpecFetchRequestModel request)
    {
        var credential = await _requestFactory.ResolveCredentialAsync(request.VaultSecretPath);
        using var httpRequest = _requestFactory.Create(request, credential);
        using var response = await _httpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead);

        EnsureSuccessfulStatus(response);
        var mediaType = GetAllowedMediaType(response);
        EnsureDeclaredSizeWithinLimit(response);
        var content = await ReadWithinLimitAsync(response);
        EnsureContentIsNotEmpty(content);

        return new SpecFetchResultModel(content, mediaType);
    }

    // Ilk guard olarak basarisiz HTTP durumlarini icerik incelenmeden reddeder.
    private static void EnsureSuccessfulStatus(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new BusinessException(SpecSourceExceptionCodes.FetchHttpStatusRejected);
        }
    }

    // Ikinci guard olarak yalniz yapilandirilmis izin listesindeki medya tipini kabul eder.
    private string GetAllowedMediaType(HttpResponseMessage response)
    {
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (mediaType == null ||
            !_options.AllowedMediaTypes.Contains(mediaType, StringComparer.OrdinalIgnoreCase))
        {
            throw new BusinessException(SpecSourceExceptionCodes.FetchContentTypeRejected);
        }

        return mediaType.ToLowerInvariant();
    }

    // Ucuncu guard'in hizli yolunda dogru Content-Length sinir ustuyse govdeyi acmadan reddeder.
    private void EnsureDeclaredSizeWithinLimit(HttpResponseMessage response)
    {
        if (response.Content.Headers.ContentLength > _options.MaxContentSizeBytes)
        {
            throw new BusinessException(SpecSourceExceptionCodes.FetchContentTooLarge);
        }
    }

    // Content-Length eksik veya yanlis olsa da azami boyuttan bir bayt sonra okumayi keser.
    private async Task<byte[]> ReadWithinLimitAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var output = new MemoryStream(
            Math.Min(_options.MaxContentSizeBytes, SpecSourceConsts.FetchReadBufferSizeBytes));
        var buffer = new byte[SpecSourceConsts.FetchReadBufferSizeBytes];
        var totalBytesRead = 0;

        while (true)
        {
            var remainingWithOverflowProbe = (long)_options.MaxContentSizeBytes - totalBytesRead + 1;
            var requestedBytes = (int)Math.Min(buffer.Length, remainingWithOverflowProbe);
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, requestedBytes));
            if (bytesRead == 0)
            {
                return output.ToArray();
            }

            totalBytesRead += bytesRead;
            if (totalBytesRead > _options.MaxContentSizeBytes)
            {
                throw new BusinessException(SpecSourceExceptionCodes.FetchContentTooLarge);
            }

            await output.WriteAsync(buffer.AsMemory(0, bytesRead));
        }
    }

    // Dorduncu guard olarak sifir baytlik govdeyi gecersiz spec kabul eder.
    private static void EnsureContentIsNotEmpty(byte[] content)
    {
        if (content.Length == 0)
        {
            throw new BusinessException(SpecSourceExceptionCodes.FetchContentEmpty);
        }
    }
}
