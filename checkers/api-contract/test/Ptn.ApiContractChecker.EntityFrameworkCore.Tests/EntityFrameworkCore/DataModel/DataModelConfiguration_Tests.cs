using Microsoft.EntityFrameworkCore;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Entities.Runs;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Entities.Sources;
using Shouldly;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.DataModel;

// islevi: KBP-606 EF modelinin tablo, FK, tenant index ve owned JSON sozlesmesini dogrular.
// sistemdeki gorevi: Migration uretilmeden once veya sonra kanonik 10 tablo modelinin sessizce genislemesini engeller.
[Collection(EfCoreIntegrationCollection.Name)]
public class DataModelConfiguration_Tests : ApiContractCheckerEntityFrameworkCoreTestBase
{
    private readonly IDbContextProvider<ApiContractCheckerDbContext> _dbContextProvider;

    public DataModelConfiguration_Tests()
    {
        _dbContextProvider = GetRequiredService<IDbContextProvider<ApiContractCheckerDbContext>>();
    }

    // Checker semasinda yalniz bes entity ve bes lookup tablosu bulundugunu dogrular.
    [Fact]
    public async Task Model_Should_Contain_Exactly_The_Ten_Checker_Tables()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = await _dbContextProvider.GetDbContextAsync();
            var tables = dbContext.Model.GetEntityTypes()
                .Where(entityType => !entityType.IsOwned())
                .Where(entityType => entityType.GetSchema() == ApiContractCheckerDbProperties.CheckerSchema)
                .Select(entityType => entityType.GetTableName())
                .ToHashSet(StringComparer.Ordinal);

            tables.ShouldBe(new HashSet<string?>
            {
                ApiContractCheckerTableNames.SpecSources,
                ApiContractCheckerTableNames.SpecDocuments,
                ApiContractCheckerTableNames.SpecContents,
                ApiContractCheckerTableNames.SpecSnapshots,
                ApiContractCheckerTableNames.ContractCheckRuns,
                ApiContractCheckerTableNames.SpecFormats,
                ApiContractCheckerTableNames.CheckRunStatuses,
                ApiContractCheckerTableNames.DifferenceSeverities,
                ApiContractCheckerTableNames.DifferenceDirections,
                ApiContractCheckerTableNames.DifferenceKinds
            }, ignoreOrder: true);
        });
    }

    // Uygulama-domain FK'larinin fiziksel silmeyle tarihce kaybettirmemesini dogrular.
    [Fact]
    public async Task Historical_Foreign_Keys_Should_Use_Restrict()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = await _dbContextProvider.GetDbContextAsync();
            var entityTypes = new[]
            {
                typeof(SpecDocument),
                typeof(SpecSnapshot),
                typeof(ContractCheckRun)
            };

            var foreignKeys = entityTypes
                .Select(type => dbContext.Model.FindEntityType(type).ShouldNotBeNull())
                .SelectMany(entityType => entityType.GetForeignKeys())
                .ToList();

            foreignKeys.Count.ShouldBe(7);
            foreignKeys.ShouldAllBe(foreignKey => foreignKey.DeleteBehavior == DeleteBehavior.Restrict);
        });
    }

    // Tenant kayitlarini tenant icinde, host kayitlarini olusturan kullanici icinde tekil tutan filtreli indexleri dogrular.
    [Fact]
    public async Task Source_Unique_Indexes_Should_Match_Visibility_Scope()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = await _dbContextProvider.GetDbContextAsync();

            AssertUniqueIndex<SpecSource>(
                dbContext,
                "\"TenantId\" IS NOT NULL",
                nullsAreDistinct: null,
                nameof(SpecSource.TenantId),
                nameof(SpecSource.Name));
            AssertUniqueIndex<SpecSource>(
                dbContext,
                "\"TenantId\" IS NULL",
                nullsAreDistinct: false,
                nameof(SpecSource.CreatorId),
                nameof(SpecSource.Name));
        });
    }

    // Icerik hash'i tenant icinde, host kapsaminda ise tek ortak kayit olarak tekil kalir.
    [Fact]
    public async Task Content_Hash_Unique_Index_Should_Treat_Nulls_As_Equal()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = await _dbContextProvider.GetDbContextAsync();

            AssertUniqueIndex<SpecContent>(
                dbContext,
                filter: null,
                nullsAreDistinct: false,
                nameof(SpecContent.TenantId),
                nameof(SpecContent.RawHash));
        });
    }

    // ContractCheckRun bulgularinin ayrik tablo degil jsonb container olarak eslendigini dogrular.
    [Fact]
    public void Findings_Should_Be_Mapped_To_Jsonb()
    {
        var options = new DbContextOptionsBuilder<ApiContractCheckerDbContext>()
            .UseNpgsql("Host=localhost;Database=model-verification")
            .UseSnakeCaseNamingConvention()
            .Options;
        using var dbContext = new ApiContractCheckerDbContext(options);
        var table = dbContext.Model.GetRelationalModel().Tables.Single(candidate =>
            candidate.Name == ApiContractCheckerTableNames.ContractCheckRuns &&
            candidate.Schema == ApiContractCheckerDbProperties.CheckerSchema);
        var findingsColumn = table.Columns.Single(column => column.Name == "findings");

        findingsColumn.StoreType.ShouldBe("jsonb");
    }

    // Verilen entity uzerindeki kolon sirali unique indexin filtre ve PostgreSQL null semantigini denetler.
    private static void AssertUniqueIndex<TEntity>(
        ApiContractCheckerDbContext dbContext,
        string? filter,
        bool? nullsAreDistinct,
        params string[] propertyNames)
    {
        var entityType = dbContext.Model.FindEntityType(typeof(TEntity)).ShouldNotBeNull();
        var index = entityType.GetIndexes()
            .Single(candidate => candidate.Properties.Select(property => property.Name).SequenceEqual(propertyNames));

        index.IsUnique.ShouldBeTrue();
        index.GetFilter().ShouldBe(filter);
        index.GetAreNullsDistinct().ShouldBe(nullsAreDistinct);
    }
}
