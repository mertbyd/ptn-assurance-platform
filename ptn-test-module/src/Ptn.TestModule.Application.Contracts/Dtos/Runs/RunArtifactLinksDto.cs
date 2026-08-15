namespace Ptn.TestModule.Dtos.Runs;

// islevi: Terminal sonucun uc ihracat formatina ait resource_link degerlerini disari acar.
// sistemdeki gorevi: Agir ciktinin kendisini degil, blob deposundaki kararli adresini tasiyan public sozlesmedir (PLAN-0003 TM-13).
/// <summary>
/// Bir terminal sonucun ihracat artefaktlarina isaret eden baglarini tasir.
/// </summary>
public class RunArtifactLinksDto
{
    /// <summary>CTRF raporunun kalici blob adidir; uretilmediyse null'dir.</summary>
    public string? CtrfBlobName { get; set; }

    /// <summary>JUnit raporunun kalici blob adidir; uretilmediyse null'dir.</summary>
    public string? JUnitBlobName { get; set; }

    /// <summary>SARIF raporunun kalici blob adidir; uretilmediyse null'dir.</summary>
    public string? SarifBlobName { get; set; }
}
