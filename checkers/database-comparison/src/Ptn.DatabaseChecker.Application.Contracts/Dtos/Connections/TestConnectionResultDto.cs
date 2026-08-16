namespace Ptn.DatabaseChecker.Dtos.Connections;

// islevi: Baglanti testi ve en az yetki probe'u sonucunun API cevap modeli.
// sistemdeki gorevi: test-connection ucu bunu doner; kimlik/sifre icermez, erisim durumu ile fazla yetki bulgusunu birlikte raporlar.
public class TestConnectionResultDto
{
    /// <summary>
    /// Hedefe baglanilabildi mi.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Basariliysa hedef sunucunun surum bilgisi.
    /// </summary>
    public string? ServerVersion { get; set; }

    /// <summary>
    /// Basarisizsa insan-okur hata mesaji.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Hedef kimlik genis yazma/olusturma yetkisi tasiyor mu.
    /// </summary>
    public bool CanWrite { get; set; }

    /// <summary>
    /// Hedef kimlik superuser veya sysadmin rolunde mi.
    /// </summary>
    public bool IsSuperUser { get; set; }

    /// <summary>
    /// Fazla yetki bulunduysa kararli uyari kodu; baglanti testi yine basarili olabilir.
    /// </summary>
    public string? PrivilegeWarningCode { get; set; }
}
