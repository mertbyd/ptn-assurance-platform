namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Veri karsilastirmasinda COUNT(*) alinacak tabloyu motor-bagimsiz sema+ad kimligiyle tasir.
// sistemdeki gorevi: Scope/snapshot tarafindan guvenli secilmis tablo listesini EFCore provider repository'lerine ham DTO tasimadan iletir.
public class ComparisonTableIdentifierModel
{
    // COUNT(*) alinacak tablonun semasi; identifier quoting provider repository'de yapilir.
    public string SchemaName { get; set; } = default!;

    // COUNT(*) alinacak tablonun adi.
    public string TableName { get; set; } = default!;
}
