namespace Ptn.ApiContractChecker.Permissions;

// islevi: Lookup referans verisinin okuma ve tarihceyi koruyan yonetim uclarinin yetki sabitlerini tanimlar.
// sistemdeki gorevi: Bes lookup controller'i ayni View/Manage yetkisini paylasir; permission string'leri tek kaynaktan gelir.
public partial class ApiContractCheckerPermissions
{
    public static class Lookups
    {
        public const string Default = GroupName + ".Lookups";

        // Lookup listeleme/detay okuma yetkisi.
        public const string View = Default + ".View";

        // Lookup olusturma, guncelleme ve pasiflestirme yetkisi.
        public const string Manage = Default + ".Manage";
    }
}
