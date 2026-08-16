namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Tek gerekceli ve redakte edilmis alan ornegini public response'a tasir.
// sistemdeki gorevi: Ajanin ornek degeri kisit, konum ve beklenen sonuc kodlariyla birlikte okumasini saglar.
public class FieldSampleDto
{
    public string FieldPointer { get; set; } = string.Empty;
    public string ConstraintCode { get; set; } = string.Empty;
    public string SampleKindCode { get; set; } = string.Empty;
    public string PositionCode { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string ExpectedOutcomeCode { get; set; } = string.Empty;
}
