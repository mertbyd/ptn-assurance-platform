namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Aktif, discoverable ve toolset kodlarini response bicimiyle tasir.
// sistemdeki gorevi: Progressive disclosure istemcisinin ayni anda en fazla yedi tool yuklemesini saglar.
public sealed class ToolCatalogDto
{
    /// <summary>
    /// Cevabin concise veya ayrintili sunum bicimini belirtir.
    /// </summary>
    public string ResponseFormat { get; set; } = string.Empty;
    /// <summary>
    /// Kontrollu sozlukteki ilgili kodlari kararli sirada listeler.
    /// </summary>
    public List<string> ActiveToolCodes { get; set; } = [];
    /// <summary>
    /// Kontrollu sozlukteki ilgili kodlari kararli sirada listeler.
    /// </summary>
    public List<string> DiscoverableToolCodes { get; set; } = [];
    /// <summary>
    /// Kontrollu sozlukteki ilgili kodlari kararli sirada listeler.
    /// </summary>
    public List<string> ToolsetCodes { get; set; } = [];
    /// <summary>
    /// Ayrintili kaynaga erisim adresini belirtir.
    /// </summary>
    public string? ResourceLink { get; set; }
}
