using Pintern.Authenticator;
using Pintern.Notifications;
using Ptn.TestModule.Localization;
using Volo.Abp.Domain;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace Ptn.TestModule;

// islevi: Test Module'un sabit, hata kodu ve localization sozlesmesini ABP'ye kaydeder.
// sistemdeki gorevi: Auth ve Notification sozlesmelerinin uzerine modulun kendi paylasilan dilini ekler.
[DependsOn(
    typeof(AbpValidationModule),
    typeof(AbpDddDomainSharedModule),
    typeof(AuthenticatorDomainSharedModule),
    typeof(NotificationsDomainSharedModule)
)]
public class TestModuleDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<TestModuleDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<TestModuleResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/TestModule");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("TestModule", typeof(TestModuleResource));
        });
    }
}
