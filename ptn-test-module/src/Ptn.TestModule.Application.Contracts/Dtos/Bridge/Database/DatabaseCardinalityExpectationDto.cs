namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Assertion icin beklenen satir kardinalitesini tasir.
// sistemdeki gorevi: Cardinality navigation alanini Mapperly ile dogrudan eslenebilir tutar.
public sealed class DatabaseCardinalityExpectationDto
{
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string KindCode { get; set; } = string.Empty;
    /// <summary>
    /// Assertion icin beklenen satir sayisini belirtir.
    /// </summary>
    public long ExpectedCount { get; set; }
}
