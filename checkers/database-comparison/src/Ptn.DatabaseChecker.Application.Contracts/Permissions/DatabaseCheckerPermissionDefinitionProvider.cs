using Ptn.DatabaseChecker.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Ptn.DatabaseChecker.Permissions;

public partial class DatabaseCheckerPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(DatabaseCheckerPermissions.GroupName, L("Permission:DatabaseChecker"));

        AddCapabilityPermissions(myGroup);
        AddLookupsPermissions(myGroup);
        AddConnectionsPermissions(myGroup);
        AddDefinitionsPermissions(myGroup);
        AddRunsPermissions(myGroup);
        AddAssertionsPermissions(myGroup);
        AddDiagnosisPermissions(myGroup);
        AddProjectionPermissions(myGroup);
    }

    // islevi: Capabilities ana iznini Probe ve Capture alt izinleriyle ABP permission agacina kaydeder.
    private void AddCapabilityPermissions(PermissionGroupDefinition group)
    {
        var capabilities = group.AddPermission(
            DatabaseCheckerPermissions.Capabilities.Default,
            L("Permission:Capabilities"));
        capabilities.AddChild(
            DatabaseCheckerPermissions.Capabilities.Probe,
            L("Permission:Capabilities.Probe"));
        capabilities.AddChild(
            DatabaseCheckerPermissions.Capabilities.Capture,
            L("Permission:Capabilities.Capture"));
    }

    // islevi: Projections ana iznini ve Execute alt iznini lokalize adlariyla kaydeder.
    private void AddProjectionPermissions(PermissionGroupDefinition group)
    {
        var projections = group.AddPermission(
            DatabaseCheckerPermissions.Projections.Default,
            L("Permission:Projections"));
        projections.AddChild(
            DatabaseCheckerPermissions.Projections.Execute,
            L("Permission:Projections.Execute"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<DatabaseCheckerResource>(name);
    }
}
