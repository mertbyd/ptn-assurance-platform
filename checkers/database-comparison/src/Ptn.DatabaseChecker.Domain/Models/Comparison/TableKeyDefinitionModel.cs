using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: DescribeTable sonucunda tek PK veya unique index'in ad ve sirali kolonlarini tasir.
// sistemdeki gorevi: Senaryo yazari guvenli assertion anahtarini katalog sozlesmesinden secebilir.
public sealed class TableKeyDefinitionModel
{
    public string Name { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = new();
}
