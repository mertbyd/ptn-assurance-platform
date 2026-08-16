using System.ComponentModel;
using System.IO;
using System.Reflection;
using ModelContextProtocol.Server;

namespace Ptn.TestModule.Mcp;

// islevi: Git'te tutulan ajan politikasini MCP Resource olarak sunar.
// sistemdeki gorevi: Ajanin nasil davranacagini anlatan politikayi is kurallarindan ayri Resource'ta tutar.
[McpServerResourceType]
public sealed class AgentPolicyResource
{
    [McpServerResource(Name = "ptn_agent_policy", UriTemplate = "ptn://authoring/agent-policy.md", MimeType = "text/markdown")]
    [Description("Deterministic authoring agent policy for the PTN test platform.")]
    public static string Read()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Ptn.TestModule.Authoring.agent-policy.md")!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
