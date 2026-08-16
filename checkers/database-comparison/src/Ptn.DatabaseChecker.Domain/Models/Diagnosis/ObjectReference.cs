namespace Ptn.DatabaseChecker.Models.Diagnosis;

// islevi: Hata kimliginden cikarilan sema, tablo, kolon ve constraint adlarini yapilandirilmis olarak tasir.
// sistemdeki gorevi: Provider adlarini ancak canli katalog dogrulamasi sonrasinda rapor konumuna tasinabilecek tek nesne referansinda toplar.
public sealed class ObjectReference
{
    public string? SchemaName { get; set; }
    public string? TableName { get; set; }
    public string? ColumnName { get; set; }
    public string? ConstraintName { get; set; }
    public bool IsCatalogVerified { get; set; }

    // islevi: Referansta katalogda dogrulanabilecek herhangi bir ad kalip kalmadigini bildirir.
    public bool HasName()
        => !string.IsNullOrWhiteSpace(SchemaName) ||
           !string.IsNullOrWhiteSpace(TableName) ||
           !string.IsNullOrWhiteSpace(ColumnName) ||
           !string.IsNullOrWhiteSpace(ConstraintName);
}
