namespace Ptn.DatabaseChecker.Permissions;

// islevi: Lookup (referans veri) CRUD uclarinin yetki sabitlerini tanimlar.
// sistemdeki gorevi: 8 lookup controller'i ayni View/Manage yetkisini paylasir; permission string'leri constant olarak tutulur (golden rule 3: hard-coded string yok).
public partial class DatabaseCheckerPermissions
{
    public static class Lookups
    {
        public const string Default = GroupName + ".Lookups";

        // Lookup listeleme/detay okuma yetkisi.
        public const string View = Default + ".View";

        // Lookup olusturma/guncelleme/silme yetkisi.
        public const string Manage = Default + ".Manage";
    }
}
