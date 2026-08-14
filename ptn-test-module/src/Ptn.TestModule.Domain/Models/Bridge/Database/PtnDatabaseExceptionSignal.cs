using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Database;

// islevi: Database teshisindeki engine exception kodlarini ve provider alanlarini tasir.
// sistemdeki gorevi: Exception union kolunu checker DTO'sundan bagimsiz domain modeli olarak tutar.
public sealed class PtnDatabaseExceptionSignal
{
    public string EngineCode { get; set; } = string.Empty;
    public string SqlState { get; set; } = string.Empty;
    public Dictionary<string, string?> ProviderFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
