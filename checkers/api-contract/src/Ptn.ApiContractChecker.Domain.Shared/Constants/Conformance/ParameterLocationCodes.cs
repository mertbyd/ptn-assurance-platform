namespace Ptn.ApiContractChecker.Constants.Conformance;

// islevi: OpenAPI parametre konumlarinin kararli kodlarini tanimlar.
// sistemdeki gorevi: Request validation ve ornek uretiminde path/query/header tokenlarini ortaklastirir.
public static class ParameterLocationCodes
{
    public const string Path = "path";
    public const string Query = "query";
    public const string Header = "header";
    public const string Cookie = "cookie";
}
