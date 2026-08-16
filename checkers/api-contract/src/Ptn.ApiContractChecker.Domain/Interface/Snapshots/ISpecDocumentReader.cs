using Ptn.ApiContractChecker.Models.Snapshots;

namespace Ptn.ApiContractChecker.Interface.Snapshots;

// islevi: Ham spec baytini ayristirip format, surum, kanonik metin ve yapisal snapshot uretme yetenegini tanimlar.
// sistemdeki gorevi: Domain'i OpenAPI ayristirici kutuphanesinden ve surum modelinden bagimsiz tutan tek okuma siniridir.
public interface ISpecDocumentReader
{
    // Govdeyi ayristirir; bozuk veya tani ureten dokumani kararli hata koduyla reddeder.
    Task<ParsedSpecModel> ReadAsync(byte[] content);
}
