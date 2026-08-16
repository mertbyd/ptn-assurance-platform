using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Models.Capabilities;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Correlation;

namespace Ptn.DatabaseChecker.Interface.Capabilities;

// islevi: Motor-ozel write-set capability yoklama, capture ve idempotent release islemlerini tanimlar.
// sistemdeki gorevi: Manager'i Npgsql replication protokolu ve provider hata ayrintilarindan ayirir.
public interface IWriteSetRepository : IEngineComponent
{
    Task<CapabilityLevel> ProbeAsync(
        DatabaseConnectionInfo info,
        CancellationToken cancellationToken = default);

    Task<WriteSetResult> CaptureAsync(
        DatabaseConnectionInfo info,
        Guid captureRef,
        List<ComparisonTableIdentifierModel> candidateTables,
        CorrelationRef? correlation,
        CancellationToken cancellationToken = default);

    Task<bool> ReleaseAsync(
        DatabaseConnectionInfo info,
        Guid captureRef,
        CancellationToken cancellationToken = default);
}
