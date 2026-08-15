using System.Collections.Generic;

namespace Ptn.TestModule.Models.Shared;

// islevi: Cozulmus executable, arguman, ortam, workspace ve hata kodu planini tasir.
// sistemdeki gorevi: ProcessStartInfo kurulumunda yorumlanacak karar birakmaz.
public sealed class ProcessStartDescriptor
{
    public string Executable { get; set; } = string.Empty;
    public IReadOnlyList<string> Arguments { get; set; } = [];
    public IReadOnlyDictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();
    public ProcessWorkspaceLayout Workspace { get; set; } = new();
    public int TimeoutMs { get; set; }
    public string StartFailureErrorCode { get; set; } = string.Empty;
    public string TimeoutErrorCode { get; set; } = string.Empty;
}
