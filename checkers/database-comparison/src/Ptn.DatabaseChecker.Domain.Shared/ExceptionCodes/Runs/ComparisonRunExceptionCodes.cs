namespace Ptn.DatabaseChecker.ExceptionCodes;

// islevi: ComparisonRun modulunun hata ve validasyon kodlarini tanimlar.
// sistemdeki gorevi: Manager ve validator string literal yerine bu sabitleri kullanir (golden rule 3: hard-coded string yok).
public static class ComparisonRunExceptionCodes
{
    private const string Prefix = "DatabaseChecker.ComparisonRun";

    // Istenen rapor formati generator tarafindan desteklenmediginde firlatilir.
    public const string InvalidReportFormat = $"{Prefix}:InvalidReportFormat";

    // Background execution beklenmeyen bir altyapi/motor hatasiyla terminal Failed oldugunda saklanir.
    public const string ExecutionFailed = $"{Prefix}:ExecutionFailed";

    // Pending/Running disindaki bir run yeniden baslatilmak veya tamamlanmak istendiginde firlatilir.
    public const string InvalidStatusTransition = $"{Prefix}:InvalidStatusTransition";

    // Bulgu sayfalama veya cevap butcesi setting'i pozitif olmadiginda firlatilir.
    public const string InvalidFindingSetting = $"{Prefix}:InvalidFindingSetting";

    // SinceRunId ayni tenant ve definition icindeki daha eski Completed run'i gostermediginde firlatilir.
    public const string InvalidFindingReferenceRun = $"{Prefix}:InvalidFindingReferenceRun";

    // Girdi-format validasyon kodlari; FluentValidation mesajlari bu sabitleri kullanir.
    public static class Validation
    {
        public const string ComparisonDefinitionIdInvalid = $"{Prefix}:Validation:ComparisonDefinitionIdInvalid";
        public const string SourceConnectionIdRequired = $"{Prefix}:Validation:SourceConnectionIdRequired";
        public const string TargetConnectionIdRequired = $"{Prefix}:Validation:TargetConnectionIdRequired";
        public const string ComparisonTypeIdRequired = $"{Prefix}:Validation:ComparisonTypeIdRequired";

        // Anlik karsilastirma modu kodu bos olamaz ve yalnizca izinli ComparisonTypeCodes degerlerinden biri olabilir.
        public const string ComparisonTypeCodeRequired = $"{Prefix}:Validation:ComparisonTypeCodeRequired";
        public const string ComparisonTypeCodeInvalid = $"{Prefix}:Validation:ComparisonTypeCodeInvalid";
        public const string StatusIdRequired = $"{Prefix}:Validation:StatusIdRequired";
        public const string ErrorMessageMaxLength = $"{Prefix}:Validation:ErrorMessageMaxLength";
        public const string CountCannotBeNegative = $"{Prefix}:Validation:CountCannotBeNegative";

        // Calistir-ve-sakla akisi bir tariften dogar; tarif kimligi bos Guid olamaz.
        public const string ExecuteComparisonDefinitionIdRequired = $"{Prefix}:Validation:ExecuteComparisonDefinitionIdRequired";

        public const string FindingSeverityInvalid = $"{Prefix}:Validation:FindingSeverityInvalid";
        public const string FindingKindInvalid = $"{Prefix}:Validation:FindingKindInvalid";
        public const string FindingObjectTypeInvalid = $"{Prefix}:Validation:FindingObjectTypeInvalid";
        public const string FindingSchemaNameTooLong = $"{Prefix}:Validation:FindingSchemaNameTooLong";
        public const string FindingTableNameTooLong = $"{Prefix}:Validation:FindingTableNameTooLong";
        public const string FindingSinceRunIdInvalid = $"{Prefix}:Validation:FindingSinceRunIdInvalid";
        public const string FindingFingerprintInvalid = $"{Prefix}:Validation:FindingFingerprintInvalid";
        public const string FindingFingerprintDuplicate = $"{Prefix}:Validation:FindingFingerprintDuplicate";
        public const string FindingFingerprintLimitExceeded = $"{Prefix}:Validation:FindingFingerprintLimitExceeded";
    }
}
