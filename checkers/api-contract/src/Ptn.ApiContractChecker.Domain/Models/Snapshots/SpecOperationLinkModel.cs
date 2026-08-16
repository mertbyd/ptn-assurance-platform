namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: OpenAPI response link'inin hedef operasyonunu ve parametre runtime expression'larini tasir.
// sistemdeki gorevi: Provider link tipini domaine sizdirmadan beyan edilmis adim zinciri kanitini korur.
public class SpecOperationLinkModel
{
    public string Name { get; set; } = string.Empty;
    public string? TargetOperationId { get; set; }
    public string? TargetOperationReference { get; set; }
    public Dictionary<string, string> ParameterExpressions { get; set; } = new(StringComparer.Ordinal);
}
