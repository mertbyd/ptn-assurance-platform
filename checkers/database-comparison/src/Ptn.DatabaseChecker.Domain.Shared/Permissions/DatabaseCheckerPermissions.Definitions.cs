namespace Ptn.DatabaseChecker.Permissions;

// islevi: ComparisonDefinition modulunun yetki sabitlerini tanimlar.
// sistemdeki gorevi: Controller [Authorize] nitelikleri string literal yerine bu sabitleri kullanir.
public partial class DatabaseCheckerPermissions
{
    public static class Definitions
    {
        public const string Default = GroupName + ".Definitions";

        // Tarif listeleme/detay okuma yetkisi.
        public const string View = Default + ".View";

        // Tarif olusturma/guncelleme/silme yetkisi.
        public const string Manage = Default + ".Manage";
    }
}
