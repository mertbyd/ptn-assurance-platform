using System;
using Ptn.DatabaseChecker.Constants.Comparison;

namespace Ptn.DatabaseChecker.Dtos.Definitions;

// islevi: Yeni karsilastirma tarifi olusturma istegidir.
// sistemdeki gorevi: Display alanlari tasimadan kaynak/hedef baglanti ve karsilastirma modunu alir.
public class CreateComparisonDefinitionDto
{
    /// <summary>
    /// Tarifin kiraci icindeki benzersiz adi.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Karsilastirmanin kaynak tarafindaki baglanti kimligi.
    /// </summary>
    public Guid SourceConnectionId { get; set; }

    /// <summary>
    /// Karsilastirmanin hedef tarafindaki baglanti kimligi.
    /// </summary>
    public Guid TargetConnectionId { get; set; }

    /// <summary>
    /// Kaynak baglantinin Reference veya Audited rolu.
    /// </summary>
    public string SourceRoleCode { get; set; } = ComparisonSideRoleCodes.Reference;

    /// <summary>
    /// Karsilastirma modu lookup kimligi.
    /// </summary>
    public Guid ComparisonTypeId { get; set; }

    /// <summary>
    /// Tarifin opsiyonel aciklamasi.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Yeni tarifin baslangic aktiflik durumu.
    /// </summary>
    public bool IsActive { get; set; }
}
