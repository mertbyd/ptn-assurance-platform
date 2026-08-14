using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Aktif ve talep uzerine kesfedilen tool kodlarini toolset kodlariyla tasir.
// sistemdeki gorevi: Tum tool semalarinin ayni anda ajan baglamina yuklenmesini engeller.
public sealed class PtnToolCatalog
{
    public string ResponseFormat { get; set; } = string.Empty;
    public List<string> ActiveToolCodes { get; set; } = [];
    public List<string> DiscoverableToolCodes { get; set; } = [];
    public List<string> ToolsetCodes { get; set; } = [];
    public string? ResourceLink { get; set; }
}
