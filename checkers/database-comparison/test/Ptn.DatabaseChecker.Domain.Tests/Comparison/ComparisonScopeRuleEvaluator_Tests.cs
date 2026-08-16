using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Scope;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Comparison;

// islevi: ComparisonScopeRuleEvaluator kapsam-kurali semantigini dogrular.
// sistemdeki gorevi: T6/T7 motoru eklenmeden once Include/Exclude/Ignore/DataCompare onceliklerinin kirilmamasini garanti eder.
public class ComparisonScopeRuleEvaluator_Tests
{
    [Fact]
    public void Should_Treat_Include_As_Whitelist_When_Include_Rules_Exist()
    {
        var evaluator = new ComparisonScopeRuleEvaluator();
        var tables = BuildTables();
        var rules = new List<ComparisonScopeRule>
        {
            Rule(ScopeKindCodes.Include, "public", "customers")
        };

        var result = evaluator.FilterComparableTables(tables, rules);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("customers");
    }

    [Fact]
    public void Should_Let_Exclude_And_Ignore_Win_Over_Include()
    {
        var evaluator = new ComparisonScopeRuleEvaluator();
        var tables = BuildTables();
        var rules = new List<ComparisonScopeRule>
        {
            Rule(ScopeKindCodes.Include, "public"),
            Rule(ScopeKindCodes.Exclude, "public", "orders"),
            Rule(ScopeKindCodes.Ignore, "public", "audit_log")
        };

        var result = evaluator.FilterComparableTables(tables, rules);

        result.Select(table => table.Name).ShouldBe(new[] { "customers" });
    }

    [Fact]
    public void Should_Select_Only_DataCompare_Tables_That_Are_Still_Comparable()
    {
        var evaluator = new ComparisonScopeRuleEvaluator();
        var tables = BuildTables();
        var rules = new List<ComparisonScopeRule>
        {
            Rule(ScopeKindCodes.DataCompare, "public", "customers"),
            Rule(ScopeKindCodes.DataCompare, "public", "orders"),
            Rule(ScopeKindCodes.Exclude, "public", "orders")
        };

        var result = evaluator.FilterDataCompareTables(tables, rules);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("customers");
    }

    [Fact]
    public void Should_Build_Data_Read_Plan_Directly_From_Scope_Rules()
    {
        var evaluator = new ComparisonScopeRuleEvaluator();
        var rules = new List<ComparisonScopeRule>
        {
            Rule(ScopeKindCodes.DataCompare, "public", "customers"),
            Rule(ScopeKindCodes.DataCompare, "public", "orders"),
            Rule(ScopeKindCodes.Exclude, "public", "orders")
        };

        var result = evaluator.BuildDataCompareTableIdentifiers(rules);

        var table = result.ShouldHaveSingleItem();
        table.SchemaName.ShouldBe("public");
        table.TableName.ShouldBe("customers");
    }

    // islevi: Testlerde kullanilan basit snapshot tablo listesini kurar.
    private static List<SchemaTableModel> BuildTables()
        => new()
        {
            Table("public", "customers"),
            Table("public", "orders"),
            Table("public", "audit_log")
        };

    // islevi: Test icin sema/tablo adindan minimal SchemaTableModel olusturur.
    private static SchemaTableModel Table(string schema, string name)
        => new()
        {
            Schema = schema,
            Name = name
        };

    // islevi: Test icin sema seviyeli veya nokta-atisi kapsam kuralini olusturur.
    private static ComparisonScopeRule Rule(string scopeKindCode, string schemaName, string? objectName = null)
        => new()
        {
            ScopeKindCode = scopeKindCode,
            SchemaName = schemaName,
            ObjectName = objectName
        };
}
