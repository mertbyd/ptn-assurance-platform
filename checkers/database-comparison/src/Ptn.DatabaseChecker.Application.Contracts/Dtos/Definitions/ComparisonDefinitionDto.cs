using System;
using Volo.Abp.Application.Dtos;

namespace Ptn.DatabaseChecker.Dtos.Definitions;

// islevi: Kayitli karsilastirma tarifinin API cevap modelidir.
// sistemdeki gorevi: Baglanti ve mod FK'lerinin yaninda insan-okur companion alanlarini tasir.
public class ComparisonDefinitionDto : EntityDto<Guid>
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
    /// Karsilastirmanin kaynak tarafindaki baglantinin insan-okur adi.
    /// </summary>
    public string SourceConnectionName { get; set; } = default!;

    /// <summary>
    /// Karsilastirmanin hedef tarafindaki baglanti kimligi.
    /// </summary>
    public Guid TargetConnectionId { get; set; }

    /// <summary>
    /// Karsilastirmanin hedef tarafindaki baglantinin insan-okur adi.
    /// </summary>
    public string TargetConnectionName { get; set; } = default!;

    /// <summary>
    /// Kaynak baglantinin Reference veya Audited rolu; hedef baglanti karsi roldedir.
    /// </summary>
    public string SourceRoleCode { get; set; } = default!;

    /// <summary>
    /// Karsilastirma modu lookup kimligi.
    /// </summary>
    public Guid ComparisonTypeId { get; set; }

    /// <summary>
    /// Karsilastirma modunun kararli teknik kodu.
    /// </summary>
    public string ComparisonTypeCode { get; set; } = default!;

    /// <summary>
    /// Karsilastirma modunun insan-okur adi.
    /// </summary>
    public string ComparisonTypeName { get; set; } = default!;

    /// <summary>
    /// Tarifin opsiyonel aciklamasi.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Tarifin yeni calistirmalarda secilebilir olup olmadigi.
    /// </summary>
    public bool IsActive { get; set; }
}
