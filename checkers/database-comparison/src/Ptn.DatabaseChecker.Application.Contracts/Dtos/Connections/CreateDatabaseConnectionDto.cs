using System;
using Ptn.DatabaseChecker.Constants.Comparison;

namespace Ptn.DatabaseChecker.Dtos.Connections;

// islevi: Yeni veritabani baglantisi olusturma istegidir.
// sistemdeki gorevi: Kimlik bilgisini (kullanici adi + sifre) alir; AppService bunlari Vault'a yazar, DB'ye yalnizca hesaplanmis vault_secret_path gider. Sifre asla cevap/log'a donmez.
public class CreateDatabaseConnectionDto
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
    /// Veritabani kullanici adi (tercihen read-only). Vault'a yazilir, DB'de tutulmaz.
    /// </summary>
    public string Username { get; set; } = default!;

    /// <summary>
    /// Veritabani sifresi. Yalnizca Vault'a yazilir; hicbir cevapta/log'da gorunmez, DB'de tutulmaz.
    /// </summary>
    public string Password { get; set; } = default!;

    /// <summary>
    /// Baglantinin kararli TLS politika kodu (Require, Prefer veya Disable).
    /// </summary>
    public string TlsModeCode { get; set; } = TlsModeCodes.Require;

    /// <summary>
    /// Yalnizca acikca secildiginde sunucu sertifika zinciri dogrulamasini atlar.
    /// </summary>
    public bool TrustServerCertificate { get; set; }

    /// <summary>
    /// Yeni baglantinin baslangic aktiflik durumu.
    /// </summary>
    public bool IsActive { get; set; }
}
