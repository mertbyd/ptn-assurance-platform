using System;

namespace Ptn.TestModule.Models.Runs;

// islevi: Tenant ayarindan cozulmus mantiksal ortam baglamasini tasir.
// sistemdeki gorevi: Runner girdisi ile test_runs snapshot alanlarini secret degeri tasimadan birlestirir.
/// <summary>
/// Bir test kosumu icin dogrulanmis ortam baglamasini tasir.
/// </summary>
public class TestRunEnvironmentBinding
{
    /// <summary>API ve veritabani hedeflerinin ortak mantiksal ortam anahtaridir.</summary>
    public string EnvironmentKey { get; set; } = string.Empty;

    /// <summary>Hedef sistemin API taban adresidir.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>API Checker spec snapshot kimligidir.</summary>
    public Guid SpecSnapshotId { get; set; }

    /// <summary>Database Checker baglanti kimligidir.</summary>
    public Guid DbConnectionId { get; set; }

    /// <summary>Veritabani sirri yerine tasinan mantiksal secret referansidir.</summary>
    public string SecretRef { get; set; } = string.Empty;

    /// <summary>Korumali API ucu icin tasinan opsiyonel mantiksal secret referansidir; bos ise kosum kimliksiz gider.</summary>
    public string ApiSecretRef { get; set; } = string.Empty;
}
