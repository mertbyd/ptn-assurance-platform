using System;

namespace Ptn.TestModule.Dtos.Authoring;

// islevi: Oturumda grounding kanitina cozulmus tek API adimini public olarak tasir.
// sistemdeki gorevi: Sonraki turda kullanici ve ajanin mekanik birlesim durumunu okumasini saglar.
public sealed class AuthoringStepDto
{
    public string StepId { get; set; } = string.Empty;
    public Guid OperationReferenceId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? RequestBodyJson { get; set; }
    public List<string> AssertionPaths { get; set; } = [];
}
