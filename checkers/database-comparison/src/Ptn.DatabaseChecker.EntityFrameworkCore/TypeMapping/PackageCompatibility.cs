using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.TypeMapping;

// islevi: 0.1.x EF paketindeki public tip-esleme API'sini yeni Domain Manager sahiplerine yonlendirir.
// sistemdeki gorevi: PackageValidation uyumlulugunu korur; runtime DI yalniz Managers/Comparison uygulamalarini kaydeder.
/// <summary>Eski EF paketi tip-esleme taban sinifi icin ikili uyumluluk kabugu.</summary>
[Obsolete("Use Ptn.DatabaseChecker.Managers.Comparison.EngineTypeMapProviderBase.")]
public abstract class EngineTypeMapProviderBase : IEngineTypeMapProvider
{
    /// <summary>Ham motor tipi esleme tablosunu dondurur.</summary>
    protected abstract IReadOnlyDictionary<string, CanonicalTypeMapping> TypeMappings { get; }

    /// <summary>Desteklenen kararli motor kodunu dondurur.</summary>
    public abstract string EngineCode { get; }

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

    /// <summary>Kayipsiz kanonik esleme sonucu kurar.</summary>
    protected static CanonicalTypeMapping Exact(string canonicalTypeCode)
        => new(canonicalTypeCode, TypeMappingFidelityCodes.Exact);

    /// <summary>Kayipli kanonik esleme sonucu kurar.</summary>
    protected static CanonicalTypeMapping Approximate(string canonicalTypeCode)
        => new(canonicalTypeCode, TypeMappingFidelityCodes.Approximate);
}

// islevi: Eski PostgreSQL public tipini Domain Manager'in tek esleme tablosuna baglar.
// sistemdeki gorevi: Eski assembly/type kimligini korurken ikinci bir DI saglayicisi uretilmesini engeller.
/// <summary>Eski PostgreSQL tip saglayicisi icin paket uyumluluk kabugu.</summary>
[DisableConventionalRegistration]
[Obsolete("Use Ptn.DatabaseChecker.Managers.Comparison.PostgreSqlEngineTypeMapProvider.")]
public class PostgreSqlEngineTypeMapProvider : EngineTypeMapProviderBase, ITransientDependency
{
    /// <inheritdoc />
    protected override IReadOnlyDictionary<string, CanonicalTypeMapping> TypeMappings
        => global::Ptn.DatabaseChecker.Managers.Comparison.PostgreSqlEngineTypeMapProvider.CompatibilityMappings;

    /// <inheritdoc />
    public override string EngineCode => DatabaseEngineCodes.PostgreSql;
}

// islevi: Eski SQL Server public tipini Domain Manager'in tek esleme tablosuna baglar.
// sistemdeki gorevi: Eski assembly/type kimligini korurken ikinci bir DI saglayicisi uretilmesini engeller.
/// <summary>Eski SQL Server tip saglayicisi icin paket uyumluluk kabugu.</summary>
[DisableConventionalRegistration]
[Obsolete("Use Ptn.DatabaseChecker.Managers.Comparison.SqlServerEngineTypeMapProvider.")]
public class SqlServerEngineTypeMapProvider : EngineTypeMapProviderBase, ITransientDependency
{
    /// <inheritdoc />
    protected override IReadOnlyDictionary<string, CanonicalTypeMapping> TypeMappings
        => global::Ptn.DatabaseChecker.Managers.Comparison.SqlServerEngineTypeMapProvider.CompatibilityMappings;

    /// <inheritdoc />
    public override string EngineCode => DatabaseEngineCodes.SqlServer;
}
