using Microsoft.Extensions.Caching.Memory;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Models.Snapshots;
using Ptn.ApiContractChecker.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Users;

namespace Ptn.ApiContractChecker.Managers.Snapshots;

// islevi: Kirpilmis yazarlik ozetlerinin tam halini kisa omurlu ve cagiriciya bagli resultRef ile saklar.
// sistemdeki gorevi: Ilk istegi yeniden calistirmadan acik ikinci cagriyla tam ozeti geri verir.
public sealed class SnapshotAuthoringResultStore : ITransientDependency
{
    private readonly IMemoryCache _cache;
    private readonly ICurrentTenant _tenant;
    private readonly ICurrentUser _user;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ISettingProvider _settings;

    public SnapshotAuthoringResultStore(
        IMemoryCache cache,
        ICurrentTenant tenant,
        ICurrentUser user,
        IGuidGenerator guidGenerator,
        ISettingProvider settings)
    {
        _cache = cache;
        _tenant = tenant;
        _user = user;
        _guidGenerator = guidGenerator;
        _settings = settings;
    }

    // islevi: Tam ozeti mevcut tenant ve kullaniciya baglayip TTL sonunda dusen handle dondurur.
    public async Task<string> SaveAsync(SnapshotAuthoringResultEnvelope result)
    {
        var resultRef = _guidGenerator.Create().ToString("N");
        var minutes = await ResolveLifetimeMinutesAsync();
        _cache.Set(BuildKey(resultRef), new Entry(_tenant.Id, _user.Id, result), TimeSpan.FromMinutes(minutes));
        return resultRef;
    }

    // islevi: Handle'i her cagrida tenant ve kullanici bagina gore yeniden yetkilendirir.
    public SnapshotAuthoringResultEnvelope? Find(string resultRef)
    {
        if (!_cache.TryGetValue<Entry>(BuildKey(resultRef), out var entry) || entry is null)
        {
            return null;
        }

        return entry.TenantId == _tenant.Id && entry.UserId == _user.Id ? entry.Result : null;
    }

    // islevi: ResultRef omrunu setting zincirinden pozitif dakika olarak cozer.
    private async Task<int> ResolveLifetimeMinutesAsync()
    {
        var raw = await _settings.GetOrNullAsync(ApiContractCheckerSettings.Snapshots.ResultReferenceMinutes);
        return int.TryParse(raw, out var minutes) && minutes > 0
            ? minutes
            : SnapshotAuthoringConstants.DefaultResultReferenceMinutes;
    }

    // islevi: ResultRef'i diger cache ailelerinden ayiran kararli anahtari kurar.
    private static string BuildKey(string resultRef) => SnapshotAuthoringConstants.ResultCachePrefix + resultRef;

    private sealed record Entry(Guid? TenantId, Guid? UserId, SnapshotAuthoringResultEnvelope Result);
}
