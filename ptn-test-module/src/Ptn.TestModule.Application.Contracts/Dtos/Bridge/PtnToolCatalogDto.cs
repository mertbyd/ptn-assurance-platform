namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Aktif, discoverable ve toolset kodlarini response bicimiyle tasir.
// sistemdeki gorevi: Progressive disclosure istemcisinin ayni anda en fazla yedi tool yuklemesini saglar.
public sealed class PtnToolCatalogDto
{
    public string ResponseFormat { get; set; } = string.Empty;
    public List<string> ActiveToolCodes { get; set; } = [];
    public List<string> DiscoverableToolCodes { get; set; } = [];
    public List<string> ToolsetCodes { get; set; } = [];
    public string? ResourceLink { get; set; }
}
