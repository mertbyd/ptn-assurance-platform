namespace Ptn.TestModule.Models.Bridge.Diagnosis;

// islevi: Database checker diagnosis kanitinin kaynak alan seklini tasir.
// sistemdeki gorevi: Ref ve fact normalizasyonunu Manager'a birakip Mapperly eslemesini attribute'suz tutar.
public sealed class PtnDatabaseDiagnosisEvidence
{
    public string ProbeKindCode { get; set; } = string.Empty;
    public string FactCode { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public string? ObservedValue { get; set; }
}
