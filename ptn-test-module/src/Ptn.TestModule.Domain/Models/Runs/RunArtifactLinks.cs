namespace Ptn.TestModule.Models.Runs;

// islevi: Bir terminal sonucun uc ihracat formatina ait blob adlarini tasir.
// sistemdeki gorevi: Agir ciktiyi tabloya sokmadan satirda tutulan resource_link kumesini tek domain modelinde toplar (PLAN-0003 TM-13).
/// <summary>
/// Terminal sonucun ihracat artefaktlarina isaret eden blob adlarini tasir.
/// </summary>
public class RunArtifactLinks
{
    /// <summary>CTRF raporunun kalici blob adidir; uretilmediyse null'dir.</summary>
    public string? CtrfBlobName { get; set; }

    /// <summary>JUnit raporunun kalici blob adidir; uretilmediyse null'dir.</summary>
    public string? JUnitBlobName { get; set; }

    /// <summary>SARIF raporunun kalici blob adidir; uretilmediyse null'dir.</summary>
    public string? SarifBlobName { get; set; }
}
