using System.Linq;
using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Agent;

// islevi: Tool butce isteginin moment, tool ve sayac seklini dogrular.
// sistemdeki gorevi: Negatif sayaclari ve bilinmeyen kodlari Manager'dan uzak tutar.
public sealed class ToolBudgetRequestDtoValidator : AbstractValidator<ToolBudgetRequestDto>
{
    public ToolBudgetRequestDtoValidator()
    {
        RuleFor(input => input.MomentCode).Must(AgentMomentCodes.All.Contains)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.AgentMomentInvalid);
        RuleFor(input => input.ToolCode).Must(PtnToolCodes.Governed.Contains)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.ToolCodeInvalid);
        RuleFor(input => input.UsedTurns).GreaterThanOrEqualTo(0)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.ToolBudgetInvalid);
        RuleFor(input => input.UsedTokens).GreaterThanOrEqualTo(0)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.ToolBudgetInvalid);
    }
}
