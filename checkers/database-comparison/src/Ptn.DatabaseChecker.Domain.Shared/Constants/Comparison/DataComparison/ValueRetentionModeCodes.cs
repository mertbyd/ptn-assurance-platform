using System;
using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Veri bulgularinda hedef hucre ve anahtar degerlerinin nasil saklanacagini belirleyen kararli kodlari tanimlar.
// sistemdeki gorevi: Ayar, resolver ve saf redactor ayni kapali politika kumesine baglanir; varsayilan politika ham veriyi kalicilastirmaz.
public static class ValueRetentionModeCodes
{
    public const string None = "None";
    public const string Hashed = "Hashed";
    public const string Masked = "Masked";
    public const string Full = "Full";

    private static readonly HashSet<string> Values = new(StringComparer.Ordinal)
    {
        None,
        Hashed,
        Masked,
        Full
    };

    // islevi: Verilen saklama modu kodunun desteklenen kapali kumede olup olmadigini bildirir.
    public static bool IsDefined(string? code)
        => code is not null && Values.Contains(code);
}
