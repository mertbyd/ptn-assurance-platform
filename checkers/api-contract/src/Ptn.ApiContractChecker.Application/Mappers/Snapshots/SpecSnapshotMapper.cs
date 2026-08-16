using Ptn.ApiContractChecker.Dtos.Snapshots;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Models.Snapshots;
using Riok.Mapperly.Abstractions;
using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.Models.Conformance;

namespace Ptn.ApiContractChecker.Application.Mappers.Snapshots;

// islevi: SpecSnapshot cekim yaniti, gecmis satiri ve navigation'li detay grafiginin DTO donusumlerini uretir.
// sistemdeki gorevi: Snapshot yuzeyindeki tum katmanlar arasi alan tasimanin Mapperly sahibidir.
[Mapper]
public partial class SpecSnapshotMapper
{
    // Snapshot entity'sini secret tasimayan cekim yanitina cevirir.
    public partial SpecSnapshotDto MapToDto(SpecSnapshot snapshot);

    // Hafif gecmis satirini liste DTO'suna tasir.
    public partial SpecSnapshotHeaderDto MapToHeaderDto(SpecSnapshotHeaderModel model);

    // Hafif gecmis satirlarini liste DTO'larina tasir.
    public partial List<SpecSnapshotHeaderDto> MapToHeaderDto(List<SpecSnapshotHeaderModel> models);

    // Include ile yuklenen snapshot grafigini icerik ve format alt nesneleriyle detay DTO'suna tasir.
    public partial SpecSnapshotDetailDto MapToDetailDto(SpecSnapshot snapshot);

    // Degismez icerigi detayin alt nesnesine tasir; kimlik ust nesnede SpecContentId olarak zaten vardir,
    // tenant ve audit kullanicisi ise sinir disina cikmaz.
    private partial SpecContentDto MapToContentDto(SpecContent content);

    // Envanter filtre ve sayfa penceresini domain istegine tasir.
    public partial SnapshotOperationInventoryRequest MapToInventoryRequest(ListSnapshotOperationsInput input);

    // Butcelenmis domain operasyon envanterini ABP sayfa sozlesmesine tasir.
    public partial SnapshotOperationInventoryDto MapToInventoryDto(SnapshotOperationInventoryResult result);

    // Operasyon yazarlik ozetini HTTP DTO'suna tasir.
    public partial OperationSummaryDto MapToDto(OperationSummaryResult result);

    // Sema yazarlik ozetini HTTP DTO'suna tasir.
    public partial SchemaDescriptionDto MapToDto(SchemaDescriptionResult result);

    // ResultRef ile bulunan tam yazarlik ozetini HTTP DTO'suna tasir.
    public partial SnapshotAuthoringResultDto MapToDto(SnapshotAuthoringResultEnvelope result);

    [MapperIgnoreSource(nameof(OperationSelectionDto.SnapshotId))]
    [MapperIgnoreSource(nameof(OperationSelectionDto.VerbosityCode))]
    public partial OperationSelectionRequest MapToSelection(OperationSelectionDto input);
}
