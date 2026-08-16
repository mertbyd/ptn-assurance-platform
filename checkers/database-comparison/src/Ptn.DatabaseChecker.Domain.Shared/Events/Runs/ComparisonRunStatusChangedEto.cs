namespace Ptn.DatabaseChecker.Events.Runs;

// islevi: Database comparison run'inin yeni yasam dongusu durumunu, tenant adresini ve bulgu ozetini tasir.
// sistemdeki gorevi: Notifications, Test Module veya MCP adapterinin checker internallerine baglanmadan durum gecisini ve bulgu agirligini tek olayda okumasini saglar.
public class ComparisonRunStatusChangedEto
{
    public Guid RunId { get; }
    public Guid? TenantId { get; }
    public string StatusCode { get; }

    // Ayni definition icindeki onceki tamamlanmis kosuda gorulmemis bulgu sayisi.
    public int NewFindingCount { get; }

    // Kosudaki en agir DifferenceSeverityCodes degeri; bulgu yoksa null.
    public string? MaxSeverityCode { get; }

    // 0.2.0-alpha.2 cagiranlari icin bulgu ozeti tasimayan imzayi korur.
    public ComparisonRunStatusChangedEto(Guid runId, Guid? tenantId, string statusCode)
        : this(runId, tenantId, statusCode, 0, null)
    {
    }

    public ComparisonRunStatusChangedEto(
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
