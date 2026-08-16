using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Findings;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: SchemaComparisonAddressModel + yon bilgisi ikilisinden SchemaDifferenceModel uretir.
// sistemdeki gorevi: OnlyInSource/OnlyInTarget/Modified bulgu insasi tek yerde kalir; comparer ve manager ayni owned-JSON finding seklini tekrar kurmaz.
public static class SchemaDifferenceFactory
{
    // islevi: Kaynakta olup hedefte olmayan nesne bulgusunu olusturur.
    public static SchemaDifferenceModel OnlyInSource(
        SchemaComparisonAddressModel address,
        string? sourceDefinition,
        string confidenceCode)
        => Create(address, DifferenceKindCodes.OnlyInSource, confidenceCode, sourceDefinition, null, null);

    // islevi: Hedefte olup kaynakta olmayan nesne bulgusunu olusturur.
    public static SchemaDifferenceModel OnlyInTarget(
        SchemaComparisonAddressModel address,
        string? targetDefinition,
        string confidenceCode)
        => Create(address, DifferenceKindCodes.OnlyInTarget, confidenceCode, null, targetDefinition, null);

    // islevi: Iki tarafta da bulunan ama tanimi farkli olan nesne bulgusunu olusturur.
    public static SchemaDifferenceModel Modified(
        SchemaComparisonAddressModel address,
        string? sourceDefinition,
        string? targetDefinition,
        string? changeSummary,
        string confidenceCode)
        => Create(address, DifferenceKindCodes.Modified, confidenceCode, sourceDefinition, targetDefinition, changeSummary);

    // islevi: Ortak alan atamalarini tek yerde toplar.
    private static SchemaDifferenceModel Create(
        SchemaComparisonAddressModel address,
        string kindCode,
        string confidenceCode,
        string? sourceDefinition,
        string? targetDefinition,
        string? changeSummary)
        => new()
        {
            SchemaName = address.SchemaName,
            ObjectName = address.ObjectName,
            ChildName = address.ChildName,
            ObjectTypeCode = address.ObjectTypeCode,
            KindCode = kindCode,
            ConfidenceCode = confidenceCode,
            SourceDefinition = sourceDefinition,
            TargetDefinition = targetDefinition,
            ChangeSummary = changeSummary
        };
}
