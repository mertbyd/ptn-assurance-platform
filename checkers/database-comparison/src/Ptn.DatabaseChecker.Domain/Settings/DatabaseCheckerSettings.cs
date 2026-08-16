using Ptn.DatabaseChecker.Constants.Comparison;

namespace Ptn.DatabaseChecker.Settings;

// islevi: Database Checker icin tenant -> global -> default zincirinden okunabilen kararli ayar adlarini ve varsayilanlarini tanimlar.
// sistemdeki gorevi: Baglanti emniyeti ve veri saklama esiklerinin kod icinde daginik sabitlere donusmesini engeller.
public static class DatabaseCheckerSettings
{
    public const string GroupName = "DatabaseChecker";

    // islevi: Hedef baglantinin timeout, read-only ve uygulama kimligi setting sozlesmesini gruplar.
    // sistemdeki gorevi: ConnectionSafetyProfileResolver'in okudugu ad ve varsayilanlari tek sahipte tutar.
    public static class Connection
    {
        public const int DefaultConnectTimeoutSeconds = 10;
        public const int DefaultStatementTimeoutSeconds = 30;
        public const int DefaultLockTimeoutSeconds = 5;
        public const bool DefaultReadOnlyTransaction = true;
        public const string DefaultApplicationNamePrefix = "CheckNexus.DatabaseComparison";

        public const string ConnectTimeoutSeconds = GroupName + ".Connection.ConnectTimeoutSeconds";
        public const string StatementTimeoutSeconds = GroupName + ".Connection.StatementTimeoutSeconds";
        public const string LockTimeoutSeconds = GroupName + ".Connection.LockTimeoutSeconds";
        public const string ReadOnlyTransaction = GroupName + ".Connection.ReadOnlyTransaction";
        public const string ApplicationNamePrefix = GroupName + ".Connection.ApplicationNamePrefix";
    }

    // islevi: Exact veri karsilastirmasinin satir limiti ve bulgu deger saklama setting sozlesmesini gruplar.
    // sistemdeki gorevi: Veri okuma limiti ile redaction resolver'inin ad ve varsayilanlarini tek sahipte tutar.
    public static class DataComparison
    {
        // Exact row/cell karsilastirmasinda tablo basina varsayilan guvenlik limiti.
        public const int DefaultMaxRowsPerTable = 100000;

        // Tenant bazinda degistirilebilir exact row/cell tablo limitinin kararli setting adi.
        public const string MaxRowsPerTable = GroupName + ".DataComparison.MaxRowsPerTable";

        public const string DefaultValueRetentionMode = ValueRetentionModeCodes.None;
        public const string DefaultValueRedactionSalt = "";

        public const string ValueRetentionMode = GroupName + ".DataComparison.ValueRetentionMode";
        public const string ValueRedactionSalt = GroupName + ".DataComparison.ValueRedactionSalt";
    }

    // islevi: Test Module assertion timeout, polling, cevap boyutu, regex ve batch setting sozlesmesini gruplar.
    // sistemdeki gorevi: AssertionSettingsResolver'in tenant-aware okudugu ad ve varsayilanlari mevcut setting sahibinde tutar.
    public static class Assertion
    {
        public const int DefaultMaxTimeoutMs = 30000;
        public const int DefaultMinPollIntervalMs = 100;
        public const int DefaultMaxRowsPerAssertion = 100;
        public const int DefaultRegexTimeoutMs = 200;
        public const int DefaultMaxBatchSize = 20;

        public const string MaxTimeoutMs = GroupName + ".Assertion.MaxTimeoutMs";
        public const string MinPollIntervalMs = GroupName + ".Assertion.MinPollIntervalMs";
        public const string MaxRowsPerAssertion = GroupName + ".Assertion.MaxRowsPerAssertion";
        public const string RegexTimeoutMs = GroupName + ".Assertion.RegexTimeoutMs";
        public const string MaxBatchSize = GroupName + ".Assertion.MaxBatchSize";
    }

    // islevi: Dinamik teshis probe ve rapor butcesinin tenant-aware setting sozlesmesini gruplar.
    // sistemdeki gorevi: Canli hedef okumalarinin sure/adet ve hipotez cikti tavanlarini tek sahipte tutar.
    public static class Diagnosis
    {
        public const int DefaultMaxProbeCount = 8;
        public const int DefaultMaxDurationMs = 3000;
        public const int DefaultProbeStatementTimeoutMs = 1000;
        public const int DefaultMaxHypotheses = 5;

        public const string MaxProbeCount = GroupName + ".Diagnosis.MaxProbeCount";
        public const string MaxDurationMs = GroupName + ".Diagnosis.MaxDurationMs";
        public const string ProbeStatementTimeoutMs = GroupName + ".Diagnosis.ProbeStatementTimeoutMs";
        public const string MaxHypotheses = GroupName + ".Diagnosis.MaxHypotheses";
    }

    // islevi: MCP bulgu okumasinin sayfa ve cevap boyutu setting sozlesmesini gruplar.
    // sistemdeki gorevi: Kucuk-parca okuma tavanlarini tenant -> global -> default zincirinde tutar.
    /// <summary>
    /// MCP bulgu okumasinin sayfa ve cevap boyutu ayar adlari ile varsayilanlari.
    /// </summary>
    public static class Findings
    {
        /// <summary>Varsayilan bulgu sayfasi kayit sayisi.</summary>
        public const int DefaultPageSize = ComparisonRunConsts.DefaultFindingPageSize;
        /// <summary>Varsayilan azami bulgu sayfasi kayit sayisi.</summary>
        public const int DefaultMaxPageSize = ComparisonRunConsts.DefaultMaxFindingPageSize;
        /// <summary>Varsayilan azami bulgu cevabi UTF-8 byte sayisi.</summary>
        public const int DefaultMaxResponseBytes = ComparisonRunConsts.DefaultFindingResponseBytes;

        /// <summary>Varsayilan sayfa boyutu setting adi.</summary>
        public const string PageSize = GroupName + ".Findings.PageSize";
        /// <summary>Azami sayfa boyutu setting adi.</summary>
        public const string MaxPageSize = GroupName + ".Findings.MaxPageSize";
        /// <summary>Azami cevap byte butcesi setting adi.</summary>
        public const string MaxResponseBytes = GroupName + ".Findings.MaxResponseBytes";
    }
}
