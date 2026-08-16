namespace Ptn.ApiContractChecker.Constants.Diagnosis;

// islevi: Kimlik ve hipotez guven kodlarini kararli siraya cevirir.
// sistemdeki gorevi: Siralamayi lokalize metinden ve DI koleksiyon gelis sirasindan bagimsiz tutar.
public static class DiagnosisConfidenceCodes
{
    public const string High = "High";
    public const string Low = "Low";
    public const string Confirmed = "Confirmed";
    public const string Likely = "Likely";
    public const string Possible = "Possible";
    public const string RuledOut = "RuledOut";

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
