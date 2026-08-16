namespace Ptn.DatabaseChecker.BackgroundJobs;

// islevi: Tenant baglamiyla calisan ABP background job payload'lari icin ortak kontrati tanimlar.
// sistemdeki gorevi: Checker job'larinin ayni TenantId tasima ve CurrentTenant.Change akisina uymasini saglar.
public interface ITenantBackgroundJobArgs
{
    Guid? TenantId { get; set; }
}
