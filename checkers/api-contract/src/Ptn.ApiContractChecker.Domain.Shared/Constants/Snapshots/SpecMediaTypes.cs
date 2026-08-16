namespace Ptn.ApiContractChecker.Constants.Snapshots;

// islevi: Bir spec govdesinin tasinabilecegi kararli HTTP medya tiplerini tanimlar.
// sistemdeki gorevi: Cekim guard'i, format tespiti ve SpecContent.MediaType ayni medya tipi sozlesmesini tek kaynaktan okur.
public static class SpecMediaTypes
{
    public const string Json = "application/json";
    public const string OpenApiJson = "application/vnd.oai.openapi+json";
    public const string Yaml = "application/yaml";
    public const string XYaml = "application/x-yaml";
    public const string TextYaml = "text/yaml";
    public const string TextXYaml = "text/x-yaml";
    public const string PlainText = "text/plain";

    public static IReadOnlyCollection<string> All { get; } =
        [Json, OpenApiJson, Yaml, XYaml, TextYaml, TextXYaml, PlainText];
}
