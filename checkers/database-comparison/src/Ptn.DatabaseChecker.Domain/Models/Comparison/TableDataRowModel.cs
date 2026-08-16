using System;
using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Tek data-comparison satirinin kolon degerlerini provider-notr kanonik metinlerle tasir.
// sistemdeki gorevi: PK, row hash ve cell diff hesaplari kolon adlarini case-insensitive esler; null gercek DB NULL anlamini korur.
public class TableDataRowModel
{
    // Kolon adi -> kanonik metin/null deger sozlugu.
    public Dictionary<string, string?> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
