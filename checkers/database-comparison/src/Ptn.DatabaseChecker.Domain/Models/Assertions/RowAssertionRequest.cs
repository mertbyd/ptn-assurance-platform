using System.Collections.Generic;
using Ptn.DatabaseChecker.Models.Correlation;

namespace Ptn.DatabaseChecker.Models.Assertions;

// islevi: Tek hedef satir assertion'inin baglanti, tablo, anahtar, beklenti ve bekleme parametrelerini tasir.
// sistemdeki gorevi: Persist edilmeyen domain istegi RowAssertionManager'in tekli ve batch cekirdeklerine girer; DTO Domain katmanina gecmez.
public sealed class RowAssertionRequest
{
    public Guid ConnectionId { get; set; }
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<ColumnExpectation> Expectations { get; set; } = new();
    public CardinalityExpectation Cardinality { get; set; } = new();
    public int TimeoutMs { get; set; }
    public int PollIntervalMs { get; set; }
    public bool IncludeRowOnFailure { get; set; }
    public CorrelationRef? Correlation { get; set; }
}
