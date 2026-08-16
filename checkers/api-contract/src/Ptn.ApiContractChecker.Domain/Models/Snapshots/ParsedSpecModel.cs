namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Ayristirilmis spec dokumanindan kalicilik ve karsilastirma icin gereken kararli bilgileri tasir.
// sistemdeki gorevi: Ayristirici kutuphane tipini domaine sokmadan format, surum, kanonik metin ve yapisal modeli tek sonucta verir.
public class ParsedSpecModel
{
    // Okunan dokumanin SpecFormatCodes ile eslesen kararli format kodu.
    public string FormatCode { get; }

    // Spec'in kendi info.version degeri; dokumanda bulunmayabilir.
    public string? ApiVersion { get; }

    // Bicim gurultusunden arindirilmis, ayni girdi icin ayni ciktiyi veren kanonik metin.
    public string CanonicalText { get; }

    // Karsilastirma zincirine girecek provider-bagimsiz operasyon ve sema fotografi.
    public SpecSnapshotModel Snapshot { get; }

    // Ayristirma sonucunu tek seferde kurar; alanlar sonradan degistirilemez.
    public ParsedSpecModel(
        string formatCode,
        string? apiVersion,
        string canonicalText,
        SpecSnapshotModel snapshot)
    {
        FormatCode = formatCode;
        ApiVersion = apiVersion;
        CanonicalText = canonicalText;
        Snapshot = snapshot;
    }
}
