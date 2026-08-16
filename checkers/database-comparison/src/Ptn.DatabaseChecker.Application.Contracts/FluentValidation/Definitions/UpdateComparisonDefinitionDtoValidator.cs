using Ptn.DatabaseChecker.Dtos.Definitions;

namespace Ptn.DatabaseChecker.FluentValidation.Definitions;

// islevi: ComparisonDefinition update isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Ortak tarif kurallarini update DTO tipine baglar.
public class UpdateComparisonDefinitionDtoValidator : ComparisonDefinitionDtoValidatorBase<UpdateComparisonDefinitionDto>
{
    public UpdateComparisonDefinitionDtoValidator()
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
