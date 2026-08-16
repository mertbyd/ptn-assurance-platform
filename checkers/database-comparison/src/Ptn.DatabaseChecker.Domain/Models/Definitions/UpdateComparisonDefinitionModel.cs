using System;
using Ptn.DatabaseChecker.Constants.Comparison;

namespace Ptn.DatabaseChecker.Models.Definitions;

// islevi: Karsilastirma tarifinin guncellemesinde degistirilebilir alanlari tasir.
// sistemdeki gorevi: Tarifin ad, baglanti, mod, aciklama ve aktiflik degisikliklerini manager dogrulamasina tasir.
public class UpdateComparisonDefinitionModel
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

    // Karsilastirma modu lookup kimligi (Guid lookup Id).
    public Guid ComparisonTypeId { get; set; }

    // Tarif aciklamasi.
    public string? Description { get; set; }

    // Aktiflik durumu (emekliye ayirma / geri alma).
    public bool IsActive { get; set; }
}
