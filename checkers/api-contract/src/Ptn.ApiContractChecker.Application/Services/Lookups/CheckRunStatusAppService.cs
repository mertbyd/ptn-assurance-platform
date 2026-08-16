using Ptn.ApiContractChecker.Application.Mappers.Lookups;
using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Models.Lookups;
using Ptn.ApiContractChecker.Services.Lookups;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Ptn.ApiContractChecker.Application.Services.Lookups;

// islevi: CheckRunStatus lookup okuma, olusturma, update ve pasiflestirme akisini orkestre eder.
// sistemdeki gorevi: Ortak lookup tabanina yalniz run durumu entity kurulumu ve Mapperly baglantilarini verir.
[RemoteService(IsEnabled = false)]
public class CheckRunStatusAppService
    : LookupAppServiceBase<CheckRunStatus, CheckRunStatusDto, CreateCheckRunStatusDto, UpdateCheckRunStatusDto>,
      ICheckRunStatusAppService
{
    private static readonly CheckRunStatusMapper Mapper = new();

    // Durum repository baglantisini ortak lookup akisina verir.
    public CheckRunStatusAppService(
        IAbpLazyServiceProvider provider,
        IRepository<CheckRunStatus, Guid> repository)
        : base(provider, repository)
    {
    }

    // Dogrulanmis create modelinden ABP kimlikli yeni run durumu satiri kurar.
    protected override CheckRunStatus CreateEntity(LookupCreateModel model)
        => new(GuidGenerator.Create(), model.Code, model.Name, model.Description, model.IsActive);

    // Durum entity'sini dis sozlesme DTO'suna cevirir.
    protected override CheckRunStatusDto MapToDto(CheckRunStatus entity) => Mapper.MapToDto(entity);
    // Durum listesini dis sozlesme DTO listesine cevirir.
    protected override List<CheckRunStatusDto> MapToDto(List<CheckRunStatus> entities) => Mapper.MapToDto(entities);
    // Create girdisini domain modeline cevirir.
    protected override LookupCreateModel MapToCreateModel(CreateCheckRunStatusDto input) => Mapper.MapToCreateModel(input);
    // Update girdisini domain modeline cevirir.
    protected override LookupUpdateModel MapToUpdateModel(UpdateCheckRunStatusDto input) => Mapper.MapToUpdateModel(input);
}
