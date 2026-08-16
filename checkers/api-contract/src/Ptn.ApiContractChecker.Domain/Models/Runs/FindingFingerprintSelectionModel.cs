namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: Manager'in bir degisim durumu icin sectigi fingerprint kumesini repository'ye tasir.
// sistemdeki gorevi: Degisim kararini manager'da, bu kararin SQL filtresini repository'de tutar.
public class FindingFingerprintSelectionModel
{
    public IReadOnlyCollection<string> Fingerprints { get; init; } = [];
    public bool IncludeMissingFingerprint { get; init; }
}
