namespace Ptn.ApiContractChecker.Constants.Differences;

// islevi: Spec farklarinin adres, deger ve kimlik metinlerinde kullandigi kararli parcalari tanimlar.
// sistemdeki gorevi: Comparison manager ile rapor modelinin anlamli string sozlesmelerini Domain.Shared'da tek yerde tutar.
public static class SpecComparisonTextConstants
{
    public const string OperationIdKeyPrefix = "operation-id";
    public const string MethodPathKeyPrefix = "method-path";
    public const string Optional = "optional";
    public const string Required = "required";
    public const string Nullable = "nullable";
    public const string NonNullable = "non-nullable";
    public const string SuccessStatusWildcard = "2XX";
}
