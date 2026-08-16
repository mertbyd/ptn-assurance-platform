using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Conformance;

namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Hedef operasyon icin butcelenmis ve sirali onceki operasyon onerilerini tasir.
// sistemdeki gorevi: Oneriyi karar gibi sunmadan Test Module yazarlik yuzeyine tasir.
public sealed class OperationBindingResult
{
    public string OutcomeCode { get; }
    public List<OperationBindingSuggestion> Suggestions { get; }

    public OperationBindingResult(string outcomeCode, List<OperationBindingSuggestion> suggestions)
    {
        OutcomeCode = outcomeCode;
        Suggestions = suggestions;
    }

    public void TrimToBudget()
    {
        if (Suggestions.Count > ConformanceAuthoringConstants.MaxBindingSuggestions)
        {
            Suggestions.RemoveRange(
                ConformanceAuthoringConstants.MaxBindingSuggestions,
                Suggestions.Count - ConformanceAuthoringConstants.MaxBindingSuggestions);
        }

        while (MeasureUtf8Bytes() > ConformanceAuthoringConstants.MaxBindingSuggestionBytes && Suggestions.Count > 0)
        {
            Suggestions.RemoveAt(Suggestions.Count - 1);
        }
    }

    public int MeasureUtf8Bytes()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this).Length;
    }
}
