using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Comparison;

// islevi: Kolon tip ciftlerinin Exact/Canonical/Approximate/Incomparable guven kurallarini dogrular.
// sistemdeki gorevi: Capraz-motor fidelity kararini repository veya comparer I/O'sundan bagimsiz saf domain testleriyle sabitler.
public class ColumnTypeConfidenceResolver_Tests
{
    private readonly ColumnTypeConfidenceResolver _resolver = new();

    // islevi: Ayni motorun kanonik bilgi olmasa da ham tip kesinligini korudugunu dogrular.
    [Fact]
    public void Same_Engine_Should_Be_Exact()
    {
        var confidenceCode = _resolver.Resolve(
            DatabaseEngineCodes.PostgreSql,
            DatabaseEngineCodes.PostgreSql,
            Column(),
            Column());

        confidenceCode.ShouldBe(ComparisonConfidenceCodes.Exact);
    }

    // islevi: Capraz motorda iki kayipsiz eslemenin aileler farkli olsa da Canonical guven urettigini dogrular.
    [Fact]
    public void Cross_Engine_Exact_Mappings_Should_Be_Canonical()
    {
        var confidenceCode = _resolver.Resolve(
            DatabaseEngineCodes.PostgreSql,
            DatabaseEngineCodes.SqlServer,
            Column(CanonicalDataTypeCodes.Integer, TypeMappingFidelityCodes.Exact),
            Column(CanonicalDataTypeCodes.BigInteger, TypeMappingFidelityCodes.Exact));

        confidenceCode.ShouldBe(ComparisonConfidenceCodes.Canonical);
    }

    // islevi: Iki taraftan birindeki kayipli money eslemesinin bulguyu Approximate yaptigini dogrular.
    [Fact]
    public void Cross_Engine_Approximate_Mapping_Should_Be_Approximate()
    {
        var confidenceCode = _resolver.Resolve(
            DatabaseEngineCodes.PostgreSql,
            DatabaseEngineCodes.SqlServer,
            Column(CanonicalDataTypeCodes.Money, TypeMappingFidelityCodes.Exact),
            Column(CanonicalDataTypeCodes.Money, TypeMappingFidelityCodes.Approximate));

        confidenceCode.ShouldBe(ComparisonConfidenceCodes.Approximate);
    }

    // islevi: Capraz motorda eslenemeyen tek bir tarafin makine guvenini Incomparable yaptigini dogrular.
    [Fact]
    public void Cross_Engine_Unmapped_Type_Should_Be_Incomparable()
    {
        var confidenceCode = _resolver.Resolve(
            DatabaseEngineCodes.PostgreSql,
            DatabaseEngineCodes.SqlServer,
            Column(),
            Column(CanonicalDataTypeCodes.String, TypeMappingFidelityCodes.Exact));

        confidenceCode.ShouldBe(ComparisonConfidenceCodes.Incomparable);
    }

    // islevi: Guven testi icin yalniz kanonik aile ve fidelity alanlari doldurulmus kolon uretir.
    private static SchemaColumnModel Column(string? canonicalTypeCode = null, string? fidelityCode = null)
        => new()
        {
            CanonicalDataType = canonicalTypeCode,
            TypeMappingFidelityCode = fidelityCode
        };
}
