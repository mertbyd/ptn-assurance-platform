using Ptn.DatabaseChecker.Localization;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker;

// islevi: Database Checker AppService'lerine localization, Mapperly context ve lazy service cozumleme tabani verir.
// sistemdeki gorevi: Concrete use-case orkestratorlerinin ABP uygulama altyapisini tekrar kurmasini engeller.
public abstract class DatabaseCheckerAppService : ApplicationService
{
    private readonly IAbpLazyServiceProvider? _abpLazyServiceProvider;

    protected DatabaseCheckerAppService()
    {
        LocalizationResource = typeof(DatabaseCheckerResource);
        ObjectMapperContext = typeof(DatabaseCheckerApplicationModule);
    }

    protected DatabaseCheckerAppService(IAbpLazyServiceProvider abpLazyServiceProvider)
        : this()
    {
        _abpLazyServiceProvider = abpLazyServiceProvider;
    }

    protected TService LazyGetRequiredService<TService>()
        where TService : notnull
    {
        return (_abpLazyServiceProvider ?? LazyServiceProvider).LazyGetRequiredService<TService>();
    }
}
