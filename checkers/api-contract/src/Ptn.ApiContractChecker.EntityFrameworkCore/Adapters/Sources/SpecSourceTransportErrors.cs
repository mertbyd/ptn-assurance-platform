using Polly.Timeout;
using Ptn.ApiContractChecker.ExceptionCodes.Sources;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Adapters.Sources;

// islevi: Spec kaynagi tasimasindan gelen yabanci exception'lari kararli alan hata kodlarina cevirir.
// sistemdeki gorevi: Cekim ve erisilebilirlik yollarinin ayni hata haritasini paylasmasini saglar; harita ikinci bir yerde yazilmaz.
public static class SpecSourceTransportErrors
{
    // Yalniz tasima kaynakli exception'i sinirda cevrilebilir sayar; is hatasi ve programlama hatasi oldugu gibi yukari cikar.
    // Cagiran taraf henuz CancellationToken tasimadigi icin iptal ile zaman asimi ayni kovada durur; token KBP-615'te geldiginde ayrilir.
    public static bool IsTransportFailure(Exception exception)
    {
        return exception is HttpRequestException
            or TimeoutRejectedException
            or TimeoutException
            or TaskCanceledException;
    }

    // Tasima hatasini ag ve zaman asimi basliklarindan birine indirger.
    public static string Resolve(Exception exception)
    {
        return exception is HttpRequestException
            ? SpecSourceExceptionCodes.FetchNetworkFailed
            : SpecSourceExceptionCodes.FetchTimedOut;
    }
}
