using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Catalog;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Catalog;

// islevi: Arazzo taslaginin kalicilastirilmayan derleme onizlemesini tanimlar.
// sistemdeki gorevi: Domain derleyiciyi dis HTTP yuzeyine Application sozlesmesiyle baglar.
public interface IScenarioCompilationAppService : IApplicationService
{
    Task<ScenarioCompilePreviewResultDto> CompilePreviewAsync(ScenarioCompilePreviewDto input);
}
