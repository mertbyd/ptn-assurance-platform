namespace Ptn.DatabaseChecker.Constants.Diagnosis;

// islevi: Salt-okuma probe turlerini ve yapilandirilmis probe sonuc olgularini kararli kodlarla tanimlar.
// sistemdeki gorevi: Kurallarin probe implementasyonuna veya aciklama metnine baglanmadan kanit istemesini ve yorumlamasini saglar.
public static class ProbeKindCodes
{
    public const string RowExists = "RowExists";
    public const string KeyMatchCount = "KeyMatchCount";
    public const string ServerSetting = "ServerSetting";

    // islevi: Probe sonucunun bulundu, bulunmadi, eslesti, farkli veya katalog olgusu kodlarini gruplar.
    // sistemdeki gorevi: Kurallarin kaniti aciklama metni veya bool anlam tahmini olmadan yorumlamasini saglar.
    public static class Facts
    {
        public const string Found = "Found";
        public const string Missing = "Missing";
        public const string Matches = "Matches";
        public const string Mismatch = "Mismatch";
        public const string Catalog = "Catalog";
    }
}
