using System;
using System.Linq;
using System.Threading.Tasks;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.ExceptionCodes;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Ptn.ApiContractChecker.Managers.Shared;

// islevi: Runtime setting ile sinirlanan enum degerlerini dogrular.
// sistemdeki gorevi: Her manager'in setting parse etmesini engelleyip enum allowlist kontrolunu tek yerde toplar.
public class EnumValidationManager : ApiContractCheckerDomainService
{
    private ISettingProvider SettingProvider => LazyGetRequiredService<ISettingProvider>();

    public EnumValidationManager(IAbpLazyServiceProvider abpLazyServiceProvider)
        : base(abpLazyServiceProvider)
    {
    }

    public async Task ValidateAllowedEnumAsync<TEnum>(TEnum enumValue, string settingName)
        where TEnum : struct, Enum
    {
        var allowedValuesText = await SettingProvider.GetOrNullAsync(settingName);
        if (string.IsNullOrWhiteSpace(allowedValuesText))
        {
            throw new BusinessException(GeneralExceptionCodes.InvalidOperation);
        }

        var allowedValues = allowedValuesText
            .Split(SettingValueConstants.ListSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(value => int.TryParse(value, out var parsed) ? parsed : (int?)null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();

        if (!allowedValues.Contains(Convert.ToInt32(enumValue)))
        {
            throw new BusinessException(GeneralExceptionCodes.InvalidEnumValue);
        }
    }
}
