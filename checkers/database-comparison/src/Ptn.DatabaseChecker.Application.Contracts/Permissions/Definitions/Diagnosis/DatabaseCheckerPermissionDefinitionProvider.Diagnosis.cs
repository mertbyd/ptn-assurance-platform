using Volo.Abp.Authorization.Permissions;

namespace Ptn.DatabaseChecker.Permissions;

// islevi: Dinamik teshis calistirma yetkisini ABP permission agacina ekler.
// sistemdeki gorevi: Test Module'un teshis oracle'ina erisimini assertion ve connection yonetiminden bagimsiz yetkilendirir.
public partial class DatabaseCheckerPermissionDefinitionProvider
{
    // islevi: Diagnosis ana iznini ve Execute alt iznini lokalize adlariyla kaydeder.
    private void AddDiagnosisPermissions(PermissionGroupDefinition group)
    {
        var diagnosis = group.AddPermission(
            DatabaseCheckerPermissions.Diagnosis.Default,
            L("Permission:Diagnosis"));

        diagnosis.AddChild(
            DatabaseCheckerPermissions.Diagnosis.Execute,
            L("Permission:Diagnosis.Execute"));
    }
}
