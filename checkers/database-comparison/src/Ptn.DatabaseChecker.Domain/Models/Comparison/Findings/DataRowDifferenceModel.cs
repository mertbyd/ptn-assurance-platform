using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Models.Comparison.Findings;

// islevi: Fark bulunan tek bir satir; motor iki taraftaki satirlari PK uzerinden eslestirir ve yonunu yazar (kademe 3).
// sistemdeki gorevi: Eski DataRowDifference tablosunun owned-JSON karsiligi; DataDifferenceModel.RowDifferences icinde tasinir, degisen hucreleri (ValueDifferences) ic ice tasir.
public class DataRowDifferenceModel
{
    // Farkli satirin kimligi; PK her tabloda baska tipte oldugu icin tek kanonik serilestirme kuraliyla metne cevrilir ("5" veya "5|2026").
    public string PrimaryKeyValue { get; set; } = default!;

    // Fark yonunun kararli kodu (DifferenceKindCodes.*): OnlyInSource / OnlyInTarget / Modified.
    public string KindCode { get; set; } = default!;

    // Degisen hucreler; yalnizca Kind=Modified satirlarda dolar (OnlyIn* satirlarda karsilastirilacak cift yok).
    public List<DataValueDifferenceModel> ValueDifferences { get; set; } = new();
}
