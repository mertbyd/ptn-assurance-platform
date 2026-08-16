using System.Security.Claims;
using System.Text.RegularExpressions;
using Ptn.ApiContractChecker.Constants.Formats.Lookups;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Dtos.Snapshots;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.ExceptionCodes.Sources;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Managers.Snapshots;
using Ptn.ApiContractChecker.Managers.Sources;
using Ptn.ApiContractChecker.Services.Snapshots;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Snapshots;

// islevi: Snapshot gecmisi okuma yuzeyinin siralama, sayfalama, projeksiyon, detay grafigi ve tenant izolasyonunu gercek EF saglayicisiyla dogrular.
// sistemdeki gorevi: Liste yolunun ham spec govdesine dokunmasini ve gecmisin tenant ya da dokuman sinirini asmasini kalici olarak engeller.
[Collection(EfCoreIntegrationCollection.Name)]
public class SpecSnapshotHistory_Tests : ApiContractCheckerEntityFrameworkCoreTestBase
{
    private readonly ISpecSnapshotAppService _appService;
    private readonly SpecIngestionManager _ingestionManager;
    private readonly SpecSourceManager _sourceManager;
    private readonly ISpecSnapshotRepository _snapshotRepository;
    private readonly ISpecSourceRepository _sourceRepository;
    private readonly IRepository<SpecContent, Guid> _contentRepository;
    private readonly IRepository<SpecFormat, Guid> _formatRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;
    private readonly SqlCommandCaptureInterceptor _commandCapture;

    public SpecSnapshotHistory_Tests()
    {
        _appService = GetRequiredService<ISpecSnapshotAppService>();
        _ingestionManager = GetRequiredService<SpecIngestionManager>();
        _sourceManager = GetRequiredService<SpecSourceManager>();
        _snapshotRepository = GetRequiredService<ISpecSnapshotRepository>();
        _sourceRepository = GetRequiredService<ISpecSourceRepository>();
        _contentRepository = GetRequiredService<IRepository<SpecContent, Guid>>();
        _formatRepository = GetRequiredService<IRepository<SpecFormat, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
        _commandCapture = GetRequiredService<SqlCommandCaptureInterceptor>();
    }

    // Gecmisin en yeni snapshot'tan basladigini ve ABP sayfalamasinin bu sirayi bozmadigini kanitlar.
    [Fact]
    public async Task List_Should_Page_And_Order_By_CreationTime_Descending()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateDocumentAsync(tenantId);
        var oldest = await CreateSnapshotAsync(fixture, DateTime.UtcNow.AddHours(-3));
        var middle = await CreateSnapshotAsync(fixture, DateTime.UtcNow.AddHours(-2));
        var newest = await CreateSnapshotAsync(fixture, DateTime.UtcNow.AddHours(-1));

        using (_currentTenant.Change(tenantId))
        {
            var firstPage = await _appService.GetListAsync(BuildInput(fixture, maxResultCount: 2));
            var secondPage = await _appService.GetListAsync(BuildInput(fixture, maxResultCount: 2, skipCount: 2));

            firstPage.TotalCount.ShouldBe(3);
            firstPage.Items.Select(item => item.Id).ShouldBe([newest.Id, middle.Id]);
            secondPage.Items.Single().Id.ShouldBe(oldest.Id);
        }
    }

    // Liste SQL'inin ham icerik kolonunu hic secmedigini, detay yolunun ise onu bilerek cektigini kanitlar.
    [Fact]
    public async Task List_Should_Not_Read_The_Raw_Spec_Content()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateDocumentAsync(tenantId);
        var snapshot = await CreateSnapshotAsync(fixture, DateTime.UtcNow);

        using (_currentTenant.Change(tenantId))
        {
            _commandCapture.Clear();
            var list = await _appService.GetListAsync(BuildInput(fixture));

            list.Items.ShouldContain(item => item.Id == snapshot.Id);
            SelectsRawContentColumn(_commandCapture.Commands).ShouldBeFalse();

            _commandCapture.Clear();
            await _appService.GetAsync(snapshot.Id);

            SelectsRawContentColumn(_commandCapture.Commands).ShouldBeTrue();
        }
    }

    // Liste satirinin karsilastirma ekranina gereken olculeri, detayin ise tam icerik/format grafigini verdigini kanitlar.
    [Fact]
    public async Task Detail_Should_Return_The_Content_And_Format_Graph()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateDocumentAsync(tenantId);
        var snapshot = await CreateSnapshotAsync(fixture, DateTime.UtcNow);
        var content = await GetContentAsync(fixture, snapshot.SpecContentId);

        using (_currentTenant.Change(tenantId))
        {
            var header = (await _appService.GetListAsync(BuildInput(fixture))).Items.Single();
            var detail = await _appService.GetAsync(snapshot.Id);

            header.FormatCode.ShouldBe(SpecFormatCodes.OpenApi31);
            header.ByteSize.ShouldBe(content.ByteSize);
            header.ShortCanonicalHash
                .ShouldBe(content.CanonicalHash[..SpecSnapshotConsts.ShortHashLength]);

            detail.SpecDocumentId.ShouldBe(fixture.DocumentId);
            detail.SpecFormat.Code.ShouldBe(SpecFormatCodes.OpenApi31);
            detail.SpecContent.Content.ShouldBe(content.Content);
            detail.SpecContent.RawHash.ShouldBe(content.RawHash);
            detail.SpecContent.MediaType.ShouldBe(content.MediaType);
        }
    }

    // Baska bir tenant'in snapshot'ina ne detay ne de gecmis listesi uzerinden erisilebildigini kanitlar.
    [Fact]
    public async Task Other_Tenant_Snapshot_Should_Not_Be_Visible()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var fixture = await CreateDocumentAsync(tenantA);
        var snapshot = await CreateSnapshotAsync(fixture, DateTime.UtcNow);

        using (_currentTenant.Change(tenantB))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() => _appService.GetAsync(snapshot.Id));

            var exception = await Should.ThrowAsync<BusinessException>(
                () => _appService.GetListAsync(BuildInput(fixture)));

            exception.Code.ShouldBe(SpecSourceExceptionCodes.ActiveDocumentNotFound);
        }
    }

    // Host kullanicisinin kendi snapshot'ini okuyup baska bir host kullanicisinin ham govdesine erisememesini kanitlar.
    [Fact]
    public async Task Host_User_Should_Not_See_Other_User_Snapshot()
    {
        var ownerUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        SpecSnapshot ownerSnapshot;
        SpecSnapshot otherSnapshot;

        using (_currentPrincipalAccessor.Change(BuildPrincipal(ownerUserId)))
        {
            var fixture = await CreateDocumentAsync(null);
            ownerSnapshot = await CreateSnapshotAsync(fixture, DateTime.UtcNow.AddMinutes(-1));
        }

        using (_currentPrincipalAccessor.Change(BuildPrincipal(otherUserId)))
        {
            var fixture = await CreateDocumentAsync(null);
            otherSnapshot = await CreateSnapshotAsync(fixture, DateTime.UtcNow);
        }

        using (_currentTenant.Change(null))
        using (_currentPrincipalAccessor.Change(BuildPrincipal(ownerUserId)))
        {
            var detail = await _appService.GetAsync(ownerSnapshot.Id);
            detail.Id.ShouldBe(ownerSnapshot.Id);

            await Should.ThrowAsync<EntityNotFoundException>(() => _appService.GetAsync(otherSnapshot.Id));
        }
    }

    // Kaynaga ait olmayan bir dokuman kimliginin gecmis dondurmedigini, kararli alan hatasi verdigini kanitlar.
    [Fact]
    public async Task Unknown_Document_Should_Be_NotFound()
    {
        var tenantId = Guid.NewGuid();
        var fixture = await CreateDocumentAsync(tenantId);

        using (_currentTenant.Change(tenantId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(
                () => _appService.GetListAsync(new GetSpecSnapshotsInput
                {
                    SpecSourceId = fixture.SourceId,
                    SpecDocumentId = Guid.NewGuid()
                }));

            exception.Code.ShouldBe(SpecSourceExceptionCodes.ActiveDocumentNotFound);
        }
    }

    // Gecmisi okunacak kaynagi tek dokumaniyla tenant sahipligi altinda kurar.
    private async Task<DocumentFixture> CreateDocumentAsync(Guid? tenantId)
    {
        var sourceId = Guid.NewGuid();
        var documentId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var source = new SpecSource(sourceId, $"source-{Guid.NewGuid():N}", "https://orders.test", null, tenantId);
                _sourceManager.AddDocument(source, documentId, "v1", "/openapi/v1.json");
                await _sourceRepository.InsertAsync(source, autoSave: true);
            }
        });

        return new DocumentFixture(tenantId, sourceId, documentId);
    }

    // Dokuman icin benzersiz icerikli ve verilen zamanda acilmis tek snapshot satiri kurar.
    private async Task<SpecSnapshot> CreateSnapshotAsync(DocumentFixture fixture, DateTime creationTime)
    {
        var snapshot = default(SpecSnapshot);

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(fixture.TenantId))
            {
                var hash = $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
                var body = $"{{\"openapi\":\"3.1.0\",\"x-id\":\"{Guid.NewGuid():N}\"}}";
                var content = _ingestionManager.CreateContent(
                    Guid.NewGuid(),
                    hash,
                    hash,
                    body,
                    body.Length,
                    "application/json",
                    fixture.TenantId);
                await _contentRepository.InsertAsync(content, autoSave: true);

                var format = await _formatRepository.FirstAsync(value => value.Code == SpecFormatCodes.OpenApi31);
                snapshot = _ingestionManager.CreateSnapshot(
                    Guid.NewGuid(),
                    fixture.DocumentId,
                    content.Id,
                    format.Id,
                    "1.0.0",
                    creationTime,
                    fixture.TenantId);
                snapshot.CreationTime = creationTime;
                await _snapshotRepository.InsertAsync(snapshot, autoSave: true);
            }
        });

        return snapshot!;
    }

    // Iddialarin karsilastiracagi kalici icerik satirini sahibi tenant baglaminda okur.
    private async Task<SpecContent> GetContentAsync(DocumentFixture fixture, Guid contentId)
    {
        using (_currentTenant.Change(fixture.TenantId))
        {
            return await _contentRepository.GetAsync(contentId);
        }
    }

    // Gecmis sorgusunu rotadan gelen kaynak/dokuman kapsamiyla kurar.
    private static GetSpecSnapshotsInput BuildInput(
        DocumentFixture fixture,
        int maxResultCount = 10,
        int skipCount = 0)
    {
        return new GetSpecSnapshotsInput
        {
            SpecSourceId = fixture.SourceId,
            SpecDocumentId = fixture.DocumentId,
            MaxResultCount = maxResultCount,
            SkipCount = skipCount
        };
    }

    // Yakalanan SQL metinlerinde ham spec govdesi kolonunun secilip secilmedigini kelime siniriyla belirler.
    // Kelime siniri "spec_contents" tablosunu ve "spec_content_id" kolonunu yanlislikla eslestirmez.
    private static bool SelectsRawContentColumn(IReadOnlyCollection<string> commands)
    {
        var pattern = $@"\b{SpecContentConsts.ContentColumnName}\b";
        return commands.Any(command => Regex.IsMatch(command, pattern, RegexOptions.IgnoreCase));
    }

    // Host gorunurluk senaryosu icin yalniz kararli kullanici kimligini tasiyan principal kurar.
    private static ClaimsPrincipal BuildPrincipal(Guid userId)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(AbpClaimTypes.UserId, userId.ToString())],
            nameof(SpecSnapshotHistory_Tests)));
    }

    // Testlerin kurdugu kaynak-dokuman grafigini sahibi tenant'la birlikte tasir.
    private sealed record DocumentFixture(Guid? TenantId, Guid SourceId, Guid DocumentId);
}
