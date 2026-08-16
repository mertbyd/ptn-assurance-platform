using System;
using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Hedef veritabani baglantilarinda desteklenen kararli TLS politika kodlarini tanimlar.
// sistemdeki gorevi: DTO, manager ve provider adapterleri ayni kapali kod kumesini kullanir; motor katmanina serbest metin sizmaz.
public static class TlsModeCodes
{
    public const string Require = "Require";
    public const string Prefer = "Prefer";
    public const string Disable = "Disable";

    private static readonly HashSet<string> Values = new(StringComparer.Ordinal)
    {
        Require,
        Prefer,
        Disable
    };

    // islevi: Verilen TLS kodunun desteklenen kapali kumede olup olmadigini bildirir.
    public static bool IsDefined(string? code)
        => code is not null && Values.Contains(code);
}
