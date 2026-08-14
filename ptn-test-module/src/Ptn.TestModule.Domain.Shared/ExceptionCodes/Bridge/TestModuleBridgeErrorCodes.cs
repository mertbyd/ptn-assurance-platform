namespace Ptn.TestModule.ExceptionCodes.Bridge;

// islevi: Kopru profil, kanit butcesi ve checker cagrilarinin kararli hata kodlarini tanimlar.
// sistemdeki gorevi: Beklenen hatalari mesaj metninden bagimsiz ABP sozlesmesine baglar.
public static class TestModuleBridgeErrorCodes
{
    private const string Prefix = "TestModule.Bridge";

    public const string ProfilePackNotFound = $"{Prefix}:ProfilePackNotFound";
    public const string ProfilePackInvalid = $"{Prefix}:ProfilePackInvalid";
    public const string ProfileFingerprintMismatch = $"{Prefix}:ProfileFingerprintMismatch";
    public const string ConceptNotBound = $"{Prefix}:ConceptNotBound";
    public const string EvidencePathNotFound = $"{Prefix}:EvidencePathNotFound";
    public const string HopBudgetExceeded = $"{Prefix}:HopBudgetExceeded";
    public const string EvidenceUnavailable = $"{Prefix}:EvidenceUnavailable";
    public const string CheckerCallFailed = $"{Prefix}:CheckerCallFailed";
    public const string ToolBudgetExceeded = $"{Prefix}:ToolBudgetExceeded";

    // Bridge DTO girdi-format dogrulama kodlarini sabitler.
    public static class Validation
    {
        public const string ConnectionIdRequired = $"{Prefix}:Validation:ConnectionIdRequired";
        public const string SnapshotIdRequired = $"{Prefix}:Validation:SnapshotIdRequired";
        public const string SchemaNameRequired = $"{Prefix}:Validation:SchemaNameRequired";
        public const string TableNameRequired = $"{Prefix}:Validation:TableNameRequired";
        public const string ColumnNameRequired = $"{Prefix}:Validation:ColumnNameRequired";
        public const string MatcherKindRequired = $"{Prefix}:Validation:MatcherKindRequired";
        public const string CardinalityKindRequired = $"{Prefix}:Validation:CardinalityKindRequired";
        public const string LocationRequired = $"{Prefix}:Validation:LocationRequired";
        public const string MethodRequired = $"{Prefix}:Validation:MethodRequired";
        public const string PathRequired = $"{Prefix}:Validation:PathRequired";
        public const string AssertionPathRequired = $"{Prefix}:Validation:AssertionPathRequired";
        public const string StatusCodeInvalid = $"{Prefix}:Validation:StatusCodeInvalid";
        public const string TimeoutInvalid = $"{Prefix}:Validation:TimeoutInvalid";
        public const string PollIntervalInvalid = $"{Prefix}:Validation:PollIntervalInvalid";
        public const string ProjectionRowLimitInvalid = $"{Prefix}:Validation:ProjectionRowLimitInvalid";
        public const string RequestRequired = $"{Prefix}:Validation:RequestRequired";
        public const string BatchRequired = $"{Prefix}:Validation:BatchRequired";
        public const string ProfileKeyRequired = $"{Prefix}:Validation:ProfileKeyRequired";
        public const string OperationReferenceRequired = $"{Prefix}:Validation:OperationReferenceRequired";
        public const string AssertionReferenceRequired = $"{Prefix}:Validation:AssertionReferenceRequired";
        public const string ResponseFormatInvalid = $"{Prefix}:Validation:ResponseFormatInvalid";
        public const string ConceptCodeInvalid = $"{Prefix}:Validation:ConceptCodeInvalid";
        public const string CorrelationTraceIdInvalid = $"{Prefix}:Validation:CorrelationTraceIdInvalid";
        public const string CorrelationStepKeyInvalid = $"{Prefix}:Validation:CorrelationStepKeyInvalid";
        public const string DerivabilityAssertionsRequired = $"{Prefix}:Validation:DerivabilityAssertionsRequired";
        public const string DerivabilityKeyColumnsRequired = $"{Prefix}:Validation:DerivabilityKeyColumnsRequired";
        public const string DerivabilityExpectedColumnsRequired = $"{Prefix}:Validation:DerivabilityExpectedColumnsRequired";
    }
}
