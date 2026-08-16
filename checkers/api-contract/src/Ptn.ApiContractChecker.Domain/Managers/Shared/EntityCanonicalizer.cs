using System;

namespace Ptn.ApiContractChecker.Managers.Shared;

// islevi: Entity alanlarina yazilmadan once ortak metin kanoniklestirmesini tek yerde uygular.
// sistemdeki gorevi: Manager'lar arasinda trim ve bos-deger normalizasyonunun kopyalanmasini engeller.
internal static class EntityCanonicalizer
{
    // Zorunlu metni manager yazimlari icin trimlenmis kanonik bicime getirir.
    internal static string NormalizeRequired(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Trim();
    }

    // Opsiyonel metni trimler ve anlamsiz bos degeri null'a indirger.
    internal static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }
}
