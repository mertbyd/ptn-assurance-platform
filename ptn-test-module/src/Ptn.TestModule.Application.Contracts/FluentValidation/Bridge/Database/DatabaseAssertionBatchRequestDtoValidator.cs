using FluentValidation;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Database;

// islevi: Batch assertion girdisinin bos olmamasini ve her istegin sekil kurallarini dogrular.
// sistemdeki gorevi: Liste girdisini tek Application.Contracts validation sinirinda tutar.
public sealed class DatabaseAssertionBatchRequestDtoValidator : AbstractValidator<DatabaseAssertionBatchRequestDto>
{
    public DatabaseAssertionBatchRequestDtoValidator()
    {
        RuleFor(input => input.Requests).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.BatchRequired);
        RuleForEach(input => input.Requests).NotNull().WithMessage(TestModuleBridgeErrorCodes.Validation.RequestRequired)
            .SetValidator(new DatabaseAssertionRequestDtoValidator());
    }
}
