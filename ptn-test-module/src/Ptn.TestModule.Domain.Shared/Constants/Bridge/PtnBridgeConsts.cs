namespace Ptn.TestModule.Constants.Bridge;

// islevi: Kopru motorunun kanit, profil ve rapor butcelerini tek yerde tanimlar.
// sistemdeki gorevi: Sinirsiz zincir, projeksiyon ve dosya okumalarini deterministik olarak engeller.
public static class PtnBridgeConsts
{
    public const int MaxHopCount = 6;
    public const int MaxEvidencePerNode = 3;
    public const int MaxNodeCount = 32;
    public const int MaxProjectionRows = 25;
    public const int MaxProfilePackBytes = 262144;
    public const int MaxBusinessRulesBytes = 262144;
    public const int MaxReportBytes = 4096;
    public const int MinimumOperationScore = 70;
    public const int DefaultOperationLinkCandidates = 5;
    public const int MaxOperationLinkCandidates = 5;
    public const string ReferenceIdFormat = "D";
    public const string EvidenceReferenceSeparator = ":";
}
