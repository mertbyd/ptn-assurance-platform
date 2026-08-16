using System;
using System.Security.Cryptography;
using System.Text;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Models.Comparison;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: Ham hucre veya PK metnini None, HMAC-SHA256, maskeli ya da tam saklama bicimine deterministik cevirir.
// sistemdeki gorevi: Veri bulgusu uretilirken ham hedef verisinin metod disina cikmadigi tek saf redaction noktasidir.
public sealed class FindingValueRedactor : ITransientDependency
{
    private const byte NullHashDiscriminator = 0;
    private const byte ValueHashDiscriminator = 1;
    private const string MaskMarker = "***";

    // islevi: Tek bir nullable degeri secilen saklama politikasina gore bulguya guvenle yazar.
    public string? Redact(string? value, ValueRetentionPolicy policy)
        => policy.ModeCode switch
        {
            ValueRetentionModeCodes.None => null,
            ValueRetentionModeCodes.Hashed => Hash(value, policy.Salt),
            ValueRetentionModeCodes.Masked => Mask(value),
            ValueRetentionModeCodes.Full => value,
            _ => throw new BusinessException(GeneralExceptionCodes.InvalidEnumValue)
        };

    // islevi: Salt'i HMAC anahtari yaparak null ile bos metni de ayiran kararli SHA-256 temsili uretir.
    private static string Hash(string? value, string salt)
    {
        var valueBytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        var payload = new byte[valueBytes.Length + 1];
        payload[0] = value is null ? NullHashDiscriminator : ValueHashDiscriminator;
        valueBytes.CopyTo(payload, 1);
        return Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(salt), payload));
    }

    // islevi: E-posta ve genel metinlerde yalniz sinirli kenar karakterlerini koruyan deterministik maske uretir.
    private static string? Mask(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var atIndex = value.IndexOf('@');
        var lastDotIndex = value.LastIndexOf('.');
        if (atIndex > 0 && lastDotIndex > atIndex)
        {
            return $"{value[0]}{MaskMarker}@{MaskMarker}{value[lastDotIndex..]}";
        }

        if (value.Length <= 4)
        {
            return MaskMarker;
        }

        if (value.Length <= 7)
        {
            return $"{value[0]}{MaskMarker}{value[^1]}";
        }

        return $"{value[..3]}{MaskMarker}{value[^4..]}";
    }
}
