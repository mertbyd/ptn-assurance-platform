using System.Collections.Generic;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Models.Catalog;
using Ptn.TestModule.Models.Compilation;
using Riok.Mapperly.Abstractions;

namespace Ptn.TestModule.Mappers.Catalog;

// islevi: Senaryo katalogu DTO, domain model, entity ve karar eslemelerini tanimlar.
// sistemdeki gorevi: Application katmanindaki tum katmanlar-arasi donusumlerin tek Mapperly sahibidir.
[Mapper]
public partial class TestScenarioMapper
{
    public partial ScenarioCompilePreviewResultDto Map(ArazzoCompilationResult source);
    public partial TestScenarioCreateModel Map(CreateTestScenarioDto source);
    public partial TestScenarioUpdateModel Map(UpdateTestScenarioDto source);
    public partial TestScenarioMaterialSeal Map(TestScenarioMaterialSealDto source);
    public partial TestScenarioDto Map(TestScenario source);
    public partial List<TestScenarioDto> Map(List<TestScenario> source);
    public partial TestScenarioPublishDecisionDto Map(TestScenarioPublishDecision source);
    public partial TestScenarioScheduleModel Map(UpdateScenarioScheduleDto source);
    public partial ScenarioPublicationCandidate MapToPublicationCandidate(TestScenario source);
}
