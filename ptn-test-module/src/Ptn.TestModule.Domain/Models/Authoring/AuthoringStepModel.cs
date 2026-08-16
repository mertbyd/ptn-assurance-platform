using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Authoring;

// islevi: Modelin onerdigi tek API adiminin kapali operasyon referansi ve assertion yollarini tasir.
// sistemdeki gorevi: Tam Arazzo belgesi yerine yalniz bir adimin tipli domain girdisini sinirlar.
public sealed class AuthoringStepModel
{
    public string StepId { get; set; } = string.Empty;
    public Guid OperationReferenceId { get; set; }
    public string? RequestBodyJson { get; set; }
    public List<string> AssertionPaths { get; set; } = [];
}
