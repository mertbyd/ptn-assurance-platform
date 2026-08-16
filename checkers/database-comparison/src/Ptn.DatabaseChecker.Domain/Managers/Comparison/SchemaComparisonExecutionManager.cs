using System.Collections.Generic;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Managers.SchemaDiscovery;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Ptn.DatabaseChecker.Models.Comparison.Scope;
using Volo.Abp.Domain.Services;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: Iki baglantinin secili schema, migration ve tablo verisini okuyup karsilastirma motorlarini calistiran orkestrasyon kapisidir.
// sistemdeki gorevi: Modun gerektirdigi I/O'yu secer; schema snapshot, migration ve exact data diff sorumluluklarini kendi saf motorlarinda tutar.
public class SchemaComparisonExecutionManager : DomainService
{
    // Kayitli baglanti uzerinden hedef veritabaninin sema fotografini okuyan kesif servisi.
    private SchemaDiscoveryManager DiscoveryManager
        => LazyServiceProvider.LazyGetRequiredService<SchemaDiscoveryManager>();

    // Iki snapshot'i normalize edip yon bilgili bulgulari ureten saf diff motoru.
    private SchemaComparisonManager ComparisonManager
        => LazyServiceProvider.LazyGetRequiredService<SchemaComparisonManager>();

    // Migration history ve exact tablo verisi icin secret coz + engine repository secimi yapan servis.
    private DatabaseDataComparisonManager DataComparisonManager
        => LazyServiceProvider.LazyGetRequiredService<DatabaseDataComparisonManager>();

    // Migration defteri farklarini yon bilgili finding'e ceviren saf domain servisi.
    private MigrationComparisonManager MigrationComparisonManager
        => LazyServiceProvider.LazyGetRequiredService<MigrationComparisonManager>();

    // Tablo snapshot'larini PK/satir/hucre seviyesinde veri finding'lerine ceviren saf domain servisi.
    private TableDataComparisonManager TableDataComparisonManager
        => LazyServiceProvider.LazyGetRequiredService<TableDataComparisonManager>();

    // Tenant -> global -> default zincirinden veri bulgusu saklama politikasini cozer.
    private ValueRetentionPolicyResolver ValueRetentionPolicyResolver
        => LazyServiceProvider.LazyGetRequiredService<ValueRetentionPolicyResolver>();

    // DataCompare scope kurallarindan exact tablo okuma planini uretir.
    private ComparisonScopeRuleEvaluator ScopeRuleEvaluator
        => LazyServiceProvider.LazyGetRequiredService<ComparisonScopeRuleEvaluator>();

    // Tum bulgu ailelerine ortak siddet ve fingerprint metadata'sini uygular.
    private FindingMetadataEnricher MetadataEnricher
        => LazyServiceProvider.LazyGetRequiredService<FindingMetadataEnricher>();

    // islevi: Kaynak ve hedef baglantinin (opsiyonel sema filtresiyle) fotografini okuyup kapsam kurallariyla kiyaslar.
    // sistemdeki gorevi: comparisonTypeCode moda gore hangi bloklarin (sema+migration / veri) calisacagina karar verir; SchemaOnly veriyi, DataOnly semayi hic okumaz (canliyi bosuna yormaz). Tur anlamini ComparisonTypeCodes tek yerden verir.
    public Task<ComparisonFindings> CompareSchemasAsync(
        DatabaseConnection sourceConnection,
        DatabaseConnection targetConnection,
        List<string> schemaNames,
        List<ComparisonScopeRule> scopeRules,
        string comparisonTypeCode)
        => CompareSchemasAsync(
            sourceConnection,
            targetConnection,
            schemaNames,
            scopeRules,
            comparisonTypeCode,
            ComparisonSideRoleCodes.Reference);

    public async Task<ComparisonFindings> CompareSchemasAsync(
        DatabaseConnection sourceConnection,
        DatabaseConnection targetConnection,
        List<string> schemaNames,
        List<ComparisonScopeRule> scopeRules,
        string comparisonTypeCode,
        string sourceRoleCode)
    {
        var includesSchema = ComparisonTypeCodes.IncludesSchema(comparisonTypeCode);
        var includesData = ComparisonTypeCodes.IncludesData(comparisonTypeCode);

        var findings = new ComparisonFindings();

        if (includesSchema)
        {
            var sourceSnapshot = await DiscoveryManager.ReadSnapshotAsync(sourceConnection, schemaNames);
            var targetSnapshot = await DiscoveryManager.ReadSnapshotAsync(targetConnection, schemaNames);
            findings = ComparisonManager.Compare(sourceSnapshot, targetSnapshot, scopeRules);
            findings.MigrationDifferences.AddRange(await CompareMigrationsAsync(sourceConnection, targetConnection));
        }

        if (includesData)
        {
            findings.DataDifferences.AddRange(await CompareDataAsync(
                sourceConnection,
                targetConnection,
                scopeRules));
        }

        MetadataEnricher.Enrich(
            findings,
            sourceConnection.Engine.Code,
            targetConnection.Engine.Code,
            sourceRoleCode);
        return findings;
    }

    // islevi: Iki baglantinin EF migration defterini okuyup isim bazli farklari bulguya cevirir.
    private async Task<List<MigrationDifferenceModel>> CompareMigrationsAsync(
        DatabaseConnection sourceConnection,
        DatabaseConnection targetConnection)
    {
        var sourceMigrations = await DataComparisonManager.ReadMigrationHistoryAsync(sourceConnection);
        var targetMigrations = await DataComparisonManager.ReadMigrationHistoryAsync(targetConnection);
        return MigrationComparisonManager.Compare(sourceMigrations, targetMigrations);
    }

    // islevi: DataCompare isaretli tablolari her iki taraftan batch okuyup PK/satir/hucre farklarini bulguya cevirir.
    private async Task<List<DataDifferenceModel>> CompareDataAsync(
        DatabaseConnection sourceConnection,
        DatabaseConnection targetConnection,
        List<ComparisonScopeRule> scopeRules)
    {
        var tables = ScopeRuleEvaluator.BuildDataCompareTableIdentifiers(scopeRules);
        if (tables.Count == 0)
        {
            return new List<DataDifferenceModel>();
        }

        var retentionPolicy = await ValueRetentionPolicyResolver.ResolveAsync();
        var sourceData = await DataComparisonManager.ReadTableDataAsync(sourceConnection, tables);
        var targetData = await DataComparisonManager.ReadTableDataAsync(targetConnection, tables);
        return TableDataComparisonManager.Compare(tables, sourceData, targetData, retentionPolicy);
    }
}
