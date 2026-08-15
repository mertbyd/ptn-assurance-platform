using System;

namespace Ptn.TestModule.Models.Runs;

// islevi: Yeni Pending kosum kaydinin kullanici tarafindan secilen girdilerini tasir.
// sistemdeki gorevi: Transporttan bagimsiz create verisini TestRunManager'a aktarir.
/// <summary>
/// Yeni bir test kosumu olusturmak icin gereken domain girdilerini tasir.
/// </summary>
public class TestRunCreateModel
{
    /// <summary>Bagli senaryo surumunun kimligidir; ad-hoc veya dry-run icin null olabilir.</summary>
    public Guid? ScenarioId { get; set; }

    /// <summary>Trendlerde kullanilan kararli test anahtaridir.</summary>
    public string TestKey { get; set; } = string.Empty;

    /// <summary>Kosumun nasil baslatildigini belirleyen lookup kodudur.</summary>
    public string TriggerKindCode { get; set; } = string.Empty;

    /// <summary>Cron, webhook veya bulgu gibi tetikleyici referansidir.</summary>
    public string? TriggerRef { get; set; }

    /// <summary>Kosumun saglik hesaplarina girmeyen kuru kosum olup olmadigidir.</summary>
    public bool IsDryRun { get; set; }
}
