namespace Ptn.DatabaseChecker.Models.Comparison.Findings;

// islevi: Iki tarafin __EFMigrationsHistory defterleri arasindaki tek bir kume farki; "NEDEN farkli?" sorusunun cevabi.
// sistemdeki gorevi: Eski MigrationDifference tablosunun owned-JSON karsiligi; ComparisonFindings.MigrationDifferences icinde kaynak/hedef migration semasiyla tasinir. Sema farklarini yorumlanmis deployment raporuna yukseltir.
public class MigrationDifferenceModel
{
    /// <summary>
    /// Ayni migration farkini kosular arasinda kararli olarak tanimlayan SHA-256 degeri.
    /// </summary>
    public string Fingerprint { get; set; } = default!;

    /// <summary>
    /// Migration farkinin uyumluluk etkisini belirleyen kararli siddet kodu.
    /// </summary>
    public string SeverityCode { get; set; } = default!;

    // Kaynak migration history tablosunun semasi; migration kaynakta yoksa null.
    public string? SourceSchemaName { get; set; }

    // Hedef migration history tablosunun semasi; migration hedefte yoksa null.
    public string? TargetSchemaName { get; set; }

    // Migration'in tam adi ("20260620093000_AddInvoiceModule"); zaman damgasi sayesinde alfabetik = kronolojik.
    public string MigrationId { get; set; } = default!;

    // Fark yonunun kararli kodu (DifferenceKindCodes.*): OnlyInSource = bekleyen deploy, OnlyInTarget = arastirilmali, Modified = surum farki.
    public string KindCode { get; set; } = default!;

    // Migration'i kaynaga uygulayan EF Core surumu; kaynakta yoksa null.
    public string? SourceProductVersion { get; set; }

    // Hedefteki EF Core surumu; ikisi dolu ve farkliysa surum kaymasi uyarisidir.
    public string? TargetProductVersion { get; set; }
}
