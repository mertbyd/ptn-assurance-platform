using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Models.Runs;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Kuru kosumda kirmizi hukum varsa deterministik celiski bildirimi kurar.
// sistemdeki gorevi: Gozlemi degistirmeden ve ajana eylem onermeden RULE-0005 sinirini korur.
public class DryRunContradictionManager : TestModuleDomainService
{
    // Son kosum raporunu yonlendirme icermeyen celiski bildirimine cevirir.
    public DryRunContradictionReport Create(TestRunReport report)
    {
        var isRed = report.OutcomeCode is not null &&
                    report.OutcomeCode != TestOutcomeStatusCodes.Passed &&
                    report.OutcomeCode != TestOutcomeStatusCodes.Skipped;
        return new DryRunContradictionReport
        {
            IsDryRun = report.Run.IsDryRun,
            HasContradiction = report.Run.IsDryRun && isRed,
            Observation = report.OutcomeCode is null ? "No terminal observation" : $"Observed outcome: {report.OutcomeCode}",
            Contract = "The published scenario contract remains authoritative; dry-run does not change its verdict.",
            Location = report.Result?.FailedStepPath ?? report.Result?.FailedStepName ?? string.Empty,
            OutcomeCode = report.OutcomeCode
        };
    }
}
