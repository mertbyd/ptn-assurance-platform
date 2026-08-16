namespace Ptn.DatabaseChecker.ExceptionCodes;

// islevi: Teshis girisi, engine eslesmesi, setting ve probe cozumleme ihlallerinin kararli hata kodlarini tanimlar.
// sistemdeki gorevi: Beklenen teshis hatalarini provider exception metni veya controller ayrintisina baglanmadan API sinirina tasir.
public static class DiagnosisExceptionCodes
{
    private const string Prefix = "DatabaseChecker.Diagnosis";

    public const string EngineMismatch = $"{Prefix}:EngineMismatch";
    public const string InvalidSetting = $"{Prefix}:InvalidSetting";
    public const string ProbeNotFound = $"{Prefix}:ProbeNotFound";

    // islevi: Diagnose public input alanlarinin shape ihlali hata kodlarini gruplar.
    // sistemdeki gorevi: FluentValidation sonucunu alan metnine veya dile baglanmadan istemciye tasir.
    public static class Validation
    {
        public const string ConnectionRequired = $"{Prefix}:Validation:ConnectionRequired";
        public const string ExactlyOneSignalRequired = $"{Prefix}:Validation:ExactlyOneSignalRequired";
        public const string SchemaRequired = $"{Prefix}:Validation:SchemaRequired";
        public const string SchemaTooLong = $"{Prefix}:Validation:SchemaTooLong";
        public const string TableRequired = $"{Prefix}:Validation:TableRequired";
        public const string TableTooLong = $"{Prefix}:Validation:TableTooLong";
        public const string KeyRequired = $"{Prefix}:Validation:KeyRequired";
        public const string OutcomeRequired = $"{Prefix}:Validation:OutcomeRequired";
        public const string OutcomeInvalid = $"{Prefix}:Validation:OutcomeInvalid";
        public const string EngineRequired = $"{Prefix}:Validation:EngineRequired";
        public const string EngineInvalid = $"{Prefix}:Validation:EngineInvalid";
        public const string ErrorCodeRequired = $"{Prefix}:Validation:ErrorCodeRequired";
        public const string ErrorCodeInvalid = $"{Prefix}:Validation:ErrorCodeInvalid";
        public const string ProviderFieldNameRequired = $"{Prefix}:Validation:ProviderFieldNameRequired";
        public const string ProviderFieldsInvalid = $"{Prefix}:Validation:ProviderFieldsInvalid";
    }
}
