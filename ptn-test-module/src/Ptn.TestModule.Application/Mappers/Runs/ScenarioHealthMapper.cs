using System.Collections.Generic;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Models.Runs;
using Riok.Mapperly.Abstractions;

namespace Ptn.TestModule.Mappers.Runs;

// islevi: Senaryo saglik query ve gorunum eslemelerini tanimlar.
// sistemdeki gorevi: Saglik dikeyindeki katmanlar-arasi donusumlerin tek Mapperly sahibidir.
/// <summary>Senaryo saglik dikeyinin saf Mapperly eslemelerini tasir.</summary>
[Mapper]
public partial class ScenarioHealthMapper
{
    /// <summary>Liste girdisini repository sorgu modeline cevirir.</summary>
    public partial ScenarioHealthQuery Map(ScenarioHealthListInput source);

    /// <summary>View satirini public saglik DTO'suna cevirir.</summary>
    public partial ScenarioHealthDto Map(ScenarioHealth source);

    /// <summary>View satir sayfasini tek collection eslemesiyle public DTO listesine cevirir.</summary>
    public partial List<ScenarioHealthDto> Map(List<ScenarioHealth> source);
}
