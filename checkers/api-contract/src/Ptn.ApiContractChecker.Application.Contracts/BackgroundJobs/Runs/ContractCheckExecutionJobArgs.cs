using Ptn.ApiContractChecker.Dtos.Runs;

namespace Ptn.ApiContractChecker.BackgroundJobs.Runs;

// islevi: Kuyruga alinmis contract-check calistirmasinin yeniden teslim edilebilir payload'ini tasir.
// sistemdeki gorevi: Run ve tenant kimligiyle gecici kapsam kurallarini tabloya yazmadan ABP job'una tasir.
public class ContractCheckExecutionJobArgs : ITenantBackgroundJobArgs
{
    // Calistirilacak Pending run kimligi.
    public Guid RunId { get; set; }

    // Job baslarken acilacak tenant baglami.
    public Guid? TenantId { get; set; }

    // Yalniz bu teslim zincirinde yasayan kapsam kurallari.
    public List<ContractCheckScopeRuleDto> ScopeRules { get; set; } = new();

    // x-internal true yuzeylerini gecici olarak dislama karari.
    public bool IgnoreInternal { get; set; }
}
