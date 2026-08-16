using Ptn.ApiContractChecker.Interface.Runs;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.ExceptionCodes.Runs;
using Ptn.ApiContractChecker.Constants.Runs.Lookups;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Runs;

// islevi: Bir run'in fingerprintlerini ayni tanimin onceki tamamlanmis kosusuyla kume olarak siniflandirir.
// sistemdeki gorevi: New, Known, Resolved ve Unknown kararlarini repository projeksiyonundan ayirir.
public sealed class FindingChangeStateManager : ITransientDependency
{
    private readonly IContractCheckRunRepository _repository;

    public FindingChangeStateManager(IContractCheckRunRepository repository)
    {
        _repository = repository;
    }

    // 0.2.0-alpha.1 cagiranlari icin otomatik onceki-run siniflandirma imzasini korur.
    public Task<FindingChangeClassificationModel> ClassifyAsync(Guid runId)
        => ClassifyAsync(runId, null);

    // islevi: Mevcut ve onceki fingerprint projeksiyonlarindan degisim siniflandirmasini kurar.
    public async Task<FindingChangeClassificationModel> ClassifyAsync(
        Guid runId,
        Guid? referenceRunId)
    {
        var current = await _repository.GetFindingFingerprintsAsync(runId);
        var previous = referenceRunId.HasValue
            ? await GetRequiredReferenceFingerprintsAsync(runId, referenceRunId.Value)
            : await _repository.FindPreviousCompletedFindingFingerprintsAsync(runId);
        return Classify(current, previous);
    }

    // islevi: Explicit fingerprint kumesini change-state secimiyle kesistirip tek repository filtresi kurar.
    public FindingFingerprintSelectionModel? BuildSelection(
        FindingChangeClassificationModel classification,
        string? changeStateCode,
        Guid? referenceRunId,
        IReadOnlyCollection<string> fingerprints)
    {
        var effectiveChangeState = changeStateCode ??
                                   (referenceRunId.HasValue ? FindingChangeStateCodes.New : null);
        return IntersectSelection(classification.Select(effectiveChangeState), fingerprints);
    }

    // islevi: Opsiyonel change-state secimini explicit SHA-256 kumesiyle ordinal olarak kesistirir.
    private static FindingFingerprintSelectionModel? IntersectSelection(
        FindingFingerprintSelectionModel? selection,
        IReadOnlyCollection<string> fingerprints)
    {
        if (fingerprints.Count == 0)
        {
            return selection;
        }

        var requested = fingerprints
            .Select(fingerprint => fingerprint.ToUpperInvariant())
            .ToHashSet(StringComparer.Ordinal);
        if (selection is null)
        {
            return new FindingFingerprintSelectionModel { Fingerprints = requested };
        }

        return new FindingFingerprintSelectionModel
        {
            Fingerprints = selection.IncludeMissingFingerprint
                ? []
                : selection.Fingerprints.Where(requested.Contains).ToArray()
        };
    }

    // islevi: Nullable fingerprint kumelerini I/O olmadan New, Known ve Resolved olarak ayirir.
    public FindingChangeClassificationModel Classify(
        IReadOnlyCollection<string?> current,
        IReadOnlyCollection<string?>? previous)
    {
        var result = new FindingChangeClassificationModel { HasPreviousRun = previous is not null };
        var currentSet = ToKnownSet(current);
        var previousSet = previous is null ? [] : ToKnownSet(previous);

        foreach (var fingerprint in currentSet)
        {
            (previousSet.Contains(fingerprint) ? result.KnownFingerprints : result.NewFingerprints)
                .Add(fingerprint);
        }

        foreach (var fingerprint in previousSet)
        {
            if (!currentSet.Contains(fingerprint))
            {
                result.ResolvedFingerprints.Add(fingerprint);
            }
        }

        return result;
    }

    // islevi: Repository sayfasini hesaplanmis change state kodlariyla cikti butcesine hazirlar.
    public FindingPageResultModel BuildPage(
        IReadOnlyCollection<FindingReadModel> findings,
        FindingChangeClassificationModel classification,
        long totalCount,
        int requestedMaxResultCount,
        int effectiveMaxResultCount)
    {
        var result = new FindingPageResultModel
        {
            TotalCount = totalCount,
            RequestedMaxResultCount = requestedMaxResultCount,
            EffectiveMaxResultCount = effectiveMaxResultCount,
            IsTruncated = effectiveMaxResultCount < requestedMaxResultCount
        };
        foreach (var finding in findings)
        {
            finding.ChangeStateCode = classification.Classify(finding.Fingerprint);
            result.Items.Add(finding);
        }

        return result;
    }

    // islevi: Eski null fingerprintleri kume islemlerinden dislayip kararli ordinal kume kurar.
    private static HashSet<string> ToKnownSet(IReadOnlyCollection<string?> fingerprints)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var fingerprint in fingerprints)
        {
            if (fingerprint is not null)
            {
                result.Add(fingerprint);
            }
        }

        return result;
    }

    // islevi: Explicit referansin gorulebilir, tamamlanmis, daha eski ve ayni dokuman cifti oldugunu repository kanitiyla zorunlu kilar.
    private async Task<List<string?>> GetRequiredReferenceFingerprintsAsync(
        Guid runId,
        Guid referenceRunId)
    {
        var fingerprints = await _repository.FindCompletedReferenceFindingFingerprintsAsync(
            runId, referenceRunId);
        return fingerprints ?? throw new BusinessException(
            ContractCheckRunExceptionCodes.InvalidFindingReferenceRun);
    }
}
