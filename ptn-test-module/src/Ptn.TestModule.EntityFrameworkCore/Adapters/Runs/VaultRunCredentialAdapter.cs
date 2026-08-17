using System.Threading;
using System.Threading.Tasks;
using Ptn.ApiContractChecker.Interface.Secrets;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp.DependencyInjection;

namespace Ptn.TestModule.EntityFrameworkCore.Adapters.Runs;

// islevi: Mantiksal secret referansini composition host'un Vault saglayicisi uzerinden kimlik basligina cevirir.
// sistemdeki gorevi: Secret deposu sozlesmesini provider katmaninda tutup Domain'e sadece kendi modelini verir.
/// <summary>Kosum API sirrini mevcut secret saglayicisindan cozen provider adapter'idir.</summary>
[ExposeServices(typeof(IRunCredentialPort))]
public sealed class VaultRunCredentialAdapter : IRunCredentialPort, ITransientDependency
{
    /// <summary>Host tarafindan Vault'a baglanan ortak secret saglayicisidir.</summary>
    private readonly ISecretProvider _secretProvider;

    // Secret saglayicisini kosum kimlik sinirina baglar.
    /// <summary>Adapter'i aktif secret saglayicisi ile kurar.</summary>
    public VaultRunCredentialAdapter(ISecretProvider secretProvider)
    {
        _secretProvider = secretProvider;
    }

    /// <inheritdoc />
    public async Task<RunCredential?> ResolveAsync(string secretRef, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(secretRef))
        {
            return null;
        }

        var credential = await _secretProvider.GetApiCredentialAsync(secretRef);
        return new RunCredential
        {
            HeaderName = credential.HeaderName,
            HeaderValue = credential.HeaderValue
        };
    }
}
