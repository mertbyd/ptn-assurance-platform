using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Repository.Comparison;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Projections;

// islevi: Projection SQL kurucusunun iki motorda yalniz SELECT ve bagli deger parametreleri urettigini dogrular.
// sistemdeki gorevi: ADR-0007 salt-okunurluk ve enjeksiyon degismezlerini onayli dinamik-SQL omurgasinda korur.
public class ProjectionRepositoryTests
{
    [Theory]
    [InlineData(DatabaseEngineCodes.PostgreSql)]
    [InlineData(DatabaseEngineCodes.SqlServer)]
    public void Projection_Query_Should_Be_Select_Only(string engineCode)
    {
        var query = BuildQuery(engineCode, "42");
        var normalized = query.Sql.ToUpperInvariant();

        normalized.TrimStart().ShouldStartWith("SELECT ");
        normalized.ShouldNotContain(" INSERT ");
        normalized.ShouldNotContain(" UPDATE ");
        normalized.ShouldNotContain(" DELETE ");
        normalized.ShouldNotContain(" DROP ");
        normalized.ShouldNotContain(" ALTER ");
        normalized.ShouldNotContain(" CREATE ");
    }

    [Theory]
    [InlineData(DatabaseEngineCodes.PostgreSql)]
    [InlineData(DatabaseEngineCodes.SqlServer)]
    public void Projection_Key_Values_Should_Be_Parameters(string engineCode)
    {
        const string injectionAttempt = "42'; delete from users; --";

        var query = BuildQuery(engineCode, injectionAttempt);

        query.Sql.ShouldNotContain(injectionAttempt);
        query.Parameters.ShouldContain(injectionAttempt);
    }

    // islevi: Test motoru icin katalogda dogrulanmis projection query'sini onayli base uzerinden kurar.
    private static DynamicSqlQuery BuildQuery(string engineCode, string keyValue)
    {
        DatabaseDataComparisonRepositoryBase repository = engineCode == DatabaseEngineCodes.PostgreSql
            ? new PostgreSqlDatabaseDataComparisonRepository()
            : new SqlServerDatabaseDataComparisonRepository();
        return repository.BuildProjectionQuery(
            CreateStructure(),
            new Dictionary<string, string?> { ["id"] = keyValue },
            ["email", "status"]);
    }

    // islevi: SQL testine mevcut tablo, unique anahtar ve secili kolon katalog modeli verir.
    private static TableDataStructureModel CreateStructure()
        => new()
        {
            SchemaName = "public",
            TableName = "users",
            ColumnNames = ["id", "email", "status"],
            Columns =
            [
                new TableDataColumnModel { Name = "id", CanonicalDataTypeCode = CanonicalDataTypeCodes.String },
                new TableDataColumnModel { Name = "email", CanonicalDataTypeCode = CanonicalDataTypeCodes.String },
                new TableDataColumnModel { Name = "status", CanonicalDataTypeCode = CanonicalDataTypeCodes.String }
            ],
            UniqueKeyColumnSets = [["id"]]
        };
}
