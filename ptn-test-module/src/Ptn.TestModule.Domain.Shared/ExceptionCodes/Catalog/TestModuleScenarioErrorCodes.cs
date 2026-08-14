namespace Ptn.TestModule.ExceptionCodes.Catalog;

// islevi: Senaryo katalogu invariant ve validation hatalarinin kararli kodlarini tanimlar.
// sistemdeki gorevi: Benzersizlik, gecis, onay, silme ve public girdi hatalarini mesaj metninden ayirir.
public static class TestModuleScenarioErrorCodes
{
    private const string Prefix = "TestModule.Scenario";

    public const string VersionAlreadyExists = $"{Prefix}:VersionAlreadyExists";
    public const string ContentAlreadyExists = $"{Prefix}:ContentAlreadyExists";
    public const string InvalidStateTransition = $"{Prefix}:InvalidStateTransition";
    public const string ApprovalRequired = $"{Prefix}:ApprovalRequired";
    public const string ApprovalHashMismatch = $"{Prefix}:ApprovalHashMismatch";
    public const string PublicationGateFailed = $"{Prefix}:PublicationGateFailed";
    public const string DeletionNotAllowed = $"{Prefix}:DeletionNotAllowed";
    public const string InvalidScenarioKey = $"{Prefix}:InvalidScenarioKey";
    public const string InvalidHash = $"{Prefix}:InvalidHash";

    public static class Validation
    {
        public const string ScenarioKeyRequired = $"{Prefix}:Validation:ScenarioKeyRequired";
        public const string ScenarioKeyInvalid = $"{Prefix}:Validation:ScenarioKeyInvalid";
        public const string TitleRequired = $"{Prefix}:Validation:TitleRequired";
        public const string TitleTooLong = $"{Prefix}:Validation:TitleTooLong";
        public const string DescriptionTooLong = $"{Prefix}:Validation:DescriptionTooLong";
        public const string SourceDocumentRequired = $"{Prefix}:Validation:SourceDocumentRequired";
        public const string CompiledDocumentRequired = $"{Prefix}:Validation:CompiledDocumentRequired";
        public const string HashRequired = $"{Prefix}:Validation:HashRequired";
        public const string HashInvalid = $"{Prefix}:Validation:HashInvalid";
        public const string AssertionCountInvalid = $"{Prefix}:Validation:AssertionCountInvalid";
        public const string DerivabilityCodeTooLong = $"{Prefix}:Validation:DerivabilityCodeTooLong";
        public const string AgentModelRefTooLong = $"{Prefix}:Validation:AgentModelRefTooLong";
        public const string NotesTooLong = $"{Prefix}:Validation:NotesTooLong";
        public const string MaterialSealRequired = $"{Prefix}:Validation:MaterialSealRequired";
        public const string MaterialIdentityInvalid = $"{Prefix}:Validation:MaterialIdentityInvalid";
        public const string SourceDescriptionsRequired = $"{Prefix}:Validation:SourceDescriptionsRequired";
        public const string SourceDescriptionInvalid = $"{Prefix}:Validation:SourceDescriptionInvalid";
    }
}
