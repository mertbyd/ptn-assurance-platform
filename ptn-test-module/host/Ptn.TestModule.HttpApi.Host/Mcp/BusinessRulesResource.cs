using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Ptn.TestModule.Interface.Bridge;

namespace Ptn.TestModule.Mcp;

// islevi: Test edilecek yazilimin Git'te tutulan is kurallarini MCP Resource olarak sunar.
// sistemdeki gorevi: rules_fingerprint'i besleyen kaynagin ayni baytlarini ajana gomulu kopya olmadan verir.
[McpServerResourceType]
public sealed class BusinessRulesResource
{
    [McpServerResource(Name = "ptn_business_rules", UriTemplate = "ptn://authoring/kurallar.md", MimeType = "text/markdown")]
    [Description("Business rules of the software under test; the source of rules_fingerprint.")]
    public static async Task<string> Read(IBusinessRuleSourcePort source, CancellationToken cancellationToken)
    {
        var content = await source.ReadAsync(cancellationToken);
        return Encoding.UTF8.GetString(content);
    }
}
