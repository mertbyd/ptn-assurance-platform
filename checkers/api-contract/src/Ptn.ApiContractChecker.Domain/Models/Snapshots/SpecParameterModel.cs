namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Bir operasyon parametresinin kimligini ve istemci sozlesmesini tasir.
// sistemdeki gorevi: Parametreleri ad ve konum ciftine gore eslestirecek diff adimina provider-bagimsiz veri verir.
public class SpecParameterModel
{
    // Parametrenin spec'te bildirilen adi.
    public string Name { get; set; } = string.Empty;

    // Parametrenin path, query, header veya cookie konumu.
    public string In { get; set; } = string.Empty;

    // Parametrenin istemci tarafindan gonderilmesinin zorunlu olup olmadigi.
    public bool Required { get; set; }

    // Parametrenin semadan indirgenen tip ifadesi.
    public string? Type { get; set; }

    // Parametrenin null deger kabul edip etmedigi.
    public bool Nullable { get; set; }

    // Parametrenin kabul ettigi enum degerlerinin provider-bagimsiz metinleri.
    public List<string> EnumValues { get; set; } = new();

    // Parametre semasi bir component'e bagliysa korunacak referans kimligi.
    public string? ReferenceId { get; set; }

    // Runtime request validation ve placeholder uretimi icin tam parametre semasi.
    public SpecSchemaModel? Schema { get; set; }
}
