using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Managers.Diagnosis.Rules;
using Ptn.DatabaseChecker.Models.Assertions;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Diagnosis;

// islevi: Olgu tabanli diagnosis kurallarinin kod esitligi olmadan probe ve katalog kanitini guvene cevirmesini dogrular.
// sistemdeki gorevi: FK, scope, row-existence, value mismatch ve NOT VALID hipotezlerinin kararli regresyon kanitidir.
public class DiagnosisRules_Tests
{
    // islevi: FK parent probe Missing sonucunun Confirmed hipotez urettigini dogrular.
    [Fact]
    public void Missing_Foreign_Key_Parent_Should_Be_Confirmed()
    {
        var rule = new ForeignKeyParentMissingRule();
        var context = CreateContext();
        context.Constraint = new SchemaConstraintModel { TypeCode = SchemaConstraintTypeCodes.ForeignKey };
        context.ParentStructure = CreateStructure("parents");
        context.ParentKeyValues["id"] = "42";
        var proof = Evidence(rule.HypothesisKindCode, ProbeKindCodes.RowExists, ProbeKindCodes.Facts.Missing);

        var result = rule.Assess(new FailureIdentity(), context, new() { proof });

        rule.AppliesTo(new FailureIdentity(), context).ShouldBeTrue();
        result.ConfidenceCode.ShouldBe(DiagnosisConfidenceCodes.Confirmed);
    }

    // islevi: Dar kimlikte bulunan satirin scope hipotezini kanitlayip never-created hipotezini curuttugunu dogrular.
    [Fact]
    public void Row_In_Another_Scope_Should_Confirm_And_Never_Created_Should_Be_Ruled_Out()
    {
        var context = CreateContext();
        context.RowWasReportedMissing = true;
        context.TargetKeyValues["id"] = "42";
        context.TargetKeyValues["tenant_id"] = "7";
        context.IdentityKeyValues["id"] = "42";
        var scopeRule = new RowInAnotherScopeRule();
        var missingRule = new RowNeverCreatedRule();
        var scopeProof = Evidence(scopeRule.HypothesisKindCode, ProbeKindCodes.KeyMatchCount, ProbeKindCodes.Facts.Found);
        var rowProof = Evidence(missingRule.HypothesisKindCode, ProbeKindCodes.RowExists, ProbeKindCodes.Facts.Found);

        var scope = scopeRule.Assess(new FailureIdentity(), context, new() { scopeProof });
        var missing = missingRule.Assess(new FailureIdentity(), context, new() { rowProof });

        scope.ConfidenceCode.ShouldBe(DiagnosisConfidenceCodes.Confirmed);
        missing.ConfidenceCode.ShouldBe(DiagnosisConfidenceCodes.RuledOut);
    }

    // islevi: Value mismatch'in failed-expectation kanitini Confirmed ve redaction'li kullandigini dogrular.
    [Fact]
    public void Value_Mismatch_Should_Use_Failed_Expectations_As_Confirmed_Evidence()
    {
        var rule = new RowValueDiffersRule();
        var context = CreateContext();
        context.ValueWasReportedDifferent = true;
        context.FailedExpectations.Add(new FailedExpectation
        {
            ColumnName = "status",
            ExpectedValue = null,
            ObservedValue = null
        });

        var result = rule.Assess(new FailureIdentity(), context, new());

        result.ConfidenceCode.ShouldBe(DiagnosisConfidenceCodes.Confirmed);
        result.Evidence.Single().ExpectedValue.ShouldBeNull();
        result.Evidence.Single().ObservedValue.ShouldBeNull();
    }

    // islevi: Constraint IsValidated katalog olgusunun Likely/RuledOut sonucunu belirledigini dogrular.
    [Theory]
    [InlineData(true, DiagnosisConfidenceCodes.RuledOut)]
    [InlineData(false, DiagnosisConfidenceCodes.Likely)]
    public void Constraint_Validation_Fact_Should_Determine_Confidence(
        bool isValidated,
        string expectedConfidence)
    {
        var rule = new ConstraintNotValidatedRule();
        var context = CreateContext();
        context.Constraint = new SchemaConstraintModel { IsValidated = isValidated };

        var result = rule.Assess(new FailureIdentity(), context, new());

        result.ConfidenceCode.ShouldBe(expectedConfidence);
    }

    // islevi: Kural testleri icin katalogda dogrulanmis hedef tablo ve structure baglamini kurar.
    private static ResolvedFailureContext CreateContext()
        => new()
        {
            Table = new SchemaTableModel { Schema = "public", Name = "orders" },
            TargetStructure = CreateStructure("orders")
        };

    // islevi: Tek integer PK'li probe hedef yapisini kurar.
    private static TableDataStructureModel CreateStructure(string tableName)
        => new()
        {
            SchemaName = "public",
            TableName = tableName,
            ColumnNames = new() { "id" },
            PrimaryKeyColumns = new() { "id" },
            UniqueKeyColumnSets = new() { new() { "id" } },
            Columns = new() { new TableDataColumnModel { Name = "id", CanonicalDataTypeCode = CanonicalDataTypeCodes.Integer } }
        };

    // islevi: Tek hipoteze bagli yapilandirilmis probe kaniti kurar.
    private static ProbeEvidence Evidence(string hypothesis, string probe, string fact)
        => new() { HypothesisKindCode = hypothesis, ProbeKindCode = probe, FactCode = fact };
}
