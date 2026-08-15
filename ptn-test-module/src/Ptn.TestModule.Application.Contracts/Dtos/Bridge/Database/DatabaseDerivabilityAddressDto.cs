namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Turetilebilirligi sinanacak DB assertion'in katalog ve matcher adresini tasir.
// sistemdeki gorevi: Checker DTO'sunu sizdirmadan x-checknexus-db yayin kapisi girdisini tipler.
public sealed class DatabaseDerivabilityAddressDto
{
    /// <summary>
    /// Hedef semanin kararli adini belirtir.
    /// </summary>
    public string SchemaName { get; set; } = string.Empty;
    /// <summary>
    /// Hedef tablonun adini veya kararli adresini belirtir.
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    /// <summary>
    /// Isleme katilan kolon adlarini kararli sirada listeler.
    /// </summary>
    public List<string> KeyColumns { get; set; } = [];
    /// <summary>
    /// Isleme katilan kolon adlarini kararli sirada listeler.
    /// </summary>
    public List<string> ExpectedColumns { get; set; } = [];
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string MatcherCode { get; set; } = string.Empty;
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string CardinalityKindCode { get; set; } = string.Empty;
}
