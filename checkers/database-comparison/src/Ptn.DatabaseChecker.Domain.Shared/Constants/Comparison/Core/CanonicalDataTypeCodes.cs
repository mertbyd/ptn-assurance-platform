namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Veritabani motorlarinin ham kolon tiplerini indirgeyecegi kapali kanonik tip ailesini sabitler.
// sistemdeki gorevi: Discovery, capraz-motor karsilastirma ve guven siniflandirmasi ayni motor-bagimsiz tip kodlarini paylasir.
public static class CanonicalDataTypeCodes
{
    public const string String = "String";
    public const string Text = "Text";
    public const string Integer = "Integer";
    public const string SmallInteger = "SmallInteger";
    public const string BigInteger = "BigInteger";
    public const string Decimal = "Decimal";
    public const string Float = "Float";
    public const string Double = "Double";
    public const string Boolean = "Boolean";
    public const string Date = "Date";
    public const string Time = "Time";
    public const string Timestamp = "Timestamp";
    public const string TimestampWithTimeZone = "TimestampWithTimeZone";
    public const string Interval = "Interval";
    public const string Uuid = "Uuid";
    public const string Binary = "Binary";
    public const string Json = "Json";
    public const string Xml = "Xml";
    public const string Money = "Money";
    public const string Enum = "Enum";
    public const string Array = "Array";
    public const string Network = "Network";
    public const string Geometry = "Geometry";
    public const string Unknown = "Unknown";
}
