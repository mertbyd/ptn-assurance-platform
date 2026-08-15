using System;

namespace Ptn.TestModule.BackgroundJobs.Runs;

// islevi: Setting-driven retention purge job'inin tenant baglamini tasir.
// sistemdeki gorevi: ABP background worker teslimini dogru tenant veri filtresiyle calistirir.
/// <summary>Retention purge job'inin tenant-aware argumanlarini tasir.</summary>
public class PurgeExpiredRunsArgs : ITestModuleTenantBackgroundJobArgs
{
    /// <summary>Job icinde tekrar acilacak tenant kimligidir.</summary>
    public Guid? TenantId { get; set; }
}
