namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Provider katalogundan okunan tablo kimligini ortak snapshot akisi icin tasir.
// sistemdeki gorevi: PostgreSQL ve SQL Server katalog sorgulari tablo satirlarini bu ara modele cevirir; EFCore base repository bu modeli SchemaTableModel'e donusturur.
/// <summary>
/// Provider katalogundan gelen tablo kimligini ve adini, motor bagimsiz snapshot kurulumuna tasiyan ara Domain modelidir.
/// </summary>
/// <typeparam name="TKey">
/// Katalog kimlik tipi. PostgreSQL tarafinda oid icin uint, SQL Server tarafinda object_id/schema_id icin int kullanilir.
/// </typeparam>
public sealed class SchemaDiscoveredTableModel<TKey>
    where TKey : struct
{
    // Katalogdaki tablo nesne kimligi; kolon sorgularinda tabloya geri baglanma anahtaridir.
    public TKey Id { get; set; }

    // Tablonun bagli oldugu sema kimligi; base repository bu alanla sema adini sozlukten bulur.
    public TKey SchemaId { get; set; }

    // Tablonun veritabani icindeki adi; diff motorunda Schema + Name tablo eslestirme anahtarini olusturur.
    public string Name { get; set; } = string.Empty;
}
