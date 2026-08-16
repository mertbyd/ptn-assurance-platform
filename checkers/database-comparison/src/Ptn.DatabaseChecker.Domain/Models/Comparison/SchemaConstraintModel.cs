using System.Collections.Generic;
using Volo.Abp.ObjectExtending;

namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Tek bir tablo constraint'inin kanonik temsilidir (PK/unique/FK/check).
// sistemdeki gorevi: Diff motoru constraint'leri ad, tur, kolon listesi, hedef tablo/kolon ve ham tanim metni uzerinden karsilastirir.
public class SchemaConstraintModel : ExtensibleObject
{
    // Constraint adi; sistem uretimi olabilecegi icin tek basina eslestirme olcutu degildir.
    public string Name { get; set; } = string.Empty;

    // Constraint turunun kararli kodu (SchemaConstraintTypeCodes.*).
    public string TypeCode { get; set; } = string.Empty;

    // Constraint'in kaynak kolonlari; PK/unique/check icin tablo kolonlari, FK icin parent kolonlaridir.
    public List<string> Columns { get; set; } = new();

    // FK icin hedef tablo tam adi (schema.table); FK disinda null kalir.
    public string? ReferencedTable { get; set; }

    // FK icin hedef kolonlar; FK disinda bos kalir.
    public List<string> ReferencedColumns { get; set; } = new();

    // FK ON DELETE davranisi (SchemaReferentialActionCodes.*); FK disinda null kalir.
    public string? DeleteActionCode { get; set; }

    // FK ON UPDATE davranisi (SchemaReferentialActionCodes.*); FK disinda null kalir.
    public string? UpdateActionCode { get; set; }

    // Motorun constraint icin geri urettigi ham tanim metni; normalizasyon diff fazinda yapilir.
    public string? Definition { get; set; }

    // Kisit mevcut satirlar dahil dogrulanmis/guvenilir mi; yeni model kurulumlarinda guvenli varsayilan true'dur.
    public bool IsValidated { get; set; } = true;

    // Kisit veri degisikliklerinde etkin mi; provider katalogundaki disabled bilgisinin tersidir.
    public bool IsEnabled { get; set; } = true;

    // Kisit transaction sonuna ertelenebilir mi; desteklemeyen motorlarda false kalir.
    public bool IsDeferrable { get; set; }

    // Ertelenebilir kisit varsayilan olarak transaction sonuna mi ertelenir; desteklemeyen motorlarda false kalir.
    public bool IsInitiallyDeferred { get; set; }
}
