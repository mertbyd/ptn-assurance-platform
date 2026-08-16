using Ptn.ApiContractChecker.Interface.Secrets;
using Ptn.ApiContractChecker.Models.Secrets;
using Ptn.ApiContractChecker.Models.Sources;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Adapters.Sources;

// islevi: Spec kaynagi GET istegini opsiyonel Vault credential header'iyla kurar.
// sistemdeki gorevi: Fetch ve reachability adapterlerinin URL/header kurulumunu tek secretsiz cikis noktasinda paylasmasini saglar.
public class SpecSourceHttpRequestFactory : ITransientDependency
{
    private readonly ISecretProvider _secretProvider;

    public SpecSourceHttpRequestFactory(ISecretProvider secretProvider)
    {
        _secretProvider = secretProvider;
    }

    // Secret yolu varsa credential'i request kurulmadan hemen once cozer.
    public async Task<ApiCredentialModel?> ResolveCredentialAsync(string? secretPath)
    {
        if (secretPath == null)
        {
            return null;
        }

        return await _secretProvider.GetApiCredentialAsync(secretPath);
    }

    // Credential'i yalniz giden GET request'ine ekleyip tasima nesnesini kurar.
    public HttpRequestMessage Create(SpecFetchRequestModel input, ApiCredentialModel? credential)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, input.DocumentUri);
        if (credential != null)
        {
            request.Headers.TryAddWithoutValidation(credential.HeaderName, credential.HeaderValue);
        }

        return request;
    }
}
