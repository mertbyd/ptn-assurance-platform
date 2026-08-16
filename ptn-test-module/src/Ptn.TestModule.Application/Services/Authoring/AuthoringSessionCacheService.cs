using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Ptn.TestModule.Constants.Authoring;
using Ptn.TestModule.Interface.Authoring;
using Ptn.TestModule.Models.Authoring;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace Ptn.TestModule.Services.Authoring;

// islevi: Authoring session okumasi ve TTL'li yazimini ABP distributed cache'e uygular.
// sistemdeki gorevi: Cache I/O'sunu AppService ve Manager kararlarindan tek yerde ayirir.
public sealed class AuthoringSessionCacheService : IAuthoringSessionStore, ITransientDependency
{
    private readonly IDistributedCache<AuthoringSession, Guid> _cache;

    // Tenant-aware tipli ABP cache yuzeyini authoring storage sinirina baglar.
    public AuthoringSessionCacheService(IDistributedCache<AuthoringSession, Guid> cache)
    {
        _cache = cache;
    }

    // Tenant anahtari altindaki session'i okur; TTL dolmussa null doner.
    public Task<AuthoringSession?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        _cache.GetAsync(id, token: cancellationToken);

    // Session'i task sozlesmesiyle ayni bildirilen TTL boyunca cache'te tutar.
    public Task SetAsync(AuthoringSession session, CancellationToken cancellationToken) =>
        _cache.SetAsync(
            session.Id,
            session,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(AuthoringSessionConsts.TtlMinutes)
            },
            token: cancellationToken);
}
