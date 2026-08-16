namespace Ptn.ApiContractChecker.Constants.Snapshots.Lookups;

// islevi: Operasyon envanterinde filtrelenebilen HTTP metotlarinin kapali kod kumesini tanimlar.
// sistemdeki gorevi: Metot adini cagirana tahmin ettirmez; secilebilir degerleri checker'in sahiplendigi tek listede tutar.
public static class SpecOperationMethodCodes
{
    public const string Get = "GET";
    public const string Put = "PUT";
    public const string Post = "POST";
    public const string Delete = "DELETE";
    public const string Options = "OPTIONS";
    public const string Head = "HEAD";
    public const string Patch = "PATCH";
    public const string Trace = "TRACE";

    public static readonly IReadOnlyCollection<string> All =
        [Get, Put, Post, Delete, Options, Head, Patch, Trace];
}
