using Ptn.ApiContractChecker.Models.Sources;

namespace Ptn.ApiContractChecker.Interface.Sources;

// islevi: Canli dokuman URL'sinden guard'lanmis ham spec govdesi cekme yetenegini tanimlar.
// sistemdeki gorevi: Domain ve sonraki snapshot akisini HTTP, resilience ve Vault implementasyonundan ayirir.
public interface ISpecFetcherClient
{
    // Tek dokumani durum, medya tipi, boyut ve bosluk guard'larindan gecirerek getirir.
    Task<SpecFetchResultModel> FetchAsync(SpecFetchRequestModel request);
}
