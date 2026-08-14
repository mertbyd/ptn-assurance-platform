namespace Ptn.TestModule.Models.Bridge.Diagnosis;

// islevi: Database checker diagnosis konumunun kaynak alan seklini tasir.
// sistemdeki gorevi: SchemaName ve TableName semantigini ortak konuma Manager'in cevirmesini saglar.
public sealed class PtnDatabaseDiagnosisLocation
{
    public string? SchemaName { get; set; }
    public string? TableName { get; set; }
    public string? ColumnName { get; set; }
}
