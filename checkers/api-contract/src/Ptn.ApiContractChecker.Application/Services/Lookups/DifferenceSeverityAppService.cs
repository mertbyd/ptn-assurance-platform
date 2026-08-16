using Ptn.ApiContractChecker.Application.Mappers.Lookups;
using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Models.Lookups;
using Ptn.ApiContractChecker.Services.Lookups;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Ptn.ApiContractChecker.Application.Services.Lookups;

// islevi: DifferenceSeverity lookup okuma, olusturma, update ve pasiflestirme akisini orkestre eder.
// sistemdeki gorevi: Ortak lookup tabanina yalniz fark siddeti entity kurulumu ve Mapperly baglantilarini verir.
[RemoteService(IsEnabled = false)]
public class DifferenceSeverityAppService
    : LookupAppServiceBase<DifferenceSeverity, DifferenceSeverityDto, CreateDifferenceSeverityDto, UpdateDifferenceSeverityDto>,
      IDifferenceSeverityAppService
{
    private static readonly DifferenceSeverityMapper Mapper = new();

    // Siddet repository baglantisini ortak lookup akisina verir.
    public DifferenceSeverityAppService(
        IAbpLazyServiceProvider provider,
        IRepository<DifferenceSeverity, Guid> repository)
        : base(provider, repository)
    {
    }

    // Dogrulanmis create modelinden ABP kimlikli yeni fark siddeti satiri kurar.
    protected override DifferenceSeverity CreateEntity(LookupCreateModel model)
        => new(GuidGenerator.Create(), model.Code, model.Name, model.Description, model.IsActive);

    // Siddet entity'sini dis sozlesme DTO'suna cevirir.
    protected override DifferenceSeverityDto MapToDto(DifferenceSeverity entity) => Mapper.MapToDto(entity);
    // Siddet listesini dis sozlesme DTO listesine cevirir.
    protected override List<DifferenceSeverityDto> MapToDto(List<DifferenceSeverity> entities) => Mapper.MapToDto(entities);
    // Create girdisini domain modeline cevirir.
    protected override LookupCreateModel MapToCreateModel(CreateDifferenceSeverityDto input) => Mapper.MapToCreateModel(input);
    // Update girdisini domain modeline cevirir.
    protected override LookupUpdateModel MapToUpdateModel(UpdateDifferenceSeverityDto input) => Mapper.MapToUpdateModel(input);
}
