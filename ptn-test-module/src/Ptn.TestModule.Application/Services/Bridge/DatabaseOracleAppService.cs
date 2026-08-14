using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Services.Assertions;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Mappers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Database;
using Volo.Abp;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Database checker assertion servisini Bridge portuna baglar.
// sistemdeki gorevi: Checker I/O'sunu Application katmaninda, DTO-model eslemesini Mapperly'de tutar.
[RemoteService(IsEnabled = false)]
public class DatabaseOracleAppService : TestModuleAppService, IDatabaseOraclePort
{
    private static readonly DatabaseOracleMapper Mapper = new();
    private readonly IDatabaseAssertionAppService _appService;
    private readonly DatabaseOracleManager _manager;

    // Database checker public servisini anti-corruption sinirina baglar.
    public DatabaseOracleAppService(IDatabaseAssertionAppService appService, DatabaseOracleManager manager)
    {
        _appService = appService;
        _manager = manager;
    }

    // Satir assertion sonucunu normalize Bridge modeline cevirir.
    public async Task<PtnAssertionResult> AssertRowAsync(PtnDatabaseAssertionRequest request, CancellationToken cancellationToken)
    {
        return _manager.Normalize(Mapper.Map(
            await _appService.AssertRowAsync(Mapper.Map(request), cancellationToken)));
    }

    // Count assertion sonucunu normalize Bridge modeline cevirir.
    public async Task<PtnAssertionResult> AssertCountAsync(PtnDatabaseAssertionRequest request, CancellationToken cancellationToken)
    {
        return _manager.Normalize(Mapper.Map(
            await _appService.AssertCountAsync(Mapper.Map(request), cancellationToken)));
    }

    // Absence assertion sonucunu normalize Bridge modeline cevirir.
    public async Task<PtnAssertionResult> AssertAbsentAsync(PtnDatabaseAssertionRequest request, CancellationToken cancellationToken)
    {
        return _manager.Normalize(Mapper.Map(
            await _appService.AssertAbsentAsync(Mapper.Map(request), cancellationToken)));
    }

    // Assertion listesini tek checker cagrisi uzerinden sirali Bridge sonuclarina cevirir.
    public async Task<IReadOnlyList<PtnAssertionResult>> AssertBatchAsync(
        IReadOnlyList<PtnDatabaseAssertionRequest> requests,
        CancellationToken cancellationToken)
    {
        var results = await _appService.AssertBatchAsync(requests.Select(Mapper.Map).ToList(), cancellationToken);
        return _manager.Normalize(results.Select(Mapper.Map).ToList());
    }

    // Checker projeksiyon ucu bulunmadigi icin kanit yoklugunu Unavailable olarak bildirir.
    public Task<PtnProjectionResult> ProjectAsync(PtnProjectionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_manager.CreateUnavailableProjection());
    }

}
