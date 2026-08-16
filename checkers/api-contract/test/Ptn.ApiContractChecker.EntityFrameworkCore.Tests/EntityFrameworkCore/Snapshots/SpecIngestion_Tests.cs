using System.Text;
using Ptn.ApiContractChecker.Constants.Formats.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.ExceptionCodes;
using Ptn.ApiContractChecker.ExceptionCodes.Snapshots;
using Ptn.ApiContractChecker.Interface;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Managers.Snapshots;
using Ptn.ApiContractChecker.Managers.Sources;
using Ptn.ApiContractChecker.Models.Sources;
using Shouldly;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Xunit;
using Ptn.ApiContractChecker.EntityFrameworkCore.Adapters.Snapshots;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Snapshots;

// islevi: Cekim karar akisinin icerik tekilligi, bicim degisikligi ve bozuk spec davranisini gercek EF saglayicisiyla dogrular.
// sistemdeki gorevi: Ayni icerigin ikinci kez satir acmasini ve gecersiz spec'in snapshot birakmasini engelleyen regresyon kapisidir.
[Collection(EfCoreIntegrationCollection.Name)]
public class SpecIngestion_Tests : ApiContractCheckerEntityFrameworkCoreTestBase
{
    private const string OpenApi31Body =
        "{\"openapi\":\"3.1.0\",\"info\":{\"title\":\"orders\",\"version\":\"1.4.0\"}," +
        "\"paths\":{\"/orders\":{\"get\":{\"responses\":{\"200\":{\"description\":\"ok\"}}}}}}";

    private const string OpenApi30Body =
        "{\"openapi\":\"3.0.3\",\"info\":{\"title\":\"orders\",\"version\":\"1.4.0\"}," +
        "\"paths\":{\"/orders\":{\"get\":{\"responses\":{\"200\":{\"description\":\"ok\"}}}}}}";

    private const string Swagger20Body =
        "{\"swagger\":\"2.0\",\"info\":{\"title\":\"orders\",\"version\":\"1.4.0\"}," +
        "\"paths\":{\"/orders\":{\"get\":{\"responses\":{\"200\":{\"description\":\"ok\"}}}}}}";

    // Canli ptn-payment-auth-api-dev dokumaninda olculen iki gercek ihlali tasir:
    // ayni imzaya cozulen iki path ve component key regex'ine uymayan ABP generic sema adi.
    private const string UpstreamQualityViolationBody = """
        {"openapi":"3.0.1","info":{"title":"payment-auth","version":"1.0"},
         "paths":{
           "/api/notification-template/{id}":{"get":{"responses":{"200":{"description":"ok"}}}},
           "/api/notification-template/{notificationTemplateId}":{"delete":{"responses":{"200":{"description":"ok"}}}}},
         "components":{"schemas":{
           "Volo.Abp.Application.Dtos.PagedResultDto`1[[Ptn.Authenticator.Dtos.Users.PtnUserDto, Ptn.Authenticator.Application.Contracts, Version=10.0.0.0, Culture=neutral, PublicKeyToken=null]]":
             {"type":"object","properties":{"totalCount":{"type":"integer"}}}}}}
        """;

    // Okuyucu dokumani uretir ama /orders yolunu cozemez: bos ruleset'e ragmen
    // Diagnostic.Errors dolar. Kismi cozulen spec yanlis fark uretecegi icin reddedilir.
    private const string PartiallyResolvedBody =
        "{\"openapi\":\"3.1.0\",\"info\":{\"title\":\"orders\",\"version\":\"1.4.0\"}," +
        "\"paths\":{\"/orders\":\"not-an-object\"}}";

    private readonly SpecIngestionManager _manager;
    private readonly SpecSourceManager _sourceManager;
    private readonly ISpecSourceRepository _sourceRepository;
    private readonly ISpecContentRepository _contentRepository;
    private readonly ISpecSnapshotRepository _snapshotRepository;
    private readonly IBaseRepository<SpecFormat> _formatRepository;
    private readonly ICurrentTenant _currentTenant;

    public SpecIngestion_Tests()
    {
        _manager = GetRequiredService<SpecIngestionManager>();
        _sourceManager = GetRequiredService<SpecSourceManager>();
        _sourceRepository = GetRequiredService<ISpecSourceRepository>();
        _contentRepository = GetRequiredService<ISpecContentRepository>();
        _snapshotRepository = GetRequiredService<ISpecSnapshotRepository>();
        _formatRepository = GetRequiredService<IBaseRepository<SpecFormat>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    // Ayni ham icerigin ikinci cekiminde yeni icerik ve yeni snapshot satiri acilmadigini kanitlar.
    [Fact]
    public async Task Same_Raw_Content_Should_Not_Open_A_Second_Snapshot()
    {
        var tenantId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var documentId = await CreateDocumentAsync("orders");

                var first = await _manager.IngestAsync(documentId, BuildFetchResult(OpenApi31Body));
                var second = await _manager.IngestAsync(documentId, BuildFetchResult(OpenApi31Body));

                second.Id.ShouldBe(first.Id);
                second.LastSeenAt.ShouldBeGreaterThanOrEqualTo(first.LastSeenAt);
                (await _contentRepository.GetCountAsync()).ShouldBe(1);
                (await _snapshotRepository.GetCountAsync()).ShouldBe(1);
            }
        });
    }

    // Yalniz bicimi degisen govdenin yeni snapshot actigini ama kanonik hash'in ayni kaldigini kanitlar.
    [Fact]
    public async Task Formatting_Only_Change_Should_Open_A_Snapshot_With_The_Same_Canonical_Hash()
    {
        var tenantId = Guid.NewGuid();
        var reformattedBody = OpenApi31Body
            .Replace(",", ",\n  ")
            .Replace("{", "{\n  ");

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var documentId = await CreateDocumentAsync("orders");

                var first = await _manager.IngestAsync(documentId, BuildFetchResult(OpenApi31Body));
                var second = await _manager.IngestAsync(documentId, BuildFetchResult(reformattedBody));

                second.Id.ShouldNotBe(first.Id);
                var firstContent = await _contentRepository.GetAsync(first.SpecContentId);
                var secondContent = await _contentRepository.GetAsync(second.SpecContentId);
                secondContent.RawHash.ShouldNotBe(firstContent.RawHash);
                secondContent.CanonicalHash.ShouldBe(firstContent.CanonicalHash);
            }
        });
    }

    // Gercekten degisen icerigin hem ham hem kanonik hash'i degistirdigini kanitlar.
    [Fact]
    public async Task Changed_Content_Should_Change_Both_Hashes()
    {
        var tenantId = Guid.NewGuid();
        var changedBody = OpenApi31Body.Replace("1.4.0", "1.5.0");

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var documentId = await CreateDocumentAsync("orders");

                var first = await _manager.IngestAsync(documentId, BuildFetchResult(OpenApi31Body));
                var second = await _manager.IngestAsync(documentId, BuildFetchResult(changedBody));

                var firstContent = await _contentRepository.GetAsync(first.SpecContentId);
                var secondContent = await _contentRepository.GetAsync(second.SpecContentId);
                secondContent.RawHash.ShouldNotBe(firstContent.RawHash);
                secondContent.CanonicalHash.ShouldNotBe(firstContent.CanonicalHash);
                second.ApiVersion.ShouldBe("1.5.0");
            }
        });
    }

    // Uc spec surumunun de dogru SpecFormat lookup satirina baglandigini kanitlar.
    [Theory]
    [InlineData(Swagger20Body, SpecFormatCodes.Swagger20)]
    [InlineData(OpenApi30Body, SpecFormatCodes.OpenApi30)]
    [InlineData(OpenApi31Body, SpecFormatCodes.OpenApi31)]
    public async Task Snapshot_Should_Bind_The_Detected_Spec_Format(string body, string expectedCode)
    {
        var tenantId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var documentId = await CreateDocumentAsync("orders");

                var snapshot = await _manager.IngestAsync(documentId, BuildFetchResult(body));

                var format = await _formatRepository.GetAsync(snapshot.SpecFormatId);
                format.Code.ShouldBe(expectedCode);
            }
        });
    }

    // Okunamayan govdenin reddedildigini ve hicbir satir birakmadigini kanitlar.
    [Fact]
    public async Task Unreadable_Document_Should_Be_Rejected_Without_Writing_Rows()
    {
        var tenantId = Guid.NewGuid();
        const string unreadableBody = "{\"hello\":\"world\"}";

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var documentId = await CreateDocumentAsync("orders");

                var exception = await Should.ThrowAsync<BusinessException>(
                    () => _manager.IngestAsync(documentId, BuildFetchResult(unreadableBody)));

                exception.Code.ShouldBe(SpecSnapshotExceptionCodes.ParseFailed);
                (await _contentRepository.GetCountAsync()).ShouldBe(0);
                (await _snapshotRepository.GetCountAsync()).ShouldBe(0);
            }
        });
    }

    // Bos ruleset'in okuyucu guard'ini bozmadigini kanitlar: dokuman uretilse de
    // eksik cozulmusse DocumentInvalid ile reddedilir ve hicbir satir yazilmaz.
    [Fact]
    public async Task Partially_Resolved_Document_Should_Be_Rejected_Without_Writing_Rows()
    {
        var tenantId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var documentId = await CreateDocumentAsync("orders");

                var exception = await Should.ThrowAsync<BusinessException>(
                    () => _manager.IngestAsync(documentId, BuildFetchResult(PartiallyResolvedBody)));

                exception.Code.ShouldBe(SpecSnapshotExceptionCodes.DocumentInvalid);
                (await _contentRepository.GetCountAsync()).ShouldBe(0);
                (await _snapshotRepository.GetCountAsync()).ShouldBe(0);
            }
        });
    }

    // Spec surumu taninmayan govdenin format hatasiyla reddedildigini kanitlar.
    [Fact]
    public async Task Unrecognized_Body_Should_Be_Rejected_As_Unsupported_Format()
    {
        var tenantId = Guid.NewGuid();
        const string htmlBody = "<!DOCTYPE html><html><body><h1>502 Bad Gateway</h1></body></html>";

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var documentId = await CreateDocumentAsync("orders");

                var exception = await Should.ThrowAsync<BusinessException>(
                    () => _manager.IngestAsync(documentId, BuildFetchResult(htmlBody)));

                exception.Code.ShouldBe(SpecFormatExceptionCodes.UnsupportedFormat);
                (await _snapshotRepository.GetCountAsync()).ShouldBe(0);
            }
        });
    }

    // Izlenen servisin kendi spec kalite ihlallerinin snapshot almayi engellemedigini kanitlar.
    [Fact]
    public async Task Upstream_Spec_Quality_Violations_Should_Not_Block_Ingestion()
    {
        var tenantId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var documentId = await CreateDocumentAsync("payment-auth");

                var snapshot = await _manager.IngestAsync(
                    documentId,
                    BuildFetchResult(UpstreamQualityViolationBody));

                snapshot.ShouldNotBeNull();
                (await _snapshotRepository.GetCountAsync()).ShouldBe(1);
            }
        });
    }

    // Snapshot'in bagli olacagi dokumani kaynak aggregate'i uzerinden kurar.
    private async Task<Guid> CreateDocumentAsync(string name)
    {
        var source = new SpecSource(Guid.NewGuid(), name, "https://orders.test", null, _currentTenant.Id);
        var document = _sourceManager.AddDocument(source, Guid.NewGuid(), "v1", "/openapi/v1.json");
        await _sourceRepository.InsertAsync(source, autoSave: true);
        return document.Id;
    }

    // Cekim hattinin urettigi sonucu test govdesinden kurar.
    private static SpecFetchResultModel BuildFetchResult(string body)
    {
        return new SpecFetchResultModel(Encoding.UTF8.GetBytes(body), "application/json");
    }
}
