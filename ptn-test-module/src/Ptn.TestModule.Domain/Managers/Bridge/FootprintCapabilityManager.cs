using System.Collections.Generic;
using System.Linq;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Models.Bridge.Footprint;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Ortam yetenegini yoklar, yazma kumesi stratejisini secer ve slot yasam dongusunu kapatir.
// sistemdeki gorevi: Paylasimli ortamda capture'i durdurur ve her sonucu advisory sozlesmesinde tutar.
public class FootprintCapabilityManager : TestModuleDomainService
{
    // Ortam olgularina gore kullanilabilir en guclu footprint seviyesini dondurur.
    public PtnCapabilityLevel ResolveCapability(
        bool hasLogicalDecoding,
        bool canReplicate,
        bool hasExclusiveSandbox)
    {
        if (!hasExclusiveSandbox)
        {
            return CreateUnavailableCapability(false);
        }

        return CreateCapability(hasLogicalDecoding, canReplicate, true);
    }

    // Capability seviyesini ayni footprint sonuc sozlesmesine cevirir.
    public PtnFootprintResult Describe(PtnCapabilityLevel capability) => new()
    {
        StrengthCode = capability.FootprintStrengthCode,
        IsAdvisoryOnly = true,
        Reasons = capability.Reasons.ToList()
    };

    // Teknik provider sonucunun oracle bayragini acmasini engeller.
    public static PtnFootprintResult EnsureAdvisory(PtnFootprintResult result)
    {
        result.IsAdvisoryOnly = true;
        return result;
    }

    // Provider olgularini kapali footprint seviyesine cevirir.
    public static PtnCapabilityLevel CreateCapability(
        bool hasLogicalDecoding,
        bool canReplicate,
        bool hasExclusiveSandbox) => new()
    {
        FootprintStrengthCode = hasLogicalDecoding && canReplicate && hasExclusiveSandbox
            ? PtnFootprintStrengthCodes.Exact
            : PtnFootprintStrengthCodes.Unavailable,
        HasLogicalDecoding = hasLogicalDecoding && canReplicate,
        HasExclusiveSandbox = hasExclusiveSandbox,
        HasProjectionSurface = false,
        Reasons = hasLogicalDecoding && canReplicate
            ? []
            : [TestModuleBridgeErrorCodes.EvidenceUnavailable]
    };

    // Yetenek yoklugunu exception yerine kapali capability sonucuna cevirir.
    public static PtnCapabilityLevel CreateUnavailableCapability(bool hasExclusiveSandbox) => new()
    {
        FootprintStrengthCode = PtnFootprintStrengthCodes.Unavailable,
        HasExclusiveSandbox = hasExclusiveSandbox,
        HasProjectionSurface = false,
        Reasons = [TestModuleBridgeErrorCodes.EvidenceUnavailable]
    };

    // Yetenek yoklugunu advisory footprint sonucuna cevirir.
    public static PtnFootprintResult CreateUnavailableFootprint(IEnumerable<string> reasons) => new()
    {
        StrengthCode = PtnFootprintStrengthCodes.Unavailable,
        IsAdvisoryOnly = true,
        Reasons = reasons.ToList()
    };
}
