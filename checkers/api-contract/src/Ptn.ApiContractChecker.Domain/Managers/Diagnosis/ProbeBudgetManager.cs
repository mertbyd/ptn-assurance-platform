using Ptn.ApiContractChecker.ExceptionCodes.Diagnosis;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Ptn.ApiContractChecker.Settings;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Ptn.ApiContractChecker.Diagnostics;
using Ptn.ApiContractChecker.Constants.Diagnostics;

namespace Ptn.ApiContractChecker.Managers.Diagnosis;

// islevi: Probe adet, toplam sure ve probe timeout ayarlarini tenant-aware setting zincirinden uygular.
// sistemdeki gorevi: Teshisin ikinci kesinti olmasini engelleyip timeout veya probe hatasinda kismi kanit dondurur.
public sealed class ProbeBudgetManager : ITransientDependency
{
    private readonly ISettingProvider _settingProvider;
    private readonly IClock _clock;
    private readonly IReadOnlyDictionary<string, IDiagnosisProbe> _probes;

    public ProbeBudgetManager(ISettingProvider settingProvider, IClock clock, IEnumerable<IDiagnosisProbe> probes)
    {
        _settingProvider = settingProvider;
        _clock = clock;
        _probes = probes.ToDictionary(item => item.ProbeKindCode, StringComparer.Ordinal);
    }

    // islevi: Probe isteklerini kararli sirada adet ve sure tavani icinde calistirir.
    public async Task<List<ProbeEvidence>> RunAsync(
        List<ProbeRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var maxCount = await ReadPositiveAsync(ApiContractCheckerSettings.Diagnosis.MaxProbeCount,
            ApiContractCheckerSettings.Diagnosis.DefaultMaxProbeCount);
        var maxDuration = await ReadPositiveAsync(ApiContractCheckerSettings.Diagnosis.MaxProbeDurationMs,
            ApiContractCheckerSettings.Diagnosis.DefaultMaxProbeDurationMs);
        var timeout = await ReadPositiveAsync(ApiContractCheckerSettings.Diagnosis.ProbeTimeoutMs,
            ApiContractCheckerSettings.Diagnosis.DefaultProbeTimeoutMs);
        return await RunWithinBudgetAsync(requests, maxCount, maxDuration, timeout, cancellationToken);
    }

    // islevi: Raporun tenant-aware azami hipotez sayisini cozer.
    public Task<int> ResolveMaxHypothesesAsync()
        => ReadPositiveAsync(ApiContractCheckerSettings.Diagnosis.MaxHypotheses,
            ApiContractCheckerSettings.Diagnosis.DefaultMaxHypotheses);

    // islevi: Sirali istekleri kalan toplam sure ve adet icinde calistirip kismi listeyi korur.
    private async Task<List<ProbeEvidence>> RunWithinBudgetAsync(
        List<ProbeRequest> requests,
        int maxCount,
        int maxDurationMs,
        int timeoutMs,
        CancellationToken cancellationToken)
    {
        var evidence = new List<ProbeEvidence>();
        var startedAt = _clock.Now;
        foreach (var request in requests.Take(maxCount))
        {
            var remaining = maxDurationMs - (int)Math.Max(0, (_clock.Now - startedAt).TotalMilliseconds);
            var result = await RunOneAsync(request, Math.Min(remaining, timeoutMs), cancellationToken);
            if (result is null)
            {
                break;
            }

            evidence.Add(result);
        }

        return evidence;
    }

    // islevi: Tek probe'u timeout ile yaristirir; probe hatasi veya timeout'ta null dondurur.
    private async Task<ProbeEvidence?> RunOneAsync(
        ProbeRequest request,
        int timeoutMs,
        CancellationToken cancellationToken)
    {
        if (timeoutMs <= 0)
        {
            return null;
        }

        using var activity = ApiContractCheckerActivity.Start(
            ApiContractCheckerDiagnostics.DiagnosisProbeSpan,
            ApiContractCheckerDiagnostics.MomentDiagnosis);
        var probe = ResolveProbe(request.ProbeKindCode);
        using var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var probeTask = probe.RunAsync(request, source.Token);
        var completed = await Task.WhenAny(probeTask, Task.Delay(timeoutMs, cancellationToken));
        if (completed != probeTask)
        {
            source.Cancel();
            activity?.SetTag(
                ApiContractCheckerDiagnostics.ErrorTypeAttribute,
                ApiContractCheckerDiagnostics.TimeoutErrorType);
            return null;
        }

        try
        {
            return await probeTask;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            activity?.SetTag(
                ApiContractCheckerDiagnostics.ErrorTypeAttribute,
                ApiContractCheckerDiagnostics.TimeoutErrorType);
            return null;
        }
        catch when (!cancellationToken.IsCancellationRequested)
        {
            activity?.SetTag(
                ApiContractCheckerDiagnostics.ErrorTypeAttribute,
                ApiContractCheckerDiagnostics.ProbeErrorType);
            return null;
        }
    }

    // islevi: Probe kodunu DI koleksiyonundaki tek implementasyona cozer.
    private IDiagnosisProbe ResolveProbe(string probeKindCode)
        => _probes.GetValueOrDefault(probeKindCode)
           ?? throw new BusinessException(DiagnosisExceptionCodes.ProbeNotFound);

    // islevi: Pozitif integer setting degerini tenant zincirinden cozer.
    private async Task<int> ReadPositiveAsync(string name, int defaultValue)
    {
        var value = await _settingProvider.GetAsync(name, defaultValue);
        return value > 0
            ? value
            : throw new BusinessException(DiagnosisExceptionCodes.InvalidSetting)
                .WithData(BusinessExceptionDataKeys.SettingName, name);
    }
}
