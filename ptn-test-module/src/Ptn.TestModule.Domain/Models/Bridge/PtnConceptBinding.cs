using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Kapali bir is kavraminin onayli tablo ve kolon baglamasini tasir.
// sistemdeki gorevi: Somut sema bilgisini ajan tahmini yerine surumlu profil paketinden saglar.
public sealed class PtnConceptBinding
{
    public string ConceptCode { get; set; } = string.Empty;
    public string DbSchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, string> ColumnMap { get; set; } = new(StringComparer.Ordinal);
    public string PatternCode { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
}
