using System;
using System.Security.Cryptography;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Bridge.Profiles;

// islevi: Is kurali belgesinin boyut butcesini uygular ve icerigini SHA-256 ile muhurler.
// sistemdeki gorevi: rules_fingerprint uretiminin tek sahibidir; kural metnini yorumlamaz, yalniz adresler.
public class BusinessRuleFingerprintManager : TestModuleDomainService
{
    // Kural kaynaginin ayarli kok icinde ve mevcut olmasini okuma oncesinde zorunlu kilar.
    public void EnsureSourceIsAddressable(string rootedPrefix, string filePath, bool exists)
    {
        if (!filePath.StartsWith(rootedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.BusinessRulesInvalid);
        }

        if (!exists)
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.BusinessRulesNotFound);
        }
    }

    // Kural belgesinin bos veya butce disi olmasini okuma oncesinde reddeder.
    public void EnsureWithinBudget(long byteCount)
    {
        if (byteCount <= 0 || byteCount > PtnBridgeConsts.MaxBusinessRulesBytes)
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.BusinessRulesInvalid);
        }
    }

    // Ham kural baytlarini profil paketiyle ayni lowercase sha256: sozlesmesine cevirir.
    public string ComputeFingerprint(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        EnsureWithinBudget(bytes.LongLength);
        return PtnBridgeSettingNames.FingerprintPrefix +
               Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}
