using System.Collections.Generic;
using Volo.Abp.ObjectExtending;

namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Tek bir tablonun kanonik temsili: kimligi (sema+ad) ve alt nesneleri (kolon, index, trigger).
// sistemdeki gorevi: Diff motoru tablolari "Schema.Name" anahtariyla eslestirir; motora ozgu ek bilgiler ExtraProperties cantasinda kaybolmadan tasinir.
public class SchemaTableModel : ExtensibleObject
{
    // Tablonun ait oldugu sema; her zaman acik tasinir cunku "yazilmamis sema" varsayimi motorlarda farklidir (PG: public, MSSQL: dbo).
    public string Schema { get; set; } = string.Empty;

    // Tablo adi; Schema ile birlikte diff eslestirmesinin anahtaridir.
    public string Name { get; set; } = string.Empty;

    // Tablonun kolonlari; sira bilgisi kolonun kendi Ordinal alanindadir.
    public List<SchemaColumnModel> Columns { get; set; } = new();

    // Birincil anahtar dahil tum index'ler.
    public List<SchemaIndexModel> Indexes { get; set; } = new();

    // Tablonun PK/unique/FK/check constraint'leri.
    public List<SchemaConstraintModel> Constraints { get; set; } = new();

    // Tabloya bagli DML trigger'lar (PG: pg_trigger, MSSQL: sys.triggers).
    public List<SchemaTriggerModel> Triggers { get; set; } = new();
}
