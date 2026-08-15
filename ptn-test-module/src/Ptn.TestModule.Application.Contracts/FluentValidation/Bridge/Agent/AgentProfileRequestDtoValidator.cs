using System.Linq;
using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Agent;

// islevi: Ajan profil isteginin kapali moment kodunu dogrular.
// sistemdeki gorevi: Bilinmeyen an kodunu setting cozumlemesinden once reddeder.
public sealed class AgentProfileRequestDtoValidator : AbstractValidator<AgentProfileRequestDto>
{
    public AgentProfileRequestDtoValidator() => RuleFor(input => input.MomentCode)
        .Must(AgentMomentCodes.All.Contains).WithMessage(TestModuleBridgeErrorCodes.Validation.AgentMomentInvalid);
}
