namespace Ptn.DatabaseChecker.Constants.Comparison.Projections;

// islevi: Salt-okunur projection sonucunun kapali outcome kod kumesini tanimlar.
// sistemdeki gorevi: Domain karari ile public API cevabinin ayni kararli kodlari tasimasini saglar.
public static class ProjectionOutcomeCodes
{
    public const string Projected = "Projected";
    public const string TableNotFound = "TableNotFound";
    public const string ColumnNotFound = "ColumnNotFound";
    public const string KeyNotUnique = "KeyNotUnique";
    public const string NotAuthorized = "NotAuthorized";
    public const string Truncated = "Truncated";

    public static IReadOnlyCollection<string> All { get; } =
        [Projected, TableNotFound, ColumnNotFound, KeyNotUnique, NotAuthorized, Truncated];
}
