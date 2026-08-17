using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Mappers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Runs;
using Ptn.TestModule.Services.Bridge;
using Volo.Abp.DependencyInjection;

namespace Ptn.TestModule.Services.Runs;

// islevi: HAR'in her adimini mevcut Bridge yuzeyleri uzerinden hakemlere dagitir ve teshis cagrisini yapar.
// sistemdeki gorevi: Uzak checker cagrilarini UoW disinda sirayla yapan ince orkestrasyondur; her karar Manager'dadir (ADR-0015 §B/§F).
[ExposeServices(typeof(IOracleDispatchPort))]
public sealed class OracleDispatchService : IOracleDispatchPort, ITransientDependency
{
    // Bridge DTO'lari ile domain modelleri arasindaki compile-time eslemeleri saglar.
    private static readonly ApiOracleMapper ApiMapper = new();
    private static readonly FailureDiagnosisMapper DiagnosisMapper = new();

    // HAR govdesini adima baglanmis belge modeline ceviren Manager'dir.
    private readonly HarInterpreter _harInterpreter;

    // Hangi adimin hangi hakeme gidecegini ve hukmun ne anlama geldigini sahiplenen Manager'dir.
    private readonly OracleDispatchManager _dispatchManager;

    // Adim hukumlerinden terminal kosum hukmunu tureten Manager'dir.
    private readonly RunOutcomeResolver _outcomeResolver;

    // API sozlesme hukmunun kayit sahibi Bridge yuzeyidir.
    private readonly IApiOracleAppService _apiOracleAppService;

    // Iki checker ailesinin ortak teshis Bridge yuzeyidir.
    private readonly IFailureDiagnosisAppService _failureDiagnosisAppService;

    // Yargi asamasinin Manager sahiplerini mevcut Bridge yuzeylerine baglar.
    public OracleDispatchService(
        HarInterpreter harInterpreter,
        OracleDispatchManager dispatchManager,
        RunOutcomeResolver outcomeResolver,
        IApiOracleAppService apiOracleAppService,
        IFailureDiagnosisAppService failureDiagnosisAppService)
    {
        _harInterpreter = harInterpreter;
        _dispatchManager = dispatchManager;
        _outcomeResolver = outcomeResolver;
        _apiOracleAppService = apiOracleAppService;
        _failureDiagnosisAppService = failureDiagnosisAppService;
    }

    // HAR'in her entry'sini yargilar, kirmiziyi teshise gonderir ve terminal hukmu dondurur.
    public async Task<TestRunJudgement> JudgeAsync(
        TestRunExecutionContext context,
        WorkflowRunOutcome outcome,
        string? harBlobName,
        CancellationToken cancellationToken = default)
    {
        var document = _harInterpreter.Interpret(outcome.HarContent, context.DocumentFacts);
        var plan = _dispatchManager.CreatePlan(document, context, outcome);
        var responseResults = new List<ConformanceResult>();
        foreach (var step in plan.Steps.Where(step => step.Observation is not null))
        {
            var result = await _apiOracleAppService.AssertResponseAsync(
                ApiMapper.MapToDto(step.Observation!), cancellationToken);
            responseResults.Add(ApiMapper.MapResult(result));
        }
        var judgements = _dispatchManager.CompletePlan(plan, responseResults);
        var diagnosisPlan = _dispatchManager.CreateDiagnosisPlan(judgements, context);
        var apiReports = new List<DiagnosisReport>();
        foreach (var request in diagnosisPlan.ApiRequests)
        {
            var report = await _failureDiagnosisAppService.DiagnoseApiAsync(
                DiagnosisMapper.MapToDto(request), cancellationToken);
            apiReports.Add(DiagnosisMapper.MapReport(report));
        }
        var databaseReports = new List<DiagnosisReport>();
        foreach (var request in diagnosisPlan.DatabaseRequests)
        {
            var report = await _failureDiagnosisAppService.DiagnoseDatabaseAsync(
                DiagnosisMapper.MapToDto(request), cancellationToken);
            databaseReports.Add(DiagnosisMapper.MapReport(report));
        }
        var dispatch = _dispatchManager.CompleteDiagnosis(judgements, apiReports, databaseReports);
        return _outcomeResolver.Resolve(dispatch, harBlobName);
    }
}
