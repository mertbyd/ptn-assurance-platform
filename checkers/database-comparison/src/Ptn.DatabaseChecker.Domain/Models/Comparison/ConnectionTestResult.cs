namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Bir baglanti testinin erisim, sunucu surumu ve en az yetki bulgularini tasir.
// sistemdeki gorevi: Connection tester bunu doldurur, AppService DTO'ya cevirir. Basarisizlik istisna degil sonuctur; fazla yetki de hata degil uyaridir.
public class ConnectionTestResult
{
    // Baglanti acilabildi mi.
    public bool Succeeded { get; set; }

    // Basariliysa hedef sunucunun surum bilgisi; basarisizsa null.
    public string? ServerVersion { get; set; }

    // Basarisizsa insan-okur hata mesaji; basariliysa null.
    public string? Message { get; set; }

    // Probe edilen kimlik hedefte genis yazma/olusturma yetkisi tasiyor mu.
    public bool CanWrite { get; set; }

    // Probe edilen kimlik motorun superuser/sysadmin rolunde mi.
    public bool IsSuperUser { get; set; }

    // Fazla yetki bulunduysa kararli rapor kodu; baglanti yine basarili sayilir.
    public string? PrivilegeWarningCode { get; set; }
}
