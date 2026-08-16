using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Interface.Formats;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Models.Snapshots;
using Ptn.ApiContractChecker.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Ptn.ApiContractChecker.Managers.Snapshots;

// islevi: Spec icerigini bir kez okuyup normalize eder ve secilen response semasini dialect bilesenine verir.
// sistemdeki gorevi: Kosum ani oracle'inin ref/allOf, cache ve format farklarini gormedigi tek sema siniridir.
public class SpecSchemaResolver : ISpecSchemaResolver, ITransientDependency
{
    private readonly IMemoryCache _cache;
    private readonly ISpecDocumentReader _reader;
    private readonly SpecSnapshotNormalizer _normalizer;
    private readonly ISpecFormatComponentResolver<ISpecSchemaDialectComponent> _dialectResolver;
    private readonly ISettingProvider _settingProvider;

    public SpecSchemaResolver(
        IMemoryCache cache,
        ISpecDocumentReader reader,
        SpecSnapshotNormalizer normalizer,
        ISpecFormatComponentResolver<ISpecSchemaDialectComponent> dialectResolver,
        ISettingProvider settingProvider)
    {
        _cache = cache;
        _reader = reader;
        _normalizer = normalizer;
        _dialectResolver = dialectResolver;
        _settingProvider = settingProvider;
    }

    public async Task<SpecSnapshotModel> GetSnapshotAsync(SpecContent content)
    {
        return (await GetParsedAsync(content)).Snapshot;
    }

    public async Task<ResolvedSpecSchemaModel?> ResolveAsync(
        SpecContent content,
        SpecOperationModel operation,
        string statusCode,
        string mediaType)
    {
        var parsed = await GetParsedAsync(content);
        var response = operation.Responses.SingleOrDefault(candidate =>
            candidate.StatusCode == statusCode &&
            string.Equals(candidate.MediaType, mediaType, StringComparison.OrdinalIgnoreCase));
        if (response?.Schema == null)
        {
            return null;
        }

        return await _dialectResolver.Resolve(parsed.FormatCode).BuildAsync(response.Schema);
    }

    public async Task<ResolvedSpecSchemaModel?> ResolveRequestAsync(
        SpecContent content,
        SpecOperationModel operation,
        string mediaType)
    {
        var parsed = await GetParsedAsync(content);
        var requestBody = operation.RequestBodies.SingleOrDefault(candidate =>
            string.Equals(candidate.MediaType, mediaType, StringComparison.OrdinalIgnoreCase));
        return requestBody?.Schema == null
            ? null
            : await _dialectResolver.Resolve(parsed.FormatCode).BuildAsync(requestBody.Schema);
    }

    // CanonicalHash disinda cache kimligi uretmez; ayni kanonik spec tek normalize sonucunu paylasir.
    private async Task<ParsedSpecModel> GetParsedAsync(SpecContent content)
    {
        var cached = await _cache.GetOrCreateAsync(content.CanonicalHash, async entry =>
        {
            entry.SlidingExpiration = await ResolveCacheDurationAsync();
            var parsed = await _reader.ReadAsync(Encoding.UTF8.GetBytes(content.Content));
            return new ParsedSpecModel(
                parsed.FormatCode,
                parsed.ApiVersion,
                parsed.CanonicalText,
                _normalizer.Normalize(parsed.Snapshot));
        });
        return cached!;
    }

    private async Task<TimeSpan> ResolveCacheDurationAsync()
    {
        var rawValue = await _settingProvider.GetOrNullAsync(
            ApiContractCheckerSettings.Conformance.SchemaCacheMinutes);
        return TimeSpan.FromMinutes(int.TryParse(rawValue, out var minutes) && minutes > 0
            ? minutes
            : ApiContractCheckerSettings.Conformance.DefaultSchemaCacheMinutes);
    }
}
