namespace Ptn.TestModule.Models.Catalog;

// islevi: Senaryo zamanlamasinin Manager girdisini tasir.
// sistemdeki gorevi: Public DTO'yu domain kararindan ayiran tipli zamanlama sozlesmesidir.
public class TestScenarioScheduleModel
{
    public string? ScheduleCron { get; set; }
    public bool ScheduleEnabled { get; set; }
}
