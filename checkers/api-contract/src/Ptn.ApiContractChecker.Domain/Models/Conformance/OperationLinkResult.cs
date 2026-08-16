namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Kaynak operasyon icin esik ustu zincir adaylarini kapali sonucuyla tasir.
// sistemdeki gorevi: Aday bulunmamasini tahminle doldurmadan Test Module yazarlik yuzeyine iletir.
public sealed class OperationLinkResult
{
    public string OutcomeCode { get; }
    public List<OperationLinkCandidate> Candidates { get; }

    // Kapali sonucu ve guven sirasindaki adaylari tek modelde kurar.
    public OperationLinkResult(string outcomeCode, List<OperationLinkCandidate> candidates)
    {
        OutcomeCode = outcomeCode;
        Candidates = candidates;
    }
}
