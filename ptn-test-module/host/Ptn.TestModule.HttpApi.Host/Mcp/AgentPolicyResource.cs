using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Ptn.TestModule.Interface.Bridge;

namespace Ptn.TestModule.Mcp;

// islevi: Git'te tutulan ajan politikasini MCP Resource olarak sunar.
// sistemdeki gorevi: Politikayi is kurallariyla ayni ayarli kokten okur; assembly kopyasi tutmaz.
[McpServerResourceType]
public sealed class AgentPolicyResource
{
    [McpServerResource(Name = "ptn_agent_policy", UriTemplate = "ptn://authoring/agent-policy.md", MimeType = "text/markdown")]
    [Description("Deterministic authoring agent policy for the PTN test platform.")]
    public static async Task<string> Read(IAgentPolicySourcePort source, CancellationToken cancellationToken)
    {
        var content = await source.ReadAsync(cancellationToken);
        return Encoding.UTF8.GetString(content);
    }
}
