namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Tenant-aware ihlal sayisi ve response bayt tavanini birlikte tasir.
// sistemdeki gorevi: Setting okuma ve sayisal dogrulamayi sonuc kirpma adimindan ayirir.
public sealed class ConformanceLimits
{
    public int MaxViolations { get; }
    public int MaxResponseBytes { get; }

    public ConformanceLimits(int maxViolations, int maxResponseBytes)
    {
        MaxViolations = maxViolations;
        MaxResponseBytes = maxResponseBytes;
    }
}
