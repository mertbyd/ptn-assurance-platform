using System.Collections.Generic;
using Ptn.TestModule.Dtos.Lookups;
using Ptn.TestModule.Entities.Lookups;
using Riok.Mapperly.Abstractions;

namespace Ptn.TestModule.Mappers.Lookups;

// islevi: Bes test lookup entity'sinin public okuma DTO eslemelerini tanimlar.
// sistemdeki gorevi: Lookup dikeyindeki tum katmanlar-arasi donusumlerin tek Mapperly sahibidir.
/// <summary>Test lookup dikeyinin saf Mapperly eslemelerini tasir.</summary>
[Mapper]
public partial class TestLookupMapper
{
    /// <summary>Kosum durumu satirini public lookup DTO'suna cevirir.</summary>
    public partial TestRunStatusDto Map(TestRunStatus source);

    /// <summary>Kosum durumu sayfasini tek collection eslemesiyle DTO listesine cevirir.</summary>
    public partial List<TestRunStatusDto> Map(List<TestRunStatus> source);

    /// <summary>Test hukmu satirini build politikasiyla birlikte public lookup DTO'suna cevirir.</summary>
    public partial TestOutcomeStatusDto Map(TestOutcomeStatus source);

    /// <summary>Test hukmu sayfasini tek collection eslemesiyle DTO listesine cevirir.</summary>
    public partial List<TestOutcomeStatusDto> Map(List<TestOutcomeStatus> source);

    /// <summary>Bulgu kategorisi satirini public lookup DTO'suna cevirir.</summary>
    public partial TestFailureCategoryDto Map(TestFailureCategory source);

    /// <summary>Bulgu kategorisi sayfasini tek collection eslemesiyle DTO listesine cevirir.</summary>
    public partial List<TestFailureCategoryDto> Map(List<TestFailureCategory> source);

    /// <summary>Tetikleme turu satirini public lookup DTO'suna cevirir.</summary>
    public partial TestTriggerKindDto Map(TestTriggerKind source);

    /// <summary>Tetikleme turu sayfasini tek collection eslemesiyle DTO listesine cevirir.</summary>
    public partial List<TestTriggerKindDto> Map(List<TestTriggerKind> source);

    /// <summary>Senaryo yayin durumu satirini public lookup DTO'suna cevirir.</summary>
    public partial TestScenarioStateDto Map(TestScenarioState source);

    /// <summary>Senaryo yayin durumu sayfasini tek collection eslemesiyle DTO listesine cevirir.</summary>
    public partial List<TestScenarioStateDto> Map(List<TestScenarioState> source);
}
