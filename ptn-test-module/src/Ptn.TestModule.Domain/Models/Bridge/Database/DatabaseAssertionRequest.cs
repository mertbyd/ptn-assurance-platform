using System;
using System.Collections.Generic;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Models.Bridge.Database;

// islevi: Satir, sayim ve yokluk assertion'larinin ortak tipli veritabani girdisini tasir.
// sistemdeki gorevi: Database Checker DTO'sunu, secret'i ve serbest SQL'i domain sinirinin disinda tutar.
public sealed class DatabaseAssertionRequest
{
    public Guid ConnectionId { get; set; }
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<ColumnExpectation> Expectations { get; set; } = [];
    public DatabaseCardinalityExpectation Cardinality { get; set; } = new();
    public int TimeoutMs { get; set; }
    public int PollIntervalMs { get; set; }
    public bool IncludeRowOnFailure { get; set; } = true;
    public CorrelationRef? Correlation { get; set; }
}
