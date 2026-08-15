using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Serbest SQL icermeyen, anahtarla sinirli ve butceli veritabani projeksiyonunu tasir.
// sistemdeki gorevi: Kanit motorunun yalniz izinli tablo ve kolonlardan redaksiyonlu olgu istemesini saglar.
public sealed class ProjectionRequest
{
    public Guid ConnectionId { get; set; }
    public string DbSchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> ProjectColumns { get; set; } = [];
    public int MaxRows { get; set; }
    public CorrelationRef? Correlation { get; set; }
}
