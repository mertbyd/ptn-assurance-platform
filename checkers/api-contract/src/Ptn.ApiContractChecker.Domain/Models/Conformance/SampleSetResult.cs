namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Operasyon icin uretilen gerekceli alan orneklerini ve kapali sonucu tasir.
// sistemdeki gorevi: Snapshot veya operasyon bulunamama durumunu bos listeyle karistirmadan HTTP katmanina iletir.
public sealed class SampleSetResult
{
    public string OutcomeCode { get; }
    public List<FieldSample> Samples { get; }

    // Kapali sonucu ve kararli ornek listesini tek sonuc modelinde kurar.
    public SampleSetResult(string outcomeCode, List<FieldSample> samples)
    {
        OutcomeCode = outcomeCode;
        Samples = samples;
    }
}
