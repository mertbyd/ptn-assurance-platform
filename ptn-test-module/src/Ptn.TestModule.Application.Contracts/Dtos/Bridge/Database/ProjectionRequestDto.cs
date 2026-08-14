namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Anahtarla sinirli database projeksiyon istegini tasir.
// sistemdeki gorevi: Serbest SQL'i public Bridge yuzeyinin disinda tutar.
public sealed class ProjectionRequestDto
{
    public Guid ConnectionId { get; set; }
    public string DbSchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> ProjectColumns { get; set; } = [];
    public int MaxRows { get; set; }
}
