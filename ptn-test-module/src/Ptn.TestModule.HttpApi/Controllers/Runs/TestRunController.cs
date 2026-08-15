using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Runs;
using SystemStandards.Results;
using Volo.Abp.Application.Dtos;

namespace Ptn.TestModule.Controllers.Runs;

// islevi: Test kosumu okuma, olusturma, claim ve terminal yazma use-case'lerini HTTP'ye acar.
// sistemdeki gorevi: Route, binding ve operation permission'ini tasiyip her istegi tek AppService cagrisina yonlendirir.
/// <summary>Test kosumu HTTP islemlerini sunar.</summary>
[Route(TestRunRoutes.Root)]
[ApiExplorerSettings(GroupName = TestRunRoutes.SwaggerGroupName)]
public class TestRunController : TestModuleController
{
    /// <summary>Test kosumu use-case AppService'ini lazy cozer.</summary>
    private ITestRunAppService AppService => LazyGetRequiredService<ITestRunAppService>();

    /// <summary>Kosum ihracat use-case AppService'ini lazy cozer.</summary>
    private IRunReportExportService ExportService => LazyGetRequiredService<IRunReportExportService>();

    /// <summary>Kimligi verilen test kosumunu getirir.</summary>
    /// <param name="id">Okunacak TestRun aggregate kimligi.</param>
    /// <returns>Ev standardi icinde guncel kosum gorunumu.</returns>
    [HttpGet(TestRunRoutes.ById)]
    [Authorize(TestModulePermissions.Runs.View)]
    public virtual async Task<Result<TestRunDto>> Get(Guid id)
    {
        var result = await AppService.GetAsync(id);
        return result;
    }

    /// <summary>Test kosumlarini kararli sayfalama ile getirir.</summary>
    /// <param name="input">Sayfalama girdisi.</param>
    /// <returns>Ev standardi icinde agir rapor kolonlari tasimayan kosum sayfasi.</returns>
    [HttpGet]
    [Authorize(TestModulePermissions.Runs.View)]
    public virtual async Task<Result<PagedResultDto<TestRunHeaderDto>>> GetList(
        [FromQuery] TestRunListInput input)
    {
        var result = await AppService.GetListAsync(input);
        return result;
    }

    [HttpPost(TestRunRoutes.Cancel)]
    [Authorize(TestModulePermissions.Runs.Cancel)]
    public virtual async Task<Result> Cancel(Guid id)
    {
        await AppService.CancelAsync(id);
        return Result.NoContent();
    }

    [HttpGet(TestRunRoutes.ResultArtifactContent)]
    [Authorize(TestModulePermissions.Runs.View)]
    public virtual async Task<Result<RunArtifactContentDto>> GetArtifactContent(Guid id, string format)
    {
        var result = await AppService.GetArtifactContentAsync(id, format);
        return result;
    }

    [HttpGet(TestRunRoutes.HarContent)]
    [Authorize(TestModulePermissions.Runs.View)]
    public virtual async Task<Result<RunArtifactContentDto>> GetHarContent(Guid id)
    {
        var result = await AppService.GetHarContentAsync(id);
        return result;
    }

    /// <summary>Kosumu terminal hukmu, bulgulari ve teshis raporuyla birlikte getirir.</summary>
    /// <param name="id">Raporu okunacak TestRun aggregate kimligi.</param>
    /// <returns>Ev standardi icinde bulgulu ve teshisli kosum raporu.</returns>
    [HttpGet(TestRunRoutes.ReportById)]
    [Authorize(TestModulePermissions.Runs.View)]
    public virtual async Task<Result<TestReportDetailDto>> GetReport(Guid id)
    {
        var result = await AppService.GetReportAsync(id);
        return result;
    }

    /// <summary>Kuru kosumun gozlem-sozlesme celiskisini yonlendirme vermeden getirir.</summary>
    [HttpGet(TestRunRoutes.DryRunContradictionById)]
    [Authorize(TestModulePermissions.Runs.View)]
    public virtual async Task<Result<DryRunContradictionReportDto>> GetDryRunContradiction(Guid id)
    {
        var result = await AppService.GetDryRunContradictionAsync(id);
        return result;
    }

    /// <summary>Kosumu CTRF, JUnit ve SARIF formatlarina ihrac edip artefakt baglarini getirir.</summary>
    /// <param name="id">Ihrac edilecek TestRun aggregate kimligi.</param>
    /// <returns>Ev standardi icinde uretilen ihracat artefaktlarinin resource_link gorunumu.</returns>
    [HttpPost(TestRunRoutes.Export)]
    [Authorize(TestModulePermissions.Runs.Export)]
    public virtual async Task<Result<RunArtifactLinksDto>> Export(Guid id)
    {
        var result = await ExportService.ExportAsync(id);
        return result;
    }

    /// <summary>Terminal sonucun ihracat artefaktlarina isaret eden baglarini getirir.</summary>
    /// <param name="id">Baglari okunacak TestRunResult aggregate kimligi.</param>
    /// <returns>Ev standardi icinde uc ihracat formatinin resource_link gorunumu.</returns>
    [HttpGet(TestRunRoutes.ResultArtifactsById)]
    [Authorize(TestModulePermissions.Runs.View)]
    public virtual async Task<Result<RunArtifactLinksDto>> GetArtifactLinks(Guid id)
    {
        var result = await AppService.GetArtifactLinksAsync(id);
        return result;
    }

    /// <summary>Kimligi verilen terminal sonucu tum bulgulariyla getirir.</summary>
    /// <param name="id">Okunacak TestRunResult aggregate kimligi.</param>
    /// <returns>Ev standardi icinde bulgulu terminal sonuc.</returns>
    [HttpGet(TestRunRoutes.ResultById)]
    [Authorize(TestModulePermissions.Runs.View)]
    public virtual async Task<Result<TestRunResultDto>> GetResult(Guid id)
    {
        var result = await AppService.GetResultAsync(id);
        return result;
    }

    /// <summary>Tenant ortam ayarini cozip yeni Pending kosum olusturur.</summary>
    /// <param name="input">Kosum, ortam ve fingerprint girdileri.</param>
    /// <returns>Ev standardi icinde kalicilastirilan Pending kosum.</returns>
    [HttpPost]
    [Authorize(TestModulePermissions.Runs.Trigger)]
    public virtual async Task<Result<TestRunDto>> Create([FromBody] CreateTestRunDto input)
    {
        var result = await AppService.CreateAsync(input);
        return result;
    }

    /// <summary>Yeni kosum olusturup dayanikli icra job'ini kuyruga verir.</summary>
    /// <param name="input">Kosum, ortam ve fingerprint girdileri.</param>
    /// <returns>Ev standardi icinde kuyruga verilen Pending kosum.</returns>
    [HttpPost(TestRunRoutes.Trigger)]
    [Authorize(TestModulePermissions.Runs.Trigger)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public virtual async Task<Result<TestRunDto>> Trigger([FromBody] CreateTestRunDto input)
    {
        var result = await AppService.TriggerAsync(input);
        Response.StatusCode = StatusCodes.Status202Accepted;
        return result;
    }

    /// <summary>Pending kosumu idempotent bicimde Running durumuna claim eder.</summary>
    /// <param name="id">Claim edilecek TestRun aggregate kimligi.</param>
    /// <returns>Ev standardi icinde claim karari ve guncel kosum.</returns>
    [HttpPost(TestRunRoutes.Start)]
    [Authorize(TestModulePermissions.Runs.Start)]
    public virtual async Task<Result<TestRunClaimDto>> Start(Guid id)
    {
        var result = await AppService.StartAsync(id);
        return result;
    }

    /// <summary>Running kosuma terminal sonucu ve bulgulari atomik yazar.</summary>
    /// <param name="id">Terminale tasinacak TestRun aggregate kimligi.</param>
    /// <param name="input">Hukum, teshis, sure, artefakt ve bulgu girdileri.</param>
    /// <returns>Ev standardi icinde kalicilastirilan bulgulu terminal sonuc.</returns>
    [HttpPost(TestRunRoutes.Terminal)]
    [Authorize(TestModulePermissions.Runs.WriteResult)]
    public virtual async Task<Result<TestRunResultDto>> WriteTerminal(
        Guid id,
        [FromBody] WriteTestRunTerminalDto input)
    {
        var result = await AppService.WriteTerminalAsync(id, input);
        return result;
    }
}
