using System;
using Volo.Abp.Application.Dtos;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Kalici TestRun aggregate'inin public okunabilir gorunumunu tasir.
// sistemdeki gorevi: Entity ve internal mutasyon alanlarini API tuketicisinden ayirir.
/// <summary>Bir test kosum kaydinin public gorunumudur.</summary>
public sealed class TestRunDto : AuditedEntityDto<Guid>
{
    /// <summary>Bagli senaryo surumunun kimligidir.</summary>
    public Guid? ScenarioId { get; set; }

    /// <summary>Trendlerde kullanilan kararli test anahtaridir.</summary>
    public string TestKey { get; set; } = string.Empty;

    /// <summary>Kanonik girdilerden uretilen SHA-256 trend kovasi kimligidir.</summary>
    public string? HistoryId { get; set; }

    /// <summary>Kosum aninda snapshot'lanan mantiksal ortam anahtaridir.</summary>
    public string EnvironmentKey { get; set; } = string.Empty;

    /// <summary>API Checker snapshot kimligidir.</summary>
    public Guid? SpecSnapshotId { get; set; }

    /// <summary>Database Checker baglanti kimligidir.</summary>
    public Guid? DbConnectionId { get; set; }

    /// <summary>Kosum motoru durum lookup kimligidir.</summary>
    public Guid RunStatusId { get; set; }

    /// <summary>Tetikleyici turu lookup kimligidir.</summary>
    public Guid TriggerKindId { get; set; }

    /// <summary>Cron, webhook veya bulgu gibi tetikleyici referansidir.</summary>
    public string? TriggerRef { get; set; }

    /// <summary>Operasyonel izle bag kuran W3C trace kimligidir.</summary>
    public string? TraceId { get; set; }

    /// <summary>Kosum anindaki API sozlesmesi fingerprint snapshot'idir.</summary>
    public string? SpecFingerprint { get; set; }

    /// <summary>Kosum anindaki veritabani semasi fingerprint snapshot'idir.</summary>
    public string? DbSchemaFingerprint { get; set; }

    /// <summary>Kullanilan dis runner ve surum referansidir.</summary>
    public string? RunnerRef { get; set; }

    /// <summary>Kosumun saglik hesaplarina girmeyen dry-run olup olmadigidir.</summary>
    public bool IsDryRun { get; set; }

    /// <summary>ABP BLOB Storing icindeki HAR artefakt adidir.</summary>
    public string? HarBlobName { get; set; }

    /// <summary>Kosumun fiilen claim edildigi zamandir.</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>Kosum motorunun terminal duruma girdigi zamandir.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Kaydi sahiplenen tenant kimligidir.</summary>
    public Guid? TenantId { get; set; }
}
