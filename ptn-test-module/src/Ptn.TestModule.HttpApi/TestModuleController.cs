using Ptn.TestModule.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.DependencyInjection;

namespace Ptn.TestModule;

// islevi: Test Module controller'larinin localization ve lazy servis cozumleme tabanini tanimlar.
// sistemdeki gorevi: Controller'larin transport disi ortak mekanigi yeniden kurmasini engeller.
public abstract class TestModuleController : AbpControllerBase
{
    private readonly IAbpLazyServiceProvider? _abpLazyServiceProvider;

    protected TestModuleController()
    {
        LocalizationResource = typeof(TestModuleResource);
    }

    protected TestModuleController(IAbpLazyServiceProvider abpLazyServiceProvider)
        : this()
    {
        _abpLazyServiceProvider = abpLazyServiceProvider;
    }

    // Verilen servisi once ctor'dan gelen provider, yoksa ABP'nin lazy provider'i uzerinden cozer.
    protected TService LazyGetRequiredService<TService>()
        where TService : notnull
    {
        return (_abpLazyServiceProvider ?? LazyServiceProvider).LazyGetRequiredService<TService>();
    }
}
