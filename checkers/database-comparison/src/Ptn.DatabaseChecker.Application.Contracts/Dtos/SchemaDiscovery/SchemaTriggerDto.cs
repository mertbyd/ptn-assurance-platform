namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: Tek bir trigger'in API cevap modelidir.
// sistemdeki gorevi: Schema snapshot cevabinda trigger adi ve provider'in urettigi ham tanim metnini tasir.
public class SchemaTriggerDto
{
    /// <summary>
    /// Trigger adi.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Motorun trigger icin urettigi ham CREATE/definition metni.
    /// </summary>
    public string Definition { get; set; } = default!;

    /// <summary>
    /// Trigger etkin mi.
    /// </summary>
    public bool IsEnabled { get; set; }
}
