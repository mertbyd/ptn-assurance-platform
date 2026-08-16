using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Authoring;

public sealed class AuthoringExpectation
{
    public string ColumnName { get; set; } = string.Empty;
    public string MatcherCode { get; set; } = string.Empty;
    public string? Value { get; set; }
}
