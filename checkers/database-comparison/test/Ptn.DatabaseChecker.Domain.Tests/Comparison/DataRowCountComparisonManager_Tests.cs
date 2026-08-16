using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Comparison;

// islevi: DataRowCountComparisonManager'in secili tablo COUNT(*) farklarini yon ve signed farkla urettigini dogrular.
// sistemdeki gorevi: T7 row-count acceptance'ini provider SQL'inden ayirip saf domain davranisi olarak sabitler.
public class DataRowCountComparisonManager_Tests
{
    [Fact]
    public void Should_Report_Source_Extra_Rows_With_Positive_Difference()
    {
        var manager = new DataRowCountComparisonManager();
        var tables = new List<ComparisonTableIdentifierModel> { Table("public", "orders") };
        var source = new List<TableRowCountModel> { RowCount("public", "orders", 120) };
        var target = new List<TableRowCountModel> { RowCount("public", "orders", 100) };

        var differences = manager.Compare(tables, source, target);

        differences.Count.ShouldBe(1);
        differences[0].SchemaName.ShouldBe("public");
        differences[0].TableName.ShouldBe("orders");
        differences[0].KindCode.ShouldBe(DifferenceKindCodes.OnlyInSource);
        differences[0].SourceRowCount.ShouldBe(120);
        differences[0].TargetRowCount.ShouldBe(100);
        differences[0].RowCountDifference.ShouldBe(20);
    }

    [Fact]
    public void Should_Report_Target_Extra_Rows_With_Negative_Difference()
    {
        var manager = new DataRowCountComparisonManager();
        var tables = new List<ComparisonTableIdentifierModel> { Table("dbo", "Customers") };
        var source = new List<TableRowCountModel> { RowCount("dbo", "Customers", 10) };
        var target = new List<TableRowCountModel> { RowCount("dbo", "Customers", 15) };

        var differences = manager.Compare(tables, source, target);

        differences.Count.ShouldBe(1);
        differences[0].KindCode.ShouldBe(DifferenceKindCodes.OnlyInTarget);
        differences[0].RowCountDifference.ShouldBe(-5);
    }

    [Fact]
    public void Should_Not_Report_Equal_Row_Counts()
    {
        var manager = new DataRowCountComparisonManager();
        var tables = new List<ComparisonTableIdentifierModel> { Table("public", "customers") };
        var source = new List<TableRowCountModel> { RowCount("public", "customers", 42) };
        var target = new List<TableRowCountModel> { RowCount("public", "customers", 42) };

        var differences = manager.Compare(tables, source, target);

        differences.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Report_Table_Counted_Only_On_Source_As_OnlyInSource()
    {
        var manager = new DataRowCountComparisonManager();
        var tables = new List<ComparisonTableIdentifierModel> { Table("public", "orders") };
        var source = new List<TableRowCountModel> { RowCount("public", "orders", 50) };
        var target = new List<TableRowCountModel>();

        var differences = manager.Compare(tables, source, target);

        differences.Count.ShouldBe(1);
        differences[0].KindCode.ShouldBe(DifferenceKindCodes.OnlyInSource);
        differences[0].SourceRowCount.ShouldBe(50);
        differences[0].TargetRowCount.ShouldBeNull();
        differences[0].RowCountDifference.ShouldBeNull();
    }

    [Fact]
    public void Should_Report_Table_Counted_Only_On_Target_As_OnlyInTarget()
    {
        var manager = new DataRowCountComparisonManager();
        var tables = new List<ComparisonTableIdentifierModel> { Table("public", "orders") };
        var source = new List<TableRowCountModel>();
        var target = new List<TableRowCountModel> { RowCount("public", "orders", 50) };

        var differences = manager.Compare(tables, source, target);

        differences.Count.ShouldBe(1);
        differences[0].KindCode.ShouldBe(DifferenceKindCodes.OnlyInTarget);
        differences[0].SourceRowCount.ShouldBeNull();
        differences[0].TargetRowCount.ShouldBe(50);
    }

    [Fact]
    public void Should_Not_Report_When_Requested_Table_Absent_On_Both_Sides()
    {
        var manager = new DataRowCountComparisonManager();
        var tables = new List<ComparisonTableIdentifierModel> { Table("public", "orders") };

        var differences = manager.Compare(tables, new List<TableRowCountModel>(), new List<TableRowCountModel>());

        differences.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Report_Only_Tables_That_Differ_Among_Many()
    {
        var manager = new DataRowCountComparisonManager();
        var tables = new List<ComparisonTableIdentifierModel> { Table("public", "orders"), Table("public", "customers") };
        var source = new List<TableRowCountModel> { RowCount("public", "orders", 10), RowCount("public", "customers", 5) };
        var target = new List<TableRowCountModel> { RowCount("public", "orders", 10), RowCount("public", "customers", 8) };

        var differences = manager.Compare(tables, source, target);

        var difference = differences.ShouldHaveSingleItem();
        difference.TableName.ShouldBe("customers");
        difference.KindCode.ShouldBe(DifferenceKindCodes.OnlyInTarget);
        difference.RowCountDifference.ShouldBe(-3);
    }

    [Fact]
    public void Should_Report_Zero_Versus_NonZero_Count()
    {
        var manager = new DataRowCountComparisonManager();
        var tables = new List<ComparisonTableIdentifierModel> { Table("public", "orders") };
        var source = new List<TableRowCountModel> { RowCount("public", "orders", 0) };
        var target = new List<TableRowCountModel> { RowCount("public", "orders", 5) };

        var differences = manager.Compare(tables, source, target);

        differences.ShouldHaveSingleItem().RowCountDifference.ShouldBe(-5);
    }

    // islevi: Test icin count alinacak tablo kimligini kurar.
    private static ComparisonTableIdentifierModel Table(string schemaName, string tableName)
        => new()
        {
            SchemaName = schemaName,
            TableName = tableName
        };

    // islevi: Test icin provider row-count sonucunu kurar.
    private static TableRowCountModel RowCount(string schemaName, string tableName, long rowCount)
        => new()
        {
            SchemaName = schemaName,
            TableName = tableName,
            RowCount = rowCount
        };
}
