using System;

namespace Ptn.TestModule.BackgroundJobs.Runs;

// islevi: Pending bir kosumun dayanikli icra job'ina gecen kimlik payload'ini tasir.
// sistemdeki gorevi: HTTP istegi disinda calisacak kosumu tenant ve trace baglamiyla birlikte kuyruga verir.
/// <summary>
/// Test kosumu icra job'inin argumanlarini tasir.
/// </summary>
public class ExecuteTestRunArgs : ITestModuleTenantBackgroundJobArgs
{
    /// <summary>Icra edilecek Pending kosumun kimligidir.</summary>
    public Guid TestRunId { get; set; }

    /// <summary>Job icinde tekrar acilacak tenant kimligidir.</summary>
    public Guid? TenantId { get; set; }

    /// <summary>Kosumu operasyonel izle baglayan W3C trace kimligidir.</summary>
    public string TraceId { get; set; } = string.Empty;
}
