using System;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Managers.Bridge.Profiles;
using Ptn.TestModule.Interface.Compilation;
using Ptn.TestModule.Managers.Compilation;
using Ptn.TestModule.Mappers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Database;
using Ptn.TestModule.Models.Compilation;
using Ptn.TestModule.Services.Bridge;
using Ptn.TestModule.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Ptn.TestModule.Services.Compilation;

// islevi: Muhurlu malzemeden profili cozer, senaryoyu derler ve iki turetilebilirlik yuzeyine sorar.
// sistemdeki gorevi: Yayin kanitinin tamamini sunucuda ureten duz Application orkestrasyonudur; karar vermez (ADR-0015 §C).
public sealed class ScenarioCompilationService : IScenarioCompilationPort, ITransientDependency
{
    private static readonly DatabaseOracleMapper DatabaseMapper = new();
    private static readonly ApiOracleMapper ApiMapper = new();
    private readonly ArazzoCompilerManager _compilerManager;
    private readonly ProfilePackFileManager _profilePackFileManager;
    private readonly ProfilePackManager _profilePackManager;
    private readonly ISchemaKnowledgeAppService _schemaKnowledgeAppService;
    private readonly IDatabaseOracleAppService _databaseOracleAppService;
    private readonly IApiOracleAppService _apiOracleAppService;
    private readonly ISettingProvider _settingProvider;

    // Derleyiciyi profil kaynagina, sema muhrune ve iki checker yuzeyine baglar.
    public ScenarioCompilationService(
        ArazzoCompilerManager compilerManager,
        ProfilePackFileManager profilePackFileManager,
        ProfilePackManager profilePackManager,
        ISchemaKnowledgeAppService schemaKnowledgeAppService,
        IDatabaseOracleAppService databaseOracleAppService,
        IApiOracleAppService apiOracleAppService,
        ISettingProvider settingProvider)
    {
        _compilerManager = compilerManager;
        _profilePackFileManager = profilePackFileManager;
        _profilePackManager = profilePackManager;
        _schemaKnowledgeAppService = schemaKnowledgeAppService;
        _databaseOracleAppService = databaseOracleAppService;
        _apiOracleAppService = apiOracleAppService;
        _settingProvider = settingProvider;
    }

    // Senaryonun muhurlu malzemesinden yayin kapisinin okudugu tam makine kanitini uretir.
    public async Task<ScenarioCompilationEvidence> CompileAsync(
        TestScenario scenario,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var profilePack = await ResolveProfilePackAsync(scenario, cancellationToken);
        var compilation = await _compilerManager.CompileAsync(
            scenario.SourceDocument,
            profilePack,
            scenario.SpecSnapshotId ?? Guid.Empty,
            cancellationToken);
        var apiDerivability = await ValidateApiAssertionsAsync(compilation, cancellationToken);
        var databaseDerivability = await ValidateDatabaseAssertionsAsync(scenario, compilation, cancellationToken);
        return _compilerManager.CreateEvidence(compilation, apiDerivability, databaseDerivability);
    }

    // Aktif profil paketini ayardan cozer, dosyadan okur ve guncel sema muhruyle dogrular.
    private async Task<ProfilePack> ResolveProfilePackAsync(
        TestScenario scenario,
        CancellationToken cancellationToken)
    {
        var profileKey = await _settingProvider.GetOrNullAsync(TestModuleSettings.ProfilePackKey);
        var pack = await _profilePackFileManager.LoadAsync(profileKey!, cancellationToken);
        var fingerprint = scenario.DbConnectionId.HasValue
            ? await _schemaKnowledgeAppService.GetSchemaFingerprintAsync(scenario.DbConnectionId.Value, cancellationToken)
            : pack.DbSchemaFingerprint;
        return _profilePackManager.GetValidated(pack, profileKey!, fingerprint);
    }

    // Derlenmis DB adreslerini checker'in turetilebilirlik yuzeyine tasir; adres yoksa cagri yapmaz.
    private async Task<DatabaseDerivabilityResult?> ValidateDatabaseAssertionsAsync(
        TestScenario scenario,
        ArazzoCompilationResult compilation,
        CancellationToken cancellationToken)
    {
        if (compilation.DatabaseAssertions.Count == 0 || !scenario.DbConnectionId.HasValue)
        {
            return null;
        }

        var request = DatabaseMapper.MapToDto(new DatabaseDerivabilityRequest
        {
            ConnectionId = scenario.DbConnectionId.Value,
            Assertions = compilation.DatabaseAssertions
        });
        return DatabaseMapper.Map(
            await _databaseOracleAppService.ValidateDerivabilityAsync(request, cancellationToken));
    }

    // Her derlenmis API operasyonunu sozlesme yuzeyine sorar; pointer yoksa cagri yapmaz.
    private async Task<DerivabilityResult?> ValidateApiAssertionsAsync(
        ArazzoCompilationResult compilation,
        CancellationToken cancellationToken)
    {
        if (compilation.ApiAssertions.Count == 0)
        {
            return null;
        }

        var merged = new DerivabilityResult();
        foreach (var request in compilation.ApiAssertions)
        {
            var result = ApiMapper.MapResult(await _apiOracleAppService.ValidateScenarioAssertionsAsync(
                ApiMapper.MapToDto(request),
                cancellationToken));
            merged.Assertions.AddRange(result.Assertions);
            merged.IsTruncated |= result.IsTruncated;
        }
        return merged;
    }
}
