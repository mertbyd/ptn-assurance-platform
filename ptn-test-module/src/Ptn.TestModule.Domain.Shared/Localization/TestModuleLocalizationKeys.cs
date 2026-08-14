namespace Ptn.TestModule.Localization;

// islevi: Localization kaynaklarinda kod tarafindan kullanilan kararli anahtarlari tanimlar.
// sistemdeki gorevi: Permission tanimlarini localization JSON anahtarlariyla compile-time tek sozlesmede bulusturur.
public static class TestModuleLocalizationKeys
{
    // Permission agaci gorunen adlarinin kararli localization anahtarlarini toplar.
    public static class Permissions
    {
        public const string Group = "Permission:TestModule";
        public const string Bridge = "Permission:Bridge";
        public const string BridgeGround = "Permission:Bridge.Ground";
        public const string BridgeExplain = "Permission:Bridge.Explain";
        public const string BridgeValidate = "Permission:Bridge.Validate";
        public const string BridgeKnowledge = "Permission:Bridge.Knowledge";
        public const string BridgeFootprint = "Permission:Bridge.Footprint";
        public const string Scenarios = "Permission:Scenarios";
        public const string ScenariosCreate = "Permission:Scenarios.Create";
        public const string ScenariosUpdate = "Permission:Scenarios.Update";
        public const string ScenariosDelete = "Permission:Scenarios.Delete";
        public const string ScenariosPublish = "Permission:Scenarios.Publish";
        public const string ScenariosApprove = "Permission:Scenarios.Approve";
    }
}
