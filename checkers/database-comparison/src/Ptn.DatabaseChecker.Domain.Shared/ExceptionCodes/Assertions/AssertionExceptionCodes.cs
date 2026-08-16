namespace Ptn.DatabaseChecker.ExceptionCodes;

// islevi: Assertion sozlesmesinin format ve is-kurali ihlalleri icin kararli hata kodlarini tanimlar.
// sistemdeki gorevi: FluentValidation ve manager beklenen hatalari ham mesaj veya provider exception'i olmadan disariya tasir.
public static class AssertionExceptionCodes
{
    private const string Prefix = "DatabaseChecker.Assertion";

    public const string InvalidMatcherKind = $"{Prefix}:InvalidMatcherKind";
    public const string InvalidCardinalityKind = $"{Prefix}:InvalidCardinalityKind";
    public const string InvalidExpectedValue = $"{Prefix}:InvalidExpectedValue";
    public const string InvalidSetting = $"{Prefix}:InvalidSetting";
    public const string TableNotFound = $"{Prefix}:TableNotFound";
    public const string ProjectionNotAuthorized = $"{Prefix}:ProjectionNotAuthorized";

    // islevi: Assertion DTO'larinin sinir dogrulama hata kodlarini gruplar.
    // sistemdeki gorevi: Validator mesajlari istemci tarafinda alan-bazli ve dil-bagimsiz yorumlanabilir.
    public static class Validation
    {
        public const string ConnectionIdRequired = $"{Prefix}:Validation:ConnectionIdRequired";
        public const string SchemaRequired = $"{Prefix}:Validation:SchemaRequired";
        public const string SchemaMaxLength = $"{Prefix}:Validation:SchemaMaxLength";
        public const string TableRequired = $"{Prefix}:Validation:TableRequired";
        public const string TableMaxLength = $"{Prefix}:Validation:TableMaxLength";
        public const string KeyRequired = $"{Prefix}:Validation:KeyRequired";
        public const string KeyColumnRequired = $"{Prefix}:Validation:KeyColumnRequired";
        public const string ExpectationColumnRequired = $"{Prefix}:Validation:ExpectationColumnRequired";
        public const string MatcherRequired = $"{Prefix}:Validation:MatcherRequired";
        public const string MatcherInvalid = $"{Prefix}:Validation:MatcherInvalid";
        public const string CardinalityRequired = $"{Prefix}:Validation:CardinalityRequired";
        public const string CardinalityInvalid = $"{Prefix}:Validation:CardinalityInvalid";
        public const string ExpectedCountInvalid = $"{Prefix}:Validation:ExpectedCountInvalid";
        public const string TimeoutInvalid = $"{Prefix}:Validation:TimeoutInvalid";
        public const string PollIntervalInvalid = $"{Prefix}:Validation:PollIntervalInvalid";
        public const string ToleranceInvalid = $"{Prefix}:Validation:ToleranceInvalid";
        public const string BatchRequired = $"{Prefix}:Validation:BatchRequired";
        public const string BatchTooLarge = $"{Prefix}:Validation:BatchTooLarge";
        public const string CorrelationTraceIdInvalid = $"{Prefix}:Validation:CorrelationTraceIdInvalid";
        public const string CorrelationStepKeyInvalid = $"{Prefix}:Validation:CorrelationStepKeyInvalid";
        public const string BatchResultCountMismatch = $"{Prefix}:Validation:BatchResultCountMismatch";
        public const string ProjectionColumnsRequired = $"{Prefix}:Validation:ProjectionColumnsRequired";
        public const string ProjectionColumnsTooMany = $"{Prefix}:Validation:ProjectionColumnsTooMany";
        public const string ProjectionColumnRequired = $"{Prefix}:Validation:ProjectionColumnRequired";
        public const string ProjectionMaxRowsInvalid = $"{Prefix}:Validation:ProjectionMaxRowsInvalid";
        public const string DerivabilityAssertionsRequired = $"{Prefix}:Validation:DerivabilityAssertionsRequired";
        public const string DerivabilityKeyColumnsRequired = $"{Prefix}:Validation:DerivabilityKeyColumnsRequired";
        public const string DerivabilityExpectedColumnsRequired = $"{Prefix}:Validation:DerivabilityExpectedColumnsRequired";
    }
}
