namespace Ptn.ApiContractChecker.Constants.Conformance.Lookups;

// islevi: Uretilen ornegin hedef API tarafindan beklenen kabul veya ret sonucunu tanimlar.
// sistemdeki gorevi: Sinir degerini negatif vakadan ayirirken beklenen yargiyi kapali sozlesmede tutar.
public static class SampleExpectedOutcomeCodes
{
    public const string ShouldAccept = "ShouldAccept";
    public const string ShouldReject = "ShouldReject";

    public static IReadOnlyCollection<string> All { get; } = [ShouldAccept, ShouldReject];
}
