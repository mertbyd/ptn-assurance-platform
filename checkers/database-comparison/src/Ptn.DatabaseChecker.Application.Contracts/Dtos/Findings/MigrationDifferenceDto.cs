namespace Ptn.DatabaseChecker.Dtos.Findings;

// islevi: Tek bir migration defter farkinin API modelidir (ComparisonRunDetailDto.Findings icinde).
// sistemdeki gorevi: MigrationDifferenceModel'in cevap karsiligi; kaynak/hedef semasini ve fark yonunu kararli Code ile tasir.
public class MigrationDifferenceDto
{
    /// <summary>
    /// Kosular arasinda kararli bulgu parmak izi.
    /// </summary>
    public string Fingerprint { get; set; } = default!;

    /// <summary>
    /// Migration farkinin kararli siddet kodu.
    /// </summary>
    public string SeverityCode { get; set; } = default!;

    /// <summary>
    /// Kaynak migration history tablosunun semasi; migration kaynakta yoksa null.
    /// </summary>
    public string? SourceSchemaName { get; set; }

    /// <summary>
    /// Hedef migration history tablosunun semasi; migration hedefte yoksa null.
    /// </summary>
    public string? TargetSchemaName { get; set; }

    /// <summary>
    /// Migration'in tam adi ("20260620093000_AddInvoiceModule").
    /// </summary>
    public string MigrationId { get; set; } = default!;

    /// <summary>
    /// Fark yonunun kararli kodu (DifferenceKindCodes.*).
    /// </summary>
    public string KindCode { get; set; } = default!;

    /// <summary>
    /// Migration'i kaynaga uygulayan EF Core surumu; kaynakta yoksa null.
    /// </summary>
    public string? SourceProductVersion { get; set; }

    /// <summary>
    /// Hedefteki EF Core surumu; ikisi dolu ve farkliysa surum kaymasidir.
    /// </summary>
    public string? TargetProductVersion { get; set; }
}
