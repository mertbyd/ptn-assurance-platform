using System.ComponentModel;
using System.IO;
using System.Reflection;
using ModelContextProtocol.Server;

namespace Ptn.TestModule.Mcp;

// islevi: Git'te tutulan kurallar.md yazarlik bilgisini MCP Resource olarak sunar.
// sistemdeki gorevi: Is bilgisini tool cevabina gommeden istemcinin talep uzerine okumasini saglar.
[McpServerResourceType]
public sealed class AuthoringRulesResource
{
    [McpServerResource(Name = "ptn_authoring_rules", UriTemplate = "ptn://authoring/kurallar.md", MimeType = "text/markdown")]
    [Description("Deterministic authoring rules for the PTN test platform.")]
    public static string Read()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Ptn.TestModule.Authoring.kurallar.md")!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
