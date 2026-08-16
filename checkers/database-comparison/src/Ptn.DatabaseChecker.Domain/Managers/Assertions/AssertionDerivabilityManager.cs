using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Managers.SchemaDiscovery;
using Ptn.DatabaseChecker.Models.Assertions;
using Ptn.DatabaseChecker.Models.Comparison;
using Volo.Abp.Domain.Services;

namespace Ptn.DatabaseChecker.Managers.Assertions;

// islevi: DB assertion adreslerini tablo, kolon, unique anahtar ve matcher-tip kapilarindan sirayla gecirir.
// sistemdeki gorevi: RULE-0006 yayim kararini canli katalogdan tureten fail-closed domain sahibidir.
public class AssertionDerivabilityManager : DomainService
{
    private readonly SchemaDiscoveryManager _schemaDiscoveryManager;
    private readonly ColumnTypeConfidenceResolver _typeResolver;

    // islevi: Turetilebilirlik kapisini mevcut schema discovery ve kanonik tip resolver'i ile kurar.
    public AssertionDerivabilityManager(
        SchemaDiscoveryManager schemaDiscoveryManager,
        ColumnTypeConfidenceResolver typeResolver)
    {
        _schemaDiscoveryManager = schemaDiscoveryManager;
        _typeResolver = typeResolver;
    }

    // islevi: Tek batch katalog okumasindan sonra her assertion icin tam bir outcome uretir.
    public virtual async Task<DerivabilityResult> ValidateAsync(
        DatabaseConnection connection,
        DerivabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var targets = CreateTargets(request.Assertions);
        var descriptions = await _schemaDiscoveryManager.DescribeTablesAsync(
            connection, targets, cancellationToken);
        var byAddress = BuildDescriptionMap(descriptions);
        return new DerivabilityResult
        {
            Assertions = request.Assertions.Select(item => Evaluate(item, byAddress)).ToList()
        };
    }

    // islevi: Assertion adreslerini tek katalog okumasina girecek tekrarsiz tablo hedeflerine indirger.
    private static List<ComparisonTableIdentifierModel> CreateTargets(IEnumerable<DerivabilityAddress> assertions)
        => assertions.Select(item => new ComparisonTableIdentifierModel
            {
                SchemaName = item.SchemaName,
                TableName = item.TableName
            })
            .DistinctBy(item => FindingAddressGrammar.FormatTargetAddress(item.SchemaName, item.TableName, null))
            .ToList();

    // islevi: Dar tablo tanimlarini case-insensitive kanonik adresle eslenen lookup'a cevirir.
    private static IReadOnlyDictionary<string, TableDescriptionModel> BuildDescriptionMap(
        IEnumerable<TableDescriptionModel> descriptions)
        => descriptions.ToDictionary(
            item => FindingAddressGrammar.FormatTargetAddress(item.SchemaName, item.TableName, null),
            StringComparer.OrdinalIgnoreCase);

    // islevi: Tek assertion icin ilk basarisiz katalog/anahtar/tip kapisini secip kismi gecisi engeller.
    private DerivabilityItem Evaluate(
        DerivabilityAddress address,
        IReadOnlyDictionary<string, TableDescriptionModel> descriptions)
    {
        var tableRef = FindingAddressGrammar.FormatTargetAddress(
            address.SchemaName, address.TableName, null);
        if (!descriptions.TryGetValue(tableRef, out var description))
        {
            return CreateItem(tableRef, FirstExpectedColumn(address), AssertionDerivabilityCodes.TableNotFound);
        }

        var missingColumn = FindMissingExpectedColumn(description, address.ExpectedColumns);
        if (missingColumn is not null)
        {
            return CreateItem(tableRef, missingColumn, AssertionDerivabilityCodes.ColumnNotFound);
        }

        if (!IsUniqueKey(description, address.KeyColumns))
        {
            return CreateItem(tableRef, FirstExpectedColumn(address), AssertionDerivabilityCodes.KeyNotUnique);
        }

        var incompatibleColumn = FindIncompatibleColumn(description, address);
        return CreateItem(
            tableRef,
            incompatibleColumn ?? FirstExpectedColumn(address),
            incompatibleColumn is null
                ? AssertionDerivabilityCodes.Derivable
                : AssertionDerivabilityCodes.MatcherTypeMismatch);
    }

    // islevi: Beklenen kolonlardan katalogda bulunmayan ilkini girdi sirasiyla dondurur.
    private static string? FindMissingExpectedColumn(
        TableDescriptionModel description,
        IEnumerable<string> expectedColumns)
    {
        var columns = description.Columns.Select(column => column.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return expectedColumns.FirstOrDefault(column => !columns.Contains(column));
    }

    // islevi: Istek anahtar kolonlarinin PK veya unique index kolon kumesiyle tam eslestigini bildirir.
    private static bool IsUniqueKey(TableDescriptionModel description, IEnumerable<string> keyColumns)
    {
        var requested = keyColumns.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var keys = description.UniqueIndexes.AsEnumerable();
        if (description.PrimaryKey is not null)
        {
            keys = keys.Prepend(description.PrimaryKey);
        }

        return keys.Any(key => key.Columns.Count == requested.Count && key.Columns.All(requested.Contains));
    }

    // islevi: Matcher'in kanonik kolon tipiyle uyusmadigi ilk beklenen kolonu dondurur.
    private string? FindIncompatibleColumn(TableDescriptionModel description, DerivabilityAddress address)
        => address.ExpectedColumns.FirstOrDefault(columnName =>
        {
            var column = description.Columns.First(item =>
                string.Equals(item.Name, columnName, StringComparison.OrdinalIgnoreCase));
            return !_typeResolver.IsMatcherCompatible(address.MatcherCode, column.CanonicalDataTypeCode);
        });

    // islevi: Sonuc adresinde kullanilacak ilk beklenen kolonu kararli olarak secer.
    private static string FirstExpectedColumn(DerivabilityAddress address)
        => address.ExpectedColumns.FirstOrDefault() ?? string.Empty;

    // islevi: Tek tablo/kolon referansi ve outcome kodundan public-sekillendirilebilir item kurar.
    private static DerivabilityItem CreateItem(string tableRef, string columnRef, string outcomeCode)
        => new() { TableRef = tableRef, ColumnRef = columnRef, OutcomeCode = outcomeCode };
}
