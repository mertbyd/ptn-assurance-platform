using Ptn.TestModule.Localization;
using Volo.Abp.Authorization.Permissions;

namespace Ptn.TestModule.Permissions;

// islevi: Senaryo katalogu permission agacini ABP permission yonetimine ekler.
// sistemdeki gorevi: Katalog AppService policy sabitlerini lokalize ve yonetilebilir izinlere baglar.
public partial class TestModulePermissionDefinitionProvider
{
    // Senaryo kokunu ve use-case izinlerini permission agacina baglar.
    private void AddScenarioPermissions(PermissionGroupDefinition group)
    {
        var scenarios = group.AddPermission(
            TestModulePermissions.Scenarios.Default,
            L(TestModuleLocalizationKeys.Permissions.Scenarios));
        scenarios.AddChild(TestModulePermissions.Scenarios.Create, L(TestModuleLocalizationKeys.Permissions.ScenariosCreate));
        scenarios.AddChild(TestModulePermissions.Scenarios.Update, L(TestModuleLocalizationKeys.Permissions.ScenariosUpdate));
        scenarios.AddChild(TestModulePermissions.Scenarios.Delete, L(TestModuleLocalizationKeys.Permissions.ScenariosDelete));
        scenarios.AddChild(TestModulePermissions.Scenarios.Publish, L(TestModuleLocalizationKeys.Permissions.ScenariosPublish));
        scenarios.AddChild(TestModulePermissions.Scenarios.Approve, L(TestModuleLocalizationKeys.Permissions.ScenariosApprove));
        scenarios.AddChild(TestModulePermissions.Scenarios.Quarantine, L(TestModuleLocalizationKeys.Permissions.ScenariosQuarantine));
        scenarios.AddChild(TestModulePermissions.Scenarios.Schedule, L(TestModuleLocalizationKeys.Permissions.ScenariosSchedule));
    }
}
