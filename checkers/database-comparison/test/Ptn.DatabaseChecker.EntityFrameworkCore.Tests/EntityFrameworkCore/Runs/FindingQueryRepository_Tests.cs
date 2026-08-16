using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System.Threading;
using System.Text.Json;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities;
using Ptn.DatabaseChecker.Interface.Runs;
using Ptn.DatabaseChecker.Managers.Runs;
using Ptn.DatabaseChecker.Models.Runs;
using Ptn.DatabaseChecker.Repository.Runs;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Users;
using Xunit;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Runs;

// islevi: Owned JSON bulgu filtre, limit ve PostgreSQL projection davranislarini dogrular.
// sistemdeki gorevi: MCP okumasinin tam findings belgesini materyalize etmeden sunucuda altkume ve sayfa urettigini kanitlar.
/// <summary>
/// Filtre, limit ve PostgreSQL owned JSON projection davranislarini dogrular.
/// </summary>
public class FindingQueryRepository_Tests
{
    [Fact]
    public async Task Finding_Query_Should_Filter_Clamp_And_Project_Server_Side()
    {
        Combined_Filters_And_Since_Run_Should_Return_Only_New_Matching_Findings();
        await Requested_Page_Size_Should_Be_Clamped_To_Setting_Max_And_Keep_Total_Count();
        PostgreSql_Query_Should_Project_Json_Elements_With_Server_Limit();
    }

    [Fact]
    public async Task Manager_Should_Reject_Invalid_Reference_And_Normalize_Valid_Fingerprints()
    {
        var currentRunId = Guid.NewGuid();
        var referenceRunId = Guid.NewGuid();
        var repository = CreateRepositorySubstitute();
        repository.IsValidCompletedReferenceRunAsync(currentRunId, referenceRunId, Arg.Any<CancellationToken>())
            .Returns(false, true);
        repository.GetFindingsAsync(currentRunId, Arg.Any<FindingQueryModel>(), Arg.Any<CancellationToken>())
            .Returns(new FindingPageModel());
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        var manager = new FindingQueryManager(repository, new FindingQuerySettingsResolver(settings));
        var input = new FindingQueryModel
        {
            SinceRunId = referenceRunId,
            Fingerprints = ["aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"]
        };

        var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
            manager.GetFindingsAsync(currentRunId, input));
        exception.Code.ShouldBe(Ptn.DatabaseChecker.ExceptionCodes.ComparisonRunExceptionCodes.InvalidFindingReferenceRun);

        await manager.GetFindingsAsync(currentRunId, input);
        await repository.Received().GetFindingsAsync(
            currentRunId,
            Arg.Is<FindingQueryModel>(query =>
                query.SinceRunId == referenceRunId &&
                query.Fingerprints.Single() == "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"),
            Arg.Any<CancellationToken>());
    }

    private static void Combined_Filters_And_Since_Run_Should_Return_Only_New_Matching_Findings()
    {
        var findings = new[]
        {
            CreateFinding("orders", DifferenceSeverityCodes.Breaking, "a"),
            CreateFinding("customers", DifferenceSeverityCodes.Breaking, "b"),
            CreateFinding("orders", DifferenceSeverityCodes.Warning, "c"),
            CreateFinding("orders", DifferenceSeverityCodes.Breaking, "d"),
            CreateFinding("orders", DifferenceSeverityCodes.Breaking, null)
        }.AsQueryable();

        var filtered = ComparisonRunRepository.ApplyFindingFilters(findings, new FindingQueryModel
        {
            SeverityCode = DifferenceSeverityCodes.Breaking,
            TableName = "orders",
            Fingerprints = ["a", "d"]
        });
        filtered = ComparisonRunRepository.ApplySinceRunFilter(filtered, ["a"]);

        filtered.Single().Fingerprint.ShouldBe("d");
    }

    private static async Task Requested_Page_Size_Should_Be_Clamped_To_Setting_Max_And_Keep_Total_Count()
    {
        var repository = CreateRepositorySubstitute();
        repository.GetFindingsAsync(
                Arg.Any<Guid>(),
                Arg.Any<FindingQueryModel>(),
                Arg.Any<CancellationToken>())
            .Returns(call => BuildPage(call.ArgAt<FindingQueryModel>(1)));
        var settingProvider = Substitute.For<ISettingProvider>();
        settingProvider.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        var manager = new FindingQueryManager(
            repository,
            new FindingQuerySettingsResolver(settingProvider));

        var page = await manager.GetFindingsAsync(Guid.NewGuid(), new FindingQueryModel
        {
            MaxResultCount = 1000
        });

        page.TotalCount.ShouldBe(105);
        page.Items.Count.ShouldBeLessThanOrEqualTo(ComparisonRunConsts.DefaultMaxFindingPageSize);
        JsonSerializer.SerializeToUtf8Bytes(page).Length
            .ShouldBeLessThanOrEqualTo(ComparisonRunConsts.DefaultFindingResponseBytes);
        await repository.Received().GetFindingsAsync(
            Arg.Any<Guid>(),
            Arg.Is<FindingQueryModel>(input =>
                input.MaxResultCount == ComparisonRunConsts.DefaultMaxFindingPageSize),
            Arg.Any<CancellationToken>());
    }

    // islevi: Default interface govdesine baglanmadan gercek repository override'ini izlenebilir test double olarak kurar.
    private static ComparisonRunRepository CreateRepositorySubstitute()
        => Substitute.For<ComparisonRunRepository>(
            Substitute.For<IDbContextProvider<DatabaseCheckerDbContext>>(),
            Substitute.For<IDataFilter<IPassivable>>(),
            Substitute.For<ICurrentTenant>(),
            Substitute.For<ICurrentUser>());

    private static void PostgreSql_Query_Should_Project_Json_Elements_With_Server_Limit()
    {
        var options = new DbContextOptionsBuilder<DatabaseCheckerDbContext>()
            .UseNpgsql("Host=localhost;Database=query-proof;Username=test;Password=test")
            .UseSnakeCaseNamingConvention()
            .Options;
        using var dbContext = new DatabaseCheckerDbContext(options);
        var runs = dbContext.ComparisonRuns
            .IgnoreQueryFilters()
            .Where(run => run.Id == Guid.NewGuid());
        var sql = ComparisonRunRepository.BuildSchemaFindingQuery(runs)
            .OrderBy(finding => finding.Fingerprint)
            .Take(20)
            .ToQueryString();

        sql.ToUpperInvariant().ShouldContain("LATERAL");
        sql.ToUpperInvariant().ShouldContain("LIMIT");
        sql.ShouldNotContain("SELECT r.findings");
    }

    // islevi: Repository substitute'unda uygulanan sayfa boyutuyla sinirli 105 toplamli cevap kurar.
    private static Task<FindingPageModel> BuildPage(FindingQueryModel input)
        => Task.FromResult(new FindingPageModel
        {
            TotalCount = 105,
            Items = Enumerable.Range(0, Math.Min(105, input.MaxResultCount))
                .Select(index => CreateFinding("orders", DifferenceSeverityCodes.NonBreaking, index.ToString("D3")))
                .ToList()
        });

    // islevi: Ortak projection bulgusunu filtre, sayfalama ve SQL fixture'lari icin kurar.
    private static FindingReadModel CreateFinding(string tableName, string severityCode, string? fingerprint)
        => new()
        {
            Fingerprint = fingerprint,
            SeverityCode = severityCode,
            SchemaName = "public",
            ObjectName = tableName,
            TableName = tableName,
            ObjectTypeCode = SchemaObjectTypeCodes.Table,
            KindCode = DifferenceKindCodes.OnlyInSource,
            ConfidenceCode = ComparisonConfidenceCodes.Exact,
            SourceValue = tableName
        };
}
