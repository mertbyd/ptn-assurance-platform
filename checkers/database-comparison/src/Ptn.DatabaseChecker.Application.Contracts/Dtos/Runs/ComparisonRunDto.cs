using System;
using Volo.Abp.Application.Dtos;

namespace Ptn.DatabaseChecker.Dtos.Runs;

// islevi: Karsilastirma calistirma kaydinin API cevap modelidir.
// sistemdeki gorevi: Run snapshot FK'lerinin yaninda tarif/baglanti/mod/durum companion alanlarini tasir.
public class ComparisonRunDto : EntityDto<Guid>
{
    /// <summary>
    /// Run'in dogdugu opsiyonel tarif kimligi; null ise ad-hoc calistirmadir.
    /// </summary>
    public Guid? ComparisonDefinitionId { get; set; }

    /// <summary>
    /// Run'in dogdugu opsiyonel tarif adi.
    /// </summary>
    public string? ComparisonDefinitionName { get; set; }

    /// <summary>
    /// Run snapshot'indaki referans baglanti kimligi.
    /// </summary>
    public Guid SourceConnectionId { get; set; }

    /// <summary>
    /// Run snapshot'indaki referans baglanti adi.
    /// </summary>
    public string SourceConnectionName { get; set; } = default!;

    /// <summary>
    /// Run snapshot'indaki hedef baglanti kimligi.
    /// </summary>
    public Guid TargetConnectionId { get; set; }

    /// <summary>
    /// Run snapshot'indaki hedef baglanti adi.
    /// </summary>
    public string TargetConnectionName { get; set; } = default!;

    /// <summary>
    /// Run snapshot'indaki karsilastirma modu lookup kimligi.
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
    /// Run durum lookup kimligi.
    /// </summary>
    public Guid StatusId { get; set; }

    /// <summary>
    /// Run durumunun kararli teknik kodu.
    /// </summary>
    public string StatusCode { get; set; } = default!;

    /// <summary>
    /// Run durumunun insan-okur adi.
    /// </summary>
    public string StatusName { get; set; } = default!;

    /// <summary>
    /// Run'in fiilen basladigi zaman.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Run'in tamamlandigi zaman.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Basarisiz run icin kullaniciya gosterilecek hata ozeti.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Run sonunda bulunan sema farki sayisi.
    /// </summary>
    public int SchemaDifferenceCount { get; set; }

    /// <summary>
    /// Run sonunda bulunan veri farki sayisi.
    /// </summary>
    public int DataDifferenceCount { get; set; }

    /// <summary>
    /// Run sonunda bulunan migration farki sayisi.
    /// </summary>
    public int MigrationDifferenceCount { get; set; }

    /// <summary>
    /// Run kaydinin olusturulma zamani.
    /// </summary>
    public DateTime CreationTime { get; set; }
}
