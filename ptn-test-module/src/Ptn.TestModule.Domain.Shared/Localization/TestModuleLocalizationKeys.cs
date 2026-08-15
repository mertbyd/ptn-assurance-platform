using Ptn.TestModule.ExceptionCodes.Runs;

namespace Ptn.TestModule.Localization;

// islevi: Localization kaynaklarinda kod tarafindan kullanilan kararli anahtarlari tanimlar.
// sistemdeki gorevi: Permission ve domain hata kodlarini localization JSON anahtarlariyla compile-time tek sozlesmede bulusturur.
/// <summary>
/// Test Module localization kaynaklarinin kararli anahtarlarini tasir.
/// </summary>
public static class TestModuleLocalizationKeys
{
    // Permission agaci gorunen adlarinin kararli localization anahtarlarini toplar.
    /// <summary>Permission gorunen adlarinin localization anahtarlarini tasir.</summary>
    public static class Permissions
    {
        /// <summary>Test Module permission grup adinin anahtaridir.</summary>
        public const string Group = "Permission:TestModule";

        /// <summary>Bridge permission adinin anahtaridir.</summary>
        public const string Bridge = "Permission:Bridge";

        /// <summary>Bridge ground permission adinin anahtaridir.</summary>
        public const string BridgeGround = "Permission:Bridge.Ground";

        /// <summary>Bridge explain permission adinin anahtaridir.</summary>
        public const string BridgeExplain = "Permission:Bridge.Explain";

        /// <summary>Bridge validate permission adinin anahtaridir.</summary>
        public const string BridgeValidate = "Permission:Bridge.Validate";

        /// <summary>Bridge knowledge permission adinin anahtaridir.</summary>
        public const string BridgeKnowledge = "Permission:Bridge.Knowledge";

        /// <summary>Bridge footprint permission adinin anahtaridir.</summary>
        public const string BridgeFootprint = "Permission:Bridge.Footprint";

        /// <summary>Senaryo permission adinin anahtaridir.</summary>
        public const string Scenarios = "Permission:Scenarios";

        /// <summary>Senaryo create permission adinin anahtaridir.</summary>
        public const string ScenariosCreate = "Permission:Scenarios.Create";

        /// <summary>Senaryo update permission adinin anahtaridir.</summary>
        public const string ScenariosUpdate = "Permission:Scenarios.Update";

        /// <summary>Senaryo delete permission adinin anahtaridir.</summary>
        public const string ScenariosDelete = "Permission:Scenarios.Delete";

        /// <summary>Senaryo publish permission adinin anahtaridir.</summary>
        public const string ScenariosPublish = "Permission:Scenarios.Publish";

        /// <summary>Senaryo approve permission adinin anahtaridir.</summary>
        public const string ScenariosApprove = "Permission:Scenarios.Approve";

        /// <summary>Test kosumu permission adinin anahtaridir.</summary>
        public const string Runs = "Permission:Runs";

        /// <summary>Test kosumu okuma permission adinin anahtaridir.</summary>
        public const string RunsView = "Permission:Runs.View";

        /// <summary>Test kosumu tetikleme permission adinin anahtaridir.</summary>
        public const string RunsTrigger = "Permission:Runs.Trigger";

        /// <summary>Test kosumu claim permission adinin anahtaridir.</summary>
        public const string RunsStart = "Permission:Runs.Start";

        /// <summary>Test kosumu terminal yazma permission adinin anahtaridir.</summary>
        public const string RunsWriteResult = "Permission:Runs.WriteResult";

        /// <summary>Test lookup okuma permission adinin anahtaridir.</summary>
        public const string Lookups = "Permission:Lookups";
    }

    // Kosum BusinessException kodlarini localization kaynagindaki ayni anahtarlara baglar.
    /// <summary>Test kosum domain hatalarinin localization anahtarlarini tasir.</summary>
    public static class RunErrors
    {
        /// <summary>Baglanmamis ortam hata mesajinin anahtaridir.</summary>
        public const string EnvironmentNotBound = TestModuleRunErrorCodes.EnvironmentNotBound;

        /// <summary>Uyusmayan ortam hata mesajinin anahtaridir.</summary>
        public const string EnvironmentMismatch = TestModuleRunErrorCodes.EnvironmentMismatch;

        /// <summary>Daha once claim edilmis kosum hata mesajinin anahtaridir.</summary>
        public const string RunAlreadyClaimed = TestModuleRunErrorCodes.RunAlreadyClaimed;

        /// <summary>Daha once yazilmis attempt hata mesajinin anahtaridir.</summary>
        public const string AttemptAlreadyWritten = TestModuleRunErrorCodes.AttemptAlreadyWritten;

        /// <summary>Passed hukumde sorun verisi hata mesajinin anahtaridir.</summary>
        public const string PassedOutcomeCarriesFailureData = TestModuleRunErrorCodes.PassedOutcomeCarriesFailureData;

        /// <summary>Gecersiz trace kimligi hata mesajinin anahtaridir.</summary>
        public const string InvalidTraceId = TestModuleRunErrorCodes.InvalidTraceId;

        /// <summary>Buyuk diagnosis raporu hata mesajinin anahtaridir.</summary>
        public const string DiagnosisReportTooLarge = TestModuleRunErrorCodes.DiagnosisReportTooLarge;

        /// <summary>Kosum silme reddi hata mesajinin anahtaridir.</summary>
        public const string RunDeletionNotAllowed = TestModuleRunErrorCodes.RunDeletionNotAllowed;

        /// <summary>Gecersiz fingerprint hata mesajinin anahtaridir.</summary>
        public const string InvalidFingerprint = TestModuleRunErrorCodes.InvalidFingerprint;

        /// <summary>Eksik material drift hata mesajinin anahtaridir.</summary>
        public const string MaterialDriftRequired = TestModuleRunErrorCodes.MaterialDriftRequired;

        /// <summary>Desteklenmeyen kaynak checker hata mesajinin anahtaridir.</summary>
        public const string SourceCheckerNotSupported = TestModuleRunErrorCodes.SourceCheckerNotSupported;

        /// <summary>Gecersiz kosum suresi hata mesajinin anahtaridir.</summary>
        public const string DurationInvalid = TestModuleRunErrorCodes.DurationInvalid;
    }
}
