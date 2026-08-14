using Volo.Abp.Reflection;

namespace Ptn.TestModule.Permissions;

public class TestModulePermissions
{
    public const string GroupName = "TestModule";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(TestModulePermissions));
    }
}
