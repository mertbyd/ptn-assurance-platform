namespace Ptn.ApiContractChecker.Constants.Formats.Lookups;

// islevi: Desteklenen ve rezerve OpenAPI formatlarinin kararli kodlarini tanimlar.
// sistemdeki gorevi: Parser secimi, seed ve snapshot FK cozumunun tek kod sozlesmesidir.
public static class SpecFormatCodes
{
    public const string Swagger20 = "swagger-20";
    public const string OpenApi30 = "oas-30";
    public const string OpenApi31 = "oas-31";
    public const string OpenApi32 = "oas-32";

    public static IReadOnlyCollection<string> All { get; } =
        [Swagger20, OpenApi30, OpenApi31, OpenApi32];
}
