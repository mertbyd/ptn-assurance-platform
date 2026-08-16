using Localization.Resources.AbpUi;
using Ptn.ApiContractChecker.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Ptn.ApiContractChecker;

[DependsOn(
    typeof(ApiContractCheckerApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule))]
public class ApiContractCheckerHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(ApiContractCheckerHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<ApiContractCheckerResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
