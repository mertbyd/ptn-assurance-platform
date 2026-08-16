using System.Collections.Generic;
using System.Linq;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Managers.Reports;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Reports;

// islevi: Zenginlestirilmis rapor icerigi uretimini (ne degisti + kaynak->hedef + satir sayilari + is-anlami) dogrular.
// sistemdeki gorevi: Raporun "yetersiz" olmamasinin regresyon muhafizi; teknik kodun yaninda ChangeSummary, tanim kaniti, row-count ve hucre detayinin HTML/Markdown ciktisinda bulundugunu pinler.
public class ComparisonReportContentGenerator_Tests
{
    private static ComparisonFindings RichFindings() => new()
    {
        SchemaDifferences = new List<SchemaDifferenceModel>
        {
            new()
            {
                SchemaName = "person", ObjectName = "person", ChildName = "title",
                ObjectTypeCode = SchemaObjectTypeCodes.Column, KindCode = DifferenceKindCodes.Modified,
                ChangeSummary = "DataType, MaxLength",
                SourceDefinition = "type=varchar;max=8", TargetDefinition = "type=varchar;max=20"
            },
            new()
            {
                SchemaName = "ridership", ObjectName = "refunds",
                ObjectTypeCode = SchemaObjectTypeCodes.Table, KindCode = DifferenceKindCodes.OnlyInTarget,
                TargetDefinition = "ridership.refunds"
            }
        },
        MigrationDifferences = new List<MigrationDifferenceModel>
        {
            new()
            {
                SourceSchemaName = "public", TargetSchemaName = "dbo",
                MigrationId = "20260101_Init", KindCode = DifferenceKindCodes.Modified,
                SourceProductVersion = "10.0.1", TargetProductVersion = "10.0.2"
            }
        },
        DataDifferences = new List<DataDifferenceModel>
        {
            new()
            {
                SchemaName = "fares", TableName = "fare_prices", KindCode = DifferenceKindCodes.Modified,
                SourceRowCount = 60, TargetRowCount = 60, RowCountDifference = 0,
                RowDifferences = new List<DataRowDifferenceModel>
                {
                    new()
                    {
                        PrimaryKeyValue = "1", KindCode = DifferenceKindCodes.Modified,
                        ValueDifferences = new List<DataValueDifferenceModel>
                        {
                            new() { ColumnName = "amount", SourceValue = "3", TargetValue = "3.5" }
                        }
                    }
                }
            }
        }
    };

    [Fact]
    public void Html_Report_Includes_What_Changed_Source_Target_And_Row_Detail()
    {
        var html = new ComparisonReportContentGenerator().Generate(RichFindings())
            .Single(c => c.FormatCode == ReportFormatCodes.Html).Content;

        // business-language direction (not just the technical code)
        html.ShouldContain(ComparisonReportTextConstants.KindModifiedText);
        html.ShouldContain(ComparisonReportTextConstants.KindOnlyInTargetText);
        // schema: what changed + source->target evidence
        html.ShouldContain("person.person.title");
        html.ShouldContain(ComparisonReportTextConstants.ChangedFieldsLabel);
        html.ShouldContain("DataType, MaxLength");
        html.ShouldContain("type=varchar;max=8");
        html.ShouldContain("type=varchar;max=20");
        html.ShouldContain(ComparisonReportTextConstants.SourceTargetArrow.Trim());
        // migration: version drift
        html.ShouldContain(ComparisonReportTextConstants.MigrationSchemaLabel);
        html.ShouldContain("public");
        html.ShouldContain("dbo");
        html.ShouldContain(ComparisonReportTextConstants.MigrationVersionLabel);
        html.ShouldContain("10.0.1");
        html.ShouldContain("10.0.2");
        // data: row counts + cell-level change
        html.ShouldContain(ComparisonReportTextConstants.RowsLabel);
        html.ShouldContain("fares.fare_prices");
        html.ShouldContain("amount");
        html.ShouldContain("3.5");
        // object-type breakdown present
        html.ShouldContain("Breakdown by object type");
    }

    [Fact]
    public void Markdown_Report_Includes_Change_And_Row_Detail()
    {
        var md = new ComparisonReportContentGenerator().Generate(RichFindings())
            .Single(c => c.FormatCode == ReportFormatCodes.Markdown).Content;

        md.ShouldContain("person.person.title");
        md.ShouldContain("DataType, MaxLength");
        md.ShouldContain(ComparisonReportTextConstants.KindModifiedText);
        md.ShouldContain(ComparisonReportTextConstants.MarkdownMigrationSchema.Trim());
        md.ShouldContain("public");
        md.ShouldContain("dbo");
        md.ShouldContain("10.0.1");
        md.ShouldContain("10.0.2");
    }

    [Fact]
    public void Empty_Findings_Still_Render_Without_Errors()
    {
        var contents = new ComparisonReportContentGenerator().Generate(new ComparisonFindings());
        contents.Single(c => c.FormatCode == ReportFormatCodes.Html).Content.ShouldContain("Comparison Results");
    }
}
