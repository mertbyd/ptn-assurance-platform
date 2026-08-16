namespace Ptn.ApiContractChecker.Permissions;

// islevi: Dinamik API teshis kosumunun kararli izin kodlarini tanimlar.
// sistemdeki gorevi: Diagnosis endpoint yetkisini conformance ve genel check yetkilerinden ayirir.
public partial class ApiContractCheckerPermissions
{
    public static class Diagnosis
    {
        public const string Default = GroupName + ".Diagnosis";
        public const string Execute = Default + ".Execute";
    }
}
