namespace Ptn.DatabaseChecker.Models.Assertions;

// islevi: Tek assertion adresinin tablo/kolon referansini ve fail-closed turetilebilirlik sonucunu tasir.
// sistemdeki gorevi: API checker'in adres+outcome seklinin veritabani karsiligidir.
public sealed class DerivabilityItem
{
    public string TableRef { get; set; } = string.Empty;
    public string ColumnRef { get; set; } = string.Empty;
    public string OutcomeCode { get; set; } = string.Empty;
}
