namespace Ptn.TestModule.Permissions;

// islevi: Senaryo katalogu use-case permission sabitlerini tanimlar.
// sistemdeki gorevi: Okuma, yazarlik, silme reddi ve insan onayini kararli policy adlarina baglar.
public partial class TestModulePermissions
{
    public static class Scenarios
    {
        public const string Default = GroupName + ".Scenarios";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Publish = Default + ".Publish";
        public const string Approve = Default + ".Approve";

        /// <summary>Senaryoyu sinirli sureyle karantinaya alma permission'idir.</summary>
        public const string Quarantine = Default + ".Quarantine";
    }
}
