namespace Ptn.ApiContractChecker.Events.Runs;

// islevi: Contract check run'inin yeni yasam dongusu durumunu, tenant adresini ve bulgu ozetini tasir.
// sistemdeki gorevi: Modul disi notification, test veya MCP adapterlerinin checker internallerine baglanmadan durum gecisini ve bulgu agirligini tek olayda okumasini saglar.
public class ContractCheckRunStatusChangedEto
{
    public Guid RunId { get; }
    public Guid? TenantId { get; }
    public string StatusCode { get; }

    // Ayni dokuman cifti icindeki onceki tamamlanmis kosuda gorulmemis bulgu sayisi.
    public int NewFindingCount { get; }

    // Kosudaki en agir DifferenceSeverityCodes degeri; bulgu yoksa null.
    public string? MaxSeverityCode { get; }

    // 0.2.0-alpha.2 cagiranlari icin bulgu ozeti tasimayan imzayi korur.
    public ContractCheckRunStatusChangedEto(Guid runId, Guid? tenantId, string statusCode)
        : this(runId, tenantId, statusCode, 0, null)
    {
    }

    public ContractCheckRunStatusChangedEto(
        Guid runId,
        Guid? tenantId,
        string statusCode,
        int newFindingCount,
        string? maxSeverityCode)
    {
        RunId = runId;
        TenantId = tenantId;
        StatusCode = statusCode;
        NewFindingCount = newFindingCount;
        MaxSeverityCode = maxSeverityCode;
    }
}
