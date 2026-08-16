using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Projections;

namespace Ptn.DatabaseChecker.Interface.Projections;

// islevi: Katalogda dogrulanmis adres, anahtar ve kolonlarla sinirli salt-okunur projection portunu tanimlar.
// sistemdeki gorevi: Manager'i EF, SQL ve motor ayrintilarindan ayirir; resolver her motorun mevcut data repository bilesenini secer.
public interface IProjectionRepository : IEngineComponent
{
    // islevi: Yalniz secili kolonlari, bagli anahtar parametreleri ve satir butcesiyle okur.
    Task<List<ProjectionRow>> ReadProjectionRowsAsync(
        DatabaseConnectionInfo info,
        TableDataStructureModel structure,
        Dictionary<string, string?> keyValues,
        List<string> projectColumns,
        int maxRows,
        CancellationToken cancellationToken = default);
}
