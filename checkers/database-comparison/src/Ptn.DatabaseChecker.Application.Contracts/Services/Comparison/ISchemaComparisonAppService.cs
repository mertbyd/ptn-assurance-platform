using System.Threading.Tasks;
using Ptn.DatabaseChecker.Dtos.Comparison;
using Ptn.DatabaseChecker.Dtos.Findings;
using Volo.Abp.Application.Services;

namespace Ptn.DatabaseChecker.Services.Comparison;

// islevi: Iki baglanti arasinda sema karsilastirma use-case'inin servis kontratini tanimlar.
// sistemdeki gorevi: Kesif (tek baglanti) akisindan ayri, iki tarafi okuyup motoru calistiran ve bulgulari donen Application kontrati; sonucu persist etmez (T10 run tetikleme ayri is).
public interface ISchemaComparisonAppService : IApplicationService
{
    // islevi: Kaynak ve hedef baglantinin semalarini kapsam filtresiyle kiyaslar, yapisal bulgulari doner.
    Task<ComparisonFindingsDto> CompareSchemaAsync(CompareSchemaRequestDto input);
}
