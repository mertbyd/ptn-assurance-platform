using Ptn.ApiContractChecker.Constants.Conformance.Lookups;

namespace Ptn.ApiContractChecker.Settings;

// islevi: API Contract Checker runtime esik ve retention setting adlariyla varsayilanlarini tanimlar.
// sistemdeki gorevi: Tenant -> global -> default zincirindeki oracle butcesi ve deger saklama politikasinin Domain.Shared sahibidir.
public static class ApiContractCheckerSettings
{
    public const string GroupName = "ApiContractChecker";

    // islevi: Response conformance sonuc ve schema cache esiklerini tek setting ailesinde toplar.
    // sistemdeki gorevi: Manager ve resolver'in inline limit kullanmadan tenant-aware ayar okumasini saglar.
    public static class Conformance
    {
        public const int DefaultMaxViolations = 4;
        public const int DefaultMaxResponseBytes = 512;
        public const int DefaultSchemaCacheMinutes = 60;
        public const string DefaultValueRetentionMode = ValueRetentionModeCodes.None;
        public const string DefaultValueRedactionSalt = "";

        public const string MaxViolations = GroupName + ".Conformance.MaxViolations";
        public const string MaxResponseBytes = GroupName + ".Conformance.MaxResponseBytes";
        public const string SchemaCacheMinutes = GroupName + ".Conformance.SchemaCacheMinutes";
        public const string ValueRetentionMode = GroupName + ".Conformance.ValueRetentionMode";
        public const string ValueRedactionSalt = GroupName + ".Conformance.ValueRedactionSalt";
    }

    public static class Diagnosis
    {
        public const int DefaultMaxProbeCount = 5;
        public const int DefaultMaxProbeDurationMs = 2000;
        public const int DefaultProbeTimeoutMs = 500;
        public const int DefaultMaxHypotheses = 19;
        public const string MaxProbeCount = GroupName + ".Diagnosis.MaxProbeCount";
        public const string MaxProbeDurationMs = GroupName + ".Diagnosis.MaxProbeDurationMs";
        public const string ProbeTimeoutMs = GroupName + ".Diagnosis.ProbeTimeoutMs";
        public const string MaxHypotheses = GroupName + ".Diagnosis.MaxHypotheses";
    }

    // islevi: Run bulgu sayfasi boyut ve adet esiklerini tek setting ailesinde toplar.
    // sistemdeki gorevi: Bakim ani cikti butcesini tenant ve global override zincirine acar.
    public static class Findings
    {
        public const string DefaultPageSize = GroupName + ".Findings.DefaultPageSize";
        public const string MaxPageSize = GroupName + ".Findings.MaxPageSize";
        public const string MaxResponseBytes = GroupName + ".Findings.MaxResponseBytes";
    }

    public static class Snapshots
    {
        public const string ResultReferenceMinutes = GroupName + ".Snapshots.ResultReferenceMinutes";
    }
}
