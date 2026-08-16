using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.ExceptionCodes.Conformance;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Conformance;

// islevi: Profil ve kural kodunu Ignore/Info/Warn/Fail seviyesine cozer.
// sistemdeki gorevi: Response ve request oracle'larinin ayni kapali politika matrisini kullanmasini saglar.
public class ConformancePolicyResolver : ITransientDependency
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Profiles =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
        {
            [ConformanceProfileCodes.Strict] = BuildProfile(
                ConformanceLevelCodes.Fail,
                ConformanceLevelCodes.Fail,
                ConformanceLevelCodes.Fail),
            [ConformanceProfileCodes.Runtime] = BuildProfile(
                ConformanceLevelCodes.Warn,
                ConformanceLevelCodes.Warn,
                ConformanceLevelCodes.Warn),
            [ConformanceProfileCodes.Lenient] = BuildProfile(
                ConformanceLevelCodes.Ignore,
                ConformanceLevelCodes.Warn,
                ConformanceLevelCodes.Ignore)
        };

    public string Resolve(string profileCode, string ruleCode)
    {
        if (!Profiles.TryGetValue(profileCode, out var rules) || !rules.TryGetValue(ruleCode, out var level))
        {
            throw new BusinessException(ConformanceExceptionCodes.ProfileInvalid);
        }

        return level;
    }

    // Profil farkini yalniz ek property, header/security ve medya kurallarinda uygular.
    private static IReadOnlyDictionary<string, string> BuildProfile(
        string additionalLevel,
        string boundaryLevel,
        string securityLevel)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ConformanceRuleCodes.NotAServerError] = ConformanceLevelCodes.Fail,
            [ConformanceRuleCodes.StatusCodeConformance] = ConformanceLevelCodes.Fail,
            [ConformanceRuleCodes.ContentTypeConformance] = boundaryLevel,
            [ConformanceRuleCodes.ResponseHeadersConformance] = boundaryLevel,
            [ConformanceRuleCodes.ResponseSchemaConformance] = ConformanceLevelCodes.Fail,
            [ConformanceRuleCodes.AdditionalProperties] = additionalLevel,
            [ConformanceRuleCodes.SecurityRequirement] = securityLevel
        };
    }
}
