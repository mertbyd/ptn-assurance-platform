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
    private readonly GroundingManager _groundingManager;
    private readonly EvidenceChainManager _evidenceChainManager;
    private readonly ProfilePackManager _profilePackManager;
    private readonly ProfilePackFileManager _profilePackFileManager;
    private readonly ToolCatalogManager _toolCatalogManager;
    private readonly ISchemaKnowledgeAppService _schemaKnowledgeAppService;
    private readonly IWriteSetCapabilityService _writeSetCapabilityService;
    private readonly IDatabaseOracleAppService _databaseOracleAppService;
    private readonly IValidator<GroundRequestDto> _groundValidator;
    private readonly IValidator<ExplainRequestDto> _explainValidator;
    private readonly IValidator<ValidateRequestDto> _validateValidator;
    private readonly IValidator<KnowledgeRequestDto> _knowledgeValidator;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    public PtnBridgeAppService(
        GroundingManager groundingManager,
        EvidenceChainManager evidenceChainManager,
        ProfilePackManager profilePackManager,
        ProfilePackFileManager profilePackFileManager,
        ToolCatalogManager toolCatalogManager,
        ISchemaKnowledgeAppService schemaKnowledgeAppService,
        IWriteSetCapabilityService writeSetCapabilityService,
        IDatabaseOracleAppService databaseOracleAppService,
        IValidator<GroundRequestDto> groundValidator,
        IValidator<ExplainRequestDto> explainValidator,
        IValidator<ValidateRequestDto> validateValidator,
        IValidator<KnowledgeRequestDto> knowledgeValidator,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        _groundingManager = groundingManager;
        _evidenceChainManager = evidenceChainManager;
        _profilePackManager = profilePackManager;
        _profilePackFileManager = profilePackFileManager;
        _toolCatalogManager = toolCatalogManager;
        _schemaKnowledgeAppService = schemaKnowledgeAppService;
        _writeSetCapabilityService = writeSetCapabilityService;
        _databaseOracleAppService = databaseOracleAppService;
        _groundValidator = groundValidator;
        _explainValidator = explainValidator;
        _validateValidator = validateValidator;
        _knowledgeValidator = knowledgeValidator;
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    // Tek ground istegini dogrulayip birlesik grounding sonucuna cevirir.
    public async Task<GroundResultDto> GroundAsync(GroundRequestDto input)
    {
        var cancellationToken = _cancellationTokenProvider.Token;
        await _groundValidator.ValidateAndThrowAsync(input, cancellationToken);
        var pack = await _profilePackFileManager.LoadAsync(input.ProfileKey, cancellationToken);
        var fingerprint = await _schemaKnowledgeAppService.GetSchemaFingerprintAsync(input.ConnectionId, cancellationToken);
        var capability = await _writeSetCapabilityService.ProbeCapabilityAsync(
            input.ConnectionId, input.HasExclusiveSandbox, cancellationToken);
        return Mapper.Map(_groundingManager.Ground(
            Mapper.Map(input), pack, fingerprint, Mapper.Map(capability)));
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
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Mapper.Map(_toolCatalogManager.GetCatalog(PtnResponseFormatCodes.Concise)));
    }
}
