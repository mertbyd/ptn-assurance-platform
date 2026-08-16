using System.Text.Json.Nodes;
using NJsonSchema;
using Ptn.ApiContractChecker.Constants.Formats.Lookups;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Snapshots;

// islevi: OpenAPI 3.0 nullable ve validator dialect kararini uygular.
public class OpenApi30SchemaDialectComponent : SpecSchemaDialectComponentBase, ITransientDependency
{
    public override string FormatCode => SpecFormatCodes.OpenApi30;
    protected override string DialectUri => SpecSchemaDialectUris.Draft04;
    protected override SchemaType SchemaType => SchemaType.OpenApi3;

    protected override void AddNullable(JsonObject node, string? type)
    {
        node[SpecSchemaJsonPropertyNames.Nullable] = true;
    }
}
