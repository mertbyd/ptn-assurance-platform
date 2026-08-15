using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Normalize edilmis operasyon baglama sonucu ve skorlanmis onerileri tasir.
// sistemdeki gorevi: API checker sonucunu ham outcome casing'i veya DTO tipi sizdirmadan domaine verir.
public sealed class OperationBinding
{
    public string OutcomeCode { get; set; } = string.Empty;
    public List<OperationSuggestion> Suggestions { get; set; } = [];
}
