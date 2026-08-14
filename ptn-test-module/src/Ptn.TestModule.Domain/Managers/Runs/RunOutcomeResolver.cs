using System;
using System.Collections.Generic;
using System.Linq;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Adim hukumlerini, malzeme kaymasini ve beklenmeyen hatalari tek terminal kosum hukmune cevirir.
// sistemdeki gorevi: Hukum, hata kategorisi ve motor durumu esleminin tek domain sahibidir (ADR-0015 §E, ADR-0016 §I).
/// <summary>
/// Bir kosumun terminal hukmunu ve motor durumunu belirler.
/// </summary>
public class RunOutcomeResolver : TestModuleDomainService
{
    // Adim hukumlerinden kosu hukmunu turetir ve birincil basarisiz adimi rapora tasir.
    /// <summary>Oracle dagitim sonucunu terminal kosum hukmune cevirir.</summary>
    public TestRunJudgement Resolve(OracleDispatchResult dispatch, string? harBlobName)
    {
        ArgumentNullException.ThrowIfNull(dispatch);
        var primary = SelectPrimary(dispatch.Judgements);
        var outcomeCode = ResolveOutcomeCode(dispatch.Judgements);

        return new TestRunJudgement
        {
            Terminal = CreateTerminal(outcomeCode, primary, dispatch),
            HarBlobName = harBlobName
        };
    }

    // Kayan malzeme ana yolun hic kosmadigini gosterir; hukum Failed degil Inconclusive'dir (ADR-0020 §C).
    /// <summary>Malzeme kaymasini Inconclusive terminal hukmune cevirir.</summary>
    public TestRunJudgement ResolveMaterialDrift(TestRunMaterialDrift drift)
    {
        ArgumentNullException.ThrowIfNull(drift);
        if (!drift.HasDrift)
        {
            throw new BusinessException(TestModuleRunErrorCodes.MaterialDriftRequired);
        }

        return CreateFailureJudgement(
            TestOutcomeStatusCodes.Inconclusive,
            TestFailureCategoryCodes.Technical,
            TestModuleRunErrorCodes.MaterialDriftDetected,
            TestRunStatusCodes.Aborted,
            string.Join(",", drift.DriftedMaterialCodes));
    }

    // Iptali teknik hatadan ayirir, bilinen domain kodunu korur, bilinmeyeni kararli koda indirger.
    /// <summary>Icra veya yargi asamasindaki hatayi terminal hukme cevirir.</summary>
    public TestRunJudgement ResolveFailure(Exception? exception)
    {
        var cause = Unwrap(exception);
        if (cause is OperationCanceledException)
        {
            return CreateFailureJudgement(
                TestOutcomeStatusCodes.Inconclusive,
                failureCategoryCode: null,
                TestModuleRunErrorCodes.RunCancelled,
                TestRunStatusCodes.Cancelled,
                detail: null);
        }

        return CreateFailureJudgement(
            TestOutcomeStatusCodes.Broken,
            TestFailureCategoryCodes.Technical,
            ResolveErrorCode(cause),
            ResolveRunStatusCode(cause),
            detail: null);
    }

    // Kirmizi varsa Failed, yoksa belirsizlik varsa Inconclusive, aksi halde Passed hukmu verir.
    /// <summary>Adim hukumlerinden kosu hukum kodunu turetir.</summary>
    private static string ResolveOutcomeCode(IReadOnlyList<StepJudgement> judgements)
    {
        if (judgements.Any(judgement => judgement.OutcomeCode == TestOutcomeStatusCodes.Failed))
        {
            return TestOutcomeStatusCodes.Failed;
        }

        return judgements.Any(judgement => judgement.OutcomeCode == TestOutcomeStatusCodes.Inconclusive)
            ? TestOutcomeStatusCodes.Inconclusive
            : TestOutcomeStatusCodes.Passed;
    }

    // Raporun basarisiz adim alanlarini dolduracak ilk olumsuz hukmu secer.
    /// <summary>Raporlanacak birincil olumsuz adim hukmunu getirir.</summary>
    private static StepJudgement? SelectPrimary(IReadOnlyList<StepJudgement> judgements)
    {
        return judgements.FirstOrDefault(judgement => judgement.OutcomeCode == TestOutcomeStatusCodes.Failed) ??
               judgements.FirstOrDefault(judgement => judgement.OutcomeCode == TestOutcomeStatusCodes.Inconclusive);
    }

    // Passed hukmunde hicbir sorun alani tasinmaz; aksi halde birincil adim rapora yazilir.
    /// <summary>Terminal hukum modelini bulgular ve teshis raporuyla kurar.</summary>
    private static TestRunTerminalModel CreateTerminal(
        string outcomeCode,
        StepJudgement? primary,
        OracleDispatchResult dispatch)
    {
        if (outcomeCode == TestOutcomeStatusCodes.Passed || primary is null)
        {
            return new TestRunTerminalModel
            {
                OutcomeCode = TestOutcomeStatusCodes.Passed,
                RunStatusCode = TestRunStatusCodes.Completed,
                Findings = dispatch.Findings.ToList()
            };
        }

        return new TestRunTerminalModel
        {
            OutcomeCode = outcomeCode,
            RunStatusCode = TestRunStatusCodes.Completed,
            FailureCategoryCode = primary.FailureCategoryCode,
            ErrorCode = primary.ErrorCode ?? primary.CheckerOutcomeCode,
            FailedStepOrdinal = primary.Entry.Ordinal > 0 ? primary.Entry.Ordinal : null,
            FailedStepName = primary.Entry.StepKey,
            FailedStepPath = primary.Entry.Url,
            LastCompletedOrdinal = ResolveLastCompletedOrdinal(dispatch.Judgements, primary),
            DiagnosisReport = dispatch.DiagnosisReport,
            Findings = dispatch.Findings.ToList()
        };
    }

    // Birincil basarisizliktan once gecen son adimin sirasini raporlar.
    /// <summary>Basarisizliktan onceki son gecen adimin sirasini getirir.</summary>
    private static int? ResolveLastCompletedOrdinal(
        IReadOnlyList<StepJudgement> judgements,
        StepJudgement primary)
    {
        var completed = judgements
            .Where(judgement =>
                judgement.OutcomeCode == TestOutcomeStatusCodes.Passed &&
                judgement.Entry.Ordinal < primary.Entry.Ordinal)
            .Select(judgement => judgement.Entry.Ordinal)
            .ToList();
        return completed.Count > 0 ? completed.Max() : null;
    }

    // Hukum, kategori, kod ve motor durumunu tasiyan bulgusuz terminal modeli kurar.
    /// <summary>Bulgu tasimayan terminal hukum modelini kurar.</summary>
    private static TestRunJudgement CreateFailureJudgement(
        string outcomeCode,
        string? failureCategoryCode,
        string errorCode,
        string runStatusCode,
        string? detail)
    {
        return new TestRunJudgement
        {
            Terminal = new TestRunTerminalModel
            {
                OutcomeCode = outcomeCode,
                RunStatusCode = runStatusCode,
                FailureCategoryCode = failureCategoryCode,
                ErrorCode = errorCode,
                Detail = detail
            }
        };
    }

    // Job'in yakaladigi toplu hatayi tek gercek nedene indirger.
    /// <summary>Sarmalanmis hatanin ilk gercek nedenini getirir.</summary>
    private static Exception? Unwrap(Exception? exception)
    {
        return exception is AggregateException aggregate
            ? aggregate.Flatten().InnerExceptions.FirstOrDefault()
            : exception;
    }

    // Bilinen domain kodunu korur; bilinmeyen hatayi ham ayrinti sizdirmadan kararli koda indirger.
    /// <summary>Hatanin kalici hata kodunu getirir.</summary>
    private static string ResolveErrorCode(Exception? exception)
    {
        return exception is BusinessException { Code: not null } business
            ? business.Code
            : TestModuleRunErrorCodes.RunFailedUnexpectedly;
    }

    // Zaman asimini motorun coktugu durumdan ayirir.
    /// <summary>Hataya karsilik gelen terminal motor durumunu getirir.</summary>
    private static string ResolveRunStatusCode(Exception? exception)
    {
        return exception is BusinessException { Code: TestModuleRunErrorCodes.RunnerTimedOut }
            ? TestRunStatusCodes.TimedOut
            : TestRunStatusCodes.Aborted;
    }
}
