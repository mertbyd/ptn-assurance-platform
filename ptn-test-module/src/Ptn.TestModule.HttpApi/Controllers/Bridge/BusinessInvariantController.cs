using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge.Invariants;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Bridge;
using SystemStandards.Results;

namespace Ptn.TestModule.Controllers.Bridge;

// islevi: Is degismezi degerlendiricisini HTTP'ye acar.
// sistemdeki gorevi: Route, binding ve operation permission'ini tasiyip istegi tek AppService cagrisina yonlendirir.
/// <summary>Is degismezi yoklamasini sunar.</summary>
[Route(PtnInvariantRoutes.Root)]
[ApiExplorerSettings(GroupName = PtnInvariantRoutes.SwaggerGroupName)]
public class BusinessInvariantController : TestModuleController
{
    /// <summary>Degismez AppService'ini lazy cozer.</summary>
    private IBusinessInvariantAppService AppService => LazyGetRequiredService<IBusinessInvariantAppService>();

    /// <summary>Tek is degismezini kapali desen kodu ve olculen degerlerle degerlendirir.</summary>
    /// <returns>Ev standardi icinde kapali gecti/kaldi karari ve gerekce kodu.</returns>
    [HttpPost(PtnInvariantRoutes.Check)]
    [Authorize(TestModulePermissions.Bridge.Invariant)]
    public virtual async Task<Result<BusinessInvariantResultDto>> Check(
        BusinessInvariantRequestDto input,
        CancellationToken cancellationToken)
    {
        var result = await AppService.CheckAsync(input, cancellationToken);
        return result;
    }
}
