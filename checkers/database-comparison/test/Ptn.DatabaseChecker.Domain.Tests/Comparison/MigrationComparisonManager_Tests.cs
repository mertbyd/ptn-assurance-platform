using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Comparison;

// islevi: MigrationComparisonManager'in __EFMigrationsHistory farklarini yon bilgisiyle urettigini dogrular.
// sistemdeki gorevi: T7 migration acceptance'ini provider baglantisina cikmadan saf domain testiyle korur.
public class MigrationComparisonManager_Tests
{
    [Fact]
    public void Should_Report_Source_And_Target_Only_Migrations()
    {
        var manager = new MigrationComparisonManager();
        var source = new List<MigrationHistoryEntryModel>
        {
            Migration("20260701090000_CreateUsers", "10.0.2"),
            Migration("20260701100000_AddInvoices", "10.0.2")
        };
        var target = new List<MigrationHistoryEntryModel>
        {
            Migration("20260701090000_CreateUsers", "10.0.2", "dbo"),
            Migration("20260701110000_AddPayments", "10.0.2", "dbo")
        };

        var differences = manager.Compare(source, target);

        differences.Count.ShouldBe(2);
        differences.ShouldContain(difference =>
            difference.MigrationId == "20260701100000_AddInvoices" &&
            difference.KindCode == DifferenceKindCodes.OnlyInSource &&
            difference.SourceSchemaName == "public" &&
            difference.TargetSchemaName == null &&
            difference.SourceProductVersion == "10.0.2" &&
            difference.TargetProductVersion == null);
        differences.ShouldContain(difference =>
            difference.MigrationId == "20260701110000_AddPayments" &&
            difference.KindCode == DifferenceKindCodes.OnlyInTarget &&
            difference.SourceSchemaName == null &&
            difference.TargetSchemaName == "dbo" &&
            difference.SourceProductVersion == null &&
            difference.TargetProductVersion == "10.0.2");
    }

    [Fact]
    public void Should_Report_Product_Version_Difference_As_Modified()
    {
        var manager = new MigrationComparisonManager();
        var source = new List<MigrationHistoryEntryModel>
        {
            Migration("20260701090000_CreateUsers", "10.0.1")
        };
        var target = new List<MigrationHistoryEntryModel>
        {
            Migration("20260701090000_CreateUsers", "10.0.2", "dbo")
        };

        var differences = manager.Compare(source, target);

        differences.Count.ShouldBe(1);
        differences[0].MigrationId.ShouldBe("20260701090000_CreateUsers");
        differences[0].KindCode.ShouldBe(DifferenceKindCodes.Modified);
        differences[0].SourceSchemaName.ShouldBe("public");
        differences[0].TargetSchemaName.ShouldBe("dbo");
        differences[0].SourceProductVersion.ShouldBe("10.0.1");
        differences[0].TargetProductVersion.ShouldBe("10.0.2");
    }

    [Fact]
    public void Should_Report_No_Difference_For_Identical_Histories()
    {
        var manager = new MigrationComparisonManager();
        var history = new List<MigrationHistoryEntryModel>
        {
            Migration("20260701090000_CreateUsers", "10.0.2"),
            Migration("20260701100000_AddInvoices", "10.0.2")
        };

        var differences = manager.Compare(history, new List<MigrationHistoryEntryModel>(history));

        differences.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Report_Nothing_For_Two_Empty_Histories()
    {
        var manager = new MigrationComparisonManager();

        var differences = manager.Compare(new List<MigrationHistoryEntryModel>(), new List<MigrationHistoryEntryModel>());

        differences.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Order_Differences_By_MigrationId()
    {
        var manager = new MigrationComparisonManager();
        var source = new List<MigrationHistoryEntryModel>
        {
            Migration("20260703_C", "10.0.2"),
            Migration("20260701_A", "10.0.2")
        };

        var differences = manager.Compare(source, new List<MigrationHistoryEntryModel>());

        differences.Count.ShouldBe(2);
        differences[0].MigrationId.ShouldBe("20260701_A");
        differences[1].MigrationId.ShouldBe("20260703_C");
        differences.ShouldAllBe(difference => difference.KindCode == DifferenceKindCodes.OnlyInSource);
    }

    [Fact]
    public void Should_Treat_Same_Migration_With_Same_Version_As_Equal()
    {
        var manager = new MigrationComparisonManager();
        var source = new List<MigrationHistoryEntryModel> { Migration("20260701_A", "10.0.2") };
        var target = new List<MigrationHistoryEntryModel> { Migration("20260701_A", "10.0.2") };

        var differences = manager.Compare(source, target);

        differences.ShouldBeEmpty();
    }

    // islevi: Test icin migration history kaydi olusturur.
    private static MigrationHistoryEntryModel Migration(
        string migrationId,
        string productVersion,
        string schemaName = "public")
        => new()
        {
            SchemaName = schemaName,
            MigrationId = migrationId,
            ProductVersion = productVersion
        };
}
