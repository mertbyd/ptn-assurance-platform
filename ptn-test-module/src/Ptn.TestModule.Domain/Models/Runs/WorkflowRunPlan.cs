using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Runner surecinin executable, argument, ortam degiskeni, dosya adi ve sert kill butcesini tasir.
// sistemdeki gorevi: Hangi imajin hangi bayraklarla kosacagi kararini Domain Manager'da tutup Application surecine veri olarak gecer.
/// <summary>
/// Dis runner surecinin kararli cagri planini tasir.
/// </summary>
public class WorkflowRunPlan
{
    /// <summary>Konteyner surecini baslatan executable adidir.</summary>
    public string Executable { get; set; } = string.Empty;

    /// <summary>Shell birlestirmesi yapilmadan gecirilecek argument listesidir.</summary>
    public IReadOnlyList<string> Arguments { get; set; } = [];

    /// <summary>Secret tasiyan girdilerin process listesine dusmeden gectigi ortam degiskenleridir.</summary>
    public IReadOnlyDictionary<string, string> EnvironmentVariables { get; set; } =
        new Dictionary<string, string>();

    /// <summary>Belgenin yazilacagi mount ici dosya adidir.</summary>
    public string DocumentFileName { get; set; } = string.Empty;

    /// <summary>Runner'in uretecegi HAR artefakt dosya adidir.</summary>
    public string HarFileName { get; set; } = string.Empty;

    /// <summary>Runner'in uretecegi JSON ozet dosya adidir.</summary>
    public string JsonFileName { get; set; } = string.Empty;

    /// <summary>Runner kendi butcesini asarsa surecin sert oldurulecegi milisaniyedir.</summary>
    public int HardKillMs { get; set; }

    /// <summary>Bu plani ureten runner surumunun kararli referansidir.</summary>
    public string RunnerRef { get; set; } = string.Empty;
}
