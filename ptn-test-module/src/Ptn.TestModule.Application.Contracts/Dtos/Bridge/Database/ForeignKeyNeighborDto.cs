namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Tek gelen veya giden FK komsusunu public Bridge cevabinda tasir.
// sistemdeki gorevi: DB binding onerilerinin yon ve kolon kanitini checker tipinden bagimsiz sunar.
public sealed class ForeignKeyNeighborDto
{
    public string DirectionCode { get; set; } = string.Empty;
    public string ConstraintName { get; set; } = string.Empty;
    public string DbSchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<string> LocalColumns { get; set; } = [];
    public List<string> NeighborColumns { get; set; } = [];
}
