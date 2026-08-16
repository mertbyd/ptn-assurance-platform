using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Managers.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Adapters.Diagnosis;

// islevi: Fixed safe method kullanan network probe'larin hedef guard ve header-only HTTP akislarini birlestirir.
// sistemdeki gorevi: Dort adapterde ayni SSRF kontrolu ve response disposal akisini tekrar etmeden unsafe method yuzeyi acmaz.
public abstract class SafeHttpDiagnosisProbeBase : IDiagnosisProbe
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ProbeTargetGuard _targetGuard;

    public abstract string ProbeKindCode { get; }
    protected abstract HttpMethod SafeMethod { get; }

    protected SafeHttpDiagnosisProbeBase(IHttpClientFactory httpClientFactory, ProbeTargetGuard targetGuard)
    {
        _httpClientFactory = httpClientFactory;
        _targetGuard = targetGuard;
    }

    // islevi: Hedefi snapshot allow-list'ine karsi dogrulayip fixed safe method ile header-only response alir.
    public async Task<ProbeEvidence> RunAsync(
        ProbeRequest request,
        CancellationToken cancellationToken = default)
    {
        _targetGuard.EnsureAllowed(request);
        using var message = new HttpRequestMessage(SafeMethod, request.TargetUri);
        var client = _httpClientFactory.CreateClient(ProbeKindCodes.HttpClientName);
        var startedAt = Environment.TickCount64;
        using var response = await client.SendAsync(
            message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        return BuildEvidence(request, response, Environment.TickCount64 - startedAt);
    }

    // islevi: Safe response status ve header'larini probe-ozel yapilandirilmis kanita cevirir.
    protected abstract ProbeEvidence BuildEvidence(
        ProbeRequest request,
        HttpResponseMessage response,
        long observedAtMs);

    // islevi: Ortak probe kimligi ve gozlem zamanini evidence nesnesine yerlestirir.
    protected ProbeEvidence CreateEvidence(ProbeRequest request, string factCode, long observedAtMs)
        => new()
        {
            ProbeKindCode = ProbeKindCode,
            HypothesisKindCode = request.HypothesisKindCode,
            FactCode = factCode,
            ObservedAtMs = observedAtMs
        };
}
