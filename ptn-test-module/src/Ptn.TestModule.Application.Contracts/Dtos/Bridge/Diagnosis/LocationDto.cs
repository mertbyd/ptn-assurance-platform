namespace Ptn.TestModule.Dtos.Bridge.Diagnosis;

// islevi: API ve database bulgularinin ortak tipli konumunu tasir.
// sistemdeki gorevi: Kaynak-ozgul adresleri tek public navigation altinda toplar.
public sealed class LocationDto
{
    /// <summary>
    /// Hedef semanin kararli adini belirtir.
    /// </summary>
    public string? ApiSchemaName { get; set; }
    /// <summary>
    /// Hedef semanin kararli adini belirtir.
    /// </summary>
    public string? DbSchemaName { get; set; }
    /// <summary>
    /// Hedef tablonun adini veya kararli adresini belirtir.
    /// </summary>
    public string? DbTableName { get; set; }
    /// <summary>
    /// Hedef kolonun adini veya kararli adresini belirtir.
    /// </summary>
    public string? ColumnName { get; set; }
    /// <summary>
    /// Ilgili kaynagin kararli kimligini veya referansini belirtir.
    /// </summary>
    public string? OperationId { get; set; }
    /// <summary>
    /// HTTP operasyonunun yontemini belirtir.
    /// </summary>
    public string? Method { get; set; }
    /// <summary>
    /// Ilgili verinin kararli yol veya pointer adresini belirtir.
    /// </summary>
    public string? Path { get; set; }
    /// <summary>
    /// Ilgili verinin kararli yol veya pointer adresini belirtir.
    /// </summary>
    public string? JsonPointer { get; set; }
}
