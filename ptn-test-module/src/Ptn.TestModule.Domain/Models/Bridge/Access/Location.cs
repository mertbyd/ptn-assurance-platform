namespace Ptn.TestModule.Models.Bridge;

// islevi: API ve veritabani konumlarini anlam cakismasi olmadan tek modelde tasir.
// sistemdeki gorevi: OpenAPI semasi ile veritabani semasinin SchemaName adinda birlesmesini engeller.
public sealed class Location
{
    public string? ApiSchemaName { get; set; }
    public string? DbSchemaName { get; set; }
    public string? DbTableName { get; set; }
    public string? ColumnName { get; set; }
    public string? OperationId { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public string? JsonPointer { get; set; }
}
