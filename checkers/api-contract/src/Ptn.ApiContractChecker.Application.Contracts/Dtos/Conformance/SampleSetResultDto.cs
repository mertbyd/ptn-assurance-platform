namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Kapali sonucu ve semadan uretilen alan orneklerini public response'a tasir.
// sistemdeki gorevi: Bos ornek listesini snapshot veya operasyon cozum sonucuyla birlikte aciklar.
public class SampleSetResultDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public List<FieldSampleDto> Samples { get; set; } = new();
}
