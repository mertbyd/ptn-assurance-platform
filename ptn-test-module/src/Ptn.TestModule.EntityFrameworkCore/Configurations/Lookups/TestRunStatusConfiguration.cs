using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Lookups;

namespace Ptn.TestModule.EntityFrameworkCore.Configurations.Lookups;

// islevi: TestRunStatus lookup'unu kararli tablo sabitine baglar.
// sistemdeki gorevi: Ortak lookup eslemesini kosum durumlari tablosuna uygular.
public class TestRunStatusConfiguration : LookupEntityConfigurationBase<TestRunStatus>
{
    protected override string TableName => TestModuleTableNames.RunStatuses;
}
