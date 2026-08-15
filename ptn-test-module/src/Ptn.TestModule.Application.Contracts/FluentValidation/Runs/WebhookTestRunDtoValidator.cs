using FluentValidation;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;

namespace Ptn.TestModule.FluentValidation.Runs;

// islevi: Webhook kosum tetiginin bounded bicimini dogrular.
// sistemdeki gorevi: Teslim kimligi, senaryo anahtari ve ortam alanlarini Manager'dan once kapilar.
public sealed class WebhookTestRunDtoValidator : AbstractValidator<WebhookTestRunDto>
{
    public WebhookTestRunDtoValidator()
    {
        RuleFor(input => input.DeliveryId)
            .NotEmpty()
            .MaximumLength(TestModuleRunSettingNames.MaxWebhookDeliveryIdLength)
            .WithErrorCode(TestModuleRunErrorCodes.WebhookDeliveryIdInvalid);
        RuleFor(input => input.ScenarioKey)
            .NotEmpty()
            .MaximumLength(TestScenarioConsts.MaxScenarioKeyLength)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.ScenarioKeyTooLong);
        RuleFor(input => input.EnvironmentKey)
            .MaximumLength(TestRunConsts.MaxEnvironmentKeyLength)
            .WithErrorCode(TestModuleRunErrorCodes.Validation.EnvironmentKeyTooLong);
    }
}
