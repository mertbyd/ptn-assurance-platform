namespace Ptn.TestModule.Dtos.Bridge.Invariants;

// islevi: Is degismezi yoklamasinin desen kodunu ve olculen degerlerini tasir.
// sistemdeki gorevi: Alan, tablo veya kolon adi tasimadan saf aritmetik girdiyi public yuzeyde sinirlar.
public sealed class BusinessInvariantRequestDto
{
    /// <summary>Degerlendirilecek kapali is degismezi desen kodunu belirtir.</summary>
    public string PatternCode { get; set; } = string.Empty;

    /// <summary>Karsilastirmanin sol tarafindaki olculen degeri belirtir.</summary>
    public decimal Left { get; set; }

    /// <summary>Karsilastirmanin sag tarafindaki olculen degeri belirtir.</summary>
    public decimal Right { get; set; }

    /// <summary>Delta deseninde beklenen tam degisim miktarini belirtir.</summary>
    public decimal Delta { get; set; }
}
