using Ptn.DatabaseChecker.Models.Capabilities;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Correlation;

namespace Ptn.DatabaseChecker.Interface.Capabilities;

// islevi: Logical decoding yokken mevcut karsilastirma motorlariyla once/sonra advisory farki okur.
// sistemdeki gorevi: Engine resolver ile secilen veri repository'sini yeniden kullanir ve yeni bir comparison motoru olusturmaz.
public interface IDiffWriteSetRepository
{
    Task<WriteSetResult> CaptureAsync(
        DatabaseConnection connection,
        List<ComparisonTableIdentifierModel> candidateTables,
        CorrelationRef? correlation,
        CancellationToken cancellationToken = default);
}
