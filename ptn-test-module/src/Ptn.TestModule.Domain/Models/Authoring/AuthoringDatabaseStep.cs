using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Authoring;

public sealed class AuthoringDatabaseStep
{
    public string StepId { get; set; } = string.Empty;
    public Guid TableReferenceId { get; set; }
    public string OperationCode { get; set; } = string.Empty;
    public Dictionary<string, string?> KeyBindings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<AuthoringExpectation> Expectations { get; set; } = [];
    public int TimeoutMs { get; set; }
    public int PollIntervalMs { get; set; }
}
