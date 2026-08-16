namespace Ptn.DatabaseChecker.Permissions;

// islevi: DatabaseConnection modulunun yetki sabitlerini tanimlar.
// sistemdeki gorevi: Controller [Authorize] nitelikleri string literal yerine bu sabitleri kullanir.
public partial class DatabaseCheckerPermissions
{
    public static class Connections
    {
        public const string Default = GroupName + ".Connections";

        // Baglanti listeleme/detay okuma yetkisi.
        public const string View = Default + ".View";

        // Baglanti olusturma/guncelleme/silme yetkisi.
        public const string Manage = Default + ".Manage";
    }
}
