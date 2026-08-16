using System.Text.Json.Serialization;
using Ptn.ApiContractChecker.Dtos.Correlation;

namespace Ptn.ApiContractChecker.Dtos.Diagnosis;

// islevi: RFC 9457 alanlari ile diagnosis identity, location, hypotheses ve nextChecks uzantilarini tasir.
// sistemdeki gorevi: Test Module'a en fazla 4 KB deterministik teshis kontratini sunar.
public sealed class DiagnosisReportDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("detail")]
    public string Detail { get; set; } = string.Empty;

    [JsonPropertyName("instance")]
    public string Instance { get; set; } = string.Empty;

    [JsonPropertyName("checknexus:identity")]
    public FailureIdentityDto Identity { get; set; } = new();

    [JsonPropertyName("checknexus:location")]
    public ObjectReferenceDto Location { get; set; } = new();

    [JsonPropertyName("checknexus:hypotheses")]
    public List<HypothesisAssessmentDto> Hypotheses { get; set; } = new();

    [JsonPropertyName("checknexus:nextChecks")]
    public List<string> NextChecks { get; set; } = new();

    [JsonPropertyName("checknexus:correlation")]
    public CorrelationRefDto? Correlation { get; set; }
}
