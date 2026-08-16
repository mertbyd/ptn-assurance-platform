using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Application.Mappers.SchemaDiscovery;
using Ptn.DatabaseChecker.Models.Comparison;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Application.Tests.Mappers.SchemaDiscovery;

// islevi: KBP-703 snapshot derinlik alanlarinin Mapperly tarafindan API DTO'larina tasindigini dogrular.
// sistemdeki gorevi: Domain snapshot ile SchemaDiscovery public cevap kontrati arasinda yeni alan kaybi olmasini engeller.
public class SchemaDiscoveryMapper_Tests
{
    // islevi: Snapshot, kolon, constraint ve trigger derinlik alanlarini tek zengin modelden DTO'ya mapler.
    [Fact]
    public void Should_Map_Collation_Trust_And_Column_Depth_Fields()
    {
        var snapshot = CreateSnapshot();

        var dto = new SchemaDiscoveryMapper().MapToSnapshotDto(snapshot);

        dto.DatabaseCollationName.ShouldBe("tr_TR");
        dto.CollationProviderCode.ShouldBe("i");
        dto.Tables[0].Columns[0].GenerationExpression.ShouldBe("price * quantity");
        dto.Tables[0].Columns[0].IdentitySeed.ShouldBe("100");
        dto.Tables[0].Columns[0].Comment.ShouldBe("Calculated total");
        dto.Tables[0].Constraints[0].IsValidated.ShouldBeFalse();
        dto.Tables[0].Triggers[0].IsEnabled.ShouldBeFalse();
    }

    // islevi: Domain lint uyarisinin kararli kod ve kolon adresini public DescribeTable DTO'suna tasir.
    [Fact]
    public void Should_Map_Schema_Lint_Warnings()
    {
        var description = new TableDescriptionModel
        {
            LintWarnings =
            [
                new SchemaLintWarningModel
                {
                    WarningCode = SchemaLintWarningCodes.GeneratedColumn,
                    ColumnName = "calculated_total"
                }
            ]
        };

        var dto = new SchemaDiscoveryMapper().MapToTableDescriptionDto(description);

        dto.LintWarnings.Single().WarningCode.ShouldBe(SchemaLintWarningCodes.GeneratedColumn);
        dto.LintWarnings.Single().ColumnName.ShouldBe("calculated_total");
    }

    // islevi: Mapper testinin tum KBP-703 alanlarini dolduran zengin snapshot girdisini kurar.
    private static SchemaSnapshotModel CreateSnapshot()
        => new()
        {
            DatabaseCollationName = "tr_TR",
            CollationProviderCode = "i",
            Tables =
            [
                new SchemaTableModel
                {
                    Columns = [CreateColumn()],
                    Constraints = [new SchemaConstraintModel { IsValidated = false, IsEnabled = false }],
                    Triggers = [new SchemaTriggerModel { IsEnabled = false }]
                }
            ]
        };

    // islevi: Mapper testinin collation/generated/identity/comment alanlari dolu kolonunu kurar.
    private static SchemaColumnModel CreateColumn()
        => new()
        {
            CollationName = "tr_TR",
            IsGenerated = true,
            GenerationExpression = "price * quantity",
            IsPersisted = true,
            IdentitySeed = "100",
            IdentityIncrement = "10",
            Comment = "Calculated total"
        };
}
