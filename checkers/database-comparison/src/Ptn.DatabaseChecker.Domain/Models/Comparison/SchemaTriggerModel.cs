using Volo.Abp.ObjectExtending;

namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Tek bir trigger'in kanonik temsili: adi ve motorun geri urettigi tanim metni.
// sistemdeki gorevi: Tanim metni ham halde tasinir (PG: pg_get_triggerdef, MSSQL: OBJECT_DEFINITION); yalanci farklari eleyen normalizasyon 7. gorevde eklenir.
public class SchemaTriggerModel : ExtensibleObject
{
    // Trigger adi; tablo icinde eslestirme anahtaridir.
    public string Name { get; set; } = string.Empty;

    // Motorun geri urettigi ham CREATE TRIGGER metni; icerik degisikligi kiyasi bu metin uzerinden yapilir.
    public string Definition { get; set; } = string.Empty;

    // Trigger veri degisikliklerinde etkin mi; provider katalogundaki disabled durumunun tersidir.
    public bool IsEnabled { get; set; } = true;
}
