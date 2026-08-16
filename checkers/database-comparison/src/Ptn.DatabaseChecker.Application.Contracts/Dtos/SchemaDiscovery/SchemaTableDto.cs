using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: Tek bir tablonun API cevap modelidir (sema + ad + kolon/index/constraint/trigger detaylari).
// sistemdeki gorevi: Derin sema okumasinda tablo kimligini ve alt nesnelerini frontend'e tasir; rapor onizleme ve diff motoru icin tam tablo fotografini sunar.
public class SchemaTableDto
{
    /// <summary>
    /// Tablonun ait oldugu sema adi.
    /// </summary>
    public string Schema { get; set; } = default!;

    /// <summary>
    /// Tablo adi.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Tablonun kolonlari (sira bilgisi kolonun Ordinal alanindadir).
    /// </summary>
    public List<SchemaColumnDto> Columns { get; set; } = new();

    /// <summary>
    /// Tablonun index'leri (PK destek index'i dahil).
    /// </summary>
    public List<SchemaIndexDto> Indexes { get; set; } = new();

    /// <summary>
    /// Tablonun PK/unique/FK/check constraint'leri.
    /// </summary>
    public List<SchemaConstraintDto> Constraints { get; set; } = new();

    /// <summary>
    /// Tabloya bagli trigger'lar.
    /// </summary>
    public List<SchemaTriggerDto> Triggers { get; set; } = new();
}
