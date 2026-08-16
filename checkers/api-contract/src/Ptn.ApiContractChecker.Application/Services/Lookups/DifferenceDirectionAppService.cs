using Ptn.ApiContractChecker.Application.Mappers.Lookups;
using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Models.Lookups;
using Ptn.ApiContractChecker.Services.Lookups;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Ptn.ApiContractChecker.Application.Services.Lookups;

// islevi: DifferenceDirection lookup okuma, olusturma, update ve pasiflestirme akisini orkestre eder.
// sistemdeki gorevi: Ortak lookup tabanina yalniz fark yonu entity kurulumu ve Mapperly baglantilarini verir.
[RemoteService(IsEnabled = false)]
public class DifferenceDirectionAppService
    : LookupAppServiceBase<DifferenceDirection, DifferenceDirectionDto, CreateDifferenceDirectionDto, UpdateDifferenceDirectionDto>,
      IDifferenceDirectionAppService
{
    private static readonly DifferenceDirectionMapper Mapper = new();

    // Yon repository baglantisini ortak lookup akisina verir.
    public DifferenceDirectionAppService(
        IAbpLazyServiceProvider provider,
        IRepository<DifferenceDirection, Guid> repository)
        : base(provider, repository)
    {
    }

    // Dogrulanmis create modelinden ABP kimlikli yeni fark yonu satiri kurar.
    protected override DifferenceDirection CreateEntity(LookupCreateModel model)
        => new(GuidGenerator.Create(), model.Code, model.Name, model.Description, model.IsActive);

    // Yon entity'sini dis sozlesme DTO'suna cevirir.
    protected override DifferenceDirectionDto MapToDto(DifferenceDirection entity) => Mapper.MapToDto(entity);
    // Yon listesini dis sozlesme DTO listesine cevirir.
    protected override List<DifferenceDirectionDto> MapToDto(List<DifferenceDirection> entities) => Mapper.MapToDto(entities);
    // Create girdisini domain modeline cevirir.
    protected override LookupCreateModel MapToCreateModel(CreateDifferenceDirectionDto input) => Mapper.MapToCreateModel(input);
    // Update girdisini domain modeline cevirir.
    protected override LookupUpdateModel MapToUpdateModel(UpdateDifferenceDirectionDto input) => Mapper.MapToUpdateModel(input);
}
