using Ptn.DatabaseChecker.Dtos.Correlation;

namespace Ptn.DatabaseChecker.Dtos.Projections;

// islevi: Projection endpointinin baglanti, katalog adresi, anahtar, kolon ve satir butcesi girdisidir.
// sistemdeki gorevi: Serbest SQL veya secret tasimayan public salt-okunur projection sozlesmesidir.
public sealed class ProjectionRequestDto
{
    public Guid ConnectionId { get; set; }
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> ProjectColumns { get; set; } = [];
    public int? MaxRows { get; set; }
    public CorrelationRefDto? Correlation { get; set; }
}
