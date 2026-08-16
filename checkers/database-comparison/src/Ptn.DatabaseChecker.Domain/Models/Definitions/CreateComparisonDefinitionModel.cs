using System;
using Ptn.DatabaseChecker.Constants.Comparison;

namespace Ptn.DatabaseChecker.Models.Definitions;

// islevi: Yeni karsilastirma tarifi icin manager'in ihtiyac duydugu alanlari tasir.
// sistemdeki gorevi: ComparisonDefinitionManager ad benzersizligi ve baglanti/mod FK varliklarini bu model uzerinden dogrular.
public class CreateComparisonDefinitionModel
{
    // Tarifin insan-okur adi; kiraci icinde benzersiz.
    public string Name { get; set; } = default!;

    // Referans ortam baglantisinin kimligi.
    public Guid SourceConnectionId { get; set; }

    // Denetlenen ortam baglantisinin kimligi.
    public Guid TargetConnectionId { get; set; }

    /// <summary>
    /// SourceConnection tarafinin Reference veya Audited rolu.
    /// </summary>
    public string SourceRoleCode { get; set; } = ComparisonSideRoleCodes.Reference;

    // Karsilastirma modu lookup kimligi (Guid lookup Id): SchemaOnly / DataOnly / Both.
    public Guid ComparisonTypeId { get; set; }

    // Tarifin baglamini anlatan serbest metin.
    public string? Description { get; set; }

    // Kaydin baslangic aktiflik durumu.
    public bool IsActive { get; set; }
}
