using System.Text.Json.Nodes;
using NJsonSchema;
using Ptn.ApiContractChecker.Constants.Formats.Lookups;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Snapshots;

// islevi: Swagger 2.0 nullable ve validator dialect kararini uygular.
public class Swagger20SchemaDialectComponent : SpecSchemaDialectComponentBase, ITransientDependency
{
    public override string FormatCode => SpecFormatCodes.Swagger20;
    protected override string DialectUri => SpecSchemaDialectUris.Draft04;
    protected override SchemaType SchemaType => SchemaType.Swagger2;

    protected override void AddNullable(JsonObject node, string? type)
    {
        node[SpecSchemaJsonPropertyNames.SwaggerNullable] = true;
    }
}
