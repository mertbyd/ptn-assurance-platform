using System.Threading.Tasks;
using FluentValidation;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Managers.Bridge.Profiles;
using Ptn.TestModule.Mappers.Bridge;
using Volo.Abp;
using Volo.Abp.Threading;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Bridge agent girdilerini dogrular, ilgili manager'i cagirir ve Mapperly ile DTO dondurur.
// sistemdeki gorevi: HTTP/MCP yuzeyi ile deterministik grounding, explanation ve capability kararlarini baglar.
[RemoteService(IsEnabled = false)]
public class PtnBridgeAppService : TestModuleAppService, IPtnBridgeAppService
{
    private static readonly PtnBridgeMapper Mapper = new();
    private static readonly PtnExplanationMapper ExplanationMapper = new();
    private static readonly DatabaseOracleMapper DatabaseMapper = new();
    private static readonly ApiOracleMapper ApiMapper = new();
    private readonly GroundingManager _groundingManager;
    private readonly EvidenceChainManager _evidenceChainManager;
    private readonly ProfilePackManager _profilePackManager;
    private readonly ProfilePackFileManager _profilePackFileManager;
    private readonly ToolCatalogManager _toolCatalogManager;
    private readonly AgentProfileManager _agentProfileManager;
    private readonly ToolBudgetManager _toolBudgetManager;
    private readonly McpTaskStatusManager _mcpTaskStatusManager;
    private readonly OverlayPatchManager _overlayPatchManager;
    private readonly ISchemaKnowledgeAppService _schemaKnowledgeAppService;
    private readonly IWriteSetCapabilityAppService _writeSetCapabilityService;
    private readonly IDatabaseOracleAppService _databaseOracleAppService;
    private readonly IApiOracleAppService _apiOracleAppService;
    private readonly IValidator<GroundRequestDto> _groundValidator;
    private readonly IValidator<ExplainRequestDto> _explainValidator;
    private readonly IValidator<ValidateRequestDto> _validateValidator;
    private readonly IValidator<KnowledgeRequestDto> _knowledgeValidator;
    private readonly IValidator<AgentProfileRequestDto> _agentProfileValidator;
    private readonly IValidator<ToolBudgetRequestDto> _toolBudgetValidator;
    private readonly IValidator<McpTaskStatusRequestDto> _mcpTaskStatusValidator;
    private readonly IValidator<OverlayPatchRequestDto> _overlayPatchValidator;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    public PtnBridgeAppService(
        GroundingManager groundingManager,
        EvidenceChainManager evidenceChainManager,
        ProfilePackManager profilePackManager,
        ProfilePackFileManager profilePackFileManager,
        ToolCatalogManager toolCatalogManager,
        AgentProfileManager agentProfileManager,
        ToolBudgetManager toolBudgetManager,
        McpTaskStatusManager mcpTaskStatusManager,
        OverlayPatchManager overlayPatchManager,
        ISchemaKnowledgeAppService schemaKnowledgeAppService,
        IWriteSetCapabilityAppService writeSetCapabilityService,
        IDatabaseOracleAppService databaseOracleAppService,
        IApiOracleAppService apiOracleAppService,
        IValidator<GroundRequestDto> groundValidator,
        IValidator<ExplainRequestDto> explainValidator,
        IValidator<ValidateRequestDto> validateValidator,
        IValidator<KnowledgeRequestDto> knowledgeValidator,
        IValidator<AgentProfileRequestDto> agentProfileValidator,
        IValidator<ToolBudgetRequestDto> toolBudgetValidator,
        IValidator<McpTaskStatusRequestDto> mcpTaskStatusValidator,
        IValidator<OverlayPatchRequestDto> overlayPatchValidator,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        _groundingManager = groundingManager;
        _evidenceChainManager = evidenceChainManager;
        _profilePackManager = profilePackManager;
        _profilePackFileManager = profilePackFileManager;
        _toolCatalogManager = toolCatalogManager;
        _agentProfileManager = agentProfileManager;
        _toolBudgetManager = toolBudgetManager;
        _mcpTaskStatusManager = mcpTaskStatusManager;
        _overlayPatchManager = overlayPatchManager;
        _schemaKnowledgeAppService = schemaKnowledgeAppService;
        _writeSetCapabilityService = writeSetCapabilityService;
        _databaseOracleAppService = databaseOracleAppService;
        _apiOracleAppService = apiOracleAppService;
        _groundValidator = groundValidator;
        _explainValidator = explainValidator;
        _validateValidator = validateValidator;
        _knowledgeValidator = knowledgeValidator;
        _agentProfileValidator = agentProfileValidator;
        _toolBudgetValidator = toolBudgetValidator;
        _mcpTaskStatusValidator = mcpTaskStatusValidator;
        _overlayPatchValidator = overlayPatchValidator;
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    // Tek ground istegini dogrulayip birlesik grounding sonucuna cevirir.
    public async Task<GroundResultDto> GroundAsync(GroundRequestDto input)
    {
        var cancellationToken = _cancellationTokenProvider.Token;
        await _groundValidator.ValidateAndThrowAsync(input, cancellationToken);
        var request = Mapper.Map(input);
        var pack = await _profilePackFileManager.LoadAsync(input.ProfileKey, cancellationToken);
        var fingerprint = await _schemaKnowledgeAppService.GetSchemaFingerprintAsync(input.ConnectionId, cancellationToken);
        var capability = await _writeSetCapabilityService.ProbeCapabilityAsync(
            input.ConnectionId, input.HasExclusiveSandbox, cancellationToken);
        var inventory = ApiMapper.MapResult(await _apiOracleAppService.ListSnapshotOperationsAsync(
            input.SpecSnapshotId, cancellationToken));
        var operation = _groundingManager.ResolveOperation(request, inventory);
        var requestExample = operation is null ? null : ApiMapper.MapResult(
            await _apiOracleAppService.BuildRequestExampleAsync(
                ApiMapper.Map(_groundingManager.CreateOperationQuery(request, operation)), cancellationToken));
        var tableBinding = operation is null
            ? null
            : _groundingManager.ResolveTableBinding(request, pack, fingerprint);
        var tableDescription = tableBinding is null ? null : Mapper.Map(
            await _schemaKnowledgeAppService.DescribeTableAsync(
                Mapper.Map(_groundingManager.CreateTableQuery(request, tableBinding)), cancellationToken));
        return Mapper.Map(_groundingManager.Ground(
            request, pack, fingerprint, Mapper.Map(capability), inventory,
            operation, requestExample, tableBinding, tableDescription));
    }

    // Tek explain istegini dogrulayip yurutme-izi aciklama sonucuna cevirir.
    public async Task<ExplainResultDto> ExplainAsync(ExplainRequestDto input)
    {
        var cancellationToken = _cancellationTokenProvider.Token;
        await _explainValidator.ValidateAndThrowAsync(input, cancellationToken);
        var pack = await _profilePackFileManager.LoadAsync(input.ProfileKey, cancellationToken);
        var fingerprint = await _schemaKnowledgeAppService.GetSchemaFingerprintAsync(input.ConnectionId, cancellationToken);
        return ExplanationMapper.Map(_evidenceChainManager.Explain(Mapper.Map(input), pack, fingerprint));
    }

    // Tek validate istegini dogrulayip kapali yayin kapisi sonucuna cevirir.
    public async Task<ValidateResultDto> ValidateAsync(ValidateRequestDto input)
    {
        var cancellationToken = _cancellationTokenProvider.Token;
        await _validateValidator.ValidateAndThrowAsync(input, cancellationToken);
        var request = Mapper.Map(input);
        var pack = await _profilePackFileManager.LoadAsync(input.ProfileKey, cancellationToken);
        var fingerprint = await _schemaKnowledgeAppService.GetSchemaFingerprintAsync(input.ConnectionId, cancellationToken);
        var databaseResult = request.DatabaseAssertions.Count == 0
            ? null
            : DatabaseMapper.Map(await _databaseOracleAppService.ValidateDerivabilityAsync(
                DatabaseMapper.MapToDto(_groundingManager.CreateDatabaseDerivabilityRequest(request)),
                cancellationToken));
        return Mapper.Map(_groundingManager.Validate(
            request, pack, fingerprint, databaseResult));
    }

    // Profil bilgi istegini dogrulayip kapsam raporuna cevirir.
    public async Task<KnowledgeResultDto> GetKnowledgeAsync(KnowledgeRequestDto input)
    {
        var cancellationToken = _cancellationTokenProvider.Token;
        await _knowledgeValidator.ValidateAndThrowAsync(input, cancellationToken);
        var pack = await _profilePackFileManager.LoadAsync(input.ProfileKey, cancellationToken);
        var fingerprint = await _schemaKnowledgeAppService.GetSchemaFingerprintAsync(input.ConnectionId, cancellationToken);
        return Mapper.Map(_profilePackManager.GetKnowledge(Mapper.Map(input), pack, fingerprint));
    }

    // Aktif ve discoverable tool kodlarini varsayilan concise bicimde dondurur.
    public Task<ToolCatalogDto> GetToolCatalogAsync()
    {
        var cancellationToken = _cancellationTokenProvider.Token;
        return Task.FromResult(Mapper.Map(
            _toolCatalogManager.GetCatalog(PtnResponseFormatCodes.Concise, cancellationToken)));
    }

    // Tenant-scoped ajan profilini ABP Setting zincirinden cozer.
    public async Task<AgentProfileDto> ResolveAgentProfileAsync(AgentProfileRequestDto input)
    {
        var cancellationToken = _cancellationTokenProvider.Token;
        await _agentProfileValidator.ValidateAndThrowAsync(input, cancellationToken);
        return Mapper.Map(await _agentProfileManager.ResolveAsync(input.MomentCode, cancellationToken));
    }

    // Tek tool cagrisini aktif moment profilinin iki sayac tavanina karsi denetler.
    public async Task<ToolBudgetDecisionDto> CheckToolBudgetAsync(ToolBudgetRequestDto input)
    {
        var cancellationToken = _cancellationTokenProvider.Token;
        await _toolBudgetValidator.ValidateAndThrowAsync(input, cancellationToken);
        var profile = await _agentProfileManager.ResolveAsync(input.MomentCode, cancellationToken);
        return Mapper.Map(_toolBudgetManager.EnsureWithinBudget(
            profile, input.ToolCode, input.UsedTurns, input.UsedTokens));
    }

    // Ic kosum ve onay durumunu MCP Task wire sozlugune cevirir.
    public async Task<McpTaskStatusDto> MapTaskStatusAsync(McpTaskStatusRequestDto input)
    {
        await _mcpTaskStatusValidator.ValidateAndThrowAsync(input, _cancellationTokenProvider.Token);
        return Mapper.Map(_mcpTaskStatusManager.Map(
            input.TaskId, input.InternalStatus, input.ApprovalRequired,
            input.InfrastructureFailure, input.TtlMs, input.PollIntervalMs));
    }

    // Bulguyla bagli Overlay belgesini uygulamadan uretir.
    public async Task<OverlayPatchSuggestionDto> SuggestOverlayPatchAsync(OverlayPatchRequestDto input)
    {
        await _overlayPatchValidator.ValidateAndThrowAsync(input, _cancellationTokenProvider.Token);
        return Mapper.Map(_overlayPatchManager.Suggest(
            input.FindingFingerprint, input.Target, input.Description, input.UpdateJson));
    }
}
