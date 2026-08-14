using Ptn.TestModule.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Ptn.TestModule.Permissions;

// islevi: Modulun permission agacini ABP permission yonetimine kaydeder.
// sistemdeki gorevi: Alan bazli partial metotlari cagirarak permission dikeylerini birbirinden ayirir.
public partial class TestModulePermissionDefinitionProvider : PermissionDefinitionProvider
{
    // Kok permission grubunu kurar ve her alanin kendi tanimini ekletir.
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(
            TestModulePermissions.GroupName,
            L(TestModuleLocalizationKeys.Permissions.Group));
        AddBridgePermissions(group);
    }

    // Localization anahtarini modul kaynagindan olusturur.
    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TestModuleResource>(name);
    }
}
