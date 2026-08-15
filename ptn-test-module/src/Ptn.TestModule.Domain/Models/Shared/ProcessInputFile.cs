namespace Ptn.TestModule.Models.Shared;

// islevi: Surec baslamadan once calisma klasorune yazilacak tek bir dosyanin yolunu ve icerigini tasir.
// sistemdeki gorevi: Hangi belgenin nereye yazilacagi kararini Manager'da tutar; sinir yalniz yazar.
/// <summary>
/// Surec calisma klasorune yazilacak bir girdi dosyasini tasir.
/// </summary>
public class ProcessInputFile
{
    /// <summary>Dosyanin calisma klasorune gore yoludur.</summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>Dosyaya BOM'suz UTF-8 olarak yazilacak icerigidir.</summary>
    public string Content { get; set; } = string.Empty;
}
