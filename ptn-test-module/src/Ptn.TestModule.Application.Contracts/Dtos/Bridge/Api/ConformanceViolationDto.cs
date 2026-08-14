namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Tek response uygunluk ihlalinin kural ve konumunu tasir.
// sistemdeki gorevi: Checker ayrintisini kararli Bridge alanlarina indirger.
public sealed class ConformanceViolationDto
{
    public string RuleCode { get; set; } = string.Empty;
    public string JsonPointer { get; set; } = string.Empty;
    public string Keyword { get; set; } = string.Empty;
}
