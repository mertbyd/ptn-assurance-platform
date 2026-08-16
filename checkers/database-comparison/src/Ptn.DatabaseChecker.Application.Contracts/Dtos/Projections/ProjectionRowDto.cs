namespace Ptn.DatabaseChecker.Dtos.Projections;

// islevi: Tek projection satirini JSON nesnesi olarak yazilan redaksiyonlu kolon-deger sozlugunda tasir.
// sistemdeki gorevi: Public Rows listesinin ek wrapper olmadan Dictionary seklini korur.
public sealed class ProjectionRowDto : Dictionary<string, string?>
{
    public ProjectionRowDto()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }
}
