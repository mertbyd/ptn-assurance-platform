using Ptn.ApiContractChecker.Localization;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker;

public abstract class ApiContractCheckerAppService : ApplicationService
{
    private readonly IAbpLazyServiceProvider? _abpLazyServiceProvider;

    protected ApiContractCheckerAppService()
    {
        LocalizationResource = typeof(ApiContractCheckerResource);
        ObjectMapperContext = typeof(ApiContractCheckerApplicationModule);
    }

    protected ApiContractCheckerAppService(IAbpLazyServiceProvider abpLazyServiceProvider)
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
