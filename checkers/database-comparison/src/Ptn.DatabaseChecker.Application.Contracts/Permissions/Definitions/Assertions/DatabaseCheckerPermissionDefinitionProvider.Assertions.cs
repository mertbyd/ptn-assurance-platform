using Volo.Abp.Authorization.Permissions;

namespace Ptn.DatabaseChecker.Permissions;

// islevi: Assertion calistirma yetkisini ABP permission agacina ekler.
// sistemdeki gorevi: Test Module'un oracle uclarina erisimi baglanti yonetim yetkisinden bagimsiz yonetilir.
public partial class DatabaseCheckerPermissionDefinitionProvider
{
    // islevi: Assertions ana iznini ve Execute alt iznini lokalize adlariyla kaydeder.
    private void AddAssertionsPermissions(PermissionGroupDefinition group)
    {
        var assertions = group.AddPermission(
            DatabaseCheckerPermissions.Assertions.Default,
            L("Permission:Assertions"));

        assertions.AddChild(
            DatabaseCheckerPermissions.Assertions.Execute,
            L("Permission:Assertions.Execute"));
        assertions.AddChild(
            DatabaseCheckerPermissions.Assertions.ValidateDerivability,
            L("Permission:Assertions.ValidateDerivability"));
    }
}
