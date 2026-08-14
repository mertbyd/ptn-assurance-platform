using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Application.Services;
using Nexum.Abp.Foundation.Querying;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Interface.Catalog;
using Ptn.TestModule.Managers.Catalog;
using Ptn.TestModule.Mappers.Catalog;
using Ptn.TestModule.Models.Catalog;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Bridge;
using Ptn.TestModule.Services.Compilation;
using Volo.Abp;
using Volo.Abp.Users;

namespace Ptn.TestModule.Services.Catalog;

// islevi: Senaryo katalogu CRUD, onay ve yayin use-case'lerini duz bir akisla orkestre eder.
// sistemdeki gorevi: Mapperly, Manager, repository ve guncel sema fingerprint capability'sini baglar.
[RemoteService(IsEnabled = false)]
public class TestScenarioAppService : BaseApplicationService<
    TestScenario,
    Guid,
    TestScenarioDto,
    TestScenarioListInput,
    CreateTestScenarioDto,
    UpdateTestScenarioDto,
    TestScenarioCreateModel,
    TestScenarioUpdateModel,
    TestScenarioManager,
    ITestScenarioRepository>, ITestScenarioAppService
{
    private static readonly TestScenarioMapper Mapper = new();
    private readonly ScenarioPublicationGateManager _publicationGateManager;
    private readonly ISchemaKnowledgeAppService _schemaKnowledgeAppService;
    private readonly ScenarioCompilationService _compilationService;

    protected override string GetPolicyName => TestModulePermissions.Scenarios.Default;
    protected override string CreatePolicyName => TestModulePermissions.Scenarios.Create;
    protected override string UpdatePolicyName => TestModulePermissions.Scenarios.Update;
    protected override string DeletePolicyName => TestModulePermissions.Scenarios.Delete;

    // Yayin kapisini, sema fingerprint capability'sini ve makine derlemesini katalog akimina baglar.
    public TestScenarioAppService(
        ScenarioPublicationGateManager publicationGateManager,
        ISchemaKnowledgeAppService schemaKnowledgeAppService,
        ScenarioCompilationService compilationService)
    {
        _publicationGateManager = publicationGateManager;
        _schemaKnowledgeAppService = schemaKnowledgeAppService;
        _compilationService = compilationService;
    }

    // Draft senaryo surumunu insan onayi bekleyen duruma tasiyip kaydeder.
    public async Task<TestScenarioDto> SubmitForApprovalAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Update);
        var entity = await Manager.EnsureExistsAsync(id, cancellationToken: CancellationTokenProvider.Token);
        var updated = await Manager.SubmitForApprovalAsync(entity, CancellationTokenProvider.Token);
        var saved = await Repository.UpdateAsync(
            updated,
            autoSave: true,
            cancellationToken: CancellationTokenProvider.Token);
        return Mapper.Map(saved);
    }

    // Bes yayin kapisini derleyicinin urettigi kanitla, kalici durumu degistirmeden degerlendirir.
    public async Task<TestScenarioPublishDecisionDto> EvaluatePublicationAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Publish);
        var entity = await Manager.EnsureExistsAsync(id, cancellationToken: CancellationTokenProvider.Token);
        var evidence = await _compilationService.CompileAsync(entity, CancellationTokenProvider.Token);
        return Mapper.Map(_publicationGateManager.Evaluate(entity, evidence));
    }

    // Onayi source hash'e baglar, belgeyi yeniden derler ve kapilar gectiyse kaniti satira yazar.
    public async Task<TestScenarioDto> PublishAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Approve);
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Publish);
        var entity = await Manager.EnsureExistsAsync(id, cancellationToken: CancellationTokenProvider.Token);
        await Manager.BindApprovalAsync(entity, CurrentUser.GetId(), Clock.Now, CancellationTokenProvider.Token);
        var evidence = await _compilationService.CompileAsync(entity, CancellationTokenProvider.Token);
        var decision = _publicationGateManager.Evaluate(entity, evidence);
        var published = await Manager.PublishAsync(entity, decision, evidence, CancellationTokenProvider.Token);
        var saved = await Repository.UpdateAsync(
            published,
            autoSave: true,
            cancellationToken: CancellationTokenProvider.Token);
        return Mapper.Map(saved);
    }

    // Published senaryo surumunu silmeden Deprecated durumuna tasiyip kaydeder.
    public async Task<TestScenarioDto> DeprecateAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Update);
        var entity = await Manager.EnsureExistsAsync(id, cancellationToken: CancellationTokenProvider.Token);
        var deprecated = await Manager.DeprecateAsync(entity, CancellationTokenProvider.Token);
        var saved = await Repository.UpdateAsync(
            deprecated,
            autoSave: true,
            cancellationToken: CancellationTokenProvider.Token);
        return Mapper.Map(saved);
    }

    // Liste sorgusunu senaryo anahtari ve azalan surum numarasiyla tamamen kararli siralar.
    protected override RepositoryQuery<TestScenario> CreateListQuery(TestScenarioListInput input)
    {
        return new RepositoryQuery<TestScenario>()
            .OrderBy(entity => entity.ScenarioKey)
            .ThenByDescending(entity => entity.VersionNo);
    }

    // Guncel DB sema fingerprint'ini material seal'e uygulayip Manager ile Draft entity kurar.
    protected override async Task<TestScenario> CreateEntityAsync(
        TestScenarioCreateModel model,
        CancellationToken cancellationToken)
    {
        if (model.MaterialSeal.DbConnectionId.HasValue)
        {
            var fingerprint = await _schemaKnowledgeAppService.GetSchemaFingerprintAsync(
                model.MaterialSeal.DbConnectionId.Value, cancellationToken);
            Manager.ApplyDbSchemaFingerprint(model.MaterialSeal, fingerprint);
        }
        return await Manager.CreateAsync(model, cancellationToken);
    }

    // Guncel DB sema fingerprint'ini material seal'e uygulayip Manager mutasyonunu calistirir.
    protected override async Task<TestScenario> UpdateEntityAsync(
        TestScenario entity,
        TestScenarioUpdateModel model,
        CancellationToken cancellationToken)
    {
        if (model.MaterialSeal.DbConnectionId.HasValue)
        {
            var fingerprint = await _schemaKnowledgeAppService.GetSchemaFingerprintAsync(
                model.MaterialSeal.DbConnectionId.Value, cancellationToken);
            Manager.ApplyDbSchemaFingerprint(model.MaterialSeal, fingerprint);
        }
        return await Manager.UpdateAsync(entity, model, cancellationToken);
    }

    // Tarihsel senaryo silme istegini Manager'in kalici ret kuralina devreder.
    protected override Task EnsureCanDeleteAsync(TestScenario entity, CancellationToken cancellationToken)
    {
        return Manager.EnsureCanDeleteAsync(entity, cancellationToken);
    }

    // Create DTO'sunu Mapperly ile domain modeline cevirir.
    protected override TestScenarioCreateModel MapToCreateModel(CreateTestScenarioDto input)
    {
        return Mapper.Map(input);
    }

    // Update DTO'sunu Mapperly ile domain modeline cevirir.
    protected override TestScenarioUpdateModel MapToUpdateModel(UpdateTestScenarioDto input)
    {
        return Mapper.Map(input);
    }

    // Aggregate'i Mapperly ile public katalog DTO'suna cevirir.
    protected override TestScenarioDto MapToDto(TestScenario entity)
    {
        return Mapper.Map(entity);
    }

    // Aggregate sayfasini tek Mapperly collection eslemesiyle DTO listesine cevirir.
    protected override List<TestScenarioDto> MapToDto(IReadOnlyList<TestScenario> entities)
    {
        return Mapper.Map([.. entities]);
    }
}
