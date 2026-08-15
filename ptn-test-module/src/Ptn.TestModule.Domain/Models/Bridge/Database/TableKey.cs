using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Tekil veya birincil tablo anahtarinin kolon listesini tasir.
// sistemdeki gorevi: KeyUnique kararini serbest SQL veya satir denemesi olmadan destekler.
public sealed class TableKey
{
    public string Name { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = [];
}
