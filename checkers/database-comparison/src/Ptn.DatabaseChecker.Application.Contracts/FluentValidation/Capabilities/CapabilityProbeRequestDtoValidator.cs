using FluentValidation;
using Ptn.DatabaseChecker.Dtos.Capabilities;
using Ptn.DatabaseChecker.ExceptionCodes;

namespace Ptn.DatabaseChecker.FluentValidation.Capabilities;

// islevi: Capability probe request'in zorunlu baglanti kimligini dogrular.
// sistemdeki gorevi: Canli baglanti ve sandbox kararlarini Manager'a birakirken public girdi seklini I/O oncesi korur.
public sealed class CapabilityProbeRequestDtoValidator : AbstractValidator<CapabilityProbeRequestDto>
{
    // islevi: Probe request-shape kuralini kararli validation koduyla kaydeder.
    public CapabilityProbeRequestDtoValidator()
    {
        RuleFor(input => input.ConnectionId)
            .NotEmpty().WithMessage(DataComparisonExceptionCodes.WriteSetValidation.ConnectionIdRequired);
    }
}
