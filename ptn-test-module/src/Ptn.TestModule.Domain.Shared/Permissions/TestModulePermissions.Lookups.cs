namespace Ptn.TestModule.Permissions;

// islevi: Test lookup okuma yuzeyinin operation permission sabitini tanimlar.
// sistemdeki gorevi: Bes lookup ucunu tek kararli policy adina baglar; yazma ucu olmadigi icin yazma izni de yoktur.
/// <summary>Test lookup okuma permission adini tasir.</summary>
public partial class TestModulePermissions
{
    /// <summary>Test lookup permission agacini tasir.</summary>
    public static class Lookups
    {
        /// <summary>Bes test lookup'ini okuma permission'idir.</summary>
        public const string Default = GroupName + ".Lookups";
    }
}
