using System.Collections.Generic;

namespace Ptn.TestModule.Models.Shared;

// islevi: Tamamlanmis bir dis surecin cikis kodunu, akislarini, suresini ve okunan artefaktlarini tasir.
// sistemdeki gorevi: Surec sinirinin gozlemini hukum vermeden Manager'a aktarir.
/// <summary>
/// Dis surec cagrisinin ham sonucunu tasir.
/// </summary>
public class ProcessExecutionOutcome
{
    /// <summary>Surecin islenmemis cikis kodudur.</summary>
    public int ExitCode { get; set; }

    /// <summary>Toplandiysa standart cikti akisidir.</summary>
    public string StandardOutput { get; set; } = string.Empty;

    /// <summary>Toplandiysa standart hata akisidir.</summary>
    public string StandardError { get; set; } = string.Empty;

    /// <summary>Surec sinirinda olculen toplam calisma suresidir.</summary>
    public long DurationMs { get; set; }

    /// <summary>Plandaki yollara gore okunan artefakt icerikleridir; uretilmeyen dosya null kalir.</summary>
    public IReadOnlyDictionary<string, string?> OutputFiles { get; set; } =
        new Dictionary<string, string?>();
}
