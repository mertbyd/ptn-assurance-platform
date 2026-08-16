using System;

namespace Ptn.TestModule.Dtos.Authoring;

// islevi: Tek adimlik yapilandirilmis model onerisi ile kapali operasyon referansini tasir.
// sistemdeki gorevi: LLM'in tam Arazzo belgesi yazmasini public sozlesmede engeller.
public sealed class AddAuthoringStepDto
{
    public string StepId { get; set; } = string.Empty;
    public Guid OperationReferenceId { get; set; }
    public string? RequestBodyJson { get; set; }
    public List<string> AssertionPaths { get; set; } = [];
}
