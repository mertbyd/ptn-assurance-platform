using System.Text;
using Ptn.ApiContractChecker.Constants.Formats.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Managers.Snapshots;
using Ptn.ApiContractChecker.Managers.Sources;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Runs;

// islevi: Contract-check senaryolarinin ihtiyac duydugu kaynak/dokuman/snapshot grafigini tek yerde kurar.
// sistemdeki gorevi: Execution ve bildirim testleri ayni fixture'i ikinci kez kurmasin diye ortak davranisi tabana tasir.
public abstract class ContractCheckRunTestBase : ApiContractCheckerEntityFrameworkCoreTestBase
{
    // Tek endpoint'li referans spec; hedefte silindiginde kirici bulgu uretir.
    protected const string BaseSpec = """
                                      {"openapi":"3.0.1","info":{"title":"Contracts","version":"1"},"paths":{"/orders":{"get":{"operationId":"GetOrders","responses":{"200":{"description":"ok"}}}}}}
                                      """;

    // Endpoint'i kaldirilmis aday spec.
    protected const string TargetSpec = """
                                        {"openapi":"3.0.1","info":{"title":"Contracts","version":"2"},"paths":{}}
                                        """;

    protected readonly ISpecSourceRepository SourceRepository;
    protected readonly IRepository<SpecContent, Guid> ContentRepository;
    protected readonly IRepository<SpecSnapshot, Guid> SnapshotRepository;
    protected readonly IRepository<SpecFormat, Guid> FormatRepository;
    protected readonly ICurrentTenant CurrentTenant;
    protected readonly SpecIngestionManager IngestionManager;
    protected readonly SpecSourceManager SourceManager;

    protected ContractCheckRunTestBase()
    {
        SourceRepository = GetRequiredService<ISpecSourceRepository>();
        ContentRepository = GetRequiredService<IRepository<SpecContent, Guid>>();
        SnapshotRepository = GetRequiredService<IRepository<SpecSnapshot, Guid>>();
        FormatRepository = GetRequiredService<IRepository<SpecFormat, Guid>>();
        CurrentTenant = GetRequiredService<ICurrentTenant>();
        IngestionManager = GetRequiredService<SpecIngestionManager>();
        SourceManager = GetRequiredService<SpecSourceManager>();
    }

    // Kaynak, dokuman ve iki degismez snapshot icerigini tek tenant grafiginde kurar.
    protected async Task<ContractCheckRunFixture> CreateSnapshotGraphAsync(
        Guid tenantId,
        string baseContent,
        string targetContent)
    {
        using (CurrentTenant.Change(tenantId))
        {
            return await WithUnitOfWorkAsync(async () =>
            {
                var source = new SpecSource(
                    Guid.NewGuid(),
                    $"source-{Guid.NewGuid():N}",
                    "https://contracts.test",
                    null,
                    tenantId);
                var document = SourceManager.AddDocument(source, Guid.NewGuid(), "v1", "/openapi.json");
                await SourceRepository.InsertAsync(source, autoSave: true);
                var format = await FormatRepository.FirstAsync(item => item.Code == SpecFormatCodes.OpenApi30);
                var baseSnapshot = await InsertSnapshotAsync(tenantId, document.Id, format.Id, baseContent);
                var targetSnapshot = await InsertSnapshotAsync(tenantId, document.Id, format.Id, targetContent);
                return new ContractCheckRunFixture(
                    source.Id,
                    source.Name,
                    document.DocumentName,
                    baseSnapshot.Id,
                    targetSnapshot.Id);
            });
        }
    }

    // Tek ham spec metnini benzersiz content ve snapshot satirlarina test verisi olarak yazar.
    private async Task<SpecSnapshot> InsertSnapshotAsync(
        Guid tenantId,
        Guid documentId,
        Guid formatId,
        string contentText)
    {
        var hash = $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
        var content = IngestionManager.CreateContent(
            Guid.NewGuid(),
            hash,
            hash,
            contentText,
            Encoding.UTF8.GetByteCount(contentText),
            "application/json",
            tenantId);
        await ContentRepository.InsertAsync(content, autoSave: true);
        var snapshot = IngestionManager.CreateSnapshot(
            Guid.NewGuid(),
            documentId,
            content.Id,
            formatId,
            null,
            DateTime.UtcNow,
            tenantId);
        return await SnapshotRepository.InsertAsync(snapshot, autoSave: true);
    }
}
