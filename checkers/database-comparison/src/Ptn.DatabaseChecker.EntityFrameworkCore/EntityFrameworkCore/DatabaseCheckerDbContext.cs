using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Entities;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Definitions;

using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Entities.Runs;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.GlobalFilters;

namespace Ptn.DatabaseChecker.EntityFrameworkCore;

[ConnectionStringName(DatabaseCheckerDbProperties.ConnectionStringName)]
public class DatabaseCheckerDbContext : AbpDbContext<DatabaseCheckerDbContext>, IDatabaseCheckerDbContext
{
    // IPassivable global filter: aktif olduğunda IsActive == false kayitlar otomatik harici tutulur.
    protected virtual bool IsPassivableFilterEnabled => DataFilter?.IsEnabled<IPassivable>() ?? false;
    // Sistemin sema okuyabildigi veritabani motorlari (lookup); ilk gercek is-alani tablomuz.
    public DbSet<DatabaseEngine> DatabaseEngines { get; set; } = null!;

    // Karsilastirma modu lookup'i (SchemaOnly/DataOnly/Both); Definition ve Run bu tabloya FK ile baglanir.
    public DbSet<ComparisonType> ComparisonTypes { get; set; } = null!;

    // Kapsam kurali anlami lookup'i (Include/Exclude/DataCompare); ScopeProfileItem ve RunScopeItem baglanir.
    public DbSet<ScopeKind> ScopeKinds { get; set; } = null!;

    // Calistirma durumu lookup'i (Pending/Running/Completed/Failed); ComparisonRun baglanir.
    public DbSet<ComparisonRunStatus> ComparisonRunStatuses { get; set; } = null!;

    // Sema nesne turu lookup'i (Table/View/Column/...); SchemaDifference baglanir.
    public DbSet<SchemaObjectType> SchemaObjectTypes { get; set; } = null!;

    // Fark yonu lookup'i (OnlyInSource/OnlyInTarget/Modified); Schema/Migration/DataRow farklari ortak baglanir.
    public DbSet<DifferenceKind> DifferenceKinds { get; set; } = null!;

    // Fark guveni lookup'i (Exact/Canonical/Approximate/Incomparable); SchemaDifference baglanir.
    public DbSet<ComparisonConfidence> ComparisonConfidences { get; set; } = null!;

    // Rapor formati lookup'i (Html/Markdown); ComparisonReport baglanir.
    public DbSet<ReportFormat> ReportFormats { get; set; } = null!;

    // Karsilastirma motorunun kullanacagi kayitli veritabani baglantilari.
    public DbSet<DatabaseConnection> DatabaseConnections { get; set; } = null!;

    // Kayitli karsilastirma is tarifleri; kapsam kurallari owned jsonb (ScopeRules) olarak tarif satirinda tasinir.
    public DbSet<ComparisonDefinition> ComparisonDefinitions { get; set; } = null!;

    // Kalici karsilastirma calistirma kayitlari; bulgular ve rapor icerikleri owned jsonb kolonlari olarak bu satirda tasinir. Scope kurallari yalnizca calisma sirasinda kullanilir, run'a yazilmaz.
    public DbSet<ComparisonRun> ComparisonRuns { get; set; } = null!;



    public DatabaseCheckerDbContext(DbContextOptions<DatabaseCheckerDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureDatabaseChecker();
    }

    protected override bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType)
    {
        if (typeof(IPassivable).IsAssignableFrom(typeof(TEntity)))
        {
            return true;
        }

        return base.ShouldFilterEntity<TEntity>(entityType);
    }

    protected override Expression<Func<TEntity, bool>>? CreateFilterExpression<TEntity>(
        ModelBuilder modelBuilder,
        EntityTypeBuilder<TEntity> entityTypeBuilder)
    {
        var expression = base.CreateFilterExpression<TEntity>(modelBuilder, entityTypeBuilder);
        if (typeof(IPassivable).IsAssignableFrom(typeof(TEntity)))
        {
            Expression<Func<TEntity, bool>> passivableFilter =
                e => !IsPassivableFilterEnabled || ((IPassivable)e).IsActive;
            expression = expression == null
                ? passivableFilter
                : QueryFilterExpressionHelper.CombineExpressions(expression, passivableFilter);
        }
        return expression;
    }
}
