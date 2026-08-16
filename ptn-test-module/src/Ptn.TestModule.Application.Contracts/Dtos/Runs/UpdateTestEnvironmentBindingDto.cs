using System;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Bagli bir tenant ortaminin degistirilebilir hedeflerini tanimlar.
// sistemdeki gorevi: Mantiksal anahtari rotada birakarak yalniz hedeflerin guncellenmesini saglar.
/// <summary>Bagli test ortaminin degistirilebilir hedeflerini tasir.</summary>
public sealed class UpdateTestEnvironmentBindingDto
{
    /// <summary>Hedef sistemin mutlak API taban adresidir.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>API Checker spec snapshot kimligidir.</summary>
    public Guid SpecSnapshotId { get; set; }

    /// <summary>Database Checker baglanti kimligidir.</summary>
    public Guid DbConnectionId { get; set; }

    /// <summary>Veritabani sirri yerine tasinan mantiksal secret referansidir.</summary>
    public string SecretRef { get; set; } = string.Empty;
}
