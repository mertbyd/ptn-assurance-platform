namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Hedef operasyonu besleyebilecek tek onceki operasyonu ve alan eslesmelerini tasir.
// sistemdeki gorevi: Isim benzerligi ve tip uyumu disinda karar vermeyen aciklanabilir ODG kenaridir.
public sealed class OperationBindingSuggestion
{
    public string? SourceOperationId { get; }
    public string SourceMethod { get; }
    public string SourcePath { get; }
    public List<OperationFieldBinding> Bindings { get; }
    public int Score { get; }

    public OperationBindingSuggestion(
        string? sourceOperationId,
        string sourceMethod,
        string sourcePath,
        List<OperationFieldBinding> bindings,
        int score)
    {
        SourceOperationId = sourceOperationId;
        SourceMethod = sourceMethod;
        SourcePath = sourcePath;
        Bindings = bindings;
        Score = score;
    }
}
