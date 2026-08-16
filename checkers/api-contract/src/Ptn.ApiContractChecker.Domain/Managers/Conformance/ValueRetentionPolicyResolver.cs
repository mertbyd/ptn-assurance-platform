using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.ExceptionCodes.Conformance;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Settings;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Ptn.ApiContractChecker.Managers.Conformance;

// islevi: Tenant -> global -> default setting zincirinden bulgu deger saklama politikasini cozer.
// sistemdeki gorevi: Hashed modunun salt zorunlulugunu fail-closed uygular ve manager'lara hazir politika verir.
public class ValueRetentionPolicyResolver : ITransientDependency
{
    private readonly ISettingProvider _settingProvider;

    public ValueRetentionPolicyResolver(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    // islevi: Calisan tenant baglaminin retention modu ile gizli salt degerini dogrular.
    public async Task<ValueRetentionPolicy> ResolveAsync()
    {
        var mode = await _settingProvider.GetOrNullAsync(
                       ApiContractCheckerSettings.Conformance.ValueRetentionMode)
                   ?? ApiContractCheckerSettings.Conformance.DefaultValueRetentionMode;
        EnsureMode(mode);
        var salt = await _settingProvider.GetOrNullAsync(
                       ApiContractCheckerSettings.Conformance.ValueRedactionSalt)
                   ?? ApiContractCheckerSettings.Conformance.DefaultValueRedactionSalt;
        EnsureSalt(mode, salt);
        return new ValueRetentionPolicy(mode, salt);
    }

    // islevi: Retention kodunu kapali katalog disinda fail-closed reddeder.
    private static void EnsureMode(string mode)
    {
        if (!ValueRetentionModeCodes.IsDefined(mode))
        {
            throw new BusinessException(ConformanceExceptionCodes.RetentionModeInvalid);
        }
    }

    // islevi: Hashed retention seciminde bos HMAC salt'ini reddeder.
    private static void EnsureSalt(string mode, string salt)
    {
        if (mode == ValueRetentionModeCodes.Hashed && string.IsNullOrWhiteSpace(salt))
        {
            throw new BusinessException(ConformanceExceptionCodes.RedactionSaltMissing);
        }
    }
}
