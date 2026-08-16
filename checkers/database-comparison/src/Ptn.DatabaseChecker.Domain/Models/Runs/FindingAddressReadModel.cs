namespace Ptn.DatabaseChecker.Models.Runs;

// islevi: Owned JSON bulgusunun motor cifti ve hedef adresini repository projeksiyonunda tasir.
// sistemdeki gorevi: EF projection istisnasini Mapperly'nin public FindingAddressDto eslemesine temiz kaynak yapar.
public sealed class FindingAddressReadModel
{
    public string SourceEngineCode { get; set; } = default!;
    public string TargetEngineCode { get; set; } = default!;
    public string? SchemaName { get; set; }
    public string ObjectTypeCode { get; set; } = default!;
    public string ObjectName { get; set; } = default!;
    public string? ChildName { get; set; }
}
