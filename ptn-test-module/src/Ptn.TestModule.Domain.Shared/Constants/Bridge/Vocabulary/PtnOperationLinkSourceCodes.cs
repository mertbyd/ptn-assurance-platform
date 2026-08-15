using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Vocabulary;

// islevi: Operasyon zinciri adaylarinin mekanik kanit kaynaklarini tek Bridge sozlugunde tanimlar.
// sistemdeki gorevi: Checker kaynak kodlarini ajana ham paket sabitleri sizdirmadan sunar.
public static class PtnOperationLinkSourceCodes
{
    public const string DeclaredLink = nameof(DeclaredLink);
    public const string SchemaMatch = nameof(SchemaMatch);
    public const string LocationHeader = nameof(LocationHeader);

    public static IReadOnlyCollection<string> All { get; } =
        [DeclaredLink, SchemaMatch, LocationHeader];
}
