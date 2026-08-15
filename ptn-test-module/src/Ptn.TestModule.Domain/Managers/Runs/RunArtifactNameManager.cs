using System;
using System.Globalization;
using System.Text;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Ihracat artefaktinin kararli blob adini uretir ve yazma oncesi format, ad ve boyut kapilarini isletir.
// sistemdeki gorevi: Ad turetme ile butce kararlarini blob sinirindan ayirip tek domain sahibinde tutar (PLAN-0003 TM-13).
/// <summary>
/// Kosum ihracat artefaktlarinin blob adini ve kabul kapilarini uretir.
/// </summary>
public class RunArtifactNameManager : TestModuleDomainService
{
    // Artefakti tenant, kosum, deneme ve format dosyasindan turetilen kararli blob adina baglar.
    /// <summary>Ihracat artefaktinin kalici depodaki blob adini uretir.</summary>
    public static string CreateBlobName(Guid? tenantId, Guid runId, int attempt, string formatCode)
    {
        EnsureAttemptIsValid(attempt);
        return string.Format(
            CultureInfo.InvariantCulture,
            RunArtifactConsts.BlobNameFormat,
            tenantId?.ToString("N") ?? HarArtifactConsts.HostTenantSegment,
            runId.ToString("N"),
            attempt.ToString(CultureInfo.InvariantCulture),
            ResolveFileName(formatCode));
    }

    // BLOB adini ve yazma isleminde ihracat icerigini ortak artefakt kapisindan gecirir.
    /// <summary>Blob adinin ve varsa icerigin kalici depoya uygun oldugunu dogrular.</summary>
    public void EnsureArtifactIsValid(string blobName, string? content = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);
        if (blobName.Length > RunArtifactConsts.MaxBlobNameLength)
        {
            throw new BusinessException(TestModuleRunErrorCodes.ArtifactBlobNameTooLong)
                .WithData(nameof(blobName), blobName.Length);
        }

        if (content is not null)
        {
            EnsureContentIsWithinBudget(content);
        }
    }

    // Kapali format kumesindeki kodu blob adinin dosya bolumune cevirir.
    /// <summary>Verilen format kodunun kararli blob dosya adini getirir.</summary>
    private static string ResolveFileName(string formatCode)
    {
        return formatCode switch
        {
            RunArtifactFormatCodes.Ctrf => RunArtifactConsts.FileNames.Ctrf,
            RunArtifactFormatCodes.JUnit => RunArtifactConsts.FileNames.JUnit,
            RunArtifactFormatCodes.Sarif => RunArtifactConsts.FileNames.Sarif,
            _ => throw new BusinessException(TestModuleRunErrorCodes.ArtifactFormatNotSupported)
                .WithData(nameof(formatCode), formatCode)
        };
    }

    // Deneme numarasinin blob adini benzersiz kilan bir tabanli degerde oldugunu dogrular.
    /// <summary>Artefakt adindaki deneme numarasinin bir tabanli oldugunu dogrular.</summary>
    private static void EnsureAttemptIsValid(int attempt)
    {
        if (attempt < 1)
        {
            throw new BusinessException(TestModuleRunErrorCodes.ArtifactAttemptInvalid)
                .WithData(nameof(attempt), attempt);
        }
    }

    // Bos icerigi ve depo butcesini asan ihracati kalici yazimdan once eler.
    /// <summary>Ihracat iceriginin var oldugunu ve boyut butcesine sigdigini dogrular.</summary>
    private static void EnsureContentIsWithinBudget(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        var byteCount = Encoding.UTF8.GetByteCount(content);
        if (byteCount > RunArtifactConsts.MaxArtifactBytes)
        {
            throw new BusinessException(TestModuleRunErrorCodes.ArtifactTooLarge)
                .WithData(nameof(byteCount), byteCount);
        }
    }
}
