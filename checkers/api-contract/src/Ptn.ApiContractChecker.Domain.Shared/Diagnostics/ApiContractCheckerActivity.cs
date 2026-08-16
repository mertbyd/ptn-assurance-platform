using System.Diagnostics;
using Ptn.ApiContractChecker.Constants.Diagnostics;

namespace Ptn.ApiContractChecker.Diagnostics;

// islevi: Checker span'larini yalniz izinli kimlik, an ve response byte attribute'lariyla acar.
// sistemdeki gorevi: Govde, query degeri, token, secret path ve host adinin telemetriye sizmasini merkezi olarak engeller.
public static class ApiContractCheckerActivity
{
    private static readonly ActivitySource Source = new(ApiContractCheckerDiagnostics.ActivitySourceName);

    // islevi: Kararli span adini ve A/B/C/D anini guvenli attribute'larla baslatir.
    public static Activity? Start(string spanName, string moment, Guid? runId = null)
    {
        var activity = Source.StartActivity(spanName);
        activity?.SetTag(ApiContractCheckerDiagnostics.MomentAttribute, moment);
        if (runId.HasValue)
        {
            activity?.SetTag(ApiContractCheckerDiagnostics.RunIdAttribute, runId.Value);
        }

        return activity;
    }

    // islevi: Basarili cevabin yalniz UTF-8 byte sayisini span'a ekler.
    public static void SetResponseBytes(Activity? activity, int responseBytes)
        => activity?.SetTag(ApiContractCheckerDiagnostics.ResponseBytesAttribute, responseBytes);
}
