using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Vocabulary;

// islevi: Tamamlanan kanit zincirinin kapali hukum kodlarini tanimlar.
// sistemdeki gorevi: Sonucu metinden ve checker'a ozgu guven gramerinden bagimsiz tasir.
public static class PtnVerdictCodes
{
    public const string Confirmed = nameof(Confirmed);
    public const string Likely = nameof(Likely);
    public const string Possible = nameof(Possible);
    public const string RuledOut = nameof(RuledOut);
    public const string Inconclusive = nameof(Inconclusive);

    public static IReadOnlyCollection<string> All { get; } =
        [Confirmed, Likely, Possible, RuledOut, Inconclusive];
}
