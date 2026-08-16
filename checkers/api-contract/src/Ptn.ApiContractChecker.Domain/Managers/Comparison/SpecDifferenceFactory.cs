using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Models.Comparison;
using Ptn.ApiContractChecker.Models.Runs;
using Volo.Abp;

namespace Ptn.ApiContractChecker.Managers.Comparison;

// islevi: Adres, tur, yon ve once/sonra degerlerinden siddetlendirilmemis spec farki kurar.
// sistemdeki gorevi: OnlyInSource, OnlyInTarget ve Modified fark insasini comparer ile manager arasinda tek merkezde tutar.
public static class SpecDifferenceFactory
{
    // Kaynakta olup hedefte olmayan sozlesme parcasini fark modeline cevirir.
    public static SpecDifferenceModel OnlyInSource(
        string kindCode,
        string directionCode,
        FindingAddress address,
        string? sourceValue)
        => Create(kindCode, directionCode, address, sourceValue, null);

    // Hedefte olup kaynakta olmayan sozlesme parcasini fark modeline cevirir.
    public static SpecDifferenceModel OnlyInTarget(
        string kindCode,
        string directionCode,
        FindingAddress address,
        string? targetValue)
        => Create(kindCode, directionCode, address, null, targetValue);

    // Iki tarafta da bulunan ama sozlesmesi degisen parcayi fark modeline cevirir.
    public static SpecDifferenceModel Modified(
        string kindCode,
        string directionCode,
        FindingAddress address,
        string? sourceValue,
        string? targetValue)
        => Create(kindCode, directionCode, address, sourceValue, targetValue);

    // Ortak fark alanlarini tek nesne kurulumunda birlestirir.
    private static SpecDifferenceModel Create(
        string kindCode,
        string directionCode,
        FindingAddress address,
        string? sourceValue,
        string? targetValue)
        => new(
            EnsureKnown(kindCode, DifferenceKindCodes.All, nameof(kindCode)),
            EnsureKnown(directionCode, DifferenceDirectionCodes.All, nameof(directionCode)),
            Check.NotNull(address, nameof(address)),
            sourceValue,
            targetValue);

    // Fark kodunun ilgili kapali lookup katalogunda yer aldigini garanti eder.
    private static string EnsureKnown(
        string value,
        IReadOnlyCollection<string> knownCodes,
        string parameterName)
    {
        var normalized = Check.NotNullOrWhiteSpace(value, parameterName).Trim();
        return knownCodes.Contains(normalized, StringComparer.Ordinal)
            ? normalized
            : throw new ArgumentOutOfRangeException(parameterName);
    }
}
