using Ptn.DatabaseChecker.Models.Correlation;

namespace Ptn.DatabaseChecker.Models.Projections;

// islevi: Katalog adresi, unique anahtar ve secili kolonlarla salt-okunur projection talebini tasir.
// sistemdeki gorevi: DTO veya serbest SQL sizintisi olmadan Manager ile provider portu arasindaki girdidir.
public sealed class ProjectionRequest
{
    public Guid ConnectionId { get; set; }
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> ProjectColumns { get; set; } = [];
    public int? MaxRows { get; set; }
    public CorrelationRef? Correlation { get; set; }
}
