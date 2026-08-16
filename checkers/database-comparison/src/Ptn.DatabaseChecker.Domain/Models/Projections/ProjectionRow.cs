namespace Ptn.DatabaseChecker.Models.Projections;

// islevi: Tek projection satirinin redaksiyon uygulanmis kolon-deger sozlugunu tasir.
// sistemdeki gorevi: JSON'da dogrudan nesne olarak yazilan provider-notr ve secret-safe satir seklidir.
public sealed class ProjectionRow : Dictionary<string, string?>
{
    public ProjectionRow()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    // islevi: Redaksiyon uygulanmis kolon sozlugunu case-insensitive projection satirina kopyalar.
    public ProjectionRow(IDictionary<string, string?> values)
        : base(values, StringComparer.OrdinalIgnoreCase)
    {
    }
}
