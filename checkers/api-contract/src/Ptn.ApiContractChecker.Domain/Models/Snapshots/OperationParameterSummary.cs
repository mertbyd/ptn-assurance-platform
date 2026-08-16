namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Operasyonun zorunlu tek parametresini ad, konum ve tip ile tasir.
// sistemdeki gorevi: Ajanin request iskeleti yazarken tum operasyon modelini almasini engeller.
public class OperationParameterSummary
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Type { get; set; }
}
