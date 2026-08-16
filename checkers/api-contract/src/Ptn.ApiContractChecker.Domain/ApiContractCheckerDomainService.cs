using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace Ptn.ApiContractChecker.Managers;

// islevi: Domain servislerinde lazy dependency cozumleme altyapisini merkezi olarak tutar.
// sistemdeki gorevi: Manager siniflarinda constructor sisirmeden ABP servislerine kontrollu erisim saglar.
public abstract class ApiContractCheckerDomainService : DomainService
{
    private readonly IAbpLazyServiceProvider? _abpLazyServiceProvider;

    protected ApiContractCheckerDomainService()
    {
    }

    protected ApiContractCheckerDomainService(IAbpLazyServiceProvider abpLazyServiceProvider)
    {
        _abpLazyServiceProvider = abpLazyServiceProvider;
    }

    protected TService LazyGetRequiredService<TService>()
        where TService : notnull
    {
        return (_abpLazyServiceProvider ?? LazyServiceProvider).LazyGetRequiredService<TService>();
    }
}
