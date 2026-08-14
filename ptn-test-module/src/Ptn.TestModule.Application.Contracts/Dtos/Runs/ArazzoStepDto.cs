using System.Collections.Generic;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Bir is akisi adiminin kimligini ve tum kriter tasiyan dallarini wire seklinde tasir.
// sistemdeki gorevi: Basari, basari-sonrasi ve basarisizlik dallarindaki kriterleri kural uygulamadan acar.
public class ArazzoStepDto
{
    /// <summary>Adimin belge icindeki kararli kimligidir.</summary>
    public string? StepId { get; set; }

    /// <summary>Adimin basari kriterleridir.</summary>
    public List<ArazzoCriterionDto> SuccessCriteria { get; set; } = [];

    /// <summary>Adimin basari sonrasi aksiyonlaridir.</summary>
    public List<ArazzoActionDto> OnSuccess { get; set; } = [];

    /// <summary>Adimin basarisizlik aksiyonlaridir.</summary>
    public List<ArazzoActionDto> OnFailure { get; set; } = [];
}
