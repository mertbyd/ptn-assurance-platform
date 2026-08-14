using System;
using Ptn.TestModule.Models.Runs;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Ptn.TestModule.Entities.Runs;

// islevi: Dugmeye basildigi anda istenen kosumun DBML alanlarini tasir.
// sistemdeki gorevi: Hukum vermeden Pending-Running-terminal durum snapshot'ini tutan tenant-aware veri kabugudur.
/// <summary>
/// Bir test kosum isteginin kalici durum ve ortam snapshot kaydidir.
/// </summary>
public class TestRun : AuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>Bagli senaryo surumu kimligidir; ad-hoc kosumlarda null olabilir.</summary>
    public Guid? ScenarioId { get; internal set; }

    /// <summary>Trendlerde kullanilan kararli test anahtaridir.</summary>
    public string TestKey { get; internal set; } = string.Empty;

    /// <summary>Kanonik girdilerden uretilen SHA-256 trend kovasi kimligidir.</summary>
    public string? HistoryId { get; internal set; }

    /// <summary>Ayardan cozulup kosum aninda snapshot'lanan mantiksal ortam anahtaridir.</summary>
    public string EnvironmentKey { get; internal set; } = string.Empty;

    /// <summary>Kosum aninda cozulmus API Checker snapshot kimligidir.</summary>
    public Guid? SpecSnapshotId { get; internal set; }

    /// <summary>Kosum aninda cozulmus Database Checker baglanti kimligidir.</summary>
    public Guid? DbConnectionId { get; internal set; }

    /// <summary>Kosum motorunun mevcut durum lookup kimligidir.</summary>
    public Guid RunStatusId { get; internal set; }

    /// <summary>Kosumun nasil baslatildigini belirleyen lookup kimligidir.</summary>
    public Guid TriggerKindId { get; internal set; }

    /// <summary>Cron, webhook veya bulgu gibi tetikleyici referansidir.</summary>
    public string? TriggerRef { get; internal set; }

    /// <summary>Operasyonel izle bag kuran 32 karakterlik W3C trace kimligidir.</summary>
    public string? TraceId { get; internal set; }

    /// <summary>Kosum anindaki API sozlesmesi fingerprint snapshot'idir.</summary>
    public string? SpecFingerprint { get; internal set; }

    /// <summary>Kosum anindaki veritabani semasi fingerprint snapshot'idir.</summary>
    public string? DbSchemaFingerprint { get; internal set; }

    /// <summary>Kullanilan dis runner ve surum referansidir.</summary>
    public string? RunnerRef { get; internal set; }

    /// <summary>Kosumun saglik hesaplarina girmeyen dry-run olup olmadigidir.</summary>
    public bool IsDryRun { get; internal set; }

    /// <summary>ABP BLOB Storing icindeki HAR artefakt adidir.</summary>
    public string? HarBlobName { get; internal set; }

    /// <summary>Kosumun fiilen claim edildigi zamandir.</summary>
    public DateTime? StartedAt { get; internal set; }

    /// <summary>Kosum motorunun terminal duruma girdigi zamandir.</summary>
    public DateTime? CompletedAt { get; internal set; }

    /// <summary>ABP veri filtresinin kullandigi tenant kimligidir.</summary>
    public Guid? TenantId { get; internal set; }

    /// <summary>EF Core materializasyonu icin ayrilmis kurucudur.</summary>
    protected TestRun()
    {
    }

    // Manager'in dogruladigi Pending kosum alanlarini davranis calistirmadan atar.
    /// <summary>Dogrulanmis kosum, ortam ve snapshot alanlarini yeni veri kabuguna atar.</summary>
    public TestRun(
        Guid id,
        Guid runStatusId,
        Guid triggerKindId,
        Guid? tenantId,
        TestRunCreateModel model,
        TestRunEnvironmentBinding environmentBinding,
        string historyId,
        string traceId,
        string? specFingerprint,
        string? dbSchemaFingerprint,
        string? runnerRef)
        : base(id)
    {
        ScenarioId = model.ScenarioId;
        TestKey = model.TestKey;
        HistoryId = historyId;
        EnvironmentKey = environmentBinding.EnvironmentKey;
        SpecSnapshotId = environmentBinding.SpecSnapshotId;
        DbConnectionId = environmentBinding.DbConnectionId;
        RunStatusId = runStatusId;
        TriggerKindId = triggerKindId;
        TriggerRef = model.TriggerRef;
        TraceId = traceId;
        SpecFingerprint = specFingerprint;
        DbSchemaFingerprint = dbSchemaFingerprint;
        RunnerRef = runnerRef;
        IsDryRun = model.IsDryRun;
        TenantId = tenantId;
    }
}
