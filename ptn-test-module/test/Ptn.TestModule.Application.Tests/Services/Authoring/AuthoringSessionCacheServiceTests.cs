using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Ptn.TestModule.Constants.Authoring;
using Ptn.TestModule.Models.Authoring;
using Ptn.TestModule.Services.Authoring;
using Shouldly;
using Volo.Abp.Caching;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Services.Authoring;

// islevi: Authoring cache adapterinin mutlak TTL ve cache-miss davranisini dogrular.
// sistemdeki gorevi: Gecici oturumlarin tabloya donusmesini veya suresiz yasamasini engeller.
public class AuthoringSessionCacheServiceTests
{
    // Session yazimini task sozlesmesindeki mutlak TTL ile ABP cache'e iletir.
    [Fact]
    public async Task Should_write_with_the_declared_absolute_ttl()
    {
        var cache = Substitute.For<IDistributedCache<AuthoringSession, Guid>>();
        var service = new AuthoringSessionCacheService(cache);
        var session = new AuthoringSession { Id = Guid.NewGuid() };

        await service.SetAsync(session, CancellationToken.None);

        await cache.Received(1).SetAsync(
            session.Id,
            session,
            Arg.Is<DistributedCacheEntryOptions>(options =>
                options.AbsoluteExpirationRelativeToNow ==
                TimeSpan.FromMinutes(AuthoringSessionConsts.TtlMinutes)),
            token: CancellationToken.None);
    }

    // TTL sonrasi ABP cache miss sonucunu yeni session uydurmadan null olarak korur.
    [Fact]
    public async Task Should_return_null_for_an_expired_cache_entry()
    {
        var cache = Substitute.For<IDistributedCache<AuthoringSession, Guid>>();
        var id = Guid.NewGuid();
        cache.GetAsync(id, token: CancellationToken.None)
            .Returns(Task.FromResult<AuthoringSession?>(null));

        var result = await new AuthoringSessionCacheService(cache)
            .GetAsync(id, CancellationToken.None);

        result.ShouldBeNull();
    }
}
