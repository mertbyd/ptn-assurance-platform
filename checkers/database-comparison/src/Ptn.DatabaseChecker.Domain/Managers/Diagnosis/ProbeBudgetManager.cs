using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Ptn.DatabaseChecker.Settings;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Volo.Abp.Timing;

namespace Ptn.DatabaseChecker.Managers.Diagnosis;

// islevi: Probe adet, toplam sure, statement timeout ve hipotez setting tavanlarini tenant-aware zincirden uygular.
// sistemdeki gorevi: Salt-okuma probe'larini butce asiminda exception uretmeden durdurup kismi kanit listesini dondurur.
public sealed class ProbeBudgetManager : ITransientDependency
{
    private readonly ISettingProvider _settingProvider;
    private readonly IClock _clock;
    private readonly IReadOnlyDictionary<string, IDiagnosisProbe> _probes;

    // islevi: Butce yoneticisini ABP setting/clock ve conventional DI probe koleksiyonuyla kurar.
    public ProbeBudgetManager(
        ISettingProvider settingProvider,
        IClock clock,
        IEnumerable<IDiagnosisProbe> probes)
    {
        _settingProvider = settingProvider;
        _clock = clock;
        _probes = probes.ToDictionary(item => item.ProbeKindCode, StringComparer.Ordinal);
    }

    // islevi: Tum probe isteklerini kararli sirada adet ve sure tavani icinde calistirip kismi kanit dondurur.
    public async Task<List<ProbeEvidence>> RunAsync(
        DatabaseConnection connection,
        List<ProbeRequest> requests,
        ValueRetentionPolicy retentionPolicy,
        CancellationToken cancellationToken = default)
    {
        var maxCount = await ReadPositiveAsync(
            DatabaseCheckerSettings.Diagnosis.MaxProbeCount,
            DatabaseCheckerSettings.Diagnosis.DefaultMaxProbeCount);
        var maxDurationMs = await ReadPositiveAsync(
            DatabaseCheckerSettings.Diagnosis.MaxDurationMs,
            DatabaseCheckerSettings.Diagnosis.DefaultMaxDurationMs);
        var statementTimeoutMs = await ReadPositiveAsync(
            DatabaseCheckerSettings.Diagnosis.ProbeStatementTimeoutMs,
            DatabaseCheckerSettings.Diagnosis.DefaultProbeStatementTimeoutMs);
        return await RunWithinBudgetAsync(
            connection, requests, retentionPolicy,
            maxCount, maxDurationMs, statementTimeoutMs, cancellationToken);
    }

    // islevi: Rapor siralamasinin tenant-aware azami hipotez sayisini cozer.
    public Task<int> ResolveMaxHypothesesAsync()
        => ReadPositiveAsync(
            DatabaseCheckerSettings.Diagnosis.MaxHypotheses,
            DatabaseCheckerSettings.Diagnosis.DefaultMaxHypotheses);

    // islevi: Sirali probe'lari kalan adet/toplam sure icinde calistirir; ilk timeout'ta kismi sonucu dondurur.
    private async Task<List<ProbeEvidence>> RunWithinBudgetAsync(
        DatabaseConnection connection,
        List<ProbeRequest> requests,
        ValueRetentionPolicy retentionPolicy,
        int maxCount,
        int maxDurationMs,
        int statementTimeoutMs,
        CancellationToken cancellationToken)
    {
        var evidence = new List<ProbeEvidence>();
        var startedAt = _clock.Now;
        foreach (var request in requests.GetRange(0, Math.Min(maxCount, requests.Count)))
        {
            var remainingMs = RemainingMilliseconds(startedAt, maxDurationMs);
            if (remainingMs <= 0)
            {
                break;
            }

            var proof = await RunOneAsync(
                connection, request, retentionPolicy,
                Math.Min(remainingMs, statementTimeoutMs), cancellationToken);
            if (proof is null)
            {
                break;
            }

            evidence.Add(proof);
        }

        return evidence;
    }

    // islevi: Tek probe'u statement/kalan sure yarisi ile yaristirir; timeout'ta iptal edip exception yerine null dondurur.
    private async Task<ProbeEvidence?> RunOneAsync(
        DatabaseConnection connection,
        ProbeRequest request,
        ValueRetentionPolicy retentionPolicy,
        int timeoutMs,
        CancellationToken cancellationToken)
    {
        var probe = ResolveProbe(request.ProbeKindCode);
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var probeTask = probe.RunAsync(connection, request, retentionPolicy, timeoutSource.Token);
        var timeoutTask = Task.Delay(timeoutMs, cancellationToken);
        var completedTask = await Task.WhenAny(probeTask, timeoutTask);
        cancellationToken.ThrowIfCancellationRequested();
        if (completedTask == probeTask)
        {
            return await probeTask;
        }

        timeoutSource.Cancel();
        ObserveFault(probeTask);
        return null;
    }

    // islevi: DI koleksiyonunda istenen probe turunu bulur veya kararli BusinessException uretir.
    private IDiagnosisProbe ResolveProbe(string probeKindCode)
        => _probes.GetValueOrDefault(probeKindCode)
           ?? throw new BusinessException(DiagnosisExceptionCodes.ProbeNotFound);

    // islevi: Timeout sonrasi arka planda tamamlanan probe fault'unun unobserved exception olmasini engeller.
    private static void ObserveFault(Task<ProbeEvidence> probeTask)
    {
        _ = probeTask.ContinueWith(
            static task => _ = task.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    // islevi: ABP saatine gore toplam probe butcesinde kalan tam milisaniyeyi hesaplar.
    private int RemainingMilliseconds(DateTime startedAt, int maxDurationMs)
        => maxDurationMs - (int)Math.Max(0, (_clock.Now - startedAt).TotalMilliseconds);

    // islevi: Tek integer setting'i tenant zincirinden okuyup pozitiflik kuralini uygular.
    private async Task<int> ReadPositiveAsync(string name, int defaultValue)
    {
        var value = await _settingProvider.GetAsync(name, defaultValue);
        if (value <= 0)
        {
            throw new BusinessException(DiagnosisExceptionCodes.InvalidSetting)
                .WithData("SettingName", name);
        }

        return value;
    }
}
