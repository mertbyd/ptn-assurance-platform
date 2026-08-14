using System;

namespace Ptn.TestModule.BackgroundJobs.Runs;

// islevi: Asili Running kosularini toparlayan supurucu job'in esik payload'ini tasir.
// sistemdeki gorevi: Motor coktugunde Running'de kalan satirlarin kurtarilma penceresini kuyruga verir.
/// <summary>
/// Asili kosum kurtarma job'inin argumanlarini tasir.
/// </summary>
public class RecoverStaleRunsArgs : ITestModuleTenantBackgroundJobArgs
{
    /// <summary>Job icinde tekrar acilacak tenant kimligidir.</summary>
    public Guid? TenantId { get; set; }

    /// <summary>Bu sureden once baslamis Running kosumlar asili sayilir.</summary>
    public int ThresholdMinutes { get; set; }
}
