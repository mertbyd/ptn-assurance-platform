using Microsoft.Extensions.Options;

namespace Ptn.ApiContractChecker.Configuration;

// islevi: Zamanlanmis izleme worker'inin periyot ve tik basi dokuman esiklerinin uygulama ayaga kalkarken anlamli oldugunu dogrular.
// sistemdeki gorevi: Hicbir zaman tetiklenmeyen ya da her tikte tum kaynaklari doven yapilandirmayi ilk tiki beklemeden durdurur.
public class SpecMonitoringOptionsValidator : IValidateOptions<SpecMonitoringOptions>
{
    private const string InvalidWorkerPeriod =
        "SpecMonitoring:WorkerPeriodSeconds must be between 1 and 3600.";

    private const string InvalidMaxDocumentsPerTick =
        "SpecMonitoring:MaxDocumentsPerTick must be greater than zero.";

    // Iki esigi de ayni calistirmada denetler ve bulunan tum ihlalleri birlikte bildirir.
    public ValidateOptionsResult Validate(string? name, SpecMonitoringOptions options)
    {
        var failures = new List<string>();

        if (options.WorkerPeriodSeconds is < 1 or > SpecMonitoringOptions.MaxWorkerPeriodSeconds)
        {
            failures.Add(InvalidWorkerPeriod);
        }

        if (options.MaxDocumentsPerTick <= 0)
        {
            failures.Add(InvalidMaxDocumentsPerTick);
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
