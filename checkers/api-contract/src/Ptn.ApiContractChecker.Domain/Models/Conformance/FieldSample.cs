namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Tek alan degerini kaynak kisit, eksen konumu ve beklenen sonucuyla tasir.
// sistemdeki gorevi: Ajanin ornegi neden aldigini aciklayan redaksiyona hazir kanit birimidir.
public sealed class FieldSample
{
    public string FieldPointer { get; }
    public string ConstraintCode { get; }
    public string SampleKindCode { get; }
    public string PositionCode { get; }
    public string? Value { get; }
    public string ExpectedOutcomeCode { get; }

    // Bir mekanik ornegin tum gerekce alanlarini eksiksiz kurar.
    public FieldSample(
        string fieldPointer,
        string constraintCode,
        string sampleKindCode,
        string positionCode,
        string? value,
        string expectedOutcomeCode)
    {
        FieldPointer = fieldPointer;
        ConstraintCode = constraintCode;
        SampleKindCode = sampleKindCode;
        PositionCode = positionCode;
        Value = value;
        ExpectedOutcomeCode = expectedOutcomeCode;
    }
}
