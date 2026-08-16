using FluentValidation;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Dtos.Authoring;
using Ptn.TestModule.ExceptionCodes.Catalog;
using System;
using System.Linq;

namespace Ptn.TestModule.FluentValidation.Authoring;

public class AddDatabaseAuthoringStepDtoValidator : AbstractValidator<AddDatabaseAuthoringStepDto>
{
    public AddDatabaseAuthoringStepDtoValidator()
    {
        RuleFor(x => x.StepId)
            .NotEmpty().WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringStepIdInvalid);

        RuleFor(x => x.TableReferenceId)
            .NotEmpty().WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringTableReferenceInvalid);

        RuleFor(x => x.OperationCode)
            .NotEmpty().WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringOperationCodeInvalid);

        // Kural: TimeoutMs ve PollIntervalMs pozitif olmalidir
        RuleFor(x => x.TimeoutMs)
            .GreaterThan(0).When(x => x.TimeoutMs != 0).WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringTimeoutInvalid);

        RuleFor(x => x.PollIntervalMs)
            .GreaterThan(0).When(x => x.PollIntervalMs != 0).WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringPollIntervalInvalid);

        // Kural: Her Expectation kapali alan ve kapali matcher tasimaktadir
        RuleForEach(x => x.Expectations).ChildRules(expectation =>
        {
            expectation.RuleFor(e => e.ColumnName)
                .NotEmpty().WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringColumnNameInvalid);

            expectation.RuleFor(e => e.MatcherCode)
                .Must(code => PtnDatabaseMatcherCodes.All.Contains(code))
                .WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringMatcherCodeInvalid);
        });
    }
}
