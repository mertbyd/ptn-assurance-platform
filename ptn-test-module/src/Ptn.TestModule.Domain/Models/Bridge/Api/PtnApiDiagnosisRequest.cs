using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Api;

// islevi: API checker teshis isteginin kaynak-ozgul domain modelini tasir.
// sistemdeki gorevi: Manager kararini Mapperly ile checker DTO'suna elle alan kopyalamadan tasir.
public sealed class PtnApiDiagnosisRequest
{
    public Guid SnapshotId { get; set; }
    public Guid? ContractCheckRunId { get; set; }
    public string? OperationId { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public int? StatusCode { get; set; }
    public string? ContentType { get; set; }
    public string? SentContentType { get; set; }
    public string? Accept { get; set; }
    public string? ConformanceOutcomeCode { get; set; }
    public string? TransportErrorCode { get; set; }
    public string? ProblemType { get; set; }
    public string? ProblemTitle { get; set; }
    public string? ProblemDetail { get; set; }
    public string? ProblemInstance { get; set; }
    public string? RemoteServiceErrorCode { get; set; }
    public string? ResponseVersion { get; set; }
    public string? ResourceUrl { get; set; }
    public long? ObservedAtMs { get; set; }
    public Dictionary<string, string> ResponseHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> RequestHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<PtnProblemError> ProblemErrors { get; set; } = [];
}
