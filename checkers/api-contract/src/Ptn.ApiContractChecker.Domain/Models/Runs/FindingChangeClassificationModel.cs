using Ptn.ApiContractChecker.Constants.Runs.Lookups;

namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: Mevcut ve onceki run fingerprint kumelerinin New, Known, Resolved ve Unknown ayrimini tasir.
// sistemdeki gorevi: Sayfa satiri siniflandirmasi ile changeState repository filtresinin ayni manager kararini kullanmasini saglar.
public class FindingChangeClassificationModel
{
    public HashSet<string> NewFingerprints { get; } = new(StringComparer.Ordinal);
    public HashSet<string> KnownFingerprints { get; } = new(StringComparer.Ordinal);
    public HashSet<string> ResolvedFingerprints { get; } = new(StringComparer.Ordinal);
    public bool HasPreviousRun { get; init; }

    // islevi: Nullable fingerprint'i geriye uyumlu degisim durumuna cevirir.
    public string Classify(string? fingerprint)
    {
        // Eski owned JSON satirlarinda fingerprint null olabilir; bu satirlar asla New sayilmaz.
        if (fingerprint is null)
        {
            return FindingChangeStateCodes.Unknown;
        }

        return KnownFingerprints.Contains(fingerprint)
            ? FindingChangeStateCodes.Known
            : FindingChangeStateCodes.New;
    }

    // islevi: Secilen changeState kodunu repository'nin uygulayacagi fingerprint filtresine cevirir.
    public FindingFingerprintSelectionModel? Select(string? changeStateCode)
    {
        return changeStateCode switch
        {
            null => null,
            FindingChangeStateCodes.New => Build(NewFingerprints),
            FindingChangeStateCodes.Known => Build(KnownFingerprints),
            FindingChangeStateCodes.Resolved => Build(ResolvedFingerprints),
            FindingChangeStateCodes.Unknown => new FindingFingerprintSelectionModel
            {
                IncludeMissingFingerprint = true
            },
            _ => throw new ArgumentOutOfRangeException(nameof(changeStateCode))
        };
    }

    // islevi: Manager kumesini repository kontrati icin salt-okunur secime kopyalar.
    private static FindingFingerprintSelectionModel Build(IReadOnlyCollection<string> fingerprints)
        => new() { Fingerprints = fingerprints };
}
