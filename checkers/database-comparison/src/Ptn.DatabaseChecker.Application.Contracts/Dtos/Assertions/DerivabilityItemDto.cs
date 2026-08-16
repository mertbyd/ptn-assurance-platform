namespace Ptn.DatabaseChecker.Dtos.Assertions;

// islevi: Tek DB assertion icin tablo/kolon referansi ve kapali derivability outcome'unu tasir.
// sistemdeki gorevi: API AssertionDerivabilityItemDto'nun adres+outcome wire seklini DB adresleriyle hizalar.
public sealed class DerivabilityItemDto
{
    public string TableRef { get; set; } = string.Empty;
    public string ColumnRef { get; set; } = string.Empty;
    public string OutcomeCode { get; set; } = string.Empty;
}
