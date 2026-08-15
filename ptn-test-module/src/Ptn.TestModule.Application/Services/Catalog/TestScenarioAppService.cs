using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Nexum.Abp.Foundation.Application.Services;
using Nexum.Abp.Foundation.Querying;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Interface.Catalog;
using Ptn.TestModule.Interface.Compilation;
using Ptn.TestModule.Managers.Catalog;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Mappers.Catalog;
using Ptn.TestModule.Models.Catalog;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Bridge;
using Volo.Abp;
using Volo.Abp.Timing;
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
    private readonly IScenarioCompilationPort _compilationPort;
    private readonly ScenarioQuarantineManager _quarantineManager;
    private readonly IValidator<UpdateScenarioScheduleDto> _scheduleValidator;
    private readonly IClock _clock;

    protected override string GetPolicyName => TestModulePermissions.Scenarios.Default;
    protected override string CreatePolicyName => TestModulePermissions.Scenarios.Create;
    protected override string UpdatePolicyName => TestModulePermissions.Scenarios.Update;
    protected override string DeletePolicyName => TestModulePermissions.Scenarios.Delete;

    // Yayin kapisini, sema fingerprint capability'sini ve makine derlemesini katalog akimina baglar.
    public TestScenarioAppService(
        ScenarioPublicationGateManager publicationGateManager,
        ISchemaKnowledgeAppService schemaKnowledgeAppService,
        IScenarioCompilationPort compilationPort,
        ScenarioQuarantineManager quarantineManager,
        IValidator<UpdateScenarioScheduleDto> scheduleValidator,
        IClock clock)
    {
        _publicationGateManager = publicationGateManager;
        _schemaKnowledgeAppService = schemaKnowledgeAppService;
        _compilationPort = compilationPort;
        _quarantineManager = quarantineManager;
        _scheduleValidator = scheduleValidator;
        _clock = clock;
    }
    // Senaryoyu sinirli sureyle karantinaya alir; sure kararini Manager verir.
    public async Task<TestScenarioDto> QuarantineAsync(Guid id, QuarantineTestScenarioDto input)
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Quarantine);
        var entity = await Manager.EnsureExistsAsync(id, cancellationToken: CancellationTokenProvider.Token);
        _quarantineManager.Quarantine(entity, input.QuarantineUntil, input.QuarantineReason, _clock.Now);
        var saved = await Repository.UpdateAsync(
            entity,
            autoSave: true,
            cancellationToken: CancellationTokenProvider.Token);
        return Mapper.Map(saved);
    }
    // Suresi dolmus karantinayi temizler; suresi dolmamis senaryo karantinada kalir.
    public async Task<TestScenarioDto> ReleaseQuarantineAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Quarantine);
        var entity = await Manager.EnsureExistsAsync(id, cancellationToken: CancellationTokenProvider.Token);
        _quarantineManager.ReleaseExpired(entity, _clock.Now);
        var saved = await Repository.UpdateAsync(
            entity,
            autoSave: true,
            cancellationToken: CancellationTokenProvider.Token);
        return Mapper.Map(saved);
    }
    // Yayinlanmis surume cron zamanlamasi yazar; vade ve dogrulama kararlarini Manager verir.
    public async Task<TestScenarioDto> UpdateScheduleAsync(Guid id, UpdateScenarioScheduleDto input)
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Schedule);
        await _scheduleValidator.ValidateAndThrowAsync(input, CancellationTokenProvider.Token);
        var entity = await Manager.EnsureExistsAsync(id, cancellationToken: CancellationTokenProvider.Token);
        var scheduled = await Manager.UpdateScheduleAsync(
            entity,
            Mapper.Map(input),
            _clock.Now,
            CancellationTokenProvider.Token);
        var saved = await Repository.UpdateAsync(
            scheduled,
            autoSave: true,
            cancellationToken: CancellationTokenProvider.Token);
        return Mapper.Map(saved);
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
        var evidence = await _compilationPort.CompileAsync(entity, CancellationTokenProvider.Token);
        return Mapper.Map(_publicationGateManager.Evaluate(entity, evidence));
    }
    // Onayi source hash'e baglar, belgeyi yeniden derler ve kapilar gectiyse kaniti satira yazar.
    public async Task<TestScenarioDto> PublishAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Approve);
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Publish);
        var entity = await Manager.EnsureExistsAsync(id, cancellationToken: CancellationTokenProvider.Token);
        await Manager.BindApprovalAsync(entity, CurrentUser.GetId(), Clock.Now, CancellationTokenProvider.Token);
        var evidence = await _compilationPort.CompileAsync(entity, CancellationTokenProvider.Token);
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
