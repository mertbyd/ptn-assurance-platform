using System.Collections.Generic;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Bir adim dalinin adini ve tasidigi kriterleri wire seklinde tasir.
// sistemdeki gorevi: Kriterlerin yalniz successCriteria altinda arandigi kor noktayi kapatir.
public class ArazzoActionDto
{
    /// <summary>Aksiyonun belge icindeki adidir.</summary>
    public string? Name { get; set; }

    /// <summary>Aksiyonun tetiklenme kriterleridir.</summary>
    public List<ArazzoCriterionDto> Criteria { get; set; } = [];
}
