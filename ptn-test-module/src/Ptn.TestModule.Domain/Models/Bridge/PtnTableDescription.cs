using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Bir tablonun assertion yazarligi icin gerekli kolon ve anahtar ozetini tasir.
// sistemdeki gorevi: Database Checker sema DTO'sunu Domain icinde provider-bagimsiz veri kabuguna cevirir.
public sealed class PtnTableDescription
{
    public PtnLocation Location { get; set; } = new();
    public List<PtnTableColumn> Columns { get; set; } = [];
    public List<PtnTableKey> Keys { get; set; } = [];

    // islevi: Tek tablo kolonunun tip, nullability ve uretilme niteliklerini tasir.
    // sistemdeki gorevi: Assertion adaylarini ham provider tiplerinden bagimsiz aciklar.
    public sealed class PtnTableColumn
    {
        public string Name { get; set; } = string.Empty;
        public string DataTypeCode { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public bool IsGenerated { get; set; }
    }

    // islevi: Tekil veya birincil tablo anahtarinin kolon listesini tasir.
    // sistemdeki gorevi: KeyUnique kararini serbest SQL veya satir denemesi olmadan destekler.
    public sealed class PtnTableKey
    {
        public string KindCode { get; set; } = string.Empty;
        public List<string> Columns { get; set; } = [];
    }
}
