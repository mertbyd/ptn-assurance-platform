using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.ExceptionCodes.Catalog;
using Ptn.TestModule.Interface.Catalog;
using Ptn.TestModule.Models.Catalog;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Catalog;

// islevi: Senaryo zamanlamasinin normalizasyonunu, vade hesabini ve surumler arasi tasinmasini yonetir.
// sistemdeki gorevi: Zamanlama yalniz yayinlanmis surumun alanidir ve iki surum ayni anda vadeli olamaz (PLAN-0003 TM-29).
/// <summary>
/// Senaryo zamanlama kurallarini ve UTC vade hesabini uygular.
/// </summary>
public class ScenarioScheduleManager : TestModuleDomainService
{
    private readonly ITestScenarioRepository _repository;

    public ScenarioScheduleManager(ITestScenarioRepository repository)
    {
        _repository = repository;
    }

    // Vadeyi kuyruklama sonrasi ilerletir; okuma ve yazma tik basina tek sorgudur, senaryo basina acilmaz.
    /// <summary>Kuyruklanan senaryolarin vadesini bir sonraki cron anina tasir ve topluca kaydeder.</summary>
    public async Task AdvanceManyAsync(
        IReadOnlyCollection<Guid> scenarioIds,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scenarioIds);
        var scenarios = await _repository.GetManyForScheduleAdvanceAsync(scenarioIds, cancellationToken);
        foreach (var scenario in scenarios)
        {
            Advance(scenario, now);
        }

        await _repository.UpdateManyAsync(scenarios, autoSave: true, cancellationToken: cancellationToken);
    }

    // Ayni vadenin iki kez kosum uretmesini engelleyen kararli tetikleyici referansini kurar.
    /// <summary>Senaryo ve vade anindan kararli tetikleyici referansi uretir.</summary>
    public static string CreateTriggerRef(Guid scenarioId, DateTime? dueAt)
    {
        return string.Concat(
            scenarioId.ToString("D"),
            TestScenarioConsts.ScheduleTriggerRefSeparator,
            (dueAt ?? DateTime.UnixEpoch).ToString("O", CultureInfo.InvariantCulture));
    }

    // Cron ifadesini dogrular, kanoniklestirir ve ilk vadeyi satira yazar.
    /// <summary>Yayinlanmis surume zamanlamayi uygular ve sonraki vadeyi hesaplar.</summary>
    public TestScenario Apply(TestScenario scenario, TestScenarioScheduleModel model, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(model);
        if (!model.ScheduleEnabled)
        {
            Clear(scenario);
            return scenario;
        }

        var expression = NormalizeCron(model.ScheduleCron);
        scenario.ScheduleCron = expression;
        scenario.ScheduleEnabled = true;
        scenario.NextRunAt = ComputeNextRunAt(expression, now);
        return scenario;
    }

    // Vade dolunca ayni cron uzerinden bir sonraki calismayi belirler.
    /// <summary>Kosum kuyruklandiktan sonra siradaki vadeyi ilerletir.</summary>
    public void Advance(TestScenario scenario, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        if (!scenario.ScheduleEnabled || string.IsNullOrWhiteSpace(scenario.ScheduleCron))
        {
            Clear(scenario);
            return;
        }

        scenario.NextRunAt = ComputeNextRunAt(scenario.ScheduleCron, now);
    }

    // Yeni surum yayinlandiginda zamanlamayi onceki yayinlanmis surumden devralir.
    /// <summary>Zamanlamayi onceki yayinlanmis surumden yeni surume tasir.</summary>
    public static void Transfer(TestScenario? previous, TestScenario current)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (previous is null || previous.Id == current.Id || !previous.ScheduleEnabled)
        {
            return;
        }

        current.ScheduleCron = previous.ScheduleCron;
        current.ScheduleEnabled = previous.ScheduleEnabled;
        current.NextRunAt = previous.NextRunAt;
        Clear(previous);
    }

    // Zamanlamayi tum alanlariyla kapatir; kapali zamanlama vade tasimaz.
    /// <summary>Senaryonun zamanlamasini tamamen temizler.</summary>
    public static void Clear(TestScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        scenario.ScheduleCron = null;
        scenario.ScheduleEnabled = false;
        scenario.NextRunAt = null;
    }

    // Cron ifadesini bosluklarindan arindirip Cronos'un kabul ettigi bicimde dogrular.
    /// <summary>Cron ifadesini kanonik bicime getirir ve ayristirilabilirligini dogrular.</summary>
    private static string NormalizeCron(string? scheduleCron)
    {
        var normalized = scheduleCron?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.ScheduleCronRequired);
        }
        if (normalized.Length > TestScenarioConsts.MaxScheduleCronLength)
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.ScheduleCronInvalid);
        }

        return normalized;
    }

    // Cron ifadesini UTC olarak yorumlar ve verilen andan sonraki ilk calismayi dondurur.
    /// <summary>Verilen andan sonraki ilk cron vadesini UTC olarak hesaplar.</summary>
    public static DateTime ComputeNextRunAt(string scheduleCron, DateTime now)
    {
        var expression = Parse(scheduleCron);
        var next = expression.GetNextOccurrence(DateTime.SpecifyKind(now, DateTimeKind.Utc).ToUniversalTime());
        return next ?? throw new BusinessException(TestModuleScenarioErrorCodes.ScheduleHasNoFutureOccurrence)
            .WithData(nameof(scheduleCron), scheduleCron);
    }

    // Bes ve alti alanli ifadeleri kabul eder; ayristirilamayan ifadeyi kararli kodla reddeder.
    /// <summary>Cron metnini Cronos ifadesine cevirir.</summary>
    private static CronExpression Parse(string scheduleCron)
    {
        try
        {
            return CronExpression.Parse(scheduleCron, CronFormat.IncludeSeconds);
        }
        catch (CronFormatException)
        {
            return ParseStandard(scheduleCron);
        }
    }

    // Alti alanli deneme basarisizsa klasik bes alanli cron bicimini dener.
    /// <summary>Bes alanli standart cron bicimini ayristirir.</summary>
    private static CronExpression ParseStandard(string scheduleCron)
    {
        try
        {
            return CronExpression.Parse(scheduleCron, CronFormat.Standard);
        }
        catch (CronFormatException exception)
        {
            throw new BusinessException(
                TestModuleScenarioErrorCodes.ScheduleCronInvalid,
                innerException: exception)
                .WithData(nameof(scheduleCron), scheduleCron);
        }
    }
}
