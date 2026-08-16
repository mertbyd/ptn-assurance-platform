namespace Ptn.DatabaseChecker.Constants.Diagnosis;

// islevi: Dinamik teshis motorunun destekledigi on hipotezin kararli kimliklerini tanimlar.
// sistemdeki gorevi: Kural, lokalizasyon ve API raporunu sinif adina veya serbest metne baglamadan eslestirir.
public static class HypothesisKindCodes
{
    public const string RowNeverCreated = "RowNeverCreated";
    public const string RowCreatedLate = "RowCreatedLate";
    public const string RowValueDiffers = "RowValueDiffers";
    public const string RowInAnotherScope = "RowInAnotherScope";
    public const string ExpectedColumnMissing = "ExpectedColumnMissing";
    public const string ForeignKeyParentMissing = "ForeignKeyParentMissing";
    public const string ConstraintNotValidated = "ConstraintNotValidated";
    public const string UniqueDuplicateExists = "UniqueDuplicateExists";
    public const string GeneratedColumnWrite = "GeneratedColumnWrite";
    public const string ServerSettingMismatch = "ServerSettingMismatch";
}
