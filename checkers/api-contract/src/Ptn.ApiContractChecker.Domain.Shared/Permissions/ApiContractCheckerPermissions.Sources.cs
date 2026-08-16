namespace Ptn.ApiContractChecker.Permissions;

// islevi: Spec kaynagi ve aggregate dokuman tanimlarinin yetki sabitlerini tanimlar.
// sistemdeki gorevi: KBP-607 CRUD yuzeyinin okuma ve yonetim sinirlarini simdiden tek agaca baglar.
public partial class ApiContractCheckerPermissions
{
    public static class Sources
    {
        public const string Default = GroupName + ".Sources";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }
}
