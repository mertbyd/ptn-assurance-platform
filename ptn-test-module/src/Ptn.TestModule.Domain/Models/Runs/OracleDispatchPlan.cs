using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: HAR adim dagitimlarini ve runner on-kapi hukmunu tek planda tasir.
// sistemdeki gorevi: Oracle servisinin yalniz planlanmis checker I/O'sunu yapmasini saglar.
public sealed class OracleDispatchPlan
{
    public IReadOnlyList<OracleStepDispatch> Steps { get; set; } = [];

    public StepJudgement RunnerJudgement { get; set; } = new();
}
