namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Degisken kolonlu dis-veritabani satirini sabit sekilli EF raw-query sonucuna paketler.
// sistemdeki gorevi: PostgreSQL ve SQL Server tek batch sorgulari sema, tablo ve JSON satir payload'ini ayni CLR modele dondurur.
public class TableDataJsonRowModel
{
    // Satirin ait oldugu sema adi.
    public string SchemaName { get; set; } = default!;

    // Satirin ait oldugu tablo adi.
    public string TableName { get; set; } = default!;

    // Tum kolonlari ve null degerleri iceren provider JSON nesnesi.
    public string RowJson { get; set; } = default!;
}
