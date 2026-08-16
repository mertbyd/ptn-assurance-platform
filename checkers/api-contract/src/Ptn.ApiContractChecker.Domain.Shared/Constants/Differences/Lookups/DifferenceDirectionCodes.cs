namespace Ptn.ApiContractChecker.Constants.Differences.Lookups;

// islevi: Bir farkin sozlesmenin hangi yonunu etkiledigini belirleyen kararli kodlari tanimlar.
// sistemdeki gorevi: Engine siniflandirmasi, owned JSON ve seed arasindaki tek kod sozlesmesidir.
public static class DifferenceDirectionCodes
{
    public const string Request = "request";
    public const string Response = "response";
    public const string Endpoint = "endpoint";
    public const string Documentation = "documentation";

    public static IReadOnlyCollection<string> All { get; } =
        [Request, Response, Endpoint, Documentation];
}
