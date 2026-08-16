using System.Threading.Tasks;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Settings;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: Tenant -> global -> default ayar zincirinden bulgu deger saklama politikasini cozer.
// sistemdeki gorevi: Hashed modunda salt zorunlulugunu tek noktada fail-closed uygular; saf diff motoruna hazir politika verir.
public class ValueRetentionPolicyResolver : ITransientDependency
{
    private readonly ISettingProvider _settingProvider;

    // islevi: Resolver'i ABP'nin tenant-aware setting okuyucusuyla kurar.
    public ValueRetentionPolicyResolver(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    // islevi: Calisan tenant baglami icin kararli saklama modu ve salt'i cozer.
    public async Task<ValueRetentionPolicy> ResolveAsync()
    {
        var modeCode = await _settingProvider.GetOrNullAsync(
                           DatabaseCheckerSettings.DataComparison.ValueRetentionMode)
                       ?? DatabaseCheckerSettings.DataComparison.DefaultValueRetentionMode;
        if (!ValueRetentionModeCodes.IsDefined(modeCode))
        {
            throw new BusinessException(GeneralExceptionCodes.InvalidEnumValue);
        }

        var salt = await _settingProvider.GetOrNullAsync(
                       DatabaseCheckerSettings.DataComparison.ValueRedactionSalt)
                   ?? DatabaseCheckerSettings.DataComparison.DefaultValueRedactionSalt;
        if (modeCode == ValueRetentionModeCodes.Hashed && string.IsNullOrWhiteSpace(salt))
        {
            throw new BusinessException(DataComparisonExceptionCodes.RedactionSaltMissing);
        }

        return new ValueRetentionPolicy(modeCode, salt);
    }
}
