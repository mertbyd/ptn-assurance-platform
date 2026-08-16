namespace Ptn.TestModule.Permissions;

// islevi: Bridge ajan yuzeyinin operation permission sabitlerini tanimlar.
// sistemdeki gorevi: Her HTTP ucunu kararli ve alan bazli bir authorization policy'sine baglar.
public partial class TestModulePermissions
{
    // Bridge permission agacinin kokunu ve operation yapraklarini toplar.
    public static class Bridge
    {
        public const string Default = GroupName + ".Bridge";
        public const string Ground = Default + ".Ground";
        public const string Explain = Default + ".Explain";
        public const string Validate = Default + ".Validate";
        public const string Knowledge = Default + ".Knowledge";
        public const string Footprint = Default + ".Footprint";
        public const string Profile = Default + ".Profile";
        public const string Task = Default + ".Task";
        public const string PatchSuggest = Default + ".PatchSuggest";
        public const string Invariant = Default + ".Invariant";
        public const string ManageSources = Default + ".ManageSources";
    }
}
