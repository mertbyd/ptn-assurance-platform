using Ptn.TestModule.Localization;
using Volo.Abp.Authorization.Permissions;

namespace Ptn.TestModule.Permissions;

// islevi: Bridge ajan yuzeyinin operation permission'larini ABP permission agacina ekler.
// sistemdeki gorevi: Controller policy sabitlerini gorunen ve lokalize bir alan agacina baglar.
public partial class TestModulePermissionDefinitionProvider
{
    // Bridge kokunu ve operation izinlerini permission agacina baglar.
    private void AddBridgePermissions(PermissionGroupDefinition group)
    {
        var bridge = group.AddPermission(
            TestModulePermissions.Bridge.Default,
            L(TestModuleLocalizationKeys.Permissions.Bridge));

        bridge.AddChild(
            TestModulePermissions.Bridge.Ground,
            L(TestModuleLocalizationKeys.Permissions.BridgeGround));
        bridge.AddChild(
            TestModulePermissions.Bridge.Explain,
            L(TestModuleLocalizationKeys.Permissions.BridgeExplain));
        bridge.AddChild(
            TestModulePermissions.Bridge.Validate,
            L(TestModuleLocalizationKeys.Permissions.BridgeValidate));
        bridge.AddChild(
            TestModulePermissions.Bridge.Knowledge,
            L(TestModuleLocalizationKeys.Permissions.BridgeKnowledge));
        bridge.AddChild(
            TestModulePermissions.Bridge.Footprint,
            L(TestModuleLocalizationKeys.Permissions.BridgeFootprint));
        bridge.AddChild(TestModulePermissions.Bridge.Profile, L(TestModuleLocalizationKeys.Permissions.BridgeProfile));
        bridge.AddChild(TestModulePermissions.Bridge.Task, L(TestModuleLocalizationKeys.Permissions.BridgeTask));
        bridge.AddChild(TestModulePermissions.Bridge.PatchSuggest, L(TestModuleLocalizationKeys.Permissions.BridgePatchSuggest));
        bridge.AddChild(TestModulePermissions.Bridge.Invariant, L(TestModuleLocalizationKeys.Permissions.BridgeInvariant));
    }
}
