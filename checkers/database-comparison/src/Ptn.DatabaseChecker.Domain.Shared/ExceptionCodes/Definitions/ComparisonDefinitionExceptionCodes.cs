namespace Ptn.DatabaseChecker.ExceptionCodes;

// islevi: ComparisonDefinition modulunun hata ve validasyon kodlarini tanimlar.
// sistemdeki gorevi: Manager ve validator string literal yerine bu sabitleri kullanir (golden rule 3: hard-coded string yok).
public static class ComparisonDefinitionExceptionCodes
{
    private const string Prefix = "DatabaseChecker.ComparisonDefinition";

    // Ayni kiraci icinde ayni ada sahip ikinci tarif eklenmeye calisildiginda firlatilir.
    public const string NameAlreadyExists = $"{Prefix}:00001";

    // Pasif bir tarif yeni run baslatmak icin kullanildiginda firlatilir.
    public const string Inactive = $"{Prefix}:00002";

    // Girdi-format validasyon kodlari; FluentValidation mesajlari bu sabitleri kullanir.
    public static class Validation
    {
        public const string NameRequired = $"{Prefix}:Validation:NameRequired";
        public const string NameMaxLength = $"{Prefix}:Validation:NameMaxLength";
        public const string SourceConnectionIdRequired = $"{Prefix}:Validation:SourceConnectionIdRequired";
        public const string TargetConnectionIdRequired = $"{Prefix}:Validation:TargetConnectionIdRequired";
        public const string ComparisonTypeIdRequired = $"{Prefix}:Validation:ComparisonTypeIdRequired";
        public const string SourceRoleCodeRequired = $"{Prefix}:Validation:SourceRoleCodeRequired";
        public const string SourceRoleCodeInvalid = $"{Prefix}:Validation:SourceRoleCodeInvalid";

        // Gecici karsilastirma kapsam kurallari icin girdi-format kodlari; ScopeKindCode kararli koddur (ScopeKindCodes.*), FK degil.
        public const string ScopeRuleKindRequired = $"{Prefix}:Validation:ScopeRuleKindRequired";
        public const string ScopeRuleKindInvalid = $"{Prefix}:Validation:ScopeRuleKindInvalid";
        public const string ScopeRuleSchemaRequired = $"{Prefix}:Validation:ScopeRuleSchemaRequired";
        public const string ScopeRuleSchemaMaxLength = $"{Prefix}:Validation:ScopeRuleSchemaMaxLength";
        public const string ScopeRuleObjectRequired = $"{Prefix}:Validation:ScopeRuleObjectRequired";
        public const string ScopeRuleObjectMaxLength = $"{Prefix}:Validation:ScopeRuleObjectMaxLength";
        public const string DescriptionMaxLength = $"{Prefix}:Validation:DescriptionMaxLength";
    }
}
