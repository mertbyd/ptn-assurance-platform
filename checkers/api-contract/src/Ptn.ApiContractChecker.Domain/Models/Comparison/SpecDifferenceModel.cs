using Ptn.ApiContractChecker.Models.Runs;

namespace Ptn.ApiContractChecker.Models.Comparison;

// islevi: Iki normalize spec snapshot'i arasindaki siddetlendirilmemis tek farki tasir.
// sistemdeki gorevi: Operasyon ve sema diff adimlarinin KBP-613 siniflandirmasina verecegi saf ara modeldir.
public class SpecDifferenceModel
{
    public string KindCode { get; }
    public string DirectionCode { get; }
    public FindingAddress Address { get; }
    public string? OldValue { get; }
    public string? NewValue { get; }

    // Factory disindaki katmanlarin adres veya kapali kod invariantlarini atlamadan fark modelini tasir.
    internal SpecDifferenceModel(
        string kindCode,
        string directionCode,
        FindingAddress address,
        string? oldValue,
        string? newValue)
    {
        KindCode = kindCode;
        DirectionCode = directionCode;
        Address = address;
        OldValue = oldValue;
        NewValue = newValue;
    }
}
