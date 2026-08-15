using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Vocabulary;

// islevi: FK komsuluk baginin gelen ve giden yonlerini kapali Bridge sozlugunde tanimlar.
// sistemdeki gorevi: DB checker yon kodlarini agent yuzeyine paket sabiti sizdirmadan tasir.
public static class PtnForeignKeyDirectionCodes
{
    public const string Outgoing = nameof(Outgoing);
    public const string Incoming = nameof(Incoming);

    public static IReadOnlyCollection<string> All { get; } = [Outgoing, Incoming];
}
