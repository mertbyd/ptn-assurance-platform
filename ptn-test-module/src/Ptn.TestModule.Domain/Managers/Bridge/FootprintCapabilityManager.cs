using System;
using System.Linq;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Footprint;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Checker capability olgusunu Bridge sozlugune cevirir ve footprint butcelerini uygular.
// sistemdeki gorevi: Checker'in sahip oldugu yoklamayi tekrar etmeden her sonucu advisory sozlesmesinde tutar.
public class FootprintCapabilityManager : TestModuleDomainService
{
    // Primitive servis girdilerinden checker'a maplenecek capability domain istegini kurar.
    public CapabilityProbeRequest CreateProbeRequest(Guid connectionId, bool hasExclusiveSandbox) => new()
    {
        ConnectionId = connectionId,
        RequiresExclusiveSandbox = hasExclusiveSandbox
    };

    // Primitive servis girdilerinden checker'a maplenecek capture domain istegini kurar.
    public WriteSetCaptureRequest CreateCaptureRequest(Guid connectionId, Guid captureId) => new()
    {
        ConnectionId = connectionId,
        CaptureRef = captureId,
        Correlation = new CorrelationRef
        {
            TraceId = captureId.ToString("N"),
            StepKey = PtnCorrelationConsts.WriteSetCaptureStepKey
        }
    };

    // Checker capability kodunu birebir Bridge seviyesine ve projection olgusuna cevirir.
    public CapabilityLevel ResolveCapability(CheckerCapabilityLevel capability) => new()
    {
        FootprintStrengthCode = NormalizeStrength(capability.StrengthCode),
        HasLogicalDecoding = capability.HasLogicalDecoding,
        HasExclusiveSandbox = capability.HasExclusiveSandbox,
        HasProjectionSurface = true,
        Reasons = capability.Reasons.Take(PtnBridgeConsts.MaxEvidencePerNode).ToList()
    };

    // Capability seviyesini ayni footprint sonuc sozlesmesine cevirir.
    public FootprintResult Describe(CapabilityLevel capability) => new()
    {
        StrengthCode = capability.FootprintStrengthCode,
        IsAdvisoryOnly = true,
        Reasons = capability.Reasons.ToList()
    };

    // Teknik provider sonucunun oracle bayragini acmasini engeller.
    public FootprintResult Normalize(FootprintResult result)
    {
        result.StrengthCode = NormalizeStrength(result.StrengthCode);
        result.Tables = result.Tables.Take(PtnBridgeConsts.MaxNodeCount).ToList();
        result.Columns = result.Columns.Take(PtnBridgeConsts.MaxNodeCount).ToList();
        result.RowDeltas = result.RowDeltas.Take(PtnBridgeConsts.MaxNodeCount).ToList();
        result.Reasons = result.Reasons.Take(PtnBridgeConsts.MaxEvidencePerNode).ToList();
        result.IsAdvisoryOnly = true;
        return result;
    }

    // Bilinmeyen checker seviyesini sessiz drift yerine kapali unavailable sonucuna indirger.
    private static string NormalizeStrength(string strengthCode) =>
        PtnFootprintStrengthCodes.All.Contains(strengthCode)
            ? strengthCode
            : PtnFootprintStrengthCodes.Unavailable;
}
