using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Models.Runs;

namespace Ptn.TestModule.Interface.Runs;

// islevi: HAR'in her adimini hakemlere dagitip terminal hukmu getiren yargi capability'sini tanimlar.
// sistemdeki gorevi: Uzak checker cagrisi ayrintisini kosum kararlarindan ayirir; hakem her zaman checker'dir (RULE-0005).
/// <summary>
/// Kosum ciktisini hakemlere dagitip terminal hukmu dondiren sozlesmedir.
/// </summary>
public interface IOracleDispatchPort
{
    // Her adimi kaynak hakemine gonderir, kirmiziyi teshise tasir ve terminal hukmu toplar.
    /// <summary>Kosum ciktisini yargilayip terminal hukmu ve teshisi getirir.</summary>
    Task<TestRunJudgement> JudgeAsync(
        TestRunExecutionContext context,
        WorkflowRunOutcome outcome,
        string? harBlobName,
        CancellationToken cancellationToken = default);
}
