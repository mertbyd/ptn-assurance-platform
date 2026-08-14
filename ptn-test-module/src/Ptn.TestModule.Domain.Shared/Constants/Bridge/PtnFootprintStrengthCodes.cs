using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge;

// islevi: Yazma kumesi gozleminin dort yetenek seviyesini tanimlar.
// sistemdeki gorevi: Ortam yetenegi yokken kesinlik uydurulmasini kapali seviye sozlesmesiyle engeller.
public static class PtnFootprintStrengthCodes
{
    public const string Exact = nameof(Exact);
    public const string RowAddressed = nameof(RowAddressed);
    public const string Inferred = nameof(Inferred);
    public const string Unavailable = nameof(Unavailable);

    public static IReadOnlyCollection<string> All { get; } = [Exact, RowAddressed, Inferred, Unavailable];
}
