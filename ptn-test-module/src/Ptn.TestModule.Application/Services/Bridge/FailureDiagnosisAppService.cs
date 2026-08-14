using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Mappers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Volo.Abp;
using ApiDiagnosisService = Ptn.ApiContractChecker.Services.Diagnosis.IDiagnosisAppService;
using DatabaseDiagnosisService = Ptn.DatabaseChecker.Services.Diagnosis.IDiagnosisAppService;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Iki checker diagnosis servisini ortak Bridge portunda birlestirir.
// sistemdeki gorevi: Checker cagrisi ve Mapperly orkestrasyonunu yapip tum kararlari Manager'a birakir.
[RemoteService(IsEnabled = false)]
public class FailureDiagnosisAppService : TestModuleAppService, IFailureDiagnosisPort
{
    private static readonly FailureDiagnosisMapper Mapper = new();
    private readonly ApiDiagnosisService _apiDiagnosisService;
    private readonly DatabaseDiagnosisService _databaseDiagnosisService;
    private readonly FailureDiagnosisManager _manager;

    // Iki checker public diagnosis servisini ve ortak kural sahibini baglar.
    public FailureDiagnosisAppService(
        ApiDiagnosisService apiDiagnosisService,
        DatabaseDiagnosisService databaseDiagnosisService,
        FailureDiagnosisManager manager)
    {
        _apiDiagnosisService = apiDiagnosisService;
        _databaseDiagnosisService = databaseDiagnosisService;
        _manager = manager;
    }

    // API checker cagrisini map eder ve Manager'in normalize ettigi ortak raporu dondurur.
    public async Task<PtnDiagnosisReport> DiagnoseApiAsync(
        PtnDiagnosisRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var source = await _apiDiagnosisService.DiagnoseAsync(Mapper.Map(_manager.CreateApiRequest(request)));
        var location = _manager.CreateApiLocation(
            source.Location.SchemaName,
            source.Location.OperationId,
            source.Location.Method,
            source.Location.Path,
            source.Location.JsonPointer);
        return _manager.NormalizeApiReport(Mapper.Map(source), location, Mapper.Map(source.Identity));
    }

    // Database checker union cagrisini map eder ve Manager'in normalize ettigi ortak raporu dondurur.
    public async Task<PtnDiagnosisReport> DiagnoseDatabaseAsync(
        PtnDiagnosisRequest request,
        CancellationToken cancellationToken)
    {
        var source = await _databaseDiagnosisService.DiagnoseAsync(
            Mapper.Map(_manager.CreateDatabaseRequest(request)),
            cancellationToken);
        var location = _manager.CreateDatabaseLocation(
            source.Location.SchemaName,
            source.Location.TableName,
            source.Location.ColumnName);
        return _manager.NormalizeDatabaseReport(Mapper.Map(source), location);
    }
}
