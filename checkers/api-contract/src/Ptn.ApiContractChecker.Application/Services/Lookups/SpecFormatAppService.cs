using Ptn.ApiContractChecker.Application.Mappers.Lookups;
using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Models.Lookups;
using Ptn.ApiContractChecker.Services.Lookups;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Ptn.ApiContractChecker.Application.Services.Lookups;

// islevi: SpecFormat lookup okuma, olusturma, update ve pasiflestirme akisini orkestre eder.
// sistemdeki gorevi: Ortak lookup tabanina yalniz format entity kurulumu ve Mapperly baglantilarini verir.
[RemoteService(IsEnabled = false)]
public class SpecFormatAppService
    : LookupAppServiceBase<SpecFormat, SpecFormatDto, CreateSpecFormatDto, UpdateSpecFormatDto>,
      ISpecFormatAppService
{
    private static readonly SpecFormatMapper Mapper = new();

    // Format repository baglantisini ortak lookup akisina verir.
    public SpecFormatAppService(
        IAbpLazyServiceProvider provider,
        IRepository<SpecFormat, Guid> repository)
        : base(provider, repository)
    {
    }

    // Dogrulanmis create modelinden ABP kimlikli yeni format satiri kurar.
    protected override SpecFormat CreateEntity(LookupCreateModel model)
        => new(GuidGenerator.Create(), model.Code, model.Name, model.Description, model.IsActive);

    // Format entity'sini dis sozlesme DTO'suna cevirir.
    protected override SpecFormatDto MapToDto(SpecFormat entity) => Mapper.MapToDto(entity);
    // Format listesini dis sozlesme DTO listesine cevirir.
    protected override List<SpecFormatDto> MapToDto(List<SpecFormat> entities) => Mapper.MapToDto(entities);
    // Create girdisini domain modeline cevirir.
    protected override LookupCreateModel MapToCreateModel(CreateSpecFormatDto input) => Mapper.MapToCreateModel(input);
    // Update girdisini domain modeline cevirir.
    protected override LookupUpdateModel MapToUpdateModel(UpdateSpecFormatDto input) => Mapper.MapToUpdateModel(input);
}
