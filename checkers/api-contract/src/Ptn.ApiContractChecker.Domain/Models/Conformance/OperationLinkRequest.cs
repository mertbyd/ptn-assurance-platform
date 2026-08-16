namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Kaynak operasyon kimligi ile aday butcesini zincir onerisi akisina tasir.
// sistemdeki gorevi: Serbest method/path tahmini olmadan tek snapshot operasyonunu kaynak yapar.
public sealed class OperationLinkRequest
{
    public string SourceOperationId { get; }
    public int MaxCandidates { get; }

    // Dogrulanmis kaynak kimligi ve aday tavanini degismez modelde kurar.
    public OperationLinkRequest(string sourceOperationId, int maxCandidates)
    {
        SourceOperationId = sourceOperationId;
        MaxCandidates = maxCandidates;
    }
}
