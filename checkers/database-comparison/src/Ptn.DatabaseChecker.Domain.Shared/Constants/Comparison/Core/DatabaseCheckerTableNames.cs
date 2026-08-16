namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Comparison semasindaki tablo adlarini tek kaynakta toplar.
// sistemdeki gorevi: EF configuration dosyalarinda anlamli tablo adi string'lerinin dagilmasini engeller; migration, seed ve mapping ayni sabitleri kullanir.
public static class DatabaseCheckerTableNames
{
    // Lookup: desteklenen veritabani motorlari.
    public const string DatabaseEngines = "database_engines";

    // Lookup: karsilastirma modlari.
    public const string ComparisonTypes = "comparison_types";

    // Lookup: kapsam kural turleri.
    public const string ScopeKinds = "scope_kinds";

    // Lookup: run yasam dongusu durumlari.
    public const string ComparisonRunStatuses = "comparison_run_statuses";

    // Lookup: sema nesne turleri.
    public const string SchemaObjectTypes = "schema_object_types";

    // Lookup: fark yonleri.
    public const string DifferenceKinds = "difference_kinds";

    // Lookup: fark guven seviyeleri.
    public const string ComparisonConfidences = "comparison_confidences";

    // Lookup: rapor formatlari.
    public const string ReportFormats = "report_formats";

    // Kayitli veritabani baglantilari.
    public const string DatabaseConnections = "database_connections";

    // Kayitli karsilastirma tarifleri; kapsam kurallari owned jsonb (scope_rules) olarak tarif satirinda tasinir.
    public const string ComparisonDefinitions = "comparison_definitions";

    // Kalici karsilastirma calistirma kayitlari; kapsam snapshot'i, bulgular ve rapor icerikleri owned jsonb kolonlari (scope_snapshot/findings/reports) olarak bu tabloda tasinir.
    public const string ComparisonRuns = "comparison_runs";

}
