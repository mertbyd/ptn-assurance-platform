using System.ComponentModel;
using System.IO;
using System.Reflection;
using ModelContextProtocol.Server;

namespace Ptn.TestModule.Mcp;

// islevi: Test edilecek yazilimin Git'te tutulan is kurallarini MCP Resource olarak sunar.
// sistemdeki gorevi: rules_fingerprint'i besleyen kaynagi ajan politikasindan ayri Resource'ta tutar.
[McpServerResourceType]
public sealed class BusinessRulesResource
{
    [McpServerResource(Name = "ptn_business_rules", UriTemplate = "ptn://authoring/kurallar.md", MimeType = "text/markdown")]
    [Description("Business rules of the software under test; the source of rules_fingerprint.")]
    public static string Read()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Ptn.TestModule.Authoring.kurallar.md")!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
