using System.Collections.Generic;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Arazzo belgesindeki tek bir is akisinin wire seklini tasir.
// sistemdeki gorevi: Adim kimliklerini kaynak sirasini bozmadan cozumlemeye acar.
public class ArazzoWorkflowDto
{
    /// <summary>Is akisinin belge icindeki kimligidir.</summary>
    public string? WorkflowId { get; set; }

    /// <summary>Is akisinin adimlarinin kaynak sirasindaki listesidir.</summary>
    public List<ArazzoStepDto> Steps { get; set; } = [];
}
