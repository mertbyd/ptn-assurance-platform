using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Managers.Comparison;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Comparison;

// islevi: PostgreSQL ve SQL Server ham tip tablolarinin bilinen/bilinmeyen esleme davranisini dogrular.
// sistemdeki gorevi: Provider tablolarini ve conventional DI adlandirmasinin resolver uzerinden calistigini EFCore katmaninda sabitler.
public class EngineTypeMapProvider_Tests : DatabaseCheckerEntityFrameworkCoreTestBase
{
    // islevi: PostgreSQL varchar tipinin kayipsiz String ailesine eslendigini dogrular.
    [Fact]
    public void PostgreSql_Known_Type_Should_Map()
    {
        var provider = new PostgreSqlEngineTypeMapProvider();

        var mapped = provider.TryMap(EngineDataTypeNameCodes.PostgreSql.VarChar, out var mapping);

        mapped.ShouldBeTrue();
        mapping.CanonicalTypeCode.ShouldBe(CanonicalDataTypeCodes.String);
        mapping.FidelityCode.ShouldBe(TypeMappingFidelityCodes.Exact);
    }

    // islevi: PostgreSQL tsvector tipine uydurma kanonik karsilik verilmedigini dogrular.
    [Fact]
    public void PostgreSql_Unknown_Type_Should_Not_Map()
    {
        var provider = new PostgreSqlEngineTypeMapProvider();

        provider.TryMap(EngineDataTypeNameCodes.PostgreSql.TsVector, out _).ShouldBeFalse();
    }

    // islevi: SQL Server money tipinin Money ailesine kayipli eslendigini dogrular.
    [Fact]
    public void SqlServer_Known_Type_Should_Map_With_Fidelity()
    {
        var provider = new SqlServerEngineTypeMapProvider();

        var mapped = provider.TryMap(EngineDataTypeNameCodes.SqlServer.Money, out var mapping);

        mapped.ShouldBeTrue();
        mapping.CanonicalTypeCode.ShouldBe(CanonicalDataTypeCodes.Money);
        mapping.FidelityCode.ShouldBe(TypeMappingFidelityCodes.Approximate);
    }

    // islevi: SQL Server sql_variant tipine uydurma kanonik karsilik verilmedigini dogrular.
    [Fact]
    public void SqlServer_Unknown_Type_Should_Not_Map()
    {
        var provider = new SqlServerEngineTypeMapProvider();

        provider.TryMap(EngineDataTypeNameCodes.SqlServer.SqlVariant, out _).ShouldBeFalse();
    }

    // islevi: Conventional DI'in iki provider'i interface koleksiyonuna alip resolver ile motor koduna gore sectigini dogrular.
    [Theory]
    [InlineData(DatabaseEngineCodes.PostgreSql, typeof(PostgreSqlEngineTypeMapProvider))]
    [InlineData(DatabaseEngineCodes.SqlServer, typeof(SqlServerEngineTypeMapProvider))]
    public void Resolver_Should_Select_Engine_Type_Map_Provider(string engineCode, Type expectedProviderType)
    {
        var resolver = GetRequiredService<IEngineComponentResolver<IEngineTypeMapProvider>>();

        var provider = resolver.Resolve(engineCode);

        provider.GetType().ShouldBe(expectedProviderType);
    }
}
