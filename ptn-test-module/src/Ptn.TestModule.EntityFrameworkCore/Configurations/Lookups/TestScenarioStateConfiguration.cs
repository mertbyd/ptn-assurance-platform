using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Lookups;

namespace Ptn.TestModule.EntityFrameworkCore.Configurations.Lookups;

// islevi: TestScenarioState lookup'unu kararli tablo sabitine baglar.
// sistemdeki gorevi: Ortak lookup eslemesini senaryo yayin durumlari tablosuna uygular.
public class TestScenarioStateConfiguration : LookupEntityConfigurationBase<TestScenarioState>
{
    protected override string TableName => TestModuleTableNames.ScenarioStates;
}
