namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Karsilastirma motorunun kanonik anahtar, imza, sentinel ve escape metinlerini tek kaynakta tanimlar.
// sistemdeki gorevi: Schema/data/scope/repository katmanlarinin ayni dahili metin protokolunu farkli magic string'lerle uretmesini engeller.
public static class ComparisonCanonicalTextConstants
{
    // Sema, tablo ve nesne anahtar parcalarinin kararli ayiraci.
    public const string KeySeparator = "|";

    // Kanonik definition alanlarini ayiran token.
    public const string DefinitionFieldSeparator = ";";

    // Kanonik definition alan adi ile degerini ayiran token.
    public const string DefinitionKeyValueSeparator = "=";

    // Degisen alanlar ozetindeki etiketleri ayiran token.
    public const string ChangeSummarySeparator = ", ";

    // SQL Server sistem adindaki tur oneki ile rastgele suffix'i ayiran token.
    public const string SystemGeneratedNameSeparator = "__";

    // Kolonlardan uretilen yapisal imza parcalarini ayiran token.
    public const string SignaturePartSeparator = "_";

    // Null anahtar parcasini gercek metin degerlerinden ayiran sentinel.
    public const string NullKeyPlaceholder = "<NULL>";

    // Null hucre degerinin hash-oncesi tip isareti.
    public const string NullValueMarker = "N";

    // Dolu hucre degerinin hash-oncesi tip isareti.
    public const string ValueMarker = "V";

    // Composite key escape karakteri.
    public const string EscapeCharacter = "\\";

    // Composite key icinde escape karakterinin geri-donulebilir temsili.
    public const string EscapedEscapeCharacter = "\\\\";

    // Composite key ayiracinin geri-donulebilir temsili.
    public const string EscapedKeySeparator = "\\|";

    // Tablo hash girdisinde satirlari ayiran token.
    public const string RowSeparator = "\n";

    // Satir hash girdisinde kolon/deger alanlarini ayiran record-separator token'i.
    public const string FieldSeparator = "\u001e";

    // Ayni hash'e sahip duplicate satirlarin occurrence numarasini ayiran token.
    public const string OccurrenceSeparator = "#";

    // Uzunluk-etiketli kanonik parcalarda uzunluk ile degeri ayiran token.
    public const string LengthSeparator = ":";
}
