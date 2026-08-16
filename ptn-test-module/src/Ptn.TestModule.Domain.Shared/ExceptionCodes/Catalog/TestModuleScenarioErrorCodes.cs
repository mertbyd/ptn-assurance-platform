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
    public const string AuthoringSessionNotFound = $"{Prefix}:AuthoringSessionNotFound";
    public const string AuthoringSessionTenantMismatch = $"{Prefix}:AuthoringSessionTenantMismatch";
    public const string AuthoringAnswerInvalid = $"{Prefix}:AuthoringAnswerInvalid";
    public const string AuthoringQuestionsUnanswered = $"{Prefix}:AuthoringQuestionsUnanswered";
    public const string AuthoringOperationNotGrounded = $"{Prefix}:AuthoringOperationNotGrounded";
    public const string AuthoringStepAlreadyExists = $"{Prefix}:AuthoringStepAlreadyExists";

    /// <summary>Zamanlamanin yayinlanmamis bir surume yazilmaya calisildigini bildirir.</summary>
    public const string ScheduleRequiresPublishedVersion = $"{Prefix}:ScheduleRequiresPublishedVersion";

    /// <summary>Cron ifadesinin ayristirilamadigini bildirir.</summary>
    public const string ScheduleCronInvalid = $"{Prefix}:ScheduleCronInvalid";

    /// <summary>Zamanlamanin cron ifadesi olmadan acilmaya calisildigini bildirir.</summary>
    public const string ScheduleCronRequired = $"{Prefix}:ScheduleCronRequired";

    /// <summary>Cron ifadesinin gelecege donuk hicbir vade uretmedigini bildirir.</summary>
    public const string ScheduleHasNoFutureOccurrence = $"{Prefix}:ScheduleHasNoFutureOccurrence";

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
        public const string ScheduleCronRequired = $"{Prefix}:Validation:ScheduleCronRequired";
        public const string ScheduleCronTooLong = $"{Prefix}:Validation:ScheduleCronTooLong";
        public const string AuthoringGroundingRequired = $"{Prefix}:Validation:AuthoringGroundingRequired";
        public const string AuthoringWorkflowIdInvalid = $"{Prefix}:Validation:AuthoringWorkflowIdInvalid";
        public const string AuthoringWorkflowSummaryRequired = $"{Prefix}:Validation:AuthoringWorkflowSummaryRequired";
        public const string AuthoringSourceUrlInvalid = $"{Prefix}:Validation:AuthoringSourceUrlInvalid";
        public const string AuthoringQuestionCodeRequired = $"{Prefix}:Validation:AuthoringQuestionCodeRequired";
        public const string AuthoringSelectedOptionRequired = $"{Prefix}:Validation:AuthoringSelectedOptionRequired";
        public const string AuthoringStepIdInvalid = $"{Prefix}:Validation:AuthoringStepIdInvalid";
        public const string AuthoringOperationReferenceRequired = $"{Prefix}:Validation:AuthoringOperationReferenceRequired";
        public const string AuthoringAssertionPathInvalid = $"{Prefix}:Validation:AuthoringAssertionPathInvalid";
        public const string AuthoringRequestBodyInvalid = $"{Prefix}:Validation:AuthoringRequestBodyInvalid";
    }
}
