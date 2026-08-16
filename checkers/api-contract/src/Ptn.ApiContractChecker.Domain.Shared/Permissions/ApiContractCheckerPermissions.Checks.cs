namespace Ptn.ApiContractChecker.Permissions;

// islevi: Contract check gecmisi ve tetikleme islemlerinin yetki sabitlerini tanimlar.
// sistemdeki gorevi: Run okuma ile yeni kontrol baslatma yetkilerini birbirinden ayirir.
public partial class ApiContractCheckerPermissions
{
    public static class Checks
    {
        public const string Default = GroupName + ".Checks";
        public const string View = Default + ".View";
        public const string Execute = Default + ".Execute";
    }
}
