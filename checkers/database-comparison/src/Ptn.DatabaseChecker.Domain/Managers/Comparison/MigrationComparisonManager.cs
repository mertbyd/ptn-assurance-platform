using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: Iki migration history defterini MigrationId bazinda karsilastirip yon bilgili finding uretir.
// sistemdeki gorevi: T7 migration farki karari provider okumasindan ayrilir; repository yalniz okur, bu servis OnlyInSource/OnlyInTarget/Modified anlamini verir.
public class MigrationComparisonManager : ITransientDependency
{
    // islevi: Kaynak/hedef migration listelerinden sirali MigrationDifferenceModel listesi uretir.
    public List<MigrationDifferenceModel> Compare(
        List<MigrationHistoryEntryModel> sourceMigrations,
        List<MigrationHistoryEntryModel> targetMigrations)
    {
        var sourceById = BuildMigrationById(sourceMigrations);
        var targetById = BuildMigrationById(targetMigrations);
        var differences = new List<MigrationDifferenceModel>();

        AddOnlyInSource(sourceById, targetById, differences);
        AddOnlyInTarget(sourceById, targetById, differences);
        AddVersionDifferences(sourceById, targetById, differences);

        return differences
            .OrderBy(difference => difference.MigrationId, StringComparer.Ordinal)
            .ThenBy(difference => difference.KindCode, StringComparer.Ordinal)
            .ToList();
    }

    // islevi: Migration listesinde duplicate gelirse ilk kaydi koruyarak kararli sozluk kurar.
    private static Dictionary<string, MigrationHistoryEntryModel> BuildMigrationById(
        List<MigrationHistoryEntryModel> migrations)
    {
        return migrations
            .GroupBy(migration => migration.MigrationId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
    }

    // islevi: Kaynakta olup hedefte olmayan migration'lari bulguya cevirir.
    private static void AddOnlyInSource(
        Dictionary<string, MigrationHistoryEntryModel> sourceById,
        Dictionary<string, MigrationHistoryEntryModel> targetById,
        List<MigrationDifferenceModel> differences)
    {
        foreach (var (migrationId, sourceMigration) in sourceById)
        {
            if (!targetById.ContainsKey(migrationId))
            {
                differences.Add(new MigrationDifferenceModel
                {
                    SourceSchemaName = sourceMigration.SchemaName,
                    MigrationId = migrationId,
                    KindCode = DifferenceKindCodes.OnlyInSource,
                    SourceProductVersion = sourceMigration.ProductVersion
                });
            }
        }
    }

    // islevi: Hedefte olup kaynakta olmayan migration'lari bulguya cevirir.
    private static void AddOnlyInTarget(
        Dictionary<string, MigrationHistoryEntryModel> sourceById,
        Dictionary<string, MigrationHistoryEntryModel> targetById,
        List<MigrationDifferenceModel> differences)
    {
        foreach (var (migrationId, targetMigration) in targetById)
        {
            if (!sourceById.ContainsKey(migrationId))
            {
                differences.Add(new MigrationDifferenceModel
                {
                    TargetSchemaName = targetMigration.SchemaName,
                    MigrationId = migrationId,
                    KindCode = DifferenceKindCodes.OnlyInTarget,
                    TargetProductVersion = targetMigration.ProductVersion
                });
            }
        }
    }

    // islevi: Ayni migration iki tarafta da var ama EF ProductVersion farkliysa Modified bulgusu uretir.
    private static void AddVersionDifferences(
        Dictionary<string, MigrationHistoryEntryModel> sourceById,
        Dictionary<string, MigrationHistoryEntryModel> targetById,
        List<MigrationDifferenceModel> differences)
    {
        foreach (var (migrationId, sourceMigration) in sourceById)
        {
            if (!targetById.TryGetValue(migrationId, out var targetMigration) ||
                string.Equals(sourceMigration.ProductVersion, targetMigration.ProductVersion, StringComparison.Ordinal))
            {
                continue;
            }

            differences.Add(new MigrationDifferenceModel
            {
                SourceSchemaName = sourceMigration.SchemaName,
                TargetSchemaName = targetMigration.SchemaName,
                MigrationId = migrationId,
                KindCode = DifferenceKindCodes.Modified,
                SourceProductVersion = sourceMigration.ProductVersion,
                TargetProductVersion = targetMigration.ProductVersion
            });
        }
    }
}
