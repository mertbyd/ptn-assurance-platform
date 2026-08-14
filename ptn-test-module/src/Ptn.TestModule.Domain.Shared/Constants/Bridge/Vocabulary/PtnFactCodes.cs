using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Vocabulary;

// islevi: Iki checker'in probe olgularini tek kapali kanit sozlugunde tanimlar.
// sistemdeki gorevi: Match/Matches gibi gramer farklarinin kanit agacina sizmasini engeller.
public static class PtnFactCodes
{
    public const string Present = nameof(Present);
    public const string Absent = nameof(Absent);
    public const string Match = nameof(Match);
    public const string Mismatch = nameof(Mismatch);
    public const string Reachable = nameof(Reachable);
    public const string Unreachable = nameof(Unreachable);
    public const string TimedOut = nameof(TimedOut);
    public const string Found = nameof(Found);
    public const string Missing = nameof(Missing);
    public const string Catalog = nameof(Catalog);
    public const string Unavailable = nameof(Unavailable);

    public static IReadOnlyCollection<string> All { get; } =
        [Present, Absent, Match, Mismatch, Reachable, Unreachable, TimedOut, Found, Missing, Catalog, Unavailable];
}
