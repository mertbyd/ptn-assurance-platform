using Ptn.ApiContractChecker.Dtos.Snapshots;
using Ptn.ApiContractChecker.Dtos.Conformance;

namespace Ptn.ApiContractChecker.Services.Snapshots;

// islevi: Bir dokumanin snapshot gecmisi ve tekil anlik goruntu okuma sozlesmelerini tanimlar.
// sistemdeki gorevi: Snapshot yalniz cekim ile dogdugu icin yalniz salt-okunur yuzeyi acar; yazma veya silme sozlesmesi tasimaz.
public interface ISpecSnapshotAppService
    : IEntityReadAppService<SpecSnapshotDetailDto, SpecSnapshotHeaderDto, GetSpecSnapshotsInput>
{
    // Snapshot belgesindeki operasyon envanterini filtreli ve butceli sayfa olarak getirir.
    Task<SnapshotOperationInventoryDto> ListOperationsAsync(Guid snapshotId, ListSnapshotOperationsInput input);

    // Tek snapshot operasyonunun butceli yazarlik ozetini getirir.
    Task<OperationSummaryDto> FindOperationAsync(OperationSelectionDto input);

    // Tek snapshot component semasinin bir-seviye ozetini getirir.
    Task<SchemaDescriptionDto> DescribeSchemaAsync(DescribeSchemaDto input);

    // Kirpilmis cevabin tam ozetini resultRef ile yeniden calistirmadan getirir.
    Task<SnapshotAuthoringResultDto> GetAuthoringResultAsync(string resultRef);
}
