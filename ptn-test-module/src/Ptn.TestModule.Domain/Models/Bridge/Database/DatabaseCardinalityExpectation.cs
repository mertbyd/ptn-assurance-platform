namespace Ptn.TestModule.Models.Bridge.Database;

// islevi: Database assertion icin beklenen satir kardinalitesini nested model olarak tasir.
// sistemdeki gorevi: Mapperly'nin checker DTO'suna attribute veya elle alan kopyalamadan eslemesini saglar.
public sealed class DatabaseCardinalityExpectation
{
    public string KindCode { get; set; } = string.Empty;
    public long ExpectedCount { get; set; }
}
