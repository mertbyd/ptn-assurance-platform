using System;
using Ptn.DatabaseChecker.Constants.Comparison;

namespace Ptn.DatabaseChecker.Dtos.Connections;

// islevi: Veritabani baglantisi guncelleme istegidir.
// sistemdeki gorevi: Adres alanlarini gunceller; kimlik bilgisi opsiyoneldir - yalnizca Password verilirse secret Vault'ta yeniden yazilir, aksi halde mevcut secret'a dokunulmaz.
public class UpdateDatabaseConnectionDto
{
    /// <summary>
    /// Veritabani motoru lookup kimligi.
    /// </summary>
    public Guid EngineId { get; set; }

    /// <summary>
    /// Baglantinin kiraci icindeki benzersiz takma adi.
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
    /// Yeni veritabani kullanici adi. Yalnizca Password ile birlikte anlamlidir; secret yeniden yazilirken kullanilir.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Yeni veritabani sifresi. Bos birakilirsa mevcut secret degismez; doluysa Vault'taki secret yeniden yazilir. Cevapta/log'da gorunmez.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Baglantinin kararli TLS politika kodu (Require, Prefer veya Disable).
    /// </summary>
    public string TlsModeCode { get; set; } = TlsModeCodes.Require;

    /// <summary>
    /// Yalnizca acikca secildiginde sunucu sertifika zinciri dogrulamasini atlar.
    /// </summary>
    public bool TrustServerCertificate { get; set; }

    /// <summary>
    /// Baglantinin yeni aktiflik durumu.
    /// </summary>
    public bool IsActive { get; set; }
}
