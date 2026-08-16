namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: DescribeTable FK komsularinin hedef tablodan disari veya hedef tabloya dogru yonunu tanimlar.
// sistemdeki gorevi: Senaryo yazim araci provider'a ozel FK katalog kodu bilmeden bir seviye komsuluk yonunu yorumlar.
public static class ForeignKeyDirectionCodes
{
    public const string Outgoing = "Outgoing";
    public const string Incoming = "Incoming";
}
