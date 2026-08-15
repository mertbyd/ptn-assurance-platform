using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Bridge.Invariants;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Is degismezi degerlendirme use-case'ini tanimlar.
// sistemdeki gorevi: Arazzo criteria ile ifade edilemeyen korunum ve delta kararini salt-hesap sozlesmesi olarak sunar.
public interface IBusinessInvariantAppService : IApplicationService
{
    /// <summary>Tek is degismezini kapali desen kodu ve olculen degerlerle degerlendirir.</summary>
    Task<BusinessInvariantResultDto> CheckAsync(
        BusinessInvariantRequestDto input,
        CancellationToken cancellationToken);
}
