namespace Ptn.ApiContractChecker.Constants.Sources;

// islevi: Zamanlanmis dokuman cekiminin kararli sonuc kodlarini tanimlar.
// sistemdeki gorevi: SpecDocument'in son cekim durumunu enum veya lookup kurmadan kalici ve karsilastirilabilir tutar.
public static class SpecDocumentFetchOutcomeCodes
{
    public const string Changed = "changed";
    public const string Unchanged = "unchanged";
    public const string Unreachable = "unreachable";

    public static IReadOnlyCollection<string> All { get; } = [Changed, Unchanged, Unreachable];
}
