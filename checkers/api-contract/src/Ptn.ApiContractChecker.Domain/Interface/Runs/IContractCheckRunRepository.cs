using Ptn.ApiContractChecker.Entities.Runs;
using Ptn.ApiContractChecker.Models.Runs;

namespace Ptn.ApiContractChecker.Interface.Runs;

// islevi: ContractCheckRun detay, hafif baslik, execution fotografi, stale recovery ve gecis sorgularini tanimlar.
// sistemdeki gorevi: Findings/snapshot projeksiyonu ve tum run LINQ'inin EF repository uygulamasinda kalmasini saglar.
public interface IContractCheckRunRepository : IBaseRepository<ContractCheckRun>
{
    // Tek run basligini owned findings govdesini cekmeden getirir.
    Task<ContractCheckRunHeaderModel?> FindHeaderAsync(Guid id);

    // Tek run detayini owned findings govdesiyle getirir.
    Task<ContractCheckRunDetailModel?> FindDetailAsync(Guid id);

    // Durum gecisi icin aggregate'i owned findings govdesiyle getirir.
    Task<ContractCheckRun?> FindEntityAsync(Guid id);

    // Execution context icin run'in iki snapshot icerigini tek tenant-kapsamli sorguda getirir.
    Task<ContractCheckSnapshotPairModel?> FindExecutionSnapshotPairAsync(Guid id);

    // Esigi asmis Running run aggregate'lerini tenant filtresi altinda toplu getirir.
    Task<List<ContractCheckRun>> GetStaleRunningAsync(Guid runningStatusId, DateTime startedBefore);

    // Run basliklarini opsiyonel dokuman ve kaynak filtreleriyle CreationTime azalan sirada sayfalar.
    Task<List<ContractCheckRunHeaderModel>> GetPagedHeadersAsync(
        int skipCount,
        int maxResultCount,
        Guid? specDocumentId,
        Guid? specSourceId);

    // Ayni filtrelerle gorulebilir run toplam sayisini hesaplar.
    Task<long> GetHeaderCountAsync(Guid? specDocumentId, Guid? specSourceId);

    // Tek run'in fingerprintlerini owned bulgu govdesinin diger alanlarini cekmeden projekte eder.
    Task<List<string?>> GetFindingFingerprintsAsync(Guid id);

    // Ayni snapshot dokuman cifti icin onceki tamamlanmis run'in yalniz fingerprintlerini getirir.
    Task<List<string?>?> FindPreviousCompletedFindingFingerprintsAsync(Guid id);

    // Belirtilen run ayni dokuman cifti/tenant icinde daha eski Completed referanssa yalniz fingerprintlerini getirir.
    Task<List<string?>?> FindCompletedReferenceFindingFingerprintsAsync(
        Guid currentRunId,
        Guid referenceRunId);

    // Tek run'in bulgularini repository-native filtre ve sayfalama ile duz read modellere projekte eder.
    Task<List<FindingReadModel>> GetPagedFindingsAsync(
        Guid id,
        int skipCount,
        int maxResultCount,
        string? severityCode,
        string? kindCode,
        string? path,
        string? schemaName,
        FindingFingerprintSelectionModel? fingerprintSelection);

    // Sayfayla ayni bulgu filtrelerinin toplam satir sayisini hesaplar.
    Task<long> GetFindingCountAsync(
        Guid id,
        string? severityCode,
        string? kindCode,
        string? path,
        string? schemaName,
        FindingFingerprintSelectionModel? fingerprintSelection);
}
