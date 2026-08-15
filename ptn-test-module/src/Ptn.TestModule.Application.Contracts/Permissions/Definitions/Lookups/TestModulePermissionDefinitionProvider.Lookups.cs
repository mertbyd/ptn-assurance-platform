using Ptn.TestModule.Localization;
using Volo.Abp.Authorization.Permissions;

namespace Ptn.TestModule.Permissions;

// islevi: Test lookup okuma permission'ini ABP permission yonetimine ekler.
// sistemdeki gorevi: Bes lookup controller'inin policy sabitini lokalize ve yonetilebilir izne baglar.
/// <summary>Test lookup permission tanimini ana provider'a ekler.</summary>
public partial class TestModulePermissionDefinitionProvider
{
    /// <summary>Test lookup okuma iznini permission agacina baglar.</summary>
    private void AddLookupPermissions(PermissionGroupDefinition group)
    {
        group.AddPermission(
            TestModulePermissions.Lookups.Default,
            L(TestModuleLocalizationKeys.Permissions.Lookups));
    }
}
