using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: DescribeTable cevabinda PK veya unique index'in ad ve sirali kolonlarini tasir.
// sistemdeki gorevi: Test Module'un satiri tekil belirleyen guvenli key binding'i senaryo yaziminda secmesini saglar.
public class TableKeyDefinitionDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = new();
}
