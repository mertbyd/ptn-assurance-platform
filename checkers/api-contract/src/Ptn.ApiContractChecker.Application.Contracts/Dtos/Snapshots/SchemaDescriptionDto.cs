namespace Ptn.ApiContractChecker.Dtos.Snapshots;

// islevi: Tek component semasinin butceli bir-seviye ozetini HTTP cevabinda tasir.
public class SchemaDescriptionDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public string SchemaRef { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool Nullable { get; set; }
    public List<string> EnumValues { get; set; } = [];
    public List<SchemaFieldSummaryDto> Fields { get; set; } = [];
    public bool IsTruncated { get; set; }
    public string? ResultRef { get; set; }
}
