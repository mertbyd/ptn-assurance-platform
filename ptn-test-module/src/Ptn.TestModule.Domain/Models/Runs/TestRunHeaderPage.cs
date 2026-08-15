using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Kosum baslik sorgusunun toplam ve sayfa satirlarini tasir.
// sistemdeki gorevi: Iki repository sonucunu tek tipli okuma modelinde birlestirir.
public sealed class TestRunHeaderPage
{
    public long TotalCount { get; set; }
    public IReadOnlyList<TestRunHeader> Items { get; set; } = [];
}
