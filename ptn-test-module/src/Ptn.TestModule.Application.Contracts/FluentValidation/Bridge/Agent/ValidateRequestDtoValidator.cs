using System.Linq;
using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.ExceptionCodes.Catalog;
using Ptn.TestModule.FluentValidation.Catalog;
using Ptn.TestModule.FluentValidation.Bridge.Database;

namespace Ptn.TestModule.FluentValidation.Bridge.Agent;

// islevi: Validate girdisinin profil, snapshot, operasyon, assertion ve response format seklini dogrular.
// sistemdeki gorevi: Bos assertion referansli bir istegin yayin kapisina Allow adayi olarak girmesini engeller.
public sealed class ValidateRequestDtoValidator : AbstractValidator<ValidateRequestDto>
{
    public ValidateRequestDtoValidator()
    {
        RuleFor(input => input.ProfileKey).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.ProfileKeyRequired);
        RuleFor(input => input.ConnectionId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.ConnectionIdRequired);
        RuleFor(input => input.SpecSnapshotId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.SnapshotIdRequired);
        RuleForEach(input => input.AssertionReferenceIds).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.AssertionReferenceRequired);
        RuleFor(input => input.SourceDocument)
            .NotEmpty().When(input => input.SourceDocument is not null)
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.SourceDocumentRequired);
        RuleFor(input => input.MaterialSeal!)
            .SetValidator(new TestScenarioMaterialSealDtoValidator())
            .When(input => input.MaterialSeal is not null);
        RuleFor(input => input.MaterialSeal)
            .Must((input, seal) => seal is null ||
                                   seal.SpecSnapshotId == input.SpecSnapshotId &&
                                   seal.DbConnectionId == input.ConnectionId)
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.MaterialIdentityInvalid);
        RuleForEach(input => input.DatabaseAssertions)
            .SetValidator(new DatabaseDerivabilityAddressDtoValidator());
        RuleFor(input => input.ResponseFormat).Must(PtnResponseFormatCodes.All.Contains)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.ResponseFormatInvalid);
    }
}
