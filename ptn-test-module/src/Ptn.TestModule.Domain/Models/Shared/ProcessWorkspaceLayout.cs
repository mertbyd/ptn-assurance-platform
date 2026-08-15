using System.Collections.Generic;

namespace Ptn.TestModule.Models.Shared;

// islevi: Surec calisma kokunu, olusturulacak klasorleri ve tam dosya yollarini tasir.
// sistemdeki gorevi: Dosya sistemi kararlarini I/O sinirina tipli veri olarak gecirir.
public sealed class ProcessWorkspaceLayout
{
    public string WorkspaceRoot { get; set; } = string.Empty;
    public IReadOnlyList<string> Directories { get; set; } = [];
    public IReadOnlyDictionary<string, string> InputFiles { get; set; } = new Dictionary<string, string>();
    public IReadOnlyDictionary<string, string> OutputFiles { get; set; } = new Dictionary<string, string>();
}
