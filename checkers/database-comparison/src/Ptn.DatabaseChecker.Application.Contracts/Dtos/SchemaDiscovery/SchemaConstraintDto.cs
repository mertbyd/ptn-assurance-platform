using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: Tek bir tablo constraint'inin API cevap modelidir.
// sistemdeki gorevi: Schema snapshot cevabinda PK/unique/FK/check detaylarini frontend ve rapor onizleme akislari icin tasir.
public class SchemaConstraintDto
{
    /// <summary>
    /// Constraint adi.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Constraint tur kodu (PrimaryKey/Unique/ForeignKey/Check).
    /// </summary>
    public string TypeCode { get; set; } = default!;

    /// <summary>
    /// Constraint'in kaynak kolonlari.
    /// </summary>
    public List<string> Columns { get; set; } = new();

    /// <summary>
    /// FK hedef tablo tam adi (schema.table); FK degilse bos.
    /// </summary>
    public string? ReferencedTable { get; set; }

    /// <summary>
    /// FK hedef kolonlari.
    /// </summary>
    public List<string> ReferencedColumns { get; set; } = new();

    /// <summary>
    /// FK ON DELETE davranis kodu.
    /// </summary>
    public string? DeleteActionCode { get; set; }

    /// <summary>
    /// FK ON UPDATE davranis kodu.
    /// </summary>
    public string? UpdateActionCode { get; set; }

    /// <summary>
    /// Motorun constraint icin urettigi ham tanim metni.
    /// </summary>
    public string? Definition { get; set; }

    /// <summary>
    /// Kisit mevcut veriye karsi dogrulanmis/guvenilir mi.
    /// </summary>
    public bool IsValidated { get; set; }

    /// <summary>
    /// Kisit etkin mi.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Kisit transaction sonuna ertelenebilir mi.
    /// </summary>
    public bool IsDeferrable { get; set; }

    /// <summary>
    /// Kisit varsayilan olarak ertelenmis mi.
    /// </summary>
    public bool IsInitiallyDeferred { get; set; }
}
