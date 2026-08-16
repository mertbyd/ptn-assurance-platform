using System;
using Ptn.TestModule.Dtos.Bridge;

namespace Ptn.TestModule.Dtos.Authoring;

// islevi: Gecici yazarlik oturumunun soru, cevap, adim ve mekanik Arazzo belgesini tasir.
// sistemdeki gorevi: Sonraki model turunun onceki insan cevabini ve belge durumunu okumasini saglar.
public sealed class AuthoringSessionDto
{
    public Guid Id { get; set; }
    public Guid SpecSnapshotId { get; set; }
    public string WorkflowId { get; set; } = string.Empty;
    public string WorkflowSummary { get; set; } = string.Empty;
    public List<ClosedQuestionDto> Questions { get; set; } = [];
    public Dictionary<string, string> Answers { get; set; } = [];
    public List<AuthoringStepDto> Steps { get; set; } = [];
    public string SourceDocument { get; set; } = string.Empty;
    public long TtlMs { get; set; }
}
