using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.ApiContractChecker.Entities;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities.Runs;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Entities.Sources;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.GlobalFilters;

namespace Ptn.ApiContractChecker.EntityFrameworkCore;

[ConnectionStringName(ApiContractCheckerDbProperties.ConnectionStringName)]
public class ApiContractCheckerDbContext : AbpDbContext<ApiContractCheckerDbContext>, IApiContractCheckerDbContext
{
    // IPassivable global filter: aktif olduğunda IsActive == false kayitlar otomatik harici tutulur.
    protected virtual bool IsPassivableFilterEnabled => DataFilter?.IsEnabled<IPassivable>() ?? false;
    // Izlenen servis aggregate kokleri.
    public DbSet<SpecSource> SpecSources { get; set; } = null!;

    // SpecSource aggregate'ine ait dokuman cocuklari.
    public DbSet<SpecDocument> SpecDocuments { get; set; } = null!;

    // Tenant icinde hash ile adreslenen degismez spec icerikleri.
    public DbSet<SpecContent> SpecContents { get; set; } = null!;

    // Dokumanlarin zaman-serisi snapshot kayitlari.
    public DbSet<SpecSnapshot> SpecSnapshots { get; set; } = null!;

    // Iki snapshot arasindaki contract check calistirmalari.
    public DbSet<ContractCheckRun> ContractCheckRuns { get; set; } = null!;

    // Global spec formati sozlugu.
    public DbSet<SpecFormat> SpecFormats { get; set; } = null!;

    // Global run durumu sozlugu.
    public DbSet<CheckRunStatus> CheckRunStatuses { get; set; } = null!;

    // Global fark siddeti sozlugu.
    public DbSet<DifferenceSeverity> DifferenceSeverities { get; set; } = null!;

    // Global fark yonu sozlugu.
    public DbSet<DifferenceDirection> DifferenceDirections { get; set; } = null!;

    // Global fark turu sozlugu.
    public DbSet<DifferenceKind> DifferenceKinds { get; set; } = null!;

    public ApiContractCheckerDbContext(DbContextOptions<ApiContractCheckerDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureApiContractChecker();
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
