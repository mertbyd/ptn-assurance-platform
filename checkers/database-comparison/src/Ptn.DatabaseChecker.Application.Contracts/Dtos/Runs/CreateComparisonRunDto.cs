using System;

namespace Ptn.DatabaseChecker.Dtos.Runs;

// islevi: Yeni karsilastirma run kaydi olusturma istegidir.
// sistemdeki gorevi: Tarifli veya ad-hoc calistirma icin snapshot FK'lerini alir; sonuc sayaclari motor tarafindan yazilir.
public class CreateComparisonRunDto
{
    /// <summary>
    /// Opsiyonel tarif kimligi; null ise ad-hoc calistirmadir.
    /// </summary>
    public Guid? ComparisonDefinitionId { get; set; }

    /// <summary>
    /// Referans baglanti kimligi.
    /// </summary>
    public Guid SourceConnectionId { get; set; }

    /// <summary>
    /// Hedef baglanti kimligi.
    /// </summary>
    public Guid TargetConnectionId { get; set; }

    /// <summary>
    /// Karsilastirma modu lookup kimligi.
    /// </summary>
    public Guid ComparisonTypeId { get; set; }

    /// <summary>
    /// Run durum lookup kimligi.
    /// </summary>
    public Guid StatusId { get; set; }
}
