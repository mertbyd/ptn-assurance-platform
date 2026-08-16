using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace Ptn.DatabaseChecker.Managers;

// islevi: Domain servislerinde lazy dependency cozumleme altyapisini merkezi olarak tutar.
// sistemdeki gorevi: Manager siniflarinda constructor sisirmeden ABP servislerine kontrollu erisim saglar.
public abstract class DatabaseCheckerDomainService : DomainService
{
    private readonly IAbpLazyServiceProvider? _abpLazyServiceProvider;

    protected DatabaseCheckerDomainService()
    {
    }

    protected DatabaseCheckerDomainService(IAbpLazyServiceProvider abpLazyServiceProvider)
    {
        _abpLazyServiceProvider = abpLazyServiceProvider;
    }

    protected TService LazyGetRequiredService<TService>()
        where TService : notnull
    {
        return (_abpLazyServiceProvider ?? LazyServiceProvider).LazyGetRequiredService<TService>();
    }
}
