using Ptn.ApiContractChecker.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker;

public abstract class ApiContractCheckerController : AbpControllerBase
{
    private readonly IAbpLazyServiceProvider? _abpLazyServiceProvider;

    protected ApiContractCheckerController()
    {
        LocalizationResource = typeof(ApiContractCheckerResource);
    }

    protected ApiContractCheckerController(IAbpLazyServiceProvider abpLazyServiceProvider)
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
