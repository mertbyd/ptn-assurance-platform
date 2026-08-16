namespace Ptn.ApiContractChecker.Constants.Conformance.Lookups;

// islevi: Senaryo assertion yolunun sozlesmeden turetilebilirlik durumlarini tanimlar.
// sistemdeki gorevi: G2 kapisinin metinden bagimsiz ve kapali sonuc katalogudur.
public static class AssertionDerivabilityCodes
{
    public const string Derivable = "Derivable";
    public const string AssertionNotInContract = "AssertionNotInContract";
    public const string DerivableButOptional = "DerivableButOptional";
    public static readonly IReadOnlyCollection<string> All =
        [Derivable, AssertionNotInContract, DerivableButOptional];
}
