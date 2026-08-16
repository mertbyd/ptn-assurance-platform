using FluentValidation;
using Ptn.ApiContractChecker.Constants.Runs;
using Ptn.ApiContractChecker.Dtos.Runs;
using Ptn.ApiContractChecker.ExceptionCodes.Runs;

namespace Ptn.ApiContractChecker.FluentValidation.Runs;

// islevi: Gecici contract-check kapsam kuralinin kod ve desen bicimini dogrular.
// sistemdeki gorevi: Repository gerektirmeyen request-shape hatalarini job kuyruguna girmeden durdurur.
public class ContractCheckScopeRuleDtoValidator : AbstractValidator<ContractCheckScopeRuleDto>
{
    public ContractCheckScopeRuleDtoValidator()
    {
        // Kural turu zorunludur ve Domain.Shared kararli kodlarindan biri olmalidir.
        RuleFor(rule => rule.KindCode)
            .NotEmpty().WithMessage(ContractCheckRunExceptionCodes.Validation.ScopeKindRequired)
            .Must(ContractCheckScopeCodes.Kinds.All.Contains)
            .WithMessage(ContractCheckRunExceptionCodes.Validation.ScopeKindInvalid);

        // Kural hedefi zorunludur ve desteklenen OpenAPI kimliklerinden biri olmalidir.
        RuleFor(rule => rule.TargetCode)
            .NotEmpty().WithMessage(ContractCheckRunExceptionCodes.Validation.ScopeTargetRequired)
            .Must(ContractCheckScopeCodes.Targets.All.Contains)
            .WithMessage(ContractCheckRunExceptionCodes.Validation.ScopeTargetInvalid);

        // Eslesme deseni bos olamaz ve job payload'i sinirini asamaz.
        RuleFor(rule => rule.Pattern)
            .NotEmpty().WithMessage(ContractCheckRunExceptionCodes.Validation.ScopePatternRequired)
            .MaximumLength(ContractCheckRunConsts.MaxScopePatternLength)
            .WithMessage(ContractCheckRunExceptionCodes.Validation.ScopePatternMaxLength);
    }
}
