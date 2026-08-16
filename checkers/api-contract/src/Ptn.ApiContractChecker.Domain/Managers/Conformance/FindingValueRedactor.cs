using System.Security.Cryptography;
using System.Text;
using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.ExceptionCodes.Conformance;
using Ptn.ApiContractChecker.Models.Conformance;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Conformance;

// islevi: Ham bulgu degerini None, HMAC-SHA256, maskeli veya tam saklama bicimine cevirir.
// sistemdeki gorevi: Finding ve conformance sonucuna deger sizmasini tek saf redaction noktasinda engeller.
public sealed class FindingValueRedactor : ITransientDependency
{
    // islevi: Nullable degeri secilen retention politikasina gore guvenli temsile cevirir.
    public string? Redact(string? value, ValueRetentionPolicy policy)
    {
        return policy.ModeCode switch
        {
            ValueRetentionModeCodes.None => null,
            ValueRetentionModeCodes.Hashed => Hash(value, policy.Salt),
            ValueRetentionModeCodes.Masked => Mask(value),
            ValueRetentionModeCodes.Full => value,
            _ => throw new BusinessException(ConformanceExceptionCodes.RetentionModeInvalid)
        };
    }

    // islevi: Null ve bos metni ayiran HMAC-SHA256 retention temsilini uretir.
    private static string Hash(string? value, string salt)
    {
        var valueBytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        var payload = new byte[valueBytes.Length + 1];
        payload[0] = value is null
            ? ValueRetentionConstants.NullHashDiscriminator
            : ValueRetentionConstants.ValueHashDiscriminator;
        valueBytes.CopyTo(payload, 1);
        return Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(salt), payload));
    }

    // islevi: Ham metnin yalniz sinirli kenar karakterlerini koruyan deterministik maskesini uretir.
    private static string? Mask(string? value)
    {
        if (value is null)
        {
            return null;
        }

        return value.Length switch
        {
            <= 4 => ValueRetentionConstants.MaskMarker,
            <= 7 => $"{value[0]}{ValueRetentionConstants.MaskMarker}{value[^1]}",
            _ => $"{value[..3]}{ValueRetentionConstants.MaskMarker}{value[^4..]}"
        };
    }
}
