using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Database;

// islevi: Database teshisindeki assertion kaynak, anahtar ve failure alanlarini tasir.
// sistemdeki gorevi: DbSchemaName cakismasini ortak konuma sokmadan kaynak-ozgul DTO eslemesini saglar.
public sealed class DatabaseAssertionSignal
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string OutcomeCode { get; set; } = string.Empty;
    public List<FailedExpectation> FailedExpectations { get; set; } = [];
}
