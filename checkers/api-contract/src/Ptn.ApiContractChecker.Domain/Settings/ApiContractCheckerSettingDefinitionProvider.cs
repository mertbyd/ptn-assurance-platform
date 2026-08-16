using Volo.Abp.Settings;

namespace Ptn.ApiContractChecker.Settings;

// islevi: API Contract Checker conformance ayarlarini guvenli varsayilanlarla ABP setting sistemine kaydeder.
// sistemdeki gorevi: Tenant ve global override yokken oracle butcesi ile None retention politikasinin tek tanim kaynagidir.
public class ApiContractCheckerSettingDefinitionProvider : SettingDefinitionProvider
{
    // islevi: Conformance limit, cache ve deger saklama ayarlarini setting kataloguna ekler.
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            Define(ApiContractCheckerSettings.Conformance.MaxViolations,
                ApiContractCheckerSettings.Conformance.DefaultMaxViolations),
            Define(ApiContractCheckerSettings.Conformance.MaxResponseBytes,
                ApiContractCheckerSettings.Conformance.DefaultMaxResponseBytes),
            Define(ApiContractCheckerSettings.Conformance.SchemaCacheMinutes,
                ApiContractCheckerSettings.Conformance.DefaultSchemaCacheMinutes),
            new SettingDefinition(
                ApiContractCheckerSettings.Conformance.ValueRetentionMode,
                ApiContractCheckerSettings.Conformance.DefaultValueRetentionMode),
            new SettingDefinition(
                ApiContractCheckerSettings.Conformance.ValueRedactionSalt,
                ApiContractCheckerSettings.Conformance.DefaultValueRedactionSalt),
            Define(ApiContractCheckerSettings.Diagnosis.MaxProbeCount,
                ApiContractCheckerSettings.Diagnosis.DefaultMaxProbeCount),
            Define(ApiContractCheckerSettings.Diagnosis.MaxProbeDurationMs,
                ApiContractCheckerSettings.Diagnosis.DefaultMaxProbeDurationMs),
            Define(ApiContractCheckerSettings.Diagnosis.ProbeTimeoutMs,
                ApiContractCheckerSettings.Diagnosis.DefaultProbeTimeoutMs),
            Define(ApiContractCheckerSettings.Diagnosis.MaxHypotheses,
                ApiContractCheckerSettings.Diagnosis.DefaultMaxHypotheses),
            Define(ApiContractCheckerSettings.Findings.DefaultPageSize,
                Constants.Runs.ContractCheckRunConsts.DefaultFindingPageSize),
            Define(ApiContractCheckerSettings.Findings.MaxPageSize,
                Constants.Runs.ContractCheckRunConsts.DefaultMaxFindingPageSize),
            Define(ApiContractCheckerSettings.Findings.MaxResponseBytes,
                Constants.Runs.ContractCheckRunConsts.DefaultFindingPageMaxBytes),
            Define(ApiContractCheckerSettings.Snapshots.ResultReferenceMinutes,
                Constants.Snapshots.SnapshotAuthoringConstants.DefaultResultReferenceMinutes));
    }

    // islevi: Sayisal varsayilani invariant metinle ABP setting tanimina cevirir.
    private static SettingDefinition Define(string name, int defaultValue)
    {
        return new SettingDefinition(name, defaultValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}
