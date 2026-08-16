using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Correlation;

namespace Ptn.DatabaseChecker.Dtos.Assertions;

// islevi: Tek assertion'in baglanti, tablo, anahtar, matcher, cardinality ve polling API girdisidir.
// sistemdeki gorevi: Row/count/absent ve batch endpoint'lerinin ortak request sozlesmesidir; secret veya serbest SQL tasimaz.
public class RowAssertionRequestDto
{
    public Guid ConnectionId { get; set; }
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<ColumnExpectationDto> Expectations { get; set; } = new();
    public CardinalityExpectationDto Cardinality { get; set; } = new();
    public int TimeoutMs { get; set; }
    public int PollIntervalMs { get; set; }
    public bool IncludeRowOnFailure { get; set; }
    public CorrelationRefDto? Correlation { get; set; }
}
