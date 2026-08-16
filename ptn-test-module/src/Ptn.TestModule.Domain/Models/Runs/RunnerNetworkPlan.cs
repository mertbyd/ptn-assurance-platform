using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Runner konteynerinin ag modunu ve ek host kayitlarini tek modelde tasir.
// sistemdeki gorevi: Hedef sistem konteyner disinda kostugunda docker ag argumanlarinin kaynagidir.
/// <summary>
/// Dis runner konteynerinin ag sinirini tasir.
/// </summary>
public class RunnerNetworkPlan
{
    /// <summary>Konteynerin baglanacagi docker ag modudur; bos ise ag argumani yazilmaz.</summary>
    public string Mode { get; set; } = string.Empty;

    /// <summary>Konteyner icine eklenecek ek host kayitlaridir; bos ise ek host argumani yazilmaz.</summary>
    public IReadOnlyList<string> ExtraHosts { get; set; } = [];
}
