namespace Ptn.DatabaseChecker.Permissions;

// islevi: Kayit dunyasi (ComparisonRun + bulgu tablolari + rapor) yetki sabitlerini tanimlar.
// sistemdeki gorevi: Controller [Authorize] nitelikleri string literal yerine bu sabitleri kullanir; kayit dunyasinda update/delete yetkisi bilerek yoktur (tarih degistirilemez).
public partial class DatabaseCheckerPermissions
{
    public static class Runs
    {
        public const string Default = GroupName + ".Runs";

        // Run, bulgu ve rapor listeleme/detay okuma yetkisi.
        public const string View = Default + ".View";

        // Run, bulgu ve rapor yazma yetkisi (motorun/tetikleyicinin kullandigi Create uclari).
        public const string Create = Default + ".Create";
    }
}
