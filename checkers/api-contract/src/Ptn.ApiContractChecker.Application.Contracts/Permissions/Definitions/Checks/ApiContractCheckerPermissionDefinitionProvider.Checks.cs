using Volo.Abp.Authorization.Permissions;
using Ptn.ApiContractChecker.Localization;

namespace Ptn.ApiContractChecker.Permissions;

// islevi: Contract check gecmisi ve tetikleme yetkilerini ABP permission agacina ekler.
// sistemdeki gorevi: Sonuc okuma ile yeni asenkron kontrol baslatma yetkisini ayri denetlenebilir kilar.
public partial class ApiContractCheckerPermissionDefinitionProvider
{
    // Kontrol okuma ve tetikleme izinlerini ana permission grubuna ekler.
    private void AddChecksPermissions(PermissionGroupDefinition group)
    {
        var checks = group.AddPermission(ApiContractCheckerPermissions.Checks.Default, L(ApiContractCheckerLocalizationKeys.Permissions.Checks));
        checks.AddChild(ApiContractCheckerPermissions.Checks.View, L(ApiContractCheckerLocalizationKeys.Permissions.ChecksView));
        checks.AddChild(ApiContractCheckerPermissions.Checks.Execute, L(ApiContractCheckerLocalizationKeys.Permissions.ChecksExecute));
    }
}
