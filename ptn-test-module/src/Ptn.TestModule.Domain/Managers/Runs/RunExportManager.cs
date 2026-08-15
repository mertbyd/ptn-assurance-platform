using System;
using System.Collections.Generic;
using System.Linq;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Ihracat kabul kapisini isletir, her formatin adini ve govdesini uretir, satira yazilacak bag kumesini kurar.
// sistemdeki gorevi: Format sahiplerini tek deterministik ihracat akisinda toplayan Manager'dir (PLAN-0003 TM-14/TM-30).
/// <summary>
/// Bir kosumun tum ihracat artefaktlarini deterministik olarak uretir.
/// </summary>
public class RunExportManager : TestModuleDomainService
{
    /// <summary>CTRF belgesini ureten format sahibidir.</summary>
    private readonly CtrfReportManager _ctrfReportManager;

    /// <summary>JUnit belgesini ureten format sahibidir.</summary>
    private readonly JUnitReportManager _jUnitReportManager;

    // Ihracati format sahiplerine baglar.
    /// <summary>Ihracat manager'ini format sahipleriyle kurar.</summary>
    public RunExportManager(CtrfReportManager ctrfReportManager, JUnitReportManager jUnitReportManager)
    {
        _ctrfReportManager = ctrfReportManager;
        _jUnitReportManager = jUnitReportManager;
    }

    // Ihracat girdisi bulunamazsa kosum kimligiyle kararli not-found firlatir.
    /// <summary>Ihracat girdisinin var oldugunu dogrular.</summary>
    public RunExportSource EnsureExportable(RunExportSource? source, Guid runId)
    {
        if (source is null)
        {
            throw new EntityNotFoundException(typeof(TestRun), runId);
        }

        if (source.Attempts.Count == 0)
        {
            throw new BusinessException(TestModuleRunErrorCodes.ExportRequiresTerminalResult)
                .WithData(nameof(runId), runId);
        }

        return source;
    }

    // Her format icin kararli adi ve deterministik govdeyi tek geciste uretir.
    /// <summary>Kosumun tum ihracat artefaktlarini uretir.</summary>
    public IReadOnlyList<RunExportArtifact> CreateArtifacts(RunExportSource source, int attempt)
    {
        ArgumentNullException.ThrowIfNull(source);
        return
        [
            CreateArtifact(source, attempt, RunArtifactFormatCodes.Ctrf, _ctrfReportManager.Create(source)),
            CreateArtifact(source, attempt, RunArtifactFormatCodes.JUnit, _jUnitReportManager.Create(source))
        ];
    }

    // Uretilmis artefaktlari satirda tutulan uc bag kolonuna esler.
    /// <summary>Ihracat artefaktlarini terminal satirinin bag kumesine cevirir.</summary>
    public static RunArtifactLinks ToLinks(IReadOnlyList<RunExportArtifact> artifacts)
    {
        ArgumentNullException.ThrowIfNull(artifacts);
        return new RunArtifactLinks
        {
            CtrfBlobName = FindBlobName(artifacts, RunArtifactFormatCodes.Ctrf),
            JUnitBlobName = FindBlobName(artifacts, RunArtifactFormatCodes.JUnit),
            SarifBlobName = FindBlobName(artifacts, RunArtifactFormatCodes.Sarif)
        };
    }

    // Tek formatin adini ve govdesini kalici yazima hazir artefakta baglar.
    /// <summary>Verilen format icin ihracat artefaktini kurar.</summary>
    private static RunExportArtifact CreateArtifact(
        RunExportSource source,
        int attempt,
        string formatCode,
        string content)
    {
        return new RunExportArtifact
        {
            FormatCode = formatCode,
            BlobName = RunArtifactNameManager.CreateBlobName(
                source.Run.TenantId,
                source.Run.Id,
                attempt,
                formatCode),
            Content = content
        };
    }

    // Istenen formatin blob adini uretilmis artefaktlar arasinda arar.
    /// <summary>Verilen formatin blob adini getirir; uretilmemisse null doner.</summary>
    private static string? FindBlobName(IReadOnlyList<RunExportArtifact> artifacts, string formatCode)
    {
        return artifacts.FirstOrDefault(artifact => artifact.FormatCode == formatCode)?.BlobName;
    }
}
