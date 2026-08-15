using System.Collections.Generic;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Arazzo belgesinin kosum kararlari icin okunan wire seklini tasir.
// sistemdeki gorevi: YAML cozumlemesinin tek hedefidir; belge hakkindaki hicbir kural bu tipte yasamaz.
public class ArazzoDocumentDto
{
    /// <summary>Belgenin bildirdigi Arazzo surumudur.</summary>
    public string? Arazzo { get; set; }

    /// <summary>Belgedeki is akislarinin kaynak sirasindaki listesidir.</summary>
    public List<ArazzoWorkflowDto> Workflows { get; set; } = [];
}
