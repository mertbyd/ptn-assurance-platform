namespace Ptn.TestModule.Models.Bridge.Invariants;

// islevi: Tek is degismezi yoklamasinin desen kodunu ve iki olculen degerini tasir.
// sistemdeki gorevi: Alan adi, tablo adi veya kolon adi tasimadan saf aritmetik girdiyi sinirlar.
public sealed class BusinessInvariantRequest
{
    public string PatternCode { get; set; } = string.Empty;
    public decimal Left { get; set; }
    public decimal Right { get; set; }
    public decimal Delta { get; set; }
}
