using Localization.Resources.AbpUi;
using Ptn.DatabaseChecker.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Ptn.DatabaseChecker;

[DependsOn(
    typeof(DatabaseCheckerApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule))]
public class DatabaseCheckerHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(DatabaseCheckerHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<DatabaseCheckerResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
