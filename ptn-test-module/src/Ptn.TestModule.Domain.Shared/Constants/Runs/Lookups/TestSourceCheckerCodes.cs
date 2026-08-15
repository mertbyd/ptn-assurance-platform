using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Runs.Lookups;

// islevi: Kalici terminal bulgularinin uc izinli kaynagini kapali kodlarla tanimlar.
// sistemdeki gorevi: Checker sozlugunu lookup tablosu acmadan Domain.Shared sahibinde tutar.
/// <summary>
/// Test sonucu bulgularini uretebilen checker ve runner kodlarini tasir.
/// </summary>
public static class TestSourceCheckerCodes
{
    /// <summary>API sozlesme checker'i kaynak kodudur.</summary>
    public const string ApiContract = nameof(ApiContract);

    /// <summary>Veritabani karsilastirma checker'i kaynak kodudur.</summary>
    public const string DatabaseComparison = nameof(DatabaseComparison);

    /// <summary>Dis Arazzo runner'i kaynak kodudur.</summary>
    public const string Runner = nameof(Runner);

    /// <summary>Kalici bulgularda kabul edilen tum kaynak kodlaridir.</summary>
    public static IReadOnlyCollection<string> All { get; } =
        [ApiContract, DatabaseComparison, Runner];
}
