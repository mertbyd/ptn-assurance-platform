using System.Collections.Generic;
using System.Text.Json;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Comparison;

// islevi: TableDataComparisonManager'in PK, row-hash ve cell-level fark semantigini dogrular.
// sistemdeki gorevi: Esit row-count'un veri esitligi sanilmasini ve yon bilgisinin ters donmesini engelleyen T7 regresyon setidir.
public class TableDataComparisonManager_Tests
{
    [Fact]
    public void Should_Report_Modified_Cell_When_Row_Counts_Are_Equal()
    {
        var manager = new TableDataComparisonManager();
        var source = Table(Row(("id", "1"), ("name", "Ada")));
        var target = Table(Row(("id", "1"), ("name", "Grace")));

        var result = manager.Compare(RequestedTables(), new() { source }, new() { target }, FullRetentionPolicy());

        var tableDifference = result.ShouldHaveSingleItem();
        tableDifference.KindCode.ShouldBe(DifferenceKindCodes.Modified);
        tableDifference.SourceRowCount.ShouldBe(1);
        tableDifference.TargetRowCount.ShouldBe(1);
        tableDifference.SourceHash.ShouldNotBe(tableDifference.TargetHash);
        var rowDifference = tableDifference.RowDifferences.ShouldHaveSingleItem();
        rowDifference.PrimaryKeyValue.ShouldBe("1");
        rowDifference.KindCode.ShouldBe(DifferenceKindCodes.Modified);
        var valueDifference = rowDifference.ValueDifferences.ShouldHaveSingleItem();
        valueDifference.ColumnName.ShouldBe("name");
        valueDifference.SourceValue.ShouldBe("Ada");
        valueDifference.TargetValue.ShouldBe("Grace");
    }

    [Fact]
    public void Should_Report_Primary_Key_Rows_With_Correct_Direction()
    {
        var manager = new TableDataComparisonManager();
        var source = Table(Row(("id", "1")), Row(("id", "2")));
        var target = Table(Row(("id", "1")), Row(("id", "3")));

        var result = manager.Compare(RequestedTables(), new() { source }, new() { target }, FullRetentionPolicy());

        var rows = result.ShouldHaveSingleItem().RowDifferences;
        rows.Count.ShouldBe(2);
        rows.ShouldContain(row => row.PrimaryKeyValue == "2" && row.KindCode == DifferenceKindCodes.OnlyInSource);
        rows.ShouldContain(row => row.PrimaryKeyValue == "3" && row.KindCode == DifferenceKindCodes.OnlyInTarget);
    }

    [Fact]
    public void Should_Use_Whole_Row_Multiset_When_Table_Has_No_Primary_Key()
    {
        var manager = new TableDataComparisonManager();
        var source = TableWithoutPrimaryKey(Row(("name", "Ada")));
        var target = TableWithoutPrimaryKey(Row(("name", "Grace")));

        var result = manager.Compare(RequestedTables(), new() { source }, new() { target }, FullRetentionPolicy());

        var rows = result.ShouldHaveSingleItem().RowDifferences;
        rows.Count.ShouldBe(2);
        rows.ShouldContain(row => row.KindCode == DifferenceKindCodes.OnlyInSource);
        rows.ShouldContain(row => row.KindCode == DifferenceKindCodes.OnlyInTarget);
        rows.ShouldAllBe(row => row.ValueDifferences.Count == 0);
    }

    [Fact]
    public void Should_Return_No_Difference_For_Equivalent_Tables()
    {
        var manager = new TableDataComparisonManager();
        var source = Table(Row(("id", "1"), ("name", "Ada")));
        var target = Table(Row(("name", "Ada"), ("id", "1")));

        var result = manager.Compare(RequestedTables(), new() { source }, new() { target }, FullRetentionPolicy());

        result.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Distinguish_Sql_Null_From_Literal_Null_Text()
    {
        var manager = new TableDataComparisonManager();
        var source = Table(Row(("id", "1"), ("name", null)));
        var target = Table(Row(("id", "1"), ("name", "<NULL>")));

        var result = manager.Compare(RequestedTables(), new() { source }, new() { target }, FullRetentionPolicy());

        var tableDifference = result.ShouldHaveSingleItem();
        tableDifference.SourceHash.ShouldNotBe(tableDifference.TargetHash);
        var rowDifference = tableDifference.RowDifferences.ShouldHaveSingleItem();
        rowDifference.KindCode.ShouldBe(DifferenceKindCodes.Modified);
        var valueDifference = rowDifference.ValueDifferences.ShouldHaveSingleItem();
        valueDifference.ColumnName.ShouldBe("name");
        valueDifference.SourceValue.ShouldBeNull();
        valueDifference.TargetValue.ShouldBe("<NULL>");
    }

    [Fact]
    public void Should_Fall_Back_To_Multiset_When_Primary_Key_Has_Duplicates()
    {
        var manager = new TableDataComparisonManager();
        var source = Table(Row(("id", "1"), ("name", "Ada")), Row(("id", "1"), ("name", "Bob")));
        var target = Table(Row(("id", "1"), ("name", "Ada")), Row(("id", "1"), ("name", "Carol")));

        var result = manager.Compare(RequestedTables(), new() { source }, new() { target }, FullRetentionPolicy());

        var rows = result.ShouldHaveSingleItem().RowDifferences;
        rows.Count.ShouldBe(2);
        rows.ShouldContain(row => row.KindCode == DifferenceKindCodes.OnlyInSource);
        rows.ShouldContain(row => row.KindCode == DifferenceKindCodes.OnlyInTarget);
        rows.ShouldAllBe(row => row.ValueDifferences.Count == 0);
    }

    [Fact]
    public void Should_Report_Table_Present_Only_In_Source_Snapshot_As_OnlyInSource()
    {
        var manager = new TableDataComparisonManager();
        var source = Table(Row(("id", "1"), ("name", "Ada")), Row(("id", "2"), ("name", "Bob")));

        var result = manager.Compare(RequestedTables(), new() { source }, new(), FullRetentionPolicy());

        var tableDifference = result.ShouldHaveSingleItem();
        tableDifference.KindCode.ShouldBe(DifferenceKindCodes.OnlyInSource);
        tableDifference.SourceRowCount.ShouldBe(2);
        tableDifference.TargetRowCount.ShouldBeNull();
        tableDifference.RowDifferences.Count.ShouldBe(2);
        tableDifference.RowDifferences.ShouldAllBe(row => row.KindCode == DifferenceKindCodes.OnlyInSource);
    }

    [Fact]
    public void Should_Report_Table_Present_Only_In_Target_Snapshot_As_OnlyInTarget()
    {
        var manager = new TableDataComparisonManager();
        var target = Table(Row(("id", "1"), ("name", "Ada")));

        var result = manager.Compare(RequestedTables(), new(), new() { target }, FullRetentionPolicy());

        var tableDifference = result.ShouldHaveSingleItem();
        tableDifference.KindCode.ShouldBe(DifferenceKindCodes.OnlyInTarget);
        tableDifference.SourceRowCount.ShouldBeNull();
        tableDifference.RowDifferences.ShouldAllBe(row => row.KindCode == DifferenceKindCodes.OnlyInTarget);
    }

    [Fact]
    public void Should_Report_No_Difference_When_Requested_Table_Absent_On_Both_Sides()
    {
        var manager = new TableDataComparisonManager();

        var result = manager.Compare(RequestedTables(), new(), new(), FullRetentionPolicy());

        result.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Match_Rows_By_Composite_Primary_Key()
    {
        var manager = new TableDataComparisonManager();
        var columns = new List<string> { "id", "region", "name" };
        var primaryKey = new List<string> { "id", "region" };
        var source = TableWith(columns, primaryKey, Row(("id", "1"), ("region", "EU"), ("name", "Ada")));
        var target = TableWith(columns, primaryKey, Row(("id", "1"), ("region", "EU"), ("name", "Grace")));

        var result = manager.Compare(RequestedTables(), new() { source }, new() { target }, FullRetentionPolicy());

        var rowDifference = result.ShouldHaveSingleItem().RowDifferences.ShouldHaveSingleItem();
        rowDifference.KindCode.ShouldBe(DifferenceKindCodes.Modified);
        rowDifference.PrimaryKeyValue.ShouldBe("1|EU");
        rowDifference.ValueDifferences.ShouldHaveSingleItem().ColumnName.ShouldBe("name");
    }

    [Fact]
    public void Should_Report_Every_Changed_Cell_In_A_Modified_Row()
    {
        var manager = new TableDataComparisonManager();
        var columns = new List<string> { "id", "name", "city" };
        var primaryKey = new List<string> { "id" };
        var source = TableWith(columns, primaryKey, Row(("id", "1"), ("name", "Ada"), ("city", "NYC")));
        var target = TableWith(columns, primaryKey, Row(("id", "1"), ("name", "Eve"), ("city", "LA")));

        var result = manager.Compare(RequestedTables(), new() { source }, new() { target }, FullRetentionPolicy());

        var valueDifferences = result.ShouldHaveSingleItem().RowDifferences.ShouldHaveSingleItem().ValueDifferences;
        valueDifferences.Count.ShouldBe(2);
        valueDifferences.ShouldContain(value => value.ColumnName == "name" && value.SourceValue == "Ada" && value.TargetValue == "Eve");
        valueDifferences.ShouldContain(value => value.ColumnName == "city" && value.SourceValue == "NYC" && value.TargetValue == "LA");
    }

    [Fact]
    public void Should_Return_No_Difference_For_Identical_MultiRow_Tables_Regardless_Of_Order()
    {
        var manager = new TableDataComparisonManager();
        var source = Table(Row(("id", "1"), ("name", "Ada")), Row(("id", "2"), ("name", "Bob")));
        var target = Table(Row(("id", "2"), ("name", "Bob")), Row(("id", "1"), ("name", "Ada")));

        var result = manager.Compare(RequestedTables(), new() { source }, new() { target }, FullRetentionPolicy());

        result.ShouldBeEmpty();
    }

    [Fact]
    public void None_Retention_Should_Not_Expose_Any_Source_Value_In_Finding()
    {
        var manager = new TableDataComparisonManager();
        var source = Table(Row(("id", "customer-123"), ("name", "ReadableSourceValue")));
        var target = Table(Row(("id", "customer-123"), ("name", "ReadableTargetValue")));

        var result = manager.Compare(
            RequestedTables(),
            new() { source },
            new() { target },
            new ValueRetentionPolicy(ValueRetentionModeCodes.None, string.Empty));

        var serializedFinding = JsonSerializer.Serialize(result);
        serializedFinding.ShouldNotContain("customer-123");
        serializedFinding.ShouldNotContain("ReadableSourceValue");
        serializedFinding.ShouldNotContain("ReadableTargetValue");
        var rowDifference = result.ShouldHaveSingleItem().RowDifferences.ShouldHaveSingleItem();
        rowDifference.PrimaryKeyValue.ShouldBeEmpty();
        rowDifference.ValueDifferences.ShouldHaveSingleItem().SourceValue.ShouldBeNull();
        rowDifference.ValueDifferences.ShouldHaveSingleItem().TargetValue.ShouldBeNull();
    }

    [Fact]
    public void Retention_Mode_Should_Not_Change_Difference_Detection_Or_Address()
    {
        var manager = new TableDataComparisonManager();
        var source = Table(Row(("id", "1"), ("name", "Ada")));
        var target = Table(Row(("id", "1"), ("name", "Grace")));

        var noneResult = manager.Compare(
            RequestedTables(), new() { source }, new() { target },
            new ValueRetentionPolicy(ValueRetentionModeCodes.None, string.Empty));
        var hashedResult = manager.Compare(
            RequestedTables(), new() { source }, new() { target },
            new ValueRetentionPolicy(ValueRetentionModeCodes.Hashed, TestSalt()));

        noneResult.Count.ShouldBe(hashedResult.Count);
        noneResult[0].SchemaName.ShouldBe(hashedResult[0].SchemaName);
        noneResult[0].TableName.ShouldBe(hashedResult[0].TableName);
        noneResult[0].KindCode.ShouldBe(hashedResult[0].KindCode);
        noneResult[0].RowDifferences.Count.ShouldBe(hashedResult[0].RowDifferences.Count);
        noneResult[0].RowDifferences[0].KindCode.ShouldBe(hashedResult[0].RowDifferences[0].KindCode);
        noneResult[0].RowDifferences[0].ValueDifferences[0].ColumnName
            .ShouldBe(hashedResult[0].RowDifferences[0].ValueDifferences[0].ColumnName);
        noneResult[0].SourceHash.ShouldBe(hashedResult[0].SourceHash);
        noneResult[0].TargetHash.ShouldBe(hashedResult[0].TargetHash);
    }

    [Fact]
    public void Same_Input_And_Policy_Should_Produce_Deterministic_Findings()
    {
        var manager = new TableDataComparisonManager();
        var source = Table(Row(("id", "1"), ("name", "Ada")));
        var target = Table(Row(("id", "1"), ("name", "Grace")));
        var policy = new ValueRetentionPolicy(ValueRetentionModeCodes.Hashed, TestSalt());

        var first = manager.Compare(RequestedTables(), new() { source }, new() { target }, policy);
        var second = manager.Compare(RequestedTables(), new() { source }, new() { target }, policy);

        JsonSerializer.Serialize(first).ShouldBe(JsonSerializer.Serialize(second));
    }

    // islevi: Mevcut diff semantigi testlerinde ham degerlerin gorunur kalmasini saglayan acik Full politika kurar.
    private static ValueRetentionPolicy FullRetentionPolicy()
        => new(ValueRetentionModeCodes.Full, string.Empty);

    // islevi: HMAC testlerinin gercek veya takip edilen bir salt degeri tasimadan deterministik anahtar kullanmasini saglar.
    private static string TestSalt()
        => new('s', 16);

    // islevi: Test icin ozel kolon/PK yapisina sahip tablo fotografi kurar (composite PK ve cok kolon senaryolari).
    private static TableDataSnapshotModel TableWith(
        List<string> columnNames,
        List<string> primaryKeyColumns,
        params TableDataRowModel[] rows)
        => new()
        {
            SchemaName = "public",
            TableName = "people",
            RowCount = rows.Length,
            ColumnNames = columnNames,
            PrimaryKeyColumns = primaryKeyColumns,
            Rows = new(rows)
        };

    // islevi: Test motoruna tek public.people tablo adresini verir.
    private static List<ComparisonTableIdentifierModel> RequestedTables()
        => new()
        {
            new()
            {
                SchemaName = "public",
                TableName = "people"
            }
        };

    // islevi: PK'li tablo fotografini verilen satirlarla kurar.
    private static TableDataSnapshotModel Table(params TableDataRowModel[] rows)
        => new()
        {
            SchemaName = "public",
            TableName = "people",
            RowCount = rows.Length,
            ColumnNames = new() { "id", "name" },
            PrimaryKeyColumns = new() { "id" },
            Rows = new(rows)
        };

    // islevi: PK fallback yolunu dogrulamak icin anahtarsiz tablo fotografi kurar.
    private static TableDataSnapshotModel TableWithoutPrimaryKey(params TableDataRowModel[] rows)
    {
        var table = Table(rows);
        table.PrimaryKeyColumns.Clear();
        return table;
    }

    // islevi: Kolon/deger ciftlerinden case-insensitive test satiri olusturur.
    private static TableDataRowModel Row(params (string Column, string? Value)[] values)
    {
        var row = new TableDataRowModel();
        foreach (var value in values)
        {
            row.Values[value.Column] = value.Value;
        }

        return row;
    }
}
