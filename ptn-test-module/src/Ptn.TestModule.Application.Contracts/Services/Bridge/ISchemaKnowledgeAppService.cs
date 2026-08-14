using Ptn.TestModule.Dtos.Bridge.Database;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Tablo tanimi, sema snapshot'i ve kanonik fingerprint kullanim senaryolarini tanimlar.
// sistemdeki gorevi: Database checker sema DTO'larini public Test Module sozlesmesinden gizler.
public interface ISchemaKnowledgeAppService : IApplicationService
{
    // Tek tablonun provider-bagimsiz kolon ve anahtar tanimini getirir.
    Task<TableDescriptionDto> DescribeTableAsync(TableQueryDto input, CancellationToken cancellationToken);

    // Baglantinin kanonik sema snapshot'ini getirir.
    Task<SchemaSnapshotDto> GetSnapshotAsync(Guid connectionId, CancellationToken cancellationToken);

    // Baglantinin kanonik sema fingerprint'ini getirir.
    Task<string> GetSchemaFingerprintAsync(Guid connectionId, CancellationToken cancellationToken);
}
