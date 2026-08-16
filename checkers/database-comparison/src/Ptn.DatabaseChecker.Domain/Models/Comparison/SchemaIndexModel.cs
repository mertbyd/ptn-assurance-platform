using System.Collections.Generic;
using Volo.Abp.ObjectExtending;

namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Tek bir index'in (PK dahil) kanonik temsili.
// sistemdeki gorevi: Diff motoru index'leri kolon listesi + ozellikleriyle kiyaslar; sistem uretimi adlar (PK__xxx) tek basina fark sayilmaz (04 no'lu belge).
public class SchemaIndexModel : ExtensibleObject
{
    // Index adi; sistem uretimi olabilecegi icin eslestirmede tek basina guvenilmez, ozellik kiyasinin yaninda tasinir.
    public string Name { get; set; } = string.Empty;

    // Tekillik zorunlulugu var mi (UNIQUE index/constraint).
    public bool IsUnique { get; set; }

    // Birincil anahtari destekleyen index mi; PK eksigi isterlerdeki "eksik kisit" bulgusudur.
    public bool IsPrimaryKey { get; set; }

    // Index'in anahtar kolonlari, tanimdaki sirasiyla; index kiyasinin asil olcutu budur.
    public List<string> Columns { get; set; } = new();

    // INCLUDE (...) ile index yapragina tasinan ek kolonlar; iki motorda da ayni fikir.
    public List<string> IncludedColumns { get; set; } = new();

    // Kosullu index'in WHERE ifadesi (PG: partial index, MSSQL: filtered index); yoksa null.
    public string? FilterDefinition { get; set; }

    // Motorun index icin geri urettigi ham tanim metni; kolon ayrismasi yetersiz kaldiginda rapor kaniti olarak kullanilir.
    public string? Definition { get; set; }
}
