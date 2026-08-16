namespace Ptn.ApiContractChecker.Dtos.Snapshots;

// islevi: Operasyonun zorunlu parametre ad, konum ve tipini HTTP cevabinda tasir.
public class OperationParameterSummaryDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Type { get; set; }
}
