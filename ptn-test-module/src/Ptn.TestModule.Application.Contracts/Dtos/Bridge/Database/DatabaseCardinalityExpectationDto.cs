namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Assertion icin beklenen satir kardinalitesini tasir.
// sistemdeki gorevi: Cardinality navigation alanini Mapperly ile dogrudan eslenebilir tutar.
public sealed class DatabaseCardinalityExpectationDto
{
    public string KindCode { get; set; } = string.Empty;
    public long ExpectedCount { get; set; }
}
