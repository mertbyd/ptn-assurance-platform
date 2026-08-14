using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Lookups;

// islevi: Kanit dugumlerinin rapor icindeki kapali alaka seviyelerini tanimlar.
// sistemdeki gorevi: Siralamayi ajanin sozel guveninden bagimsiz motor kararina baglar.
public static class PtnRelevanceCodes
{
    public const string High = nameof(High);
    public const string Normal = nameof(Normal);

    public static IReadOnlyCollection<string> All { get; } = [High, Normal];
}
