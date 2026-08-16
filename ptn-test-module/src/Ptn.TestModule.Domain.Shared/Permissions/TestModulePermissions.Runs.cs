namespace Ptn.TestModule.Permissions;

// islevi: Test kosumu okuma, tetikleme, claim ve terminal yazim permission sabitlerini tanimlar.
// sistemdeki gorevi: Application ve HTTP policy adlarini kararli Domain.Shared sozlesmesine baglar.
/// <summary>Test kosumu use-case permission adlarini tasir.</summary>
public partial class TestModulePermissions
{
    /// <summary>Test kosumu permission agacini tasir.</summary>
    public static class Runs
    {
        /// <summary>Test kosumu alanina erisim permission'idir.</summary>
        public const string Default = GroupName + ".Runs";

        /// <summary>Kosum ve bulgulu sonuc okuma permission'idir.</summary>
        public const string View = Default + ".View";

        /// <summary>Yeni Pending kosum olusturma permission'idir.</summary>
        public const string Trigger = Default + ".Trigger";

        /// <summary>Pending kosumu Running durumuna claim etme permission'idir.</summary>
        public const string Start = Default + ".Start";

        /// <summary>Terminal sonuc ve bulgulari atomik yazma permission'idir.</summary>
        public const string WriteResult = Default + ".WriteResult";

        /// <summary>Kosumu standart formatlara ihrac etme permission'idir.</summary>
        public const string Export = Default + ".Export";

        /// <summary>Running kosuma kooperatif iptal talebi yazma permission'idir.</summary>
        public const string Cancel = Default + ".Cancel";

        /// <summary>Yazma yetkili sandbox verisini sifirlama permission'idir.</summary>
        public const string SandboxReset = Default + ".SandboxReset";

        /// <summary>Tenant ortam baglama haritasini yazma permission'idir.</summary>
        public const string ManageEnvironments = Default + ".ManageEnvironments";
    }
}
