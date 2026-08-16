namespace Ptn.ApiContractChecker.Constants.Conformance.Lookups;

// islevi: Bulgu degerlerinin nasil saklanacagini belirleyen kapali retention kodlarini tanimlar.
// sistemdeki gorevi: Ayar, resolver ve redactor'u varsayilan olarak deger tasimayan tek politika sozlesmesine baglar.
public static class ValueRetentionModeCodes
{
    public const string None = "None";
    public const string Hashed = "Hashed";
    public const string Masked = "Masked";
    public const string Full = "Full";

    public static IReadOnlyCollection<string> All { get; } = [None, Hashed, Masked, Full];

    // islevi: Verilen kodun desteklenen retention katalogunda olup olmadigini bildirir.
    public static bool IsDefined(string? code)
    {
        return code is not null && All.Contains(code, StringComparer.Ordinal);
    }
}
