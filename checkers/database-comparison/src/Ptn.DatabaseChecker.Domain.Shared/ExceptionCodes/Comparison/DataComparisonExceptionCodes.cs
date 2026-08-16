namespace Ptn.DatabaseChecker.ExceptionCodes;

// islevi: Data comparison motorunun runtime guvenlik ve veri-butunlugu hata kodlarini tanimlar.
// sistemdeki gorevi: Manager ve background run tarihcesi ham exception metni yerine kararli house kodlari kullanir.
public static class DataComparisonExceptionCodes
{
    private const string Prefix = "DatabaseChecker.DataComparison";

    // Secili bir tablonun exact row/cell karsilastirmasi tenant row limitini astiginda firlatilir.
    public const string RowLimitExceeded = $"{Prefix}:RowLimitExceeded";

    // Hashed saklama politikasi secildigi halde HMAC anahtari olacak salt bos birakildiginda firlatilir.
    public const string RedactionSaltMissing = $"{Prefix}:RedactionSaltMissing";

    // islevi: Yazma kumesi public girdi sinirlarinin kararli validation kodlarini gruplar.
    public static class WriteSetValidation
    {
        public const string ConnectionIdRequired = $"{Prefix}:WriteSet:Validation:ConnectionIdRequired";
        public const string CaptureRefRequired = $"{Prefix}:WriteSet:Validation:CaptureRefRequired";
        public const string CandidateTablesRequired = $"{Prefix}:WriteSet:Validation:CandidateTablesRequired";
        public const string CandidateTablesTooMany = $"{Prefix}:WriteSet:Validation:CandidateTablesTooMany";
        public const string CandidateTableInvalid = $"{Prefix}:WriteSet:Validation:CandidateTableInvalid";
    }
}
