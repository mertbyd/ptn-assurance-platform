namespace Ptn.ApiContractChecker.Constants.Conformance;

// islevi: Deger saklama bicimlerinin kararli maske ve hash ayirici sabitlerini tanimlar.
// sistemdeki gorevi: Redactor ciktisinin kod icinde daginik literal veya belirsiz null hash'i uretmesini engeller.
public static class ValueRetentionConstants
{
    public const byte NullHashDiscriminator = 0;
    public const byte ValueHashDiscriminator = 1;
    public const string MaskMarker = "***";
    public const int TransportSerializationMarginBytes = 128;
}
