using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Lookups;

namespace Ptn.TestModule.EntityFrameworkCore.Configurations.Lookups;

// islevi: TestTriggerKind lookup'unu kararli tablo sabitine baglar.
// sistemdeki gorevi: Ortak lookup eslemesini tetikleme turleri tablosuna uygular.
public class TestTriggerKindConfiguration : LookupEntityConfigurationBase<TestTriggerKind>
{
    protected override string TableName => TestModuleTableNames.TriggerKinds;
}
