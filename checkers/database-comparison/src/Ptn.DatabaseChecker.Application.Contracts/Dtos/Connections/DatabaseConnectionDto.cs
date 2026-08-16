using System;
using Volo.Abp.Application.Dtos;

namespace Ptn.DatabaseChecker.Dtos.Connections;

// islevi: Kayitli veritabani baglantisinin API cevap modelidir.
// sistemdeki gorevi: EngineId yaninda EngineCode/EngineName tasiyarak FE'nin lookup icin ek sorgu atmasini engeller.
public class DatabaseConnectionDto : EntityDto<Guid>
{
    /// <summary>
    /// Veritabani motoru lookup kimligi.
    /// </summary>
    public Guid EngineId { get; set; }

    /// <summary>
    /// Veritabani motorunun kararli teknik kodu.
    /// </summary>
    public string EngineCode { get; set; } = default!;

    /// <summary>
    /// Veritabani motorunun insan-okur adi.
    /// </summary>
    public string EngineName { get; set; } = default!;

    /// <summary>
    /// Baglantinin kiraci icindeki insan-okur takma adi.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Veritabani sunucusunun host adi veya IP adresi.
    /// </summary>
    public string Host { get; set; } = default!;

    /// <summary>
    /// Veritabani sunucusunun TCP portu.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Sunucudaki hedef veritabani adi.
    /// </summary>
    public string DatabaseName { get; set; } = default!;

    /// <summary>
    /// Baglantinin kararli TLS politika kodu.
    /// </summary>
    public string TlsModeCode { get; set; } = default!;

    /// <summary>
    /// Sunucu sertifika zinciri dogrulamasinin acikca atlanip atlanmadigi.
    /// </summary>
    public bool TrustServerCertificate { get; set; }

    /// <summary>
    /// Baglantinin yeni karsilastirmalarda secilebilir olup olmadigi.
    /// </summary>
    public bool IsActive { get; set; }
}
