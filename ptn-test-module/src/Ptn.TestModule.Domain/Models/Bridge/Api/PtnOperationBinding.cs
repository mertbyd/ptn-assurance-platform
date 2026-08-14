using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Normalize edilmis operasyon baglama sonucu ve skorlanmis onerileri tasir.
// sistemdeki gorevi: API checker sonucunu ham outcome casing'i veya DTO tipi sizdirmadan domaine verir.
public sealed class PtnOperationBinding
{
    public string OutcomeCode { get; set; } = string.Empty;
    public List<PtnOperationSuggestion> Suggestions { get; set; } = [];

    // islevi: Tek kaynak operasyon adayi ile alan baglarini ve mekanik skorunu tasir.
    // sistemdeki gorevi: Grounding manager'in esik kararini checker DTO'sundan bagimsiz yapar.
    public sealed class PtnOperationSuggestion
    {
        public string? SourceOperationId { get; set; }
        public string SourceMethod { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public int Score { get; set; }
        public List<PtnFieldBinding> Bindings { get; set; } = [];
    }

    // islevi: Kaynak ve hedef JSON pointer arasindaki tek mekanik alan bagini tasir.
    // sistemdeki gorevi: Ajanin serbest alan adi yazmasi yerine checker tarafindan onerilen referansi tasir.
    public sealed class PtnFieldBinding
    {
        public string SourcePointer { get; set; } = string.Empty;
        public string TargetPointer { get; set; } = string.Empty;
        public string? TypeCode { get; set; }
        public int Score { get; set; }
        public string Expression { get; set; } = string.Empty;
    }
}
