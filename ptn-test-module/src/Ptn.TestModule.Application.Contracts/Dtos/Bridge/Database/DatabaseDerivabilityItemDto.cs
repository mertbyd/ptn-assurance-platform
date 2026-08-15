namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Tek DB assertion adresinin kapali turetilebilirlik sonucunu tasir.
// sistemdeki gorevi: Yayim kapisina tablo, kolon ve normalize outcome kanitini verir.
public sealed class DatabaseDerivabilityItemDto
{
    /// <summary>
    /// Ilgili kaynagin kararli kimligini veya referansini belirtir.
    /// </summary>
    public string TableRef { get; set; } = string.Empty;
    /// <summary>
    /// Ilgili kaynagin kararli kimligini veya referansini belirtir.
    /// </summary>
    public string ColumnRef { get; set; } = string.Empty;
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string OutcomeCode { get; set; } = string.Empty;
}
