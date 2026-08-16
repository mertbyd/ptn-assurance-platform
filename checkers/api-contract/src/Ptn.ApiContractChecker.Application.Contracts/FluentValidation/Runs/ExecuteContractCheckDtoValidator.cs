using FluentValidation;
using Ptn.ApiContractChecker.Constants.Runs;
using Ptn.ApiContractChecker.Dtos.Runs;
using Ptn.ApiContractChecker.ExceptionCodes.Runs;

namespace Ptn.ApiContractChecker.FluentValidation.Runs;

// islevi: Asenkron contract-check tetikleme isteginin snapshot ve kapsam bicimini dogrular.
// sistemdeki gorevi: Snapshot varlik ve tenant kararini manager'a birakip bos kimlik ve sinirsiz job payload'ini engeller.
public class ExecuteContractCheckDtoValidator : AbstractValidator<ExecuteContractCheckDto>
{
    public ExecuteContractCheckDtoValidator()
    {
        // Referans snapshot kimligi istemci tarafinda bos birakilamaz.
        RuleFor(input => input.BaseSnapshotId)
            .NotEmpty().WithMessage(ContractCheckRunExceptionCodes.Validation.BaseSnapshotRequired);

        // Aday snapshot kimligi istemci tarafinda bos birakilamaz.
        RuleFor(input => input.TargetSnapshotId)
            .NotEmpty().WithMessage(ContractCheckRunExceptionCodes.Validation.TargetSnapshotRequired);

        // Job payload'i yalniz sinirli sayida gecici kural tasir.
        RuleFor(input => input.ScopeRules)
            .Must(rules => rules == null || rules.Count <= ContractCheckRunConsts.MaxScopeRuleCount)
            .WithMessage(ContractCheckRunExceptionCodes.Validation.ScopeRuleLimitExceeded);

        // Her gecici kapsam kurali ayni kararli kod sozlesmesine uyar.
        RuleForEach(input => input.ScopeRules)
            .SetValidator(new ContractCheckScopeRuleDtoValidator());
    }
}
