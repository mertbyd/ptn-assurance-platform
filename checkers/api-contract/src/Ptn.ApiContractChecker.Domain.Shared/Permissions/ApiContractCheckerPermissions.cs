using Volo.Abp.Reflection;

namespace Ptn.ApiContractChecker.Permissions;

public partial class ApiContractCheckerPermissions
{
    public const string GroupName = "ApiContractChecker";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(ApiContractCheckerPermissions));
    }

    // Response conformance oracle'inin ayri yetki yuzeyi.
    public static class Conformance
    {
        public const string Default = GroupName + ".Conformance";
        public const string Execute = Default + ".Execute";
        public const string GenerateSamples = Default + ".GenerateSamples";
        public const string SuggestLinks = Default + ".SuggestLinks";
    }
}
