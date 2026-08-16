using Volo.Abp.Authorization.Permissions;

namespace Ptn.DatabaseChecker.Permissions;

// islevi: Veritabani baglantisi yetkilerini ABP permission agacina ekler.
// sistemdeki gorevi: Connections controller ve ilerideki yazma uclari icin policy tanimlarini merkezi tutar.
public partial class DatabaseCheckerPermissionDefinitionProvider
{
    private void AddConnectionsPermissions(PermissionGroupDefinition group)
    {
        var connections = group.AddPermission(
            DatabaseCheckerPermissions.Connections.Default,
            L("Permission:Connections"));

        connections.AddChild(
            DatabaseCheckerPermissions.Connections.View,
            L("Permission:Connections.View"));

        connections.AddChild(
            DatabaseCheckerPermissions.Connections.Manage,
            L("Permission:Connections.Manage"));
    }
}
