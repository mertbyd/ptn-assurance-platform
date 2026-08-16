using Volo.Abp.Reflection;

namespace Ptn.DatabaseChecker.Permissions;

public partial class DatabaseCheckerPermissions
{
    public const string GroupName = "DatabaseChecker";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(DatabaseCheckerPermissions));
    }

    // islevi: Write-set capability yuzeyinin named permission sabitlerini gruplar.
    // sistemdeki gorevi: Probe ile slot/diff capture yetkilerini birbirinden bagimsiz yonetir.
    public static class Capabilities
    {
        public const string Default = GroupName + ".Capabilities";
        public const string Probe = Default + ".Probe";
        public const string Capture = Default + ".Capture";
    }

    // islevi: Dinamik teshis API yuzeyinin named permission sabitlerini gruplar.
    // sistemdeki gorevi: Controller ve AppService'in ayni operasyon-ozel yetkiyi kullanmasini saglar.
    public static class Diagnosis
    {
        public const string Default = GroupName + ".Diagnosis";
        public const string Execute = Default + ".Execute";
    }

    // islevi: Salt-okunur projection API yuzeyinin named permission sabitlerini gruplar.
    // sistemdeki gorevi: Projection controller ve permission agacinin ayni operasyon-ozel yetkiyi kullanmasini saglar.
    public static class Projections
    {
        public const string Default = GroupName + ".Projections";
        public const string Execute = Default + ".Execute";
    }
}
