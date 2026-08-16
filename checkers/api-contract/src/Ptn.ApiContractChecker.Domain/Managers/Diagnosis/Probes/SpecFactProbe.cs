using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Probes;

// islevi: Snapshot operasyonu, request medya tipi ve security scope olgularini aga cikmadan uretir.
// sistemdeki gorevi: Katalogdan cevaplanabilen hipotezlerin network probe butcesi harcamamasini saglar.
public sealed class SpecFactProbe : IDiagnosisProbe, ITransientDependency
{
    public string ProbeKindCode => ProbeKindCodes.SpecFact;

    // islevi: Istek fact adini ilgili snapshot olgusuna cevirir.
    public Task<ProbeEvidence> RunAsync(ProbeRequest request, CancellationToken cancellationToken = default)
    {
        var present = request.FactName switch
        {
            ProbeKindCodes.Names.Operation => request.Context.Operation != null,
            ProbeKindCodes.Names.RequestMediaType => HasRequestMediaType(request),
            ProbeKindCodes.Names.SecurityScope => HasSecurityScopes(request),
            _ => false
        };
        return Task.FromResult(CreateEvidence(request, present));
    }

    // islevi: Gonderilen medya tipinin operasyon request body katalogunda bulunmasini denetler.
    private static bool HasRequestMediaType(ProbeRequest request)
        => request.Context.Operation?.RequestBodies.Any(body =>
            string.Equals(body.MediaType, request.Context.Signal.SentContentType,
                StringComparison.OrdinalIgnoreCase)) == true;

    // islevi: Challenge scope'larinin operasyon security requirement scope'larini kapsamasini denetler.
    private static bool HasSecurityScopes(ProbeRequest request)
    {
        var required = request.Context.Operation?.SecurityRequirements
            .SelectMany(item => item.Schemes)
            .SelectMany(item => item.Scopes)
            .Distinct(StringComparer.Ordinal)
            .ToList() ?? new List<string>();
        var actual = request.Context.Identity.ChallengeScopes.ToHashSet(StringComparer.Ordinal);
        return required.Count == 0 || required.All(actual.Contains);
    }

    // islevi: Boolean katalog sonucunu kararli evidence koduna cevirir.
    private static ProbeEvidence CreateEvidence(ProbeRequest request, bool present)
        => new()
        {
            ProbeKindCode = ProbeKindCodes.SpecFact,
            HypothesisKindCode = request.HypothesisKindCode,
            FactCode = present ? ProbeKindCodes.Facts.Present : ProbeKindCodes.Facts.Absent,
            ExpectedValue = request.ExpectedValue
        };
}
