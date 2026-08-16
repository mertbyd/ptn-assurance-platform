using System.Net;
using System.Text;
using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.EntityFrameworkCore.Sources;
using Ptn.ApiContractChecker.ExceptionCodes.Sources;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Managers.Sources;
using Ptn.ApiContractChecker.Services.Sources;
using Shouldly;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Xunit;
using Ptn.ApiContractChecker.EntityFrameworkCore.Adapters.Snapshots;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Snapshots;

// islevi: Snapshot alma endpoint'inin cekim, ingest karari ve dokuman guard'ini gercek EF saglayicisiyla dogrular.
// sistemdeki gorevi: KBP-617'de acilan tetikleme yuzeyinin "ayni icerik yeni satir acmaz" sozlesmesini ve pasif dokuman reddini sabitler.
[Collection(EfCoreIntegrationCollection.Name)]
public class SpecSourceSnapshotCapture_Tests : ApiContractCheckerEntityFrameworkCoreTestBase
{
    // Test modulunun 64 baytlik cekim sinirinin altinda kalan gecerli OAS 3.1 dokumani.
    private const string CompactSpec = "{\"openapi\":\"3.1.0\",\"info\":{\"title\":\"o\",\"version\":\"1\"}}";

    // Ayni dokumanin yalniz surumu degismis ikinci hali; yine sinir altinda kalir.
    private const string ChangedSpec = "{\"openapi\":\"3.1.0\",\"info\":{\"title\":\"o\",\"version\":\"2\"}}";

    private readonly ISpecSourceAppService _appService;
    private readonly SpecSourceManager _sourceManager;
    private readonly ISpecSourceRepository _sourceRepository;
    private readonly ISpecSnapshotRepository _snapshotRepository;
    private readonly SpecSourceHttpClientFactory _httpClientFactory;
    private readonly ICurrentTenant _currentTenant;

    public SpecSourceSnapshotCapture_Tests()
    {
        _appService = GetRequiredService<ISpecSourceAppService>();
        _sourceManager = GetRequiredService<SpecSourceManager>();
        _sourceRepository = GetRequiredService<ISpecSourceRepository>();
        _snapshotRepository = GetRequiredService<ISpecSnapshotRepository>();
        _httpClientFactory = (SpecSourceHttpClientFactory)GetRequiredService<IHttpClientFactory>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    // Endpoint'in canli govdeyi cekip contract check'e verilebilir bir snapshot kimligi dondurdugunu kanitlar.
    [Fact]
    public async Task Capture_Should_Return_A_Persisted_Snapshot_Id()
    {
        var tenantId = Guid.NewGuid();
        var (sourceId, documentId) = await CreateSourceAsync(tenantId);
        ConfigureSpecResponse(CompactSpec);

        using (_currentTenant.Change(tenantId))
        {
            var dto = await _appService.CaptureSnapshotAsync(sourceId, documentId);

            dto.Id.ShouldNotBe(Guid.Empty);
            dto.SpecDocumentId.ShouldBe(documentId);
            dto.SpecFormatId.ShouldNotBe(Guid.Empty);
            dto.ApiVersion.ShouldBe("1");
            (await _snapshotRepository.FindAsync(dto.Id)).ShouldNotBeNull();
        }
    }

    // Degismeyen icerigin ikinci cekimde yeni snapshot satiri acmadigini, degisen icerigin actigini kanitlar.
    [Fact]
    public async Task Capture_Should_Open_A_New_Snapshot_Only_When_The_Content_Changes()
    {
        var tenantId = Guid.NewGuid();
        var (sourceId, documentId) = await CreateSourceAsync(tenantId);

        using (_currentTenant.Change(tenantId))
        {
            ConfigureSpecResponse(CompactSpec);
            var first = await _appService.CaptureSnapshotAsync(sourceId, documentId);

            ConfigureSpecResponse(CompactSpec);
            var unchanged = await _appService.CaptureSnapshotAsync(sourceId, documentId);

            ConfigureSpecResponse(ChangedSpec);
            var changed = await _appService.CaptureSnapshotAsync(sourceId, documentId);

            unchanged.Id.ShouldBe(first.Id);
            unchanged.LastSeenAt.ShouldBeGreaterThanOrEqualTo(first.LastSeenAt);
            changed.Id.ShouldNotBe(first.Id);
            changed.ApiVersion.ShouldBe("2");
        }
    }

    // Pasif dokumanin snapshot alinamadigini ve kararli alan hatasi verdigini kanitlar.
    [Fact]
    public async Task Capture_Should_Reject_A_Passivated_Document()
    {
        var tenantId = Guid.NewGuid();
        var (sourceId, documentId) = await CreateSourceAsync(tenantId, documentIsActive: false);
        ConfigureSpecResponse(CompactSpec);

        using (_currentTenant.Change(tenantId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(
                () => _appService.CaptureSnapshotAsync(sourceId, documentId));

            exception.Code.ShouldBe(SpecSourceExceptionCodes.ActiveDocumentNotFound);
        }
    }

    // Kaynaga ait olmayan dokuman kimliginin snapshot alamadigini kanitlar.
    [Fact]
    public async Task Capture_Should_Reject_A_Document_Of_Another_Source()
    {
        var tenantId = Guid.NewGuid();
        var (sourceId, _) = await CreateSourceAsync(tenantId);
        ConfigureSpecResponse(CompactSpec);

        using (_currentTenant.Change(tenantId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(
                () => _appService.CaptureSnapshotAsync(sourceId, Guid.NewGuid()));

            exception.Code.ShouldBe(SpecSourceExceptionCodes.ActiveDocumentNotFound);
        }
    }

    // Snapshot alinacak kaynagi tek dokumaniyla tenant sahipligi altinda kurar.
    private async Task<(Guid SourceId, Guid DocumentId)> CreateSourceAsync(
        Guid tenantId,
        bool documentIsActive = true)
    {
        var sourceId = Guid.NewGuid();
        var documentId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                var source = new SpecSource(sourceId, "orders", "https://orders.test", null, tenantId);
                _sourceManager.AddDocument(source, documentId, "v1", "/openapi/v1.json", documentIsActive);
                await _sourceRepository.InsertAsync(source, autoSave: true);
            }
        });

        return (sourceId, documentId);
    }

    // Cekim adapterine verilen govdeyi basarili JSON yaniti olarak yapilandirir.
    private void ConfigureSpecResponse(string body)
    {
        _httpClientFactory.ConfigureResponse(
            HttpStatusCode.OK,
            Encoding.UTF8.GetBytes(body),
            "application/json");
    }
}
