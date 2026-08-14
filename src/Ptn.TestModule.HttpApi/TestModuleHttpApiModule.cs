using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using Ptn.TestModule.Localization;
using SystemStandards.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace Ptn.TestModule;

// islevi: Test Module HTTP controller yuzeyini ve ortak Result sozlesmesini MVC'ye kaydeder.
// sistemdeki gorevi: Ince controller'lari application part olarak hosta tasir; auth uclarini tasimaz.
[DependsOn(
    typeof(TestModuleApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(SystemStandardsAbpModule)
)]
public class TestModuleHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(TestModuleHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<TestModuleResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
