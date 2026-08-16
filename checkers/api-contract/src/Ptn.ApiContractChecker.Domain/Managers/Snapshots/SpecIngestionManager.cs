using System.Security.Cryptography;
using System.Text;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.ExceptionCodes;
using Ptn.ApiContractChecker.Interface;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Managers.Shared;
using Ptn.ApiContractChecker.Models.Snapshots;
using Ptn.ApiContractChecker.Models.Sources;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Snapshots;

// islevi: Cekilen ham govdeyi ayristirip SpecContent ve SpecSnapshot kurallarini ve dedup kararini isletir.
// sistemdeki gorevi: Ince snapshot entity'leri icin kanonik kimlik, geriye gitmeyen gorulme zamani ve kalicilik akisinin tek sahibidir.
public class SpecIngestionManager : ApiContractCheckerDomainService
{
    // Ham bayti format, surum ve kanonik metne ceviren ayristirici sinir.
    private ISpecDocumentReader Reader => LazyGetRequiredService<ISpecDocumentReader>();

    // Ham hash ile degismez icerigi bulan icerik-adresli depo.
    private ISpecContentRepository ContentRepository => LazyGetRequiredService<ISpecContentRepository>();

    // Dokumanin son anlik goruntusunu veren zaman serisi deposu.
    private ISpecSnapshotRepository SnapshotRepository => LazyGetRequiredService<ISpecSnapshotRepository>();

    // Format kodunu lookup satirina cozen genel depo.
    private IBaseRepository<SpecFormat> FormatRepository => LazyGetRequiredService<IBaseRepository<SpecFormat>>();

    public SpecIngestionManager(IAbpLazyServiceProvider provider)
        : base(provider)
    {
    }

    // Govdeyi ayristirir ve ya son snapshot'i tazeler ya da yeni snapshot acar.
    public async Task<SpecSnapshot> IngestAsync(Guid specDocumentId, SpecFetchResultModel fetched)
    {
        EnsureNotEmpty(specDocumentId);
        var parsed = await Reader.ReadAsync(fetched.Content);
        var rawHash = ComputeHash(fetched.Content);
        var existingContent = await ContentRepository.FindByRawHashAsync(rawHash);
        var latestSnapshot = await SnapshotRepository.FindLatestForDocumentAsync(specDocumentId);

        if (IsUnchanged(latestSnapshot, existingContent))
        {
            return await MarkSeenAsync(latestSnapshot!);
        }

        var content = existingContent ?? await InsertContentAsync(rawHash, parsed, fetched);
        return await InsertSnapshotAsync(specDocumentId, content, parsed);
    }

    // Dogrulanmis alanlardan kanonik kimlikli degismez icerigi kurar.
    public SpecContent CreateContent(
        Guid id,
        string rawHash,
        string canonicalHash,
        string content,
        int byteSize,
        string mediaType,
        Guid? tenantId)
    {
        return new SpecContent(
            id,
            NormalizeHash(rawHash),
            NormalizeHash(canonicalHash),
            EnsureContentNotEmpty(content),
            EnsureByteSizeNotNegative(byteSize),
            NormalizeMediaType(mediaType),
            tenantId);
    }

    // Dogrulanmis FK ve kanonik surum alanlariyla yeni snapshot'i kurar.
    public SpecSnapshot CreateSnapshot(
        Guid id,
        Guid specDocumentId,
        Guid specContentId,
        Guid specFormatId,
        string? apiVersion,
        DateTime lastSeenAt,
        Guid? tenantId)
    {
        return new SpecSnapshot(
            id,
            EnsureNotEmpty(specDocumentId),
            EnsureNotEmpty(specContentId),
            EnsureNotEmpty(specFormatId),
            NormalizeOptional(apiVersion),
            lastSeenAt,
            tenantId);
    }

    // Ayni snapshot'in gorulme zamanini geriye gitmeyecek bicimde ilerletir.
    public void MarkSeen(SpecSnapshot snapshot, DateTime seenAt)
    {
        if (seenAt < snapshot.LastSeenAt)
        {
            throw new ArgumentOutOfRangeException(nameof(seenAt));
        }

        snapshot.LastSeenAt = seenAt;
    }

    // Son snapshot ayni icerigi gosteriyorsa yeni satir acilmayacagini belirler.
    private static bool IsUnchanged(SpecSnapshot? latestSnapshot, SpecContent? existingContent)
    {
        return latestSnapshot != null &&
               existingContent != null &&
               latestSnapshot.SpecContentId == existingContent.Id;
    }

    // Degismeyen icerigin yalniz son gorulme zamanini ilerletir.
    private async Task<SpecSnapshot> MarkSeenAsync(SpecSnapshot snapshot)
    {
        MarkSeen(snapshot, Clock.Now);
        return await SnapshotRepository.UpdateAsync(snapshot, autoSave: true);
    }

    // Ham baytlari ve kanonik metnin hash'ini tasiyan degismez icerigi yazar.
    private async Task<SpecContent> InsertContentAsync(
        string rawHash,
        ParsedSpecModel parsed,
        SpecFetchResultModel fetched)
    {
        var content = CreateContent(
            GuidGenerator.Create(),
            rawHash,
            ComputeHash(Encoding.UTF8.GetBytes(parsed.CanonicalText)),
            Encoding.UTF8.GetString(fetched.Content),
            fetched.ByteSize,
            fetched.MediaType,
            CurrentTenant.Id);

        return await ContentRepository.InsertAsync(content, autoSave: true);
    }

    // Dokumanin yeni anlik goruntusunu format ve surum bilgisiyle acar.
    private async Task<SpecSnapshot> InsertSnapshotAsync(
        Guid specDocumentId,
        SpecContent content,
        ParsedSpecModel parsed)
    {
        var snapshot = CreateSnapshot(
            GuidGenerator.Create(),
            specDocumentId,
            content.Id,
            await ResolveFormatIdAsync(parsed.FormatCode),
            parsed.ApiVersion,
            Clock.Now,
            CurrentTenant.Id);

        return await SnapshotRepository.InsertAsync(snapshot, autoSave: true);
    }

    // Kararli format kodunu lookup kimligine cevirir; karsiligi yoksa desteklenmeyen format hatasi verir.
    private async Task<Guid> ResolveFormatIdAsync(string formatCode)
    {
        var format = await FormatRepository.FindAsync(candidate => candidate.Code == formatCode);
        return format?.Id ?? throw new BusinessException(SpecFormatExceptionCodes.UnsupportedFormat);
    }

    // Ham baytlarin SHA-256 ozetini kalici kolon bicimiyle ayni lowercase hex olarak uretir.
    private static string ComputeHash(byte[] content)
    {
        return Convert.ToHexStringLower(SHA256.HashData(content));
    }

    // SHA-256 metnini sabit uzunlukta lowercase hex bicimine indirger.
    private static string NormalizeHash(string value)
    {
        var normalized = EntityCanonicalizer.NormalizeRequired(value).ToLowerInvariant();
        if (normalized.Length != SpecContentConsts.HashLength ||
            normalized.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException(null, nameof(value));
        }

        return normalized;
    }

    // Medya tipini karsilastirma ve saklama icin trimlenmis lowercase bicime indirger.
    private static string NormalizeMediaType(string value)
    {
        var normalized = EntityCanonicalizer.NormalizeRequired(value).ToLowerInvariant();
        return normalized.Length <= SpecContentConsts.MaxMediaTypeLength
            ? normalized
            : throw new ArgumentOutOfRangeException(nameof(value));
    }

    // Dis kaynaktan gelen istege bagli surumu trimler ve kalici kolon sinirinda tutar.
    private static string? NormalizeOptional(string? value)
    {
        var normalized = EntityCanonicalizer.NormalizeOptional(value);
        return normalized == null || normalized.Length <= SpecSnapshotConsts.MaxApiVersionLength
            ? normalized
            : throw new ArgumentOutOfRangeException(nameof(value));
    }

    // Zorunlu FK kimliginin bos Guid olmasini programci hatasi olarak reddeder.
    private static Guid EnsureNotEmpty(Guid value)
    {
        return value != Guid.Empty
            ? value
            : throw new ArgumentException(null, nameof(value));
    }

    // Ham icerigin bos veya yalniz whitespace olmasini programci hatasi olarak reddeder.
    private static string EnsureContentNotEmpty(string value)
    {
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException(null, nameof(value));
    }

    // Olculen icerik boyutunun negatif olmasini programci hatasi olarak reddeder.
    private static int EnsureByteSizeNotNegative(int value)
    {
        return value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value));
    }
}
