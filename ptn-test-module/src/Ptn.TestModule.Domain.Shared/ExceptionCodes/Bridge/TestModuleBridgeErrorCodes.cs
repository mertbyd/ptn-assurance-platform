namespace Ptn.TestModule.ExceptionCodes.Bridge;

// islevi: Kopru profil, kanit butcesi ve checker cagrilarinin kararli hata kodlarini tanimlar.
// sistemdeki gorevi: Beklenen hatalari mesaj metninden bagimsiz ABP sozlesmesine baglar.
public static class TestModuleBridgeErrorCodes
{
    public const string ProfilePackNotFound = "TestModule.Bridge:ProfilePackNotFound";
    public const string ProfilePackInvalid = "TestModule.Bridge:ProfilePackInvalid";
    public const string ProfileFingerprintMismatch = "TestModule.Bridge:ProfileFingerprintMismatch";
    public const string ConceptNotBound = "TestModule.Bridge:ConceptNotBound";
    public const string EvidencePathNotFound = "TestModule.Bridge:EvidencePathNotFound";
    public const string HopBudgetExceeded = "TestModule.Bridge:HopBudgetExceeded";
    public const string EvidenceUnavailable = "TestModule.Bridge:EvidenceUnavailable";
    public const string CheckerCallFailed = "TestModule.Bridge:CheckerCallFailed";
}
