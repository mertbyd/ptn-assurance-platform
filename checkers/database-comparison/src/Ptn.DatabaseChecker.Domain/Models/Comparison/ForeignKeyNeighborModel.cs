using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Hedef tabloyla dogrudan FK bagi bulunan tek komsu tablonun yon ve kolon eslesmesini tasir.
// sistemdeki gorevi: DescribeTable cevabinda yalniz bir seviye iliski baglamini tasir; otomatik binding onerisi uretmez.
public sealed class ForeignKeyNeighborModel
{
    public string DirectionCode { get; set; } = string.Empty;
    public string ConstraintName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<string> LocalColumns { get; set; } = new();
    public List<string> NeighborColumns { get; set; } = new();
}
