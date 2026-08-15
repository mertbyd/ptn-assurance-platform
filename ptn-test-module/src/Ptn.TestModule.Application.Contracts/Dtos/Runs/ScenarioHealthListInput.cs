using Ptn.TestModule.Constants.Runs;
using Volo.Abp.Application.Dtos;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Senaryo saglik listesinin sinirli filtre ve siralama girdilerini tanimlar.
// sistemdeki gorevi: Trend taramasinin tipli ve kapali query sozlesmesidir.
public sealed class ScenarioHealthListInput : PagedResultRequestDto
{
    public string? ScenarioKey { get; set; }
    public double? MinFlakyRatio { get; set; }
    public double? MaxPassRatio { get; set; }
    public string? Sorting { get; set; }

    public ScenarioHealthListInput()
    {
        MaxResultCount = ScenarioHealthConsts.DefaultPageSize;
    }
}
