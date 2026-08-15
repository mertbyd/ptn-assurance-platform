using System;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Interface.Shared;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Mappers.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp.DependencyInjection;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ptn.TestModule.Services.Runs;

// islevi: Arazzo belgesini wire DTO'suna cozer ve Planner'in kurdugu plani ortak surec sinirinda kosar.
// sistemdeki gorevi: Mapperly ve Manager cagrilarini siralayan ince Application orkestrasyonudur (ADR-0015 §A).
public sealed class WorkflowRunnerService : IWorkflowRunnerPort, ITransientDependency
{
    private static readonly TestRunMapper Mapper = new();
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private readonly WorkflowRunPlanner _planner;
    private readonly IProcessBoundaryPort _processBoundary;

    // Belge ve surec sinirlarini plan ile yorum sahibi Manager'a baglar.
    public WorkflowRunnerService(WorkflowRunPlanner planner, IProcessBoundaryPort processBoundary)
    {
        _planner = planner;
        _processBoundary = processBoundary;
    }

    // Belgeyi wire DTO'suna cozup domain modeline map eder ve olgu cikarimini Manager'a birakir.
    public WorkflowDocumentFacts ReadDocumentFacts(string document)
    {
        _planner.EnsureDocumentCanBeRead(document);
        try
        {
            return _planner.ReadFacts(Mapper.Map(
                Deserializer.Deserialize<ArazzoDocumentDto>(document) ?? new ArazzoDocumentDto()));
        }
        catch (YamlException exception)
        {
            _planner.ThrowInvalidDocument(exception);
            return default!;
        }
    }

    // Plani Manager'dan alir, ortak surec sinirinda kosar ve yorumu yine Manager'a birakir.
    public async Task<WorkflowRunOutcome> ExecuteAsync(
        WorkflowRunRequest request,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planner.CreatePlanAsync(request, cancellationToken);
        return _planner.Interpret(
            await _processBoundary.ExecuteAsync(plan.Process, cancellationToken),
            plan);
    }
}
