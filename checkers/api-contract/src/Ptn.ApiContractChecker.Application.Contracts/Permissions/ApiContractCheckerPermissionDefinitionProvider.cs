using Ptn.ApiContractChecker.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Ptn.ApiContractChecker.Permissions;

public partial class ApiContractCheckerPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ApiContractCheckerPermissions.GroupName, L(ApiContractCheckerLocalizationKeys.Permissions.Group));

        AddLookupsPermissions(myGroup);
        AddSourcesPermissions(myGroup);
        AddChecksPermissions(myGroup);
        AddConformancePermissions(myGroup);
        AddDiagnosisPermissions(myGroup);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ApiContractCheckerResource>(name);
    }
}
