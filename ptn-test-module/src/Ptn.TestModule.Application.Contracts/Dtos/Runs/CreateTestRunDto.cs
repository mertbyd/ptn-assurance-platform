using System;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Yeni Pending kosum icin kullanicidan alinabilen alanlari tasir.
// sistemdeki gorevi: Ortam cozumleme ve kimlik uretim kararlarini Application ile Manager'a birakir.
/// <summary>Yeni bir test kosumu olusturma girdisidir.</summary>
public sealed class CreateTestRunDto
{
    /// <summary>Bagli senaryo surumunun kimligidir; ad-hoc kosumlarda null olabilir.</summary>
    public Guid? ScenarioId { get; set; }

    /// <summary>Trendlerde kullanilan kararli test anahtaridir.</summary>
    public string TestKey { get; set; } = string.Empty;

    /// <summary>Tenant ayarindan cozulmesi istenen mantiksal ortam anahtaridir.</summary>
    public string EnvironmentKey { get; set; } = string.Empty;

    /// <summary>Kosumun nasil tetiklendigini belirleyen lookup kodudur.</summary>
    public string TriggerKindCode { get; set; } = string.Empty;

    /// <summary>Cron, webhook veya bulgu gibi tetikleyici referansidir.</summary>
    public string? TriggerRef { get; set; }

    /// <summary>History kimligine katilacak onceden kanoniklestirilmis girdilerdir.</summary>
    public string CanonicalInputs { get; set; } = string.Empty;

    /// <summary>Kosum anindaki API sozlesmesi fingerprint'idir.</summary>
    public string? SpecFingerprint { get; set; }

    /// <summary>Kosum anindaki veritabani semasi fingerprint'idir.</summary>
    public string? DbSchemaFingerprint { get; set; }

    /// <summary>Kullanilacak dis runner ve surum referansidir.</summary>
    public string? RunnerRef { get; set; }

    /// <summary>Kosumun saglik hesaplarina girmeyen dry-run olup olmadigidir.</summary>
    public bool IsDryRun { get; set; }
}
