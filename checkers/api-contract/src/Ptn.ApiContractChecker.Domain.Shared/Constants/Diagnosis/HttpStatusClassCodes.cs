namespace Ptn.ApiContractChecker.Constants.Diagnosis;

// islevi: HTTP durum kodlarini RFC 9110 siniflarina ve tasima hatasi sinifina indirger.
// sistemdeki gorevi: Teshis kurallarinin tekil durum koduna metin eslemek yerine protokol olgusuyla calismasini saglar.
public static class HttpStatusClassCodes
{
    public const string Informational = "Informational";
    public const string Success = "Success";
    public const string Redirection = "Redirection";
    public const string ClientError = "ClientError";
    public const string ServerError = "ServerError";
    public const string Transport = "Transport";

    public static string FromStatusCode(int? statusCode)
    {
        if (!statusCode.HasValue)
        {
            return Transport;
        }

        return (statusCode.Value / 100) switch
        {
            1 => Informational,
            2 => Success,
            3 => Redirection,
            4 => ClientError,
            5 => ServerError,
            _ => Transport
        };
    }
}
