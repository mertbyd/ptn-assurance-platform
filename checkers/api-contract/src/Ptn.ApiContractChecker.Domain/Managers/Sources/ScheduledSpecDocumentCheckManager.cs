using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.Constants.Sources;
using Ptn.ApiContractChecker.ExceptionCodes.Sources;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Managers.Snapshots;
using Ptn.ApiContractChecker.Models.Sources;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Sources;

// islevi: Vadesi gelmis dokumanin vade ilerletme, cekim ve "icerik degisti mi" kararlarini isletir.
// sistemdeki gorevi: Zamanlanmis kontrolu elle snapshot alma yolundaki manager'lari yeniden kullanarak kisa UOW adimlarina boler; degisim sinyalini ayri bir mekanizma kurmadan dedup sonucundan okur.
public class ScheduledSpecDocumentCheckManager : ApiContractCheckerDomainService
{
    // Aggregate yukleme, aktif dokuman guard'i ve cekim istegi kurulumunun sahibi.
    private SpecSourceManager SourceManager => LazyGetRequiredService<SpecSourceManager>();

    // "Ayni icerik yeni satir acmaz" snapshot karar akisinin sahibi.
    private SpecIngestionManager IngestionManager => LazyGetRequiredService<SpecIngestionManager>();

    // Vade yazimi icin aggregate kokune erisir.
    private ISpecSourceRepository SourceRepository => LazyGetRequiredService<ISpecSourceRepository>();

    // Cekim oncesi son snapshot'i karsilastirmanin taban tarafi olarak belirler.
    private ISpecSnapshotRepository SnapshotRepository => LazyGetRequiredService<ISpecSnapshotRepository>();

    // Canli govdeyi guard'lariyla getiren cekim siniri.
    private ISpecFetcherClient FetcherClient => LazyGetRequiredService<ISpecFetcherClient>();

    public ScheduledSpecDocumentCheckManager(IAbpLazyServiceProvider provider)
        : base(provider)
    {
    }

    // islevi: Kisa UOW adimi; cekim baglamini kurar ve vadeyi cekimden ONCE ilerletir.
    // Sira bilincli: erisilemeyen servis exception firlattiginda vade zaten yazilmis olur, dokuman ne susar
    // ne de her tikte yeniden kuyruga girer. Vadeyi cekimden sonra yazan bir sira, try/catch olmadan bu
    // davranisi veremez ve ADR-0006'nin sahiplendigi hata sinirini job'a tasirdi.
    public async Task<ScheduledDocumentCheckContextModel> BeginAsync(Guid sourceId, Guid documentId)
    {
        var source = await SourceRepository.FindWithDetailsAsync(sourceId)
                     ?? throw new BusinessException(SpecSourceExceptionCodes.ActiveDocumentNotFound);
        var document = SourceManager.GetRequiredActiveDocument(source, documentId);
        var previousSnapshot = await SnapshotRepository.FindLatestForDocumentAsync(document.Id);

        SourceManager.MarkDocumentChecked(source, document.Id, Clock.Now);
        await SourceRepository.UpdateAsync(source, autoSave: true);

        return BuildContext(source, document, previousSnapshot?.Id);
    }

    // islevi: UOW disinda calisan uzun dis I/O adimi; canli spec govdesini ceker.
    public Task<SpecFetchResultModel> FetchAsync(ScheduledDocumentCheckContextModel context)
    {
        return FetcherClient.FetchAsync(context.FetchRequest);
    }

    // islevi: Kisa UOW adimi; dedup kararini isletir ve yeni satir acildiysa karsilastirilacak snapshot ciftini bildirir.
    // Ayri bir "degisti mi" mekanizmasi yoktur: yeni snapshot kimligi cekim oncesindeki son snapshot'tan
    // farkliysa icerik degismistir. Tekrar teslimde ayni govde yeni satir acmadigi icin cift de olusmaz.
    public async Task<ScheduledDocumentCheckPairModel?> IngestAsync(
        ScheduledDocumentCheckContextModel context,
        SpecFetchResultModel fetched)
    {
        var snapshot = await IngestionManager.IngestAsync(context.SpecDocumentId, fetched);
        var pair = BuildChangedPair(context.PreviousSnapshotId, snapshot.Id);
        await RecordFetchOutcomeAsync(
            context,
            pair is null ? SpecDocumentFetchOutcomeCodes.Unchanged : SpecDocumentFetchOutcomeCodes.Changed);
        return pair;
    }

    // Ulasilamayan cekimi, ABP job retry semantigini degistirmeden dokuman durumuna yazar.
    public Task RecordUnreachableAsync(ScheduledDocumentCheckContextModel context)
    {
        return RecordFetchOutcomeAsync(context, SpecDocumentFetchOutcomeCodes.Unreachable);
    }

    // Cekim istegini ve karsilastirma tabanini tek salt-okunur baglamda toplar.
    private static ScheduledDocumentCheckContextModel BuildContext(
        SpecSource source,
        SpecDocument document,
        Guid? previousSnapshotId)
    {
        return new ScheduledDocumentCheckContextModel
        {
            SpecSourceId = source.Id,
            SpecDocumentId = document.Id,
            FetchRequest = new SpecFetchRequestModel(source.BaseUrl, document.Path, source.VaultSecretPath),
            PreviousSnapshotId = previousSnapshotId
        };
    }

    // Yeni snapshot gercek bir onceki snapshot'tan farkliysa mevcut check tetikleme ciftini kurar.
    private static ScheduledDocumentCheckPairModel? BuildChangedPair(Guid? previousSnapshotId, Guid snapshotId)
    {
        if (previousSnapshotId is not { } baseSnapshotId || snapshotId == baseSnapshotId)
        {
            return null;
        }

        return new ScheduledDocumentCheckPairModel
        {
            BaseSnapshotId = baseSnapshotId,
            TargetSnapshotId = snapshotId
        };
    }

    // Cekim sonucunu ayni kisa ingest UOW'unda SpecSource aggregate'i uzerinden kalicilastirir.
    private async Task RecordFetchOutcomeAsync(ScheduledDocumentCheckContextModel context, string outcome)
    {
        var source = await SourceRepository.FindWithDetailsAsync(context.SpecSourceId)
                     ?? throw new BusinessException(SpecSourceExceptionCodes.ActiveDocumentNotFound);
        SourceManager.RecordDocumentFetchOutcome(source, context.SpecDocumentId, outcome);
        await SourceRepository.UpdateAsync(source, autoSave: true);
    }
}
