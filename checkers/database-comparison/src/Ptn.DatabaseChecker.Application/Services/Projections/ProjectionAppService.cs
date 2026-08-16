using Ptn.DatabaseChecker.Application.Mappers.Projections;
using Ptn.DatabaseChecker.Dtos.Projections;
using Ptn.DatabaseChecker.Interface.Connections;
using Ptn.DatabaseChecker.Managers.Projections;
using Ptn.DatabaseChecker.Services.Projections;
using Volo.Abp;
using Volo.Abp.Uow;

namespace Ptn.DatabaseChecker.Application.Services.Projections;

// islevi: Salt-okunur projection use-case'ini kayitli baglanti, Mapperly ve ProjectionManager uzerinden orkestre eder.
// sistemdeki gorevi: Uzun hedef DB okumasinda UOW tutmayan ince Application katmani akisidir.
[RemoteService(IsEnabled = false)]
[UnitOfWork(IsDisabled = true)]
public class ProjectionAppService : DatabaseCheckerAppService, IProjectionAppService
{
    private static readonly ProjectionMapper Mapper = new();

    private ProjectionManager Manager
        => LazyServiceProvider.LazyGetRequiredService<ProjectionManager>();

    private IDatabaseConnectionRepository ConnectionRepository
        => LazyServiceProvider.LazyGetRequiredService<IDatabaseConnectionRepository>();

    // islevi: Baglantiyi yukler, request'i mapler, projection manager'i calistirir ve sonucu mapler.
    public async Task<ProjectionResultDto> ProjectRowsAsync(
        ProjectionRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var connection = await ConnectionRepository.GetWithDetailsAsync(
            input.ConnectionId, cancellationToken);
        var request = Mapper.MapToRequest(input);
        var result = await Manager.ProjectAsync(connection, request, cancellationToken);
        return Mapper.MapToResultDto(result);
    }
}
