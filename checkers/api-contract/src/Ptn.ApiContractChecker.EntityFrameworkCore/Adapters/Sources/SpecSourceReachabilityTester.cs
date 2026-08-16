using Ptn.ApiContractChecker.Constants.Sources;
using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.ExceptionCodes.Sources;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Models.Secrets;
using Ptn.ApiContractChecker.Models.Sources;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Adapters.Sources;

// islevi: SpecSource dokumanlarinin HTTP erisilebilirligini Vault kimligiyle yoklar.
// sistemdeki gorevi: Manager'in provider kontratini cekim hattiyla ayni tasima ve ayni hata haritasi uzerinden gerceklestirir.
[ExposeServices(typeof(ISpecSourceReachabilityTester))]
public class SpecSourceReachabilityTester : ISpecSourceReachabilityTester, ITransientDependency
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SpecSourceHttpRequestFactory _requestFactory;

    public SpecSourceReachabilityTester(
        IHttpClientFactory httpClientFactory,
        SpecSourceHttpRequestFactory requestFactory)
    {
        _httpClientFactory = httpClientFactory;
        _requestFactory = requestFactory;
    }

    // Aktif dokumanlari paralel yoklar ve ilk basarisiz sonucu kaynak ozeti olarak dondurur.
    public async Task<SpecSourceReachabilityModel> TestAsync(
        SpecSource source,
        IReadOnlyCollection<SpecDocument> documents)
    {
        // Vault cagrisi yoklamanin disindadir: secret deposu arizasi erisilemezlik olarak raporlanmaz.
        var credential = await _requestFactory.ResolveCredentialAsync(source.VaultSecretPath);
        var client = _httpClientFactory.CreateClient(SpecSourceConsts.HttpClientName);
        var probes = documents.Select(document => ProbeDocumentAsync(client, source, document, credential));
        var results = await Task.WhenAll(probes);
        var failure = results.FirstOrDefault(result => !result.IsReachable);

        if (failure != null)
        {
            failure.TestedDocumentCount = documents.Count;
            return failure;
        }

        return new SpecSourceReachabilityModel
        {
            IsReachable = true,
            TestedDocumentCount = documents.Count,
            StatusCode = results[0].StatusCode
        };
    }

    // Tek dokuman URL'sine GET yapar ve sonucu yalniz durum kodu ile kararli hata koduna indirger.
    private async Task<SpecSourceReachabilityModel> ProbeDocumentAsync(
        HttpClient client,
        SpecSource source,
        SpecDocument document,
        ApiCredentialModel? credential)
    {
        var input = new SpecFetchRequestModel(source.BaseUrl, document.Path, source.VaultSecretPath);
        using var request = _requestFactory.Create(input, credential);

        try
        {
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            return new SpecSourceReachabilityModel
            {
                IsReachable = response.IsSuccessStatusCode,
                TestedDocumentCount = 1,
                StatusCode = (int)response.StatusCode,
                ErrorMessage = response.IsSuccessStatusCode
                    ? null
                    : SpecSourceExceptionCodes.FetchHttpStatusRejected
            };
        }
        catch (Exception exception) when (SpecSourceTransportErrors.IsTransportFailure(exception))
        {
            // Ham exception metni ic ag ayrintisi tasir; disariya yalniz cekim hattiyla ortak kararli kod verilir.
            return new SpecSourceReachabilityModel
            {
                IsReachable = false,
                TestedDocumentCount = 1,
                ErrorMessage = SpecSourceTransportErrors.Resolve(exception)
            };
        }
    }
}
