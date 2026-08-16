using Volo.Abp.Settings;

namespace Ptn.DatabaseChecker.Settings;

// islevi: Database Checker ayarlarinin ABP setting tanimlarini ve guvenli varsayilanlarini kaydeder.
// sistemdeki gorevi: Tenant/global override yoksa connection ve retention politikalarinin tek varsayilan kaynagi olur.
public class DatabaseCheckerSettingDefinitionProvider : SettingDefinitionProvider
{
    // islevi: Baglanti emniyeti, veri limiti ve deger saklama ayarlarini ABP setting sistemine ekler.
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                DatabaseCheckerSettings.DataComparison.MaxRowsPerTable,
                DatabaseCheckerSettings.DataComparison.DefaultMaxRowsPerTable.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Connection.ConnectTimeoutSeconds,
                DatabaseCheckerSettings.Connection.DefaultConnectTimeoutSeconds.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Connection.StatementTimeoutSeconds,
                DatabaseCheckerSettings.Connection.DefaultStatementTimeoutSeconds.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Connection.LockTimeoutSeconds,
                DatabaseCheckerSettings.Connection.DefaultLockTimeoutSeconds.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Connection.ReadOnlyTransaction,
                DatabaseCheckerSettings.Connection.DefaultReadOnlyTransaction.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Connection.ApplicationNamePrefix,
                DatabaseCheckerSettings.Connection.DefaultApplicationNamePrefix),
            new SettingDefinition(
                DatabaseCheckerSettings.DataComparison.ValueRetentionMode,
                DatabaseCheckerSettings.DataComparison.DefaultValueRetentionMode),
            new SettingDefinition(
                DatabaseCheckerSettings.DataComparison.ValueRedactionSalt,
                DatabaseCheckerSettings.DataComparison.DefaultValueRedactionSalt),
            new SettingDefinition(
                DatabaseCheckerSettings.Assertion.MaxTimeoutMs,
                DatabaseCheckerSettings.Assertion.DefaultMaxTimeoutMs.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Assertion.MinPollIntervalMs,
                DatabaseCheckerSettings.Assertion.DefaultMinPollIntervalMs.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Assertion.MaxRowsPerAssertion,
                DatabaseCheckerSettings.Assertion.DefaultMaxRowsPerAssertion.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Assertion.RegexTimeoutMs,
                DatabaseCheckerSettings.Assertion.DefaultRegexTimeoutMs.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Assertion.MaxBatchSize,
                DatabaseCheckerSettings.Assertion.DefaultMaxBatchSize.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Diagnosis.MaxProbeCount,
                DatabaseCheckerSettings.Diagnosis.DefaultMaxProbeCount.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Diagnosis.MaxDurationMs,
                DatabaseCheckerSettings.Diagnosis.DefaultMaxDurationMs.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Diagnosis.ProbeStatementTimeoutMs,
                DatabaseCheckerSettings.Diagnosis.DefaultProbeStatementTimeoutMs.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Diagnosis.MaxHypotheses,
                DatabaseCheckerSettings.Diagnosis.DefaultMaxHypotheses.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Findings.PageSize,
                DatabaseCheckerSettings.Findings.DefaultPageSize.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Findings.MaxPageSize,
                DatabaseCheckerSettings.Findings.DefaultMaxPageSize.ToString()),
            new SettingDefinition(
                DatabaseCheckerSettings.Findings.MaxResponseBytes,
                DatabaseCheckerSettings.Findings.DefaultMaxResponseBytes.ToString()));
    }
}
