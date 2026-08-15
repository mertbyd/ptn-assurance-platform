using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Models.Runs;

namespace Ptn.TestModule.Interface.Runs;

// islevi: Arazzo belgesini okuyan ve is akisini pinli dis runner'da icra eden capability'yi tanimlar.
// sistemdeki gorevi: Belge bicimi ve process ayrintisini Domain kararlarindan ayirir; kendi HTTP motorumuzu yazmayi engeller (ADR-0015 §A).
/// <summary>
/// Kosum belgesini okuyup dogrulanmis bir istegi dis runner surecinde calistiran sozlesmedir.
/// </summary>
public interface IWorkflowRunnerPort
{
    // Belgeyi hukum vermeden okur; kabul, ret ve baglama kurallari Manager'da kalir.
    /// <summary>Verilen Arazzo belgesinin kosulabilirlik olgularini getirir.</summary>
    WorkflowDocumentFacts ReadDocumentFacts(string document);

    // Belgeyi pinli runner imajinda kosar ve HAR ile JSON artefaktini geri verir.
    /// <summary>Kosum istegini dis runner surecinde icra edip ciktisini dondurur.</summary>
    Task<WorkflowRunOutcome> ExecuteAsync(
        WorkflowRunRequest request,
        CancellationToken cancellationToken = default);
}
