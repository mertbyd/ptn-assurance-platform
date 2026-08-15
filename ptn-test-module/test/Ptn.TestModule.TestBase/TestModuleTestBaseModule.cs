using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Autofac;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.Data;
using Volo.Abp.Guids;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace Ptn.TestModule;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(AbpAuthorizationModule),
    typeof(AbpBlobStoringFileSystemModule),
    typeof(AbpGuidsModule)
)]
public class TestModuleTestBaseModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAlwaysAllowAuthorization();
        ConfigureBlobStoring();
    }

    /* Uretim hostu gibi testler de bir BLOB saglayicisi kaydetmek zorundadir; aksi halde
     * artefakt deposunu enjekte eden her servis kompozisyon aninda cozulemez. Her uygulama
     * ornegi kendi gecici kokunu alir, boylece paralel test kosumu birbirini gormez. */
    private void ConfigureBlobStoring()
    {
        var basePath = Path.Combine(Path.GetTempPath(), "ptn-test-module-blobs", Guid.NewGuid().ToString("N"));
        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers.ConfigureAll((_, container) =>
                container.UseFileSystem(fileSystem => fileSystem.BasePath = basePath));
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        SeedTestData(context);
    }

    private static void SeedTestData(ApplicationInitializationContext context)
    {
        AsyncHelper.RunSync(async () =>
        {
            using (var scope = context.ServiceProvider.CreateScope())
            {
                await scope.ServiceProvider
                    .GetRequiredService<IDataSeeder>()
                    .SeedAsync();
            }
        });
    }
}
