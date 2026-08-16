using FluentValidation;
using Ptn.DatabaseChecker.Dtos.Runs;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.FluentValidation.Scopes;

namespace Ptn.DatabaseChecker.FluentValidation.Runs;

// islevi: Calistir-ve-sakla isteginin girdi-format kurallarini tanimlar.
// sistemdeki gorevi: Tarif kimligini ve bu calistirmaya ozel gecici kapsam kurallarini dogrular; tarif varlik kontrolu manager katmaninda yapilir.
public class ExecuteComparisonRunDtoValidator : AbstractValidator<ExecuteComparisonRunDto>
{
    public ExecuteComparisonRunDtoValidator()
    {
        // ComparisonDefinitionId bos Guid olamaz; tarif varlik kontrolu manager katmaninda yapilir.
        RuleFor(x => x.ComparisonDefinitionId)
            .NotEmpty().WithMessage(ComparisonRunExceptionCodes.Validation.ExecuteComparisonDefinitionIdRequired);

        // Null veya bos liste kisit yoktur; verilen her gecici kapsam kurali ortak validator ile dogrulanir.
        RuleForEach(x => x.ScopeRules)
            .SetValidator(new ScopeRuleDtoValidator());
    }
}
