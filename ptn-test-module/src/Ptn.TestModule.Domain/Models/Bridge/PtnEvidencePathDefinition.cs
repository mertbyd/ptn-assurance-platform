using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Tetikleyici, sirali adimlar ve kapali hukum ifadelerinden olusan kanit yolunu tasir.
// sistemdeki gorevi: Yeni teshis akisini yeni C# dali yerine profil verisi olarak eklenebilir yapar.
public sealed class PtnEvidencePathDefinition
{
    public string PathKey { get; set; } = string.Empty;
    public PtnEvidencePathTrigger Trigger { get; set; } = new();
    public List<PtnEvidencePathStep> Steps { get; set; } = [];
    public string ConfirmedWhen { get; set; } = string.Empty;
    public string InconclusiveWhen { get; set; } = string.Empty;

    // islevi: Kanit yolunun kapali HTTP ve operasyon tetikleyicilerini tasir.
    // sistemdeki gorevi: Yol secimini serbest ifade veya hard-coded vaka dalindan uzak tutar.
    public sealed class PtnEvidencePathTrigger
    {
        public List<int> StatusCodes { get; set; } = [];
        public List<string> OperationIds { get; set; } = [];
    }

    // islevi: Kanit yolundaki tek deterministik probe adiminin veri tanimini tasir.
    // sistemdeki gorevi: Dugum kaynagi, kavrami ve onceki dugum bagini sirali yurutmeye verir.
    public sealed class PtnEvidencePathStep
    {
        public string NodeKindCode { get; set; } = string.Empty;
        public string SourceCode { get; set; } = string.Empty;
        public string? ConceptCode { get; set; }
        public string? JoinFromNodeKindCode { get; set; }
        public Dictionary<string, string?> Parameters { get; set; } = new(StringComparer.Ordinal);
    }
}
