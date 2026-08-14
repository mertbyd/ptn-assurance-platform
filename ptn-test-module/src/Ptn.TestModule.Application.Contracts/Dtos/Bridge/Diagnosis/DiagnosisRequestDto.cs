using Ptn.TestModule.Dtos.Bridge.Database;

namespace Ptn.TestModule.Dtos.Bridge.Diagnosis;

// islevi: API veya database teshis sinyalini ortak public girdide tasir.
// sistemdeki gorevi: Checker DTO ailelerini Test Module Application.Contracts sinirinin disinda tutar.
public sealed class DiagnosisRequestDto
{
    public Guid? SpecSnapshotId { get; set; }
    public Guid? ApiRunId { get; set; }
    public Guid ConnectionId { get; set; }
    public LocationDto Location { get; set; } = new();
    public int? StatusCode { get; set; }
    public string? ContentType { get; set; }
    public string? OutcomeCode { get; set; }
    public string? TransportErrorCode { get; set; }
    public string? EngineCode { get; set; }
    public string? SqlState { get; set; }
    public long? ObservedAtMs { get; set; }
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<FailedExpectationDto> FailedExpectations { get; set; } = [];
    public Dictionary<string, string?> ProviderFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
