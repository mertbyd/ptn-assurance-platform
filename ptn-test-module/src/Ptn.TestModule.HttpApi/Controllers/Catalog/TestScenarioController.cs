using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Catalog;
using SystemStandards.Results;
using Volo.Abp.Application.Dtos;

namespace Ptn.TestModule.Controllers.Catalog;

// islevi: Senaryo katalogu CRUD, onay ve yayin use-case'lerini HTTP'ye acar.
// sistemdeki gorevi: Route, binding ve operation permission'ini tasiyip her istegi tek AppService cagrisina yonlendirir.
/// <summary>Test senaryosu katalog HTTP islemlerini sunar.</summary>
[Route(TestScenarioRoutes.Root)]
[ApiExplorerSettings(GroupName = TestScenarioRoutes.SwaggerGroupName)]
public class TestScenarioController : TestModuleController
{
    /// <summary>Senaryo katalogu AppService'ini lazy cozer.</summary>
    private ITestScenarioAppService AppService => LazyGetRequiredService<ITestScenarioAppService>();

    /// <summary>Kimligi verilen senaryo surumunu getirir.</summary>
    /// <param name="id">Okunacak senaryo surumu kimligi.</param>
    /// <returns>Ev standardi icinde senaryo gorunumu.</returns>
    [HttpGet(TestScenarioRoutes.ById)]
    [Authorize(TestModulePermissions.Scenarios.Default)]
    public virtual async Task<Result<TestScenarioDto>> Get(Guid id)
    {
        var result = await AppService.GetAsync(id);
        return result;
    }

    /// <summary>Senaryo surumlerini kararli sayfalama ile getirir.</summary>
    /// <param name="input">Sayfalama girdisi.</param>
    /// <returns>Ev standardi icinde senaryo sayfasi.</returns>
    [HttpGet]
    [Authorize(TestModulePermissions.Scenarios.Default)]
    public virtual async Task<Result<PagedResultDto<TestScenarioDto>>> GetList(
        [FromQuery] TestScenarioListInput input)
    {
        var result = await AppService.GetListAsync(input);
        return result;
    }

    /// <summary>Draft durumunda yeni senaryo surumu olusturur.</summary>
    /// <param name="input">Senaryo yazarlik girdisi.</param>
    /// <returns>Ev standardi icinde olusturulan senaryo.</returns>
    [HttpPost]
    [Authorize(TestModulePermissions.Scenarios.Create)]
    public virtual async Task<Result<TestScenarioDto>> Create([FromBody] CreateTestScenarioDto input)
    {
        var result = await AppService.CreateAsync(input);
        return result;
    }

    /// <summary>Draft veya onay bekleyen senaryo surumunu gunceller.</summary>
    /// <param name="id">Guncellenecek senaryo surumu kimligi.</param>
    /// <param name="input">Degistirilebilir senaryo alanlari.</param>
    /// <returns>Ev standardi icinde guncellenen senaryo.</returns>
    [HttpPut(TestScenarioRoutes.ById)]
    [Authorize(TestModulePermissions.Scenarios.Update)]
    public virtual async Task<Result<TestScenarioDto>> Update(
        Guid id,
        [FromBody] UpdateTestScenarioDto input)
    {
        var result = await AppService.UpdateAsync(id, input);
        return result;
    }

    /// <summary>Tarihsel senaryo silme istegini domain kuralina yonlendirir.</summary>
    /// <param name="id">Silme kuralina sunulacak senaryo surumu kimligi.</param>
    /// <returns>Basarili olursa iceriksiz ev standardi sonucu.</returns>
    [HttpDelete(TestScenarioRoutes.ById)]
    [Authorize(TestModulePermissions.Scenarios.Delete)]
    public virtual async Task<Result> Delete(Guid id)
    {
        await AppService.DeleteAsync(id);
        return Result.NoContent();
    }

    /// <summary>Draft senaryo surumunu insan onayina sunar.</summary>
    /// <param name="id">Onaya sunulacak senaryo surumu kimligi.</param>
    /// <returns>Ev standardi icinde guncellenen senaryo.</returns>
    [HttpPost(TestScenarioRoutes.SubmitForApproval)]
    [Authorize(TestModulePermissions.Scenarios.Update)]
    public virtual async Task<Result<TestScenarioDto>> SubmitForApproval(Guid id)
    {
        var result = await AppService.SubmitForApprovalAsync(id);
        return result;
    }

    /// <summary>Bes yayin kapisini makine kanitiyla, kalici durum degistirmeden degerlendirir.</summary>
    /// <param name="id">Degerlendirilecek senaryo surumu kimligi.</param>
    /// <returns>Ev standardi icinde yayin kapisi karari.</returns>
    [HttpPost(TestScenarioRoutes.EvaluatePublication)]
    [Authorize(TestModulePermissions.Scenarios.Publish)]
    public virtual async Task<Result<TestScenarioPublishDecisionDto>> EvaluatePublication(Guid id)
    {
        var result = await AppService.EvaluatePublicationAsync(id);
        return result;
    }

    /// <summary>Onayi source hash'e baglayip tum kapilar gectiyse senaryoyu yayinlar.</summary>
    /// <param name="id">Yayinlanacak senaryo surumu kimligi.</param>
    /// <returns>Ev standardi icinde yayinlanan senaryo.</returns>
    [HttpPost(TestScenarioRoutes.Publish)]
    [Authorize(TestModulePermissions.Scenarios.Publish)]
    [Authorize(TestModulePermissions.Scenarios.Approve)]
    public virtual async Task<Result<TestScenarioDto>> Publish(Guid id)
    {
        var result = await AppService.PublishAsync(id);
        return result;
    }

    /// <summary>Yayinlanmis senaryo surumunu tarihsel kaydi koruyarak kullanimdan kaldirir.</summary>
    /// <param name="id">Kullanimdan kaldirilacak senaryo surumu kimligi.</param>
    /// <returns>Ev standardi icinde guncellenen senaryo.</returns>
    [HttpPost(TestScenarioRoutes.Deprecate)]
    [Authorize(TestModulePermissions.Scenarios.Update)]
    public virtual async Task<Result<TestScenarioDto>> Deprecate(Guid id)
    {
        var result = await AppService.DeprecateAsync(id);
        return result;
    }
}
