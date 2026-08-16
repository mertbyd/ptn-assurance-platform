using Ptn.DatabaseChecker.Dtos.Assertions;
using Volo.Abp.Application.Services;

namespace Ptn.DatabaseChecker.Services.Assertions;

// islevi: DB assertion adreslerinin canli katalogdan turetilebilirligini dogrulayan use-case'i tanimlar.
// sistemdeki gorevi: RULE-0006 yayim kapisini HTTP katmanindan bagimsiz Application kontrati olarak sunar.
public interface IAssertionDerivabilityAppService : IApplicationService
{
    // islevi: Bir baglantidaki tum assertion adresleri icin girdi-sirali kapali outcome listesi dondurur.
    Task<DerivabilityResultDto> ValidateDerivabilityAsync(
        DerivabilityRequestDto input,
        CancellationToken cancellationToken = default);
}
