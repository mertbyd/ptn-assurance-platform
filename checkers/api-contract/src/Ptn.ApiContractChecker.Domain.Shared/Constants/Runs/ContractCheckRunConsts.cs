namespace Ptn.ApiContractChecker.Constants.Runs;

// islevi: ContractCheckRun hata, gecici scope payload'i ve stale recovery sinirlarini tanimlar.
// sistemdeki gorevi: Domain gecisleri, validator, EF kolon eslemesi ve toparlama akisinin ayni merkezi esikleri kullanmasini saglar.
public static class ContractCheckRunConsts
{
    public const int MaxErrorMessageLength = 512;
    public const int MaxScopeRuleCount = 100;
    public const int MaxScopePatternLength = 256;
    public const int StaleRunningThresholdMinutes = 30;
    public const string FindingsJsonColumnName = "findings";
    public const int DefaultFindingPageSize = 20;
    public const int DefaultMaxFindingPageSize = 100;
    public const int DefaultFindingPageMaxBytes = 32 * 1024;
    public const int MaxFindingFilterLength = 512;
    public const int MaxFindingFingerprintFilterCount = 100;
    public const int FindingFingerprintHexLength = 64;
    public const string FindingFingerprintPattern = "^[0-9A-Fa-f]{64}$";
    public const string FingerprintSeparator = "|";
    public const string FingerprintEmptyComponent = "<empty>";
    public const string FingerprintMissingValue = "missing";
    public const string FingerprintValuePrefix = "value:";
}
