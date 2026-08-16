using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Volo.Abp.Domain.Services;

namespace Ptn.DatabaseChecker.Managers.SchemaDiscovery;

// islevi: Tek tablonun assertion yazimini etkileyen anahtar ve generated kolon risklerini siniflandirir.
// sistemdeki gorevi: Salt-okunur katalog modelini kararli schema lint uyari kodlarina ceviren domain sahibidir.
public class DatabaseLintManager : DomainService
{
    // islevi: Eksik PK, eksik kullanilabilir unique key ve generated kolon uyarilarini kararli sirada uretir.
    public virtual List<SchemaLintWarningModel> Evaluate(SchemaTableModel table)
    {
        var warnings = new List<SchemaLintWarningModel>();
        if (!HasPrimaryKey(table))
        {
            warnings.Add(CreateWarning(SchemaLintWarningCodes.MissingPrimaryKey));
        }

        if (!HasUsableUniqueKey(table))
        {
            warnings.Add(CreateWarning(SchemaLintWarningCodes.MissingUniqueKey));
        }

        warnings.AddRange(CreateGeneratedColumnWarnings(table.Columns));
        return warnings;
    }

    // islevi: Provider modelinde index veya constraint olarak tasinan PK bilgisini tek kararda birlestirir.
    private static bool HasPrimaryKey(SchemaTableModel table)
        => table.Indexes.Any(index => index.IsPrimaryKey) ||
           table.Constraints.Any(constraint =>
               constraint.TypeCode == SchemaConstraintTypeCodes.PrimaryKey);

    // islevi: PK ya da tum tabloyu kapsayan filtresiz unique index/constraint bulunup bulunmadigini bildirir.
    private static bool HasUsableUniqueKey(SchemaTableModel table)
        => table.Indexes.Any(index =>
               (index.IsPrimaryKey || index.IsUnique) &&
               index.Columns.Count > 0 &&
               string.IsNullOrWhiteSpace(index.FilterDefinition)) ||
           table.Constraints.Any(constraint =>
               constraint.TypeCode is SchemaConstraintTypeCodes.PrimaryKey or SchemaConstraintTypeCodes.Unique &&
               constraint.Columns.Count > 0);

    // islevi: Generated kolonlari katalog ordinal'i ve adiyla kararli uyari listesine cevirir.
    private static IEnumerable<SchemaLintWarningModel> CreateGeneratedColumnWarnings(
        IEnumerable<SchemaColumnModel> columns)
        => columns.Where(column => column.IsGenerated)
            .OrderBy(column => column.Ordinal)
            .ThenBy(column => column.Name, StringComparer.Ordinal)
            .Select(column => CreateWarning(
                SchemaLintWarningCodes.GeneratedColumn,
                column.Name));

    // islevi: Kararli uyari kodu ve opsiyonel kolon adresinden lint sonucunu kurar.
    private static SchemaLintWarningModel CreateWarning(string warningCode, string? columnName = null)
        => new() { WarningCode = warningCode, ColumnName = columnName };
}
