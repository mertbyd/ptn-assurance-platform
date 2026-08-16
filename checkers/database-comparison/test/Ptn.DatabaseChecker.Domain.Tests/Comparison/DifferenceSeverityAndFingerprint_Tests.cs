using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Comparison;

// islevi: Siddet yon/asimetri kurallari ile kararli bulgu parmak izini birlikte dogrular.
// sistemdeki gorevi: MCP bulgu kimligi ve uyumluluk etkisinin run sirasindan ya da ilgisiz bulgulardan etkilenmesini engeller.
/// <summary>
/// Siddet asimetrisi ve kararli bulgu parmak izi kurallarini dogrular.
/// </summary>
public class DifferenceSeverityAndFingerprint_Tests
{
    [Fact]
    public void Column_Severity_Should_Respect_Narrowing_Widening_Documentation_And_Role()
    {
        var classifier = new DifferenceSeverityClassifier();
        classifier.Classify(ModifiedColumn("nullable=True;max=100", "nullable=False;max=100", "Nullable"), ComparisonSideRoleCodes.Reference)
            .ShouldBe(DifferenceSeverityCodes.Breaking);
        classifier.Classify(ModifiedColumn("nullable=True;max=100", "nullable=True;max=200", "MaxLength"), ComparisonSideRoleCodes.Reference)
            .ShouldBe(DifferenceSeverityCodes.NonBreaking);
        classifier.Classify(ModifiedColumn("canonicalType=Integer", "canonicalType=BigInteger", "DataType"), ComparisonSideRoleCodes.Reference)
            .ShouldBe(DifferenceSeverityCodes.NonBreaking);
        classifier.Classify(ModifiedColumn("canonicalType=BigInteger", "canonicalType=Integer", "DataType"), ComparisonSideRoleCodes.Reference)
            .ShouldBe(DifferenceSeverityCodes.Breaking);
        classifier.Classify(ModifiedColumn("nullable=True;max=100", "nullable=True;max=100", "Comment"), ComparisonSideRoleCodes.Reference)
            .ShouldBe(DifferenceSeverityCodes.DocsOnly);

        var approximateBreaking = ModifiedColumn("nullable=True;max=100", "nullable=False;max=100", "Nullable");
        approximateBreaking.ConfidenceCode = ComparisonConfidenceCodes.Approximate;
        classifier.Classify(approximateBreaking, ComparisonSideRoleCodes.Reference)
            .ShouldBe(DifferenceSeverityCodes.Warning);

        var auditedDefaultedColumn = PresenceDifference(DifferenceKindCodes.OnlyInTarget);
        auditedDefaultedColumn.ObjectTypeCode = SchemaObjectTypeCodes.Column;
        auditedDefaultedColumn.TargetDefinition = "nullable=False;default=0";
        classifier.Classify(auditedDefaultedColumn, ComparisonSideRoleCodes.Reference)
            .ShouldBe(DifferenceSeverityCodes.NonBreaking);

        var missingTarget = PresenceDifference(DifferenceKindCodes.OnlyInSource);
        classifier.Classify(missingTarget, ComparisonSideRoleCodes.Reference).ShouldBe(DifferenceSeverityCodes.Breaking);
        classifier.Classify(missingTarget, ComparisonSideRoleCodes.Audited).ShouldBe(DifferenceSeverityCodes.NonBreaking);
    }

    [Fact]
    public void Fingerprint_Should_Be_Stable_For_Repeated_Input_And_Unrelated_Findings()
    {
        var calculator = new FindingFingerprintCalculator();
        var finding = ModifiedColumn("nullable=True;max=100", "nullable=True;max=200", "MaxLength");
        var first = calculator.Calculate(DatabaseEngineCodes.PostgreSql, DatabaseEngineCodes.SqlServer, finding);
        var repeated = calculator.Calculate(DatabaseEngineCodes.PostgreSql, DatabaseEngineCodes.SqlServer, finding);
        var unrelated = PresenceDifference(DifferenceKindCodes.OnlyInTarget);
        _ = calculator.Calculate(DatabaseEngineCodes.PostgreSql, DatabaseEngineCodes.SqlServer, unrelated);
        var afterUnrelated = calculator.Calculate(DatabaseEngineCodes.PostgreSql, DatabaseEngineCodes.SqlServer, finding);

        repeated.ShouldBe(first);
        afterUnrelated.ShouldBe(first);
    }

    [Fact]
    public void Finding_Address_Grammar_Should_Preserve_Component_Order_Nulls_And_Quoted_Display()
    {
        FindingAddressGrammar.FingerprintComponentOrder.ShouldBe(
            "SourceEngineCode,TargetEngineCode,SchemaName,ObjectTypeCode,ObjectName,ChildName");
        FindingAddressGrammar.BuildFingerprintAddress(
                DatabaseEngineCodes.PostgreSql,
                DatabaseEngineCodes.SqlServer,
                null,
                SchemaObjectTypeCodes.Column,
                "orders",
                string.Empty)
            .ShouldBe("V10:PostgreSqlV9:SqlServerNV6:ColumnV6:ordersV0:");
        FindingAddressGrammar.FormatTargetAddress("public", "orders", "amount")
            .ShouldBe("public.orders.amount");
        FindingAddressGrammar.FormatTargetAddress("Sales Schema", "Order.Items", "Amount\"Gross")
            .ShouldBe("\"Sales Schema\".\"Order.Items\".\"Amount\"\"Gross\"");
        FindingAddressGrammar.FormatTargetAddress(null, "orders", null)
            .ShouldBe("<default>.orders");
        FindingAddressGrammar.FormatTargetAddress(string.Empty, "orders", null)
            .ShouldBe("<empty>.orders");
    }

    // islevi: Kolon modified bulgusunu classifier/fingerprint testleri icin kurar.
    private static SchemaDifferenceModel ModifiedColumn(string source, string target, string summary)
        => new()
        {
            SchemaName = "public",
            ObjectName = "orders",
            ChildName = "amount",
            ObjectTypeCode = SchemaObjectTypeCodes.Column,
            KindCode = DifferenceKindCodes.Modified,
            ConfidenceCode = ComparisonConfidenceCodes.Exact,
            SourceDefinition = source,
            TargetDefinition = target,
            ChangeSummary = summary
        };

    // islevi: Tek tarafli tablo bulgusunu rol asimetrisi ve ilgisiz fingerprint girdisi icin kurar.
    private static SchemaDifferenceModel PresenceDifference(string kindCode)
        => new()
        {
            SchemaName = "public",
            ObjectName = "audit_log",
            ObjectTypeCode = SchemaObjectTypeCodes.Table,
            KindCode = kindCode,
            ConfidenceCode = ComparisonConfidenceCodes.Exact,
            SourceDefinition = kindCode == DifferenceKindCodes.OnlyInSource ? "public.audit_log" : null,
            TargetDefinition = kindCode == DifferenceKindCodes.OnlyInTarget ? "public.audit_log" : null
        };
}
