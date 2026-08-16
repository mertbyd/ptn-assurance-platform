using Ptn.ApiContractChecker.Constants.Diagnosis;

namespace Ptn.ApiContractChecker.Models.Diagnosis;

// islevi: HTTP sinifini ve yalniz yapilandirilmis, katalogda dogrulanmis hata olgularini tasir.
// sistemdeki gorevi: Kurallarin durum kodu eslemesi veya lokalize govde metni kullanmadan karar vermesini saglar.
public sealed class FailureIdentity
{
    public string SourceKindCode { get; set; } = FailureSourceKindCodes.HttpStatus;
    public int? StatusCode { get; set; }
    public string StatusClassCode { get; set; } = HttpStatusClassCodes.Transport;
    public string IdentityConfidenceCode { get; set; } = DiagnosisConfidenceCodes.Low;
    public string? ProblemType { get; set; }
    public string? ProblemTitle { get; set; }
    public string? ProblemDetail { get; set; }
    public string? ProblemInstance { get; set; }
    public string? RemoteServiceErrorCode { get; set; }
    public string? ChallengeScheme { get; set; }
    public string? ChallengeError { get; set; }
    public List<string> ChallengeScopes { get; set; } = new();
    public List<string> AllowedMethods { get; set; } = new();
    public string? ConformanceOutcomeCode { get; set; }
    public string? TransportErrorCode { get; set; }
    public string? SentContentType { get; set; }
    public List<ProblemErrorSignal> ProblemErrors { get; set; } = new();
    public List<ObjectReference> ObjectReferences { get; set; } = new();
    internal bool RejectedStructuredName { get; private set; }

    // islevi: Dogrulanmis yapilandirilmis alan bulundugunda kimlik guvenini yuksek yapar.
    public void Upgrade()
    {
        if (!RejectedStructuredName)
        {
            IdentityConfidenceCode = DiagnosisConfidenceCodes.High;
        }
    }

    // islevi: Snapshot'ta dogrulanamayan adi atip kimligi fail-closed Low seviyesine indirir.
    public void RejectStructuredName()
    {
        RejectedStructuredName = true;
        IdentityConfidenceCode = DiagnosisConfidenceCodes.Low;
    }
}
