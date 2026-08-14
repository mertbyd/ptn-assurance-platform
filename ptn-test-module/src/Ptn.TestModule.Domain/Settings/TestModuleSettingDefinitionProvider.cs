using Ptn.TestModule.Constants.Bridge;
using Volo.Abp.Settings;

namespace Ptn.TestModule.Settings;

// islevi: Test Module setting kataloguna kopru profil paketi yolunu kaydeder.
// sistemdeki gorevi: Profil saglayicisinin ortama gore ezilebilir ama kararli bir dosya kokunden okumasini saglar.
public class TestModuleSettingDefinitionProvider : SettingDefinitionProvider
{
    // Kopru profil paketlerinin varsayilan dosya kokunu ABP setting sistemine tanimlar.
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(new SettingDefinition(
            TestModuleSettings.ProfilePackPath,
            PtnBridgeSettingNames.DefaultProfilePackPath));
    }
}
