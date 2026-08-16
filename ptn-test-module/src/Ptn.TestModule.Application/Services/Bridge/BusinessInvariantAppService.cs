using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Ptn.TestModule.Dtos.Bridge.Invariants;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Mappers.Bridge;
using Volo.Abp;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Is degismezi girdisini dogrular, Manager'i cagirir ve Mapperly ile DTO dondurur.
// sistemdeki gorevi: Saf aritmetik karari HTTP ve MCP yuzeyine baglayan ince Application orkestrasyonudur.
[RemoteService(IsEnabled = false)]
public class BusinessInvariantAppService : TestModuleAppService, IBusinessInvariantAppService
{
    private static readonly BusinessInvariantMapper Mapper = new();
    private readonly BusinessInvariantManager _manager;
    private readonly IValidator<BusinessInvariantRequestDto> _validator;

    // Degismez degerlendiricisini dogrulama ve mapping sinirina baglar.
    public BusinessInvariantAppService(
        BusinessInvariantManager manager,
        IValidator<BusinessInvariantRequestDto> validator)
    {
        _manager = manager;
        _validator = validator;
    }

    // Tek degismez istegini dogrulayip kapali gecti/kaldi sonucuna cevirir.
    public async Task<BusinessInvariantResultDto> CheckAsync(
        BusinessInvariantRequestDto input,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(input, cancellationToken);
        return Mapper.Map(_manager.Evaluate(Mapper.Map(input)));
    }
}
