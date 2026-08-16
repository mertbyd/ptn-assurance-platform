using Volo.Abp.Authorization.Permissions;

namespace Ptn.DatabaseChecker.Permissions;

// islevi: Karsilastirma tanimi yetkilerini ABP permission agacina ekler.
// sistemdeki gorevi: ComparisonDefinition controller yetkilendirmesini tek modul altinda toplar.
public partial class DatabaseCheckerPermissionDefinitionProvider
{
    private void AddDefinitionsPermissions(PermissionGroupDefinition group)
    {
        var definitions = group.AddPermission(
            DatabaseCheckerPermissions.Definitions.Default,
            L("Permission:Definitions"));

        definitions.AddChild(
            DatabaseCheckerPermissions.Definitions.View,
            L("Permission:Definitions.View"));

        definitions.AddChild(
            DatabaseCheckerPermissions.Definitions.Manage,
            L("Permission:Definitions.Manage"));
    }
}
