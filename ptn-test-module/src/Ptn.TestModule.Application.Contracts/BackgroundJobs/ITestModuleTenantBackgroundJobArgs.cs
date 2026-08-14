using System;

namespace Ptn.TestModule.BackgroundJobs;

// islevi: Tenant baglamiyla calisan ABP background job payload'lari icin ortak kontrati tanimlar.
// sistemdeki gorevi: Job handler'larinin ayni TenantId tasima ve CurrentTenant.Change akisina uymasini saglar.
/// <summary>
/// Tenant-aware Test Module background job argumanlarinin ortak sozlesmesidir.
/// </summary>
public interface ITestModuleTenantBackgroundJobArgs
{
    /// <summary>Job request disinda tekrar acilacak tenant kimligidir.</summary>
    Guid? TenantId { get; set; }
}
