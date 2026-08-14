using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Data;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.ExceptionCodes.Catalog;
using Ptn.TestModule.Interface.Catalog;
using Ptn.TestModule.Managers.Catalog;
using Ptn.TestModule.Models.Catalog;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Ptn.TestModule.EntityFrameworkCore.Catalog;

// islevi: Senaryo Manager, repository, EF indeks ve tenant davranislarini birlikte dogrular.
// sistemdeki gorevi: Surum, benzersizlik, onay invalidasyonu ve tarihsel kayit kurallarini gercek SQLite modelinde korur.
public class TestScenarioCatalogTests : TestModuleEntityFrameworkCoreTestBase
{
    // Ayni anahtarin farkli icerikleri monoton surum alirken ayni icerik domain koduyla reddedilmelidir.
    [Fact]
    public async Task Should_increment_versions_and_reject_duplicate_content()
    {
        await RunSeedAsync();

        await WithUnitOfWorkAsync(async () =>
        {
            var manager = ServiceProvider.GetRequiredService<TestScenarioManager>();
            var repository = ServiceProvider.GetRequiredService<ITestScenarioRepository>();
            var first = await manager.CreateAsync(CreateModel("catalog.version", 'a'));
            await repository.InsertAsync(first, autoSave: true);
            var second = await manager.CreateAsync(CreateModel("catalog.version", 'b'));
            await repository.InsertAsync(second, autoSave: true);

            first.VersionNo.ShouldBe(1);
            second.VersionNo.ShouldBe(2);
            var exception = await Should.ThrowAsync<BusinessException>(
                () => manager.CreateAsync(CreateModel("catalog.version", 'a')));
            exception.Code.ShouldBe(TestModuleScenarioErrorCodes.ContentAlreadyExists);
        });
    }

    // Source hash degistiginde hash'e bagli insan onayi temizlenip surum Draft'a donmelidir.
    [Fact]
    public async Task Should_invalidate_approval_when_source_hash_changes()
    {
        await RunSeedAsync();

        await WithUnitOfWorkAsync(async () =>
        {
            var manager = ServiceProvider.GetRequiredService<TestScenarioManager>();
            var scenario = await manager.CreateAsync(CreateModel("catalog.approval", 'a'));
            await manager.SubmitForApprovalAsync(scenario);
            await manager.BindApprovalAsync(scenario, Guid.NewGuid(), DateTime.UtcNow);

            await manager.UpdateAsync(scenario, UpdateModel('b'));

            scenario.ApprovedBy.ShouldBeNull();
            scenario.ApprovedAt.ShouldBeNull();
            scenario.ApprovalBoundToHash.ShouldBeNull();
        });
    }

    // Senaryo surumleri tarihsel kayit oldugu icin delete use-case'i her durumda reddedilmelidir.
    [Fact]
    public async Task Should_reject_deletion()
    {
        await RunSeedAsync();

        await WithUnitOfWorkAsync(async () =>
        {
            var manager = ServiceProvider.GetRequiredService<TestScenarioManager>();
            var scenario = await manager.CreateAsync(CreateModel("catalog.delete", 'a'));

            var exception = await Should.ThrowAsync<BusinessException>(
                () => manager.EnsureCanDeleteAsync(scenario));
            exception.Code.ShouldBe(TestModuleScenarioErrorCodes.DeletionNotAllowed);
        });
    }

    // ABP tenant filtresi baska kiracinin senaryo surumunu liste sonucundan gizlemelidir.
    [Fact]
    public async Task Should_isolate_scenarios_by_tenant()
    {
        await RunSeedAsync();
        var firstTenant = Guid.NewGuid();
        var secondTenant = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var context = await ServiceProvider
                .GetRequiredService<IDbContextProvider<TestModuleDbContext>>()
                .GetDbContextAsync();
            var draftStateId = await context.TestScenarioStates
                .Where(state => state.Code == TestScenarioStateCodes.Draft)
                .Select(state => state.Id)
                .SingleAsync();
            context.TestScenarios.Add(new TestScenario(
                Guid.NewGuid(), 1, draftStateId, firstTenant, CreateModel("catalog.tenant-one", 'a')));
            context.TestScenarios.Add(new TestScenario(
                Guid.NewGuid(), 1, draftStateId, secondTenant, CreateModel("catalog.tenant-two", 'b')));
            await context.SaveChangesAsync();

            using (ServiceProvider.GetRequiredService<ICurrentTenant>().Change(firstTenant))
            {
                var rows = await context.TestScenarios.AsNoTracking().ToListAsync();
                rows.Select(row => row.ScenarioKey).ShouldBe(["catalog.tenant-one"]);
            }
        });
    }

    // EF modeli DBML'deki iki unique indeks ile durum indeksini kararli adlariyla tasimalidir.
    [Fact]
    public void Should_map_catalog_indexes_and_restrict_state_deletion()
    {
        using var context = new TestModuleDbContext(
            new DbContextOptionsBuilder<TestModuleDbContext>().UseSqlite("DataSource=:memory:").Options);
        var entityType = context.Model.FindEntityType(typeof(TestScenario))!;

        entityType.GetIndexes().Single(index => index.GetDatabaseName() == TestScenarioConsts.VersionIndexName).IsUnique.ShouldBeTrue();
        entityType.GetIndexes().Single(index => index.GetDatabaseName() == TestScenarioConsts.ContentIndexName).IsUnique.ShouldBeTrue();
        entityType.GetIndexes().Single(index => index.GetDatabaseName() == TestScenarioConsts.StateIndexName).IsUnique.ShouldBeFalse();
        entityType.GetForeignKeys().Single().DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);
    }

    // Test Module lookup seed'ini Manager durum cozumlemesi icin gercek UOW'da calistirir.
    private Task RunSeedAsync()
    {
        return WithUnitOfWorkAsync(async () =>
        {
            var contributor = ServiceProvider.GetRequiredService<TestModuleLookupDataSeedContributor>();
            await contributor.SeedAsync(new DataSeedContext());
        });
    }

    // Manager create kurallari icin gecerli ve fingerprint'leri dolu model kurar.
    private static TestScenarioCreateModel CreateModel(string scenarioKey, char hashSeed)
    {
        return new TestScenarioCreateModel
        {
            ScenarioKey = scenarioKey,
            Title = "Scenario",
            SourceDocument = "source",
            SourceHash = Hash(hashSeed),
            CompiledDocument = "compiled",
            CompiledHash = Hash('c'),
            AssertionCount = 1,
            MaterialSeal = CreateMaterialSeal()
        };
    }

    // Manager update kurallari icin yeni kaynak hash'i tasiyan gecerli model kurar.
    private static TestScenarioUpdateModel UpdateModel(char hashSeed)
    {
        return new TestScenarioUpdateModel
        {
            Title = "Scenario updated",
            SourceDocument = "source updated",
            SourceHash = Hash(hashSeed),
            CompiledDocument = "compiled updated",
            CompiledHash = Hash('d'),
            AssertionCount = 1,
            MaterialSeal = CreateMaterialSeal()
        };
    }

    // Dolu malzeme gate'i icin dort kimlik ve fingerprint bagini kurar.
    private static TestScenarioMaterialSeal CreateMaterialSeal()
    {
        return new TestScenarioMaterialSeal
        {
            RulesFingerprint = Hash('e'),
            SpecSnapshotId = Guid.NewGuid(),
            SpecFingerprint = Hash('f'),
            DbConnectionId = Guid.NewGuid(),
            DbSchemaFingerprint = Hash('1'),
            ProfileFingerprint = Hash('2')
        };
    }

    // Test fingerprint'lerini DBML'nin 64 karakterlik digest biciminde uretir.
    private static string Hash(char value) => new(value, 64);
}
