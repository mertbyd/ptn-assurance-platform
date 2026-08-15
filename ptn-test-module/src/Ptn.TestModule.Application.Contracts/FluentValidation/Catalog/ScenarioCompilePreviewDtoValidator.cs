using FluentValidation;
using Ptn.TestModule.Constants.Compilation;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.ExceptionCodes.Compilation;

namespace Ptn.TestModule.FluentValidation.Catalog;

// islevi: Derleme onizlemesi taslak boyutu ve snapshot kimligi seklini dogrular.
// sistemdeki gorevi: Public preview girdisinin bicim kapisidir.
public sealed class ScenarioCompilePreviewDtoValidator : AbstractValidator<ScenarioCompilePreviewDto>
{
    public ScenarioCompilePreviewDtoValidator()
    {
        RuleFor(input => input.SourceDocument)
            .NotEmpty().WithErrorCode(TestModuleCompilationErrorCodes.InvalidDocument)
            .Must(value => System.Text.Encoding.UTF8.GetByteCount(value) <= ArazzoCompilationConsts.MaxDocumentBytes)
            .WithErrorCode(TestModuleCompilationErrorCodes.InvalidDocument);
        RuleFor(input => input.SpecSnapshotId)
            .NotEmpty().WithErrorCode(TestModuleCompilationErrorCodes.InvalidDocument);
    }
}
