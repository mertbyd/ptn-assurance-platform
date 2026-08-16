namespace Ptn.ApiContractChecker.Constants.Conformance.Lookups;

// islevi: Orneklerin kaynaklandigi JSON Schema kisitlarini kapali kod kumesinde tanimlar.
// sistemdeki gorevi: Her uretilen degeri aciklanabilir ve ajan tarafindan secilebilir bir gerekceye baglar.
public static class ConstraintCodes
{
    public const string MinLength = "MinLength";
    public const string MaxLength = "MaxLength";
    public const string Minimum = "Minimum";
    public const string Maximum = "Maximum";
    public const string Pattern = "Pattern";
    public const string Enum = "Enum";
    public const string Required = "Required";
    public const string Type = "Type";
    public const string Format = "Format";

    public static IReadOnlyCollection<string> All { get; } =
    [
        MinLength, MaxLength, Minimum, Maximum, Pattern, Enum, Required, Type, Format
    ];
}
