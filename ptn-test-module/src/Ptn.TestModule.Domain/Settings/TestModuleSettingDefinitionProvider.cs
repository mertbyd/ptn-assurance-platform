using System.Globalization;
using Ptn.TestModule.Constants.Bridge;
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
        context.Add(new SettingDefinition(
            TestModuleSettings.HarRetentionDays,
            TestModuleRunSettingNames.DefaultHarRetentionDays));
        context.Add(new SettingDefinition(
            TestModuleSettings.StaleRunThresholdMinutes,
            TestModuleRunSettingNames.DefaultStaleRunThresholdMinutes));
    }
}
