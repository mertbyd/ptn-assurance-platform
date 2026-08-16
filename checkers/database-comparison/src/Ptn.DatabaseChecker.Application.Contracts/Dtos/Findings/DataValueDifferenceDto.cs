namespace Ptn.DatabaseChecker.Dtos.Findings;

// islevi: Modified bir satirda degisen tek bir hucrenin API modelidir.
// sistemdeki gorevi: DataValueDifferenceModel'in cevap karsiligi; DataRowDifferenceDto.ValueDifferences icinde tasinir.
public class DataValueDifferenceDto
{
    /// <summary>
    /// Degisen hucrenin kolon adi.
    /// </summary>
    public string ColumnName { get; set; } = default!;

    /// <summary>
    /// Kaynaktaki deger (kanonik metin temsili); null = hucre kaynakta NULL.
    /// </summary>
    public string? SourceValue { get; set; }

    /// <summary>
    /// Hedefteki deger (kanonik metin temsili); null = hucre hedefte NULL.
    /// </summary>
    public string? TargetValue { get; set; }
}
