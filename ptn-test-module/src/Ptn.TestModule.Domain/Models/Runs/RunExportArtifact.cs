namespace Ptn.TestModule.Models.Runs;

// islevi: Uretilmis tek ihracat artefaktinin format kodunu, blob adini ve govdesini tasir.
// sistemdeki gorevi: Ad turetme ve icerik uretimi kararlarini Manager'da birakip servise yalniz yazma isini verir (PLAN-0003 TM-13/TM-14).
/// <summary>
/// Kalici depoya yazilmaya hazir tek ihracat artefaktini tasir.
/// </summary>
public class RunExportArtifact
{
    /// <summary>Artefaktin kapali kume icindeki format kodudur.</summary>
    public string FormatCode { get; set; } = string.Empty;

    /// <summary>Artefaktin kalici depodaki kararli blob adidir.</summary>
    public string BlobName { get; set; } = string.Empty;

    /// <summary>Artefaktin deterministik olarak uretilmis govdesidir.</summary>
    public string Content { get; set; } = string.Empty;
}
