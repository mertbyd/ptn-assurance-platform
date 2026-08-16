namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Bir yanit header'inin ad, zorunluluk ve tip sozlesmesini tasir.
// sistemdeki gorevi: Header ekleme, silme ve tip degisikligini response diff adimina provider tipinden bagimsiz verir.
public class SpecHeaderModel
{
    // Header'in spec'te bildirilen adi.
    public string Name { get; set; } = string.Empty;

    // Header'in yanitta bulunmasinin zorunlu olup olmadigi.
    public bool Required { get; set; }

    // Header semasindan indirgenen tip ifadesi.
    public string? Type { get; set; }

    // Header degerinin null kabul edip etmedigi.
    public bool Nullable { get; set; }

    // Header semasi bir component'e bagliysa korunacak referans kimligi.
    public string? ReferenceId { get; set; }

    // Spec'te bildirilen kararli header orneginin kanonik JSON temsili.
    public string? Example { get; set; }
}
