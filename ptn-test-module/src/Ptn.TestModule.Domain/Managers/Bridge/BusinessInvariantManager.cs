using System;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Models.Bridge.Invariants;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: RESEARCH-0009 M-1..M-4 desenlerini saf aritmetik karsilastirma olarak degerlendirir.
// sistemdeki gorevi: Arazzo criteria ile ifade edilemeyen korunum, delta, tutarlilik ve tekillik kararini uretir.
public class BusinessInvariantManager : TestModuleDomainService
{
    // Kapali desen kodunu ilgili karsilastirmaya yonlendirir ve kodlu gerekceyi dondurur.
    public BusinessInvariantResult Evaluate(BusinessInvariantRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.PatternCode switch
        {
            PtnInvariantPatternCodes.Conservation => Decide(
                request.Left == request.Right,
                PtnInvariantReasonCodes.Balanced,
                PtnInvariantReasonCodes.NotBalanced),
            PtnInvariantPatternCodes.Delta => Decide(
                request.Right - request.Left == request.Delta,
                PtnInvariantReasonCodes.DeltaMatched,
                PtnInvariantReasonCodes.DeltaMismatched),
            PtnInvariantPatternCodes.Consistency => Decide(
                request.Left == request.Right,
                PtnInvariantReasonCodes.Consistent,
                PtnInvariantReasonCodes.Inconsistent),
            PtnInvariantPatternCodes.Uniqueness => Decide(
                request.Left == decimal.Zero,
                PtnInvariantReasonCodes.Unique,
                PtnInvariantReasonCodes.Duplicated),
            _ => throw new BusinessException(TestModuleBridgeErrorCodes.Validation.InvariantPatternInvalid)
        };
    }

    // Karsilastirma sonucunu gecti/kaldi gerekce ciftine baglar.
    private static BusinessInvariantResult Decide(bool passed, string passedReasonCode, string failedReasonCode) => new()
    {
        Passed = passed,
        ReasonCode = passed ? passedReasonCode : failedReasonCode
    };
}
