using System;
using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Authoring;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Authoring;

// islevi: Yazarlik oturumunu baslatma, okuma, cevaplama ve tek adim ekleme use-case'lerini tanimlar.
// sistemdeki gorevi: Gecici cache state'ini entity veya cache tipi acmadan public yuzeye tasir.
public interface IAuthoringSessionAppService : IApplicationService
{
    Task<AuthoringSessionDto> CreateAsync(CreateAuthoringSessionDto input);
    Task<AuthoringSessionDto> GetAsync(Guid id);
    Task<AuthoringSessionDto> AnswerAsync(Guid id, AnswerAuthoringSessionDto input);
    Task<AuthoringSessionDto> AddStepAsync(Guid id, AddAuthoringStepDto input);
    Task<AuthoringSessionDto> AddDatabaseStepAsync(Guid id, AddDatabaseAuthoringStepDto input);
}
