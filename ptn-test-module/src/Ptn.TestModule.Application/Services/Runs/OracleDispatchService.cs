using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Mappers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Runs;
using Ptn.TestModule.Services.Bridge;
using Volo.Abp.DependencyInjection;

namespace Ptn.TestModule.Services.Runs;

// islevi: HAR'in her adimini mevcut Bridge yuzeyleri uzerinden hakemlere dagitir ve teshis cagrisini yapar.
// sistemdeki gorevi: Uzak checker cagrilarini UoW disinda sirayla yapan ince orkestrasyondur; her karar Manager'dadir (ADR-0015 §B/§F).
public sealed class OracleDispatchService : ITransientDependency
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
        var judgements = new List<StepJudgement>(document.Entries.Count + 1);
        foreach (var entry in document.Entries)
        {
            judgements.Add(await JudgeEntryAsync(entry, context, cancellationToken));
        }

        judgements.Add(_dispatchManager.JudgeRunner(outcome));
        var diagnosis = await DiagnoseAsync(judgements, context, cancellationToken);
        return _outcomeResolver.Resolve(_dispatchManager.Combine(judgements, diagnosis), harBlobName);
    }

    // Veritabani adimini yanit olarak okur, API adimini uygunluk yuzeyine gonderir (ADR-0015 §D).
    private async Task<StepJudgement> JudgeEntryAsync(
        HarEntryModel entry,
        TestRunExecutionContext context,
        CancellationToken cancellationToken)
    {
        if (entry.IsDatabaseAssertion)
        {
            return _dispatchManager.JudgeDatabaseAssertion(entry);
        }

        var observation = ApiMapper.MapToDto(_dispatchManager.CreateObservation(entry, context));
        var result = await _apiOracleAppService.AssertResponseAsync(observation, cancellationToken);
        return _dispatchManager.JudgeResponse(entry, ApiMapper.MapResult(result));
    }

    // Birincil kirmizi adimi kaynak hakemine gore dogru teshis ucuna gonderir.
    private async Task<DiagnosisReport?> DiagnoseAsync(
        IReadOnlyList<StepJudgement> judgements,
        TestRunExecutionContext context,
        CancellationToken cancellationToken)
    {
        var target = OracleDispatchManager.SelectDiagnosisTarget(judgements);
        if (target is null)
        {
            return null;
        }

        var request = DiagnosisMapper.MapToDto(_dispatchManager.CreateDiagnosisRequest(target, context));
        var report = target.SourceCheckerCode == TestSourceCheckerCodes.DatabaseComparison
            ? await _failureDiagnosisAppService.DiagnoseDatabaseAsync(request, cancellationToken)
            : await _failureDiagnosisAppService.DiagnoseApiAsync(request, cancellationToken);
        return DiagnosisMapper.MapReport(report);
    }
}
