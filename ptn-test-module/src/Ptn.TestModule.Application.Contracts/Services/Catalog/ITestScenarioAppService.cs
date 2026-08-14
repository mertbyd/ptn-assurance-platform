using System;
using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Catalog;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Catalog;

// islevi: Senaryo katalogu CRUD, onay ve yayin use-case'lerini tanimlar.
// sistemdeki gorevi: Persistence entity'sini acmadan Application katmaninin public sozlesmesini sunar.
public interface ITestScenarioAppService : IApplicationService
{
    // Kimligi verilen senaryo surumunu getirir.
    Task<TestScenarioDto> GetAsync(Guid id);

    // Senaryo surumlerini kararli siralama ve sayfalamayla getirir.
    Task<PagedResultDto<TestScenarioDto>> GetListAsync(TestScenarioListInput input);

    // Draft durumunda yeni ve monoton surumlu senaryo olusturur.
    Task<TestScenarioDto> CreateAsync(CreateTestScenarioDto input);

    // Draft veya onay bekleyen senaryo surumunu gunceller.
    Task<TestScenarioDto> UpdateAsync(Guid id, UpdateTestScenarioDto input);

    // Tarihsel senaryo surumunu silme istegini domain kuralina yonlendirir.
    Task DeleteAsync(Guid id);

    // Draft senaryo surumunu insan onayi bekleyen duruma tasir.
    Task<TestScenarioDto> SubmitForApprovalAsync(Guid id);

    // Bes yayin kapisini makine kanitiyla, durum degistirmeden degerlendirir.
    Task<TestScenarioPublishDecisionDto> EvaluatePublicationAsync(Guid id);

    // Insan onayini source hash'e baglayip tum kapilar gectiyse senaryoyu yayinlar.
    Task<TestScenarioDto> PublishAsync(Guid id);

    // Yayinlanmis senaryo surumunu tarihsel kaydi koruyarak kullanimdan kaldirir.
    Task<TestScenarioDto> DeprecateAsync(Guid id);
}
