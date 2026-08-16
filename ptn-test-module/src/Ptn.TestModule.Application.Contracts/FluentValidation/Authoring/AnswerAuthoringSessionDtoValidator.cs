using FluentValidation;
using Ptn.TestModule.Dtos.Authoring;
using Ptn.TestModule.ExceptionCodes.Catalog;

namespace Ptn.TestModule.FluentValidation.Authoring;

// islevi: Kapali authoring cevabinin soru kodu ve secenek degeri seklini dogrular.
// sistemdeki gorevi: Bos cevabin domain secenek karsilastirmasina ulasmasini engeller.
public sealed class AnswerAuthoringSessionDtoValidator : AbstractValidator<AnswerAuthoringSessionDto>
{
    public AnswerAuthoringSessionDtoValidator()
    {
        RuleFor(input => input.QuestionCode)
            .NotEmpty().WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringQuestionCodeRequired);
        RuleFor(input => input.SelectedOption)
            .NotEmpty().WithErrorCode(TestModuleScenarioErrorCodes.Validation.AuthoringSelectedOptionRequired);
    }
}
