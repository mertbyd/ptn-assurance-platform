using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Application.Mappers.Assertions;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Interface.Connections;
using Ptn.DatabaseChecker.Managers.Assertions;
using Ptn.DatabaseChecker.Services.Assertions;
using Volo.Abp;
using Volo.Abp.Uow;

namespace Ptn.DatabaseChecker.Application.Services.Assertions;

// islevi: Row, count, absence ve batch assertion use-case'lerini kayitli baglanti uzerinden orkestre eder.
// sistemdeki gorevi: Repository baglantisi -> Mapperly request -> RowAssertionManager semantigi -> Mapperly result zinciridir; uzun hedef DB I/O'su acik UOW tutmaz.
[RemoteService(IsEnabled = false)]
[UnitOfWork(IsDisabled = true)]
public class DatabaseAssertionAppService : DatabaseCheckerAppService, IDatabaseAssertionAppService
{
    private static readonly DatabaseAssertionMapper Mapper = new();

    private RowAssertionManager Manager
        => LazyServiceProvider.LazyGetRequiredService<RowAssertionManager>();

    private IDatabaseConnectionRepository ConnectionRepository
        => LazyServiceProvider.LazyGetRequiredService<IDatabaseConnectionRepository>();

    // islevi: Row assertion'ini Exactly(1) cardinality ile ortak cekirdekte calistirir.
    public async Task<RowAssertionResultDto> AssertRowAsync(
        RowAssertionRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var connection = await ConnectionRepository.GetWithDetailsAsync(input.ConnectionId, cancellationToken);
        var request = Mapper.MapToRequest(input);
        var result = await Manager.AssertRowAsync(connection, request, cancellationToken);
        return Mapper.MapToResultDto(result);
    }

    // islevi: Request'teki cardinality beklentisini ortak cekirdekte calistirir.
    public async Task<RowAssertionResultDto> AssertCountAsync(
        RowAssertionRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var connection = await ConnectionRepository.GetWithDetailsAsync(input.ConnectionId, cancellationToken);
        var request = Mapper.MapToRequest(input);
        var result = await Manager.AssertCountAsync(connection, request, cancellationToken);
        return Mapper.MapToResultDto(result);
    }

    // islevi: Absence assertion'ini None cardinality ile ortak cekirdekte calistirir.
    public async Task<RowAssertionResultDto> AssertAbsentAsync(
        RowAssertionRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var connection = await ConnectionRepository.GetWithDetailsAsync(input.ConnectionId, cancellationToken);
        var request = Mapper.MapToRequest(input);
        var result = await Manager.AssertAbsentAsync(connection, request, cancellationToken);
        return Mapper.MapToResultDto(result);
    }

    // islevi: Batch boyutunu uygular, baglantilari tek metadata sorgusuyla okur ve her assertion sonucunu bagimsiz dondurur.
    public async Task<List<RowAssertionResultDto>> AssertBatchAsync(
        List<RowAssertionRequestDto> input,
        CancellationToken cancellationToken = default)
    {
        var requests = Mapper.MapToRequests(input);
        var connectionIds = requests.Select(request => request.ConnectionId).Distinct().ToList();
        var connections = await ConnectionRepository.GetWithDetailsByIdsAsync(connectionIds, cancellationToken);
        var results = await Manager.AssertBatchAsync(connections, requests, cancellationToken);
        return Mapper.MapToResultDtos(results);
    }
}
