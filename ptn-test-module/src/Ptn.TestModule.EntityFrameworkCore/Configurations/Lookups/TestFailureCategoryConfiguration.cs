using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Lookups;

namespace Ptn.TestModule.EntityFrameworkCore.Configurations.Lookups;

// islevi: TestFailureCategory lookup'unu kararli tablo sabitine baglar.
// sistemdeki gorevi: Ortak lookup eslemesini bulgu kategorileri tablosuna uygular.
public class TestFailureCategoryConfiguration : LookupEntityConfigurationBase<TestFailureCategory>
{
    protected override string TableName => TestModuleTableNames.FailureCategories;
}
