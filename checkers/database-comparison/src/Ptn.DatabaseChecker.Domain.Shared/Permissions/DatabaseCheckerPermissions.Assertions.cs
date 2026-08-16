namespace Ptn.DatabaseChecker.Permissions;

// islevi: Test Module assertion yuzeyinin yetki sabitlerini tanimlar.
// sistemdeki gorevi: Assertion AppService ve controller uclari ayni named policy ile korunur.
public partial class DatabaseCheckerPermissions
{
    public static class Assertions
    {
        public const string Default = GroupName + ".Assertions";
        public const string Execute = Default + ".Execute";
        public const string ValidateDerivability = Default + ".ValidateDerivability";
    }
}
