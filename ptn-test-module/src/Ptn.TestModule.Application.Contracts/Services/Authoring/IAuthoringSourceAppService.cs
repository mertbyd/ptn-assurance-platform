using System.Collections.Generic;
using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Authoring;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Authoring;

// islevi: Is kurali ve profil paketi malzemesinin yuklenme ve listelenme use-case'lerini tanimlar.
// sistemdeki gorevi: Yazarlik malzemesini host yeniden derlenmeden tek kaynaktan degistirilebilir kilar.
public interface IAuthoringSourceAppService : IApplicationService
{
    Task<AuthoringSourceDto> UploadBusinessRulesAsync(UploadBusinessRulesDto input);
    Task<AuthoringSourceDto> UploadProfilePackAsync(UploadProfilePackDto input);
    Task<List<ProfilePackSummaryDto>> GetProfilePacksAsync();
}
