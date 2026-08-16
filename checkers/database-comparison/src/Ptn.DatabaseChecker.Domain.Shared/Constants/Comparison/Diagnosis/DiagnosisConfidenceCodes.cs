namespace Ptn.DatabaseChecker.Constants.Diagnosis;

// islevi: Kimlik guveni ile hipotez sonuc guvenlerinin kararli kodlarini ve siralama agirligini tanimlar.
// sistemdeki gorevi: Confirmed-Likely-Possible-RuledOut siralamasini metinden ve koleksiyon gelis sirasindan bagimsiz yapar.
public static class DiagnosisConfidenceCodes
{
    public const string High = "High";
    public const string Low = "Low";
    public const string Confirmed = "Confirmed";
    public const string Likely = "Likely";
    public const string Possible = "Possible";
    public const string RuledOut = "RuledOut";

    // islevi: Hipotez guvenini artan siralama agirligina cevirir.
    public static int Rank(string confidenceCode)
        => confidenceCode switch
        {
            Confirmed => 0,
            Likely => 1,
            Possible => 2,
            RuledOut => 3,
            _ => 4
        };
}
