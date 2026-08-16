using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: Motor tip saglayicilarinin sozlukten esleme ve fidelity sonucu kurma davranisini ortaklastirir.
// sistemdeki gorevi: PostgreSQL ve SQL Server bilesenlerinde yalniz provider veri tablosu ile EngineCode farkinin kalmasini saglar.
/// <summary>
/// Motor tip saglayicilarinin ortak kanonik esleme davranisini tanimlar.
/// </summary>
public abstract class EngineTypeMapProviderBase : IEngineTypeMapProvider
{
    // Motorun ham tip adi -> kanonik esleme tablosu.
    protected abstract IReadOnlyDictionary<string, CanonicalTypeMapping> TypeMappings { get; }

    /// <summary>Bu saglayicinin destekledigi kararli motor kodunu dondurur.</summary>
    public abstract string EngineCode { get; }

    // islevi: Ham tip adini trim/casing gurultusundan bagimsiz esler; bilinmeyen tipi uydurmadan false dondurur.
    /// <summary>Ham motor tipini kanonik tip ve fidelity degerine eslemeyi dener.</summary>
    public bool TryMap(string rawTypeName, out CanonicalTypeMapping mapping)
    {
        if (!string.IsNullOrWhiteSpace(rawTypeName) &&
            TypeMappings.TryGetValue(rawTypeName.Trim(), out var resolvedMapping))
        {
            mapping = resolvedMapping;
            return true;
        }

        mapping = null!;
        return false;
    }

    // islevi: Kayipsiz kanonik aile esleme sonucunu kurar.
    protected static CanonicalTypeMapping Exact(string canonicalTypeCode)
        => new(canonicalTypeCode, TypeMappingFidelityCodes.Exact);

    // islevi: Anlam daralmasi tasiyan kanonik aile esleme sonucunu kurar.
    protected static CanonicalTypeMapping Approximate(string canonicalTypeCode)
        => new(canonicalTypeCode, TypeMappingFidelityCodes.Approximate);
}
