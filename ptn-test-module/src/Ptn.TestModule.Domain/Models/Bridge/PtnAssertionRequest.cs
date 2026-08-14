using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Satir, sayim ve yokluk assertion'larinin ortak tipli veritabani girdisini tasir.
// sistemdeki gorevi: Database Checker DTO'sunu, secret'i ve serbest SQL'i domain sinirinin disinda tutar.
public sealed class PtnAssertionRequest
{
    public Guid ConnectionId { get; set; }
    public string DbSchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<PtnColumnExpectation> Expectations { get; set; } = [];
    public string CardinalityKindCode { get; set; } = string.Empty;
    public long ExpectedCount { get; set; }
    public int TimeoutMs { get; set; }
    public int PollIntervalMs { get; set; }

    // islevi: Tek kolon matcher'inin kapali kod, beklenen deger ve tolerans alanlarini tasir.
    // sistemdeki gorevi: Assertion beklentisini SQL veya provider ifadesi olmadan checker portuna verir.
    public sealed class PtnColumnExpectation
    {
        public string ColumnName { get; set; } = string.Empty;
        public string MatcherKindCode { get; set; } = string.Empty;
        public string? ExpectedValue { get; set; }
        public List<string?> ExpectedValues { get; set; } = [];
        public decimal? Tolerance { get; set; }
    }
}
