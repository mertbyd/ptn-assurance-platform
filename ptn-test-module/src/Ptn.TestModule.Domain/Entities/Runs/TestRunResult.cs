using System;
using System.Collections.Generic;
using Ptn.TestModule.Models.Runs;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Ptn.TestModule.Entities.Runs;

// islevi: Tek kosum denemesinin hukum ve teshis alanlarini kalici olarak tasir.
// sistemdeki gorevi: Degismez terminal satirini ve kendi TestResultFinding cocuklarini tenant-aware aggregate olarak tutar.
/// <summary>
/// Bir test kosumu denemesinin kalici terminal sonucudur.
/// </summary>
public class TestRunResult : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>Aggregate icindeki degistirilebilir EF Core bulgu koleksiyonudur.</summary>
    private List<TestResultFinding> _findings = [];

    /// <summary>Sonucun ait oldugu TestRun aggregate kimligidir.</summary>
    public Guid TestRunId { get; internal set; }

    /// <summary>Ayni kosum icindeki bir tabanli deneme numarasidir.</summary>
    public int Attempt { get; internal set; }

    /// <summary>Passed, Failed, Broken, Skipped veya Inconclusive lookup kimligidir.</summary>
    public Guid OutcomeStatusId { get; internal set; }

    /// <summary>Bu denemenin milisaniye cinsinden suresidir.</summary>
    public int DurationMs { get; internal set; }

    /// <summary>Hangi hakemin hayir dedigini belirleyen opsiyonel lookup kimligidir.</summary>
    public Guid? FailureCategoryId { get; internal set; }

    /// <summary>Kararli ve makine-okur hata kodudur.</summary>
    public string? ErrorCode { get; internal set; }

    /// <summary>Bu olusuma ozel RFC 9457 detail metnidir.</summary>
    public string? Detail { get; internal set; }

    /// <summary>Basarisiz adimin bir tabanli sira numarasidir.</summary>
    public int? FailedStepOrdinal { get; internal set; }

    /// <summary>Basarisiz adimin gorunen adidir.</summary>
    public string? FailedStepName { get; internal set; }

    /// <summary>Basarisiz adimin makine-okur yoludur.</summary>
    public string? FailedStepPath { get; internal set; }

    /// <summary>Kosumun terminale giderken izledigi dal yoludur.</summary>
    public string? TakenBranchPath { get; internal set; }

    /// <summary>Basariyla tamamlanan son adimin bir tabanli sira numarasidir.</summary>
    public int? LastCompletedOrdinal { get; internal set; }

    /// <summary>En fazla 4 KB olan yapilandirilmis diagnosis JSON metnidir.</summary>
    public string? DiagnosisReport { get; internal set; }

    /// <summary>Terminal yazimla birlikte kalicilastirilan sirali bulgulardir.</summary>
    public IReadOnlyCollection<TestResultFinding> Findings => _findings;

    /// <summary>ABP veri filtresinin kullandigi tenant kimligidir.</summary>
    public Guid? TenantId { get; internal set; }

    /// <summary>EF Core materializasyonu icin ayrilmis kurucudur.</summary>
    protected TestRunResult()
    {
    }

    // Manager'in dogruladigi terminal alanlarini ve cocuklari davranis calistirmadan atar.
    /// <summary>Dogrulanmis hukum, teshis ve bulgu alanlarini yeni aggregate'e atar.</summary>
    public TestRunResult(
        Guid id,
        Guid testRunId,
        int attempt,
        Guid outcomeStatusId,
        Guid? failureCategoryId,
        int durationMs,
        Guid? tenantId,
        TestRunTerminalModel model,
        IReadOnlyCollection<TestResultFinding> findings)
        : base(id)
    {
        TestRunId = testRunId;
        Attempt = attempt;
        OutcomeStatusId = outcomeStatusId;
        DurationMs = durationMs;
        FailureCategoryId = failureCategoryId;
        ErrorCode = model.ErrorCode;
        Detail = model.Detail;
        FailedStepOrdinal = model.FailedStepOrdinal;
        FailedStepName = model.FailedStepName;
        FailedStepPath = model.FailedStepPath;
        TakenBranchPath = model.TakenBranchPath;
        LastCompletedOrdinal = model.LastCompletedOrdinal;
        DiagnosisReport = model.DiagnosisReport;
        _findings = new List<TestResultFinding>(findings);
        TenantId = tenantId;
    }
}
