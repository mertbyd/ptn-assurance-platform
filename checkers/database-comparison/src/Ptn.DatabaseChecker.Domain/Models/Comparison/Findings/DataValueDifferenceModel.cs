namespace Ptn.DatabaseChecker.Models.Comparison.Findings;

// islevi: Modified bir satirda degisen tek bir hucre; "hangi hucre, neyden neye?" sorusunun verisi (mikroskop seviyesi).
// sistemdeki gorevi: Eski DataValueDifference tablosunun owned-JSON karsiligi; DataRowDifferenceModel.ValueDifferences icinde tasinir. Zincirin son halkasi.
public class DataValueDifferenceModel
{
    // Degisen hucrenin kolon adi.
    public string ColumnName { get; set; } = default!;

    // Kaynaktaki deger (kanonik metin temsili); null = o hucre kaynakta NULL (satir var, deger NULL - o da farktir).
    public string? SourceValue { get; set; }

    // Hedefteki deger (kanonik metin temsili); null = o hucre hedefte NULL.
    public string? TargetValue { get; set; }
}
