using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Kosum belgesinden okunan surum, kriter turu ve adim kimligi olgularini tasir.
// sistemdeki gorevi: Belge bicimi cozumlemesini surec sinirinda birakip kabul, ret ve baglama kararini Manager'a olgu olarak verir.
/// <summary>
/// Arazzo belgesinden cikarilan kosulabilirlik olgularini tasir.
/// </summary>
public class WorkflowDocumentFacts
{
    /// <summary>Belgenin bildirdigi Arazzo surumudur; bulunamazsa bos kalir.</summary>
    public string ArazzoVersion { get; set; } = string.Empty;

    /// <summary>Belgede runner'in desteklemedigi XPath kriteri bulunup bulunmadigidir.</summary>
    public bool HasXPathCriterion { get; set; }

    /// <summary>Belgedeki tum is akisi adimlarinin kararli kimlikleridir.</summary>
    public IReadOnlyList<string> StepKeys { get; set; } = [];
}
