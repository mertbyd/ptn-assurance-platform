using System;
using System.Linq;
using FluentValidation;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.ExceptionCodes.Catalog;

namespace Ptn.TestModule.FluentValidation.Catalog;

// islevi: Yayin kanitindaki sourceDescriptions snapshot kimliklerini dogrular.
// sistemdeki gorevi: Bos, sifir veya yinelenen kaynak referanslarini gate degerlendirmesinden once reddeder.
public sealed class PublishTestScenarioDtoValidator : AbstractValidator<PublishTestScenarioDto>
{
    public PublishTestScenarioDtoValidator()
    {
        RuleFor(input => input.SourceDescriptionSpecSnapshotIds)
            .NotEmpty().WithErrorCode(TestModuleScenarioErrorCodes.Validation.SourceDescriptionsRequired)
            .Must(ids => ids.All(id => id != Guid.Empty) && ids.Distinct().Count() == ids.Count)
            .WithErrorCode(TestModuleScenarioErrorCodes.Validation.SourceDescriptionInvalid);
    }
}
