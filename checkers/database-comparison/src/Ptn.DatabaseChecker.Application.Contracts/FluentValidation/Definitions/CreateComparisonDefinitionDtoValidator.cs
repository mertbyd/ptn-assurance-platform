using Ptn.DatabaseChecker.Dtos.Definitions;

namespace Ptn.DatabaseChecker.FluentValidation.Definitions;

// islevi: ComparisonDefinition create isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Ortak tarif kurallarini create DTO tipine baglar.
public class CreateComparisonDefinitionDtoValidator : ComparisonDefinitionDtoValidatorBase<CreateComparisonDefinitionDto>
{
    public CreateComparisonDefinitionDtoValidator()
    {
        AddRules(
            x => x.Name,
            x => x.SourceConnectionId,
            x => x.TargetConnectionId,
            x => x.ComparisonTypeId,
            x => x.SourceRoleCode,
            x => x.Description);
    }
}
