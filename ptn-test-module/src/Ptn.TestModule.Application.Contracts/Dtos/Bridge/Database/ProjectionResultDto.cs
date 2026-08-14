namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Redaksiyonlu projeksiyon satirlarini ve kanit durumunu tasir.
// sistemdeki gorevi: Unavailable sonucunu yanlis yokluk hukumunden ayirir.
public sealed class ProjectionResultDto
{
    public string StateCode { get; set; } = string.Empty;
    public List<Dictionary<string, string?>> Rows { get; set; } = [];
    public long ObservedRowCount { get; set; }
    public bool Truncated { get; set; }
}
