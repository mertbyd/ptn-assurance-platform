using Volo.Abp.Authorization.Permissions;

namespace Ptn.DatabaseChecker.Permissions;

// islevi: Run, fark ve rapor yetkilerini ABP permission agacina ekler.
// sistemdeki gorevi: Run tarihcesi, fark bulgulari ve rapor okuma/yazma policy'lerini ayni modul altinda toplar.
public partial class DatabaseCheckerPermissionDefinitionProvider
{
    private void AddRunsPermissions(PermissionGroupDefinition group)
    {
        var runs = group.AddPermission(
            DatabaseCheckerPermissions.Runs.Default,
            L("Permission:Runs"));

        runs.AddChild(
            DatabaseCheckerPermissions.Runs.View,
            L("Permission:Runs.View"));

        runs.AddChild(
            DatabaseCheckerPermissions.Runs.Create,
            L("Permission:Runs.Create"));
    }
}
