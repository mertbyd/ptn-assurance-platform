using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Managers.SchemaDiscovery;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Diagnosis.Probes;

// islevi: Izinli PostgreSQL server setting'ini pg_settings LINQ yolundan okuyup beklenen katalog baglamiyla karsilastirir.
// sistemdeki gorevi: search_path veya collation farkini ham SQL, serbest setting adi ya da yazma yetenegi olmadan kanita cevirir.
[ExposeServices(typeof(IDiagnosisProbe))]
public sealed class ServerSettingProbe : IDiagnosisProbe, ITransientDependency
{
    private readonly SchemaDiscoveryManager _schemaDiscoveryManager;
    private readonly FindingValueRedactor _redactor;

    // islevi: Probe'u mevcut schema repository setting okuyucusu ve tek redactor ile kurar.
    public ServerSettingProbe(
        SchemaDiscoveryManager schemaDiscoveryManager,
        FindingValueRedactor redactor)
    {
        _schemaDiscoveryManager = schemaDiscoveryManager;
        _redactor = redactor;
    }

    public string ProbeKindCode => ProbeKindCodes.ServerSetting;

    // islevi: Katalog setting degerini okuyup Matches/Mismatch ve redaction uygulanmis observed value kaniti dondurur.
    public async Task<ProbeEvidence> RunAsync(
        DatabaseConnection connection,
        ProbeRequest request,
        ValueRetentionPolicy retentionPolicy,
        CancellationToken cancellationToken = default)
    {
        var observed = await _schemaDiscoveryManager.ReadSettingAsync(
            connection,
            request.SettingName!,
            cancellationToken);
        var matches = MatchesExpected(request.SettingName!, observed, request.ExpectedSettingValue);
        return new ProbeEvidence
        {
            ProbeKindCode = ProbeKindCode,
            HypothesisKindCode = request.HypothesisKindCode,
            FactCode = matches ? ProbeKindCodes.Facts.Matches : ProbeKindCodes.Facts.Mismatch,
            ExpectedValue = _redactor.Redact(request.ExpectedSettingValue, retentionPolicy),
            ObservedValue = _redactor.Redact(observed, retentionPolicy)
        };
    }

    // islevi: search_path listesini sema uyeligine, diger izinli setting'i ordinal esitlige yonlendirir.
    private static bool MatchesExpected(string settingName, string? observed, string? expected)
        => settingName == PostgreSqlSqlStateCodes.SettingNames.SearchPath
            ? SearchPathContains(observed, expected)
            : string.Equals(observed, expected, StringComparison.OrdinalIgnoreCase);

    // islevi: Virgul ayrimli search_path icinde beklenen semayi quote ve bosluklardan arindirarak arar.
    private static bool SearchPathContains(string? searchPath, string? schemaName)
        => !string.IsNullOrWhiteSpace(schemaName) &&
           (searchPath ?? string.Empty)
           .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
           .Select(item => item.Trim('"'))
           .Any(item => string.Equals(item, schemaName, StringComparison.OrdinalIgnoreCase));
}
