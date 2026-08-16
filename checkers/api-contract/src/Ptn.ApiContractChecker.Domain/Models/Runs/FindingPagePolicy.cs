namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: Bulgu okumasinin varsayilan adet, tavan adet ve UTF-8 cikti butcesini tasir.
// sistemdeki gorevi: Setting cozumunu sayfalama ve butce uygulamasindan ayirir.
public class FindingPagePolicy
{
    public int DefaultPageSize { get; init; }
    public int MaxPageSize { get; init; }
    public int MaxResponseBytes { get; init; }
}
