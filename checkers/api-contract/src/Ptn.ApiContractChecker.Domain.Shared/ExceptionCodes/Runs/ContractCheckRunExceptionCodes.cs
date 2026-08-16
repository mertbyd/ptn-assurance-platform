namespace Ptn.ApiContractChecker.ExceptionCodes.Runs;

// islevi: ContractCheckRun ve owned bulgu invariant ihlallerinin kararli hata kodlarini tanimlar.
// sistemdeki gorevi: Domain gecis hatalarini HTTP veya job katmanindan bagimsiz, izlenebilir bir sozlesmeye baglar.
public static class ContractCheckRunExceptionCodes
{
    public const string ExecutionFailed = "ApiContractChecker:ContractCheckRun:ExecutionFailed";
    public const string StaleRunningRecovered = "ApiContractChecker:ContractCheckRun:StaleRunningRecovered";
    public const string InvalidScopeRule = "ApiContractChecker:ContractCheckRun:InvalidScopeRule";
    public const string SnapshotNotFound = "ApiContractChecker:ContractCheckRun:SnapshotNotFound";
    public const string InvalidStatusTransition = "ApiContractChecker:ContractCheckRun:InvalidStatusTransition";
    public const string InvalidCompletionTime = "ApiContractChecker:ContractCheckRun:InvalidCompletionTime";
    public const string FailureReasonRequired = "ApiContractChecker:ContractCheckRun:FailureReasonRequired";
    public const string FindingsRequired = "ApiContractChecker:ContractCheckRun:FindingsRequired";
    public const string FindingAddressRequired = "ApiContractChecker:ContractCheckRun:FindingAddressRequired";
    public const string InvalidFindingReferenceRun = "ApiContractChecker:ContractCheckRun:InvalidFindingReferenceRun";

    public static class Validation
    {
        public const string BaseSnapshotRequired = "ApiContractChecker.ContractCheckRun:Validation:BaseSnapshotRequired";
        public const string TargetSnapshotRequired = "ApiContractChecker.ContractCheckRun:Validation:TargetSnapshotRequired";
        public const string ScopeRuleLimitExceeded = "ApiContractChecker.ContractCheckRun:Validation:ScopeRuleLimitExceeded";
        public const string ScopeKindRequired = "ApiContractChecker.ContractCheckRun:Validation:ScopeKindRequired";
        public const string ScopeKindInvalid = "ApiContractChecker.ContractCheckRun:Validation:ScopeKindInvalid";
        public const string ScopeTargetRequired = "ApiContractChecker.ContractCheckRun:Validation:ScopeTargetRequired";
        public const string ScopeTargetInvalid = "ApiContractChecker.ContractCheckRun:Validation:ScopeTargetInvalid";
        public const string ScopePatternRequired = "ApiContractChecker.ContractCheckRun:Validation:ScopePatternRequired";
        public const string ScopePatternMaxLength = "ApiContractChecker.ContractCheckRun:Validation:ScopePatternMaxLength";
        public const string FindingSinceRunIdInvalid = "ApiContractChecker.ContractCheckRun:Validation:FindingSinceRunIdInvalid";
        public const string FindingFingerprintInvalid = "ApiContractChecker.ContractCheckRun:Validation:FindingFingerprintInvalid";
        public const string FindingFingerprintDuplicate = "ApiContractChecker.ContractCheckRun:Validation:FindingFingerprintDuplicate";
        public const string FindingFingerprintLimitExceeded = "ApiContractChecker.ContractCheckRun:Validation:FindingFingerprintLimitExceeded";
    }
}
