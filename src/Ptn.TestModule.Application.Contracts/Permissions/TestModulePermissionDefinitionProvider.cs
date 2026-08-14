using Ptn.TestModule.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Ptn.TestModule.Permissions;

public class TestModulePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(TestModulePermissions.GroupName, L("Permission:TestModule"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TestModuleResource>(name);
    }
}
