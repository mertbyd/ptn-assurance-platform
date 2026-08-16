using Ptn.ApiContractChecker.Entities.Snapshots;

namespace Ptn.ApiContractChecker.Interface.Snapshots;

// islevi: Icerik-adresli SpecContent satirlarinin okuma sorgularini tanimlar.
// sistemdeki gorevi: Ayni ham icerigin tenant icinde ikinci kez yazilmasini engelleyen tekil okumayi saglar.
public interface ISpecContentRepository : IBaseRepository<SpecContent>
{
    // Tenant filtresi altinda ham hash ile eslesen degismez icerigi getirir.
    Task<SpecContent?> FindByRawHashAsync(string rawHash);
}
