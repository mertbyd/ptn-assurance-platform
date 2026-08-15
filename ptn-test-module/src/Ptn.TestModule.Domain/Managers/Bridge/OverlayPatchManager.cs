using System.Text.Json;
using System.Text.Json.Nodes;
using Ptn.TestModule.Models.Bridge.Agent;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Bulguyla bagli standart OpenAPI Overlay onerisini deterministik kurar.
// sistemdeki gorevi: Kendi DSL'ini uretmeden ve yamayi uygulamadan kademe-4 inceleme belgesi verir.
public class OverlayPatchManager : TestModuleDomainService
{
    // Tek Overlay 1.0.0 action belgesini uygulama yan etkisi olmadan uretir.
    public OverlayPatchSuggestion Suggest(string findingFingerprint, string target, string description, string updateJson)
    {
        var document = new JsonObject
        {
            ["overlay"] = "1.0.0",
            ["info"] = new JsonObject { ["title"] = description, ["version"] = "1.0.0" },
            ["actions"] = new JsonArray(new JsonObject
            {
                ["target"] = target,
                ["description"] = description,
                ["update"] = JsonNode.Parse(updateJson)
            }),
            ["x-finding-fingerprint"] = findingFingerprint
        };
        return new OverlayPatchSuggestion
        {
            FindingFingerprint = findingFingerprint,
            Document = document.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
            Applied = false
        };
    }
}
