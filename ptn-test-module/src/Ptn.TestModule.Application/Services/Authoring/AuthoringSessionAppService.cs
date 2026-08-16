using System;
using System.Threading.Tasks;
using FluentValidation;
using Ptn.TestModule.Dtos.Authoring;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.Interface.Authoring;
using Ptn.TestModule.Managers.Authoring;
using Ptn.TestModule.Mappers.Authoring;
using Ptn.TestModule.Mappers.Bridge;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Bridge;
using Volo.Abp.Threading;

namespace Ptn.TestModule.Services.Authoring;

// islevi: Grounding, cache I/O, validation, Manager ve Mapperly adimlarini authoring use-case'lerinde orkestre eder.
// sistemdeki gorevi: Dort HTTP action'in tenant-aware ve modelsiz Application gerceklestirimidir.
public class AuthoringSessionAppService : TestModuleAppService, IAuthoringSessionAppService
{
    private static readonly AuthoringSessionMapper Mapper = new();
    private static readonly PtnBridgeMapper BridgeMapper = new();
    private readonly IPtnBridgeAppService _bridgeAppService;
    private readonly IAuthoringSessionStore _sessionStore;
    private readonly AuthoringSessionManager _manager;
    private readonly IValidator<CreateAuthoringSessionDto> _createValidator;
    private readonly IValidator<AnswerAuthoringSessionDto> _answerValidator;
    private readonly IValidator<AddAuthoringStepDto> _stepValidator;
    private readonly IValidator<AddDatabaseAuthoringStepDto> _databaseStepValidator;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    // Authoring use-case'lerini mevcut grounding, cache ve domain sahiplerine baglar.
    public AuthoringSessionAppService(
        IPtnBridgeAppService bridgeAppService,
        IAuthoringSessionStore sessionStore,
        AuthoringSessionManager manager,
        IValidator<CreateAuthoringSessionDto> createValidator,
        IValidator<AnswerAuthoringSessionDto> answerValidator,
        IValidator<AddAuthoringStepDto> stepValidator,
        IValidator<AddDatabaseAuthoringStepDto> databaseStepValidator,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        _bridgeAppService = bridgeAppService;
        _sessionStore = sessionStore;
        _manager = manager;
        _createValidator = createValidator;
        _answerValidator = answerValidator;
        _stepValidator = stepValidator;
        _databaseStepValidator = databaseStepValidator;
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    // Grounding sonucunu tenant cache'inde yeni yazarlik oturumuna cevirir.
    public async Task<AuthoringSessionDto> CreateAsync(CreateAuthoringSessionDto input)
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Create);
        var token = _cancellationTokenProvider.Token;
        await _createValidator.ValidateAndThrowAsync(input, token);
        var grounding = await _bridgeAppService.GroundAsync(input.Grounding);
        var session = _manager.Create(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            Mapper.Map(input),
            BridgeMapper.Map(input.Grounding),
            BridgeMapper.Map(grounding));
        await _sessionStore.SetAsync(session, token);
        return Mapper.Map(session);
    }

    // Tenant cache'inden session durumunu getirir; expired kaydi bulunamamis sayar.
    public async Task<AuthoringSessionDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Update);
        var session = _manager.EnsureAvailable(
            await _sessionStore.GetAsync(id, _cancellationTokenProvider.Token),
            CurrentTenant.Id);
        return Mapper.Map(session);
    }

    // Kapali cevabi dogrular, Manager'a uygulatir ve ayni TTL sozlesmesiyle yazar.
    public async Task<AuthoringSessionDto> AnswerAsync(Guid id, AnswerAuthoringSessionDto input)
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Update);
        var token = _cancellationTokenProvider.Token;
        await _answerValidator.ValidateAndThrowAsync(input, token);
        var session = _manager.EnsureAvailable(
            await _sessionStore.GetAsync(id, token), CurrentTenant.Id);
        var answered = _manager.Answer(session, CurrentTenant.Id, Mapper.Map(input));
        await _sessionStore.SetAsync(answered, token);
        return Mapper.Map(answered);
    }

    // Tek-adim oneriyi dogrular, mekanik belgeye birlestirir ve session'i cache'e yazar.
    public async Task<AuthoringSessionDto> AddStepAsync(Guid id, AddAuthoringStepDto input)
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Update);
        var token = _cancellationTokenProvider.Token;
        await _stepValidator.ValidateAndThrowAsync(input, token);
        var session = _manager.EnsureAvailable(
            await _sessionStore.GetAsync(id, token), CurrentTenant.Id);
        var updated = _manager.AddStep(session, CurrentTenant.Id, Mapper.Map(input));
        await _sessionStore.SetAsync(updated, token);
        return Mapper.Map(updated);
    }

    // Tipli veritabani adimini dogrular, session'a ekler ve cache'e yazar.
    public async Task<AuthoringSessionDto> AddDatabaseStepAsync(Guid id, AddDatabaseAuthoringStepDto input)
    {
        await CheckPolicyAsync(TestModulePermissions.Scenarios.Update);
        var token = _cancellationTokenProvider.Token;
        await _databaseStepValidator.ValidateAndThrowAsync(input, token);
        var session = _manager.EnsureAvailable(
            await _sessionStore.GetAsync(id, token), CurrentTenant.Id);
        var updated = _manager.AddDatabaseStep(session, CurrentTenant.Id, Mapper.Map(input));
        await _sessionStore.SetAsync(updated, token);
        return Mapper.Map(updated);
    }
}
