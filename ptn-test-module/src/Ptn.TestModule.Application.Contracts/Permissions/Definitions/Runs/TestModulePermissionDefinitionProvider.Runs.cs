using Ptn.TestModule.Localization;
using Volo.Abp.Authorization.Permissions;

namespace Ptn.TestModule.Permissions;

// islevi: Test kosumu permission agacini ABP permission yonetimine ekler.
// sistemdeki gorevi: Run AppService ve controller policy sabitlerini lokalize ve yonetilebilir izinlere baglar.
/// <summary>Test kosumu permission tanimlarini ana provider'a ekler.</summary>
public partial class TestModulePermissionDefinitionProvider
{
    /// <summary>Test kosumu kokunu ve use-case izinlerini permission agacina baglar.</summary>
    private void AddRunPermissions(PermissionGroupDefinition group)
    {
        var runs = group.AddPermission(
            TestModulePermissions.Runs.Default,
            L(TestModuleLocalizationKeys.Permissions.Runs));
        runs.AddChild(TestModulePermissions.Runs.View, L(TestModuleLocalizationKeys.Permissions.RunsView));
        runs.AddChild(TestModulePermissions.Runs.Trigger, L(TestModuleLocalizationKeys.Permissions.RunsTrigger));
        runs.AddChild(TestModulePermissions.Runs.Start, L(TestModuleLocalizationKeys.Permissions.RunsStart));
        runs.AddChild(TestModulePermissions.Runs.WriteResult, L(TestModuleLocalizationKeys.Permissions.RunsWriteResult));
        runs.AddChild(TestModulePermissions.Runs.Export, L(TestModuleLocalizationKeys.Permissions.RunsExport));
        runs.AddChild(TestModulePermissions.Runs.Cancel, L(TestModuleLocalizationKeys.Permissions.RunsCancel));
        runs.AddChild(TestModulePermissions.Runs.SandboxReset, L(TestModuleLocalizationKeys.Permissions.RunsSandboxReset));
        runs.AddChild(
            TestModulePermissions.Runs.ManageEnvironments,
            L(TestModuleLocalizationKeys.Permissions.RunsManageEnvironments));
    }
}
