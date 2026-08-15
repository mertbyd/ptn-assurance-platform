using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Testing;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Services.Runs;

// islevi: ABP kilit implementasyonunun ayni anahtari siraya alip farkli anahtari serbest biraktigini dogrular.
// sistemdeki gorevi: RunConcurrencyManager anahtar kararinin gercek runtime kilit davranisiyla birlikte calistigini kanitlar.
public class RunConcurrencyLockTests
    : AbpIntegratedTest<RunConcurrencyLockTestModule>
{
    // Ilk handle birakilana kadar ayni ortam anahtarinin ikinci handle'i edinilememelidir.
    [Fact]
    public async Task Should_wait_for_the_same_environment_lock()
    {
        var distributedLock = ServiceProvider.GetRequiredService<IAbpDistributedLock>();
        var firstHandle = await distributedLock.TryAcquireAsync("TestModule.Runs.Environment:host:staging");
        firstHandle.ShouldNotBeNull();

        var secondHandleTask = distributedLock.TryAcquireAsync(
            "TestModule.Runs.Environment:host:staging",
            TimeSpan.FromSeconds(2));
        await Task.Delay(100);
        secondHandleTask.IsCompleted.ShouldBeFalse();

        await firstHandle.DisposeAsync();
        await using var secondHandle = await secondHandleTask;
        secondHandle.ShouldNotBeNull();
    }

    // Farkli ortam anahtari ilk ortam kilitliyken beklemeden edinilebilmelidir.
    [Fact]
    public async Task Should_not_block_a_different_environment_lock()
    {
        var distributedLock = ServiceProvider.GetRequiredService<IAbpDistributedLock>();
        await using var staging = await distributedLock.TryAcquireAsync(
            "TestModule.Runs.Environment:host:staging");

        await using var production = await distributedLock.TryAcquireAsync(
            "TestModule.Runs.Environment:host:production");

        staging.ShouldNotBeNull();
        production.ShouldNotBeNull();
    }
}
