using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Models.Runs;

namespace Ptn.TestModule.Interface.Runs;

// islevi: Mantiksal secret referansini kosum aninda tek kimlik basligina cozen sinirdir.
// sistemdeki gorevi: Domain'in secret deposu SDK'sina baglanmadan korumali hedefe kimlik tasimasini saglar.
public interface IRunCredentialPort
{
    // Referans bos ise null doner; dolu ise cozulmus baslik doner, cozulemezse saglayici hatasini yukseltir.
    Task<RunCredential?> ResolveAsync(string secretRef, CancellationToken cancellationToken = default);
}
