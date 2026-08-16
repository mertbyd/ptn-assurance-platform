using System.Text.Json;
using System.Threading;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Interface.Runs;
using Ptn.DatabaseChecker.Models.Runs;
using Ptn.DatabaseChecker.ExceptionCodes;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace Ptn.DatabaseChecker.Managers.Runs;

// islevi: Bulgu sayfa boyutunu setting tavanina kirpar ve UTF-8 cikti butcesine sigdirir.
// sistemdeki gorevi: Repository'nin sunucu tarafli JSON filtre/projeksiyonunu is limitleriyle orkestre eden MCP okuma cekirdegidir.
/// <summary>
/// Bulgu sorgusunu tenant-aware sayfa ve UTF-8 cevap limitleriyle orkestre eder.
/// </summary>
public class FindingQueryManager : DomainService
{
    private readonly IComparisonRunRepository _repository;
    private readonly FindingQuerySettingsResolver _settingsResolver;

    /// <summary>
    /// Manager'i bulgu repository'si ve tenant-aware limit resolver'i ile kurar.
    /// </summary>
    public FindingQueryManager(
        IComparisonRunRepository repository,
        FindingQuerySettingsResolver settingsResolver)
    {
        _repository = repository;
        _settingsResolver = settingsResolver;
    }

    /// <summary>
    /// Filtreli bulgu toplam sayisini ve butceye sigan tek sayfayi dondurur.
    /// </summary>
    public async Task<FindingPageModel> GetFindingsAsync(
        Guid id,
        FindingQueryModel input,
        CancellationToken cancellationToken = default)
    {
        await EnsureReferenceRunAsync(id, input.SinceRunId, cancellationToken);
        NormalizeFingerprints(input);
        var settings = await _settingsResolver.ResolveAsync();
        input.SkipCount = Math.Max(input.SkipCount, 0);
        input.MaxResultCount = ResolvePageSize(input.MaxResultCount, settings);
        var page = await _repository.GetFindingsAsync(id, input, cancellationToken);
        var itemBudget = Math.Max(
            0,
            settings.MaxResponseBytes - ComparisonRunConsts.FindingResponseEnvelopeReserveBytes);
        TrimToBudget(page, itemBudget);
        return page;
    }

    // islevi: SinceRunId'nin ayni gorunurluk/definition icindeki daha eski Completed run oldugunu dogrular.
    private async Task EnsureReferenceRunAsync(
        Guid currentRunId,
        Guid? referenceRunId,
        CancellationToken cancellationToken)
    {
        if (!referenceRunId.HasValue)
        {
            return;
        }

        if (!await _repository.IsValidCompletedReferenceRunAsync(
                currentRunId, referenceRunId.Value, cancellationToken))
        {
            throw new BusinessException(ComparisonRunExceptionCodes.InvalidFindingReferenceRun);
        }
    }

    // islevi: Gecerli hexadecimal fingerprintleri repository IN filtresi icin buyuk harfe kanoniklestirir.
    private static void NormalizeFingerprints(FindingQueryModel input)
    {
        for (var index = 0; index < input.Fingerprints.Count; index++)
        {
            input.Fingerprints[index] = input.Fingerprints[index].ToUpperInvariant();
        }
    }

    // islevi: Sifir/negatif sayfa boyutunu varsayilana, buyuk degeri ayar tavanina kirpar.
    private static int ResolvePageSize(int requested, FindingQuerySettings settings)
        => requested <= 0
            ? settings.DefaultPageSize
            : Math.Min(requested, settings.MaxPageSize);

    // islevi: Sayfa UTF-8 butceyi asarken sondaki bulgulari cikartir; toplam sayiyi korur.
    private static void TrimToBudget(FindingPageModel page, int maxResponseBytes)
    {
        while (MeasureUtf8Bytes(page) > maxResponseBytes && page.Items.Count > 0)
        {
            page.Items.RemoveAt(page.Items.Count - 1);
        }
    }

    // islevi: Dondurulecek sayfali modelin gercek UTF-8 JSON boyutunu olcer.
    private static int MeasureUtf8Bytes(FindingPageModel page)
        => JsonSerializer.SerializeToUtf8Bytes(page).Length;
}
