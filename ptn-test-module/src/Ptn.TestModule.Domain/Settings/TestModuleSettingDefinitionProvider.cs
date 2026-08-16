using System.Globalization;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Constants.Runs;
using Volo.Abp.Settings;

namespace Ptn.TestModule.Settings;

// islevi: Test Module setting kataloguna profil yolu ile tenant ortam baglama haritasini kaydeder.
// sistemdeki gorevi: Profil ve kosum ortamlarinin ABP setting provider zincirinden cozulmesini saglar.
/// <summary>
/// Test Module'un profil ve ortam ABP setting tanimlarini kaydeder.
/// </summary>
public class TestModuleSettingDefinitionProvider : SettingDefinitionProvider
{
    // Kopru profil yolu ile bos tenant ortam haritasini ABP setting sistemine tanimlar.
    /// <summary>Test Module tarafindan sahiplenilen setting tanimlarini ekler.</summary>
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(new SettingDefinition(
            TestModuleSettings.ProfilePackPath,
            PtnBridgeSettingNames.DefaultProfilePackPath));
        context.Add(new SettingDefinition(
            TestModuleSettings.ProfilePackKey,
            PtnBridgeSettingNames.DefaultProfilePackKey));
        context.Add(new SettingDefinition(
            TestModuleSettings.BusinessRulesPath,
            PtnBridgeSettingNames.DefaultBusinessRulesPath));
        context.Add(new SettingDefinition(
            TestModuleSettings.AgentPolicyPath,
            PtnBridgeSettingNames.DefaultAgentPolicyPath));
        context.Add(new SettingDefinition(
            TestModuleSettings.EnvironmentBindings,
            TestModuleRunSettingNames.DefaultEnvironmentBindings));
        context.Add(new SettingDefinition(
            TestModuleSettings.RunnerImage,
            WorkflowRunnerConsts.RedoclyCliImage));
        context.Add(new SettingDefinition(
            TestModuleSettings.RunnerExecutionTimeoutSeconds,
            WorkflowRunnerConsts.DefaultExecutionTimeoutSeconds.ToString(CultureInfo.InvariantCulture)));
        context.Add(new SettingDefinition(
            TestModuleSettings.RunnerMaxFetchTimeoutSeconds,
            WorkflowRunnerConsts.DefaultMaxFetchTimeoutSeconds.ToString(CultureInfo.InvariantCulture)));
        // Ag ayarlari bos birakildiginda docker arguman listesi bugunku haliyle birebir korunur.
        context.Add(new SettingDefinition(
            TestModuleSettings.RunnerNetworkMode,
            TestModuleRunSettingNames.DefaultRunnerNetworkMode));
        context.Add(new SettingDefinition(
            TestModuleSettings.RunnerExtraHosts,
            TestModuleRunSettingNames.DefaultRunnerExtraHosts));
        context.Add(new SettingDefinition(
            TestModuleSettings.HarRetentionDays,
            TestModuleRunSettingNames.DefaultHarRetentionDays));
        context.Add(new SettingDefinition(
            TestModuleSettings.RunRetentionDays,
            TestModuleRunSettingNames.DefaultRunRetentionDays));
        context.Add(new SettingDefinition(
            TestModuleSettings.RunPurgeBatchSize,
            TestModuleRunSettingNames.DefaultRunPurgeBatchSize));
        context.Add(new SettingDefinition(
            TestModuleSettings.StaleRunThresholdMinutes,
            TestModuleRunSettingNames.DefaultStaleRunThresholdMinutes));
        foreach (var momentCode in AgentMomentCodes.All)
        {
            context.Add(new SettingDefinition(
                PtnBridgeSettingNames.AllowedTools(momentCode),
                PtnBridgeSettingNames.DefaultAllowedTools(momentCode)));
            context.Add(new SettingDefinition(PtnBridgeSettingNames.MaxTurns(momentCode), "8"));
            context.Add(new SettingDefinition(PtnBridgeSettingNames.TokenLimit(momentCode), "12000"));
        }
        context.Add(new SettingDefinition(
            TestModuleSettings.SandboxResetStrategy,
            TestModuleRunSettingNames.DefaultSandboxResetStrategy));
        context.Add(new SettingDefinition(
            TestModuleSettings.RunConcurrencyWaitSeconds,
            TestModuleRunSettingNames.DefaultRunConcurrencyWaitSeconds));
        context.Add(new SettingDefinition(
            TestModuleSettings.QuarantineSweepPeriodSeconds,
            TestModuleCatalogSettingNames.DefaultQuarantineSweepPeriodSeconds));
        context.Add(new SettingDefinition(
            TestModuleSettings.QuarantineSweepBatchSize,
            TestModuleCatalogSettingNames.DefaultQuarantineSweepBatchSize));
        context.Add(new SettingDefinition(
            TestModuleSettings.ScenarioHealthRefreshPeriodSeconds,
            TestModuleRunSettingNames.DefaultScenarioHealthRefreshPeriodSeconds));
        context.Add(new SettingDefinition(
            TestModuleSettings.ScheduleSweepPeriodSeconds,
            TestModuleCatalogSettingNames.DefaultScheduleSweepPeriodSeconds));
        context.Add(new SettingDefinition(
            TestModuleSettings.MaxScenariosPerTick,
            TestModuleCatalogSettingNames.DefaultMaxScenariosPerTick));
        context.Add(new SettingDefinition(
            TestModuleSettings.AutomationEnvironmentKey,
            TestModuleRunSettingNames.DefaultAutomationEnvironmentKey));

        // Sir bos birakilir; webhook ucu ayar tanimlanmadan acilmaz ve deger hicbir yanitta veya logda gorunmez.
        context.Add(new SettingDefinition(
            TestModuleSettings.WebhookSecret,
            TestModuleRunSettingNames.DefaultWebhookSecret,
            isVisibleToClients: false,
            isEncrypted: true));
    }
}
