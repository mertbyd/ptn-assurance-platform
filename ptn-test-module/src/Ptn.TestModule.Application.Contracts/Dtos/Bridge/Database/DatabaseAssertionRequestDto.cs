namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Satir, sayim ve yokluk assertion girdisini tasir.
// sistemdeki gorevi: Database checker DTO'sunu public Test Module sozlesmesine sizdirmadan tasima siniri kurar.
public sealed class DatabaseAssertionRequestDto
{
    public Guid ConnectionId { get; set; }
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<ColumnExpectationDto> Expectations { get; set; } = [];
    public DatabaseCardinalityExpectationDto Cardinality { get; set; } = new();
    public int TimeoutMs { get; set; }
    public int PollIntervalMs { get; set; }
    public bool IncludeRowOnFailure { get; set; } = true;
}
