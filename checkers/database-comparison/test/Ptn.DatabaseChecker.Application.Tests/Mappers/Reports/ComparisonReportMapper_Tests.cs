using Ptn.DatabaseChecker.Application.Mappers.Reports;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Mappers.Reports;

// islevi: Rapor Mapperly eslemelerinin migration finding alanlarini kayipsiz tasidigini dogrular.
// sistemdeki gorevi: Kaynak ve hedef migration semalarinin API DTO'suna ulasmasini regresyona karsi korur.
public class ComparisonReportMapper_Tests
{
    [Fact]
    public void Should_Map_Migration_Source_And_Target_Schema_Names()
    {
        var model = new MigrationDifferenceModel
        {
            SourceSchemaName = "public",
            TargetSchemaName = "dbo",
            MigrationId = "20260701090000_CreateUsers",
            KindCode = "Modified",
            SourceProductVersion = "10.0.1",
            TargetProductVersion = "10.0.2"
        };

        var dto = new ComparisonReportMapper().MapToDto(model);

        dto.SourceSchemaName.ShouldBe("public");
        dto.TargetSchemaName.ShouldBe("dbo");
        dto.MigrationId.ShouldBe(model.MigrationId);
        dto.KindCode.ShouldBe(model.KindCode);
        dto.SourceProductVersion.ShouldBe(model.SourceProductVersion);
        dto.TargetProductVersion.ShouldBe(model.TargetProductVersion);
    }
}
