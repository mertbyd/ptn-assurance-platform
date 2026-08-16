using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Managers.SchemaDiscovery;
using Ptn.DatabaseChecker.Models.Comparison;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.SchemaDiscovery;

// islevi: Hedefli schema lint kurallarinin anahtar ve generated kolon siniflandirmasini dogrular.
// sistemdeki gorevi: Senaryo yayin kapisina giden kapali uyari kodlarinin provider ve katalog sirasindan bagimsiz kalmasini korur.
public class DatabaseLintManagerTests
{
    // islevi: Anahtarsiz tabloda iki key uyarisini ve generated kolonlari ordinal sirasinda uretir.
    [Fact]
    public void Keyless_Table_Should_Report_Key_Risks_And_Generated_Columns()
    {
        var table = new SchemaTableModel
        {
            Columns =
            [
                new SchemaColumnModel { Name = "updated_total", Ordinal = 3, IsGenerated = true },
                new SchemaColumnModel { Name = "id", Ordinal = 1 },
                new SchemaColumnModel { Name = "calculated_total", Ordinal = 2, IsGenerated = true }
            ]
        };

        var warnings = new DatabaseLintManager().Evaluate(table);

        warnings.Select(item => item.WarningCode).ShouldBe(
        [
            SchemaLintWarningCodes.MissingPrimaryKey,
            SchemaLintWarningCodes.MissingUniqueKey,
            SchemaLintWarningCodes.GeneratedColumn,
            SchemaLintWarningCodes.GeneratedColumn
        ]);
        warnings.Where(item => item.WarningCode == SchemaLintWarningCodes.GeneratedColumn)
            .Select(item => item.ColumnName)
            .ShouldBe(["calculated_total", "updated_total"]);
    }

    // islevi: Filtresiz unique index'in satir anahtari sagladigini fakat PK eksigi uyarisini kapatmadigini dogrular.
    [Fact]
    public void Unique_Index_Should_Satisfy_The_Unique_Key_Gate_Without_Hiding_Missing_Primary_Key()
    {
        var table = new SchemaTableModel
        {
            Indexes =
            [
                new SchemaIndexModel
                {
                    Name = "uq_orders_external_id",
                    IsUnique = true,
                    Columns = ["external_id"]
                }
            ]
        };

        var warnings = new DatabaseLintManager().Evaluate(table);

        warnings.Select(item => item.WarningCode)
            .ShouldBe([SchemaLintWarningCodes.MissingPrimaryKey]);
    }

    // islevi: Filtreli unique index'in tum satirlari tanimlamadigi icin unique key kapisini gecemedigini dogrular.
    [Fact]
    public void Filtered_Unique_Index_Should_Not_Satisfy_The_Unique_Key_Gate()
    {
        var table = new SchemaTableModel
        {
            Indexes =
            [
                new SchemaIndexModel
                {
                    Name = "uq_orders_external_id",
                    IsUnique = true,
                    Columns = ["external_id"],
                    FilterDefinition = "external_id IS NOT NULL"
                }
            ]
        };

        var warnings = new DatabaseLintManager().Evaluate(table);

        warnings.Select(item => item.WarningCode).ShouldBe(
        [
            SchemaLintWarningCodes.MissingPrimaryKey,
            SchemaLintWarningCodes.MissingUniqueKey
        ]);
    }

    // islevi: Primary key constraint'inin hem PK hem kullanilabilir unique key kapisini gectigini dogrular.
    [Fact]
    public void Primary_Key_Constraint_Should_Satisfy_Both_Key_Gates()
    {
        var table = new SchemaTableModel
        {
            Constraints =
            [
                new SchemaConstraintModel
                {
                    Name = "pk_orders",
                    TypeCode = SchemaConstraintTypeCodes.PrimaryKey,
                    Columns = ["id"]
                }
            ]
        };

        new DatabaseLintManager().Evaluate(table).ShouldBeEmpty();
    }
}
