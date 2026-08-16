using Volo.Abp.Authorization.Permissions;

namespace Ptn.DatabaseChecker.Permissions;

// islevi: Lookup yetkilerini ABP permission agacina ekler.
// sistemdeki gorevi: Ana provider'in Define metodu bu partial metodu cagirir; yeni modul eklemek mevcut satirlari degistirmeden tek cagri ekler.
public partial class DatabaseCheckerPermissionDefinitionProvider
{
    private void AddLookupsPermissions(PermissionGroupDefinition group)
    {
        var lookups = group.AddPermission(
            DatabaseCheckerPermissions.Lookups.Default,
            L("Permission:Lookups"));

        lookups.AddChild(
            DatabaseCheckerPermissions.Lookups.View,
            L("Permission:Lookups.View"));

        lookups.AddChild(
            DatabaseCheckerPermissions.Lookups.Manage,
            L("Permission:Lookups.Manage"));
    }
}
