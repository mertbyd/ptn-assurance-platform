using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Constants.Formats.Lookups;
using Ptn.ApiContractChecker.Constants.Runs;
using Ptn.ApiContractChecker.Constants.Runs.Lookups;
using Ptn.ApiContractChecker.Dtos.Runs;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities.Runs;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.ExceptionCodes;
using Ptn.ApiContractChecker.ExceptionCodes.Runs;
using Ptn.ApiContractChecker.Interface.Runs;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Managers.Runs;
using Ptn.ApiContractChecker.Managers.Snapshots;
using Ptn.ApiContractChecker.Managers.Sources;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Services.Runs;
using Ptn.ApiContractChecker.Repository.Runs;
using Ptn.ApiContractChecker.EntityFrameworkCore;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Runs;

// islevi: ContractCheckRun projeksiyon, detay, rapor, filtre, sayfalama ve tenant izolasyonunu gercek EF saglayicisiyla dogrular.
// sistemdeki gorevi: Findings JSON'un liste/status yoluna sizmasini ve run gecmisinin siralama ya da tenant regresyonunu engeller.
[Collection(EfCoreIntegrationCollection.Name)]
public class ContractCheckRunRepository_Tests : ApiContractCheckerEntityFrameworkCoreTestBase
{
    private readonly IContractCheckRunAppService _appService;
    private readonly IContractCheckRunRepository _runRepository;
    private readonly ISpecSourceRepository _sourceRepository;
    private readonly IRepository<SpecContent, Guid> _contentRepository;
    private readonly IRepository<SpecSnapshot, Guid> _snapshotRepository;
    private readonly IRepository<SpecFormat, Guid> _formatRepository;
    private readonly IRepository<CheckRunStatus, Guid> _statusRepository;
    private readonly ContractCheckRunManager _manager;
    private readonly SpecIngestionManager _ingestionManager;
    private readonly SpecSourceManager _sourceManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly SqlCommandCaptureInterceptor _commandCapture;

    public ContractCheckRunRepository_Tests()
    {
        _appService = GetRequiredService<IContractCheckRunAppService>();
        _runRepository = GetRequiredService<IContractCheckRunRepository>();
        _sourceRepository = GetRequiredService<ISpecSourceRepository>();
        _contentRepository = GetRequiredService<IRepository<SpecContent, Guid>>();
        _snapshotRepository = GetRequiredService<IRepository<SpecSnapshot, Guid>>();
        _formatRepository = GetRequiredService<IRepository<SpecFormat, Guid>>();
        _statusRepository = GetRequiredService<IRepository<CheckRunStatus, Guid>>();
        _manager = GetRequiredService<ContractCheckRunManager>();
        _ingestionManager = GetRequiredService<SpecIngestionManager>();
        _sourceManager = GetRequiredService<SpecSourceManager>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _commandCapture = GetRequiredService<SqlCommandCaptureInterceptor>();
    }

    // Liste SQL'inin findings kolonunu secmedigini ve detay yolunun tum bulgulari dondurdugunu kanitlar.
    [Fact]
    public async Task List_Should_Project_Header_Without_Findings_And_Detail_Should_Return_Findings()
    {
        var tenantId = Guid.NewGuid();
        var findings = CreateFindings();
        var fixture = await CreateRunGraphAsync(tenantId, DateTime.UtcNow.AddMinutes(-5), findings);

        using (_currentTenant.Change(tenantId))
        {
            _commandCapture.Clear();
            var list = await _appService.GetListAsync(new GetContractCheckRunsInput());

            list.Items.ShouldContain(item => item.Id == fixture.RunId);
            string.Join(Environment.NewLine, _commandCapture.Commands)
                .ToLowerInvariant()
                .ShouldNotContain(ContractCheckRunConsts.FindingsJsonColumnName);

            _commandCapture.Clear();
            var detail = await _appService.GetAsync(fixture.RunId);

            detail.Findings.Items.Count.ShouldBe(findings.Items.Count);
            detail.Findings.Items.Select(item => item.KindCode)
                .ShouldBe(findings.Items.Select(item => item.KindCode));
            string.Join(Environment.NewLine, _commandCapture.Commands)
                .ToLowerInvariant()
                .ShouldContain(ContractCheckRunConsts.FindingsJsonColumnName);
        }
    }

    // Kaynak ve dokuman filtrelerinin ABP sayfalama ile CreationTime azalan sirayi birlikte korudugunu kanitlar.
    [Fact]
    public async Task List_Should_Filter_Page_And_Order_By_CreationTime_Descending()
    {
        var tenantId = Guid.NewGuid();
        var older = await CreateRunGraphAsync(tenantId, DateTime.UtcNow.AddHours(-3), ContractCheckFindings.Empty());
        await CreateRunGraphAsync(tenantId, DateTime.UtcNow.AddHours(-2), ContractCheckFindings.Empty());
        var newerRunId = Guid.Empty;
        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                newerRunId = await CreateRunAsync(
                    tenantId,
                    older.BaseSnapshotId,
                    older.TargetSnapshotId,
                    DateTime.UtcNow.AddHours(-1),
                    ContractCheckFindings.Empty());
            }
        });

        using (_currentTenant.Change(tenantId))
        {
            var firstPage = await _appService.GetListAsync(new GetContractCheckRunsInput
            {
                SpecSourceId = older.SourceId,
                MaxResultCount = 1
            });
            var secondPage = await _appService.GetListAsync(new GetContractCheckRunsInput
            {
                SpecDocumentId = older.DocumentId,
                SkipCount = 1,
                MaxResultCount = 1
            });

            firstPage.TotalCount.ShouldBe(2);
            firstPage.Items.Single().Id.ShouldBe(newerRunId);
            secondPage.TotalCount.ShouldBe(2);
            secondPage.Items.Single().Id.ShouldBe(older.RunId);
        }
    }

    // Baska tenant'a ait run detayinin kararli NotFound BusinessException'i oldugunu kanitlar.
    [Fact]
    public async Task Other_Tenant_Run_Should_Be_NotFound()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var fixture = await CreateRunGraphAsync(tenantA, DateTime.UtcNow, ContractCheckFindings.Empty());

        using (_currentTenant.Change(tenantB))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() => _appService.GetAsync(fixture.RunId));
            exception.Code.ShouldBe(GeneralExceptionCodes.NotFound);
        }
    }

    // Ayni findings govdesinin her istekte ayni raporu ve entity sayaclariyla tutarli ozeti urettigini kanitlar.
    [Fact]
    public async Task Report_Should_Be_Deterministic_And_Match_Run_Counts()
    {
        var tenantId = Guid.NewGuid();
        var findings = CreateFindings();
        var fixture = await CreateRunGraphAsync(
            tenantId,
            DateTime.UtcNow,
            ContractCheckFindings.Empty(),
            complete: false);

        using (_currentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(() => _manager.StartAsync(fixture.RunId));
            await WithUnitOfWorkAsync(() => _manager.CompleteAsync(fixture.RunId, findings));
            var first = await _appService.GetReportAsync(fixture.RunId);
            var second = await _appService.GetReportAsync(fixture.RunId);
            var status = await _appService.GetStatusAsync(fixture.RunId);

            JsonSerializer.Serialize(first).ShouldBe(JsonSerializer.Serialize(second));
            first.Summary.TotalFindingCount.ShouldBe(3);
            first.Summary.BreakingCount.ShouldBe(status.BreakingCount);
            first.Summary.NonBreakingCount.ShouldBe(status.NonBreakingCount);
            first.Summary.DocsOnlyCount.ShouldBe(status.DocsOnlyCount);
            first.Groups.Sum(group => group.FindingCount).ShouldBe(first.Summary.TotalFindingCount);
        }
    }

    // Pending run'in findings govdesi bosken status ve raporun sifir sayacli bos ozet dondurdugunu kanitlar.
    [Fact]
    public async Task Pending_Run_Should_Return_Empty_Report()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateRunGraphAsync(
            tenantId,
            DateTime.UtcNow,
            ContractCheckFindings.Empty(),
            complete: false);

        using (_currentTenant.Change(tenantId))
        {
            var status = await _appService.GetStatusAsync(fixture.RunId);
            var report = await _appService.GetReportAsync(fixture.RunId);

            status.StatusCode.ShouldBe(CheckRunStatusCodes.Pending);
            status.BreakingCount.ShouldBe(0);
            report.Summary.TotalFindingCount.ShouldBe(0);
            report.Summary.BreakingCount.ShouldBe(0);
            report.Summary.NonBreakingCount.ShouldBe(0);
            report.Summary.DocsOnlyCount.ShouldBe(0);
            report.Summary.SeverityCounts.ShouldBeEmpty();
            report.Summary.DirectionCounts.ShouldBeEmpty();
            report.Summary.KindCounts.ShouldBeEmpty();
            report.Groups.ShouldBeEmpty();
        }
    }

    // SQLite geri cekilmesinin yalniz secili run govdesini alip filtre ve sayfayi dogru uyguladigini kanitlar.
    [Fact]
    public async Task Findings_Should_Filter_Page_And_Query_Only_Selected_Run()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateRunGraphAsync(tenantId, DateTime.UtcNow, CreateFingerprintFindings(3));
        var referenceRunId = Guid.Empty;
        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                referenceRunId = await CreateRunAsync(
                    tenantId,
                    fixture.BaseSnapshotId,
                    fixture.TargetSnapshotId,
                    DateTime.UtcNow.AddMinutes(-1),
                    CreateReferenceFindings());
            }
        });

        using (_currentTenant.Change(tenantId))
        {
            _commandCapture.Clear();
            var page = await _appService.GetFindingsAsync(fixture.RunId, new GetContractCheckFindingsInput
            {
                SeverityCode = DifferenceSeverityCodes.Breaking,
                Path = "/orders/",
                SinceRunId = referenceRunId,
                Fingerprints = [Fingerprint(1), Fingerprint(2)],
                MaxResultCount = 2
            });

            page.TotalCount.ShouldBe(2);
            page.Items.Count.ShouldBe(2);
            page.Items.ShouldAllBe(item => item.ChangeStateCode == FindingChangeStateCodes.New);
            page.Items.ShouldAllBe(item => item.Address.HttpMethod == "GET");
            page.Items.Select(item => item.Address.Path).ShouldBe(["/orders/1", "/orders/2"]);
            var sql = string.Join(Environment.NewLine, _commandCapture.Commands).ToLowerInvariant();
            sql.ShouldContain(ContractCheckRunConsts.FindingsJsonColumnName);
            sql.ShouldContain("where");
        }
    }

    // Ayni tenant'ta farkli base/target dokuman tanimina ait run'in explicit referans olamayacagini kanitlar.
    [Fact]
    public async Task Findings_Should_Reject_Reference_From_Different_Document_Definition()
    {
        var tenantId = Guid.NewGuid();
        var current = await CreateRunGraphAsync(tenantId, DateTime.UtcNow, CreateFingerprintFindings(2));
        var unrelated = await CreateRunGraphAsync(
            tenantId, DateTime.UtcNow.AddMinutes(-1), CreateFingerprintFindings(1));

        using (_currentTenant.Change(tenantId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _appService.GetFindingsAsync(current.RunId, new GetContractCheckFindingsInput
                {
                    SinceRunId = unrelated.RunId
                }));
            exception.Code.ShouldBe(ContractCheckRunExceptionCodes.InvalidFindingReferenceRun);
        }
    }

    // PostgreSQL yolunun owned JSON'u sunucuda acip fingerprint scalarini run predicate'iyle projekte ettigini kanitlar.
    [Fact]
    public void PostgreSql_Fingerprint_Query_Should_Stay_Server_Side()
    {
        var options = new DbContextOptionsBuilder<ApiContractCheckerDbContext>()
            .UseNpgsql("Host=localhost;Database=query-proof;Username=test;Password=test")
            .UseSnakeCaseNamingConvention()
            .Options;
        using var dbContext = new ApiContractCheckerDbContext(options);
        var method = typeof(ContractCheckRunRepository).GetMethod(
            "BuildFindingQuery", BindingFlags.Static | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        var query = method.Invoke(null, [
            dbContext.ContractCheckRuns.IgnoreQueryFilters(), Guid.NewGuid()
        ]).ShouldBeAssignableTo<IQueryable<Finding>>()!;
        var sql = query
            .Where(finding => finding.Fingerprint != null)
            .Select(finding => finding.Fingerprint)
            .ToQueryString()
            .ToLowerInvariant();

        sql.ShouldContain("where");
        sql.ShouldContain("fingerprint");
        sql.ShouldContain("jsonb");
    }

    // Buyuk finding degerlerinin 32 KB cikti tavaninda acik metadata ile kirpildigini kanitlar.
    [Fact]
    public async Task Findings_Should_Trim_To_Byte_Budget_And_Keep_Total_Count()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateRunGraphAsync(
            tenantId,
            DateTime.UtcNow,
            CreateFingerprintFindings(40, new string('x', 2048)));

        using (_currentTenant.Change(tenantId))
        {
            var page = await _appService.GetFindingsAsync(fixture.RunId, new GetContractCheckFindingsInput
            {
                MaxResultCount = 100
            });

            page.TotalCount.ShouldBe(40);
            page.IsTruncated.ShouldBeTrue();
            page.Items.Count.ShouldBeLessThan(40);
            page.ResponseBytes.ShouldBeLessThanOrEqualTo(ContractCheckRunConsts.DefaultFindingPageMaxBytes);
            JsonSerializer.SerializeToUtf8Bytes(page).Length
                .ShouldBeLessThanOrEqualTo(ContractCheckRunConsts.DefaultFindingPageMaxBytes);
        }
    }

    // Run, kaynak, dokuman ve iki snapshot'tan olusan tenant sahiplikli test grafigini tek UOW'de kurar.
    private async Task<(Guid RunId, Guid SourceId, Guid DocumentId, Guid BaseSnapshotId, Guid TargetSnapshotId)>
        CreateRunGraphAsync(
            Guid tenantId,
            DateTime creationTime,
            ContractCheckFindings findings,
            bool complete = true)
    {
        var result = default((Guid RunId, Guid SourceId, Guid DocumentId, Guid BaseSnapshotId, Guid TargetSnapshotId));
        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var source = new SpecSource(
                    Guid.NewGuid(),
                    $"source-{Guid.NewGuid():N}",
                    "https://contracts.test",
                    null,
                    tenantId);
                var document = _sourceManager.AddDocument(source, Guid.NewGuid(), "v1", "/openapi/v1.json");
                await _sourceRepository.InsertAsync(source, autoSave: true);

                var format = await _formatRepository.FirstAsync(value => value.Code == SpecFormatCodes.OpenApi31);
                var baseSnapshot = await CreateSnapshotAsync(tenantId, document.Id, format.Id, creationTime.AddMinutes(-2));
                var targetSnapshot = await CreateSnapshotAsync(tenantId, document.Id, format.Id, creationTime.AddMinutes(-1));
                var runId = await CreateRunAsync(
                    tenantId,
                    baseSnapshot.Id,
                    targetSnapshot.Id,
                    creationTime,
                    findings,
                    complete);

                result = (runId, source.Id, document.Id, baseSnapshot.Id, targetSnapshot.Id);
            }
        });

        return result;
    }

    // Verilen snapshot cifti icin pending veya tamamlanmis run kaydini kurup saklar.
    private async Task<Guid> CreateRunAsync(
        Guid tenantId,
        Guid baseSnapshotId,
        Guid targetSnapshotId,
        DateTime creationTime,
        ContractCheckFindings findings,
        bool complete = true)
    {
        var run = await _manager.CreateAsync(
            Guid.NewGuid(),
            baseSnapshotId,
            targetSnapshotId,
            tenantId);
        run.CreationTime = creationTime;

        if (complete)
        {
            var running = await _statusRepository.FirstAsync(status => status.Code == CheckRunStatusCodes.Running);
            var completed = await _statusRepository.FirstAsync(status => status.Code == CheckRunStatusCodes.Completed);
            _manager.Start(run, running.Id, creationTime.AddSeconds(1));
            _manager.Complete(run, completed.Id, creationTime.AddSeconds(2), findings);
        }

        await _runRepository.InsertAsync(run, autoSave: true);
        return run.Id;
    }

    // Tek dokuman icin benzersiz icerikli snapshot kaydini kurup saklar.
    private async Task<SpecSnapshot> CreateSnapshotAsync(
        Guid tenantId,
        Guid documentId,
        Guid formatId,
        DateTime seenAt)
    {
        var rawHash = $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
        var content = _ingestionManager.CreateContent(
            Guid.NewGuid(),
            rawHash,
            rawHash,
            "{}",
            2,
            "application/json",
            tenantId);
        await _contentRepository.InsertAsync(content, autoSave: true);

        var snapshot = _ingestionManager.CreateSnapshot(
            Guid.NewGuid(),
            documentId,
            content.Id,
            formatId,
            null,
            seenAt,
            tenantId);
        await _snapshotRepository.InsertAsync(snapshot, autoSave: true);
        return snapshot;
    }

    // Rapor kirilimlarini ve detay mapping'ini birlikte kanitlayan uc farkli severity bulgusu kurar.
    private static ContractCheckFindings CreateFindings()
    {
        return new ContractCheckFindings(
        [
            new Finding(
                DifferenceKindCodes.EndpointRemoved,
                DifferenceSeverityCodes.Breaking,
                DifferenceDirectionCodes.Endpoint,
                new FindingAddress(httpMethod: "GET", path: "/orders")),
            new Finding(
                DifferenceKindCodes.SchemaAdded,
                DifferenceSeverityCodes.NonBreaking,
                DifferenceDirectionCodes.Response,
                new FindingAddress(schemaName: "Order")),
            new Finding(
                DifferenceKindCodes.DescriptionChanged,
                DifferenceSeverityCodes.DocsOnly,
                DifferenceDirectionCodes.Documentation,
                new FindingAddress(schemaName: "Order", propertyPath: "id"))
        ]);
    }

    // Repository filtre, sayfa ve butce testleri icin kararli fingerprintli breaking bulgular kurar.
    private static ContractCheckFindings CreateFingerprintFindings(int count, string? retainedValue = null)
    {
        var findings = new List<Finding>(count);
        for (var index = 0; index < count; index++)
        {
            findings.Add(new Finding(
                DifferenceKindCodes.EndpointRemoved,
                DifferenceSeverityCodes.Breaking,
                DifferenceDirectionCodes.Endpoint,
                new FindingAddress(httpMethod: "GET", path: $"/orders/{index}"),
                retainedValue,
                null,
                Fingerprint(index)));
        }

        return new ContractCheckFindings(findings);
    }

    // Referans run'da bilinen bir fingerprint ile legacy null fingerprint'i birlikte kurar.
    private static ContractCheckFindings CreateReferenceFindings()
        => new([
            new Finding(
                DifferenceKindCodes.EndpointRemoved,
                DifferenceSeverityCodes.Breaking,
                DifferenceDirectionCodes.Endpoint,
                new FindingAddress(httpMethod: "GET", path: "/orders/0"),
                null,
                null,
                Fingerprint(0)),
            new Finding(
                DifferenceKindCodes.EndpointRemoved,
                DifferenceSeverityCodes.Breaking,
                DifferenceDirectionCodes.Endpoint,
                new FindingAddress(httpMethod: "GET", path: "/legacy"),
                null,
                null,
                null)
        ]);

    // Test fingerprintlerini validator'in kabul ettigi kararli 64 haneli uppercase hexadecimal sekle cevirir.
    private static string Fingerprint(int index) => index.ToString("X64");
}
