using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Runs;

// islevi: Kosum ihracatinin uc kapali format kodunu tanimlar.
// sistemdeki gorevi: Format sozlugunu lookup tablosu acmadan Domain.Shared sahibinde tutar (PLAN-0003 TM-13/TM-14/TM-30).
/// <summary>
/// Kosum ihracatinda uretilebilen artefakt format kodlarini tasir.
/// </summary>
public static class RunArtifactFormatCodes
{
    /// <summary>Common Test Report Format ciktisinin kodudur.</summary>
    public const string Ctrf = nameof(Ctrf);

    /// <summary>JUnit XML ciktisinin kodudur.</summary>
    public const string JUnit = nameof(JUnit);

    /// <summary>SARIF 2.1.0 ciktisinin kodudur.</summary>
    public const string Sarif = nameof(Sarif);

    /// <summary>Ihracatta kabul edilen tum format kodlaridir.</summary>
    public static IReadOnlyCollection<string> All { get; } = [Ctrf, JUnit, Sarif];
}
