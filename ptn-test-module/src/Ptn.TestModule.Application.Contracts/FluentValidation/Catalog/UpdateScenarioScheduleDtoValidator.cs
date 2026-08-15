using FluentValidation;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.ExceptionCodes.Catalog;

namespace Ptn.TestModule.FluentValidation.Catalog;

// islevi: Zamanlama girdisinin bounded bicimini dogrular.
// sistemdeki gorevi: Cron varligi ve uzunlugunu kapida tutar; ifadenin ayristirilmasi Manager'in isidir.
public sealed class UpdateScenarioScheduleDtoValidator : AbstractValidator<UpdateScenarioScheduleDto>
{
    public UpdateScenarioScheduleDtoValidator()
    {
        RuleFor(input => input.ScheduleCron)
            .NotEmpty()
            .When(input => input.ScheduleEnabled)
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.ScheduleCronRequired);
        RuleFor(input => input.ScheduleCron)
            .MaximumLength(TestScenarioConsts.MaxScheduleCronLength)
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.ScheduleCronTooLong);
    }
}
