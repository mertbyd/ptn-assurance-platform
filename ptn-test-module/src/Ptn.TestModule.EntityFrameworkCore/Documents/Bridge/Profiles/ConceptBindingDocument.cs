using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Documents.Bridge.Profiles;

// islevi: YAML icindeki tek kavram baglamasinin transport alanlarini tasir.
// sistemdeki gorevi: Dosya alan adlarini domain baglama modeline kontrollu cevirir.
internal sealed class ConceptBindingDocument
{
    public string ConceptCode { get; set; } = string.Empty;
    public string DbSchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, string> ColumnMap { get; set; } = new(StringComparer.Ordinal);
    public string PatternCode { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
}
