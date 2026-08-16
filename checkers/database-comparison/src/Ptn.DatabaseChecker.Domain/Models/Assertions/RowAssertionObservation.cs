using System.Collections.Generic;
using Ptn.DatabaseChecker.Models.Comparison;

namespace Ptn.DatabaseChecker.Models.Assertions;

// islevi: Tek polling denemesinde anahtar sorgusunun sayimini ve gerekirse sinirli satirlarini tasir.
// sistemdeki gorevi: Repository'nin count/read cagrilarini RowAssertionManager'a provider ve secret bilgisi sizdirmadan tek sonuc halinde verir.
public sealed class RowAssertionObservation
{
    public long RowCount { get; set; }
    public List<TableDataRowModel> Rows { get; set; } = new();
}
