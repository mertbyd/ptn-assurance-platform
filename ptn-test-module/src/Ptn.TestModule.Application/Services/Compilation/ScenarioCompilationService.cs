using System;
using System.Collections.Generic;
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
        var fingerprintRequests = _profilePackManager.CreateSchemaFingerprintRequests(scenario);
        var profileKey = await _settingProvider.GetOrNullAsync(TestModuleSettings.ProfilePackKey);
        var pack = await _profilePackFileManager.LoadAsync(profileKey!, cancellationToken);
        var fingerprints = new List<string>();
        foreach (var connectionId in fingerprintRequests)
        {
            fingerprints.Add(await _schemaKnowledgeAppService.GetSchemaFingerprintAsync(connectionId, cancellationToken));
        }
        var profilePack = _profilePackManager.GetValidatedForCompilation(pack, profileKey!, fingerprints);
        var compilation = await _compilerManager.CompileAsync(
            scenario.SourceDocument,
            profilePack,
            scenario.SpecSnapshotId ?? Guid.Empty,
            cancellationToken);
        var plan = _compilerManager.CreateDerivabilityPlan(scenario, compilation);
        var apiResults = new List<DerivabilityResult>();
        foreach (var request in plan.ApiRequests)
        {
            apiResults.Add(ApiMapper.MapResult(await _apiOracleAppService.ValidateScenarioAssertionsAsync(
                ApiMapper.MapToDto(request),
                cancellationToken)));
        }
        var databaseResults = new List<DatabaseDerivabilityResult>();
        foreach (var request in plan.DatabaseRequests)
        {
            databaseResults.Add(DatabaseMapper.Map(
                await _databaseOracleAppService.ValidateDerivabilityAsync(
                    DatabaseMapper.MapToDto(request), cancellationToken)));
        }
        return _compilerManager.CreateEvidence(compilation, apiResults, databaseResults);
    }
}
