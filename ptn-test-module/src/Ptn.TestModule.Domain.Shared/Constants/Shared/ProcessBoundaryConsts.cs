namespace Ptn.TestModule.Constants.Shared;

// islevi: Surec sinirinin calisma klasoru koku ile argumanlardaki klasor yer tutucusunu tanimlar.
// sistemdeki gorevi: Manager'in plan kurarken ve sinirin plani kosarken ayni sozlugu kullanmasini saglar.
/// <summary>
/// Dis surec calisma klasorunun kararli sabitlerini tasir.
/// </summary>
public static class ProcessBoundaryConsts
{
    /// <summary>Argumanlarda gecici calisma klasorunun yerini tutan kararli isarettir.</summary>
    public const string WorkspaceToken = "{workspace}";

    /// <summary>Tum gecici calisma klasorlerinin sistem temp altindaki ortak kokudur.</summary>
    public const string TempRootName = "ptn-test-module";
}
