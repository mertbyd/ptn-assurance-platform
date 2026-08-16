using Ptn.ApiContractChecker.Application.Mappers.Lookups;
using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Models.Lookups;
using Ptn.ApiContractChecker.Services.Lookups;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Ptn.ApiContractChecker.Application.Services.Lookups;

// islevi: DifferenceKind lookup okuma, olusturma, update ve pasiflestirme akisini orkestre eder.
// sistemdeki gorevi: Ortak lookup tabanina yalniz fark turu entity kurulumu ve Mapperly baglantilarini verir.
[RemoteService(IsEnabled = false)]
public class DifferenceKindAppService
    : LookupAppServiceBase<DifferenceKind, DifferenceKindDto, CreateDifferenceKindDto, UpdateDifferenceKindDto>,
      IDifferenceKindAppService
{
    private static readonly DifferenceKindMapper Mapper = new();

    // Fark turu repository baglantisini ortak lookup akisina verir.
    public DifferenceKindAppService(
        IAbpLazyServiceProvider provider,
        IRepository<DifferenceKind, Guid> repository)
        : base(provider, repository)
    {
    }

    // Dogrulanmis create modelinden ABP kimlikli yeni fark turu satiri kurar.
    protected override DifferenceKind CreateEntity(LookupCreateModel model)
        => new(GuidGenerator.Create(), model.Code, model.Name, model.Description, model.IsActive);

    // Fark turu entity'sini dis sozlesme DTO'suna cevirir.
    protected override DifferenceKindDto MapToDto(DifferenceKind entity) => Mapper.MapToDto(entity);
    // Fark turu listesini dis sozlesme DTO listesine cevirir.
    protected override List<DifferenceKindDto> MapToDto(List<DifferenceKind> entities) => Mapper.MapToDto(entities);
    // Create girdisini domain modeline cevirir.
    protected override LookupCreateModel MapToCreateModel(CreateDifferenceKindDto input) => Mapper.MapToCreateModel(input);
    // Update girdisini domain modeline cevirir.
    protected override LookupUpdateModel MapToUpdateModel(UpdateDifferenceKindDto input) => Mapper.MapToUpdateModel(input);
}
