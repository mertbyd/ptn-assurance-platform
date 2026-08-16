namespace Ptn.ApiContractChecker.EntityFrameworkCore.Runs;

// islevi: Kurulan test grafiginin kimliklerini ve bildirim metninde beklenen adlarini tasir.
// sistemdeki gorevi: Testlerin snapshot kimliklerini ve rapor mailindeki kaynak/dokuman adlarini ayni yerden okumasini saglar.
public sealed record ContractCheckRunFixture(
    Guid SourceId,
    string SourceName,
    string DocumentName,
    Guid BaseSnapshotId,
    Guid TargetSnapshotId);
