using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Dtos.Authoring;

// islevi: Ajan tarafindan uretilen bir kolon beklentisini tasir.
// sistemdeki gorevi: Arazzo veya checker formatindan bagimsiz, tipli expectation tasinmasini saglar.
public sealed class AuthoringExpectationDto
{
    public string ColumnName { get; set; } = string.Empty;
    public string MatcherCode { get; set; } = string.Empty;
    public string? Value { get; set; }
}
