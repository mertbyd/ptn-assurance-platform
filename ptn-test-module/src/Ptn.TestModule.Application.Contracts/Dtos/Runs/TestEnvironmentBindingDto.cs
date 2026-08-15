using System;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Tenant ortam baglamasinin sir icermeyen public gorunumunu tanimlar.
// sistemdeki gorevi: UI dropdown'una API ve checker kimliklerini secret degeri olmadan sunar.
public sealed class TestEnvironmentBindingDto
{
    public string EnvironmentKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public Guid SpecSnapshotId { get; set; }
    public Guid DbConnectionId { get; set; }
}
