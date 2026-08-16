using NJsonSchema;

namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Cozulmus sema dugumu ile ona uygulanacak validation dialect'ini birlikte tasir.
// sistemdeki gorevi: NJsonSchema secimini resolver sinirinda tutup manager'a tek kullanilabilir sonuc verir.
public sealed class ResolvedSpecSchemaModel
{
    public JsonSchema SchemaNode { get; }
    public SchemaType SchemaType { get; }

    public ResolvedSpecSchemaModel(JsonSchema schemaNode, SchemaType schemaType)
    {
        SchemaNode = schemaNode;
        SchemaType = schemaType;
    }
}
