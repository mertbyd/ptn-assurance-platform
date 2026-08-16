namespace Ptn.ApiContractChecker.Dtos.Snapshots;

// islevi: Tek sema alaninin ad, tip, zorunluluk, enum ve ref ozetini HTTP cevabinda tasir.
public class SchemaFieldSummaryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool Required { get; set; }
    public bool Nullable { get; set; }
    public List<string> EnumValues { get; set; } = [];
    public string? ReferenceId { get; set; }
}
