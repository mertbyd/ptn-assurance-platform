using Ptn.ApiContractChecker.ExceptionCodes;
using Ptn.ApiContractChecker.Interface.Secrets;
using Ptn.ApiContractChecker.Models.Secrets;
using Volo.Abp;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Sources;

// islevi: SpecSource EF testlerinde Vault dis bagimliligi olmadan secret yazma, okuma ve silme davranisini taklit eder.
// sistemdeki gorevi: Uretim ISecretProvider kontratini koruyarak credential akislarini deterministik ve secretsiz test ortaminda calistirir.
public class InMemorySpecSourceSecretProvider : ISecretProvider
{
    private readonly Dictionary<string, ApiCredentialModel> _credentials = [];

    // Testlerin yazilan kararli Vault yolunu dogrulamasini saglar.
    public IReadOnlyDictionary<string, ApiCredentialModel> Credentials => _credentials;

    // Kayitli credential'i dondurur; yoksa uretim provider'iyla ayni business error'u uretir.
    public Task<ApiCredentialModel> GetApiCredentialAsync(string path)
    {
        if (!_credentials.TryGetValue(path, out var credential))
        {
            throw new BusinessException(SecretExceptionCodes.NotFound);
        }

        return Task.FromResult(credential);
    }

    // Credential'i kararli path altinda yazar veya yeniler.
    public Task SetAsync(string path, ApiCredentialModel credential)
    {
        _credentials[path] = credential;
        return Task.CompletedTask;
    }

    // Credential'i test deposundan kaldirir.
    public Task DeleteAsync(string path)
    {
        _credentials.Remove(path);
        return Task.CompletedTask;
    }
}
