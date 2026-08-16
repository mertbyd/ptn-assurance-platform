using System.Collections.Generic;
using System.Linq;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Managers.Reports;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Reports;

// islevi: ComparisonReportBuilder'in bulgulardan urettigi ozet sayaclari + tur/tablo gruplamasini dogrular.
// sistemdeki gorevi: Raporun aggregation kismi (GET {id}/report ozet + gruplar) icin kalici regresyon; sayim/yon/gruplama mantigi provider ve Mapperly'den bagimsiz saf domain testiyle korunur.
public class ComparisonReportBuilder_Tests
{
    [Fact]
    public void Empty_Findings_Produce_Zero_Summary_And_No_Groups()
    {
        var aggregation = new ComparisonReportBuilder().Build(new ComparisonFindings());

        aggregation.Summary.TotalDifferenceCount.ShouldBe(0);
        aggregation.Summary.SchemaDifferenceCount.ShouldBe(0);
        aggregation.Summary.DataDifferenceCount.ShouldBe(0);
        aggregation.Summary.MigrationDifferenceCount.ShouldBe(0);
        aggregation.Summary.KindCounts.ShouldBeEmpty();
        aggregation.ObjectTypeGroups.ShouldBeEmpty();
        aggregation.TableGroups.ShouldBeEmpty();
    }

    [Fact]
    public void Summary_Counts_Each_Category_Separately_And_Totals_Them()
    {
        var findings = Findings(
            schema: new[]
            {
                Schema("public", "customers", SchemaObjectTypeCodes.Table, DifferenceKindCodes.OnlyInSource),
                Schema("public", "orders", SchemaObjectTypeCodes.Table, DifferenceKindCodes.OnlyInTarget)
            },
            data: new[] { Data("public", "customers", DifferenceKindCodes.Modified) },
            migration: new[] { Migration("20260101_Init", DifferenceKindCodes.OnlyInSource) });

        var summary = new ComparisonReportBuilder().Build(findings).Summary;

        summary.SchemaDifferenceCount.ShouldBe(2);
        summary.DataDifferenceCount.ShouldBe(1);
        summary.MigrationDifferenceCount.ShouldBe(1);
        summary.TotalDifferenceCount.ShouldBe(4);
    }

    [Fact]
    public void KindCounts_Aggregate_Across_All_Categories_Ordered_By_Count()
    {
        var findings = Findings(
            schema: new[]
            {
                Schema("public", "a", SchemaObjectTypeCodes.Table, DifferenceKindCodes.OnlyInSource),
                Schema("public", "b", SchemaObjectTypeCodes.Table, DifferenceKindCodes.OnlyInSource)
            },
            data: new[] { Data("public", "c", DifferenceKindCodes.OnlyInSource) },
            migration: new[] { Migration("m1", DifferenceKindCodes.OnlyInTarget) });

        var kindCounts = new ComparisonReportBuilder().Build(findings).Summary.KindCounts;

        kindCounts.First().Code.ShouldBe(DifferenceKindCodes.OnlyInSource);
        kindCounts.First().Count.ShouldBe(3);
        kindCounts.Single(count => count.Code == DifferenceKindCodes.OnlyInTarget).Count.ShouldBe(1);
    }

    [Fact]
    public void ObjectType_Groups_Are_Ordered_By_Count_Descending()
    {
        var findings = Findings(schema: new[]
        {
            Schema("public", "t", SchemaObjectTypeCodes.Column, DifferenceKindCodes.Modified),
            Schema("public", "t", SchemaObjectTypeCodes.Column, DifferenceKindCodes.OnlyInSource),
            Schema("public", "t", SchemaObjectTypeCodes.Column, DifferenceKindCodes.OnlyInTarget),
            Schema("public", "t", SchemaObjectTypeCodes.Index, DifferenceKindCodes.Modified)
        });

        var groups = new ComparisonReportBuilder().Build(findings).ObjectTypeGroups;

        groups.First().GroupKey.ShouldBe(SchemaObjectTypeCodes.Column);
        groups.First().DifferenceCount.ShouldBe(3);
        groups.Last().GroupKey.ShouldBe(SchemaObjectTypeCodes.Index);
        groups.Last().DifferenceCount.ShouldBe(1);
    }

    [Fact]
    public void Table_Group_Key_Is_Schema_Dot_Object()
    {
        var findings = Findings(schema: new[]
        {
            Schema("sales", "orders", SchemaObjectTypeCodes.Table, DifferenceKindCodes.Modified),
            Schema("sales", "orders", SchemaObjectTypeCodes.Column, DifferenceKindCodes.Modified)
        });

        var groups = new ComparisonReportBuilder().Build(findings).TableGroups;

        var group = groups.ShouldHaveSingleItem();
        group.GroupKey.ShouldBe("sales.orders");
        group.DifferenceCount.ShouldBe(2);
    }

    [Fact]
    public void Migration_And_Data_Entries_Use_Category_Codes_As_Object_Type()
    {
        var findings = Findings(
            data: new[] { Data("public", "customers", DifferenceKindCodes.Modified) },
            migration: new[] { Migration("20260101_Init", DifferenceKindCodes.OnlyInSource) });

        var objectTypeCounts = new ComparisonReportBuilder().Build(findings).Summary.ObjectTypeCounts;

        objectTypeCounts.ShouldContain(count => count.Code == ComparisonReportCategoryCodes.Data && count.Count == 1);
        objectTypeCounts.ShouldContain(count => count.Code == ComparisonReportCategoryCodes.Migration && count.Count == 1);
    }

    [Fact]
    public void Group_KindCounts_Reflect_Directions_Within_The_Group()
    {
        var findings = Findings(schema: new[]
        {
            Schema("public", "t", SchemaObjectTypeCodes.Column, DifferenceKindCodes.OnlyInSource),
            Schema("public", "t", SchemaObjectTypeCodes.Column, DifferenceKindCodes.OnlyInSource),
            Schema("public", "t", SchemaObjectTypeCodes.Column, DifferenceKindCodes.Modified)
        });

        var group = new ComparisonReportBuilder().Build(findings).ObjectTypeGroups.ShouldHaveSingleItem();

        group.KindCounts.Single(count => count.Code == DifferenceKindCodes.OnlyInSource).Count.ShouldBe(2);
        group.KindCounts.Single(count => count.Code == DifferenceKindCodes.Modified).Count.ShouldBe(1);
    }

    [Fact]
    public void Table_Groups_Are_Ordered_By_Count_Then_Key()
    {
        var findings = Findings(schema: new[]
        {
            Schema("public", "busy", SchemaObjectTypeCodes.Column, DifferenceKindCodes.Modified),
            Schema("public", "busy", SchemaObjectTypeCodes.Column, DifferenceKindCodes.OnlyInSource),
            Schema("public", "quiet", SchemaObjectTypeCodes.Table, DifferenceKindCodes.Modified)
        });

        var groups = new ComparisonReportBuilder().Build(findings).TableGroups;

        groups.First().GroupKey.ShouldBe("public.busy");
        groups.First().DifferenceCount.ShouldBe(2);
        groups.Last().GroupKey.ShouldBe("public.quiet");
    }

    // islevi: Uc bulgu ailesinden ComparisonFindings kurar.
    private static ComparisonFindings Findings(
        IEnumerable<SchemaDifferenceModel>? schema = null,
        IEnumerable<DataDifferenceModel>? data = null,
        IEnumerable<MigrationDifferenceModel>? migration = null)
        => new()
        {
            SchemaDifferences = schema?.ToList() ?? new List<SchemaDifferenceModel>(),
            DataDifferences = data?.ToList() ?? new List<DataDifferenceModel>(),
            MigrationDifferences = migration?.ToList() ?? new List<MigrationDifferenceModel>()
        };

    // islevi: Test icin sema fark kaydi kurar.
    private static SchemaDifferenceModel Schema(string schema, string objectName, string objectTypeCode, string kindCode)
        => new()
        {
            SchemaName = schema,
            ObjectName = objectName,
            ObjectTypeCode = objectTypeCode,
            KindCode = kindCode
        };

    // islevi: Test icin veri fark kaydi kurar.
    private static DataDifferenceModel Data(string schema, string tableName, string kindCode)
        => new()
        {
            SchemaName = schema,
            TableName = tableName,
            KindCode = kindCode
        };

    // islevi: Test icin migration fark kaydi kurar.
    private static MigrationDifferenceModel Migration(string migrationId, string kindCode)
        => new()
        {
            MigrationId = migrationId,
            KindCode = kindCode
        };
}
