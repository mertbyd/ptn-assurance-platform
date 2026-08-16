namespace Ptn.ApiContractChecker.Dtos.Diagnosis;

// islevi: Raporun yapilandirilmis ve katalog-dogrulanmis failure identity alanlarini tasir.
// sistemdeki gorevi: HTTP sinifi, auth, problem, conformance ve transport olgularini ham govdesiz disariya acar.
public sealed class FailureIdentityDto
{
    public string SourceKindCode { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public string StatusClassCode { get; set; } = string.Empty;
    public string IdentityConfidenceCode { get; set; } = string.Empty;
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
    public List<ProblemErrorDto> ProblemErrors { get; set; } = new();
    public List<ObjectReferenceDto> ObjectReferences { get; set; } = new();
}
