using System;

namespace Ptn.ApiContractChecker.BackgroundJobs;

// islevi: Tenant baglamiyla calisan ABP background job payload'lari icin ortak kontrati tanimlar.
// sistemdeki gorevi: Job handler'larinin ayni TenantId tasima ve CurrentTenant.Change akisina uymasini saglar.
public interface ITenantBackgroundJobArgs
{
    // Job request disinda tekrar acilacak tenant kimligi.
    Guid? TenantId { get; set; }
}
