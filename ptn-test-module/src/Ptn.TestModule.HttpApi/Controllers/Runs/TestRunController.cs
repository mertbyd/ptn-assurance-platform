using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Runs;
using SystemStandards.Results;

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
