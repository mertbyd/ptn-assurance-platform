using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.Models.Catalog;
using Riok.Mapperly.Abstractions;

namespace Ptn.TestModule.Mappers.Catalog;

// islevi: Kapsam raporu domain modelinin public gorunume eslemesini tanimlar.
// sistemdeki gorevi: Kapsam dikeyindeki katmanlar-arasi donusumun tek Mapperly sahibidir.
/// <summary>Senaryo kapsam raporunun saf Mapperly eslemelerini tasir.</summary>
[Mapper]
public partial class ScenarioCoverageMapper
{
    /// <summary>Kapsam raporu domain modelini public rapor DTO'suna cevirir.</summary>
    public partial ScenarioCoverageReportDto Map(ScenarioCoverageReport source);
}
