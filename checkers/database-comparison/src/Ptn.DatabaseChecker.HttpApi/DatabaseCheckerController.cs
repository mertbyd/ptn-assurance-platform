using Ptn.DatabaseChecker.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker;

// islevi: Database Checker HTTP controller'larina localization ve lazy service cozumleme tabani verir.
// sistemdeki gorevi: Concrete controller'larin ABP transport altyapisini tekrar kurmasini engeller.
/// <summary>
/// Database Checker HTTP API controller tabani.
/// </summary>
public abstract class DatabaseCheckerController : AbpControllerBase
{
    private readonly IAbpLazyServiceProvider? _abpLazyServiceProvider;

    protected DatabaseCheckerController()
    {
        LocalizationResource = typeof(DatabaseCheckerResource);
    }

    protected DatabaseCheckerController(IAbpLazyServiceProvider abpLazyServiceProvider)
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
