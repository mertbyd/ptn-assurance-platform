namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Operasyon veya component semasindaki tek alanin yazarlik ozetini tasir.
// sistemdeki gorevi: Tam OpenAPI govdesi yerine ad, tip, zorunluluk, enum ve ref bilgisini acar.
public class SchemaFieldSummary
{
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool Required { get; set; }
    public bool Nullable { get; set; }
    public List<string> EnumValues { get; set; } = [];
    public string? ReferenceId { get; set; }
}
