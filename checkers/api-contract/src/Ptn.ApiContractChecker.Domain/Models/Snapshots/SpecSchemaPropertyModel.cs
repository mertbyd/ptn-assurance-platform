namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Bir sema ozelliginin tip, null, zorunluluk ve enum sozlesmesini tasir.
// sistemdeki gorevi: DTO alanlarini adlariyla eslestirecek sema diff adimina kararli alan verisi saglar.
public class SpecSchemaPropertyModel
{
    // Ozelligin semadaki adi.
    public string Name { get; set; } = string.Empty;

    // Ozelligin ham tip ifadesi.
    public string? Type { get; set; }

    // Ozelligin null deger kabul edip etmedigi.
    public bool Nullable { get; set; }

    // Ozelligin parent semanin required listesinde bulunup bulunmadigi.
    public bool Required { get; set; }

    // Request ornegi uretiminde istemci tarafindan gonderilmemesi gereken alan.
    public bool ReadOnly { get; set; }

    // Ozelligin kabul ettigi enum degerleri.
    public List<string> EnumValues { get; set; } = new();

    // Ozellik semasi bir component'e bagliysa korunacak referans kimligi.
    public string? ReferenceId { get; set; }

    // Runtime validation icin ozelligin ic ice veya referansla cozulmus semasi.
    public SpecSchemaModel? Schema { get; set; }
}
