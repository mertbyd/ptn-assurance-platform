using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Models.Runs;
using Riok.Mapperly.Abstractions;

namespace Ptn.TestModule.Mappers.Runs;

// islevi: Test kosumu DTO, domain model, aggregate ve claim eslemelerini tanimlar.
// sistemdeki gorevi: Kosum dikeyindeki tum katmanlar-arasi donusumlerin tek Mapperly sahibidir.
/// <summary>Test kosumu dikeyinin saf Mapperly eslemelerini tasir.</summary>
[Mapper]
public partial class TestRunMapper
{
    /// <summary>Create DTO'sunu Manager girdisi olan domain modeline cevirir.</summary>
    public partial TestRunCreateModel Map(CreateTestRunDto source);

    /// <summary>Terminal DTO'sunu Manager girdisi olan domain modeline cevirir.</summary>
    [MapperIgnoreTarget(nameof(TestRunTerminalModel.RunStatusCode))]
    public partial TestRunTerminalModel Map(WriteTestRunTerminalDto source);

    /// <summary>Nested bulgu DTO'sunu kalici bulgu domain modeline cevirir.</summary>
    public partial TestResultFindingModel Map(TestResultFindingInputDto source);

    /// <summary>TestRun aggregate'ini public okuma DTO'suna cevirir.</summary>
    public partial TestRunDto Map(TestRun source);

    /// <summary>TestRunResult aggregate'ini bulgulu public DTO'ya cevirir.</summary>
    public partial TestRunResultDto Map(TestRunResult source);

    /// <summary>TestResultFinding entity'sini public bulgu DTO'suna cevirir.</summary>
    public partial TestResultFindingDto Map(TestResultFinding source);

    /// <summary>Domain claim sonucunu public claim DTO'suna cevirir.</summary>
    public partial TestRunClaimDto Map(TestRunClaimResult source);
}
