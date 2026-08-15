using FluentValidation;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.ExceptionCodes.Runs;

namespace Ptn.TestModule.FluentValidation.Catalog;

// islevi: Karantina isteginin sure zorunlulugunu ve gerekce uzunlugunu istek sinirinda dogrular.
// sistemdeki gorevi: Suresiz karantinayi Manager'a ulasmadan once reddeder (PLAN-0003 TM-28 §2.5).
/// <summary>Karantina istek DTO'sunun bicim kurallarini tanimlar.</summary>
public sealed class QuarantineTestScenarioDtoValidator : AbstractValidator<QuarantineTestScenarioDto>
{
    /// <summary>Karantina bitisi ve gerekce kurallarini kurar.</summary>
    public QuarantineTestScenarioDtoValidator()
    {
        RuleFor(input => input.QuarantineUntil)
            .NotNull()
            .WithErrorCode(TestModuleRunErrorCodes.QuarantineRequiresExpiry);
        RuleFor(input => input.QuarantineReason)
            .MaximumLength(TestScenarioConsts.MaxQuarantineReasonLength)
            .When(input => !string.IsNullOrWhiteSpace(input.QuarantineReason))
            .WithErrorCode(TestModuleRunErrorCodes.QuarantineWindowInvalid);
    }
}
