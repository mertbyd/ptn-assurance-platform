using Microsoft.EntityFrameworkCore;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Definitions;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Entities.Runs;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Ptn.DatabaseChecker.EntityFrameworkCore;

[ConnectionStringName(DatabaseCheckerDbProperties.ConnectionStringName)]
public interface IDatabaseCheckerDbContext : IEfCoreDbContext
{
    // Sistemin sema okuyabildigi veritabani motorlari (lookup); repository ve seed bu kume uzerinden calisir.
    DbSet<DatabaseEngine> DatabaseEngines { get; }

    // Karsilastirma modu lookup'i (SchemaOnly/DataOnly/Both); repository ve seed bu kume uzerinden calisir.
    DbSet<ComparisonType> ComparisonTypes { get; }

    // Kapsam kurali anlami lookup'i (Include/Exclude/DataCompare).
    DbSet<ScopeKind> ScopeKinds { get; }

    // Calistirma durumu lookup'i (Pending/Running/Completed/Failed).
    DbSet<ComparisonRunStatus> ComparisonRunStatuses { get; }

    // Sema nesne turu lookup'i (Table/View/Column/...).
    DbSet<SchemaObjectType> SchemaObjectTypes { get; }

    // Fark yonu lookup'i (OnlyInSource/OnlyInTarget/Modified).
    DbSet<DifferenceKind> DifferenceKinds { get; }

    // Fark guveni lookup'i (Exact/Canonical/Approximate/Incomparable).
    DbSet<ComparisonConfidence> ComparisonConfidences { get; }

    // Rapor formati lookup'i (Html/Markdown).
    DbSet<ReportFormat> ReportFormats { get; }

    // Kayitli veritabani baglantilari.
    DbSet<DatabaseConnection> DatabaseConnections { get; }

    // Kayitli karsilastirma tarifleri; kapsam kurallari owned jsonb (ScopeRules) olarak tarif satirinda tasinir.
    DbSet<ComparisonDefinition> ComparisonDefinitions { get; }

    // Kalici karsilastirma calistirma kayitlari; bulgular ve rapor icerikleri owned jsonb kolonlari olarak bu satirda tasinir. Scope kurallari yalnizca calisma sirasinda kullanilir, run'a yazilmaz.
    DbSet<ComparisonRun> ComparisonRuns { get; }

    
}
