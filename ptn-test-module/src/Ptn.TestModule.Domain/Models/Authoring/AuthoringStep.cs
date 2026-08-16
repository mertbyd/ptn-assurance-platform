using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Authoring;

// islevi: Grounding kanitina cozulmus tek Arazzo API adiminin mekanik alanlarini tasir.
// sistemdeki gorevi: Cache oturumunda serbest operasyon adresi yerine checker kaynakli adimi saklar.
public sealed class AuthoringStep
{
    public string StepId { get; set; } = string.Empty;
    public Guid OperationReferenceId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? RequestBodyJson { get; set; }
    public List<string> AssertionPaths { get; set; } = [];
}
