using System.Linq;
using FluentValidation;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Dtos.Bridge.Invariants;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Invariants;

// islevi: Is degismezi girdisinin kapali desen kodu tasidigini dogrular.
// sistemdeki gorevi: Tanimsiz desen kodunun manager'a ulasmasini engeller.
public sealed class BusinessInvariantRequestDtoValidator : AbstractValidator<BusinessInvariantRequestDto>
{
    public BusinessInvariantRequestDtoValidator()
    {
        RuleFor(input => input.PatternCode).Must(PtnInvariantPatternCodes.All.Contains)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.InvariantPatternInvalid);
    }
}
