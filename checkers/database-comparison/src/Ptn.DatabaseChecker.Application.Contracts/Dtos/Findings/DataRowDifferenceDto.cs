using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Dtos.Findings;

// islevi: Fark bulunan tek bir satirin API modelidir.
// sistemdeki gorevi: DataRowDifferenceModel'in cevap karsiligi; DataDifferenceDto.RowDifferences icinde, degisen hucreleri ic ice tasir.
public class DataRowDifferenceDto
{
    /// <summary>
    /// Farkli satirin kimligi (kanonik metin; "5" veya "5|2026").
    /// </summary>
    public string PrimaryKeyValue { get; set; } = default!;

    /// <summary>
    /// Fark yonunun kararli kodu (DifferenceKindCodes.*).
    /// </summary>
    public string KindCode { get; set; } = default!;

    /// <summary>
    /// Degisen hucreler; yalnizca Modified satirlarda dolar.
    /// </summary>
    public List<DataValueDifferenceDto> ValueDifferences { get; set; } = new();
}
