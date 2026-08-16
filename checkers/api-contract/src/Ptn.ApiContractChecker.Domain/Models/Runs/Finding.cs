using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Volo.Abp;

namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: Iki spec snapshot'i arasindaki tek yonlu ve siddetlendirilmis sozlesme farkini tasir.
// sistemdeki gorevi: ContractCheckRun owned JSON govdesindeki atomik rapor satiridir.
public class Finding
{
    public string KindCode { get; private set; } = default!;
    public string SeverityCode { get; private set; } = default!;
    public string DirectionCode { get; private set; } = default!;
    public FindingAddress Address { get; private set; } = default!;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public string? Fingerprint { get; private set; }

    // EF Core JSON materializasyonu icin parametresiz ctor.
    protected Finding()
    {
    }

    // 0.1.x cagiranlari icin eski ctor imzasini korur; fingerprint yeni kurulum yolunda opsiyoneldir.
    public Finding(
        string kindCode,
        string severityCode,
        string directionCode,
        FindingAddress address,
        string? oldValue = null,
        string? newValue = null)
        : this(kindCode, severityCode, directionCode, address, oldValue, newValue, null)
    {
    }

    // Bulguyu kapali kataloglar ve varsa onceden hesaplanmis fingerprint ile kurar.
    public Finding(
        string kindCode,
        string severityCode,
        string directionCode,
        FindingAddress address,
        string? oldValue,
        string? newValue,
        string? fingerprint)
    {
        KindCode = EnsureKnown(kindCode, DifferenceKindCodes.All, nameof(kindCode));
        SeverityCode = EnsureKnown(severityCode, DifferenceSeverityCodes.All, nameof(severityCode));
        DirectionCode = EnsureKnown(directionCode, DifferenceDirectionCodes.All, nameof(directionCode));
        Address = Check.NotNull(address, nameof(address));
        OldValue = oldValue;
        NewValue = newValue;
        Fingerprint = fingerprint;
    }

    // Owned JSON kodunun ilgili kapali lookup katalogunda bulunmasini garanti eder.
    private static string EnsureKnown(string value, IReadOnlyCollection<string> knownCodes, string parameterName)
    {
        var normalized = Check.NotNullOrWhiteSpace(value, parameterName).Trim();
        return knownCodes.Contains(normalized, StringComparer.Ordinal)
            ? normalized
            : throw new ArgumentOutOfRangeException(parameterName);
    }
}
