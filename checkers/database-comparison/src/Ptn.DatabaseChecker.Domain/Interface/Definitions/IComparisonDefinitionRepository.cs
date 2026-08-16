using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Entities.Definitions;

namespace Ptn.DatabaseChecker.Interface.Definitions;

// islevi: ComparisonDefinition icin baglanti/mod navigation'lari dahil okuma sorgularinin kontratini tanimlar.
// sistemdeki gorevi: DTO'daki SourceConnectionName/TargetConnectionName/ComparisonTypeName alanlari bu detayli okumadan beslenir; kapsam kurallari gomulu owned JSON oldugu icin ek okuma gerektirmez.
public interface IComparisonDefinitionRepository : IBaseRepository<ComparisonDefinition>
{
    // Tek tarifi tum navigation'lariyla getirir; bulunamazsa null.
    Task<ComparisonDefinition?> FindWithDetailsAsync(Guid id);

    // Tarifleri navigation'lariyla sayfali listeler.
    Task<List<ComparisonDefinition>> GetPagedWithDetailsAsync(int skipCount, int maxResultCount);

    // Verilen tarif kimliklerini navigation'lariyla tek sorguda getirir.
    Task<List<ComparisonDefinition>> GetWithDetailsByIdsAsync(List<Guid> ids);

    // Verilen kimliklerden aktif tenant veya host-kullanici kapsaminda gorulebilen tarifleri getirir.
    Task<List<ComparisonDefinition>> GetAccessibleByIdsAsync(List<Guid> ids);

    // Aktif tenant veya host-kullanici kapsamindaki tarif sayisini dondurur.
    Task<long> GetAccessibleCountAsync();
}
