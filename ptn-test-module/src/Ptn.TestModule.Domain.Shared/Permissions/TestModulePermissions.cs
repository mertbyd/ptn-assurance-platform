using Volo.Abp.Reflection;

namespace Ptn.TestModule.Permissions;

// islevi: Modulun ABP permission agacinin kok adini ve tum sabitlerinin toplanma noktasini tanimlar.
// sistemdeki gorevi: Permission adlari tek sahipten gelir; alan bazli partial dosyalar bu koke baglanir.
public partial class TestModulePermissions
{
    public const string GroupName = "TestModule";

    // Tanimlanan tum permission sabitlerini ABP'nin bekledigi bicimde dondurur.
    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(TestModulePermissions));
    }
}
