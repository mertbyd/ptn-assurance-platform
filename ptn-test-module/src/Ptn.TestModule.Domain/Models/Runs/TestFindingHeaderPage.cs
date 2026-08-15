using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Bulgu baslik sorgusunun toplam ve sayfa satirlarini tasir.
// sistemdeki gorevi: Sayfali read-model sonucunu Application'a tek nesneyle verir.
public sealed class TestFindingHeaderPage
{
    public long TotalCount { get; set; }
    public IReadOnlyList<TestFindingHeader> Items { get; set; } = [];
}
