using Ptn.ApiContractChecker.Managers.Conformance;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis;

// islevi: Failure identity'yi mevcut operation resolver ile snapshot operasyonu ve RelatedFindings'e yerlestirir.
// sistemdeki gorevi: Yeni parser veya repository sorgusu acmadan rule'lara tek provider-bagimsiz context verir.
public sealed class FailureContextResolver : ITransientDependency
{
    private readonly OperationResolver _operationResolver;

    public FailureContextResolver(OperationResolver operationResolver)
    {
        _operationResolver = operationResolver;
    }

    // islevi: Operasyonu operationId veya method/path ile cozer ve kimlik adresini context'e tasir.
    public ResolvedFailureContext Resolve(
        SpecSnapshotModel snapshot,
        HttpFailureSignal signal,
        FailureIdentity identity,
        List<Finding> relatedFindings)
    {
        var operation = _operationResolver.Resolve(
            snapshot, signal.OperationId, signal.Method ?? string.Empty, signal.Path ?? string.Empty);
        return new ResolvedFailureContext
        {
            Snapshot = snapshot,
            Identity = identity,
            Operation = operation,
            Signal = signal,
            RelatedFindings = relatedFindings.ToList(),
            Location = new ObjectReference
            {
                OperationId = operation?.OperationId ?? signal.OperationId,
                Method = operation?.Method ?? signal.Method,
                Path = operation?.Path ?? signal.Path,
                JsonPointer = signal.ProblemErrors.Select(item => item.Pointer).FirstOrDefault(item => item != null)
            }
        };
    }
}
