using Ptn.DatabaseChecker.Application.Mappers.Assertions;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Interface.Connections;
using Ptn.DatabaseChecker.Managers.Assertions;
using Ptn.DatabaseChecker.Services.Assertions;
using Volo.Abp;
using Volo.Abp.Uow;

namespace Ptn.DatabaseChecker.Application.Services.Assertions;

// islevi: DB assertion derivability use-case'ini kayitli baglanti, Mapperly ve domain kapisi uzerinden orkestre eder.
// sistemdeki gorevi: Uzun katalog okumasinda UOW tutmayan ince Application katmani akisidir.
[RemoteService(IsEnabled = false)]
[UnitOfWork(IsDisabled = true)]
public class AssertionDerivabilityAppService
    : DatabaseCheckerAppService, IAssertionDerivabilityAppService
{
    private static readonly DerivabilityMapper Mapper = new();

    private AssertionDerivabilityManager Manager
        => LazyServiceProvider.LazyGetRequiredService<AssertionDerivabilityManager>();

    private IDatabaseConnectionRepository ConnectionRepository
        => LazyServiceProvider.LazyGetRequiredService<IDatabaseConnectionRepository>();

    // islevi: Baglantiyi yukler, request'i mapler, domain kapisini calistirir ve sonucu mapler.
    public async Task<DerivabilityResultDto> ValidateDerivabilityAsync(
        DerivabilityRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var connection = await ConnectionRepository.GetWithDetailsAsync(
            input.ConnectionId, cancellationToken);
        var request = Mapper.MapToRequest(input);
        var result = await Manager.ValidateAsync(connection, request, cancellationToken);
        return Mapper.MapToResultDto(result);
    }
}
