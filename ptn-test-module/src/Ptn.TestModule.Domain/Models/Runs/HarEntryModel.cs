namespace Ptn.TestModule.Models.Runs;

// islevi: HAR'daki tek bir istek/yanit cifti ile onun cozulmus adim kimligini tasir.
// sistemdeki gorevi: Entry ile senaryo adimi arasindaki bagi konumla degil StepKey ile kurar (ADR-0021).
/// <summary>
/// Bir HAR entry'sinin uygunluk kontrolu icin gereken alanlarini tasir.
/// </summary>
public class HarEntryModel
{
    /// <summary>Korelasyon echo'sundan cozulen adim kimligidir; cozulemezse null kalir.</summary>
    public string? StepKey { get; set; }

    /// <summary>Entry'nin HAR icindeki bir tabanli konumudur; yalniz raporlama icindir.</summary>
    public int Ordinal { get; set; }

    /// <summary>Istegin HTTP metodudur.</summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>Istegin tam adresidir.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Istek govdesinin content-type basligidir.</summary>
    public string? RequestContentType { get; set; }

    /// <summary>Maskelenmis istek govdesinin metin temsilidir.</summary>
    public string? RequestBody { get; set; }

    /// <summary>Yanitin HTTP durum kodudur.</summary>
    public int StatusCode { get; set; }

    /// <summary>Yanit govdesinin content-type basligidir.</summary>
    public string? ResponseContentType { get; set; }

    /// <summary>Maskelenmis yanit govdesinin metin temsilidir.</summary>
    public string? ResponseBody { get; set; }

    /// <summary>Entry'nin kosum baslangicina gore milisaniye konumudur.</summary>
    public int? StartedAtMs { get; set; }

    /// <summary>Istegin toplam suresidir.</summary>
    public int TimeMs { get; set; }

    /// <summary>Entry'nin derlenmis bir Database Checker assertion adimi olup olmadigidir.</summary>
    public bool IsDatabaseAssertion { get; set; }
}
